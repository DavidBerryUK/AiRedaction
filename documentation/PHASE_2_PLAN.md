# Phase 2 plan: layered detection with GLiNER, and benchmarking it

**Status:** planned, not started
**Owner:** David Berry
**Follows:** [Version 1 findings](EVALUATION_FINDINGS_V1.md) and [Version 2 findings](EVALUATION_FINDINGS_V2.md)

*Terminology:* "phase 2" here is the second step of the **accuracy roadmap** in the version 1 findings (section 8). It is not the same as the document-format phases 2, 2b and 2c in the specification's delivery plan (section 11), which are already built.

---

## 1. Why this phase

Phase 1 moved phi4 from 95.3% to 97.5% recall using fixed rules, and it showed what is left: items that general-purpose chat models leave out, such as company names in PDFs and scans, project codenames and contextual identifiers. In the first full run, **six occurrences were missed by all ten models**, so adding more chat models cannot reach them. What is needed is a detector of a **different kind**, whose misses differ, and a way to combine detectors and send disagreements to a person.

## 2. What GLiNER is, in brief

GLiNER is a small model (roughly 0.1–0.5 billion parameters, against 4–31 billion for our chat models) built only to **mark spans of text that match categories you name** (person, company, address and so on). It writes nothing, so it cannot invent or reword text; it returns exact positions with a confidence score; and it runs on an ordinary CPU in a fraction of a second per page. It is weaker than the chat models at judgement calls, so it complements them and does not replace them.

Published results (on other test sets, to be checked on ours) are encouraging: a 2026 redaction benchmark (RedactionBench) found BERT and GLiNER-style models beat small language models tuned for redaction, and a clinical study reported 0.98 F1 for a fine-tuned GLiNER. **None of that has been measured on this project's documents**, which is the point of the benchmark below.

## 3. Goals

1. Add GLiNER as a second kind of detector behind the existing detector interface, with its own entry in the evaluation.
2. **Benchmark it** against the current models with the same harness, answer key and scoring.
3. Measure **real combinations** (not estimates): chat model plus GLiNER plus the rules.
4. Add **agreement scoring**, so every redaction carries a confidence and disagreements are flagged for a person.
5. Add the **second-pass check**: a model re-reads the redacted output and asks whether anything identifying remains.

## 4. Work packages

### 4.1 Choose and obtain the model (about a day)

- Shortlist two or three GLiNER PII variants. Candidates found so far: **GLiNER2-PII** and Fastino's **gliner2-privacy-filter-PII-multi** (Hugging Face); others to be added after a wider search.
- For each: **licence** (must allow this use, including client work), size, languages, and whether an **ONNX** export exists or can be made.
- Record the model's version or digest in the report, as is done for the Ollama models, so results can be reproduced.

**Candidates checked so far** (from the model pages; not yet run on our documents):

| Model | Size | Licence | ONNX | Notes |
|---|---|---|---|---|
| **knowledgator/gliner-pii-edge-v1.0** | about 200–330 MB files | Apache 2.0 | **Yes** (FP16 330 MB, quantised 197 MB) | 60+ PII labels, English first. Published F1 75.5% (precision 79%, recall 72%) on its own synthetic PII set. The best fit to test first: licence is clear and the ONNX files exist |
| **fastino/gliner2-privacy-filter-PII-multi** | 205M parameters | Apache 2.0 | Not stated | 42 labels, 7 languages. Published F1 0.477 and recall about 0.72 on the SPY benchmark (precision about 0.36, so it over-marks). A conversion tool exists (`gliner2-onnx`, MIT, experimental) but lists only the base GLiNER2 models |
| nvidia/gliner-PII | large-v2.1 based | **NVIDIA Open Model License** (not Apache; needs a legal check before client use) | Not stated | Broad PII and health labels |
| urchade/gliner_multi_pii-v1 | multilingual | to confirm | Not stated | Six languages, an older fine-tune |

Published figures come from different test sets and are **not comparable with ours**, and several are modest (F1 between 48% and 76%). That is exactly why the spike measures them on our own documents first: the question is whether GLiNER catches what the chat models miss, not whether it beats them overall.

**

### 4.2 Run it inside the application, in .NET only (a few days; the main technical risk)

**Decision: everything stays in .NET**, for consistency with the application: no Python and no helper process. The model runs in-process through **ONNX Runtime for .NET** (`Microsoft.ML.OnnxRuntime`), behind the existing detector interface, with fully offline inference.

What the first candidate needs (from its published configuration; to be confirmed against the ONNX files):

| Item | Detail | What we write in C# |
|---|---|---|
| Architecture | **Token-level** GLiNER on a small **ModernBERT** encoder (about 32M parameters in the encoder, 10 layers), maximum 2,048 tokens | Nothing: ONNX Runtime runs the graph |
| Tokeniser | A Hugging Face `tokenizer.json` (byte-level BPE) | Load it with a .NET tokeniser package (`Microsoft.ML.Tokenizers` or `HuggingFace.Tokenizers`; the spike picks whichever reproduces the reference token ids exactly) |
| Input | Label prompt (`<<ENT>> label <<ENT>> label ... <<SEP>>`) followed by the document's words; word-to-token mask; text lengths | Build the input tensors (word splitting, label prompt, masks) |
| Output | Per token and per label: start, end and inside scores | Decode them into spans: threshold, join start-to-end, resolve overlaps, map back to exact character positions |
| Long documents | 2,048-token limit | Slide over the text in overlapping windows, as the chunker already does for the chat models, and merge spans from overlapping windows |

There is no ready-made C# GLiNER library, so the pre- and post-processing (about a few hundred lines) is ours to write. **Check:** compare our C# token ids and spans with the reference (Python) implementation on a few sentences once, to prove the port is faithful. This is a one-off check against published reference output, not a runtime dependency.

**Model files:** downloaded once, by hand, from Hugging Face (ONNX file, `tokenizer.json`, `gliner_config.json`) into a local `models/` folder that is **not committed**, and then used offline. The report records the model's file hash, as it records Ollama digests.

Deliverable: a `GlinerDetector` implementing the existing `IEntityDetector`, returning spans with `Source = "gliner"` and the model's confidence. It must keep to the project's rule that **documents never leave the machine** (no network calls and no model downloads at run time).

**Spike result (3 October 2026, `knowledgator/gliner-pii-edge-v1.0`, quantised ONNX, 46 MB, in .NET).** The C# port (tokeniser, prompt, windows, decoding) worked first time and finds names, companies, phone numbers, emails, addresses and dates of birth in a test sentence. The runner is `tools/AiDocumentRedactor.GlinerSpike` and it uses the evaluation's own scoring. On the 30 text-readable corpus documents (plain text, Word, text-layer PDFs; scans not included):

| Threshold | Recall | Precision | F1 | Missed | Over-redactions | Must-keep damaged |
|---:|---:|---:|---:|---:|---:|---:|
| 0.2 | 95.3% | 69.9% | 80.7% | 12 of 254 | 116 | 22 of 31 |
| 0.3 | 87.4% | 82.3% | 84.8% | 32 | 47 | 9 of 31 |
| 0.5 | 41.7% | 88.5% | 56.7% | 148 | 13 | 5 of 31 |

With the rules layer added at threshold 0.25: recall 98.0% (5 missed), precision 77%, 13 of 31 must-keep items damaged.

What it shows:
- **Speed is the headline:** about 0.02 s per document, against 3–16 s for the chat models (hundreds of times faster, on the CPU).
- **Alone it is not strong enough:** the best F1 is about 85%, against 98% for phi4 with the rules, and its confidence scores are low and spread out (typically 0.4–0.9), so no single threshold gives both high recall and high precision.
- **But it finds different things.** At a low threshold (0.25) it catches the items every chat model missed: both "Kestrel" occurrences, "2019 data breach at the Bristol depot", and the ordinary-word names and companies (Will, Mark, Rose, Apple, Shell, Amazon, Target). It misses "GBP 92,000" and the person "Paris" in the mixed document.
- **It cannot tell the two uses of a word apart** (a person called Paris and the city), so it also marks the must-keep words, which is why 13 of 31 must-keep items are damaged at 0.25.
- **So its role is a second opinion, not the redactor:** items it finds that the chat models and rules did not should be **flagged for review**, not redacted automatically, and agreement with a chat model raises confidence.

Caveats: the threshold was looked at on the same corpus (optimistic), scans are not included, the label wording ("person", "organization", "job title", and so on) was a first guess and may change results a lot, and this is the small "edge" variant: larger GLiNER PII models may do better.

**Next experiments:** label wording, a larger variant, scans (through OCR), and the agreement scoring with a chat model.

### 4.3 Map our categories to GLiNER labels

Our 15 categories are sent to GLiNER as plain-language labels (for example PERSON → "person", ID_NUMBER → "identification number"). Labels and the confidence threshold are **configuration**, not code, so they can be tuned per category, in the same spirit as the category descriptions given to the chat models. Flag-only categories (LOCATION) and the enabled/disabled switches apply exactly as today.

### 4.4 Benchmark (the part you want to see)

Run GLiNER through the existing evaluation harness so it is scored like every other model.

**What to measure**

| Measure | Why |
|---|---|
| Recall, precision, F1, must-keep items damaged | Same scoring as the other models, so they compare directly |
| Recall and precision **by category** and **by format** | Shows where it is strong (expected: names, contact details, IDs) and weak (expected: contextual identifiers) |
| **Confidence threshold sweep** (for example 0.3 to 0.9) | Finds the best recall/precision trade-off and shows the curve to a client |
| Time per document, on CPU, and memory | The cheap-detector claim needs a number |
| **Misses that differ** from the chat models | The reason for adding it: how many of the six all-model misses does it catch? |
| Label accuracy | Whether its category labels are right when it finds something |

**Combinations to measure for real** (a `--combine` mode in the evaluation, replacing the estimates in the report):

1. GLiNER alone.
2. Rules + GLiNER.
3. Rules + phi4 (the current best, for reference).
4. Rules + phi4 + GLiNER, **union**.
5. Rules + phi4 + GLiNER, **agreement** (redact when two of three agree; flag the rest for review).
6. Cascade: rules + GLiNER first, then phi4 only on passages where GLiNER's confidence is low or where it found little.

**Answer key and corpus.** Re-use the 38-document corpus, then extend it, because 337–358 occurrences is too few to make strong claims (see the version 1 caveats): more documents, harder layouts, a **held-out set never used for tuning**, and ideally a small set of real, client-approved documents marked by two people. Real client data stays on the client's premises and out of the repository.

**Rule for honest results.** The threshold is chosen on the tuning documents and the final figures are reported on the held-out ones, with confidence ranges.

### 4.5 Agreement scoring and review routing (about a week)

- Each redaction records which detectors found it (rules, chat model, GLiNER) and their confidences.
- Combine into a confidence: agreed by all, agreed by two, found by one.
- Items found by only one detector, or below a threshold, are **flagged** for the existing review screen with the reason shown ("found by GLiNER only, confidence 0.62"). Items agreed by several are redacted automatically.
- The thresholds are configuration and are reported in the evaluation.

### 4.6 Second-pass check (about a week)

- After redaction, a model reads the **redacted text** and is asked whether any person, organisation, contact detail or identifier is still visible.
- Anything it reports is flagged for review, never redacted silently, and counted in the evaluation as "caught by the second pass".
- This is measured the same way: how many of the leaks from the first pass does it catch, and how many false alarms does it add.

## 5. Acceptance criteria

The work is a success when, on the held-out documents:

1. GLiNER is benchmarked with the same scoring as the other models, and the report shows its category and format breakdown, the threshold curve, and its speed.
2. At least one measured combination **beats the best single model's recall** without more than a small rise in over-redaction (to be agreed; a working target is recall above 99% on the plain categories with every uncertain item flagged).
3. Every redaction in the output carries its sources and confidence, and disagreements appear in the review screen with a reason.
4. All of it runs **entirely locally**, with no network access at run time.
5. The documentation, the specification and the evaluation report are updated, and the version 3 findings say what was measured.

## 6. Risks and open questions

| Risk or question | Plan |
|---|---|
| GLiNER does worse than expected on our documents (published results are on other data) | The benchmark answers this early; it is cheap to try first as a standalone benchmark before the application work, so the spike can be stopped if it does not help |
| The C# port of the pre- and post-processing is harder than expected, or a .NET tokeniser cannot reproduce the reference token ids | Try both .NET tokeniser packages; compare against the reference output early; if neither works, reconsider (for example a different GLiNER variant with a simpler tokeniser) before spending more time |
| Licence or hosting terms of a particular model rule it out | Check in 4.1 before any integration; keep two candidates |
| Different tokenisation changes where spans start and end | Map spans back to exact document positions and test on wrapped lines and OCR output |
| More detectors mean more over-redaction | Agreement scoring and the threshold sweep exist to control exactly this; measured, not assumed |
| Small test set | Grow the corpus and hold out a test set before quoting any figure |
| Maintaining several models over time | Record versions in every report, and re-run the evaluation as a regression gate (version 1 findings, section 7) |

## 7. Suggested order

1. **Spike (2–3 days, in .NET):** a small console tool, `tools/AiDocumentRedactor.GlinerSpike`, loads the ONNX model with ONNX Runtime, runs it over the plain-text corpus documents and scores the spans against the answer key with the existing evaluation scoring code. It reports recall, precision and F1 at several confidence thresholds, recall by category, speed per document and the items missed, so they compare directly with the chat models. First model to try: `knowledgator/gliner-pii-edge-v1.0`. This answers "is it worth it" before any change to the application, and the spike code becomes the core of the `GlinerDetector`, so none of it is thrown away.
2. If promising, build the `GlinerDetector` (4.2) and the category mapping (4.3).
3. Run the full benchmark and the threshold sweep (4.4), then the real combinations.
4. Add agreement scoring and review routing (4.5), then the second-pass check (4.6).
5. Write the version 3 findings.

## 8. What this phase does not include

A custom fine-tuned model (the optional phase 5 in the version 1 roadmap), the audit record and redaction certificate (phase 3), and the larger real-world pilot (phase 4). Those follow once this phase shows how far layered detection gets.
