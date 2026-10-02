# Version 3 findings: five models after the phase 1 changes and the organisation rule

**Version:** 3 (follows [version 1](EVALUATION_FINDINGS_V1.md) and [version 2](EVALUATION_FINDINGS_V2.md))
**Report:** [v3-20261002-1715-phase1-5-models](evaluation-reports/v3-20261002-1715-phase1-5-models.md) (2 October 2026; the full report, with its `.scores.json`, is kept in [evaluation-reports](evaluation-reports/README.md))
**Run:** 5 models, 38 documents, 358 occurrences on the answer key, 32 minutes in total

---

## 1. Summary

| Model | Recall v1 → **v3** | Precision v1 → **v3** | F1 v1 → **v3** | Missed v1 → **v3** | Over-redactions v1 → **v3** | Must-keep damaged | Time per document |
|---|---:|---:|---:|---:|---:|---:|---:|
| **phi4** | 95.3% → **98.3%** | 95.9% → **97.7%** | 95.6% → **98.0%** | 16 → **6** | 13 → **8** | 2 of 31 | 9.2 s |
| **qwen3.6:27b** | 96.7% → **99.4%** | 85.0% → **89.5%** | 90.5% → **94.2%** | 11 → **2** | 57 → **41** | 3 of 31 | 16.0 s |
| **gemma4:31b** | 96.7% → **99.2%** | 86.0% → **91.7%** | 91.0% → **95.3%** | 11 → **3** | 53 → **32** | 3 of 31 | 14.0 s |
| **gpt-oss** | 92.6% → **97.2%** | 92.5% → **96.5%** | 92.6% → **96.9%** | 25 → **10** | 24 → **12** | 0 of 31 | 8.2 s |
| **gemma4:e4b** | 89.9% → **93.3%** | 91.7% → **94.9%** | 90.8% → **94.1%** | 34 → **24** | 26 → **17** | 0 of 31 | 3.0 s |

**What this says.**
- **Every model improved on every measure**, with no model slower. The rules layer and the organisation rule lifted recall by 3 to 5 points for four of the five, and also reduced over-redaction, because the rules find the structured items exactly and the models no longer over-reach to cover them.
- **phi4 is now the best all-round model:** 98.3% recall, 97.7% precision, and the fewest over-redactions (8) of the high-recall models, at 9 s per document.
- **qwen3.6:27b and gemma4:31b are nearly complete on recall** (99.4% and 99.2%, with 2 and 3 items missed) but redact more than is needed (41 and 32 over-redactions), mostly job titles and dates.
- **All PDF and scan formats are now 100% for all five models** (text-layer PDFs, clean and degraded scans, image-only PDFs). Version 1's weakest formats are fixed, which confirms the layout, not the models, was the problem there.
- All figures need the caveat in section 5.

## 2. What was still missed

Across the five models there are **two occurrences no model found**, plus a short list that depends on the model.

| Item | Missed by | What it is |
|---|---|---|
| **"Kestrel"** (a project codename, in the Word and Markdown meeting notes) | **all five** | A policy question: is a project codename sensitive? If a client says yes, it belongs on the client's term list, which the application already redacts every time |
| "2019 data breach at the Bristol depot", "GBP 92,000" | phi4, gemma4:e4b, gpt-oss (the first only: gemma4:31b) | Contextual identifiers: facts that identify a person without being a name |
| "Paris", "Jordan" in the mixed document | phi4, gemma4:e4b | A word used as a person's name where it is also a city or country |
| "Will", "Mark", "Rose", "Bill", "Grace", "Hope" | gemma4:e4b only | First names that are also ordinary words |
| "Apple", "Shell", "Amazon", "Target" | gemma4:e4b and gpt-oss | Companies that are also ordinary words |

**What changed from version 1:** the six occurrences all ten models missed there are down to two. "Fernleigh Surgery" and "Brightwater Analytics Ltd" are now found in every format by all five models, which is the **organisation rule** doing exactly what it was added for. The only item still missed by everything is the policy question.

## 3. What was over-redacted

The over-redactions are now small and nearly all policy-shaped, not mistakes:

| Redacted though not on the key | Times (across models) | Likely decision |
|---|---:|---|
| Job titles ("Senior Data Scientist", "Head of People", "bus driver") | 45 | A policy choice per client: titles identify people only in small organisations |
| "14 October 2025" and other dates read as a date of birth | 21 | Needs a rule or description that distinguishes a birth date from a letter date |
| Ages in passing ("61", "42") | 12 | Whether ages are sensitive is a policy choice |
| "HMRC", "Victoria Station" (public bodies and places) | 8 | The must-keep list: some models still redact HMRC |
| "he" as a gender word | 8 | Pronoun handling when the pronoun option is on |

Must-keep items damaged: 2 or 3 for three models, and **0 for gpt-oss and gemma4:e4b**.

## 4. Combining models (measured misses, estimated recall)

| Pair | Estimated recall together | Most extra over-redactions |
|---|---:|---:|
| phi4 + qwen3.6:27b | 99.4% | 49 |
| qwen3.6:27b + gpt-oss | 99.4% | 53 |
| phi4 + gemma4:31b | 99.2% | 40 |
| phi4 + gpt-oss | 98.9% | 20 |

**Pairing no longer adds much recall:** qwen3.6:27b alone already reaches 99.4%, and the only items it misses are the two "Kestrel" occurrences that every model misses, so a second model cannot recover anything. The remaining value of combining models is **precision and confidence, not recall**: for example, using agreement between phi4 (precise) and qwen3.6:27b (thorough) to decide what is redacted automatically and what goes to a person. That is what phase 2 (agreement scoring) would test, together with a detector of a different kind (GLiNER) that might catch what all of these chat models miss.

## 5. Caveats: read these before quoting any number

1. **The corpus was used to build the fixes.** The rules and the organisation suffix list were written after looking at what these same 38 documents showed, and the answer key was corrected on the same evidence. The scores are therefore **optimistic**: they show the fixes work on the cases that motivated them, and do not show how well the system will do on documents it has not seen. A fair figure needs a **held-out set** that was never used for tuning (phase 2 and phase 4 in the roadmap).
2. **A small, synthetic corpus.** 358 occurrences is too few to separate models that differ by a point or so. Treat the models as groups: top (qwen3.6:27b, gemma4:31b, phi4), then gpt-oss, then gemma4:e4b.
3. **"Perfect" on the plain categories does not mean perfect.** Emails, phones, IDs, dates of birth, addresses, secrets and company numbers are at 100% for every model, but the test documents are cleaner than clients' real ones.
4. **The 5-model run is not a like-for-like comparison with the 10-model run in version 1**: five models were dropped, the key has 21 more occurrences, and the rules layer is new.
5. **Run-to-run variation** of a few tenths of a point was seen with identical settings.

## 6. What this means for the proposal

- The layered approach works: **fixed rules plus a local model reached 98–99% recall** on this corpus and removed whole classes of failure (PDF and scan layout, gender words, structured identifiers).
- What is left is the **hard, judgement-based category** (contextual identifiers, words that are both names and ordinary words, project codenames, and policy-dependent items such as job titles). These are where a second kind of detector, agreement between models, and human review of disagreements are the right tools, and where the client's own policy and term lists matter most.
- The claim to make is **"measured, layered and reviewable"**, not "perfect": every layer is scored, every redaction records its source, and anything uncertain goes to a person.

## 7. Next steps

1. **Phase 2:** the GLiNER spike in .NET, agreement scoring and the second-pass check ([phase 2 plan](PHASE_2_PLAN.md)).
2. **Build a held-out test set** (new documents, never used to write rules) so that the next figures are honest.
3. **Add client term lists and policy options** (job titles, ages, project codenames, public bodies) as configuration, so the policy-shaped over-redactions and the "Kestrel" case are decisions a client makes, not model errors.
4. **Re-run all models** that remain interesting on the held-out set, and keep each run as a numbered version in `documentation/evaluation-reports`.
