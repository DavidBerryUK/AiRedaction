# Combination report

Built from `v5-heldout.scores.json` (2026-10-02 18:53) without running any model again: each detector's saved spans are combined and scored on 299 documents of `/Users/davidberry/sources/CSharpe/AiDocumentRedactor/tests/HeldOutCorpus` with the same strict scoring as the evaluation. Models: gemma4:31b, gpt-oss, and GLiNER. Text-readable documents only (no OCR).

**How to read the rows.** *alone*: the model by itself (with the fixed rules). *union*: redact whatever any model found. *all agree*: redact only what every model found. *majority*: what most models found. *singles flagged*: what two or more models found is redacted and what only one found is flagged for review and left in the text. *+ GLiNER flags*: the models' union, with anything only GLiNER found flagged. *vote*: groups found by at least two of the detectors, GLiNER counting as one. Rows marked *(correct flags accepted)* show an ideal reviewer who accepts only the flags that really were sensitive; the other rows leave every flag in the text (so flagged items count as missed).

| Combination | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Must-keep damaged | Flags raised (really sensitive) |
|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b alone | 98.0% (97.2–98.5) | 91.9% (90.6–93.1) | 94.9% | 34 of 1678 | 143 of 1774 | – | – |
| gpt-oss alone | 94.3% (93.1–95.3) | 88.6% (87.0–90.0) | 91.4% | 95 of 1678 | 202 of 1768 | – | – |
| union: gemma4:31b + gpt-oss | 99.5% (99.1–99.8) | 85.6% (84.0–87.1) | 92.0% | 8 of 1678 | 276 of 1918 | – | – |
| all 2 agree: gemma4:31b + gpt-oss | 93.0% (91.7–94.2) | 96.6% (95.6–97.4) | 94.8% | 117 of 1678 | 54 of 1589 | – | – |
| singles flagged (2 or more redact, 1 flags): gemma4:31b + gpt-oss | 93.0% (91.7–94.2) | 96.6% (95.6–97.4) | 94.8% | 117 of 1678 | 54 of 1589 | – | 327 (106) |
| singles flagged (2 or more redact, 1 flags): gemma4:31b + gpt-oss (correct flags accepted) | 99.4% (98.9–99.7) | 96.8% (95.9–97.6) | 98.1% | 10 of 1678 | 54 of 1695 | – | 327 (106) |
| union + GLiNER flags | 99.5% (99.1–99.8) | 85.9% (84.2–87.4) | 92.2% | 8 of 1678 | 270 of 1912 | – | 937 (3) |
| union + GLiNER flags (correct flags accepted) | 99.7% (99.3–99.9) | 85.9% (84.3–87.4) | 92.3% | 5 of 1678 | 270 of 1915 | – | 937 (3) |
| vote (2 of 3, singles flagged): gemma4:31b + gpt-oss + GLiNER | 97.2% (96.3–97.9) | 92.1% (90.7–93.3) | 94.6% | 47 of 1678 | 138 of 1742 | – | 1168 (31) |
| vote (2 of 3, singles flagged): gemma4:31b + gpt-oss + GLiNER (correct flags accepted) | 99.0% (98.4–99.4) | 92.2% (90.9–93.4) | 95.5% | 17 of 1678 | 138 of 1773 | – | 1168 (31) |

Ranges treat items as independent, so they are a little optimistic. Differences smaller than the ranges are not reliable.
