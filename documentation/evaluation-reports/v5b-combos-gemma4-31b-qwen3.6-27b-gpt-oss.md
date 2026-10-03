# Combination report

Built from `v5-heldout.scores.json` (2026-10-02 18:53) without running any model again: each detector's saved spans are combined and scored on 299 documents of `/Users/davidberry/sources/CSharpe/AiDocumentRedactor/tests/HeldOutCorpus` with the same strict scoring as the evaluation. Models: gemma4:31b, qwen3.6:27b, gpt-oss, and GLiNER. Text-readable documents only (no OCR).

**How to read the rows.** *alone*: the model by itself (with the fixed rules). *union*: redact whatever any model found. *all agree*: redact only what every model found. *majority*: what most models found. *singles flagged*: what two or more models found is redacted and what only one found is flagged for review and left in the text. *+ GLiNER flags*: the models' union, with anything only GLiNER found flagged. *vote*: groups found by at least two of the detectors, GLiNER counting as one. Rows marked *(correct flags accepted)* show an ideal reviewer who accepts only the flags that really were sensitive; the other rows leave every flag in the text (so flagged items count as missed).

| Combination | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Must-keep damaged | Flags raised (really sensitive) |
|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b alone | 98.0% (97.2–98.5) | 91.9% (90.6–93.1) | 94.9% | 34 of 1678 | 143 of 1774 | – | – |
| qwen3.6:27b alone | 98.0% (97.3–98.6) | 81.6% (79.9–83.3) | 89.1% | 33 of 1678 | 368 of 2004 | – | – |
| gpt-oss alone | 94.3% (93.1–95.3) | 88.6% (87.0–90.0) | 91.4% | 95 of 1678 | 202 of 1768 | – | – |
| union: gemma4:31b + qwen3.6:27b + gpt-oss | 100.0% (99.8–100.0) | 76.6% (74.7–78.3) | 86.7% | 0 of 1678 | 505 of 2154 | – | – |
| all 3 agree: gemma4:31b + qwen3.6:27b + gpt-oss | 91.4% (90.0–92.7) | 97.0% (96.0–97.7) | 94.1% | 144 of 1678 | 47 of 1554 | – | – |
| majority (2 of 3): gemma4:31b + qwen3.6:27b + gpt-oss | 99.0% (98.5–99.4) | 92.6% (91.3–93.7) | 95.7% | 16 of 1678 | 131 of 1765 | – | – |
| singles flagged (2 or more redact, 1 flags): gemma4:31b + qwen3.6:27b + gpt-oss | 99.0% (98.5–99.4) | 92.6% (91.3–93.7) | 95.7% | 16 of 1678 | 131 of 1765 | – | 386 (14) |
| singles flagged (2 or more redact, 1 flags): gemma4:31b + qwen3.6:27b + gpt-oss (correct flags accepted) | 99.9% (99.6–100.0) | 92.6% (91.3–93.8) | 96.1% | 2 of 1678 | 131 of 1779 | – | 386 (14) |
| union + GLiNER flags | 100.0% (99.8–100.0) | 76.8% (74.9–78.5) | 86.9% | 0 of 1678 | 499 of 2148 | – | 907 (0) |
| union + GLiNER flags (correct flags accepted) | 100.0% (99.8–100.0) | 76.8% (74.9–78.5) | 86.9% | 0 of 1678 | 499 of 2148 | – | 907 (0) |
| vote (2 of 4, singles flagged): gemma4:31b + qwen3.6:27b + gpt-oss + GLiNER | 98.7% (98.1–99.2) | 88.0% (86.4–89.4) | 93.1% | 21 of 1678 | 222 of 1851 | – | 1261 (9) |
| vote (2 of 4, singles flagged): gemma4:31b + qwen3.6:27b + gpt-oss + GLiNER (correct flags accepted) | 99.3% (98.8–99.6) | 88.1% (86.5–89.5) | 93.3% | 12 of 1678 | 222 of 1860 | – | 1261 (9) |

Ranges treat items as independent, so they are a little optimistic. Differences smaller than the ranges are not reliable.
