# Combination report

Built from `v5-heldout.scores.json` (2026-10-02 18:53) without running any model again: each detector's saved spans are combined and scored on 297 documents of `/Users/davidberry/sources/CSharpe/AiDocumentRedactor/tests/HeldOutCorpus` with the same strict scoring as the evaluation. Models: phi4, qwen3.6:27b, and GLiNER. Text-readable documents only (no OCR).

**How to read the rows.** *alone*: the model by itself (with the fixed rules). *union*: redact whatever any model found. *all agree*: redact only what every model found. *majority*: what most models found. *singles flagged*: what two or more models found is redacted and what only one found is flagged for review and left in the text. *+ GLiNER flags*: the models' union, with anything only GLiNER found flagged. *vote*: groups found by at least two of the detectors, GLiNER counting as one. Rows marked *(correct flags accepted)* show an ideal reviewer who accepts only the flags that really were sensitive; the other rows leave every flag in the text (so flagged items count as missed).

| Combination | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Must-keep damaged | Flags raised (really sensitive) |
|---|---:|---:|---:|---:|---:|---:|---:|
| phi4 alone | 94.1% (92.8–95.1) | 74.3% (72.4–76.1) | 83.0% | 99 of 1672 | 534 of 2077 | – | – |
| qwen3.6:27b alone | 98.0% (97.2–98.6) | 81.7% (79.9–83.3) | 89.1% | 33 of 1672 | 365 of 1991 | – | – |
| union: phi4 + qwen3.6:27b | 99.6% (99.2–99.8) | 68.4% (66.5–70.2) | 81.1% | 6 of 1672 | 755 of 2387 | – | – |
| all 2 agree: phi4 + qwen3.6:27b | 92.4% (91.0–93.6) | 92.3% (90.9–93.5) | 92.3% | 127 of 1672 | 127 of 1642 | – | – |
| singles flagged (2 or more redact, 1 flags): phi4 + qwen3.6:27b | 92.4% (91.0–93.6) | 92.3% (90.9–93.5) | 92.3% | 127 of 1672 | 127 of 1642 | – | 741 (116) |
| singles flagged (2 or more redact, 1 flags): phi4 + qwen3.6:27b (correct flags accepted) | 99.4% (98.9–99.7) | 92.8% (91.5–93.9) | 96.0% | 10 of 1672 | 127 of 1758 | – | 741 (116) |
| union + GLiNER flags | 99.5% (99.0–99.7) | 68.6% (66.7–70.4) | 81.2% | 9 of 1672 | 747 of 2376 | – | 857 (1) |
| union + GLiNER flags (correct flags accepted) | 99.5% (99.0–99.7) | 68.6% (66.7–70.4) | 81.2% | 9 of 1672 | 747 of 2377 | – | 857 (1) |
| vote (2 of 3, singles flagged): phi4 + qwen3.6:27b + GLiNER | 96.2% (95.1–97.0) | 85.0% (83.3–86.6) | 90.2% | 64 of 1672 | 278 of 1855 | – | 1436 (46) |
| vote (2 of 3, singles flagged): phi4 + qwen3.6:27b + GLiNER (correct flags accepted) | 98.9% (98.2–99.3) | 85.4% (83.7–86.9) | 91.6% | 19 of 1672 | 278 of 1901 | – | 1436 (46) |

Ranges treat items as independent, so they are a little optimistic. Differences smaller than the ranges are not reliable.
