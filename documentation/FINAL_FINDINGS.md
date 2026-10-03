# Final findings: one evaluation, every model, every document

**Dataset:** `final-20261003` (3 October 2026), made by the final evaluator. Open it in the results explorer (`/explorer`) to check any figure in this document.
**Code:** commit `6062ee0`, finished on `19bfc11` (nothing that decides a result changed in between; the dataset records this).
**Run:** 5 local models over 338 documents (300 held-out finance documents and 38 documents in several file types), each with the fixed rules and with GLiNER, plus three no-model baselines. 7,774 results in 4 hours 18 minutes on one idle machine (Ollama 0.35.1, 18 cores). One pass.

This replaces the earlier version-by-version findings as the account to quote. The earlier documents ([V1](EVALUATION_FINDINGS_V1.md) to [V5](EVALUATION_FINDINGS_V5.md)) record how the approach was developed. The reasoning behind each design choice is in [METHODOLOGY.md](METHODOLOGY.md).

---

## 1. Summary

1. **The best single model removes about 98 in every 100 sensitive items and is right about 92 times in 100 when it removes something** (gemma4:31b: 98.0% recall, 91.9% precision on the 300 held-out documents). That is the honest figure: those documents were never used to shape the system.
2. **Two models checking each other, with a person deciding the disagreements, reached about 99.3% recall at 95.8% precision** on the same documents (gemma4:31b with gpt-oss, about one flag per document). This assumes the reviewer is right every time, which has not been measured.
3. **No model found everything, and the misses are not random.** Across 5 models and 338 documents only one item was missed by all five (a project codename, "Kestrel"). The rest are a small, describable set: indirect identifiers, ID numbers with no obvious shape, and places where the answer key itself is doubtful.
4. **What the models get wrong when they over-redact is mostly fixable by rules.** Masked values, bracketed placeholders, generic terms such as "Borrower", and ordinary dates read as dates of birth account for a large share.
5. **The fixed rules are precise but narrow** (97% right, but only 25% of what is sensitive on the held-out set). **GLiNER on its own** finds 81% but is right only 56% of the time, and its flags are far too noisy to review in bulk.
6. **Every one of the 1,684 redacted files written passed its safety check** (no recoverable text in PDFs, nothing hidden in Word files, scans read again by OCR).
7. **Six model-and-document pairs failed** (two timeouts, four server or reply errors). They are recorded in the data, not hidden.

---

## 2. What was measured

- **Documents.** 300 held-out synthetic finance documents (text) and 38 documents in the formats corpus: plain text, CSV, JSON, Markdown, Word, PDF and scans (clean, degraded, and image-only PDF). All synthetic; no real personal data.
- **Answer keys.** The held-out key is the data generator's labels corrected by a documented rule-based audit (242 items added, 34 removed, 167 undecidable texts left unscored); the formats key was written by hand.
- **Setups.** Each of five models with the rules, and each with GLiNER in three ways: its solo finds flagged and left in the text, all flags accepted, and only the correct flags accepted (a perfect reviewer). Plus rules only, GLiNER only, and rules with GLiNER.
- **Scoring is strict.** An item counts as removed only if all of it is gone. Recall is the share of sensitive occurrences removed; precision is the share of removals that were right. Ranges are 95% ranges.
- **Settings.** Temperature 0, fixed seed, reasoning off (the lowest level for gpt-oss), 900-second limit per document, documents read from the input folder.

---

## 3. Headline results

### 3.1 The held-out set (300 documents): the figures to quote

| Setup | Documents done | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Seconds per document |
|---|---:|---:|---:|---:|---:|---:|---:|
| **gemma4:31b** | 299 | **98.0%** (97.2–98.5) | **91.9%** (90.6–93.1) | **94.9%** | 34 of 1,678 | 143 | 10.5 |
| qwen3.6:27b | 300 | **98.0%** (97.3–98.6) | 81.6% (79.8–83.2) | 89.0% | 33 of 1,679 | 370 | 11.8 |
| gpt-oss | 300 | 94.3% (93.1–95.3) | 87.3% (85.7–88.7) | 90.6% | 96 | 228 | 6.6 |
| phi4 | 297 | 94.1% (92.8–95.1) | 74.3% (72.4–76.1) | 83.0% | 99 | 534 | 7.4 |
| gemma4:e4b | 298 | 88.0% (86.3–89.5) | 74.2% (72.2–76.1) | 80.5% | 198 | 497 | 2.3 |
| rules + GLiNER (no model) | 300 | 85.5% (83.8–87.1) | 57.3% (55.4–59.1) | 68.6% | 243 | 1,126 | under 0.1 |
| GLiNER only (no model) | 300 | 81.1% (79.2–82.9) | 56.0% (54.1–57.9) | 66.3% | 317 | 1,128 | under 0.1 |
| rules only (no model) | 300 | 25.0% (23.0–27.1) | 97.2% (95.2–98.4) | 39.8% | 1,259 | 12 | 0.0 |

- gemma4:31b and qwen3.6:27b are **tied on recall**, but gemma4:31b removes far less that it should not (143 against 370 over-redactions), so it is clearly the better all-round model here.
- gpt-oss is the best **speed and accuracy balance**: about 1.6 times faster than gemma4:31b, with about 4 points less recall.
- phi4 and gemma4:e4b are tied on precision. gemma4:e4b is about three times faster than phi4 but misses twice as much.
- Models whose ranges overlap are not clearly different, so treat them in groups: top (gemma4:31b, qwen3.6:27b), middle (gpt-oss, phi4), and then gemma4:e4b.

### 3.2 The formats corpus (38 documents): file-type coverage

| Setup | Recall | Precision | F1 | Missed |
|---|---:|---:|---:|---:|
| gemma4:31b | 99.2% | 91.7% | 95.3% | 3 of 358 |
| qwen3.6:27b | 99.4% | 89.5% | 94.2% | 2 |
| gpt-oss | 98.9% | 98.0% | **98.4%** | 4 |
| phi4 | 98.3% | 97.7% | 98.0% | 6 |
| gemma4:e4b | 93.3% | 94.9% | 94.1% | 24 |
| rules + GLiNER (no model) | 96.1% | 86.2% | 90.9% | 14 |
| rules only (no model) | 55.0% | 100.0% | 71.0% | 161 |

**Do not use these as accuracy figures.** The rules, the organisation-name list and the answer key were all adjusted while looking at these same 38 documents, so the scores are optimistic. Their value is coverage: **every file type is handled.** Word, PDF with a text layer, and all three kinds of scan score at or near 100% recall for every model. Plain text is the weakest for gemma4:e4b (83.1%).

### 3.3 Why the ranking differs between the two sets

phi4 is among the best models on the formats corpus (F1 98.0%) and only fourth on the held-out set (83.0%). The reason is the precision column: on unseen finance documents phi4 removes masked values and generic terms that the answer key does not count as sensitive. **A set chosen by the developers, however carefully, can flatter a model.** That is the reason to keep a held-out set and to quote it.

### 3.4 Speed

Measured on an idle machine, so these timings can be quoted: gemma4:e4b 2.3 s per document, gpt-oss 6.6 s, phi4 7.4 s, gemma4:31b 10.5 s, qwen3.6:27b 11.8 s (held-out). Running all five models over a document takes about 39 seconds. The rules and GLiNER together take well under a tenth of a second.

---

## 4. By category (held-out, recall; number of occurrences in brackets)

| Category | gemma4:31b | qwen3.6:27b | gpt-oss | phi4 | gemma4:e4b | rules only | GLiNER only |
|---|---:|---:|---:|---:|---:|---:|---:|
| PERSON (483) | 100% | 100% | 97.1% | 97.5% | 98.3% | 3.7% | 83.4% |
| COMPANY (423) | 99.1% | 98.1% | 92.9% | **86.0%** | 95.7% | 53.2% | 83.9% |
| ADDRESS (288) | 97.6% | 99.0% | 93.1% | 95.8% | 93.2% | 8.7% | 89.9% |
| ID_NUMBER (271) | 94.5% | 93.4% | 90.0% | 96.3% | **47.8%** | 14.4% | 63.5% |
| EMAIL (88) | 100% | 100% | 100% | 100% | 97.6% | 97.7% | 78.4% |
| PHONE (73) | 100% | 97.3% | 100% | 100% | 97.0% | 24.7% | 97.3% |
| DATE_OF_BIRTH (25) | 80.0% | 100% | 80.0% | 80.0% | 100% | 0% | 52.0% |
| ONLINE_ID (19) | 84.2% | 89.5% | 100% | 100% | 68.4% | 47.4% | 68.4% |
| SECRET (9) | 100% | 100% | 100% | 88.9% | 77.8% | 0% | 77.8% |

- **Names, emails and phone numbers are solved** for the three strongest models: 97 to 100%.
- **ID numbers are the weakest important category** for all models (90 to 96%, and 48% for gemma4:e4b). They have no shape a model can rely on: reference numbers, wallet addresses, account codes.
- **Companies are hard for phi4** (86%), even with the organisation-name rule.
- The smallest categories (dates of birth, online IDs, secrets: 25 or fewer occurrences) have very wide ranges. A figure such as "80%" there means 20 of 25 and should not be read as precise.
- **The rules alone find emails (98%) but almost no names (4%).** This is why the layers are needed.

---

## 5. Where the documents are hard

- **By type (gemma4:31b, held-out):** the weakest are privacy policies (71.4%), real-estate loan agreements (75.0%), FpML (80.0%), cryptocurrency transaction reports (81.8%) and financial data feeds (83.3%), each from 5 documents with 12 to 33 occurrences. Structured and machine-oriented documents are harder than prose. Samples are small.
- **Difficulty ratings** (over the five models): held-out 168 easy, 79 moderate, 53 hard, none a "problem"; formats 31 easy, 1 moderate, 4 hard, 2 problem. The two problem documents are the same meeting notes in Word and Markdown, because of "Kestrel".
- **Missed by the most models:**

| Item | Category | Models that missed it | Likely reason |
|---|---|---:|---|
| Kestrel (a project codename) | CONTEXTUAL | 5 of 5 | A policy question: is a codename sensitive? A client term list answers it |
| 1234567890 | ID_NUMBER | 4 | A placeholder-like number: missed 61 times in 4 documents (counting each model), so the key may be wrong to treat it as sensitive |
| 2019 data breach at the Bristol depot | CONTEXTUAL | 4 | An indirect identifier: a fact that identifies a person without naming them |
| A Bitcoin wallet address, a UUID, GB00B1234567, account codes | ID_NUMBER | 3 each | No recognisable shape |
| Consignee Name, Consignee Postal Code, 338 | various | 3 | **Probably answer-key noise** (field labels and a bare number); not yet checked by hand |

---

## 6. What the models get wrong when they remove too much

These are the over-redactions that most models share (held-out):

| Redacted though not sensitive | Models | Times |
|---|---:|---:|
| "[Company Name]" (a template placeholder) | 5 | 77 |
| Head of People (a job title) | 4 | 17 |
| "1st day of January, 2023" read as a date of birth | 4 | 13 |
| Placeholder-like codes: 0000000000, PIMIUSPB765, BBBBGB2LXXX | 5 each | 10, 10, 5 |
| HM Revenue and Customs, the IRS, the FCA (public bodies) | 4 to 5 | 10, 10, 8 |
| Buyer, Seller, Financial (generic defined terms) | 4 to 5 | 8, 8, 5 |

Two models account for most of the over-redaction by category: **phi4 redacted masked values as secrets 85 times, and phi4 and qwen3.6:27b read ordinary dates as dates of birth 181 and 165 times.** These are the clearest cases for a deterministic rule after the models (never redact bracketed placeholders or masked values, a list of generic defined terms, a date of birth only near "born" or "DOB"). That is the cheapest accuracy gain left, and it mainly lifts phi4 and qwen3.6:27b.

Some "over-redactions" are policy, not error: job titles, ages and public bodies depend on what a client wants kept.

---

## 7. GLiNER

| Setup (held-out) | Recall | Precision | Flags raised (really sensitive) |
|---|---:|---:|---:|
| gemma4:31b | 98.0% | 91.9% | none |
| + GLiNER, flags left in the text | 98.0% | 91.9% | 1,443 (30) |
| + GLiNER, every flag accepted | 99.4% | **59.6%** | 1,443 (30) |
| + GLiNER, only the correct flags accepted | 99.4% | 92.1% | 1,443 (30) |

- GLiNER **never makes the result worse** when its solo finds are only flagged (the first two rows are identical), and with a perfect reviewer it lifts the best model's recall from 98.0% to 99.4%.
- **But its flags are not reviewable as they are.** It flags about 4.8 items per document for gemma4:31b, and only 30 of 1,443 (2%) are really sensitive. Accepting them all costs 32 points of precision. For weaker models the proportion is higher (gemma4:e4b: 139 of 1,409, about 10%), because they miss more.
- It is worth having as a **fast, cheap second detector** with per-category thresholds (it is good at phone numbers and addresses, poor at identifiers and companies). It is not worth having as a review queue until it is made more selective. Once two or more models vote, it adds almost nothing.

---

## 8. Combining models and review

| Combination (held-out) | Recall | Precision | F1 | Flags (really sensitive) |
|---|---:|---:|---:|---:|
| gemma4:31b alone | 98.0% | 91.9% | 94.9% | none |
| gemma4:31b + gpt-oss, either finds it (union) | 99.5% | 85.6% | 92.0% | none |
| both must agree | 92.9% | 95.5% | 94.2% | none |
| both agree, singles flagged, **ideal reviewer** | **99.3%** | **95.8%** | **97.5%** | 307 (104) |
| both agree, singles flagged, no reviewer (flags left in) | 92.9% | 95.5% | 94.2% | 307 (104) |
| 3 models (gemma4:31b, qwen3.6:27b, gpt-oss), majority, automatic | 98.9% | 93.0% | 95.9% | none |
| the same 3, singles flagged, ideal reviewer | 99.8% | 93.1% | 96.3% | 396 (15) |
| all 5, 3 of 5 agree, automatic | 97.8% | 93.5% | 95.6% | none |
| union of 3 | 100.0% | 76.1% | 86.4% | none |

- **Voting between models cuts over-redaction a lot** (precision from 74 to 92% for single models up to 93 to 96%) while keeping recall high.
- The best practical design is **two strong models, with a person deciding where they disagree**: about one item per document to check, and 99.3% recall at 95.8% precision if the person is right.
- **The "ideal reviewer" rows are a best case.** How accurate a real reviewer is has not been measured, and is the largest open question about the review step.
- A union of three models removes everything on this key (100.0% recall) but wrongly redacts about a quarter of what it removes.

---

## 9. Safety of the output and reliability

- **All 1,684 redacted files written passed their checks:** no recoverable text in PDFs, no hidden content or metadata in Word files, and scans read again by OCR with nothing sensitive found.
- **Six model-and-document pairs failed**, all held-out documents. Each is recorded with its message and all four of the model's setups on that document are marked failed:

| Model | Document | What happened |
|---|---|---|
| phi4 | ISDA definition (`heldout-173`) | Timed out after 900 s |
| phi4 | securities prospectus (`heldout-260`) | Timed out after 900 s |
| phi4 and gemma4:31b | EDI message (`heldout-093`) | Ollama returned a server error (500), both times and on a retry |
| gemma4:e4b | CSV (`heldout-068`), loan agreement (`heldout-183`) | The reply was cut off mid-way, on a retry too |

  phi4 therefore covers 335 of 338 documents, gemma4:e4b 336 and gemma4:31b 337. The two phi4 timeouts look like generation that does not finish, not slowness, since the limit was tripled. They are a known weakness of that model on long documents.
- **Repeatability.** The held-out figures for four models match the earlier run of the same code to the decimal. gpt-oss at its lowest reasoning level moved from 88.3% to 87.3% precision, so differences of a point or so for that model should be treated as noise.

---

## 10. What can and cannot be claimed

**Can be claimed**
- On 300 unseen documents the best model removed about 98% of sensitive items with about 92% precision, and layers (rules, models, agreement, review) each have a measured contribution.
- Detection runs entirely on local models. No document text leaves the machine.
- Every result can be inspected: the explorer shows each document's text with what was found, missed and wrongly removed, for every model.
- The output files are checked before release, and they passed.

**Cannot be claimed yet**
- That these figures hold on real documents. All data is synthetic, mostly finance text, and the held-out set has no Word, PDF or scan versions.
- That the human review step reaches the "ideal reviewer" figures. It has not been measured.
- That the held-out answer key is final. It was audited by rules built from what the models agreed on, and has not been checked by a second person. **A person should check a random sample of the audit's decisions before the held-out figures are quoted outside the project.** Some items in the "missed by most" list look like key noise.
- Anything finer than a point or two between models whose ranges overlap, or any claim about the small categories.
- That timings generalise: they are for one laptop with 128 GB of memory.

---

## 11. Recommendations and next steps

1. **Use gemma4:31b as the primary model**, with **gpt-oss as the second model** for agreement, and a person reviewing the disagreements. Keep gemma4:e4b only where speed matters more than recall, and phi4 as a fast option once the clean-up rules are in.
2. **Add the deterministic clean-up rules** in section 6 (masked values, bracketed placeholders, generic defined terms, dates of birth only near birth context, an IPv6 rule) and run the final evaluator again for the affected models. Expected effect: a large rise in precision for phi4 and qwen3.6:27b.
3. **Check a random sample of 100 answer-key audit decisions** by hand, and correct the rules if needed. Then the held-out figures can be quoted externally.
4. **Measure the reviewer.** Have two or three people review the flags on a sample and record how often they agree with the ideal, so the review figures stop being a best case.
5. **Make GLiNER selective:** per-category thresholds and flagging only where it is strong, then re-test.
6. **Grow the held-out set** with Word, PDF and scan versions of unseen documents, and UK-style letters and forms.
7. **Client policy as configuration:** term lists and options for codenames, job titles, ages and public bodies, so the policy-shaped items are decisions a client makes, not model errors.
8. **Investigate the six failures.** The Ollama server error on the EDI message is worth a look on its own (it is the same document for two models), and the phi4 timeouts suggest a retry-with-shorter-chunks fallback for long documents.

---

## 12. How to check or reproduce this

- **Check:** start the web app (`./run-web.sh`), choose **📊 Results**, and use the final dataset (it is opened first). Every figure here comes from it, and every document can be opened.
- **Data:** `datasets/final-20261003` holds `run.json` (code version, environment, settings, answer-key checksums, failures), the CSV files and the document text. The format is described in [RESULTS_DATASET_FORMAT.md](RESULTS_DATASET_FORMAT.md).
- **Reproduce:** [HOW_TO_RUN_EVALUATION.md](HOW_TO_RUN_EVALUATION.md) describes the final evaluator (about 4 hours 20 minutes, resumable) and the preflight check.
