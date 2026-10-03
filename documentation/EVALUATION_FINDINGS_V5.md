# Version 5 findings: the held-out test, a corrected answer key, real combinations

**Version:** 5 (follows [version 4](EVALUATION_FINDINGS_V4.md))
**Reports:** [v5 as first scored](evaluation-reports/v5-20261002-heldout-300docs-generator-key.md) · [v5b with the audited key](evaluation-reports/v5b-20261003-heldout-300docs-audited-key.md) · [key audit](evaluation-reports/v5b-key-audit.md) · combinations ([gemma4:31b + gpt-oss](evaluation-reports/v5b-combos-gemma4-31b-gpt-oss.md), [three models](evaluation-reports/v5b-combos-gemma4-31b-qwen3.6-27b-gpt-oss.md), [phi4 + qwen3.6:27b](evaluation-reports/v5b-combos-phi4-qwen3.6-27b.md)) · [formats re-run (v6)](evaluation-reports/v6-20261003-formats-38docs-5models-gliner.md)
**Runs:** 300 documents the system had never been shaped around (3 h 48 min, five models, with and without GLiNER, plus no-model baselines), and the 38-document format corpus again (1 h 47 min).

---

## 1. Summary in plain terms

1. **The first held-out scores looked bad, and mostly because the answer key was bad.** Scored against the data generator's own labels, recall was 91–95% and precision only about 55%. Reading 108 of the disagreements showed that the generator labels only part of what is sensitive (many addresses, account numbers, first names and phone numbers are unlabelled) and a few of its labels are not sensitive at all (placeholders, the word "address", a row of dashes). The models were often right and the key wrong.
2. **After a documented audit of the key, the held-out results are:** gemma4:31b **98.0% recall, 91.9% precision (F1 94.9%)**; qwen3.6:27b 98.0% / 81.6%; gpt-oss 94.3% / 88.3%; phi4 94.1% / 74.3%; gemma4:e4b 88.0% / 74.2%. Fixed rules alone: 25% recall at 97% precision. GLiNER alone: 81% recall at 56% precision.
3. **The model ranking changed.** On our own (tuned) documents phi4 was the best all-round model; on unseen finance documents it is only fourth, because it over-redacts placeholders and generic words. gemma4:31b is best here. A single corpus, or one chosen by the developers, can mislead; this is the reason to keep a held-out set.
4. **Combining models helps recall and precision more than any single model, with the caveat in section 5:** two models voting with a person checking the disagreements reached about 99.4% recall at 97% precision, and three models voting automatically about 99.0% / 92.6%.
5. **GLiNER's flags are far noisier on unseen documents than they looked.** About three flags per document with only 3–14% really sensitive, and almost nothing it adds once two models vote. Its value is as a fast, cheap second detector that needs a per-category threshold, not as a review queue as it stands.
6. **What remains wrong is specific and fixable** (section 6): masked placeholders, generic defined terms ("Borrower", "Corporation"), and arbitrary dates read as dates of birth.

## 2. What the first scoring showed, and why it was misleading

| | Recall | Precision | F1 |
|---|---:|---:|---:|
| phi4 | 92.7% | 53.7% | 68.0% |
| gemma4:31b | 94.6% | 55.8% | 70.2% |
| gpt-oss | 90.6% | 60.5% | 72.5% |

I took every item that at least 3 of the 5 models redacted and the key did not list (467 of them), and a random sample of 70, and read each in its sentence. **57% were genuinely sensitive and missing from the key** (street addresses, account and sort-code numbers, wallet addresses, first and last names in a customer table, phone numbers, an API key); **24% were true over-redactions** (placeholders like `[Company Name]`, public bodies such as HMRC and the FRC, reference codes, a renewal date, a securities number); **19% were policy categories the key never labels** (gender words, job titles, ages). Of the 38 key items that most models missed, **only about 4 to 6 were real misses** (an address in a statement, a string shaped like an IPv6 address). The rest were noise in the key: lower-case field words like "address", a line of dashes, a bare city, the last digits of a masked card.

## 3. The audit, and how far to trust it

Written rules (listed in the [audit log](evaluation-reports/v5b-key-audit.md)) decided each disagreement where at least 3 of 5 models agreed: **242 sensitive items added to the key, 34 non-sensitive items removed, 58 real over-redactions left standing, and 167 texts a rule could not decide left out of scoring**. The key now also says which categories it judges: a redaction of gender, age, job title, domain or company number is no longer counted as wrong, because the generator never labelled them. The generator's original key is kept alongside.

The rules agreed with my hand reading of 108 disagreements **91% of the time**, and nearly all of the differences were cases the rules left undecided, not wrong calls. Two things limit the audit:

- **It is built from the models' agreement.** It only added items most models agreed on, so scores for voting between those same models are somewhat flattering, and the audit has not been checked by a second person. **A person should check a random sample of 100 decisions before these figures are quoted externally.**
- The audit was written after seeing the first run. The *system* was not changed in response (nothing in the rules, prompts or thresholds), only the key.

## 4. Results on the audited key (300 documents, 1,679 occurrences)

| | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Time per document |
|---|---:|---:|---:|---:|---:|---:|
| rules only | 25.0% (23.0–27.1) | 97.2% (95.2–98.4) | 39.8% | 1259 | 12 | – |
| GLiNER only | 81.1% (79.2–82.9) | 56.0% (54.1–57.9) | 66.3% | 317 | 1128 | 0.04 s |
| phi4 | 94.1% (92.8–95.1) | 74.3% (72.4–76.1) | 83.0% | 99 | 534 | 7.6 s |
| gemma4:e4b | 88.0% (86.3–89.5) | 74.2% (72.2–76.1) | 80.5% | 198 | 497 | 2.4 s |
| qwen3.6:27b | **98.0%** (97.3–98.6) | 81.6% (79.8–83.2) | 89.0% | 33 | 370 | 13.7 s |
| gemma4:31b | **98.0%** (97.2–98.6) | **91.9%** (90.6–93.0) | **94.9%** | 34 | 143 | 12.1 s |
| gpt-oss | 94.3% (93.1–95.3) | 88.3% (86.8–89.7) | 91.2% | 95 | 207 | 6.7 s |

- **Recall by category** is high for emails (100% for most models), phone numbers (97–100%), names (96–100% for four models) and addresses (92–99%). The weakest areas are **identifier numbers for gemma4:e4b (48%)**, **dates of birth (80% for three models)** and **companies for phi4 (86%)**.
- **Weakest document types** for the best model: privacy policies (71%), real-estate loan agreements (75%), FpML (80%) and cryptocurrency transaction reports (82%).
- **Over-redaction**: gemma4:31b redacts least (143); gemma4:e4b and phi4 most.
- Ranges are wide enough that gemma4:31b and qwen3.6:27b are tied on recall, and phi4 and gemma4:e4b are tied on precision.

## 5. Combinations (scored from the saved spans, no model re-run)

| Combination | Recall | Precision | F1 | Notes |
|---|---:|---:|---:|---|
| gemma4:31b alone | 98.0% | 91.9% | 94.9% | reference |
| union of gemma4:31b + gpt-oss | 99.5% | 85.6% | 92.0% | nearly nothing missed (8 of 1,678), at a cost in precision |
| both must agree: gemma4:31b + gpt-oss | 93.0% | 96.6% | 94.8% | |
| **agree, singles flagged, ideal reviewer accepts the correct flags** | **99.4%** | **96.8%** | **98.1%** | 327 flags across 300 documents (about 1 per document), 106 really sensitive |
| majority of 3 (gemma4:31b, qwen3.6:27b, gpt-oss), fully automatic | 99.0% | 92.6% | 95.7% | |
| majority of 3 with singles flagged and an ideal reviewer | 99.9% | 92.6% | 96.1% | 386 flags, 14 sensitive |
| union of all three | 100.0% (0 missed) | 76.6% | 86.7% | |

- **Voting between models cuts over-redaction a lot** (precision from 82–92% to about 93–97%) while keeping recall high, and the disagreements are a manageable review queue (about one per document).
- **The ideal-reviewer rows assume the reviewer accepts exactly the right flags.** A real person will sometimes err; this has not been measured.
- **Caveat on the audit (section 3):** these rows are the most favourable to voting. Treat the exact figures as an upper estimate until a person has checked a sample of the key.

## 6. Where the models are really wrong, and cheap fixes

The over-redactions left after the audit are real model errors, and they cluster:

| Pattern | Example | Fix |
|---|---|---|
| Masked values redacted as secrets | `XXXXXXXXXXXXXXXX` (phi4: 77 times) | Never redact a value made only of repeated mask characters or `*` |
| Placeholders redacted as companies, dates or links | `[Company Name]`, `MM/DD/YYYY`, `[Insert Link]` | Never redact text in square brackets or a date template |
| Generic defined terms redacted as companies | "Corporation", "Borrower", "Shareholder", "Firm", "Supplier" | A list of defined terms that are never names |
| Any date read as a date of birth | renewal and due dates | Only call a date a date of birth near "born", "birth" or "DOB", or in a column named so |
| IPv6-style strings missed | `a741:45da:c53e:...` | An IPv6 rule next to the IPv4 rule |

These are deterministic rules that run after the models and are the cheapest accuracy gain left. They would have been the first things found by a held-out test, which is the point of having one.

## 7. GLiNER, revised

- On unseen documents it **alone** reaches 81% recall (85% with the rules) at 56% precision: a useful cheap first pass, not a redactor.
- As a **second opinion** it raised about 960–1,060 flags per model (about 3 per document) with 3–14% really sensitive, far noisier than the 1 per document seen on the small corpus. It finds items a *single* model missed, but once two or three models vote it adds almost nothing (3 correct flags of 937 on top of two models).
- To be useful as a review queue it needs **per-category thresholds** (it is good at phone numbers and addresses, poor at companies and identifiers) and to flag only where it is strong. That is the next GLiNER experiment, not more models.

## 8. The format corpus re-run (v6)

- Scores are as before for the same models (deterministic), with every redacted file checked: **all outputs passed the safety checks.**
- **Do not use the timings from this run.** phi4 took 92 s per document and one phi4 request hit the 300-second limit (so phi4 scored 37 of 38 documents), and gemma4:e4b and qwen3.6:27b were 2 to 6 times slower than in earlier runs. The machine was probably busy with something else during this run. The held-out run's timings match earlier measurements.
- One value shifted without a code change: gpt-oss moved from 97.2% recall in an earlier run to 98.9% here. gpt-oss at its lowest reasoning level is not perfectly repeatable, so differences of a point or two for it should be treated as noise.
- The format corpus was used to build the rules, so it shows format coverage (Word, PDF, scans), not unbiased accuracy.

## 9. Recommendations and next steps

1. **Add the deterministic clean-up rules in section 6** and re-run the held-out test. Expected effect: phi4 and qwen3.6:27b gain 10 or more points of precision.
2. **Have a person check a random sample of 100 audit decisions**, then correct the rules if needed, so the held-out figures can be quoted.
3. **Prefer gemma4:31b as the primary model for the demo of accuracy**, and show agreement between two models (gemma4:31b with gpt-oss) with a person reviewing the disagreements. Keep phi4 as the fast option once the clean-up rules are in.
4. **GLiNER:** per-category thresholds, and test it only as a cheap first pass or a flagger in categories where it is precise.
5. **Fix the evaluation environment before the next timing run:** nothing else running, and a longer request limit for slow documents (the limit is `llm.timeoutSeconds`).
6. **Grow the held-out set** with Word, PDF and scan versions of a sample, and with UK-style letters and forms, since this set is generated finance text.
