# Version 1 findings: evaluation results and the road to production-grade redaction

**Based on:** evaluation run `eval/eval-20261002-1453.md` (2 October 2026)
**Version:** 1 (findings before the phase 1 accuracy changes)  
**Status:** prototype findings, for discussion
**Author:** David Berry

---

## 1. Summary

The prototype redacts documents **entirely on one machine**, using local language models, and every redacted file it writes is **checked before it is released**. In a controlled test of 10 local models over 38 documents (text, Word, PDF and scanned images), the results were:

- The **best all-round model, phi4, removed 95.3% of sensitive occurrences** with 95.9% precision. It took about 9 seconds per document on a laptop.
- The **two largest models removed 96.7%**, at the cost of more over-redaction (85–86% precision).
- **All 380 redacted files (10 models × 38 documents) passed the output safety checks.** There was no recoverable text in PDFs, no hidden content in Word files, and no sensitive text found when the scans were re-read by OCR.
- **A single document scores the same whether it arrives as text, Word, PDF or a scan.** The document handling is sound, and the remaining gaps sit in detection.

The more important finding is **why** the remaining items were missed. Most misses are **systematic, explainable and fixable without a better model**:

- a text-matching defect when PDFs and scans wrap a name across two lines
- gender words and pronouns that models handle inconsistently, which a fixed word list would catch every time
- an occasional silent empty reply from smaller models

I estimate that fixing these would lift phi4 from **95.3% to about 98%** recall. That is an estimate and needs a re-run to confirm (section 4).

This prototype supports a production design built on **layers that check each other**, not on one model. Fixed rules handle the predictable items, one or more models handle the judgement calls, an automatic check reads the output again, and a person reviews only the disagreements. Every step would be recorded so that each redaction can be proved.

---

## 2. What was tested

| | |
|---|---|
| Documents | 38 files made from 23 synthetic documents in every format: plain text, CSV, JSON, Markdown, Word, PDF with text, and scans (clean, degraded and image-only PDF) |
| Answer key | 143 sensitive items (337 occurrences) across 15 categories, plus 31 "must-keep" items that should *not* be removed |
| Models | 10 local models, 4B to 31B parameters, run through Ollama with reasoning switched off |
| Settings | Temperature 0, fixed seed, identical prompts and category settings for every model |
| Run time | 1 h 19 min for all 10 models (one laptop, 128 GB, Apple silicon) |
| Data | Entirely invented. No real personal data was used at any point |

Scoring is strict. An item counts as removed only when *all* of its text is gone, so redacting only the surname of a full name counts as a miss.

---

## 3. Results

### 3.1 Headline table

| Model | Recall (removed) | Precision (correct redactions) | F1 | Missed | Must-keep damaged | Time per document |
|---|---:|---:|---:|---:|---:|---:|
| **phi4** (14.7B) | **95.3%** | 95.9% | **95.6%** | 16 of 337 | 2 of 31 | 9.3 s |
| gemma4:31b | **96.7%** | 86.0% | 91.0% | 11 | 3 | 13.8 s |
| qwen3.6:27b | **96.7%** | 85.0% | 90.5% | 11 | 3 | 15.8 s |
| granite4.2:30b | 93.5% | 93.4% | 93.4% | 22 | 7 | 39.2 s |
| qwen2.5:14b | 90.5% | **96.4%** | 93.3% | 32 | 2 | 10.0 s |
| gpt-oss (20.9B) | 92.6% | 92.5% | 92.6% | 25 | **0** | 8.2 s |
| llama3.1:8b | 89.9% | 93.7% | 91.8% | 34 | 5 | 4.9 s |
| gemma4:e4b (7.5B) | 89.9% | 91.7% | 90.8% | 34 | **0** | **3.0 s** |
| qwen3.5:4b | 88.7% | 92.9% | 90.8% | 38 | 3 | 4.3 s |
| mistral-small3.2 | 86.9% | 95.1% | 90.8% | 44 | 4 | 14.6 s |

**How precise the numbers are.** With 337 occurrences, a recall figure is accurate to roughly ±2 percentage points (95% confidence). For example, phi4's 95.3% means somewhere between about 93% and 97%. Differences of 1–2 points between models are therefore **not** meaningful. The groups are: phi4, gemma4:31b and qwen3.6:27b at the top, then granite, gpt-oss and qwen2.5, then the rest.

### 3.2 What the categories show

| Category | Result | What it means |
|---|---|---|
| Emails, ID numbers, dates of birth, company numbers, domains | 100% for almost every model | Structured items are solved |
| Phone numbers, secrets (passwords, keys) | 93–100% | Nearly solved, and a fixed rule would close the gap |
| People | 86–100% (phi4 98%, the three large models 100%) | Strong, and the larger models are near perfect |
| Companies | 82–93% | Good, and most misses trace to the line-wrap defect below |
| Addresses | 66–100% (phi4 and granite 100%) | Model-dependent, and some misses are the line-wrap defect |
| **Gender** (words and pronouns) | **32–100%**, very inconsistent | Models disagree about whether "her" or "Ms" is sensitive. Needs a fixed rule |
| **Contextual identifiers** (for example "the only female partner at the Bristol office") | **33–67%** recall, and **low precision** | The genuinely hard category. Models also over-flag job titles here. Needs human review |

### 3.3 Proof the output is safe

Each redacted file was written in its real format and then checked by the tool itself before release:

- PDFs are flattened to images with no text layer.
- Word files are checked for hidden text, comments, tracked changes and metadata.
- Scans are re-read by OCR to confirm that nothing sensitive is still legible.

**All 380 outputs passed.** A file that fails this check is refused, not written.

---

## 4. Why items were missed: root causes

I grouped every miss and over-redaction across all 10 models by its cause. Most fall into a few systematic patterns.

| # | Cause | Evidence | Fix | Expected effect |
|---|---|---|---|---|
| 1 | **Line-wrap defect (code fault, very likely).** In PDFs and scans, a name or address that wraps onto a new line has a line break where the model's answer has a space. The detector matches the answer exactly, so it fails to place it. | "Fernleigh Surgery" and "Harbourside Works, 7 Quay Lane…" are found by almost every model in the text and Word versions, but missed by **every model** in the PDF and scan versions (62 model-document misses between them). The scorer already ignores spacing differences, and the detector does not. | Make the detector's text matching treat any run of spaces or line breaks as equal (the scorer already does this) | Removes the largest single source of misses. A small, contained change |
| 2 | **Gender words handled inconsistently** | "man", "Ms" and "her" are missed by many models. GENDER recall ranges from 32% to 100% | A fixed list of gender words, titles and pronouns, applied by rule before the model, when the gender category is on | About 100% for this category, every time |
| 3 | **Silent empty replies** from smaller models | gemma4:e4b found nothing in two documents (0 of 8 and 0 of 6), qwen3.5:4b found nothing in one, and gpt-oss found nothing in one. The larger models found everything in the same documents | Treat "no items" as suspicious when the rules or a quick check see likely names. Retry, then fall back to a second model | Removes whole-document failures, the most dangerous kind |
| 4 | **Answer-key gaps** (the test data, not the tool) | "account ending 4821" was redacted by nearly every model (43 times) but is not on the key. A partial bank account number is arguably sensitive. An OCR-split email ("accounts @fernleigh…") counts as over-redaction although it was correctly hidden | Review the key with the team. Add partial account numbers and make the scorer tolerate OCR spacing | Fairer precision figures. No change to real safety |
| 5 | **Policy questions** | Job titles ("Head of People", "Senior Data Scientist"), well-known public bodies ("HMRC") and ages in passing were redacted by some models | Agree a written redaction policy per client or use case, and encode it in the category descriptions and must-keep lists | Fewer over-redactions, and behaviour that matches the policy |
| 6 | **Genuine judgement calls** | Contextual identifiers ("Project Kestrel", "2019 data breach at the Bristol depot"), and the same word used as a person and as a place ("Paris") | Several models plus human review of disagreements (section 6) | Cannot be fully automated. Route these to a person, with a reason shown |

**Estimated effect on phi4.** Of phi4's 16 misses:
- 4 are the line-wrap defect.
- 6 are gender words.
- 6 are contextual or mixed-use judgement calls.

Fixing causes 1 and 2 alone would take phi4 to about **98% recall** (6 of 337 missed), with no change of model. This is an estimate from the miss lists, and the next evaluation run would confirm it.

---

## 5. Combining models

The models miss *different* things, so running two together and removing whatever either finds raises recall. These estimates come from this run's own miss lists. They are ceilings, and the over-redaction figure is the most it could rise.

| Pair | Better model alone | Together (estimate) | Extra over-redactions (at most) | Time per document |
|---|---:|---:|---:|---:|
| gemma4:31b + granite4.2:30b | 96.7% | 98.2% | 74 | about 53 s |
| phi4 + gemma4:31b | 96.7% | 97.9% | 66 | about 23 s |
| **phi4 + mistral-small3.2** | 95.3% | **97.3%** | **28** | about 24 s |

phi4 with mistral-small3.2 gives the best trade-off, because it raises recall without much extra over-redaction. These gains would stack with the fixes in section 4, since they address different misses.

---

## 6. Recommended architecture for high-accuracy redaction

The aim is a process where **each layer catches what the one before misses, and every decision is recorded**.

| Layer | What it does | Why |
|---|---|---|
| 1. Extraction | Reads text from every format (with OCR for scans) and keeps the position of every character | Already built and consistent across formats |
| 2. **Fixed rules** | Emails, phone numbers, postcodes, NHS and NI numbers, IBANs, sort codes and account numbers, IP addresses, card numbers (with checksum), gender words and pronouns | Predictable items are found with certainty and repeatability, and run instantly |
| 3. **Model detection** | One or two local models find names, companies, addresses and contextual identifiers, with the surrounding words as evidence | Handles language and judgement that rules cannot |
| 4. **Agreement scoring** | Where rules and models agree, the redaction goes ahead automatically. Where they disagree, the item is flagged | Turns "the model said so" into a measurable confidence |
| 5. **Second-pass check** | A model re-reads the *redacted* output and asks whether anything identifying remains | Catches leaks the first pass missed, from a different angle |
| 6. **Human review** | A person sees only the flagged items, with the reason, and confirms or rejects each one | Already built in the prototype (manual redaction, accept or reject, area boxes) |
| 7. **Output verification** | Writes the file and proves nothing sensitive is recoverable | Already built: 380 of 380 passed |
| 8. **Audit record** | For each document: models and versions, prompt and settings fingerprint, every redaction with its source (rule, model or person), reviewer actions and timestamps, and the verification result | Each redaction can be proved and reproduced |

---

## 7. Proof and assurance: how accuracy is demonstrated

For a production system, "it works" has to be shown, not asserted. I propose the following.

1. **A redaction certificate per document.** This is a one-page summary of what was removed, by which layer, what a person reviewed, and the verification result. It would be generated from the audit record.
2. **Acceptance thresholds per category**, agreed with each client. An example would be 99.5% recall for names and identifiers, and every contextual item reviewed by a person. They would be published alongside the results.
3. **A regression gate.** The evaluation harness runs on every change, and a build that lowers recall in any category below its threshold cannot ship.
4. **A larger and harder test corpus.** It should include email threads, long multi-page documents, spreadsheets, tables, forms, handwriting and deliberately tricky cases. A **held-out set** would never be used for tuning.
5. **A real-data pilot.** A client would provide a small set of approved, real documents, marked by two people independently, with their agreement measured. This shows how the system performs on real documents, not just synthetic ones. The documents would stay on the client's own premises.
6. **Honest error bars.** Results would be reported with confidence ranges, and the test set would grow until the ranges are tight enough to support the claim being made.

---

## 8. Roadmap

| Phase | Work | Outcome | Rough size |
|---|---|---|---|
| **1. Quick fixes** | Fix line-wrap matching. Add the rules layer for structured items and gender words. Add the empty-reply guard with retry and fallback. Correct the answer key. Re-run the evaluation | About 98% recall expected for phi4, and no whole-document failures | Days |
| **2. Layered detection** | Run phi4 and a second model together with agreement scoring, add the second-pass check, and send disagreements to the existing review screen | High recall with a confidence for every item | 1–2 weeks |
| **3. Proof** | Audit record, redaction certificate, category thresholds, and the evaluation as a regression gate | Accuracy that can be demonstrated and audited | 1–2 weeks |
| **4. Real-world evidence** | Larger corpus, held-out set, and a client pilot with double-marked documents | Results that apply to clients' real documents | Weeks, depending on the client |
| **5. Optional custom model** | Fine-tune a model aimed only at whatever gap remains after phases 1–4, scored with the same harness | Closes a specific gap, if one remains | Weeks, mostly data preparation |
| **6. Production engineering** | Deployment target (server with a GPU, or a workstation), throughput, security review, retention policy, access control, and further UI refinement | A product a team can build on | Team-sized |

---

## 9. Suggested demonstration

1. **Open the evaluation report.** Show 10 models measured on identical documents, strict scoring, and 380 out of 380 outputs verified.
2. **Redact live** a Word letter, a PDF and a degraded scan of the *same* document, and show that the results match.
3. **Show "Context Text 11 (MIXED)":** the same word used as a person and as a city, handled by context.
4. **Open the Categories dialog,** switch a category off or to flag-only, and re-run. This shows that scope is configuration, not code.
5. **Show the review step:** accept or reject a suggestion, add a manual redaction, and draw an area box.
6. **Show this document's root-cause table (section 4).** The remaining misses are understood, and each has a planned fix.

---

## 10. Caveats

- The corpus is synthetic and deliberately clean. Real documents are messier, which is why the client pilot (phase 4) matters.
- 337 occurrences is enough to rank models into groups, but not enough to support a "99.5%" claim. That needs a larger corpus.
- The combination and fix estimates are worked out from miss lists, not measured. They become measured results after the next run.
- The line-wrap cause is very likely, from the pattern of misses and the code, but should be confirmed when the fix is made.
- Results depend on model versions, and the report records each model's digest so a run can be reproduced exactly.
