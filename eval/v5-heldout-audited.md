# Redaction evaluation report

> Rescored from the spans of the held-out run of 2 October 2026, against the **audited** answer key (see `tests/HeldOutCorpus/README.md` and `eval/v5-key-audit.md`). No model was run again.

Run on 2026-10-02 18:53 (macOS 27.0.1, 18 cores); took 03:47:38. 300 documents, 1141 items on the answer key, 23 models. Everything ran on this machine through local models.

## Summary

| Model | Size | Recall (95% range) | Precision (95% range) | F1 | Sensitive items missed | Over-redactions | Must-keep items damaged | Time per document | Output tokens/s |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| rules only | – | 25.0% (23.0–27.1) | 97.2% (95.2–98.4) | 39.8% | 1259 of 1679 | 12 of 430 | – | 0:00.0 | 0 |
| GLiNER only | – | 81.1% (79.2–82.9) | 56.0% (54.1–57.9) | 66.3% | 317 of 1679 | 1128 of 2565 | – | 0:00.0 | 0 |
| rules + GLiNER | – | 85.5% (83.8–87.1) | 57.3% (55.4–59.1) | 68.6% | 243 of 1679 | 1126 of 2635 | – | 0:00.0 | 0 |
| phi4 | 14.7B · 9.1 GB | 94.1% (92.8–95.1) | 74.3% (72.4–76.1) | 83.0% | 99 of 1672 | 534 of 2077 | – | 0:07.6 | 42 |
| phi4 + GLiNER | 14.7B · 9.1 GB | 94.1% (92.8–95.1) | 74.3% (72.4–76.1) | 83.0% | 99 of 1672 | 534 of 2077 | – | 0:07.6 | 42 |
| phi4 + GLiNER (all flags accepted) | 14.7B · 9.1 GB | 96.5% (95.5–97.3) | 52.4% (50.6–54.1) | 67.9% | 58 of 1672 | 1445 of 3033 | – | 0:07.6 | 42 |
| phi4 + GLiNER (correct flags accepted) | 14.7B · 9.1 GB | 96.5% (95.5–97.3) | 74.8% (72.9–76.6) | 84.3% | 58 of 1672 | 534 of 2122 | – | 0:07.6 | 42 |
| gemma4:e4b | 7.5B · 6.6 GB | 88.0% (86.3–89.5) | 74.2% (72.2–76.1) | 80.5% | 198 of 1646 | 497 of 1924 | – | 0:02.4 | 144 |
| gemma4:e4b + GLiNER | 7.5B · 6.6 GB | 88.0% (86.3–89.5) | 74.2% (72.2–76.1) | 80.5% | 198 of 1646 | 497 of 1924 | – | 0:02.4 | 144 |
| gemma4:e4b + GLiNER (all flags accepted) | 7.5B · 6.6 GB | 95.7% (94.6–96.6) | 54.0% (52.2–55.8) | 69.1% | 71 of 1646 | 1333 of 2899 | – | 0:02.4 | 144 |
| gemma4:e4b + GLiNER (correct flags accepted) | 7.5B · 6.6 GB | 95.7% (94.6–96.6) | 75.9% (74.0–77.7) | 84.7% | 71 of 1646 | 497 of 2063 | – | 0:02.4 | 144 |
| qwen3.6:27b | 27.3B · 17.8 GB | 98.0% (97.3–98.6) | 81.6% (79.8–83.2) | 89.0% | 33 of 1679 | 370 of 2007 | – | 0:13.7 | 33 |
| qwen3.6:27b + GLiNER | 27.3B · 17.8 GB | 98.0% (97.3–98.6) | 81.6% (79.8–83.2) | 89.0% | 33 of 1679 | 370 of 2007 | – | 0:13.7 | 33 |
| qwen3.6:27b + GLiNER (all flags accepted) | 27.3B · 17.8 GB | 99.3% (98.8–99.6) | 55.1% (53.4–56.9) | 70.9% | 11 of 1679 | 1355 of 3020 | – | 0:13.7 | 33 |
| qwen3.6:27b + GLiNER (correct flags accepted) | 27.3B · 17.8 GB | 99.3% (98.8–99.6) | 81.8% (80.1–83.4) | 89.7% | 11 of 1679 | 370 of 2035 | – | 0:13.7 | 33 |
| gemma4:31b | 30.7B · 20.4 GB | 98.0% (97.2–98.5) | 91.9% (90.6–93.1) | 94.9% | 34 of 1678 | 143 of 1774 | – | 0:12.1 | 27 |
| gemma4:31b + GLiNER | 30.7B · 20.4 GB | 98.0% (97.2–98.5) | 91.9% (90.6–93.1) | 94.9% | 34 of 1678 | 143 of 1774 | – | 0:12.1 | 27 |
| gemma4:31b + GLiNER (all flags accepted) | 30.7B · 20.4 GB | 99.4% (98.9–99.7) | 59.6% (57.7–61.4) | 74.5% | 10 of 1678 | 1128 of 2789 | – | 0:12.1 | 27 |
| gemma4:31b + GLiNER (correct flags accepted) | 30.7B · 20.4 GB | 99.4% (98.9–99.7) | 92.1% (90.7–93.2) | 95.6% | 10 of 1678 | 143 of 1804 | – | 0:12.1 | 27 |
| gpt-oss | 20.9B · 13.8 GB | 94.3% (93.1–95.3) | 88.3% (86.8–89.7) | 91.2% | 95 of 1679 | 207 of 1774 | – | 0:06.7 | 101 |
| gpt-oss + GLiNER | 20.9B · 13.8 GB | 94.3% (93.1–95.3) | 88.3% (86.8–89.7) | 91.2% | 95 of 1679 | 207 of 1774 | – | 0:06.7 | 101 |
| gpt-oss + GLiNER (all flags accepted) | 20.9B · 13.8 GB | 98.2% (97.5–98.7) | 57.8% (55.9–59.6) | 72.7% | 30 of 1679 | 1197 of 2834 | – | 0:06.7 | 101 |
| gpt-oss + GLiNER (correct flags accepted) | 20.9B · 13.8 GB | 98.2% (97.5–98.7) | 88.8% (87.3–90.1) | 93.3% | 30 of 1679 | 207 of 1844 | – | 0:06.7 | 101 |

### What the columns mean

- **Recall** answers: *of everything that should have been hidden, how much did the model hide?* If a document has 100 sensitive items and the model hides 95, recall is 95%. The other 5 are leaks, so for a redaction tool this is the most important number. It is strict: hiding only the surname of "Jane Smith" leaves the first name visible and counts as a miss.
- **Precision** answers: *of everything the model hid, how much really needed hiding?* If it hides 100 things and 90 were sensitive, precision is 90%. The other 10 are over-redactions: harmless, but they make the document harder to read.
- **F1** is a single score that blends recall and precision. It is high only when both are high, so a model cannot score well by hiding everything (perfect recall, poor precision) or by hiding almost nothing (high precision, poor recall). Use it for a quick ranking, but look at recall first.
- **95% range** is how far a figure could move if the same test were run on a different, similar set of documents. A wide range means the test is too small to tell models apart; when two ranges overlap, the models are not clearly different. (Items in one document are not independent, so the ranges are a little optimistic.)
- **Sensitive items missed** is the count behind recall ("3 of 120" means 3 sensitive items were left visible).
- **Over-redactions** is the count behind precision ("8 of 130" means 8 of the 130 redactions covered text that did not need hiding).
- **Must-keep items damaged** (also called *preserved*) checks the opposite risk. Each test document contains ordinary text that must survive, such as dates, job titles, amounts, product names and general places. This counts how many of those were wrongly removed. "0 of 40" is ideal, and each one damaged is information the reader needed that is now gone.
- **Time per document** is the average wall-clock time to redact one document, and **Output tokens/s** is how fast the model writes its answer (a hardware and model-size measure).

## Detail per model

Where each model's time and effort went, and how many documents it could not process. *Items fully caught* counts whole items (a full name is one item) rather than every occurrence; *label accuracy* is the share of correct redactions given the right category; *lost to OCR* counts items the scan reader never produced, which no model could have found. For rows that include **GLiNER**, *flagged for review* counts things only GLiNER found, which are left in the text for a person to check, with how many of them really were sensitive. A row marked *(all flags accepted)* shows the result if the reviewer accepted every flag, and *(correct flags accepted)* if the reviewer accepted only the flags that really were sensitive. **Rows without a model** (*rules only*, *GLiNER only*, *rules + GLiNER*) show what the cheap layers do alone, which is what the language model has to improve on.

| Model | Documents scored | Documents failed | Total model time | Median document | Slowest document | Prompt tokens | Output tokens | Items fully caught | Label accuracy | Lost to OCR | Flagged for review (really sensitive) | GLiNER time |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| rules only | 300 | 0 | 0:00:00 | 0.0 s | 0.0 s (text/heldout-001-annual-report.txt) | 0 | 0 | 279 of 1140 | 93.1% | 1 | – | – |
| GLiNER only | 300 | 0 | 0:00:12 | 0.0 s | 0.1 s (text/heldout-016-bank-statement.txt) | 0 | 0 | 885 of 1140 | 70.5% | 1 | – | 13.0 s |
| rules + GLiNER | 300 | 0 | 0:00:13 | 0.0 s | 0.1 s (text/heldout-016-bank-statement.txt) | 0 | 0 | 952 of 1140 | 74.6% | 1 | – | 13.0 s |
| phi4 | 297 | 3 | 0:37:51 | 6.8 s | 102.7 s (text/heldout-069-csv.txt) | 319,319 | 94,767 | 1060 of 1133 | 94.4% | 1 | – | – |
| phi4 + GLiNER | 297 | 0 | 0:37:51 | 6.8 s | 102.7 s (text/heldout-069-csv.txt) | 319,319 | 94,767 | 1060 of 1133 | 94.4% | 1 | 956 (45) | 12.8 s |
| phi4 + GLiNER (all flags accepted) | 297 | 0 | 0:37:51 | 6.8 s | 102.7 s (text/heldout-069-csv.txt) | 319,319 | 94,767 | 1090 of 1133 | 93.8% | 1 | 956 (45) | 12.8 s |
| phi4 + GLiNER (correct flags accepted) | 297 | 0 | 0:37:51 | 6.8 s | 102.7 s (text/heldout-069-csv.txt) | 319,319 | 94,767 | 1090 of 1133 | 93.8% | 1 | 956 (45) | 12.8 s |
| gemma4:e4b | 298 | 2 | 0:11:57 | 2.0 s | 42.5 s (text/heldout-069-csv.txt) | 343,842 | 103,552 | 979 of 1107 | 95.2% | 1 | – | – |
| gemma4:e4b + GLiNER | 298 | 0 | 0:11:57 | 2.0 s | 42.5 s (text/heldout-069-csv.txt) | 343,842 | 103,552 | 979 of 1107 | 95.2% | 1 | 975 (139) | 12.8 s |
| gemma4:e4b + GLiNER (all flags accepted) | 298 | 0 | 0:11:57 | 2.0 s | 42.5 s (text/heldout-069-csv.txt) | 343,842 | 103,552 | 1056 of 1107 | 88.6% | 1 | 975 (139) | 12.8 s |
| gemma4:e4b + GLiNER (correct flags accepted) | 298 | 0 | 0:11:57 | 2.0 s | 42.5 s (text/heldout-069-csv.txt) | 343,842 | 103,552 | 1056 of 1107 | 88.6% | 1 | 975 (139) | 12.8 s |
| qwen3.6:27b | 300 | 0 | 1:08:33 | 10.8 s | 93.4 s (text/heldout-069-csv.txt) | 342,502 | 137,654 | 1129 of 1140 | 95.7% | 1 | – | – |
| qwen3.6:27b + GLiNER | 300 | 0 | 1:08:33 | 10.8 s | 93.4 s (text/heldout-069-csv.txt) | 342,502 | 137,654 | 1129 of 1140 | 95.7% | 1 | 1013 (28) | 13.0 s |
| qwen3.6:27b + GLiNER (all flags accepted) | 300 | 0 | 1:08:33 | 10.8 s | 93.4 s (text/heldout-069-csv.txt) | 342,502 | 137,654 | 1134 of 1140 | 94.3% | 1 | 1013 (28) | 13.0 s |
| qwen3.6:27b + GLiNER (correct flags accepted) | 300 | 0 | 1:08:33 | 10.8 s | 93.4 s (text/heldout-069-csv.txt) | 342,502 | 137,654 | 1134 of 1140 | 94.3% | 1 | 1013 (28) | 13.0 s |
| gemma4:31b | 299 | 1 | 1:00:27 | 9.6 s | 83.4 s (text/heldout-069-csv.txt) | 345,081 | 97,286 | 1114 of 1139 | 95.5% | 1 | – | – |
| gemma4:31b + GLiNER | 299 | 0 | 1:00:27 | 9.6 s | 83.4 s (text/heldout-069-csv.txt) | 345,081 | 97,286 | 1114 of 1139 | 95.5% | 1 | 1015 (30) | 12.9 s |
| gemma4:31b + GLiNER (all flags accepted) | 299 | 0 | 1:00:27 | 9.6 s | 83.4 s (text/heldout-069-csv.txt) | 345,081 | 97,286 | 1134 of 1139 | 94.4% | 1 | 1015 (30) | 12.9 s |
| gemma4:31b + GLiNER (correct flags accepted) | 299 | 0 | 1:00:27 | 9.6 s | 83.4 s (text/heldout-069-csv.txt) | 345,081 | 97,286 | 1134 of 1139 | 94.4% | 1 | 1015 (30) | 12.9 s |
| gpt-oss | 300 | 0 | 0:33:43 | 4.2 s | 67.5 s (text/heldout-271-swift-message.txt) | 376,818 | 203,832 | 1083 of 1140 | 96.8% | 1 | – | – |
| gpt-oss + GLiNER | 300 | 0 | 0:33:43 | 4.2 s | 67.5 s (text/heldout-271-swift-message.txt) | 376,818 | 203,832 | 1083 of 1140 | 96.8% | 1 | 1060 (70) | 13.0 s |
| gpt-oss + GLiNER (all flags accepted) | 300 | 0 | 0:33:43 | 4.2 s | 67.5 s (text/heldout-271-swift-message.txt) | 376,818 | 203,832 | 1117 of 1140 | 94.7% | 1 | 1060 (70) | 13.0 s |
| gpt-oss + GLiNER (correct flags accepted) | 300 | 0 | 0:33:43 | 4.2 s | 67.5 s (text/heldout-271-swift-message.txt) | 376,818 | 203,832 | 1117 of 1140 | 94.7% | 1 | 1060 (70) | 13.0 s |

## Headline findings

- **Best at finding sensitive data:** qwen3.6:27b, removing 98.0% of items (33 missed) with 81.6% precision.
- **Best balance (F1):** gemma4:31b at 94.9%. **Fastest:** gemma4:e4b at about 2.4 s per document.
- **Weakest category for qwen3.6:27b:** ONLINE_ID (89.5%, 2 of 19 missed).
- **Hardest format:** Plain text (98.0% recall).
- **Over-redaction check:** qwen3.6:27b left every must-keep item (product names, public bodies, places, ordinary numbers) untouched.

## Recall by category

| Category | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| ADDRESS | 288 | 8.7% (25/288) | 89.9% (259/288) | 95.8% (274/286) | 93.2% (261/280) | 99.0% (285/288) | 97.6% (280/287) | 92.0% (265/288) |
| COMPANY | 423 | 53.2% (225/423) | 83.9% (355/423) | 86.0% (363/422) | 95.7% (404/422) | 98.1% (415/423) | 99.1% (419/423) | 93.1% (394/423) |
| DATE_OF_BIRTH | 25 | 0.0% (0/25) | 52.0% (13/25) | 80.0% (20/25) | 100.0% (19/19) | 100.0% (25/25) | 80.0% (20/25) | 80.0% (20/25) |
| EMAIL | 88 | 97.7% (86/88) | 78.4% (69/88) | 100.0% (88/88) | 97.6% (80/82) | 100.0% (88/88) | 100.0% (88/88) | 100.0% (88/88) |
| ID_NUMBER | 271 | 14.4% (39/271) | 63.5% (172/271) | 96.3% (260/270) | 47.8% (129/270) | 93.4% (253/271) | 94.5% (256/271) | 93.0% (252/271) |
| ONLINE_ID | 19 | 47.4% (9/19) | 68.4% (13/19) | 100.0% (19/19) | 68.4% (13/19) | 89.5% (17/19) | 84.2% (16/19) | 94.7% (18/19) |
| PERSON | 483 | 3.7% (18/483) | 83.4% (403/483) | 97.5% (468/480) | 98.3% (470/478) | 100.0% (483/483) | 100.0% (483/483) | 96.5% (466/483) |
| PHONE | 73 | 24.7% (18/73) | 97.3% (71/73) | 100.0% (73/73) | 97.0% (65/67) | 97.3% (71/73) | 100.0% (73/73) | 100.0% (73/73) |
| SECRET | 9 | 0.0% (0/9) | 77.8% (7/9) | 88.9% (8/9) | 77.8% (7/9) | 100.0% (9/9) | 100.0% (9/9) | 88.9% (8/9) |

## Recall by document format

| Format | Documents | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Plain text | 300 | 1679 | 25.0% (420/1679) | 81.1% (1362/1679) | 94.1% (1573/1672) | 88.0% (1448/1646) | 98.0% (1646/1679) | 98.0% (1644/1678) | 94.3% (1584/1679) |

## Recall by document type

Where each model is strong or weak, by the kind of document. Cells show recall and (items caught/items present); a dash means the type has no sensitive items on the answer key.

<details><summary>Show the table (60 document types)</summary>

| Document type | Documents | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Annual Report (synthetic finance document) | 5 | 27 | 66.7% (18/27) | 74.1% (20/27) | 96.3% (26/27) | 88.9% (24/27) | 100.0% (27/27) | 100.0% (27/27) | 100.0% (27/27) |
| Audit Report (synthetic finance document) | 5 | 43 | 83.7% (36/43) | 97.7% (42/43) | 100.0% (43/43) | 97.7% (42/43) | 100.0% (43/43) | 100.0% (43/43) | 100.0% (43/43) |
| BAI Format (synthetic finance document) | 5 | 58 | 12.1% (7/58) | 89.7% (52/58) | 96.6% (56/58) | 48.3% (28/58) | 100.0% (58/58) | 100.0% (58/58) | 91.4% (53/58) |
| Bank Statement (synthetic finance document) | 5 | 27 | 33.3% (9/27) | 70.4% (19/27) | 92.6% (25/27) | 96.3% (26/27) | 100.0% (27/27) | 92.6% (25/27) | 92.6% (25/27) |
| Bill of Lading (synthetic finance document) | 5 | 38 | 44.7% (17/38) | 63.2% (24/38) | 100.0% (38/38) | 100.0% (38/38) | 100.0% (38/38) | 100.0% (38/38) | 92.1% (35/38) |
| Business Plan (synthetic finance document) | 5 | 44 | 4.5% (2/44) | 88.6% (39/44) | 79.5% (35/44) | 97.7% (43/44) | 100.0% (44/44) | 100.0% (44/44) | 100.0% (44/44) |
| Compliance Certificate (synthetic finance document) | 5 | 40 | 20.0% (8/40) | 92.5% (37/40) | 97.5% (39/40) | 92.5% (37/40) | 100.0% (40/40) | 100.0% (40/40) | 100.0% (40/40) |
| Corporate Governance Guidelines (synthetic finance document) | 5 | 13 | 15.4% (2/13) | 100.0% (13/13) | 100.0% (13/13) | 100.0% (13/13) | 100.0% (13/13) | 100.0% (13/13) | 100.0% (13/13) |
| Corporate Tax Return (synthetic finance document) | 5 | 27 | 33.3% (9/27) | 81.5% (22/27) | 100.0% (27/27) | 81.5% (22/27) | 100.0% (27/27) | 100.0% (27/27) | 85.2% (23/27) |
| Credit Application (synthetic finance document) | 5 | 34 | 23.5% (8/34) | 70.6% (24/34) | 100.0% (34/34) | 97.1% (33/34) | 100.0% (34/34) | 100.0% (34/34) | 94.1% (32/34) |
| Credit Card Application (synthetic finance document) | 5 | 15 | 13.3% (2/15) | 93.3% (14/15) | 100.0% (15/15) | 100.0% (15/15) | 100.0% (15/15) | 100.0% (15/15) | 80.0% (12/15) |
| Credit Card Statement (synthetic finance document) | 5 | 21 | 14.3% (3/21) | 61.9% (13/21) | 76.2% (16/21) | 66.7% (14/21) | 100.0% (21/21) | 95.2% (20/21) | 100.0% (21/21) |
| Cryptocurrency Transaction Report (synthetic finance document) | 5 | 33 | 15.2% (5/33) | 100.0% (33/33) | 100.0% (33/33) | 81.8% (27/33) | 100.0% (33/33) | 81.8% (27/33) | 78.8% (26/33) |
| CSV (synthetic finance document) | 5 | 79 | 26.6% (21/79) | 59.5% (47/79) | 88.6% (70/79) | 100.0% (51/51) | 100.0% (79/79) | 100.0% (79/79) | 94.9% (75/79) |
| Currency Exchange Rate Sheet (synthetic finance document) | 5 | 28 | 7.1% (2/28) | 75.0% (21/28) | 100.0% (28/28) | 89.3% (25/28) | 100.0% (28/28) | 85.7% (24/28) | 85.7% (24/28) |
| Customer Agreement (synthetic finance document) | 5 | 19 | 21.1% (4/19) | 100.0% (19/19) | 100.0% (19/19) | 100.0% (19/19) | 100.0% (19/19) | 94.7% (18/19) | 100.0% (19/19) |
| Customer support conversational log (synthetic finance document) | 5 | 34 | 2.9% (1/34) | 82.4% (28/34) | 97.1% (33/34) | 97.1% (33/34) | 100.0% (34/34) | 100.0% (34/34) | 100.0% (34/34) |
| Dispute Resolution Policy (synthetic finance document) | 5 | 10 | 30.0% (3/10) | 100.0% (10/10) | 100.0% (10/10) | 80.0% (8/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) |
| EDI (synthetic finance document) | 5 | 36 | 2.8% (1/36) | 44.4% (16/36) | 71.4% (25/35) | 66.7% (24/36) | 100.0% (36/36) | 94.3% (33/35) | 91.7% (33/36) |
| Email (synthetic finance document) | 5 | 30 | 40.0% (12/30) | 96.7% (29/30) | 96.7% (29/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) |
| Employment Contract (synthetic finance document) | 5 | 24 | 25.0% (6/24) | 91.7% (22/24) | 95.8% (23/24) | 100.0% (24/24) | 100.0% (24/24) | 100.0% (24/24) | 95.8% (23/24) |
| Financial Aid Application (synthetic finance document) | 5 | 38 | 36.8% (14/38) | 92.1% (35/38) | 97.4% (37/38) | 94.7% (36/38) | 100.0% (38/38) | 100.0% (38/38) | 97.4% (37/38) |
| Financial Data Feed (synthetic finance document) | 5 | 12 | 16.7% (2/12) | 58.3% (7/12) | 100.0% (12/12) | 83.3% (10/12) | 66.7% (8/12) | 83.3% (10/12) | 91.7% (11/12) |
| Financial Disclosure Statement (synthetic finance document) | 5 | 38 | 63.2% (24/38) | 89.5% (34/38) | 100.0% (38/38) | 100.0% (38/38) | 100.0% (38/38) | 100.0% (38/38) | 100.0% (38/38) |
| Financial Forecast (synthetic finance document) | 5 | 15 | 46.7% (7/15) | 100.0% (15/15) | 100.0% (15/15) | 100.0% (15/15) | 100.0% (15/15) | 100.0% (15/15) | 93.3% (14/15) |
| Financial Regulatory Compliance Report (synthetic finance document) | 5 | 18 | 61.1% (11/18) | 94.4% (17/18) | 94.4% (17/18) | 100.0% (18/18) | 100.0% (18/18) | 100.0% (18/18) | 100.0% (18/18) |
| Financial Risk Assessment (synthetic finance document) | 5 | 43 | 14.0% (6/43) | 100.0% (43/43) | 100.0% (43/43) | 95.3% (41/43) | 100.0% (43/43) | 100.0% (43/43) | 76.7% (33/43) |
| Financial Statement (synthetic finance document) | 5 | 17 | 17.6% (3/17) | 76.5% (13/17) | 94.1% (16/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) |
| FIX Protocol (synthetic finance document) | 5 | 17 | 23.5% (4/17) | 58.8% (10/17) | 100.0% (17/17) | 70.6% (12/17) | 100.0% (17/17) | 100.0% (17/17) | 88.2% (15/17) |
| FpML (synthetic finance document) | 5 | 15 | 46.7% (7/15) | 80.0% (12/15) | 100.0% (15/15) | 73.3% (11/15) | 100.0% (15/15) | 80.0% (12/15) | 100.0% (15/15) |
| Health Insurance Claim Form (synthetic finance document) | 5 | 47 | 19.1% (9/47) | 72.3% (34/47) | 100.0% (47/47) | 95.7% (45/47) | 100.0% (47/47) | 100.0% (47/47) | 97.9% (46/47) |
| Insurance Claim Form (synthetic finance document) | 5 | 46 | 15.2% (7/46) | 89.1% (41/46) | 89.1% (41/46) | 93.5% (43/46) | 100.0% (46/46) | 100.0% (46/46) | 91.3% (42/46) |
| Insurance Policy (synthetic finance document) | 5 | 24 | 8.3% (2/24) | 91.7% (22/24) | 100.0% (24/24) | 95.8% (23/24) | 100.0% (24/24) | 100.0% (24/24) | 87.5% (21/24) |
| Investment Prospectus (synthetic finance document) | 5 | 22 | 13.6% (3/22) | 90.9% (20/22) | 68.2% (15/22) | 95.5% (21/22) | 100.0% (22/22) | 100.0% (22/22) | 95.5% (21/22) |
| ISDA Definition (synthetic finance document) | 5 | 10 | 10.0% (1/10) | 60.0% (6/10) | 40.0% (2/5) | 60.0% (6/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) |
| IT support ticket (synthetic finance document) | 5 | 17 | 29.4% (5/17) | 82.4% (14/17) | 94.1% (16/17) | 82.4% (14/17) | 100.0% (17/17) | 100.0% (17/17) | 88.2% (15/17) |
| Loan Agreement (synthetic finance document) | 5 | 19 | 21.1% (4/19) | 94.7% (18/19) | 100.0% (19/19) | 100.0% (14/14) | 100.0% (19/19) | 100.0% (19/19) | 100.0% (19/19) |
| Loan Application (synthetic finance document) | 5 | 50 | 42.0% (21/50) | 86.0% (43/50) | 90.0% (45/50) | 100.0% (50/50) | 100.0% (50/50) | 100.0% (50/50) | 92.0% (46/50) |
| Mortgage Amortization Schedule (synthetic finance document) | 5 | 5 | 0.0% (0/5) | 80.0% (4/5) | 100.0% (5/5) | 80.0% (4/5) | 100.0% (5/5) | 100.0% (5/5) | 100.0% (5/5) |
| Mortgage Contract (synthetic finance document) | 5 | 17 | 17.6% (3/17) | 94.1% (16/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) |
| MT940 (synthetic finance document) | 5 | 86 | 7.0% (6/86) | 54.7% (47/86) | 98.8% (85/86) | 69.8% (60/86) | 97.7% (84/86) | 100.0% (86/86) | 100.0% (86/86) |
| Payment Confirmation (synthetic finance document) | 5 | 34 | 32.4% (11/34) | 64.7% (22/34) | 88.2% (30/34) | 94.1% (32/34) | 100.0% (34/34) | 94.1% (32/34) | 91.2% (31/34) |
| Pension Plan Agreement (synthetic finance document) | 5 | 18 | 0.0% (0/18) | 77.8% (14/18) | 100.0% (18/18) | 88.9% (16/18) | 100.0% (18/18) | 100.0% (18/18) | 94.4% (17/18) |
| Policyholder's Report (synthetic finance document) | 5 | 42 | 11.9% (5/42) | 71.4% (30/42) | 83.3% (35/42) | 92.9% (39/42) | 85.7% (36/42) | 100.0% (42/42) | 100.0% (42/42) |
| Privacy Policy (synthetic finance document) | 5 | 14 | 14.3% (2/14) | 78.6% (11/14) | 71.4% (10/14) | 100.0% (14/14) | 78.6% (11/14) | 71.4% (10/14) | 64.3% (9/14) |
| Product Disclosure Statement (synthetic finance document) | 5 | 11 | 36.4% (4/11) | 90.9% (10/11) | 100.0% (11/11) | 100.0% (11/11) | 100.0% (11/11) | 100.0% (11/11) | 100.0% (11/11) |
| Real Estate Loan Agreement (synthetic finance document) | 5 | 16 | 12.5% (2/16) | 100.0% (16/16) | 81.3% (13/16) | 100.0% (16/16) | 100.0% (16/16) | 75.0% (12/16) | 100.0% (16/16) |
| Regulatory Compliance Guide (synthetic finance document) | 5 | 12 | 25.0% (3/12) | 91.7% (11/12) | 75.0% (9/12) | 100.0% (12/12) | 100.0% (12/12) | 100.0% (12/12) | 75.0% (9/12) |
| Regulatory Filing (synthetic finance document) | 5 | 24 | 41.7% (10/24) | 91.7% (22/24) | 100.0% (24/24) | 100.0% (24/24) | 100.0% (24/24) | 95.8% (23/24) | 95.8% (23/24) |
| Renewal Reminder (synthetic finance document) | 5 | 25 | 24.0% (6/25) | 92.0% (23/25) | 100.0% (25/25) | 96.0% (24/25) | 92.0% (23/25) | 100.0% (25/25) | 100.0% (25/25) |
| Safety Data Sheet (synthetic finance document) | 5 | 10 | 30.0% (3/10) | 100.0% (10/10) | 100.0% (10/10) | 90.0% (9/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) |
| Securities Prospectus (synthetic finance document) | 5 | 14 | 85.7% (12/14) | 100.0% (14/14) | 100.0% (13/13) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) |
| Shareholder Agreement (synthetic finance document) | 5 | 9 | 33.3% (3/9) | 88.9% (8/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) |
| Supply Chain Management Agreement (synthetic finance document) | 5 | 23 | 8.7% (2/23) | 91.3% (21/23) | 100.0% (23/23) | 95.7% (22/23) | 100.0% (23/23) | 100.0% (23/23) | 100.0% (23/23) |
| SWIFT Message (synthetic finance document) | 5 | 51 | 2.0% (1/51) | 86.3% (44/51) | 86.3% (44/51) | 33.3% (17/51) | 68.6% (35/51) | 100.0% (51/51) | 98.0% (50/51) |
| Tax Assessment Notice (synthetic finance document) | 5 | 24 | 33.3% (8/24) | 58.3% (14/24) | 91.7% (22/24) | 75.0% (18/24) | 100.0% (24/24) | 95.8% (23/24) | 75.0% (18/24) |
| Tax Return (synthetic finance document) | 5 | 23 | 21.7% (5/23) | 78.3% (18/23) | 100.0% (23/23) | 95.7% (22/23) | 100.0% (23/23) | 100.0% (23/23) | 95.7% (22/23) |
| Trade Confirmation (synthetic finance document) | 5 | 37 | 37.8% (14/37) | 81.1% (30/37) | 94.6% (35/37) | 86.5% (32/37) | 100.0% (37/37) | 97.3% (36/37) | 97.3% (36/37) |
| Transaction Confirmation (synthetic finance document) | 5 | 39 | 28.2% (11/39) | 84.6% (33/39) | 100.0% (39/39) | 74.4% (29/39) | 100.0% (39/39) | 100.0% (39/39) | 87.2% (34/39) |
| XBRL (synthetic finance document) | 5 | 17 | 17.6% (3/17) | 94.1% (16/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) | 100.0% (17/17) |

</details>

## Precision by redaction category

| Category | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|
| ADDRESS | 79.3% (23/29) | 82.6% (237/287) | 95.7% (269/281) | 96.6% (253/262) | 90.2% (284/315) | 98.2% (270/275) | 93.8% (255/272) |
| AGE | – | 100.0% (4/4) | – | – | – | – | – |
| COMPANY | 99.1% (226/228) | 36.1% (367/1018) | 72.5% (371/512) | 55.6% (425/765) | 86.1% (429/498) | 83.7% (443/529) | 87.0% (388/446) |
| COMPANY_ID | – | 100.0% (77/77) | 100.0% (7/7) | – | 100.0% (3/3) | 100.0% (6/6) | 100.0% (3/3) |
| CONTEXTUAL | – | 100.0% (62/62) | 100.0% (3/3) | – | 100.0% (4/4) | – | 100.0% (2/2) |
| DATE_OF_BIRTH | – | 35.3% (24/68) | 10.8% (22/203) | 21.3% (19/89) | 13.2% (25/190) | 95.2% (20/21) | 35.1% (20/57) |
| DOMAIN | – | 100.0% (46/46) | – | – | – | – | – |
| EMAIL | 100.0% (88/88) | 91.4% (53/58) | 94.6% (88/93) | 100.0% (82/82) | 100.0% (90/90) | 100.0% (89/89) | 100.0% (93/93) |
| GENDER | 100.0% (15/15) | – | – | 100.0% (1/1) | 100.0% (7/7) | 100.0% (11/11) | 100.0% (1/1) |
| ID_NUMBER | 91.1% (41/45) | 59.3% (32/54) | 73.4% (246/335) | 82.9% (131/158) | 78.8% (252/320) | 86.7% (247/285) | 78.7% (247/314) |
| ONLINE_ID | 100.0% (8/8) | 45.1% (23/51) | 56.7% (17/30) | 64.3% (9/14) | 62.5% (15/24) | 90.5% (19/21) | 87.0% (20/23) |
| PERSON | – | 77.0% (335/435) | 99.1% (436/440) | 90.4% (433/479) | 94.1% (447/475) | 97.6% (440/451) | 94.8% (456/481) |
| PHONE | 100.0% (17/17) | 94.5% (52/55) | 94.5% (69/73) | 100.0% (65/65) | 100.0% (70/70) | 100.0% (73/73) | 100.0% (73/73) |
| SECRET | – | 35.7% (125/350) | 15.0% (15/100) | 100.0% (9/9) | 100.0% (11/11) | 100.0% (13/13) | 100.0% (9/9) |

Of the correct redactions, the share given the right category label: rules only 93.1%; GLiNER only 70.5%; phi4 94.4%; gemma4:e4b 95.2%; qwen3.6:27b 95.7%; gemma4:31b 95.5%; gpt-oss 96.8%.

## Per document

<details><summary>Show the per-document table (300 documents)</summary>

| Document | Format | Items | rules only (recall · missed · over) | GLiNER only (recall · missed · over) | phi4 (recall · missed · over) | gemma4:e4b (recall · missed · over) | qwen3.6:27b (recall · missed · over) | gemma4:31b (recall · missed · over) | gpt-oss (recall · missed · over) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| text/heldout-001-annual-report.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |
| text/heldout-002-annual-report.txt | Plain text | 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-003-annual-report.txt | Plain text | 7 | 85.7% · 1 · 0 | 71.4% · 2 · 1 | 100.0% · 0 · 0 | 85.7% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-004-annual-report.txt | Plain text | 7 | 0.0% · 7 · 0 | 71.4% · 2 · 0 | 85.7% · 1 · 0 | 71.4% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-005-annual-report.txt | Plain text | 7 | 100.0% · 0 · 0 | 57.1% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-006-audit-report.txt | Plain text | 8 | 87.5% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-007-audit-report.txt | Plain text | 11 | 72.7% · 3 · 1 | 90.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-008-audit-report.txt | Plain text | 4 | 75.0% · 1 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-009-audit-report.txt | Plain text | 7 | 85.7% · 1 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-010-audit-report.txt | Plain text | 13 | 92.3% · 1 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 92.3% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-011-bai-format.txt | Plain text | 3 | 33.3% · 2 · 0 | 33.3% · 2 · 0 | 100.0% · 0 · 82 | 100.0% · 0 · 2 | 100.0% · 0 · 7 | 100.0% · 0 · 4 | 100.0% · 0 · 2 |
| text/heldout-012-bai-format.txt | Plain text | 8 | 37.5% · 5 · 0 | 100.0% · 0 · 2 | 87.5% · 1 · 0 | 75.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 87.5% · 1 · 2 |
| text/heldout-013-bai-format.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-014-bai-format.txt | Plain text | 29 | 0.0% · 29 · 0 | 93.1% · 2 · 5 | 96.6% · 1 · 0 | 6.9% · 27 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-015-bai-format.txt | Plain text | 15 | 20.0% · 12 · 0 | 86.7% · 2 · 10 | 100.0% · 0 · 0 | 93.3% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 73.3% · 4 · 0 |
| text/heldout-016-bank-statement.txt | Plain text | 4 | 50.0% · 2 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-017-bank-statement.txt | Plain text | 2 | 50.0% · 1 · 0 | 50.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-018-bank-statement.txt | Plain text | 9 | 33.3% · 6 · 0 | 77.8% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-019-bank-statement.txt | Plain text | 8 | 37.5% · 5 · 0 | 50.0% · 4 · 2 | 75.0% · 2 · 1 | 87.5% · 1 · 3 | 100.0% · 0 · 0 | 75.0% · 2 · 0 | 75.0% · 2 · 0 |
| text/heldout-020-bank-statement.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-021-bill-of-lading.txt | Plain text | 7 | 57.1% · 3 · 0 | 57.1% · 3 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 5 |
| text/heldout-022-bill-of-lading.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 6 |
| text/heldout-023-bill-of-lading.txt | Plain text | 6 | 66.7% · 2 · 0 | 50.0% · 3 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-024-bill-of-lading.txt | Plain text | 12 | 41.7% · 7 · 0 | 66.7% · 4 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-025-bill-of-lading.txt | Plain text | 10 | 40.0% · 6 · 0 | 70.0% · 3 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 70.0% · 3 · 1 |
| text/heldout-026-business-plan.txt | Plain text | 9 | 0.0% · 9 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-027-business-plan.txt | Plain text | 11 | 9.1% · 10 · 0 | 90.9% · 1 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-028-business-plan.txt | Plain text | 4 | 0.0% · 4 · 0 | 50.0% · 2 · 6 | 50.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-029-business-plan.txt | Plain text | 8 | 0.0% · 8 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 7 | 100.0% · 0 · 0 |
| text/heldout-030-business-plan.txt | Plain text | 12 | 8.3% · 11 · 0 | 83.3% · 2 · 6 | 41.7% · 7 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| text/heldout-031-compliance-certificate.txt | Plain text | 9 | 33.3% · 6 · 0 | 77.8% · 2 · 5 | 88.9% · 1 · 1 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 1 |
| text/heldout-032-compliance-certificate.txt | Plain text | 13 | 0.0% · 13 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-033-compliance-certificate.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 19 | 100.0% · 0 · 0 | 60.0% · 2 · 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 13 |
| text/heldout-034-compliance-certificate.txt | Plain text | 9 | 11.1% · 8 · 0 | 88.9% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| text/heldout-035-compliance-certificate.txt | Plain text | 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-036-corporate-governance-guidelines.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-037-corporate-governance-guidelines.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 8 | 100.0% · 0 · 0 | 100.0% · 0 · 16 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-038-corporate-governance-guidelines.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 11 | – · 0 · 1 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 1 |
| text/heldout-039-corporate-governance-guidelines.txt | Plain text | 4 | 50.0% · 2 · 0 | 100.0% · 0 · 14 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-040-corporate-governance-guidelines.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 14 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-041-corporate-tax-return.txt | Plain text | 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-042-corporate-tax-return.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 75.0% · 1 · 0 |
| text/heldout-043-corporate-tax-return.txt | Plain text | 9 | 44.4% · 5 · 1 | 77.8% · 2 · 2 | 100.0% · 0 · 3 | 88.9% · 1 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| text/heldout-044-corporate-tax-return.txt | Plain text | 2 | 0.0% · 2 · 0 | 50.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 50.0% · 1 · 0 |
| text/heldout-045-corporate-tax-return.txt | Plain text | 10 | 30.0% · 7 · 0 | 90.0% · 1 · 1 | 100.0% · 0 · 2 | 60.0% · 4 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 2 | 80.0% · 2 · 0 |
| text/heldout-046-credit-application.txt | Plain text | 5 | 20.0% · 4 · 0 | 60.0% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-047-credit-application.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-048-credit-application.txt | Plain text | 13 | 46.2% · 7 · 0 | 61.5% · 5 · 0 | 100.0% · 0 · 2 | 92.3% · 1 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 84.6% · 2 · 1 |
| text/heldout-049-credit-application.txt | Plain text | 8 | 12.5% · 7 · 0 | 62.5% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-050-credit-application.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-051-credit-card-application.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-052-credit-card-application.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 0.0% · 2 · 0 |
| text/heldout-053-credit-card-application.txt | Plain text | 4 | 25.0% · 3 · 0 | 75.0% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 75.0% · 1 · 0 |
| text/heldout-054-credit-card-application.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-055-credit-card-application.txt | Plain text | 3 | 33.3% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-056-credit-card-statement.txt | Plain text | 8 | 0.0% · 8 · 0 | 50.0% · 4 · 0 | 50.0% · 4 · 3 | 37.5% · 5 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 2 | 100.0% · 0 · 3 |
| text/heldout-057-credit-card-statement.txt | Plain text | 4 | 50.0% · 2 · 0 | 75.0% · 1 · 0 | 75.0% · 1 · 3 | 75.0% · 1 · 9 | 100.0% · 0 · 16 | 100.0% · 0 · 7 | 100.0% · 0 · 0 |
| text/heldout-058-credit-card-statement.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 2 | 66.7% · 1 · 1 | 100.0% · 0 · 0 |
| text/heldout-059-credit-card-statement.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 11 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-060-credit-card-statement.txt | Plain text | 2 | 50.0% · 1 · 0 | 0.0% · 2 · 0 | 100.0% · 0 · 13 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-061-cryptocurrency-transaction-report.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 5 | – · 0 · 3 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-062-cryptocurrency-transaction-report.txt | Plain text | 15 | 26.7% · 11 · 0 | 100.0% · 0 · 8 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 0 |
| text/heldout-063-cryptocurrency-transaction-report.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 9 | – · 0 · 6 | – · 0 · 0 | – · 0 · 0 | – · 0 · 6 | – · 0 · 0 |
| text/heldout-064-cryptocurrency-transaction-report.txt | Plain text | 11 | 9.1% · 10 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 45.5% · 6 · 0 | 100.0% · 0 · 0 | 45.5% · 6 · 0 | 100.0% · 0 · 0 |
| text/heldout-065-cryptocurrency-transaction-report.txt | Plain text | 7 | 0.0% · 7 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 0.0% · 7 · 0 |
| text/heldout-066-csv.txt | Plain text | 2 | 0.0% · 2 · 0 | 50.0% · 1 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-067-csv.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-068-csv.txt | Plain text | 28 | 25.0% · 21 · 0 | 50.0% · 14 · 3 | 67.9% · 9 · 0 | – | 100.0% · 0 · 14 | 100.0% · 0 · 8 | 85.7% · 4 · 0 |
| text/heldout-069-csv.txt | Plain text | 40 | 27.5% · 29 · 0 | 65.0% · 14 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-070-csv.txt | Plain text | 9 | 33.3% · 6 · 0 | 66.7% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-071-currency-exchange-rate-sheet.txt | Plain text | 6 | 0.0% · 6 · 0 | 16.7% · 5 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 33.3% · 4 · 0 | 33.3% · 4 · 0 |
| text/heldout-072-currency-exchange-rate-sheet.txt | Plain text | 3 | 0.0% · 3 · 0 | 33.3% · 2 · 2 | 100.0% · 0 · 1 | 33.3% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-073-currency-exchange-rate-sheet.txt | Plain text | 12 | 0.0% · 12 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-074-currency-exchange-rate-sheet.txt | Plain text | 5 | 40.0% · 3 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 1 | 100.0% · 0 · 9 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-075-currency-exchange-rate-sheet.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 50.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-076-customer-agreement.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 8 |
| text/heldout-077-customer-agreement.txt | Plain text | 5 | 20.0% · 4 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |
| text/heldout-078-customer-agreement.txt | Plain text | 5 | 40.0% · 3 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 80.0% · 1 · 0 | 100.0% · 0 · 0 |
| text/heldout-079-customer-agreement.txt | Plain text | 3 | 0.0% · 3 · 1 | 100.0% · 0 · 14 | 100.0% · 0 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-080-customer-agreement.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-081-customer-support-conversational-log.txt | Plain text | 13 | 0.0% · 13 · 0 | 61.5% · 5 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-082-customer-support-conversational-log.txt | Plain text | 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-083-customer-support-conversational-log.txt | Plain text | 9 | 0.0% · 9 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-084-customer-support-conversational-log.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 2 | 75.0% · 1 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-085-customer-support-conversational-log.txt | Plain text | 7 | 0.0% · 7 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-086-dispute-resolution-policy.txt | Plain text | 4 | 75.0% · 1 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 13 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-087-dispute-resolution-policy.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 11 | 100.0% · 0 · 1 | 0.0% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-088-dispute-resolution-policy.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 8 | – · 0 · 2 | – · 0 · 2 | – · 0 · 2 | – · 0 · 0 | – · 0 · 2 |
| text/heldout-089-dispute-resolution-policy.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-090-dispute-resolution-policy.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 9 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-091-edi.txt | Plain text | 8 | 0.0% · 8 · 0 | 0.0% · 8 · 0 | 0.0% · 8 · 8 | 25.0% · 6 · 3 | 100.0% · 0 · 10 | 100.0% · 0 · 3 | 100.0% · 0 · 20 |
| text/heldout-092-edi.txt | Plain text | 7 | 0.0% · 7 · 0 | 42.9% · 4 · 2 | 100.0% · 0 · 0 | 42.9% · 4 · 0 | 100.0% · 0 · 4 | 71.4% · 2 · 0 | 57.1% · 3 · 0 |
| text/heldout-093-edi.txt | Plain text | 1 | 0.0% · 1 · 0 | 0.0% · 1 · 2 | – | 100.0% · 0 · 2 | 100.0% · 0 · 2 | – | 100.0% · 0 · 5 |
| text/heldout-094-edi.txt | Plain text | 9 | 11.1% · 8 · 0 | 55.6% · 4 · 12 | 88.9% · 1 · 0 | 88.9% · 1 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-095-edi.txt | Plain text | 11 | 0.0% · 11 · 0 | 72.7% · 3 · 8 | 90.9% · 1 · 0 | 90.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-096-email.txt | Plain text | 2 | 50.0% · 1 · 0 | 50.0% · 1 · 0 | 50.0% · 1 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 3 | 100.0% · 0 · 3 |
| text/heldout-097-email.txt | Plain text | 13 | 38.5% · 8 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-098-email.txt | Plain text | 8 | 25.0% · 6 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-099-email.txt | Plain text | 2 | 50.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-100-email.txt | Plain text | 5 | 60.0% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 |
| text/heldout-101-employment-contract.txt | Plain text | 9 | 22.2% · 7 · 0 | 77.8% · 2 · 3 | 88.9% · 1 · 1 | 100.0% · 0 · 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 88.9% · 1 · 0 |
| text/heldout-102-employment-contract.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 20 | 100.0% · 0 · 1 | 100.0% · 0 · 10 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-103-employment-contract.txt | Plain text | 3 | 33.3% · 2 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 2 | 100.0% · 0 · 25 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 0 |
| text/heldout-104-employment-contract.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 1 | 100.0% · 0 · 9 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-105-employment-contract.txt | Plain text | 5 | 40.0% · 3 · 0 | 100.0% · 0 · 12 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-106-financial-aid-application.txt | Plain text | 10 | 50.0% · 5 · 0 | 90.0% · 1 · 4 | 100.0% · 0 · 0 | 90.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-107-financial-aid-application.txt | Plain text | 7 | 14.3% · 6 · 0 | 85.7% · 1 · 3 | 85.7% · 1 · 0 | 85.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 85.7% · 1 · 0 |
| text/heldout-108-financial-aid-application.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-109-financial-aid-application.txt | Plain text | 12 | 58.3% · 5 · 0 | 91.7% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-110-financial-aid-application.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 2 |
| text/heldout-111-financial-data-feed.txt | Plain text | 5 | 0.0% · 5 · 0 | 80.0% · 1 · 1 | 100.0% · 0 · 1 | 60.0% · 2 · 0 | 40.0% · 3 · 0 | 60.0% · 2 · 4 | 100.0% · 0 · 0 |
| text/heldout-112-financial-data-feed.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-113-financial-data-feed.txt | Plain text | 2 | 0.0% · 2 · 0 | 50.0% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-114-financial-data-feed.txt | Plain text | 2 | 0.0% · 2 · 0 | 0.0% · 2 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 50.0% · 1 · 3 | 100.0% · 0 · 0 | 50.0% · 1 · 0 |
| text/heldout-115-financial-data-feed.txt | Plain text | 3 | 66.7% · 1 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-116-financial-disclosure-statement.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 8 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-117-financial-disclosure-statement.txt | Plain text | 7 | 57.1% · 3 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-118-financial-disclosure-statement.txt | Plain text | 14 | 85.7% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-119-financial-disclosure-statement.txt | Plain text | 7 | 14.3% · 6 · 0 | 57.1% · 3 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 6 | 100.0% · 0 · 3 | 100.0% · 0 · 3 |
| text/heldout-120-financial-disclosure-statement.txt | Plain text | 7 | 100.0% · 0 · 0 | 85.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-121-financial-forecast.txt | Plain text | 6 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-122-financial-forecast.txt | Plain text | 6 | 0.0% · 6 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 83.3% · 1 · 0 |
| text/heldout-123-financial-forecast.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 8 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-124-financial-forecast.txt | Plain text | 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-125-financial-forecast.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-126-financial-regulatory-compliance-report.txt | Plain text | 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-127-financial-regulatory-compliance-report.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-128-financial-regulatory-compliance-report.txt | Plain text | 3 | 66.7% · 1 · 0 | 100.0% · 0 · 3 | 66.7% · 1 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-129-financial-regulatory-compliance-report.txt | Plain text | 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-130-financial-regulatory-compliance-report.txt | Plain text | 3 | 33.3% · 2 · 0 | 66.7% · 1 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-131-financial-risk-assessment.txt | Plain text | 10 | 50.0% · 5 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-132-financial-risk-assessment.txt | Plain text | 12 | 0.0% · 12 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 16.7% · 10 · 0 |
| text/heldout-133-financial-risk-assessment.txt | Plain text | 9 | 0.0% · 9 · 0 | 100.0% · 0 · 8 | 100.0% · 0 · 0 | 77.8% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-134-financial-risk-assessment.txt | Plain text | 8 | 0.0% · 8 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-135-financial-risk-assessment.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-136-financial-statement.txt | Plain text | 2 | 0.0% · 2 · 0 | 50.0% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-137-financial-statement.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-138-financial-statement.txt | Plain text | 5 | 60.0% · 2 · 0 | 60.0% · 2 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-139-financial-statement.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-140-financial-statement.txt | Plain text | 8 | 0.0% · 8 · 0 | 87.5% · 1 · 8 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-141-fix-protocol.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 1 | – · 0 · 0 | – · 0 · 1 |
| text/heldout-142-fix-protocol.txt | Plain text | 8 | 37.5% · 5 · 0 | 37.5% · 5 · 9 | 100.0% · 0 · 0 | 62.5% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 75.0% · 2 · 0 |
| text/heldout-143-fix-protocol.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-144-fix-protocol.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 3 | 100.0% · 0 · 1 | 66.7% · 1 · 1 | 100.0% · 0 · 5 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-145-fix-protocol.txt | Plain text | 3 | 33.3% · 2 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-146-fpml.txt | Plain text | 4 | 75.0% · 1 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-147-fpml.txt | Plain text | 2 | 0.0% · 2 · 0 | 50.0% · 1 · 3 | 100.0% · 0 · 5 | 50.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-148-fpml.txt | Plain text | 5 | 40.0% · 3 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 8 | 40.0% · 3 · 1 | 100.0% · 0 · 0 | 40.0% · 3 · 0 | 100.0% · 0 · 0 |
| text/heldout-149-fpml.txt | Plain text | 2 | 50.0% · 1 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-150-fpml.txt | Plain text | 2 | 50.0% · 1 · 0 | 50.0% · 1 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-151-health-insurance-claim-form.txt | Plain text | 13 | 30.8% · 9 · 0 | 76.9% · 3 · 0 | 100.0% · 0 · 3 | 84.6% · 2 · 5 | 100.0% · 0 · 8 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-152-health-insurance-claim-form.txt | Plain text | 12 | 25.0% · 9 · 0 | 75.0% · 3 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 |
| text/heldout-153-health-insurance-claim-form.txt | Plain text | 9 | 0.0% · 9 · 0 | 66.7% · 3 · 0 | 100.0% · 0 · 15 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 15 |
| text/heldout-154-health-insurance-claim-form.txt | Plain text | 6 | 0.0% · 6 · 0 | 66.7% · 2 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 83.3% · 1 · 0 |
| text/heldout-155-health-insurance-claim-form.txt | Plain text | 7 | 28.6% · 5 · 0 | 71.4% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-156-insurance-claim-form.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-157-insurance-claim-form.txt | Plain text | 8 | 0.0% · 8 · 0 | 87.5% · 1 · 0 | 100.0% · 0 · 1 | 87.5% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-158-insurance-claim-form.txt | Plain text | 12 | 16.7% · 10 · 0 | 83.3% · 2 · 2 | 83.3% · 2 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-159-insurance-claim-form.txt | Plain text | 14 | 28.6% · 10 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 92.9% · 1 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-160-insurance-claim-form.txt | Plain text | 9 | 11.1% · 8 · 0 | 77.8% · 2 · 1 | 66.7% · 3 · 3 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 55.6% · 4 · 3 |
| text/heldout-161-insurance-policy.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-162-insurance-policy.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-163-insurance-policy.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-164-insurance-policy.txt | Plain text | 4 | 50.0% · 2 · 0 | 75.0% · 1 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-165-insurance-policy.txt | Plain text | 7 | 0.0% · 7 · 0 | 85.7% · 1 · 9 | 100.0% · 0 · 0 | 85.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 57.1% · 3 · 0 |
| text/heldout-166-investment-prospectus.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 0.0% · 1 · 0 |
| text/heldout-167-investment-prospectus.txt | Plain text | 6 | 0.0% · 6 · 0 | 83.3% · 1 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-168-investment-prospectus.txt | Plain text | 8 | 12.5% · 7 · 0 | 100.0% · 0 · 9 | 37.5% · 5 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-169-investment-prospectus.txt | Plain text | 2 | 50.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-170-investment-prospectus.txt | Plain text | 5 | 20.0% · 4 · 0 | 80.0% · 1 · 4 | 60.0% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-171-isda-definition.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 9 | – · 0 · 16 | – · 0 · 19 | – · 0 · 0 | – · 0 · 0 | – · 0 · 4 |
| text/heldout-172-isda-definition.txt | Plain text | 4 | 0.0% · 4 · 0 | 50.0% · 2 · 0 | 25.0% · 3 · 0 | 50.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-173-isda-definition.txt | Plain text | 5 | 20.0% · 4 · 0 | 80.0% · 1 · 10 | – | 80.0% · 1 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-174-isda-definition.txt | Plain text | 1 | 0.0% · 1 · 0 | 0.0% · 1 · 2 | 100.0% · 0 · 0 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| text/heldout-175-isda-definition.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-176-it-support-ticket.txt | Plain text | 8 | 25.0% · 6 · 0 | 75.0% · 2 · 0 | 100.0% · 0 · 0 | 87.5% · 1 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 87.5% · 1 · 0 |
| text/heldout-177-it-support-ticket.txt | Plain text | 1 | 0.0% · 1 · 0 | 0.0% · 1 · 8 | 0.0% · 1 · 1 | 0.0% · 1 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-178-it-support-ticket.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 5 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 5 | – · 0 · 0 |
| text/heldout-179-it-support-ticket.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 0.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 0.0% · 1 · 0 |
| text/heldout-180-it-support-ticket.txt | Plain text | 7 | 42.9% · 4 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-181-loan-agreement.txt | Plain text | 2 | 50.0% · 1 · 0 | 50.0% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-182-loan-agreement.txt | Plain text | 4 | 50.0% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-183-loan-agreement.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 11 | 100.0% · 0 · 0 | – | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-184-loan-agreement.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 15 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-185-loan-agreement.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-186-loan-application.txt | Plain text | 10 | 30.0% · 7 · 0 | 90.0% · 1 · 1 | 90.0% · 1 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-187-loan-application.txt | Plain text | 10 | 60.0% · 4 · 0 | 80.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-188-loan-application.txt | Plain text | 10 | 60.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-189-loan-application.txt | Plain text | 10 | 40.0% · 6 · 0 | 70.0% · 3 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-190-loan-application.txt | Plain text | 10 | 20.0% · 8 · 0 | 90.0% · 1 · 6 | 60.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 60.0% · 4 · 0 |
| text/heldout-191-mortgage-amortization-schedule.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 1 | 100.0% · 0 · 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-192-mortgage-amortization-schedule.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 8 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-193-mortgage-amortization-schedule.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-194-mortgage-amortization-schedule.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 50.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-195-mortgage-amortization-schedule.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-196-mortgage-contract.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-197-mortgage-contract.txt | Plain text | 5 | 40.0% · 3 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-198-mortgage-contract.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-199-mortgage-contract.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-200-mortgage-contract.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 13 | – · 0 · 0 | – · 0 · 15 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-201-mt940.txt | Plain text | 12 | 0.0% · 12 · 0 | 100.0% · 0 · 15 | 100.0% · 0 · 0 | 41.7% · 7 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 3 |
| text/heldout-202-mt940.txt | Plain text | 19 | 5.3% · 18 · 0 | 10.5% · 17 · 10 | 100.0% · 0 · 2 | 10.5% · 17 · 1 | 100.0% · 0 · 5 | 100.0% · 0 · 3 | 100.0% · 0 · 5 |
| text/heldout-203-mt940.txt | Plain text | 24 | 4.2% · 23 · 0 | 25.0% · 18 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-204-mt940.txt | Plain text | 9 | 11.1% · 8 · 1 | 66.7% · 3 · 18 | 88.9% · 1 · 1 | 77.8% · 2 · 16 | 77.8% · 2 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-205-mt940.txt | Plain text | 22 | 13.6% · 19 · 0 | 95.5% · 1 · 23 | 100.0% · 0 · 8 | 100.0% · 0 · 7 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-206-payment-confirmation.txt | Plain text | 6 | 0.0% · 6 · 0 | 50.0% · 3 · 1 | 83.3% · 1 · 0 | 66.7% · 2 · 0 | 100.0% · 0 · 0 | 66.7% · 2 · 0 | 100.0% · 0 · 0 |
| text/heldout-207-payment-confirmation.txt | Plain text | 8 | 25.0% · 6 · 0 | 62.5% · 3 · 1 | 62.5% · 3 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 62.5% · 3 · 1 |
| text/heldout-208-payment-confirmation.txt | Plain text | 12 | 41.7% · 7 · 0 | 66.7% · 4 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 3 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-209-payment-confirmation.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 3 |
| text/heldout-210-payment-confirmation.txt | Plain text | 5 | 80.0% · 1 · 0 | 60.0% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-211-pension-plan-agreement.txt | Plain text | 8 | 0.0% · 8 · 0 | 87.5% · 1 · 2 | 100.0% · 0 · 0 | 87.5% · 1 · 11 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-212-pension-plan-agreement.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 8 | 100.0% · 0 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 75.0% · 1 · 0 |
| text/heldout-213-pension-plan-agreement.txt | Plain text | 4 | 0.0% · 4 · 0 | 50.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-214-pension-plan-agreement.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-215-pension-plan-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 2 | – · 0 · 2 | – · 0 · 1 | – · 0 · 0 |
| text/heldout-216-policyholder-s-report.txt | Plain text | 7 | 0.0% · 7 · 0 | 71.4% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-217-policyholder-s-report.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-218-policyholder-s-report.txt | Plain text | 8 | 12.5% · 7 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-219-policyholder-s-report.txt | Plain text | 17 | 0.0% · 17 · 0 | 47.1% · 9 · 3 | 58.8% · 7 · 0 | 82.4% · 3 · 0 | 64.7% · 6 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 4 |
| text/heldout-220-policyholder-s-report.txt | Plain text | 6 | 66.7% · 2 · 0 | 83.3% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-221-privacy-policy.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 5 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 66.7% · 1 · 0 | 66.7% · 1 · 0 |
| text/heldout-222-privacy-policy.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-223-privacy-policy.txt | Plain text | 6 | 33.3% · 4 · 0 | 100.0% · 0 · 5 | 33.3% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 33.3% · 4 · 0 |
| text/heldout-224-privacy-policy.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 4 | – · 0 · 2 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-225-privacy-policy.txt | Plain text | 3 | 0.0% · 3 · 0 | 0.0% · 3 · 6 | 100.0% · 0 · 6 | 100.0% · 0 · 3 | 0.0% · 3 · 0 | 0.0% · 3 · 0 | 100.0% · 0 · 4 |
| text/heldout-226-product-disclosure-statement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 8 | – · 0 · 2 | – · 0 · 15 | – · 0 · 17 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-227-product-disclosure-statement.txt | Plain text | 1 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-228-product-disclosure-statement.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-229-product-disclosure-statement.txt | Plain text | 6 | 50.0% · 3 · 0 | 83.3% · 1 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 |
| text/heldout-230-product-disclosure-statement.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-231-real-estate-loan-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 6 | – · 0 · 13 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-232-real-estate-loan-agreement.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 12 | 40.0% · 3 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 20.0% · 4 · 0 | 100.0% · 0 · 1 |
| text/heldout-233-real-estate-loan-agreement.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-234-real-estate-loan-agreement.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-235-real-estate-loan-agreement.txt | Plain text | 3 | 33.3% · 2 · 0 | 100.0% · 0 · 11 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-236-regulatory-compliance-guide.txt | Plain text | 1 | 0.0% · 1 · 1 | 100.0% · 0 · 4 | 100.0% · 0 · 6 | 100.0% · 0 · 16 | 100.0% · 0 · 6 | 100.0% · 0 · 6 | 100.0% · 0 · 2 |
| text/heldout-237-regulatory-compliance-guide.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-238-regulatory-compliance-guide.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 10 | – · 0 · 0 | – · 0 · 13 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-239-regulatory-compliance-guide.txt | Plain text | 6 | 16.7% · 5 · 0 | 100.0% · 0 · 2 | 50.0% · 3 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 50.0% · 3 · 0 |
| text/heldout-240-regulatory-compliance-guide.txt | Plain text | 2 | 100.0% · 0 · 0 | 50.0% · 1 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-241-regulatory-filing.txt | Plain text | 9 | 22.2% · 7 · 0 | 88.9% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 88.9% · 1 · 0 | 88.9% · 1 · 2 |
| text/heldout-242-regulatory-filing.txt | Plain text | 4 | 75.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-243-regulatory-filing.txt | Plain text | 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-244-regulatory-filing.txt | Plain text | 6 | 0.0% · 6 · 0 | 83.3% · 1 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 4 |
| text/heldout-245-regulatory-filing.txt | Plain text | 3 | 100.0% · 0 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |
| text/heldout-246-renewal-reminder.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-247-renewal-reminder.txt | Plain text | 9 | 22.2% · 7 · 0 | 77.8% · 2 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 77.8% · 2 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 3 |
| text/heldout-248-renewal-reminder.txt | Plain text | 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-249-renewal-reminder.txt | Plain text | 4 | 0.0% · 4 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-250-renewal-reminder.txt | Plain text | 8 | 37.5% · 5 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-251-safety-data-sheet.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-252-safety-data-sheet.txt | Plain text | 2 | 50.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 50.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-253-safety-data-sheet.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-254-safety-data-sheet.txt | Plain text | 3 | 33.3% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-255-safety-data-sheet.txt | Plain text | 3 | 33.3% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-256-securities-prospectus.txt | Plain text | 2 | 0.0% · 2 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 4 | 100.0% · 0 · 0 |
| text/heldout-257-securities-prospectus.txt | Plain text | 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-258-securities-prospectus.txt | Plain text | 3 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-259-securities-prospectus.txt | Plain text | 4 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 7 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-260-securities-prospectus.txt | Plain text | 1 | 100.0% · 0 · 0 | 100.0% · 0 · 8 | – | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-261-shareholder-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 3 | – · 0 · 6 | – · 0 · 19 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-262-shareholder-agreement.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 26 | 100.0% · 0 · 11 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-263-shareholder-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 18 | – · 0 · 22 | – · 0 · 22 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-264-shareholder-agreement.txt | Plain text | 6 | 50.0% · 3 · 0 | 100.0% · 0 · 16 | 100.0% · 0 · 0 | 100.0% · 0 · 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-265-shareholder-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 12 | – · 0 · 12 | – · 0 · 12 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-266-supply-chain-management-agreement.txt | Plain text | 5 | 20.0% · 4 · 0 | 60.0% · 2 · 4 | 100.0% · 0 · 0 | 80.0% · 1 · 0 | 100.0% · 0 · 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-267-supply-chain-management-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 15 | – · 0 · 15 | – · 0 · 13 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-268-supply-chain-management-agreement.txt | Plain text | 15 | 6.7% · 14 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-269-supply-chain-management-agreement.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 1 | – · 0 · 1 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-270-supply-chain-management-agreement.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 3 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-271-swift-message.txt | Plain text | 8 | 12.5% · 7 · 2 | 87.5% · 1 · 5 | 100.0% · 0 · 5 | 87.5% · 1 · 3 | 100.0% · 0 · 5 | 100.0% · 0 · 5 | 100.0% · 0 · 7 |
| text/heldout-272-swift-message.txt | Plain text | 6 | 0.0% · 6 · 0 | 100.0% · 0 · 28 | 100.0% · 0 · 11 | 0.0% · 6 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 12 |
| text/heldout-273-swift-message.txt | Plain text | 13 | 0.0% · 13 · 1 | 69.2% · 4 · 2 | 61.5% · 5 · 1 | 69.2% · 4 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 1 | 92.3% · 1 · 2 |
| text/heldout-274-swift-message.txt | Plain text | 21 | 0.0% · 21 · 0 | 100.0% · 0 · 18 | 90.5% · 2 · 0 | 4.8% · 20 · 0 | 23.8% · 16 · 5 | 100.0% · 0 · 1 | 100.0% · 0 · 2 |
| text/heldout-275-swift-message.txt | Plain text | 3 | 0.0% · 3 · 0 | 33.3% · 2 · 11 | 100.0% · 0 · 6 | 0.0% · 3 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-276-tax-assessment-notice.txt | Plain text | 1 | 0.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 0.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-277-tax-assessment-notice.txt | Plain text | 3 | 0.0% · 3 · 0 | 33.3% · 2 · 0 | 100.0% · 0 · 2 | 66.7% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-278-tax-assessment-notice.txt | Plain text | 7 | 57.1% · 3 · 0 | 71.4% · 2 · 0 | 100.0% · 0 · 1 | 85.7% · 1 · 0 | 100.0% · 0 · 1 | 85.7% · 1 · 0 | 100.0% · 0 · 0 |
| text/heldout-279-tax-assessment-notice.txt | Plain text | 3 | 66.7% · 1 · 0 | 66.7% · 1 · 1 | 100.0% · 0 · 5 | 66.7% · 1 · 2 | 100.0% · 0 · 7 | 100.0% · 0 · 2 | 66.7% · 1 · 0 |
| text/heldout-280-tax-assessment-notice.txt | Plain text | 10 | 20.0% · 8 · 2 | 50.0% · 5 · 0 | 80.0% · 2 · 1 | 80.0% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 50.0% · 5 · 2 |
| text/heldout-281-tax-return.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 5 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-282-tax-return.txt | Plain text | 4 | 25.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-283-tax-return.txt | Plain text | 4 | 50.0% · 2 · 0 | 50.0% · 2 · 2 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/heldout-284-tax-return.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 1 | 100.0% · 0 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| text/heldout-285-tax-return.txt | Plain text | 8 | 25.0% · 6 · 0 | 87.5% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 87.5% · 1 · 0 |
| text/heldout-286-trade-confirmation.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-287-trade-confirmation.txt | Plain text | 8 | 50.0% · 4 · 1 | 87.5% · 1 · 1 | 100.0% · 0 · 3 | 87.5% · 1 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-288-trade-confirmation.txt | Plain text | 6 | 33.3% · 4 · 0 | 83.3% · 1 · 1 | 100.0% · 0 · 0 | 83.3% · 1 · 0 | 100.0% · 0 · 4 | 83.3% · 1 · 1 | 100.0% · 0 · 3 |
| text/heldout-289-trade-confirmation.txt | Plain text | 3 | 0.0% · 3 · 0 | 66.7% · 1 · 0 | 100.0% · 0 · 3 | 66.7% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-290-trade-confirmation.txt | Plain text | 17 | 47.1% · 9 · 0 | 76.5% · 4 · 0 | 88.2% · 2 · 4 | 88.2% · 2 · 5 | 100.0% · 0 · 6 | 100.0% · 0 · 4 | 94.1% · 1 · 4 |
| text/heldout-291-transaction-confirmation.txt | Plain text | 14 | 50.0% · 7 · 0 | 85.7% · 2 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 64.3% · 5 · 0 |
| text/heldout-292-transaction-confirmation.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 10 | 100.0% · 0 · 0 | 75.0% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-293-transaction-confirmation.txt | Plain text | 3 | 0.0% · 3 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-294-transaction-confirmation.txt | Plain text | 13 | 30.8% · 9 · 0 | 76.9% · 3 · 0 | 100.0% · 0 · 0 | 38.5% · 8 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/heldout-295-transaction-confirmation.txt | Plain text | 5 | 0.0% · 5 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 80.0% · 1 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-296-xbrl.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 2 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/heldout-297-xbrl.txt | Plain text | 4 | 0.0% · 4 · 0 | 75.0% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-298-xbrl.txt | Plain text | 10 | 0.0% · 10 · 0 | 100.0% · 0 · 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-299-xbrl.txt | Plain text | 3 | 100.0% · 0 · 0 | 100.0% · 0 · 10 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/heldout-300-xbrl.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 7 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |

</details>

## Timings

Seconds for each document and model. The first figure is the model finding the sensitive items. Models are run one after another, every document with one model before the next model is loaded, so a model is loaded into memory once.

<details><summary>Show the timing table (300 documents)</summary>

| Document | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|
| text/heldout-001-annual-report.txt | 0.0 | 0.1 | 12.7 | 3.1 | 8.4 | 8.7 | 6.8 |
| text/heldout-002-annual-report.txt | 0.0 | 0.1 | 8.3 | 1.3 | 11.6 | 8.2 | 2.7 |
| text/heldout-003-annual-report.txt | 0.0 | 0.0 | 11.0 | 2.0 | 12.4 | 10.4 | 20.9 |
| text/heldout-004-annual-report.txt | 0.0 | 0.0 | 7.9 | 2.5 | 16.4 | 15.3 | 7.5 |
| text/heldout-005-annual-report.txt | 0.0 | 0.0 | 5.7 | 1.8 | 9.6 | 8.6 | 0.7 |
| text/heldout-006-audit-report.txt | 0.0 | 0.1 | 8.8 | 1.7 | 15.6 | 11.2 | 7.7 |
| text/heldout-007-audit-report.txt | 0.0 | 0.1 | 4.7 | 4.1 | 21.8 | 15.9 | 4.8 |
| text/heldout-008-audit-report.txt | 0.0 | 0.1 | 3.4 | 1.7 | 10.0 | 7.8 | 4.3 |
| text/heldout-009-audit-report.txt | 0.0 | 0.1 | 7.2 | 1.5 | 15.6 | 9.7 | 2.9 |
| text/heldout-010-audit-report.txt | 0.0 | 0.1 | 17.2 | 2.8 | 15.4 | 12.8 | 4.5 |
| text/heldout-011-bai-format.txt | 0.0 | 0.0 | 8.1 | 1.5 | 16.0 | 13.1 | 3.7 |
| text/heldout-012-bai-format.txt | 0.0 | 0.0 | 4.1 | 2.1 | 14.1 | 14.0 | 7.0 |
| text/heldout-013-bai-format.txt | 0.0 | 0.0 | 3.5 | 1.1 | 6.1 | 5.6 | 3.9 |
| text/heldout-014-bai-format.txt | 0.0 | 0.0 | 20.0 | 1.9 | 55.4 | 55.5 | 11.5 |
| text/heldout-015-bai-format.txt | 0.0 | 0.0 | 6.6 | 3.3 | 24.5 | 30.5 | 5.2 |
| text/heldout-016-bank-statement.txt | 0.0 | 0.1 | 3.9 | 2.0 | 10.9 | 10.1 | 8.4 |
| text/heldout-017-bank-statement.txt | 0.0 | 0.0 | 1.8 | 0.5 | 4.8 | 4.8 | 1.6 |
| text/heldout-018-bank-statement.txt | 0.0 | 0.1 | 8.0 | 2.9 | 15.8 | 16.7 | 8.0 |
| text/heldout-019-bank-statement.txt | 0.0 | 0.0 | 5.8 | 2.6 | 11.2 | 9.7 | 6.0 |
| text/heldout-020-bank-statement.txt | 0.0 | 0.1 | 5.1 | 1.5 | 10.9 | 9.1 | 2.1 |
| text/heldout-021-bill-of-lading.txt | 0.0 | 0.0 | 6.9 | 4.3 | 32.1 | 20.2 | 20.5 |
| text/heldout-022-bill-of-lading.txt | 0.0 | 0.0 | 5.0 | 2.1 | 11.0 | 9.6 | 7.8 |
| text/heldout-023-bill-of-lading.txt | 0.0 | 0.0 | 10.2 | 3.3 | 23.4 | 14.9 | 16.9 |
| text/heldout-024-bill-of-lading.txt | 0.0 | 0.0 | 14.9 | 3.7 | 22.5 | 17.7 | 8.0 |
| text/heldout-025-bill-of-lading.txt | 0.0 | 0.0 | 13.5 | 6.7 | 24.2 | 22.3 | 30.1 |
| text/heldout-026-business-plan.txt | 0.0 | 0.1 | 7.5 | 1.5 | 11.2 | 12.7 | 5.6 |
| text/heldout-027-business-plan.txt | 0.0 | 0.1 | 11.5 | 2.5 | 14.6 | 12.5 | 5.0 |
| text/heldout-028-business-plan.txt | 0.0 | 0.0 | 1.9 | 1.0 | 8.5 | 6.8 | 1.8 |
| text/heldout-029-business-plan.txt | 0.0 | 0.1 | 3.7 | 1.8 | 29.5 | 19.4 | 23.8 |
| text/heldout-030-business-plan.txt | 0.0 | 0.0 | 3.8 | 2.8 | 23.8 | 18.4 | 4.5 |
| text/heldout-031-compliance-certificate.txt | 0.0 | 0.0 | 10.4 | 2.1 | 13.8 | 21.0 | 18.3 |
| text/heldout-032-compliance-certificate.txt | 0.0 | 0.0 | 6.4 | 1.8 | 10.4 | 15.8 | 3.2 |
| text/heldout-033-compliance-certificate.txt | 0.0 | 0.1 | 4.7 | 3.3 | 7.6 | 7.7 | 5.1 |
| text/heldout-034-compliance-certificate.txt | 0.0 | 0.0 | 4.3 | 2.4 | 12.6 | 16.3 | 7.7 |
| text/heldout-035-compliance-certificate.txt | 0.0 | 0.0 | 9.1 | 1.6 | 8.1 | 7.4 | 7.6 |
| text/heldout-036-corporate-governance-guidelines.txt | 0.0 | 0.1 | 1.1 | 0.5 | 4.9 | 3.7 | 0.7 |
| text/heldout-037-corporate-governance-guidelines.txt | 0.0 | 0.0 | 3.3 | 3.9 | 9.0 | 8.9 | 7.9 |
| text/heldout-038-corporate-governance-guidelines.txt | 0.0 | 0.0 | 2.0 | 1.0 | 5.1 | 3.9 | 2.0 |
| text/heldout-039-corporate-governance-guidelines.txt | 0.0 | 0.1 | 7.2 | 2.1 | 8.5 | 9.5 | 4.2 |
| text/heldout-040-corporate-governance-guidelines.txt | 0.0 | 0.0 | 4.1 | 1.9 | 8.8 | 8.6 | 15.9 |
| text/heldout-041-corporate-tax-return.txt | 0.0 | 0.0 | 2.2 | 1.0 | 7.0 | 5.7 | 3.5 |
| text/heldout-042-corporate-tax-return.txt | 0.0 | 0.0 | 3.8 | 1.4 | 8.1 | 8.0 | 3.2 |
| text/heldout-043-corporate-tax-return.txt | 0.0 | 0.0 | 8.8 | 2.6 | 19.7 | 17.0 | 21.8 |
| text/heldout-044-corporate-tax-return.txt | 0.0 | 0.0 | 1.7 | 1.3 | 7.1 | 6.3 | 1.2 |
| text/heldout-045-corporate-tax-return.txt | 0.0 | 0.0 | 8.0 | 1.8 | 21.7 | 19.9 | 5.0 |
| text/heldout-046-credit-application.txt | 0.0 | 0.0 | 6.8 | 2.0 | 11.5 | 10.8 | 5.4 |
| text/heldout-047-credit-application.txt | 0.0 | 0.0 | 6.7 | 1.3 | 10.8 | 7.9 | 2.3 |
| text/heldout-048-credit-application.txt | 0.0 | 0.0 | 12.6 | 3.4 | 23.0 | 19.3 | 2.7 |
| text/heldout-049-credit-application.txt | 0.0 | 0.0 | 7.2 | 2.5 | 11.2 | 11.3 | 3.0 |
| text/heldout-050-credit-application.txt | 0.0 | 0.0 | 3.7 | 1.3 | 7.5 | 7.4 | 2.3 |
| text/heldout-051-credit-card-application.txt | 0.0 | 0.0 | 3.3 | 1.1 | 6.5 | 6.1 | 2.5 |
| text/heldout-052-credit-card-application.txt | 0.0 | 0.1 | 9.2 | 0.6 | 7.7 | 6.6 | 0.9 |
| text/heldout-053-credit-card-application.txt | 0.0 | 0.0 | 3.8 | 1.5 | 7.1 | 7.1 | 1.9 |
| text/heldout-054-credit-card-application.txt | 0.0 | 0.0 | 3.8 | 1.3 | 6.7 | 6.5 | 2.4 |
| text/heldout-055-credit-card-application.txt | 0.0 | 0.0 | 2.7 | 1.2 | 5.6 | 5.7 | 2.0 |
| text/heldout-056-credit-card-statement.txt | 0.0 | 0.0 | 6.9 | 1.5 | 32.5 | 12.8 | 5.9 |
| text/heldout-057-credit-card-statement.txt | 0.0 | 0.1 | 7.1 | 4.8 | 32.4 | 17.6 | 3.2 |
| text/heldout-058-credit-card-statement.txt | 0.0 | 0.0 | 3.4 | 0.9 | 9.8 | 5.8 | 3.2 |
| text/heldout-059-credit-card-statement.txt | 0.0 | 0.0 | 8.2 | 4.7 | 35.7 | 16.6 | 2.5 |
| text/heldout-060-credit-card-statement.txt | 0.0 | 0.1 | 13.8 | 4.0 | 19.6 | 12.7 | 4.2 |
| text/heldout-061-cryptocurrency-transaction-report.txt | 0.0 | 0.0 | 5.6 | 0.4 | 2.5 | 3.1 | 0.4 |
| text/heldout-062-cryptocurrency-transaction-report.txt | 0.0 | 0.0 | 4.8 | 4.6 | 48.1 | 35.5 | 4.1 |
| text/heldout-063-cryptocurrency-transaction-report.txt | 0.0 | 0.0 | 12.0 | 0.3 | 2.5 | 20.1 | 0.4 |
| text/heldout-064-cryptocurrency-transaction-report.txt | 0.0 | 0.0 | 20.0 | 2.5 | 33.9 | 12.9 | 11.0 |
| text/heldout-065-cryptocurrency-transaction-report.txt | 0.0 | 0.0 | 15.3 | 4.8 | 34.6 | 17.1 | 1.5 |
| text/heldout-066-csv.txt | 0.0 | 0.0 | 3.1 | 0.8 | 9.3 | 6.8 | 1.5 |
| text/heldout-067-csv.txt | 0.0 | 0.0 | 0.5 | 0.3 | 2.5 | 3.0 | 0.3 |
| text/heldout-068-csv.txt | 0.0 | 0.1 | 30.5 | – | 88.7 | 78.4 | 11.2 |
| text/heldout-069-csv.txt | 0.0 | 0.1 | 102.7 | 42.5 | 93.4 | 83.4 | 17.2 |
| text/heldout-070-csv.txt | 0.0 | 0.0 | 3.3 | 3.5 | 23.8 | 16.3 | 1.9 |
| text/heldout-071-currency-exchange-rate-sheet.txt | 0.0 | 0.0 | 10.0 | 3.1 | 13.6 | 4.6 | 2.1 |
| text/heldout-072-currency-exchange-rate-sheet.txt | 0.0 | 0.0 | 3.6 | 0.7 | 7.1 | 6.7 | 1.8 |
| text/heldout-073-currency-exchange-rate-sheet.txt | 0.0 | 0.0 | 2.5 | 5.1 | 24.7 | 24.0 | 3.4 |
| text/heldout-074-currency-exchange-rate-sheet.txt | 0.0 | 0.0 | 6.8 | 4.6 | 12.5 | 10.3 | 5.5 |
| text/heldout-075-currency-exchange-rate-sheet.txt | 0.0 | 0.0 | 3.1 | 0.6 | 6.3 | 4.8 | 4.5 |
| text/heldout-076-customer-agreement.txt | 0.0 | 0.1 | 6.2 | 2.0 | 10.5 | 9.6 | 2.4 |
| text/heldout-077-customer-agreement.txt | 0.0 | 0.1 | 18.7 | 3.6 | 14.7 | 11.1 | 5.3 |
| text/heldout-078-customer-agreement.txt | 0.0 | 0.1 | 8.5 | 3.5 | 9.8 | 11.5 | 17.1 |
| text/heldout-079-customer-agreement.txt | 0.0 | 0.1 | 4.7 | 3.5 | 9.1 | 8.9 | 4.0 |
| text/heldout-080-customer-agreement.txt | 0.0 | 0.1 | 3.8 | 1.3 | 5.1 | 6.0 | 1.7 |
| text/heldout-081-customer-support-conversational-log.txt | 0.0 | 0.1 | 6.4 | 3.5 | 15.6 | 15.9 | 5.4 |
| text/heldout-082-customer-support-conversational-log.txt | 0.0 | 0.1 | 2.6 | 1.1 | 5.7 | 5.8 | 1.7 |
| text/heldout-083-customer-support-conversational-log.txt | 0.0 | 0.1 | 7.9 | 1.7 | 15.0 | 11.5 | 5.1 |
| text/heldout-084-customer-support-conversational-log.txt | 0.0 | 0.1 | 3.8 | 1.1 | 7.4 | 8.4 | 1.8 |
| text/heldout-085-customer-support-conversational-log.txt | 0.0 | 0.1 | 4.4 | 2.9 | 14.2 | 10.3 | 21.8 |
| text/heldout-086-dispute-resolution-policy.txt | 0.0 | 0.0 | 5.0 | 3.3 | 10.3 | 8.5 | 3.5 |
| text/heldout-087-dispute-resolution-policy.txt | 0.0 | 0.0 | 8.9 | 1.9 | 11.2 | 13.0 | 4.7 |
| text/heldout-088-dispute-resolution-policy.txt | 0.0 | 0.1 | 3.2 | 0.9 | 4.6 | 3.9 | 1.8 |
| text/heldout-089-dispute-resolution-policy.txt | 0.0 | 0.0 | 0.6 | 0.4 | 3.6 | 3.9 | 0.7 |
| text/heldout-090-dispute-resolution-policy.txt | 0.0 | 0.1 | 4.4 | 2.2 | 8.3 | 8.2 | 4.6 |
| text/heldout-091-edi.txt | 0.0 | 0.0 | 24.4 | 2.5 | 29.6 | 24.4 | 31.0 |
| text/heldout-092-edi.txt | 0.0 | 0.0 | 18.6 | 3.1 | 31.8 | 21.7 | 9.8 |
| text/heldout-093-edi.txt | 0.0 | 0.1 | – | 5.2 | 19.0 | – | 8.1 |
| text/heldout-094-edi.txt | 0.0 | 0.0 | 4.8 | 4.0 | 14.8 | 14.7 | 2.5 |
| text/heldout-095-edi.txt | 0.0 | 0.0 | 7.0 | 2.1 | 12.5 | 15.4 | 4.3 |
| text/heldout-096-email.txt | 0.0 | 0.0 | 1.7 | 1.1 | 6.9 | 7.9 | 3.9 |
| text/heldout-097-email.txt | 0.0 | 0.0 | 7.2 | 2.4 | 17.4 | 19.4 | 3.2 |
| text/heldout-098-email.txt | 0.0 | 0.1 | 7.8 | 2.2 | 11.2 | 14.6 | 3.2 |
| text/heldout-099-email.txt | 0.0 | 0.1 | 9.4 | 1.0 | 4.6 | 7.0 | 2.0 |
| text/heldout-100-email.txt | 0.0 | 0.0 | 6.8 | 1.9 | 8.2 | 7.7 | 7.2 |
| text/heldout-101-employment-contract.txt | 0.0 | 0.0 | 10.6 | 5.1 | 17.9 | 16.0 | 23.5 |
| text/heldout-102-employment-contract.txt | 0.0 | 0.1 | 8.3 | 6.9 | 11.5 | 9.4 | 6.9 |
| text/heldout-103-employment-contract.txt | 0.0 | 0.0 | 6.1 | 3.8 | 9.1 | 10.6 | 4.6 |
| text/heldout-104-employment-contract.txt | 0.0 | 0.1 | 6.2 | 2.8 | 10.1 | 11.2 | 2.6 |
| text/heldout-105-employment-contract.txt | 0.0 | 0.1 | 14.5 | 3.7 | 15.7 | 15.7 | 5.4 |
| text/heldout-106-financial-aid-application.txt | 0.0 | 0.0 | 9.0 | 2.6 | 24.7 | 12.1 | 6.7 |
| text/heldout-107-financial-aid-application.txt | 0.0 | 0.1 | 4.8 | 1.8 | 9.7 | 22.9 | 6.9 |
| text/heldout-108-financial-aid-application.txt | 0.0 | 0.0 | 4.9 | 1.6 | 8.2 | 32.8 | 5.0 |
| text/heldout-109-financial-aid-application.txt | 0.0 | 0.0 | 8.2 | 3.0 | 16.3 | 13.2 | 2.4 |
| text/heldout-110-financial-aid-application.txt | 0.0 | 0.1 | 7.2 | 1.4 | 8.5 | 9.4 | 5.6 |
| text/heldout-111-financial-data-feed.txt | 0.0 | 0.0 | 7.4 | 1.6 | 5.9 | 13.6 | 4.4 |
| text/heldout-112-financial-data-feed.txt | 0.0 | 0.0 | 0.5 | 0.3 | 2.2 | 2.5 | 0.3 |
| text/heldout-113-financial-data-feed.txt | 0.0 | 0.0 | 2.7 | 1.1 | 5.4 | 5.5 | 1.9 |
| text/heldout-114-financial-data-feed.txt | 0.0 | 0.0 | 4.6 | 1.3 | 5.8 | 6.6 | 1.5 |
| text/heldout-115-financial-data-feed.txt | 0.0 | 0.0 | 3.1 | 1.2 | 5.5 | 5.0 | 2.0 |
| text/heldout-116-financial-disclosure-statement.txt | 0.0 | 0.0 | 2.7 | 1.3 | 6.6 | 7.4 | 1.6 |
| text/heldout-117-financial-disclosure-statement.txt | 0.0 | 0.0 | 9.3 | 2.1 | 14.2 | 10.4 | 2.7 |
| text/heldout-118-financial-disclosure-statement.txt | 0.0 | 0.0 | 9.0 | 2.1 | 19.5 | 13.5 | 5.7 |
| text/heldout-119-financial-disclosure-statement.txt | 0.0 | 0.0 | 13.1 | 4.3 | 21.7 | 22.2 | 11.2 |
| text/heldout-120-financial-disclosure-statement.txt | 0.0 | 0.0 | 6.0 | 3.2 | 17.6 | 14.6 | 3.3 |
| text/heldout-121-financial-forecast.txt | 0.0 | 0.0 | 7.7 | 2.2 | 10.1 | 9.1 | 2.0 |
| text/heldout-122-financial-forecast.txt | 0.0 | 0.1 | 2.6 | 1.6 | 9.7 | 10.7 | 33.1 |
| text/heldout-123-financial-forecast.txt | 0.0 | 0.1 | 1.1 | 0.5 | 4.9 | 3.9 | 0.7 |
| text/heldout-124-financial-forecast.txt | 0.0 | 0.1 | 12.6 | 0.6 | 3.6 | 3.7 | 1.1 |
| text/heldout-125-financial-forecast.txt | 0.0 | 0.0 | 5.7 | 0.9 | 4.7 | 4.6 | 1.7 |
| text/heldout-126-financial-regulatory-compliance-report.txt | 0.0 | 0.0 | 3.6 | 1.0 | 9.6 | 6.5 | 1.9 |
| text/heldout-127-financial-regulatory-compliance-report.txt | 0.0 | 0.0 | 3.7 | 1.2 | 8.0 | 8.8 | 3.2 |
| text/heldout-128-financial-regulatory-compliance-report.txt | 0.0 | 0.0 | 7.4 | 1.3 | 7.4 | 8.9 | 3.5 |
| text/heldout-129-financial-regulatory-compliance-report.txt | 0.0 | 0.1 | 2.3 | 1.3 | 11.1 | 8.2 | 2.9 |
| text/heldout-130-financial-regulatory-compliance-report.txt | 0.0 | 0.1 | 6.3 | 2.4 | 6.0 | 6.6 | 2.1 |
| text/heldout-131-financial-risk-assessment.txt | 0.0 | 0.1 | 11.4 | 3.5 | 32.9 | 26.0 | 6.5 |
| text/heldout-132-financial-risk-assessment.txt | 0.0 | 0.1 | 6.7 | 3.1 | 15.0 | 14.2 | 1.8 |
| text/heldout-133-financial-risk-assessment.txt | 0.0 | 0.1 | 3.8 | 2.9 | 16.6 | 12.8 | 2.3 |
| text/heldout-134-financial-risk-assessment.txt | 0.0 | 0.0 | 4.0 | 2.4 | 14.9 | 11.7 | 5.1 |
| text/heldout-135-financial-risk-assessment.txt | 0.0 | 0.1 | 5.4 | 1.6 | 8.4 | 7.7 | 2.2 |
| text/heldout-136-financial-statement.txt | 0.0 | 0.0 | 6.3 | 1.9 | 12.9 | 8.7 | 3.4 |
| text/heldout-137-financial-statement.txt | 0.0 | 0.0 | 1.0 | 0.6 | 5.0 | 3.9 | 3.0 |
| text/heldout-138-financial-statement.txt | 0.0 | 0.0 | 14.9 | 2.8 | 11.8 | 12.7 | 34.9 |
| text/heldout-139-financial-statement.txt | 0.0 | 0.0 | 1.7 | 0.7 | 5.7 | 4.3 | 2.7 |
| text/heldout-140-financial-statement.txt | 0.0 | 0.1 | 14.2 | 4.4 | 18.6 | 17.7 | 8.0 |
| text/heldout-141-fix-protocol.txt | 0.0 | 0.0 | 11.0 | 1.8 | 36.5 | 37.5 | 23.6 |
| text/heldout-142-fix-protocol.txt | 0.0 | 0.0 | 6.4 | 2.0 | 13.3 | 17.8 | 6.2 |
| text/heldout-143-fix-protocol.txt | 0.0 | 0.0 | 7.2 | 1.1 | 11.6 | 7.6 | 4.1 |
| text/heldout-144-fix-protocol.txt | 0.0 | 0.0 | 7.0 | 3.1 | 89.3 | 15.9 | 9.7 |
| text/heldout-145-fix-protocol.txt | 0.0 | 0.0 | 3.8 | 0.8 | 9.2 | 8.2 | 2.5 |
| text/heldout-146-fpml.txt | 0.0 | 0.0 | 6.6 | 2.5 | 7.9 | 7.6 | 3.2 |
| text/heldout-147-fpml.txt | 0.0 | 0.0 | 8.1 | 0.7 | 5.6 | 6.1 | 2.3 |
| text/heldout-148-fpml.txt | 0.0 | 0.0 | 11.7 | 1.3 | 9.0 | 4.6 | 2.5 |
| text/heldout-149-fpml.txt | 0.0 | 0.0 | 2.8 | 1.1 | 5.0 | 5.3 | 2.7 |
| text/heldout-150-fpml.txt | 0.0 | 0.0 | 9.7 | 1.8 | 9.0 | 7.2 | 4.2 |
| text/heldout-151-health-insurance-claim-form.txt | 0.0 | 0.1 | 14.7 | 5.0 | 26.0 | 21.2 | 6.6 |
| text/heldout-152-health-insurance-claim-form.txt | 0.0 | 0.0 | 13.5 | 4.0 | 17.5 | 15.4 | 3.8 |
| text/heldout-153-health-insurance-claim-form.txt | 0.0 | 0.0 | 9.8 | 2.9 | 14.8 | 14.6 | 5.1 |
| text/heldout-154-health-insurance-claim-form.txt | 0.0 | 0.0 | 10.8 | 3.9 | 19.4 | 12.9 | 8.0 |
| text/heldout-155-health-insurance-claim-form.txt | 0.0 | 0.0 | 9.4 | 2.8 | 15.9 | 11.8 | 9.1 |
| text/heldout-156-insurance-claim-form.txt | 0.0 | 0.0 | 3.3 | 1.2 | 7.0 | 6.7 | 1.8 |
| text/heldout-157-insurance-claim-form.txt | 0.0 | 0.0 | 8.8 | 3.1 | 12.5 | 12.4 | 4.0 |
| text/heldout-158-insurance-claim-form.txt | 0.0 | 0.0 | 12.4 | 5.0 | 23.5 | 21.3 | 7.2 |
| text/heldout-159-insurance-claim-form.txt | 0.0 | 0.0 | 12.4 | 5.7 | 21.8 | 26.4 | 7.0 |
| text/heldout-160-insurance-claim-form.txt | 0.0 | 0.0 | 10.5 | 3.2 | 18.8 | 19.2 | 2.6 |
| text/heldout-161-insurance-policy.txt | 0.0 | 0.0 | 6.8 | 1.7 | 13.5 | 14.5 | 13.6 |
| text/heldout-162-insurance-policy.txt | 0.0 | 0.0 | 12.5 | 3.8 | 9.7 | 8.7 | 3.6 |
| text/heldout-163-insurance-policy.txt | 0.0 | 0.0 | 7.1 | 3.3 | 8.6 | 9.3 | 29.6 |
| text/heldout-164-insurance-policy.txt | 0.0 | 0.0 | 12.7 | 3.0 | 13.8 | 14.3 | 16.7 |
| text/heldout-165-insurance-policy.txt | 0.0 | 0.0 | 6.7 | 2.4 | 14.2 | 13.9 | 3.7 |
| text/heldout-166-investment-prospectus.txt | 0.0 | 0.1 | 2.0 | 0.5 | 4.6 | 5.0 | 0.9 |
| text/heldout-167-investment-prospectus.txt | 0.0 | 0.1 | 7.0 | 1.7 | 12.6 | 10.3 | 2.2 |
| text/heldout-168-investment-prospectus.txt | 0.0 | 0.1 | 5.8 | 2.0 | 13.6 | 11.8 | 7.4 |
| text/heldout-169-investment-prospectus.txt | 0.0 | 0.1 | 4.0 | 3.1 | 13.2 | 12.9 | 4.5 |
| text/heldout-170-investment-prospectus.txt | 0.0 | 0.0 | 7.1 | 3.0 | 15.1 | 10.9 | 57.9 |
| text/heldout-171-isda-definition.txt | 0.0 | 0.1 | 12.4 | 2.6 | 4.8 | 3.9 | 6.2 |
| text/heldout-172-isda-definition.txt | 0.0 | 0.1 | 7.8 | 2.7 | 12.3 | 12.4 | 22.6 |
| text/heldout-173-isda-definition.txt | 0.0 | 0.1 | – | 1.4 | 18.8 | 14.4 | 16.0 |
| text/heldout-174-isda-definition.txt | 0.0 | 0.1 | 2.1 | 0.7 | 5.0 | 6.8 | 1.9 |
| text/heldout-175-isda-definition.txt | 0.0 | 0.1 | 1.1 | 0.6 | 4.9 | 3.7 | 0.7 |
| text/heldout-176-it-support-ticket.txt | 0.0 | 0.1 | 4.3 | 2.5 | 12.8 | 15.9 | 4.8 |
| text/heldout-177-it-support-ticket.txt | 0.0 | 0.1 | 3.7 | 0.5 | 15.2 | 11.5 | 3.5 |
| text/heldout-178-it-support-ticket.txt | 0.0 | 0.0 | 1.0 | 0.8 | 4.3 | 8.5 | 0.6 |
| text/heldout-179-it-support-ticket.txt | 0.0 | 0.0 | 2.1 | 0.4 | 3.8 | 3.7 | 0.6 |
| text/heldout-180-it-support-ticket.txt | 0.0 | 0.1 | 7.6 | 2.8 | 15.3 | 15.8 | 3.6 |
| text/heldout-181-loan-agreement.txt | 0.0 | 0.1 | 3.1 | 0.6 | 5.9 | 6.1 | 2.6 |
| text/heldout-182-loan-agreement.txt | 0.0 | 0.0 | 7.9 | 7.9 | 12.3 | 10.1 | 5.1 |
| text/heldout-183-loan-agreement.txt | 0.0 | 0.0 | 7.6 | – | 11.5 | 11.0 | 7.5 |
| text/heldout-184-loan-agreement.txt | 0.0 | 0.0 | 5.3 | 1.7 | 10.5 | 8.9 | 5.4 |
| text/heldout-185-loan-agreement.txt | 0.0 | 0.1 | 5.1 | 1.8 | 10.4 | 10.8 | 4.4 |
| text/heldout-186-loan-application.txt | 0.0 | 0.0 | 12.5 | 3.8 | 25.5 | 22.8 | 16.0 |
| text/heldout-187-loan-application.txt | 0.0 | 0.0 | 8.9 | 4.3 | 16.6 | 15.0 | 8.2 |
| text/heldout-188-loan-application.txt | 0.0 | 0.0 | 12.2 | 4.2 | 15.8 | 15.2 | 12.1 |
| text/heldout-189-loan-application.txt | 0.0 | 0.1 | 16.5 | 5.4 | 22.2 | 23.6 | 19.7 |
| text/heldout-190-loan-application.txt | 0.0 | 0.0 | 8.1 | 4.4 | 15.8 | 13.6 | 6.8 |
| text/heldout-191-mortgage-amortization-schedule.txt | 0.0 | 0.0 | 15.9 | 1.7 | 7.3 | 7.4 | 2.8 |
| text/heldout-192-mortgage-amortization-schedule.txt | 0.0 | 0.0 | 1.0 | 4.3 | 5.3 | 4.0 | 0.7 |
| text/heldout-193-mortgage-amortization-schedule.txt | 0.0 | 0.0 | 0.9 | 0.5 | 4.8 | 3.9 | 0.6 |
| text/heldout-194-mortgage-amortization-schedule.txt | 0.0 | 0.0 | 2.8 | 0.6 | 5.0 | 5.4 | 2.7 |
| text/heldout-195-mortgage-amortization-schedule.txt | 0.0 | 0.0 | 1.4 | 0.8 | 3.6 | 3.8 | 0.9 |
| text/heldout-196-mortgage-contract.txt | 0.0 | 0.0 | 8.0 | 2.0 | 11.9 | 9.0 | 6.7 |
| text/heldout-197-mortgage-contract.txt | 0.0 | 0.0 | 9.6 | 2.6 | 14.0 | 11.7 | 7.5 |
| text/heldout-198-mortgage-contract.txt | 0.0 | 0.0 | 9.1 | 4.7 | 9.0 | 8.7 | 3.1 |
| text/heldout-199-mortgage-contract.txt | 0.0 | 0.1 | 9.6 | 2.4 | 11.3 | 8.8 | 4.5 |
| text/heldout-200-mortgage-contract.txt | 0.0 | 0.1 | 1.1 | 1.3 | 4.8 | 3.8 | 0.9 |
| text/heldout-201-mt940.txt | 0.0 | 0.0 | 13.0 | 3.7 | 36.7 | 33.1 | 17.3 |
| text/heldout-202-mt940.txt | 0.0 | 0.0 | 14.1 | 1.2 | 54.0 | 43.8 | 31.5 |
| text/heldout-203-mt940.txt | 0.0 | 0.0 | 24.1 | 5.9 | 53.8 | 52.5 | 16.7 |
| text/heldout-204-mt940.txt | 0.0 | 0.0 | 7.3 | 4.2 | 21.0 | 20.2 | 16.3 |
| text/heldout-205-mt940.txt | 0.0 | 0.0 | 29.2 | 7.9 | 45.4 | 35.2 | 10.8 |
| text/heldout-206-payment-confirmation.txt | 0.0 | 0.0 | 7.0 | 1.5 | 10.8 | 7.9 | 8.6 |
| text/heldout-207-payment-confirmation.txt | 0.0 | 0.0 | 4.4 | 1.5 | 11.5 | 9.2 | 2.0 |
| text/heldout-208-payment-confirmation.txt | 0.0 | 0.0 | 10.7 | 4.2 | 18.1 | 19.8 | 5.8 |
| text/heldout-209-payment-confirmation.txt | 0.0 | 0.0 | 4.1 | 1.2 | 7.3 | 5.5 | 2.8 |
| text/heldout-210-payment-confirmation.txt | 0.0 | 0.0 | 5.1 | 1.4 | 9.2 | 8.9 | 5.7 |
| text/heldout-211-pension-plan-agreement.txt | 0.0 | 0.0 | 7.3 | 2.9 | 11.6 | 11.4 | 39.3 |
| text/heldout-212-pension-plan-agreement.txt | 0.0 | 0.0 | 8.6 | 1.2 | 9.1 | 11.6 | 1.9 |
| text/heldout-213-pension-plan-agreement.txt | 0.0 | 0.0 | 6.0 | 2.7 | 15.0 | 11.8 | 8.8 |
| text/heldout-214-pension-plan-agreement.txt | 0.0 | 0.0 | 7.2 | 1.5 | 11.0 | 9.3 | 2.2 |
| text/heldout-215-pension-plan-agreement.txt | 0.0 | 0.1 | 6.1 | 1.7 | 9.1 | 8.3 | 2.1 |
| text/heldout-216-policyholder-s-report.txt | 0.0 | 0.0 | 7.4 | 2.7 | 12.8 | 11.9 | 23.9 |
| text/heldout-217-policyholder-s-report.txt | 0.0 | 0.0 | 7.9 | 2.7 | 8.4 | 33.4 | 6.1 |
| text/heldout-218-policyholder-s-report.txt | 0.0 | 0.0 | 4.4 | 2.2 | 10.1 | 11.4 | 2.0 |
| text/heldout-219-policyholder-s-report.txt | 0.0 | 0.0 | 8.9 | 3.3 | 23.1 | 26.9 | 6.7 |
| text/heldout-220-policyholder-s-report.txt | 0.0 | 0.1 | 4.6 | 2.0 | 9.0 | 9.9 | 3.8 |
| text/heldout-221-privacy-policy.txt | 0.0 | 0.1 | 9.3 | 2.0 | 6.3 | 5.0 | 2.7 |
| text/heldout-222-privacy-policy.txt | 0.0 | 0.1 | 2.7 | 1.2 | 5.0 | 6.5 | 2.1 |
| text/heldout-223-privacy-policy.txt | 0.0 | 0.1 | 3.7 | 1.1 | 8.8 | 9.4 | 2.9 |
| text/heldout-224-privacy-policy.txt | 0.0 | 0.1 | 3.0 | 0.6 | 5.1 | 3.8 | 1.0 |
| text/heldout-225-privacy-policy.txt | 0.0 | 0.1 | 9.3 | 2.0 | 4.7 | 3.5 | 3.9 |
| text/heldout-226-product-disclosure-statement.txt | 0.0 | 0.1 | 8.9 | 3.8 | 19.2 | 3.6 | 0.7 |
| text/heldout-227-product-disclosure-statement.txt | 0.0 | 0.1 | 3.6 | 1.8 | 9.8 | 8.4 | 1.4 |
| text/heldout-228-product-disclosure-statement.txt | 0.0 | 0.1 | 1.7 | 1.8 | 3.7 | 4.1 | 1.9 |
| text/heldout-229-product-disclosure-statement.txt | 0.0 | 0.1 | 5.7 | 1.7 | 9.1 | 8.4 | 11.1 |
| text/heldout-230-product-disclosure-statement.txt | 0.0 | 0.0 | 2.5 | 1.1 | 8.2 | 7.0 | 1.7 |
| text/heldout-231-real-estate-loan-agreement.txt | 0.0 | 0.1 | 5.2 | 1.0 | 4.9 | 3.7 | 0.9 |
| text/heldout-232-real-estate-loan-agreement.txt | 0.0 | 0.0 | 10.5 | 2.3 | 9.6 | 4.9 | 5.0 |
| text/heldout-233-real-estate-loan-agreement.txt | 0.0 | 0.1 | 8.4 | 2.0 | 11.1 | 8.4 | 18.0 |
| text/heldout-234-real-estate-loan-agreement.txt | 0.0 | 0.0 | 12.9 | 5.4 | 16.3 | 9.2 | 7.0 |
| text/heldout-235-real-estate-loan-agreement.txt | 0.0 | 0.1 | 5.7 | 3.3 | 8.6 | 10.0 | 14.9 |
| text/heldout-236-regulatory-compliance-guide.txt | 0.0 | 0.1 | 9.6 | 3.1 | 16.0 | 14.3 | 4.8 |
| text/heldout-237-regulatory-compliance-guide.txt | 0.0 | 0.0 | 4.4 | 1.3 | 7.1 | 8.5 | 2.2 |
| text/heldout-238-regulatory-compliance-guide.txt | 0.0 | 0.1 | 1.1 | 2.6 | 4.9 | 3.7 | 0.7 |
| text/heldout-239-regulatory-compliance-guide.txt | 0.0 | 0.1 | 2.7 | 2.7 | 15.9 | 10.9 | 1.7 |
| text/heldout-240-regulatory-compliance-guide.txt | 0.0 | 0.1 | 1.8 | 1.1 | 4.2 | 4.4 | 1.3 |
| text/heldout-241-regulatory-filing.txt | 0.0 | 0.0 | 8.6 | 2.4 | 12.5 | 12.1 | 4.4 |
| text/heldout-242-regulatory-filing.txt | 0.0 | 0.0 | 5.2 | 1.8 | 8.4 | 7.7 | 4.5 |
| text/heldout-243-regulatory-filing.txt | 0.0 | 0.0 | 8.8 | 0.9 | 7.7 | 4.6 | 1.5 |
| text/heldout-244-regulatory-filing.txt | 0.0 | 0.0 | 5.1 | 1.8 | 12.8 | 13.4 | 9.6 |
| text/heldout-245-regulatory-filing.txt | 0.0 | 0.0 | 5.1 | 0.8 | 17.2 | 8.9 | 4.5 |
| text/heldout-246-renewal-reminder.txt | 0.0 | 0.0 | 7.0 | 1.0 | 6.8 | 7.3 | 4.3 |
| text/heldout-247-renewal-reminder.txt | 0.0 | 0.0 | 9.4 | 3.5 | 11.8 | 17.5 | 8.1 |
| text/heldout-248-renewal-reminder.txt | 0.0 | 0.0 | 3.1 | 0.6 | 5.3 | 4.0 | 1.8 |
| text/heldout-249-renewal-reminder.txt | 0.0 | 0.0 | 4.3 | 2.0 | 7.4 | 21.3 | 2.4 |
| text/heldout-250-renewal-reminder.txt | 0.0 | 0.0 | 8.6 | 2.9 | 14.2 | 11.9 | 15.0 |
| text/heldout-251-safety-data-sheet.txt | 0.0 | 0.0 | 0.7 | 0.4 | 3.4 | 4.5 | 0.6 |
| text/heldout-252-safety-data-sheet.txt | 0.0 | 0.0 | 2.0 | 0.4 | 4.6 | 4.6 | 2.4 |
| text/heldout-253-safety-data-sheet.txt | 0.0 | 0.0 | 1.9 | 0.6 | 4.7 | 4.5 | 1.5 |
| text/heldout-254-safety-data-sheet.txt | 0.0 | 0.0 | 3.4 | 1.4 | 7.0 | 6.4 | 1.6 |
| text/heldout-255-safety-data-sheet.txt | 0.0 | 0.1 | 3.8 | 1.3 | 7.1 | 6.6 | 3.0 |
| text/heldout-256-securities-prospectus.txt | 0.0 | 0.0 | 8.2 | 2.0 | 8.1 | 12.7 | 23.6 |
| text/heldout-257-securities-prospectus.txt | 0.0 | 0.1 | 11.9 | 2.8 | 11.7 | 9.6 | 10.6 |
| text/heldout-258-securities-prospectus.txt | 0.0 | 0.0 | 5.2 | 1.3 | 8.1 | 8.3 | 5.4 |
| text/heldout-259-securities-prospectus.txt | 0.0 | 0.1 | 8.5 | 2.1 | 8.6 | 9.0 | 2.4 |
| text/heldout-260-securities-prospectus.txt | 0.0 | 0.0 | – | 0.6 | 5.7 | 6.2 | 2.0 |
| text/heldout-261-shareholder-agreement.txt | 0.0 | 0.1 | 8.8 | 3.3 | 6.5 | 4.6 | 1.2 |
| text/heldout-262-shareholder-agreement.txt | 0.0 | 0.0 | 6.8 | 1.4 | 9.2 | 7.3 | 5.1 |
| text/heldout-263-shareholder-agreement.txt | 0.0 | 0.1 | 19.4 | 3.9 | 4.9 | 3.5 | 0.9 |
| text/heldout-264-shareholder-agreement.txt | 0.0 | 0.1 | 4.8 | 6.5 | 8.8 | 8.5 | 2.2 |
| text/heldout-265-shareholder-agreement.txt | 0.0 | 0.0 | 4.4 | 3.3 | 4.7 | 3.5 | 1.1 |
| text/heldout-266-supply-chain-management-agreement.txt | 0.0 | 0.1 | 5.8 | 4.3 | 29.2 | 15.6 | 6.5 |
| text/heldout-267-supply-chain-management-agreement.txt | 0.0 | 0.1 | 11.1 | 2.9 | 4.9 | 3.7 | 0.9 |
| text/heldout-268-supply-chain-management-agreement.txt | 0.0 | 0.1 | 11.8 | 5.2 | 23.9 | 20.1 | 4.2 |
| text/heldout-269-supply-chain-management-agreement.txt | 0.0 | 0.0 | 1.4 | 0.7 | 3.0 | 4.3 | 0.6 |
| text/heldout-270-supply-chain-management-agreement.txt | 0.0 | 0.1 | 8.0 | 2.3 | 9.4 | 6.5 | 2.5 |
| text/heldout-271-swift-message.txt | 0.0 | 0.0 | 11.2 | 3.7 | 21.3 | 22.3 | 67.5 |
| text/heldout-272-swift-message.txt | 0.0 | 0.0 | 10.2 | 0.3 | 11.1 | 12.4 | 10.4 |
| text/heldout-273-swift-message.txt | 0.0 | 0.0 | 5.3 | 2.6 | 33.8 | 31.8 | 14.5 |
| text/heldout-274-swift-message.txt | 0.0 | 0.0 | 6.4 | 0.6 | 19.2 | 35.8 | 11.8 |
| text/heldout-275-swift-message.txt | 0.0 | 0.0 | 27.9 | 0.5 | 50.1 | 7.5 | 12.9 |
| text/heldout-276-tax-assessment-notice.txt | 0.0 | 0.0 | 3.6 | 0.6 | 5.6 | 4.7 | 2.9 |
| text/heldout-277-tax-assessment-notice.txt | 0.0 | 0.0 | 6.3 | 1.4 | 7.8 | 7.7 | 4.9 |
| text/heldout-278-tax-assessment-notice.txt | 0.0 | 0.0 | 8.2 | 2.4 | 13.6 | 11.6 | 7.8 |
| text/heldout-279-tax-assessment-notice.txt | 0.0 | 0.0 | 8.4 | 1.8 | 16.2 | 13.9 | 5.2 |
| text/heldout-280-tax-assessment-notice.txt | 0.0 | 0.0 | 12.1 | 2.8 | 17.9 | 18.0 | 2.3 |
| text/heldout-281-tax-return.txt | 0.0 | 0.0 | 8.6 | 2.2 | 11.0 | 10.0 | 8.8 |
| text/heldout-282-tax-return.txt | 0.0 | 0.0 | 3.5 | 1.5 | 7.7 | 9.6 | 4.7 |
| text/heldout-283-tax-return.txt | 0.0 | 0.0 | 10.3 | 1.7 | 9.1 | 8.7 | 2.7 |
| text/heldout-284-tax-return.txt | 0.0 | 0.0 | 4.3 | 1.1 | 9.8 | 8.4 | 5.1 |
| text/heldout-285-tax-return.txt | 0.0 | 0.0 | 5.7 | 1.9 | 12.4 | 10.6 | 49.0 |
| text/heldout-286-trade-confirmation.txt | 0.0 | 0.0 | 7.0 | 1.4 | 9.3 | 6.6 | 4.1 |
| text/heldout-287-trade-confirmation.txt | 0.0 | 0.0 | 8.6 | 2.2 | 18.2 | 12.7 | 6.6 |
| text/heldout-288-trade-confirmation.txt | 0.0 | 0.0 | 6.5 | 2.2 | 16.9 | 10.7 | 4.0 |
| text/heldout-289-trade-confirmation.txt | 0.0 | 0.0 | 5.9 | 0.9 | 5.8 | 6.7 | 2.3 |
| text/heldout-290-trade-confirmation.txt | 0.0 | 0.0 | 16.2 | 6.3 | 29.2 | 27.9 | 7.4 |
| text/heldout-291-transaction-confirmation.txt | 0.0 | 0.0 | 10.9 | 3.3 | 19.0 | 16.7 | 33.4 |
| text/heldout-292-transaction-confirmation.txt | 0.0 | 0.0 | 4.8 | 1.2 | 7.2 | 7.1 | 7.9 |
| text/heldout-293-transaction-confirmation.txt | 0.0 | 0.0 | 7.3 | 1.2 | 7.0 | 6.5 | 5.0 |
| text/heldout-294-transaction-confirmation.txt | 0.0 | 0.0 | 9.4 | 0.6 | 18.6 | 16.0 | 9.1 |
| text/heldout-295-transaction-confirmation.txt | 0.0 | 0.0 | 9.6 | 1.7 | 13.6 | 11.9 | 3.9 |
| text/heldout-296-xbrl.txt | 0.0 | 0.1 | 18.5 | 0.3 | 2.4 | 3.2 | 0.4 |
| text/heldout-297-xbrl.txt | 0.0 | 0.0 | 2.6 | 0.9 | 4.8 | 9.6 | 3.1 |
| text/heldout-298-xbrl.txt | 0.0 | 0.0 | 3.3 | 1.1 | 7.4 | 8.7 | 4.4 |
| text/heldout-299-xbrl.txt | 0.0 | 0.0 | 4.2 | 0.9 | 4.9 | 10.5 | 5.3 |
| text/heldout-300-xbrl.txt | 0.0 | 0.0 | 21.1 | 4.8 | 2.5 | 3.1 | 0.4 |
| **Total** | **0.1** | **13.0** | **2271.8** | **717.2** | **4114.0** | **3627.9** | **2023.0** |
| Average per document | 0.0 | 0.0 | 7.6 | 2.4 | 13.7 | 12.1 | 6.7 |

</details>

## What was missed and over-redacted
*Contains text from the documents.*

### rules only

**Missed**

- `text/heldout-001-annual-report.txt` PERSON: “Eliana R. Bonolis” (1)
- `text/heldout-003-annual-report.txt` ID_NUMBER: “996238618” (1)
- `text/heldout-004-annual-report.txt` COMPANY: “2024 Inc.” (3)
- `text/heldout-004-annual-report.txt` PERSON: “Aimée Garnier-Dias” (1)
- `text/heldout-004-annual-report.txt` ID_NUMBER: “X-758472-D” (1)
- `text/heldout-004-annual-report.txt` PHONE: “488-430-9265” (1)
- `text/heldout-004-annual-report.txt` COMPANY: “2 024 Inc.” (1)
- `text/heldout-006-audit-report.txt` PERSON: “Ugolino Cainero-Paruta” (1)
- `text/heldout-007-audit-report.txt` PERSON: “Gösta M. Magnusson” (1)
- `text/heldout-007-audit-report.txt` ADDRESS: “63 rue Isabelle Rodrigues” (1)
- `text/heldout-007-audit-report.txt` ID_NUMBER: “62997511170555282974” (1)
- `text/heldout-008-audit-report.txt` PERSON: “Emigdio Jo” (1)
- `text/heldout-009-audit-report.txt` ADDRESS: “8854 April Lakes” (1)
- `text/heldout-010-audit-report.txt` SECRET: “_wfy65KIEsPmYvs^1z” (1)
- `text/heldout-011-bai-format.txt` ID_NUMBER: “CUST64550460” (1)
- `text/heldout-011-bai-format.txt` ID_NUMBER: “BAI02100001” (1)
- `text/heldout-012-bai-format.txt` PERSON: “Micaela R. Barranco” (1)
- `text/heldout-012-bai-format.txt` ID_NUMBER: “205932082” (1)
- `text/heldout-012-bai-format.txt` ADDRESS: “028 Villa Mountain, Suite 594” (1)
- `text/heldout-012-bai-format.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-012-bai-format.txt` ADDRESS: “94043” (1)
- `text/heldout-013-bai-format.txt` PERSON: “Ciro” (1)
- `text/heldout-013-bai-format.txt` PERSON: “Robert Mark Riggs” (1)
- `text/heldout-013-bai-format.txt` ADDRESS: “5539 Howe Points, South Alanberg” (1)
- `text/heldout-014-bai-format.txt` PERSON: “Rhys L. Jones” (1)
- `text/heldout-014-bai-format.txt` ADDRESS: “9/2 Fischerstr., 67274, Schongau” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “976481327” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “1234567890” (23)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “123456” (3)
- `text/heldout-015-bai-format.txt` PERSON: “BEATRIX SENOL GIRSCHNER” (4)
- `text/heldout-015-bai-format.txt` ADDRESS: “295 Ashleyhof” (5)
- `text/heldout-015-bai-format.txt` PERSON: “Beatrix Senol Girschner” (1)
- `text/heldout-015-bai-format.txt` ID_NUMBER: “GB29 ABCD1234567890123456” (1)
- `text/heldout-015-bai-format.txt` ID_NUMBER: “GB98 EFGH1234567890123456” (1)
- `text/heldout-016-bank-statement.txt` PERSON: “Aitor Iglesias” (1)
- `text/heldout-016-bank-statement.txt` ADDRESS: “4251 Nathan St, Patrickton” (1)
- `text/heldout-017-bank-statement.txt` PERSON: “John Doe” (1)
- `text/heldout-018-bank-statement.txt` PERSON: “John Doe” (2)
- `text/heldout-018-bank-statement.txt` ADDRESS: “123 Main Street, Toronto, ON, M5G 1M6” (1)
- `text/heldout-018-bank-statement.txt` PHONE: “(416) 123-4567” (1)
- `text/heldout-018-bank-statement.txt` ADDRESS: “456 Elm Street, Toronto, ON, M5H 2L7” (1)
- `text/heldout-018-bank-statement.txt` ID_NUMBER: “123-456-789” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Édith” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Anthony Carpenter” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “2345 River Road, Apt. 091” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “San Francisco, CA 94112” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Anthony Carpeter” (1)
- `text/heldout-020-bank-statement.txt` PERSON: “Rodrigo T. Gálvez” (1)
- `text/heldout-020-bank-statement.txt` ADDRESS: “915 Robert Inlet Apt. 097” (1)
- `text/heldout-020-bank-statement.txt` PERSON: “Gálvez” (1)
- `text/heldout-020-bank-statement.txt` PHONE: “1-800-123-4567” (1)
- `text/heldout-021-bill-of-lading.txt` COMPANY: “Blue Ocean Shipping Lines” (1)
- `text/heldout-021-bill-of-lading.txt` ADDRESS: “42 Rue de la République,
       75011 Paris,
       France” (1)
- `text/heldout-021-bill-of-lading.txt` COMPANY: “ABC Enterprises” (1)
- `text/heldout-022-bill-of-lading.txt` PERSON: “Zara Backer” (1)
- `text/heldout-022-bill-of-lading.txt` ADDRESS: “41 Pasadizo Sancho Valbuena, Alicante” (1)
- `text/heldout-022-bill-of-lading.txt` ONLINE_ID: “637b:41bb:6db0:82a4:9d3a:7460:a4c6:6fa8” (1)
- `text/heldout-023-bill-of-lading.txt` COMPANY: “European Luxury Interiors B.V.” (1)
- `text/heldout-023-bill-of-lading.txt` ADDRESS: “12, Keizersgracht, 1015 CG Amsterdam, Netherlands” (1)
- `text/heldout-024-bill-of-lading.txt` ADDRESS: “123 Main Street, Toronto, Ontario, M5J 1E3, Canada” (1)
- `text/heldout-024-bill-of-lading.txt` PERSON: “John Doe” (2)
- `text/heldout-024-bill-of-lading.txt` PHONE: “+1 123 456 7890” (1)
- `text/heldout-024-bill-of-lading.txt` COMPANY: “ABC Enterprises” (1)
- `text/heldout-024-bill-of-lading.txt` PERSON: “Jane Smith” (1)
- `text/heldout-024-bill-of-lading.txt` PHONE: “+44 20 123 4567” (1)
- `text/heldout-025-bill-of-lading.txt` PERSON: “Margaud O. Weber” (3)
- `text/heldout-025-bill-of-lading.txt` ADDRESS: “841 Rios Estate, Suite 427” (2)
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “S87852395” (1)
- `text/heldout-026-business-plan.txt` PERSON: “Callahan” (6)
- `text/heldout-026-business-plan.txt` ADDRESS: “79730 Justin Plaza, Jessicaberg” (1)
- `text/heldout-026-business-plan.txt` PERSON: “Simone Pratesi” (2)
- `text/heldout-027-business-plan.txt` PERSON: “Tore P. Linder” (1)
- `text/heldout-027-business-plan.txt` COMPANY: “Linder Innovations” (9)
- `text/heldout-028-business-plan.txt` PERSON: “Daniele Cesarotti-Ruggeri” (2)
- `text/heldout-028-business-plan.txt` COMPANY: “Eco-Friendly Cleaning Services” (2)
- `text/heldout-029-business-plan.txt` COMPANY: “Laurent's Community Connect” (2)
- `text/heldout-029-business-plan.txt` PERSON: “Camille Marthe Laurent” (1)
- `text/heldout-029-business-plan.txt` COMPANY: “Laurent Enterprises” (5)
- `text/heldout-030-business-plan.txt` PERSON: “Elodia Rey” (2)
- `text/heldout-030-business-plan.txt` COMPANY: “TAOC” (7)
- `text/heldout-030-business-plan.txt` ADDRESS: “50785 Contreras Lodge Suite 653” (1)
- `text/heldout-030-business-plan.txt` PERSON: “Anglada-Bárcena” (1)
- `text/heldout-031-compliance-certificate.txt` COMPANY: “THE QUEEN'S UNIVERSITY OF BELFAST” (1)
- `text/heldout-031-compliance-certificate.txt` PERSON: “Eberhard L. Knappe” (1)
- `text/heldout-031-compliance-certificate.txt` ADDRESS: “60 Hövelgasse” (1)
- `text/heldout-031-compliance-certificate.txt` ID_NUMBER: “853” (1)
- `text/heldout-031-compliance-certificate.txt` PERSON: “Dr. Patrick Johnston” (1)
- `text/heldout-031-compliance-certificate.txt` PERSON: “Knappe” (1)
- `text/heldout-032-compliance-certificate.txt` PERSON: “[Howard-Lopez]” (3)
- `text/heldout-032-compliance-certificate.txt` COMPANY: “Howard-Lopez” (6)
- `text/heldout-032-compliance-certificate.txt` PERSON: “[Kenneth Pearson-Hart]” (1)
- `text/heldout-032-compliance-certificate.txt` PERSON: “Kenneth Pearson-Hart” (1)
- `text/heldout-032-compliance-certificate.txt` ADDRESS: “394 Martha Ramp” (1)
- `text/heldout-032-compliance-certificate.txt` PERSON: “Kenneth-Pearson-Hart” (1)
- `text/heldout-033-compliance-certificate.txt` PERSON: “Milagros Ureña” (1)
- `text/heldout-033-compliance-certificate.txt` ID_NUMBER: “K8181639” (1)
- `text/heldout-033-compliance-certificate.txt` ADDRESS: “447 James Freeway” (1)
- `text/heldout-033-compliance-certificate.txt` PERSON: “Milagros” (2)
- `text/heldout-034-compliance-certificate.txt` COMPANY: “Sjödin & Andersson HB” (6)
- `text/heldout-034-compliance-certificate.txt` PERSON: “Claude C. Royer” (1)
- `text/heldout-034-compliance-certificate.txt` ADDRESS: “Johannesbaan 427” (1)
- `text/heldout-037-corporate-governance-guidelines.txt` PERSON: “Alex Peron-Vasseur” (1)
- `text/heldout-037-corporate-governance-guidelines.txt` ADDRESS: “21024 King Cove” (1)
- `text/heldout-037-corporate-governance-guidelines.txt` PERSON: “Macarena” (2)
- `text/heldout-039-corporate-governance-guidelines.txt` PERSON: “Derrick Scott” (1)
- `text/heldout-039-corporate-governance-guidelines.txt` PERSON: “Veronica F. Ortese” (1)
- `text/heldout-040-corporate-governance-guidelines.txt` PERSON: “Flor Miguela Barral” (2)
- `text/heldout-040-corporate-governance-guidelines.txt` ADDRESS: “51340 Allison Point, South Eric” (1)
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “NFMUGBGD530” (2)
- `text/heldout-042-corporate-tax-return.txt` PERSON: “Deanna Adams” (2)
- `text/heldout-042-corporate-tax-return.txt` ADDRESS: “4156 Kelly Mission, Apt. 0394” (1)
- `text/heldout-042-corporate-tax-return.txt` PERSON: “Ida” (1)
- `text/heldout-043-corporate-tax-return.txt` ID_NUMBER: “P2891850” (1)
- `text/heldout-043-corporate-tax-return.txt` PERSON: “Joep Huurdeman-Wutke” (2)
- `text/heldout-043-corporate-tax-return.txt` PERSON: “John Doe” (1)
- `text/heldout-043-corporate-tax-return.txt` PERSON: “Jane Smith” (1)
- `text/heldout-044-corporate-tax-return.txt` PERSON: “Jacques Maurice Regnier” (1)
- `text/heldout-044-corporate-tax-return.txt` ADDRESS: “154” (1)
- `text/heldout-045-corporate-tax-return.txt` PERSON: “Alfred L. Ribeiro” (1)
- `text/heldout-045-corporate-tax-return.txt` ADDRESS: “17877 Krueger Branch, Vargasview” (1)
- `text/heldout-045-corporate-tax-return.txt` PHONE: “263.528.8045 x1838” (1)
- `text/heldout-045-corporate-tax-return.txt` ID_NUMBER: “12-3456789” (2)
- `text/heldout-045-corporate-tax-return.txt` ID_NUMBER: “987654321” (2)
- `text/heldout-046-credit-application.txt` PERSON: “Evi Coret-Coredo” (3)
- `text/heldout-046-credit-application.txt` ADDRESS: “2 avenue Faure” (1)
- `text/heldout-047-credit-application.txt` PERSON: “Ann T. Davis” (3)
- `text/heldout-047-credit-application.txt` ADDRESS: “63895 Cole Circles, 22289, Port Lauren” (1)
- `text/heldout-048-credit-application.txt` PERSON: “Adam Kenneth Törnqvist” (3)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “C25-9308-090-52” (1)
- `text/heldout-048-credit-application.txt` PERSON: “Törnqvist” (1)
- `text/heldout-048-credit-application.txt` PERSON: “John Doe” (1)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “11-22-33” (1)
- `text/heldout-049-credit-application.txt` PERSON: “Peter Irene Larsson” (2)
- `text/heldout-049-credit-application.txt` ADDRESS: “Milousteeg 7” (2)
- `text/heldout-049-credit-application.txt` COMPANY: “Bank of Scotland” (1)
- `text/heldout-049-credit-application.txt` COMPANY: “Capital One” (1)
- `text/heldout-049-credit-application.txt` SECRET: “th@*C0rp%t0tr^” (1)
- `text/heldout-050-credit-application.txt` PERSON: “Angelica Tamburini-Renzi” (2)
- `text/heldout-050-credit-application.txt` DATE_OF_BIRTH: “25th November 1990” (1)
- `text/heldout-050-credit-application.txt` ADDRESS: “203 Industrigatan, 19100, Umeå” (1)
- `text/heldout-051-credit-card-application.txt` PERSON: “Cynthia Hanson-Brown” (1)
- `text/heldout-051-credit-card-application.txt` ADDRESS: “50304 Petersen Village, Apt. 143” (1)
- `text/heldout-051-credit-card-application.txt` SECRET: “^mGJ*7Rd7n)” (1)
- `text/heldout-052-credit-card-application.txt` COMPANY: “RockStar Musician Card” (2)
- `text/heldout-053-credit-card-application.txt` PERSON: “Anne-Marie Dimitri Fiebig” (1)
- `text/heldout-053-credit-card-application.txt` ADDRESS: “35 rue Benoit” (1)
- `text/heldout-053-credit-card-application.txt` ID_NUMBER: “014” (1)
- `text/heldout-054-credit-card-application.txt` PERSON: “Fernanda Arcos” (1)
- `text/heldout-054-credit-card-application.txt` PERSON: “Montenegro-Jara” (1)
- `text/heldout-054-credit-card-application.txt` ADDRESS: “096 Römerring” (1)
- `text/heldout-055-credit-card-application.txt` PERSON: “Tina Patel” (1)
- `text/heldout-055-credit-card-application.txt` ADDRESS: “Danielring 1” (1)
- `text/heldout-056-credit-card-statement.txt` PERSON: “Julien W. Michel” (1)
- `text/heldout-056-credit-card-statement.txt` ADDRESS: “899 Hardy Knoll” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Uber Eats” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Amazon” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Netflix” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Apple” (1)
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “921” (1)
- `text/heldout-056-credit-card-statement.txt` PHONE: “1-800-123-4567” (1)
- `text/heldout-057-credit-card-statement.txt` PERSON: “Valerio Guijarro-Conesa” (1)
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Your Bank” (1)
- `text/heldout-058-credit-card-statement.txt` PERSON: “Marko Pärtzelt” (1)
- `text/heldout-058-credit-card-statement.txt` ADDRESS: “870 Joshua Corner, 42199, Port Timothytown” (1)
- `text/heldout-058-credit-card-statement.txt` ONLINE_ID: “18fd:ca22:8edb:4” (1)
- `text/heldout-059-credit-card-statement.txt` PERSON: “Gastone B. Veneziano” (2)
- `text/heldout-059-credit-card-statement.txt` ADDRESS: “4099 Evan Coves
Lake Jamieville, [Postal Code]” (1)
- `text/heldout-059-credit-card-statement.txt` PHONE: “1-800-123-4567” (1)
- `text/heldout-060-credit-card-statement.txt` PERSON: “John Doe” (1)
- `text/heldout-062-cryptocurrency-transaction-report.txt` PERSON: “Karin Sonja Andersson” (4)
- `text/heldout-062-cryptocurrency-transaction-report.txt` ADDRESS: “6 Graham lane” (3)
- `text/heldout-062-cryptocurrency-transaction-report.txt` ADDRESS: “6 Graham” (4)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “IT75C287907959005626172” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “C406-8056-806” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` PERSON: “Orhan Seidel” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ADDRESS: “4 Marie-Therese-Löchel-Allee, Weißenfels” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN4” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN5” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN6” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN7” (1)
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” (7)
- `text/heldout-066-csv.txt` PERSON: “Albert Roberts-McLean” (1)
- `text/heldout-066-csv.txt` ADDRESS: “15 Rambla Miguela Carreras” (1)
- `text/heldout-068-csv.txt` PERSON: “John” (1)
- `text/heldout-068-csv.txt` PERSON: “Jane” (1)
- `text/heldout-068-csv.txt` PERSON: “Bob” (1)
- `text/heldout-068-csv.txt` PERSON: “Charlie” (1)
- `text/heldout-068-csv.txt` ADDRESS: “123 Main St” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1985-05-15” (1)
- `text/heldout-068-csv.txt` PHONE: “555-555-5556” (1)
- `text/heldout-068-csv.txt` ADDRESS: “456 Elm St” (1)
- `text/heldout-068-csv.txt` PHONE: “555-555-5557” (1)
- `text/heldout-068-csv.txt` ADDRESS: “789 Oak St” (1)
- `text/heldout-068-csv.txt` PHONE: “555-555-5558” (1)
- `text/heldout-068-csv.txt` ADDRESS: “321 Maple St” (1)
- `text/heldout-068-csv.txt` PHONE: “555-555-5559” (1)
- `text/heldout-068-csv.txt` ADDRESS: “654 Pine St” (1)
- `text/heldout-068-csv.txt` PHONE: “555-555-5560” (1)
- `text/heldout-068-csv.txt` ADDRESS: “234 Cedar St” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1990-03-03” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1988-11-11” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1992-07-07” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1989-12-12” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1991-09-09” (1)
- `text/heldout-069-csv.txt` PERSON: “John” (1)
- `text/heldout-069-csv.txt` PERSON: “Jane” (1)
- `text/heldout-069-csv.txt` PERSON: “Bob” (1)
- `text/heldout-069-csv.txt` PERSON: “Charlie” (1)
- `text/heldout-069-csv.txt` PERSON: “David” (1)
- `text/heldout-069-csv.txt` PERSON: “Doe” (1)
- `text/heldout-069-csv.txt` PHONE: “123-456-7890” (1)
- `text/heldout-069-csv.txt` PERSON: “Smith” (1)
- `text/heldout-069-csv.txt` PHONE: “234-567-8901” (1)
- `text/heldout-069-csv.txt` PERSON: “Alice” (1)
- `text/heldout-069-csv.txt` PERSON: “Johnson” (1)
- `text/heldout-069-csv.txt` PHONE: “345-678-9012” (1)
- `text/heldout-069-csv.txt` PERSON: “Brown” (1)
- `text/heldout-069-csv.txt` PHONE: “456-789-0123” (1)
- `text/heldout-069-csv.txt` PERSON: “Davis” (1)
- `text/heldout-069-csv.txt` PHONE: “567-890-1234” (1)
- `text/heldout-069-csv.txt` PERSON: “Miller” (1)
- `text/heldout-069-csv.txt` PHONE: “678-901-2345” (1)
- `text/heldout-069-csv.txt` PERSON: “Eve” (1)
- `text/heldout-069-csv.txt` PERSON: “Williams” (1)
- `text/heldout-069-csv.txt` PHONE: “789-012-3456” (1)
- `text/heldout-069-csv.txt` PERSON: “Frank” (1)
- `text/heldout-069-csv.txt` PERSON: “Thomas” (1)
- `text/heldout-069-csv.txt` PHONE: “890-123-4567” (1)
- `text/heldout-069-csv.txt` PERSON: “Grace” (1)
- `text/heldout-069-csv.txt` PERSON: “Anderson” (1)
- `text/heldout-069-csv.txt` PHONE: “901-234-5678” (1)
- `text/heldout-069-csv.txt` PERSON: “Heidi” (1)
- `text/heldout-069-csv.txt` PERSON: “Taylor” (1)
- `text/heldout-070-csv.txt` PERSON: “Isabella Fredo Storladi” (3)
- `text/heldout-070-csv.txt` ADDRESS: “7 Markboulevard, Apt. 782” (3)
- `text/heldout-071-currency-exchange-rate-sheet.txt` PERSON: “Victoire A. Pichon” (1)
- `text/heldout-071-currency-exchange-rate-sheet.txt` ADDRESS: “43418 Tony Divide Apt. 734” (1)
- `text/heldout-071-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “1994-01-31” (4)
- `text/heldout-072-currency-exchange-rate-sheet.txt` ID_NUMBER: “EMP672995” (1)
- `text/heldout-072-currency-exchange-rate-sheet.txt` PERSON: “Suzanne Leclercq” (1)
- `text/heldout-072-currency-exchange-rate-sheet.txt` ADDRESS: “591 Barrera Orchard” (1)
- `text/heldout-073-currency-exchange-rate-sheet.txt` PERSON: “Isabelle Dumas-Brun” (6)
- `text/heldout-073-currency-exchange-rate-sheet.txt` ADDRESS: “558 Mary Glens” (6)
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “63623 Phillips Walk” (1)
- `text/heldout-074-currency-exchange-rate-sheet.txt` PERSON: “Harry Wilson-Hewitt” (2)
- `text/heldout-075-currency-exchange-rate-sheet.txt` PERSON: “Jennifer Isaías Cañas” (1)
- `text/heldout-075-currency-exchange-rate-sheet.txt` ADDRESS: “348 Industrigränd, Apt. 1” (1)
- `text/heldout-076-customer-agreement.txt` PERSON: “Georgine Vogt” (1)
- `text/heldout-076-customer-agreement.txt` ADDRESS: “15169 Michelle Via” (1)
- `text/heldout-076-customer-agreement.txt` ADDRESS: “789 King Street East, Toronto, Ontario, Canada” (1)
- `text/heldout-077-customer-agreement.txt` ADDRESS: “123 Main Street, Toronto, Ontario, Canada, M1A 2Z3” (1)
- `text/heldout-077-customer-agreement.txt` PERSON: “John Doe” (1)
- `text/heldout-077-customer-agreement.txt` ADDRESS: “456 Elm Street, Vancouver, BC, Canada, V6G 8H9” (1)
- `text/heldout-077-customer-agreement.txt` ADDRESS: “789 Oak Street, Toronto, Ontario, Canada, M3J 9K8” (1)
- `text/heldout-078-customer-agreement.txt` ADDRESS: “123 High Street, London, United Kingdom” (1)
- `text/heldout-078-customer-agreement.txt` ADDRESS: “456 Park Lane, Manchester, United Kingdom” (1)
- `text/heldout-078-customer-agreement.txt` ADDRESS: “789 Oxford Street, London, United Kingdom” (1)
- `text/heldout-079-customer-agreement.txt` PERSON: “Chantal” (1)
- `text/heldout-079-customer-agreement.txt` ADDRESS: “Abbas-Beckmann-Gasse 1” (1)
- `text/heldout-079-customer-agreement.txt` PERSON: “Harriet C. Wilson” (1)
- `text/heldout-080-customer-agreement.txt` PERSON: “Luigina Pizzo-Orlando” (1)
- `text/heldout-080-customer-agreement.txt` ADDRESS: “658 rue Clémence Pereira” (1)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “John Doe” (2)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “Ilaria” (5)
- `text/heldout-081-customer-support-conversational-log.txt` ADDRESS: “467 Martinez Summit Apt. 871” (1)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “Anne B. Goncalves” (1)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “John” (4)
- `text/heldout-083-customer-support-conversational-log.txt` PERSON: “Edoardo Cheda” (5)
- `text/heldout-083-customer-support-conversational-log.txt` ADDRESS: “62625 Harris Stream, Port Pamela” (1)
- `text/heldout-083-customer-support-conversational-log.txt` PERSON: “Edoardo” (3)
- `text/heldout-084-customer-support-conversational-log.txt` PERSON: “Meinolf Tatjana Wiek” (2)
- `text/heldout-084-customer-support-conversational-log.txt` PERSON: “Meinolf” (1)
- `text/heldout-084-customer-support-conversational-log.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-085-customer-support-conversational-log.txt` PERSON: “Meral Gieß-Beer” (1)
- `text/heldout-085-customer-support-conversational-log.txt` ADDRESS: “3778 Steven Passage, Thomasview” (2)
- `text/heldout-085-customer-support-conversational-log.txt` ID_NUMBER: “SMDD83928111582486” (1)
- `text/heldout-085-customer-support-conversational-log.txt` PERSON: “Meral” (3)
- `text/heldout-086-dispute-resolution-policy.txt` PERSON: “Marthe Luc Duval” (1)
- `text/heldout-087-dispute-resolution-policy.txt` PERSON: “Paulino Cuenca-Contreras” (1)
- `text/heldout-087-dispute-resolution-policy.txt` ADDRESS: “77263 Cathy Landing” (1)
- `text/heldout-090-dispute-resolution-policy.txt` ID_NUMBER: “XJRM73197596964372” (1)
- `text/heldout-090-dispute-resolution-policy.txt` PERSON: “Jacques G. Adam” (1)
- `text/heldout-090-dispute-resolution-policy.txt` ADDRESS: “7 Tombaan, Apt. 6” (1)
- `text/heldout-090-dispute-resolution-policy.txt` ID_NUMBER: “980-30-0925” (1)
- `text/heldout-091-edi.txt` PERSON: “Shipper Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Postal Code” (1)
- `text/heldout-091-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Postal Code” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Postal Code” (1)
- `text/heldout-092-edi.txt` ADDRESS: “347 Beth Stream” (2)
- `text/heldout-092-edi.txt` PERSON: “Julien Clément” (1)
- `text/heldout-092-edi.txt` ID_NUMBER: “MXAL72620669578837” (2)
- `text/heldout-092-edi.txt` ADDRESS: “Los Angeles*CA*90001” (1)
- `text/heldout-092-edi.txt` PHONE: “1234567890” (1)
- `text/heldout-093-edi.txt` ADDRESS: “123 Main St.” (1)
- `text/heldout-094-edi.txt` PERSON: “Leon Sean Morley” (2)
- `text/heldout-094-edi.txt` ADDRESS: “7309 Elizabeth Bridge, Apt. 310” (2)
- `text/heldout-094-edi.txt` ID_NUMBER: “16-045597-08” (2)
- `text/heldout-094-edi.txt` ID_NUMBER: “W68435475” (1)
- `text/heldout-094-edi.txt` ID_NUMBER: “W6843547” (1)
- `text/heldout-095-edi.txt` PERSON: “Neil L. Talbot” (2)
- `text/heldout-095-edi.txt` ADDRESS: “8666 Boyer Mount” (2)
- `text/heldout-095-edi.txt` ID_NUMBER: “385768114” (2)
- `text/heldout-095-edi.txt` ID_NUMBER: “246879” (2)
- `text/heldout-095-edi.txt` ADDRESS: “456 Park Lane” (1)
- `text/heldout-095-edi.txt` ADDRESS: “789 Elm Street” (1)
- `text/heldout-095-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-096-email.txt` COMPANY: “at TimeMaster

P” (1)
- `text/heldout-097-email.txt` PERSON: “Jane” (7)
- `text/heldout-097-email.txt` PERSON: “Amelia Hartley” (1)
- `text/heldout-098-email.txt` PERSON: “Irene Arvidsson” (1)
- `text/heldout-098-email.txt` PERSON: “Dr. John Smith” (1)
- `text/heldout-098-email.txt` COMPANY: “XYZ Healthcare” (1)
- `text/heldout-098-email.txt` COMPANY: “ABC Tech” (1)
- `text/heldout-098-email.txt` PERSON: “Smith” (1)
- `text/heldout-098-email.txt` SECRET: “AIzaZkjcPJM9Ht” (1)
- `text/heldout-099-email.txt` PERSON: “Dr. Sarah Smith” (1)
- `text/heldout-100-email.txt` PERSON: “Viviane S. Löwer” (1)
- `text/heldout-100-email.txt` ADDRESS: “9243 Daniel Curve” (1)
- `text/heldout-101-employment-contract.txt` COMPANY: “JOHANSSON & HEDBERG AB” (1)
- `text/heldout-101-employment-contract.txt` ADDRESS: “8 Renshof” (2)
- `text/heldout-101-employment-contract.txt` COMPANY: “Johansson & Hedberg AB” (2)
- `text/heldout-101-employment-contract.txt` PERSON: “Hans Dieter E. Möchlichen” (2)
- `text/heldout-102-employment-contract.txt` ADDRESS: “500 Main Street, Anytown, USA” (1)
- `text/heldout-102-employment-contract.txt` PERSON: “Marta Perales-Riera” (1)
- `text/heldout-102-employment-contract.txt` ADDRESS: “015 Coleman Greens, 12024, Mccartyview” (1)
- `text/heldout-103-employment-contract.txt` PERSON: “John Doe” (1)
- `text/heldout-103-employment-contract.txt` ADDRESS: “123 Main Street, Anytown, USA” (1)
- `text/heldout-104-employment-contract.txt` PERSON: “Leanne Connor Brooks” (1)
- `text/heldout-104-employment-contract.txt` COMPANY: “DataGen, Inc.” (2)
- `text/heldout-105-employment-contract.txt` ADDRESS: “123 Main Street, Anytown, USA” (1)
- `text/heldout-105-employment-contract.txt` PERSON: “Danielle A. Mendès” (1)
- `text/heldout-105-employment-contract.txt` ADDRESS: “7730 Bethany Dam, Apt. 7119” (1)
- `text/heldout-106-financial-aid-application.txt` PERSON: “Christiane Besnard” (1)
- `text/heldout-106-financial-aid-application.txt` ADDRESS: “679 Christopher Underpass, West Erika” (1)
- `text/heldout-106-financial-aid-application.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-106-financial-aid-application.txt` DATE_OF_BIRTH: “01/01/1998” (1)
- `text/heldout-106-financial-aid-application.txt` ID_NUMBER: “***-**-1234” (1)
- `text/heldout-107-financial-aid-application.txt` PERSON: “Gregory Pope” (3)
- `text/heldout-107-financial-aid-application.txt` ID_NUMBER: “J-526012-P” (1)
- `text/heldout-107-financial-aid-application.txt` ADDRESS: “6092 Sutton Field, 29575, New Ericfurt” (1)
- `text/heldout-107-financial-aid-application.txt` ADDRESS: “6092 Sutton Field, New Ericfurt, postal code 29575” (1)
- `text/heldout-108-financial-aid-application.txt` PERSON: “Patrick J. Wheeler” (2)
- `text/heldout-108-financial-aid-application.txt` ADDRESS: “49338 Goodman Manors Apt. 601” (1)
- `text/heldout-108-financial-aid-application.txt` ID_NUMBER: “372” (1)
- `text/heldout-108-financial-aid-application.txt` PERSON: “Schmiedecke” (1)
- `text/heldout-109-financial-aid-application.txt` PERSON: “Alex Johnson” (2)
- `text/heldout-109-financial-aid-application.txt` DATE_OF_BIRTH: “01/01/2001” (1)
- `text/heldout-109-financial-aid-application.txt` ADDRESS: “123 Main Street, Anytown, USA, 12345” (1)
- `text/heldout-109-financial-aid-application.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-110-financial-aid-application.txt` PERSON: “Ettore S. Modugno” (2)
- `text/heldout-110-financial-aid-application.txt` ADDRESS: “47 Lauren Village, Sarahchester, [Postal Code]” (1)
- `text/heldout-111-financial-data-feed.txt` COMPANY: “LONDON_EXCHANGE” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4220” (1)
- `text/heldout-111-financial-data-feed.txt` PERSON: “Aroa P. Mate” (1)
- `text/heldout-111-financial-data-feed.txt` ID_NUMBER: “200468649” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4221” (1)
- `text/heldout-113-financial-data-feed.txt` PERSON: “Gionata M. Baroffio” (1)
- `text/heldout-113-financial-data-feed.txt` PHONE: “001-438-554-1968x78870” (1)
- `text/heldout-114-financial-data-feed.txt` COMPANY: “Rémy” (1)
- `text/heldout-114-financial-data-feed.txt` PERSON: “Teresa E. Fraser” (1)
- `text/heldout-115-financial-data-feed.txt` COMPANY: “London Stock Exchange” (1)
- `text/heldout-116-financial-disclosure-statement.txt` PERSON: “Ingeborg Berglund-Karlsson” (2)
- `text/heldout-116-financial-disclosure-statement.txt` ADDRESS: “677 Nyvägen, 89225, Falun” (1)
- `text/heldout-117-financial-disclosure-statement.txt` ADDRESS: “123 Main Street
Anytown, USA” (1)
- `text/heldout-117-financial-disclosure-statement.txt` COMPANY: “Greenpeace” (1)
- `text/heldout-117-financial-disclosure-statement.txt` COMPANY: “Sierra Club” (1)
- `text/heldout-118-financial-disclosure-statement.txt` PERSON: “Jane Doe” (1)
- `text/heldout-118-financial-disclosure-statement.txt` PERSON: “John Smith” (1)
- `text/heldout-119-financial-disclosure-statement.txt` PERSON: “Marcella M. Tirabassi” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ADDRESS: “75262 Joseph Skyway, 78028, West James” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1FfmbHfn4B2w3r3n429E49453212c272” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “0x742d35C443742d35333333333333333333333333” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “0x98765432109876543210987654321098” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “MW99999999999999999999999999999999” (1)
- `text/heldout-122-financial-forecast.txt` PERSON: “June K. Robertson” (2)
- `text/heldout-122-financial-forecast.txt` ADDRESS: “290 Timothy Route” (1)
- `text/heldout-122-financial-forecast.txt` PERSON: “Robertson” (3)
- `text/heldout-125-financial-forecast.txt` PERSON: “Hugh L. Graham” (1)
- `text/heldout-125-financial-forecast.txt` ADDRESS: “418 Tina Park” (1)
- `text/heldout-127-financial-regulatory-compliance-report.txt` PERSON: “Richard Martineau-Grenier” (2)
- `text/heldout-127-financial-regulatory-compliance-report.txt` ADDRESS: “53471 Matthew Branch” (1)
- `text/heldout-127-financial-regulatory-compliance-report.txt` PERSON: “Martineau-Grenier” (1)
- `text/heldout-128-financial-regulatory-compliance-report.txt` PERSON: “Livia F. d' Heripon” (1)
- `text/heldout-130-financial-regulatory-compliance-report.txt` ADDRESS: “13988 Jason Plains” (1)
- `text/heldout-130-financial-regulatory-compliance-report.txt` PERSON: “Vanessa Castillo” (1)
- `text/heldout-131-financial-risk-assessment.txt` PERSON: “Victoria Faye de Roos” (3)
- `text/heldout-131-financial-risk-assessment.txt` ONLINE_ID: “afc9:182a:95cc:bad9:21e9:18b1:a6b2:56ac” (1)
- `text/heldout-131-financial-risk-assessment.txt` ADDRESS: “Idrottsstigen” (1)
- `text/heldout-132-financial-risk-assessment.txt` PERSON: “Boman” (10)
- `text/heldout-132-financial-risk-assessment.txt` PERSON: “Martin Gustafsson” (1)
- `text/heldout-132-financial-risk-assessment.txt` ADDRESS: “2 Asptorget, Linköping” (1)
- `text/heldout-133-financial-risk-assessment.txt` PERSON: “Geza Weimer” (6)
- `text/heldout-133-financial-risk-assessment.txt` ID_NUMBER: “EMP769291” (2)
- `text/heldout-133-financial-risk-assessment.txt` ADDRESS: “828 Michael Route, 94381, Zamorabury” (1)
- `text/heldout-134-financial-risk-assessment.txt` COMPANY: “Procacci-Bondumier Enterprises” (7)
- `text/heldout-134-financial-risk-assessment.txt` ADDRESS: “39165 Burns Lights, North Victoria” (1)
- `text/heldout-135-financial-risk-assessment.txt` ADDRESS: “73 rue David Menard, BoucherBourg” (1)
- `text/heldout-135-financial-risk-assessment.txt` PERSON: “Vicenta D. Martin” (1)
- `text/heldout-135-financial-risk-assessment.txt` ID_NUMBER: “059332” (1)
- `text/heldout-136-financial-statement.txt` PERSON: “Heinz, Belinda & Oswald” (1)
- `text/heldout-136-financial-statement.txt` ADDRESS: “962 Pollard Stravenue Apt. 479” (1)
- `text/heldout-137-financial-statement.txt` COMPANY: “International Trade Company” (1)
- `text/heldout-138-financial-statement.txt` PERSON: “Zoe H. Lucas” (1)
- `text/heldout-138-financial-statement.txt` PHONE: “[+44] 20 7122 5555” (1)
- `text/heldout-139-financial-statement.txt` ADDRESS: “1435 Payne Streets” (1)
- `text/heldout-140-financial-statement.txt` PERSON: “Fiona Lauren Barker” (3)
- `text/heldout-140-financial-statement.txt` ID_NUMBER: “X5083723” (3)
- `text/heldout-140-financial-statement.txt` PERSON: “Fiona” (2)
- `text/heldout-142-fix-protocol.txt` PERSON: “Nadia Wessel Luitgardis van Neustrië” (1)
- `text/heldout-142-fix-protocol.txt` ADDRESS: “1 Hussain lake” (1)
- `text/heldout-142-fix-protocol.txt` PHONE: “+11234567890” (1)
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “Test_Username” (1)
- `text/heldout-142-fix-protocol.txt` SECRET: “Test_Password” (1)
- `text/heldout-143-fix-protocol.txt` PERSON: “NILSSON & JOHANSSON HB” (1)
- `text/heldout-143-fix-protocol.txt` PERSON: “MARTIN CLARK” (1)
- `text/heldout-143-fix-protocol.txt` ADDRESS: “2 Rotonda Cuda” (1)
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “24034022099218196691” (1)
- `text/heldout-144-fix-protocol.txt` PERSON: “Carina Johansson” (1)
- `text/heldout-144-fix-protocol.txt` ADDRESS: “118 Pasaje Clímaco Ribes” (1)
- `text/heldout-145-fix-protocol.txt` PERSON: “Pauline Martin-Thibault” (1)
- `text/heldout-145-fix-protocol.txt` ADDRESS: “Thomas Heights Apt.” (1)
- `text/heldout-146-fpml.txt` ADDRESS: “123 Main St, New York, NY 10001, USA” (1)
- `text/heldout-147-fpml.txt` ID_NUMBER: “ZKQI65019016425115” (1)
- `text/heldout-147-fpml.txt` PERSON: “Ryan D. Clayton” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456-20220901” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456” (2)
- `text/heldout-149-fpml.txt` PERSON: “María Fernanda Porras-Vera” (1)
- `text/heldout-150-fpml.txt` PERSON: “Iker T. Milla” (1)
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “John Doe” (1)
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “01/01/1980” (1)
- `text/heldout-151-health-insurance-claim-form.txt` ADDRESS: “123 Maple Street, Anytown, USA” (1)
- `text/heldout-151-health-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-151-health-insurance-claim-form.txt` PHONE: “(987) 654-3210” (1)
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “Dr. Jane Smith” (1)
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “123456789” (1)
- `text/heldout-151-health-insurance-claim-form.txt` COMPANY: “ABC Health Insurance” (1)
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “MD-123456” (1)
- `text/heldout-152-health-insurance-claim-form.txt` PERSON: “John Doe” (3)
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “01/01/1980” (1)
- `text/heldout-152-health-insurance-claim-form.txt` ADDRESS: “123 Maple Street, Anytown, CA 12345” (1)
- `text/heldout-152-health-insurance-claim-form.txt` PHONE: “(123) 456-7890” (2)
- `text/heldout-152-health-insurance-claim-form.txt` COMPANY: “Anytown Medical Center” (1)
- `text/heldout-152-health-insurance-claim-form.txt` ADDRESS: “456 Oak Street, Anytown, CA 12345” (1)
- `text/heldout-153-health-insurance-claim-form.txt` PERSON: “Timothy Marshall” (2)
- `text/heldout-153-health-insurance-claim-form.txt` ID_NUMBER: “346-93-3732” (1)
- `text/heldout-153-health-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-153-health-insurance-claim-form.txt` ADDRESS: “65210 Ellison Motorway, Lake Sarabury” (1)
- `text/heldout-153-health-insurance-claim-form.txt` COMPANY: “Serene Minds Counseling” (1)
- `text/heldout-153-health-insurance-claim-form.txt` ADDRESS: “23 Maple Street, Downtown, ST 12345” (1)
- `text/heldout-153-health-insurance-claim-form.txt` PHONE: “(987) 654-3210” (1)
- `text/heldout-153-health-insurance-claim-form.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-154-health-insurance-claim-form.txt` PERSON: “Camilla G. Karz” (1)
- `text/heldout-154-health-insurance-claim-form.txt` ADDRESS: “4454 Tammy Camp Apt. 805” (1)
- `text/heldout-154-health-insurance-claim-form.txt` ONLINE_ID: “e368:16fa:6c71:3dcf:fa90:d00b:7fa:dab0” (1)
- `text/heldout-154-health-insurance-claim-form.txt` COMPANY: “AlphaMed Laboratories” (1)
- `text/heldout-154-health-insurance-claim-form.txt` ADDRESS: “3567 Sunshine St., 15.916498 N, -59.876838 E” (1)
- `text/heldout-154-health-insurance-claim-form.txt` ADDRESS: “33133” (1)
- `text/heldout-155-health-insurance-claim-form.txt` PERSON: “Fabrizio” (1)
- `text/heldout-155-health-insurance-claim-form.txt` PERSON: “Beth Jackson” (1)
- `text/heldout-155-health-insurance-claim-form.txt` ADDRESS: “6881 Jordan Drive, East Trevorfurt” (1)
- `text/heldout-155-health-insurance-claim-form.txt` PERSON: “Dr. Samuel Johnson, MD” (1)
- `text/heldout-155-health-insurance-claim-form.txt` ADDRESS: “6769 Main Street
Trevorfurt, AB T1Y 2S8
Canada” (1)
- `text/heldout-156-insurance-claim-form.txt` PERSON: “Denis Besson” (1)
- `text/heldout-156-insurance-claim-form.txt` DATE_OF_BIRTH: “16/07/1918” (1)
- `text/heldout-156-insurance-claim-form.txt` ADDRESS: “125 Miller Road, Apt. 175” (1)
- `text/heldout-157-insurance-claim-form.txt` PERSON: “John Doe” (2)
- `text/heldout-157-insurance-claim-form.txt` ADDRESS: “123 Maple Street, Anytown, USA” (1)
- `text/heldout-157-insurance-claim-form.txt` DATE_OF_BIRTH: “01/01/1980” (1)
- `text/heldout-157-insurance-claim-form.txt` PERSON: “Dr. Jane Smith” (1)
- `text/heldout-157-insurance-claim-form.txt` ADDRESS: “456 Oak Avenue, Anytown, USA” (1)
- `text/heldout-157-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-157-insurance-claim-form.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-158-insurance-claim-form.txt` PERSON: “Editha K. Beckmann” (3)
- `text/heldout-158-insurance-claim-form.txt` DATE_OF_BIRTH: “03/14/1968” (1)
- `text/heldout-158-insurance-claim-form.txt` ID_NUMBER: “750-38-3379” (1)
- `text/heldout-158-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-158-insurance-claim-form.txt` ADDRESS: “4952 Wendy Flats Apt. 069” (2)
- `text/heldout-158-insurance-claim-form.txt` COMPANY: “XYZ Engineering Services” (1)
- `text/heldout-158-insurance-claim-form.txt` COMPANY: “ABC Construction Services” (1)
- `text/heldout-159-insurance-claim-form.txt` PERSON: “John Doe” (3)
- `text/heldout-159-insurance-claim-form.txt` ADDRESS: “123 Main Street, Anytown, USA” (1)
- `text/heldout-159-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-159-insurance-claim-form.txt` PERSON: “Jane Smith, Esq.” (1)
- `text/heldout-159-insurance-claim-form.txt` ADDRESS: “456 Elm Street, Anytown, USA” (1)
- `text/heldout-159-insurance-claim-form.txt` PHONE: “(987) 654-3210” (1)
- `text/heldout-159-insurance-claim-form.txt` ID_NUMBER: “L-123456789” (1)
- `text/heldout-159-insurance-claim-form.txt` ADDRESS: “PO Box 1234
Anytown, USA
12345-67” (1)
- `text/heldout-160-insurance-claim-form.txt` PERSON: “John Doe” (2)
- `text/heldout-160-insurance-claim-form.txt` ADDRESS: “123 Maple Street, Anytown, USA” (1)
- `text/heldout-160-insurance-claim-form.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “ABC Travel Agency” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “XYZ Airlines” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “Luxury Hotels London” (1)
- `text/heldout-160-insurance-claim-form.txt` ID_NUMBER: “TP-1234567” (1)
- `text/heldout-161-insurance-policy.txt` COMPANY: “National Flood Insurance Program” (2)
- `text/heldout-161-insurance-policy.txt` PERSON: “Virginie” (1)
- `text/heldout-161-insurance-policy.txt` PERSON: “Flavia Elvira Zacco” (1)
- `text/heldout-161-insurance-policy.txt` ADDRESS: “349 Scott Plaza Suite 058” (1)
- `text/heldout-162-insurance-policy.txt` COMPANY: “SureSafe Marine Insurance Company” (1)
- `text/heldout-162-insurance-policy.txt` ADDRESS: “1234 Fifth Avenue, Wilmington, DE 19801” (1)
- `text/heldout-162-insurance-policy.txt` PERSON: “Jörn Lucia Eimer” (1)
- `text/heldout-162-insurance-policy.txt` ADDRESS: “034 Cervantes Locks, Apt. 409” (1)
- `text/heldout-163-insurance-policy.txt` COMPANY: “SureGuard Insurance Company” (1)
- `text/heldout-163-insurance-policy.txt` PERSON: “Nilo Barroso” (1)
- `text/heldout-163-insurance-policy.txt` ADDRESS: “26572 Kyle Gardens, 69581, Benjaminland” (2)
- `text/heldout-164-insurance-policy.txt` ADDRESS: “783 Carr Parkway, 02121, New Erin” (1)
- `text/heldout-164-insurance-policy.txt` PERSON: “Bertrand Lefèvre-Blanchard” (1)
- `text/heldout-165-insurance-policy.txt` COMPANY: “THE PACIFIC COAST INSURANCE COMPANY” (1)
- `text/heldout-165-insurance-policy.txt` COMPANY: “The Pacific Coast Insurance Company” (1)
- `text/heldout-165-insurance-policy.txt` ADDRESS: “225 Main Street, San Francisco, CA 94105” (1)
- `text/heldout-165-insurance-policy.txt` PERSON: “Leah Decker” (1)
- `text/heldout-165-insurance-policy.txt` COMPANY: “Decker's Designs” (1)
- `text/heldout-165-insurance-policy.txt` ADDRESS: “17272 Nicholas Islands Apt. 681” (1)
- `text/heldout-165-insurance-policy.txt` ID_NUMBER: “2023-BP-12854” (1)
- `text/heldout-166-investment-prospectus.txt` ADDRESS: “5986 Williams Meadow, Apt. 956” (1)
- `text/heldout-167-investment-prospectus.txt` COMPANY: “Technology and Innovation Fund” (6)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “Horizon Healthcare Innovation Fund” (3)
- `text/heldout-168-investment-prospectus.txt` PERSON: “Paulette C. Marchand” (1)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “MedHealth Innovations” (1)
- `text/heldout-168-investment-prospectus.txt` ADDRESS: “13813 Sims Village Suite 507” (1)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “Biosynth Labs” (1)
- `text/heldout-169-investment-prospectus.txt` PERSON: “Tilmann Martine Loos” (1)
- `text/heldout-170-investment-prospectus.txt` COMPANY: “The Teton Valley Technology Startup Fund” (2)
- `text/heldout-170-investment-prospectus.txt` PHONE: “123-456-7890” (1)
- `text/heldout-170-investment-prospectus.txt` ADDRESS: “6789, 123 Fake Street
   Anytown, US” (1)
- `text/heldout-172-isda-definition.txt` COMPANY: “London Court of International Arbitration” (2)
- `text/heldout-172-isda-definition.txt` PERSON: “Alicia Salomón Vizcaíno” (1)
- `text/heldout-172-isda-definition.txt` ADDRESS: “340 Cañada de Viviana Sureda, Piso 3” (1)
- `text/heldout-173-isda-definition.txt` PERSON: “Ronaldo Giacomo Casadei” (1)
- `text/heldout-173-isda-definition.txt` ADDRESS: “15598 Laura Corner, Apt. 1592” (1)
- `text/heldout-173-isda-definition.txt` PERSON: “Casadei” (1)
- `text/heldout-173-isda-definition.txt` PERSON: “---

Ronaldo Giacomo Cas” (1)
- `text/heldout-174-isda-definition.txt` ID_NUMBER: “E88-7198-265-04” (1)
- `text/heldout-176-it-support-ticket.txt` PERSON: “Véronique P. Mallet” (1)
- `text/heldout-176-it-support-ticket.txt` PERSON: “Mirta” (4)
- `text/heldout-176-it-support-ticket.txt` ADDRESS: “81402 Jennifer Extension” (1)
- `text/heldout-177-it-support-ticket.txt` COMPANY: “IT Support Team” (1)
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft Support” (1)
- `text/heldout-180-it-support-ticket.txt` PERSON: “Pepe Martín Amores” (2)
- `text/heldout-180-it-support-ticket.txt` PHONE: “+1-555-123-4567” (1)
- `text/heldout-180-it-support-ticket.txt` ONLINE_ID: “4225:4483:c1e7:5988:b611:114e:dfa:7cfd” (1)
- `text/heldout-181-loan-agreement.txt` ADDRESS: “7080 Jamie Place Apt. 290” (1)
- `text/heldout-182-loan-agreement.txt` ADDRESS: “123 Main Street, Toronto, Ontario, Canada” (1)
- `text/heldout-182-loan-agreement.txt` ADDRESS: “456 Elm Street, Toronto, Ontario, Canada” (1)
- `text/heldout-183-loan-agreement.txt` PERSON: “Katie Lisa Lord” (1)
- `text/heldout-183-loan-agreement.txt` ADDRESS: “88658 Lawrence Parks, 88319, North Jasminefurt” (1)
- `text/heldout-183-loan-agreement.txt` COMPANY: “ABC Financial Services” (1)
- `text/heldout-183-loan-agreement.txt` ADDRESS: “12345 Oak Street, Wilmington, DE 19801” (1)
- `text/heldout-183-loan-agreement.txt` ID_NUMBER: “JHMES3H62LA025372” (1)
- `text/heldout-184-loan-agreement.txt` PERSON: “Katherine Elizabeth Gonzalez” (1)
- `text/heldout-184-loan-agreement.txt` ADDRESS: “48584 White Landing” (1)
- `text/heldout-184-loan-agreement.txt` ADDRESS: “123 Main Street, Anytown, USA” (1)
- `text/heldout-185-loan-agreement.txt` PERSON: “Andreas” (1)
- `text/heldout-185-loan-agreement.txt` ADDRESS: “34 Keith light, Flat 95T” (2)
- `text/heldout-185-loan-agreement.txt` PERSON: “Sharon Whitehead” (1)
- `text/heldout-186-loan-application.txt` PERSON: “Thomas Daniel-Gros” (1)
- `text/heldout-186-loan-application.txt` ADDRESS: “690 avenue de Foucher, Brun” (1)
- `text/heldout-186-loan-application.txt` COMPANY: “Greenfield Enterprises” (2)
- `text/heldout-186-loan-application.txt` PERSON: “Jane Doe” (1)
- `text/heldout-186-loan-application.txt` PERSON: “John Smith” (1)
- `text/heldout-186-loan-application.txt` COMPANY: “National Debtline” (1)
- `text/heldout-187-loan-application.txt` PERSON: “John Michael Doe” (1)
- `text/heldout-187-loan-application.txt` DATE_OF_BIRTH: “01/10/1985” (1)
- `text/heldout-187-loan-application.txt` ADDRESS: “123 Maple Street, Toronto, ON, Canada” (1)
- `text/heldout-187-loan-application.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-188-loan-application.txt` PERSON: “Johnathan David Smith” (3)
- `text/heldout-188-loan-application.txt` DATE_OF_BIRTH: “01/02/1980” (1)
- `text/heldout-189-loan-application.txt` PERSON: “John Doe” (1)
- `text/heldout-189-loan-application.txt` ADDRESS: “123 Maple Street, Anytown, USA” (1)
- `text/heldout-189-loan-application.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-189-loan-application.txt` DATE_OF_BIRTH: “01/01/1980” (1)
- `text/heldout-189-loan-application.txt` ID_NUMBER: “123-45-6789” (1)
- `text/heldout-189-loan-application.txt` COMPANY: “Federal Trade Commission (FTC)” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia Community Initiative” (1)
- `text/heldout-190-loan-application.txt` PERSON: “Kimberly K. Mendoza” (1)
- `text/heldout-190-loan-application.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-190-loan-application.txt` ADDRESS: “4896 Edward Junction
Lake Cynthia, 53799” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia Community Center” (3)
- `text/heldout-190-loan-application.txt` ADDRESS: “4896 Edward Junction, Lake Cynthia, 53799” (1)
- `text/heldout-191-mortgage-amortization-schedule.txt` ID_NUMBER: “KALL90112673253892” (1)
- `text/heldout-191-mortgage-amortization-schedule.txt` PERSON: “Richard Davis-Collins” (1)
- `text/heldout-191-mortgage-amortization-schedule.txt` ADDRESS: “37147 Henry Meadow, Apt. 8984” (1)
- `text/heldout-194-mortgage-amortization-schedule.txt` PERSON: “Domitila N. Jordá” (1)
- `text/heldout-194-mortgage-amortization-schedule.txt` ADDRESS: “1940 Hendricks Heights” (1)
- `text/heldout-196-mortgage-contract.txt` PERSON: “Yves Monique Guibert” (1)
- `text/heldout-196-mortgage-contract.txt` ADDRESS: “226 Brittany Drive, 91541, Wilsonbury” (2)
- `text/heldout-196-mortgage-contract.txt` COMPANY: “DFMVDEWF223” (1)
- `text/heldout-197-mortgage-contract.txt` PERSON: “Elvira B. Jönsson” (1)
- `text/heldout-197-mortgage-contract.txt` ADDRESS: “395 Hans-J.-Hering-Platz, 82195, Tirschenreuth” (2)
- `text/heldout-198-mortgage-contract.txt` ADDRESS: “123 Main Street, Toronto, Ontario” (1)
- `text/heldout-198-mortgage-contract.txt` PERSON: “Julia Martinez-Mcdaniel” (1)
- `text/heldout-198-mortgage-contract.txt` ADDRESS: “975 Wilson Island Suite 122” (1)
- `text/heldout-199-mortgage-contract.txt` COMPANY: “Clark, Moses and Dixon” (1)
- `text/heldout-199-mortgage-contract.txt` ADDRESS: “123 Main Street, Anytown, DE 12345” (1)
- `text/heldout-199-mortgage-contract.txt` PERSON: “Erwin Trüb-Henck” (1)
- `text/heldout-199-mortgage-contract.txt` ADDRESS: “249 Haley Wall” (1)
- `text/heldout-201-mt940.txt` PERSON: “LAURENCE T. MORENO” (1)
- `text/heldout-201-mt940.txt` PHONE: “(702)986-0107x4484” (2)
- `text/heldout-201-mt940.txt` ADDRESS: “51501 PAUL ESTATES, APT. 08925” (2)
- `text/heldout-201-mt940.txt` ID_NUMBER: “12345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1312345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1412345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1512345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1612345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1712345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “18123456789” (1)
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” (1)
- `text/heldout-202-mt940.txt` ID_NUMBER: “191234567890” (17)
- `text/heldout-203-mt940.txt` ID_NUMBER: “2234567890123456” (1)
- `text/heldout-203-mt940.txt` ID_NUMBER: “1234567890” (12)
- `text/heldout-203-mt940.txt` PERSON: “JBROWN” (5)
- `text/heldout-203-mt940.txt` ID_NUMBER: “4567890123” (4)
- `text/heldout-203-mt940.txt` PERSON: “JBRO” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “C214-1412-519” (2)
- `text/heldout-204-mt940.txt` PERSON: “MARK SIMPSON-TAYLOR” (2)
- `text/heldout-204-mt940.txt` ADDRESS: “603 CAROLYN CIRCLE, 38410, EAST LORETTAMOUTH” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “1234567890123456” (1)
- `text/heldout-204-mt940.txt` PERSON: “MARK SIMP” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “4567845674567456” (1)
- `text/heldout-205-mt940.txt` ID_NUMBER: “5243611ABC” (1)
- `text/heldout-205-mt940.txt` ID_NUMBER: “1234567890” (18)
- `text/heldout-206-payment-confirmation.txt` PERSON: “Joaquín Ureña-Vilalta” (2)
- `text/heldout-206-payment-confirmation.txt` COMPANY: “ABC Online Store” (1)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “338” (1)
- `text/heldout-206-payment-confirmation.txt` ADDRESS: “056 Martinez Island” (1)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “7fde39a0-1d2a-4e03-8f4a-d33ae2bf8f5c” (1)
- `text/heldout-207-payment-confirmation.txt` COMPANY: “Venmo” (3)
- `text/heldout-207-payment-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-207-payment-confirmation.txt` PERSON: “Jane Smith” (2)
- `text/heldout-208-payment-confirmation.txt` PERSON: “John Doe” (2)
- `text/heldout-208-payment-confirmation.txt` COMPANY: “XYZ Company” (3)
- `text/heldout-208-payment-confirmation.txt` ADDRESS: “456 Oak Street, Anytown, USA” (1)
- `text/heldout-208-payment-confirmation.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-209-payment-confirmation.txt` PERSON: “Zoe” (1)
- `text/heldout-209-payment-confirmation.txt` PERSON: “Madeleine M. Legros” (1)
- `text/heldout-209-payment-confirmation.txt` ADDRESS: “3379 Hayes Junction, Apt. 807” (1)
- `text/heldout-210-payment-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Seanville Public Sector” (4)
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Seanville Public Sector Employees' Retirement Plan” (2)
- `text/heldout-211-pension-plan-agreement.txt` PERSON: “Astrid Labbé” (1)
- `text/heldout-211-pension-plan-agreement.txt` ID_NUMBER: “Jp-58502” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Honoré” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Noël-Blanchard” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Jolanda C. Branciforte” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Hon” (1)
- `text/heldout-213-pension-plan-agreement.txt` PERSON: “AMANDO VALENCIA” (1)
- `text/heldout-213-pension-plan-agreement.txt` PERSON: “Amando Valencia” (1)
- `text/heldout-213-pension-plan-agreement.txt` ADDRESS: “92453 Dunn Lights” (1)
- `text/heldout-213-pension-plan-agreement.txt` ID_NUMBER: “Ua12597-R” (1)
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “City of Springfield” (2)
- `text/heldout-216-policyholder-s-report.txt` ID_NUMBER: “LJID19999357259049” (1)
- `text/heldout-216-policyholder-s-report.txt` PERSON: “Gavin A. Kerr” (1)
- `text/heldout-216-policyholder-s-report.txt` ADDRESS: “24422 Edwards Divide, Suite” (1)
- `text/heldout-216-policyholder-s-report.txt` PHONE: “1-800-392-6754” (1)
- `text/heldout-216-policyholder-s-report.txt` COMPANY: “EXAMPLE INSURANCE” (2)
- `text/heldout-216-policyholder-s-report.txt` PHONE: “1-800-EXAMPLE-INS” (1)
- `text/heldout-217-policyholder-s-report.txt` PERSON: “Darrell Douglas Harris” (2)
- `text/heldout-217-policyholder-s-report.txt` SECRET: “610fD9cE9AaD346D29F3876ba3b889eb” (1)
- `text/heldout-217-policyholder-s-report.txt` ADDRESS: “9879 Jamie Mountain, 80450, Lake Stanley” (1)
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “Richards and Sons” (4)
- `text/heldout-218-policyholder-s-report.txt` PERSON: “Pardo-Redondo” (1)
- `text/heldout-218-policyholder-s-report.txt` ADDRESS: “49830 Martin Oval, Randolphfort” (1)
- `text/heldout-218-policyholder-s-report.txt` PERSON: “Ann H. Lawrence” (1)
- `text/heldout-219-policyholder-s-report.txt` PERSON: “Adélaïde C. Georges” (4)
- `text/heldout-219-policyholder-s-report.txt` ADDRESS: “69 Nyvägen” (3)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “987654321” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “456123789” (1)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Premier Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Elite Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Supreme Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` SECRET: “de42714ebBf36ea6Ce53” (1)
- `text/heldout-220-policyholder-s-report.txt` COMPANY: “Britannia Insurance” (2)
- `text/heldout-221-privacy-policy.txt` PERSON: “Nigel Turner” (1)
- `text/heldout-221-privacy-policy.txt` PERSON: “Castañeda-Armas” (1)
- `text/heldout-221-privacy-policy.txt` ADDRESS: “Noëllelaan 0” (1)
- `text/heldout-222-privacy-policy.txt` PERSON: “Heidemarie Niemeier” (1)
- `text/heldout-222-privacy-policy.txt` ADDRESS: “6023 Lori Motorway” (1)
- `text/heldout-223-privacy-policy.txt` COMPANY: “GeoTrack” (4)
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[contact information]” (3)
- `text/heldout-228-product-disclosure-statement.txt` PERSON: “Lee Sharpe” (1)
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “TechFrontier Investments” (1)
- `text/heldout-229-product-disclosure-statement.txt` ADDRESS: “7019 Jonathan Points” (1)
- `text/heldout-229-product-disclosure-statement.txt` PERSON: “Augustin Renault-Maillard” (1)
- `text/heldout-230-product-disclosure-statement.txt` COMPANY: “Global AgriFutures Fund” (3)
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Student Housing” (1)
- `text/heldout-232-real-estate-loan-agreement.txt` ADDRESS: “73396 Tucker Orchard Apt. 627” (1)
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ University” (3)
- `text/heldout-233-real-estate-loan-agreement.txt` PERSON: “Melina A. Toso” (1)
- `text/heldout-233-real-estate-loan-agreement.txt` ADDRESS: “65778 Matthew Bypass, Apt. 139” (1)
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Alpha Finance, Inc.” (1)
- `text/heldout-233-real-estate-loan-agreement.txt` ADDRESS: “12345 Maple Leaf Lane, Anytown, USA” (1)
- `text/heldout-234-real-estate-loan-agreement.txt` PERSON: “Gioele Aulenti” (1)
- `text/heldout-234-real-estate-loan-agreement.txt` ADDRESS: “474 Justin Dale, Apt. 98126” (1)
- `text/heldout-234-real-estate-loan-agreement.txt` ADDRESS: “1234 Bank Street, Ottawa, Ontario K1P 5K9” (1)
- `text/heldout-235-real-estate-loan-agreement.txt` PERSON: “Cassandra Susanna Saragat” (1)
- `text/heldout-235-real-estate-loan-agreement.txt` ADDRESS: “20 Erna-Rohleder-Allee, Mayen” (1)
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Conduct Authority (FCA)” (1)
- `text/heldout-237-regulatory-compliance-guide.txt` PERSON: “Thierry Bonnet” (3)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Ofcom” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Canadian Radio-television and Telecommunications Commission (CRTC)” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Advertising Standards Authority (ASA)” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` PERSON: “Isabell Tröst-Trupp” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` PERSON: “Isabell” (1)
- `text/heldout-241-regulatory-filing.txt` PERSON: “Claudia Santino Sonnino” (1)
- `text/heldout-241-regulatory-filing.txt` ADDRESS: “51682 Porter Ways, Apt. 367” (2)
- `text/heldout-241-regulatory-filing.txt` SECRET: “Q39BFou+_)6quP” (1)
- `text/heldout-241-regulatory-filing.txt` PERSON: “Claudia” (2)
- `text/heldout-241-regulatory-filing.txt` DATE_OF_BIRTH: “December 24, 1977” (1)
- `text/heldout-242-regulatory-filing.txt` COMPANY: “ABC & Co.” (1)
- `text/heldout-244-regulatory-filing.txt` PERSON: “Alex T. Pinto” (1)
- `text/heldout-244-regulatory-filing.txt` ID_NUMBER: “CID-878308” (1)
- `text/heldout-244-regulatory-filing.txt` ADDRESS: “507 Joseph Shores” (1)
- `text/heldout-244-regulatory-filing.txt` DATE_OF_BIRTH: “1923-07-04” (1)
- `text/heldout-244-regulatory-filing.txt` PERSON: “Pinto” (2)
- `text/heldout-246-renewal-reminder.txt` PERSON: “Rosa Bruno Duodo” (1)
- `text/heldout-246-renewal-reminder.txt` ADDRESS: “639 Mary Field Suite 059” (1)
- `text/heldout-246-renewal-reminder.txt` ONLINE_ID: “f466:e5ca:ba7b:12ce:8df0:4592:b6dc:f100” (1)
- `text/heldout-247-renewal-reminder.txt` PERSON: “Michelle Corinne Masse” (2)
- `text/heldout-247-renewal-reminder.txt` ID_NUMBER: “790506124” (1)
- `text/heldout-247-renewal-reminder.txt` ADDRESS: “5 Pasaje de Azahar Navarrete, Piso 0” (2)
- `text/heldout-247-renewal-reminder.txt` PHONE: “+XXX-XXX-XXXX” (2)
- `text/heldout-249-renewal-reminder.txt` PERSON: “Féline Mila Hagendoorn” (1)
- `text/heldout-249-renewal-reminder.txt` ID_NUMBER: “408141094” (1)
- `text/heldout-249-renewal-reminder.txt` DATE_OF_BIRTH: “18/02/1914” (1)
- `text/heldout-249-renewal-reminder.txt` ADDRESS: “817 Contrada Gabriele, Piano 4” (1)
- `text/heldout-250-renewal-reminder.txt` PERSON: “Édouard E. Robin” (1)
- `text/heldout-250-renewal-reminder.txt` COMPANY: “Hermanos Insurance” (3)
- `text/heldout-250-renewal-reminder.txt` ADDRESS: “54 Alameda de Apolonia Pons, Piso 6” (1)
- `text/heldout-252-safety-data-sheet.txt` ADDRESS: “33500 Caitlyn Fort, Apt. 2” (1)
- `text/heldout-253-safety-data-sheet.txt` COMPANY: “Logan-Nguyen” (2)
- `text/heldout-254-safety-data-sheet.txt` PERSON: “Claus-Peter Ulla Söding” (1)
- `text/heldout-254-safety-data-sheet.txt` ADDRESS: “59, boulevard Alain Guilbert” (1)
- `text/heldout-255-safety-data-sheet.txt` PERSON: “Stefanie S. Staude” (1)
- `text/heldout-255-safety-data-sheet.txt` ADDRESS: “chemin Germain” (1)
- `text/heldout-256-securities-prospectus.txt` PERSON: “Costanzo R. Lollobrigida” (1)
- `text/heldout-256-securities-prospectus.txt` ADDRESS: “428 Stephanie Crossing, Apt. 33831” (1)
- `text/heldout-262-shareholder-agreement.txt` PERSON: “Ugolino Govoni-Corradi” (1)
- `text/heldout-262-shareholder-agreement.txt` ADDRESS: “3409 David Roads, Suite 436” (1)
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “XMYTGBDH508” (1)
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Dean K. Gibson” (1)
- `text/heldout-264-shareholder-agreement.txt` ADDRESS: “680 Judith Place, 58120, Alvarezfort” (1)
- `text/heldout-264-shareholder-agreement.txt` PHONE: “+1-880-934-2286” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ADDRESS: “45 High Street, London, UK” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Amanda Ortmann-Röhrdanz” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ID_NUMBER: “489290094” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ADDRESS: “1 Sam ferry, Studio 73H” (1)
- `text/heldout-268-supply-chain-management-agreement.txt` PERSON: “Zoe” (11)
- `text/heldout-268-supply-chain-management-agreement.txt` ADDRESS: “3032 Travis Junctions” (1)
- `text/heldout-268-supply-chain-management-agreement.txt` PERSON: “Jennifer Coca” (1)
- `text/heldout-268-supply-chain-management-agreement.txt` ADDRESS: “45 King's Cross, London, UK” (1)
- `text/heldout-270-supply-chain-management-agreement.txt` PERSON: “Anthony Robinson-Hunt” (2)
- `text/heldout-270-supply-chain-management-agreement.txt` ADDRESS: “32934 Mariah Stravenue, Apt. 196” (1)
- `text/heldout-271-swift-message.txt` PERSON: “GRAY PAUL AND THOMPSON” (2)
- `text/heldout-271-swift-message.txt` ADDRESS: “49166 Campos Circle, Apt. 226” (1)
- `text/heldout-271-swift-message.txt` PERSON: “SABINA ÁVILA-BAYO” (2)
- `text/heldout-271-swift-message.txt` ID_NUMBER: “522932ABC12” (1)
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CHASUS3325330728” (1)
- `text/heldout-272-swift-message.txt` PERSON: “SMITH, JOHN DOE” (1)
- `text/heldout-272-swift-message.txt` COMPANY: “ABC BANK NEW YORK” (3)
- `text/heldout-272-swift-message.txt` ID_NUMBER: “CDRCUS66XXX1234567890ABCDEFGHIJK” (1)
- `text/heldout-272-swift-message.txt` ID_NUMBER: “OOFFXXX0010123456ABCDEFGHIJ” (1)
- `text/heldout-273-swift-message.txt` COMPANY: “WILSON AND SONS” (2)
- `text/heldout-273-swift-message.txt` PERSON: “FRANCO” (1)
- `text/heldout-273-swift-message.txt` PERSON: “LUDOVICA TALIANI” (1)
- `text/heldout-273-swift-message.txt` PERSON: “Franco” (1)
- `text/heldout-273-swift-message.txt` COMPANY: “Wilson and Sons” (1)
- `text/heldout-273-swift-message.txt` PERSON: “Ludovica Taliani” (1)
- `text/heldout-273-swift-message.txt` ADDRESS: “065 Elizabeth Plains, Apt. 84659” (1)
- `text/heldout-273-swift-message.txt` ADDRESS: “065 ELIZABETH PLAINS, APT. 84659/CITY/CA/91010-1234/US” (1)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “O1234567890” (2)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “1234567890” (2)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “C1479015350371735835064” (1)
- `text/heldout-274-swift-message.txt` PERSON: “Cilli Zdravko Schmiedt” (1)
- `text/heldout-274-swift-message.txt` ADDRESS: “1/6 Putzweg, 43295, Füssen” (1)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “GB22ABBY09090909090909” (1)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “1234567890” (16)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “3261217076” (1)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC LIMITED” (1)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC CORPORATE” (1)
- `text/heldout-275-swift-message.txt` ID_NUMBER: “GB29WFIB1234567890” (1)
- `text/heldout-276-tax-assessment-notice.txt` ID_NUMBER: “5476-6d60-7b3” (1)
- `text/heldout-277-tax-assessment-notice.txt` ADDRESS: “123 Maple Street, Toronto, ON, M5J 1K7” (1)
- `text/heldout-277-tax-assessment-notice.txt` PERSON: “John Doe” (1)
- `text/heldout-277-tax-assessment-notice.txt` ID_NUMBER: “456789012” (1)
- `text/heldout-278-tax-assessment-notice.txt` PERSON: “Fem J. Otte” (1)
- `text/heldout-278-tax-assessment-notice.txt` ADDRESS: “6 chemin de Jacques, Apt. 63” (1)
- `text/heldout-278-tax-assessment-notice.txt` ID_NUMBER: “213-76554-56” (1)
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-280-tax-assessment-notice.txt` PERSON: “Gianpaolo Massimiliano Marcacci” (1)
- `text/heldout-280-tax-assessment-notice.txt` PHONE: “+34823 742 846” (1)
- `text/heldout-280-tax-assessment-notice.txt` ADDRESS: “Jesperboulevard 1” (1)
- `text/heldout-280-tax-assessment-notice.txt` COMPANY: “HMRC” (3)
- `text/heldout-280-tax-assessment-notice.txt` ID_NUMBER: “2023-001234-UK” (2)
- `text/heldout-281-tax-return.txt` PERSON: “John A. Smith” (1)
- `text/heldout-281-tax-return.txt` ADDRESS: “123 Retirement Lane
   Retirement City, USA 12345” (1)
- `text/heldout-281-tax-return.txt` ID_NUMBER: “123-45-6789” (1)
- `text/heldout-282-tax-return.txt` PERSON: “Richard Mellor” (1)
- `text/heldout-282-tax-return.txt` ADDRESS: “62657 Devon Loaf Suite 831” (1)
- `text/heldout-282-tax-return.txt` ID_NUMBER: “12-3456789” (1)
- `text/heldout-283-tax-return.txt` PERSON: “Gelsomina Argento” (1)
- `text/heldout-283-tax-return.txt` ADDRESS: “65 Strada Antonini, Ruvo Di Puglia” (1)
- `text/heldout-284-tax-return.txt` PERSON: “Letizia P. Staglieno” (1)
- `text/heldout-284-tax-return.txt` ID_NUMBER: “S45347676” (1)
- `text/heldout-284-tax-return.txt` ADDRESS: “544 Morton Lake, Kaisershire” (1)
- `text/heldout-284-tax-return.txt` ADDRESS: “745 Maplewood Lane, Harmonyville” (1)
- `text/heldout-285-tax-return.txt` PERSON: “Jennifer Gomez-Martin” (5)
- `text/heldout-285-tax-return.txt` ADDRESS: “70578 Manning Grove” (1)
- `text/heldout-286-trade-confirmation.txt` PERSON: “Adelmo Munari” (1)
- `text/heldout-286-trade-confirmation.txt` ADDRESS: “78833 Lee Inlet, Andrewshire, 35302” (2)
- `text/heldout-287-trade-confirmation.txt` PERSON: “Klaus-Ulrich” (1)
- `text/heldout-287-trade-confirmation.txt` PERSON: “Auguste Kambs” (1)
- `text/heldout-287-trade-confirmation.txt` ADDRESS: “08 Norahboulevard, Apt. 74” (1)
- `text/heldout-287-trade-confirmation.txt` ID_NUMBER: “EMP406537” (1)
- `text/heldout-288-trade-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-288-trade-confirmation.txt` DATE_OF_BIRTH: “12/09/1980” (1)
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “12345678A” (1)
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “GB00B1234567” (1)
- `text/heldout-289-trade-confirmation.txt` PERSON: “Isabel Kambs” (1)
- `text/heldout-289-trade-confirmation.txt` ADDRESS: “8882 Danielle Plains, Apt. 19929” (1)
- `text/heldout-289-trade-confirmation.txt` ID_NUMBER: “P28546430” (1)
- `text/heldout-290-trade-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-290-trade-confirmation.txt` PHONE: “(123) 456-7890” (1)
- `text/heldout-290-trade-confirmation.txt` PERSON: “Jane Smith” (1)
- `text/heldout-290-trade-confirmation.txt` PHONE: “(987) 654-3210” (1)
- `text/heldout-290-trade-confirmation.txt` ADDRESS: “123 Main St., Kansas City, MO 64116” (1)
- `text/heldout-290-trade-confirmation.txt` ADDRESS: “456 Oak St., Kansas City, MO 64116” (1)
- `text/heldout-290-trade-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-290-trade-confirmation.txt` ID_NUMBER: “987654321” (1)
- `text/heldout-290-trade-confirmation.txt` ADDRESS: “Grain Elevator #3, 1234 Wheat Fields Lane, Kansas City, MO 64116” (1)
- `text/heldout-291-transaction-confirmation.txt` PERSON: “John Doe” (2)
- `text/heldout-291-transaction-confirmation.txt` COMPANY: “XYZ Insurance” (4)
- `text/heldout-291-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-292-transaction-confirmation.txt` PERSON: “Eugène Maillot” (2)
- `text/heldout-292-transaction-confirmation.txt` ADDRESS: “971 Kimberly Spurs, West Ronnieport” (1)
- `text/heldout-292-transaction-confirmation.txt` ID_NUMBER: “444” (1)
- `text/heldout-293-transaction-confirmation.txt` PERSON: “Pío P. Segura” (1)
- `text/heldout-293-transaction-confirmation.txt` ADDRESS: “574 Allison Land, Apt. 154” (1)
- `text/heldout-293-transaction-confirmation.txt` PERSON: “Dr. Jane Smith” (1)
- `text/heldout-294-transaction-confirmation.txt` PERSON: “John Smith” (1)
- `text/heldout-294-transaction-confirmation.txt` COMPANY: “ABC Insurance Company” (4)
- `text/heldout-294-transaction-confirmation.txt` PERSON: “Smith” (1)
- `text/heldout-294-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-294-transaction-confirmation.txt` EMAIL: “info@abcv insurance.com” (2)
- `text/heldout-295-transaction-confirmation.txt` PERSON: “Severiano O. Porcel” (2)
- `text/heldout-295-transaction-confirmation.txt` ADDRESS: “48 Vicolo Segrè, Moio Alcantara” (1)
- `text/heldout-295-transaction-confirmation.txt` PERSON: “van der Spaendonck-Uphaus” (1)
- `text/heldout-295-transaction-confirmation.txt` ONLINE_ID: “23b8:3adf:8e87:51e6:9314:3ed1:1544:4e64” (1)
- `text/heldout-297-xbrl.txt` COMPANY: “ABC_Corporation” (2)
- `text/heldout-297-xbrl.txt` COMPANY: “ABC_Corporation_20X-03-31” (1)
- `text/heldout-297-xbrl.txt` PERSON: “John Doe” (1)
- `text/heldout-298-xbrl.txt` PERSON: “Liliana Proietti-Trapani” (1)
- `text/heldout-298-xbrl.txt` SECRET: “gz2&D_aj++k5_i@L” (1)
- `text/heldout-298-xbrl.txt` PERSON: “Liliana” (4)
- `text/heldout-298-xbrl.txt` PERSON: “Proietti-Trapani” (4)

**Over-redacted**

- `text/heldout-007-audit-report.txt` ADDRESS: “EC1A 7BA” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “Customs
Corporation” is not on the answer key
- `text/heldout-079-customer-agreement.txt` ADDRESS: “E1 4NS” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council” is not on the answer key
- `text/heldout-255-safety-data-sheet.txt` ADDRESS: “NW1 2LT” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “0123456789” is not on the answer key
- `text/heldout-280-tax-assessment-notice.txt` ADDRESS: “NW1 3XY” is not on the answer key
- `text/heldout-280-tax-assessment-notice.txt` ADDRESS: “BX9 1AS” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` ADDRESS: “NW1 2LB” is not on the answer key

### GLiNER only

**Missed**

- `text/heldout-003-annual-report.txt` EMAIL: “jenny58@ward.net” (1)
- `text/heldout-003-annual-report.txt` ID_NUMBER: “996238618” (1)
- `text/heldout-004-annual-report.txt` ID_NUMBER: “X-758472-D” (1)
- `text/heldout-004-annual-report.txt` COMPANY: “2 024 Inc.” (1)
- `text/heldout-005-annual-report.txt` COMPANY: “ABC Corporation” (3)
- `text/heldout-007-audit-report.txt` ID_NUMBER: “62997511170555282974” (1)
- `text/heldout-011-bai-format.txt` ID_NUMBER: “CUST64550460” (1)
- `text/heldout-011-bai-format.txt` ID_NUMBER: “BAI02100001” (1)
- `text/heldout-014-bai-format.txt` PERSON: “Rhys L. Jones” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-015-bai-format.txt` ID_NUMBER: “GB29 ABCD1234567890123456” (1)
- `text/heldout-015-bai-format.txt` ID_NUMBER: “GB98 EFGH1234567890123456” (1)
- `text/heldout-016-bank-statement.txt` ADDRESS: “4251 Nathan St, Patrickton” (1)
- `text/heldout-017-bank-statement.txt` PERSON: “John Doe” (1)
- `text/heldout-018-bank-statement.txt` PERSON: “John Doe” (1)
- `text/heldout-018-bank-statement.txt` COMPANY: “Doe Enterprises Inc.” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Édith” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Anthony Carpenter” (1)
- `text/heldout-019-bank-statement.txt` PERSON: “Anthony Carpeter” (1)
- `text/heldout-019-bank-statement.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-021-bill-of-lading.txt` COMPANY: “Blue Ocean Shipping Lines” (1)
- `text/heldout-021-bill-of-lading.txt` COMPANY: “UK Cargo Link Ltd.” (1)
- `text/heldout-021-bill-of-lading.txt` COMPANY: “XYZ Industries Ltd.” (1)
- `text/heldout-022-bill-of-lading.txt` ADDRESS: “41 Pasadizo Sancho Valbuena, Alicante” (1)
- `text/heldout-023-bill-of-lading.txt` COMPANY: “Maritime Express Lines Ltd.” (1)
- `text/heldout-023-bill-of-lading.txt` COMPANY: “Fine Furniture Importers Ltd.” (1)
- `text/heldout-023-bill-of-lading.txt` COMPANY: “European Luxury Interiors B.V.” (1)
- `text/heldout-024-bill-of-lading.txt` PERSON: “John Doe” (1)
- `text/heldout-024-bill-of-lading.txt` EMAIL: “john.doe@xyzcorp.com” (1)
- `text/heldout-024-bill-of-lading.txt` PERSON: “Jane Smith” (1)
- `text/heldout-024-bill-of-lading.txt` COMPANY: “Global Shipping Inc.” (1)
- `text/heldout-025-bill-of-lading.txt` PERSON: “Margaud O. Weber” (1)
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “S87852395” (1)
- `text/heldout-025-bill-of-lading.txt` ADDRESS: “Unit 7, Riverside Park
Southampton, SO15 3TG
UK” (1)
- `text/heldout-027-business-plan.txt` COMPANY: “Identify Potential Partners” (1)
- `text/heldout-028-business-plan.txt` COMPANY: “Eco-Friendly Cleaning Services” (2)
- `text/heldout-030-business-plan.txt` COMPANY: “TAOC” (2)
- `text/heldout-031-compliance-certificate.txt` ADDRESS: “60 Hövelgasse” (1)
- `text/heldout-031-compliance-certificate.txt` ID_NUMBER: “853” (1)
- `text/heldout-034-compliance-certificate.txt` ID_NUMBER: “DE39417936227405828401” (1)
- `text/heldout-042-corporate-tax-return.txt` PERSON: “Deanna Adams” (1)
- `text/heldout-043-corporate-tax-return.txt` ID_NUMBER: “P2891850” (1)
- `text/heldout-043-corporate-tax-return.txt` ID_NUMBER: “08-32-10” (1)
- `text/heldout-044-corporate-tax-return.txt` PERSON: “Jacques Maurice Regnier” (1)
- `text/heldout-045-corporate-tax-return.txt` PERSON: “Alfred L. Ribeiro” (1)
- `text/heldout-046-credit-application.txt` PERSON: “Evi Coret-Coredo” (1)
- `text/heldout-046-credit-application.txt` ID_NUMBER: “3498-2391-8851-275” (1)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “C25-9308-090-52” (1)
- `text/heldout-048-credit-application.txt` EMAIL: “[john.doe@email.com]” (1)
- `text/heldout-048-credit-application.txt` PERSON: “John Doe” (1)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “12345678” (1)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “11-22-33” (1)
- `text/heldout-049-credit-application.txt` ADDRESS: “Milousteeg 7” (1)
- `text/heldout-049-credit-application.txt` COMPANY: “Barclays Bank” (1)
- `text/heldout-049-credit-application.txt` COMPANY: “Capital One” (1)
- `text/heldout-053-credit-card-application.txt` ID_NUMBER: “014” (1)
- `text/heldout-056-credit-card-statement.txt` PERSON: “Julien W. Michel” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Uber Eats” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Netflix” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Apple” (1)
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Your Bank” (1)
- `text/heldout-058-credit-card-statement.txt` PERSON: “Marko Pärtzelt” (1)
- `text/heldout-060-credit-card-statement.txt` PERSON: “John Doe” (1)
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “1234-5678-9012-3456” (1)
- `text/heldout-066-csv.txt` PERSON: “Albert Roberts-McLean” (1)
- `text/heldout-068-csv.txt` EMAIL: “johndoe@gmail.com” (1)
- `text/heldout-068-csv.txt` PERSON: “Bob” (1)
- `text/heldout-068-csv.txt` EMAIL: “bjohnson@hotmail.com” (1)
- `text/heldout-068-csv.txt` ADDRESS: “123 Main St” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1985-05-15” (1)
- `text/heldout-068-csv.txt` ADDRESS: “789 Oak St” (1)
- `text/heldout-068-csv.txt` ADDRESS: “321 Maple St” (1)
- `text/heldout-068-csv.txt` ADDRESS: “654 Pine St” (1)
- `text/heldout-068-csv.txt` ADDRESS: “234 Cedar St” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1990-03-03” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1988-11-11” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1992-07-07” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1989-12-12” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1991-09-09” (1)
- `text/heldout-069-csv.txt` PERSON: “John” (1)
- `text/heldout-069-csv.txt` PERSON: “Jane” (1)
- `text/heldout-069-csv.txt` PERSON: “Bob” (1)
- `text/heldout-069-csv.txt` PERSON: “David” (1)
- `text/heldout-069-csv.txt` EMAIL: “charliedavis@email.com” (1)
- `text/heldout-069-csv.txt` EMAIL: “davidmiller@email.com” (1)
- `text/heldout-069-csv.txt` EMAIL: “evewilliams@email.com” (1)
- `text/heldout-069-csv.txt` PERSON: “Doe” (1)
- `text/heldout-069-csv.txt` PERSON: “Brown” (1)
- `text/heldout-069-csv.txt` PERSON: “Davis” (1)
- `text/heldout-069-csv.txt` PERSON: “Eve” (1)
- `text/heldout-069-csv.txt` PERSON: “Williams” (1)
- `text/heldout-069-csv.txt` PERSON: “Anderson” (1)
- `text/heldout-069-csv.txt` PERSON: “Taylor” (1)
- `text/heldout-070-csv.txt` ONLINE_ID: “176.169.229.38” (3)
- `text/heldout-071-currency-exchange-rate-sheet.txt` PERSON: “Victoire A. Pichon” (1)
- `text/heldout-071-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “1994-01-31” (4)
- `text/heldout-072-currency-exchange-rate-sheet.txt` ID_NUMBER: “EMP672995” (1)
- `text/heldout-072-currency-exchange-rate-sheet.txt` PERSON: “Suzanne Leclercq” (1)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “John Doe” (2)
- `text/heldout-081-customer-support-conversational-log.txt` PERSON: “John” (3)
- `text/heldout-084-customer-support-conversational-log.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-091-edi.txt` PERSON: “Shipper Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Postal Code” (1)
- `text/heldout-091-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Postal Code” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Postal Code” (1)
- `text/heldout-092-edi.txt` ADDRESS: “347 Beth Stream” (2)
- `text/heldout-092-edi.txt` PERSON: “Julien Clément” (1)
- `text/heldout-092-edi.txt` ADDRESS: “Los Angeles*CA*90001” (1)
- `text/heldout-093-edi.txt` ADDRESS: “123 Main St.” (1)
- `text/heldout-094-edi.txt` ID_NUMBER: “16-045597-08” (2)
- `text/heldout-094-edi.txt` COMPANY: “Acme Inc.” (1)
- `text/heldout-094-edi.txt` ID_NUMBER: “W6843547” (1)
- `text/heldout-095-edi.txt` ID_NUMBER: “385768114” (1)
- `text/heldout-095-edi.txt` ID_NUMBER: “246879” (1)
- `text/heldout-095-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-096-email.txt` COMPANY: “at TimeMaster

P” (1)
- `text/heldout-101-employment-contract.txt` ADDRESS: “8 Renshof” (1)
- `text/heldout-101-employment-contract.txt` ADDRESS: “NW1 2TP” (1)
- `text/heldout-106-financial-aid-application.txt` ID_NUMBER: “***-**-1234” (1)
- `text/heldout-107-financial-aid-application.txt` PERSON: “Gregory Pope” (1)
- `text/heldout-109-financial-aid-application.txt` COMPANY: “High School” (1)
- `text/heldout-111-financial-data-feed.txt` COMPANY: “LONDON_EXCHANGE” (1)
- `text/heldout-113-financial-data-feed.txt` PERSON: “Gionata M. Baroffio” (1)
- `text/heldout-114-financial-data-feed.txt` COMPANY: “Rémy” (1)
- `text/heldout-114-financial-data-feed.txt` PERSON: “Teresa E. Fraser” (1)
- `text/heldout-115-financial-data-feed.txt` COMPANY: “London Stock Exchange” (1)
- `text/heldout-119-financial-disclosure-statement.txt` COMPANY: “Cryptocurrency Holdings” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1FfmbHfn4B2w3r3n429E49453212c272” (1)
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “0x742d35C443742d35333333333333333333333333” (1)
- `text/heldout-120-financial-disclosure-statement.txt` COMPANY: “Pemberton Capital Partners LLP” (1)
- `text/heldout-130-financial-regulatory-compliance-report.txt` ADDRESS: “13988 Jason Plains” (1)
- `text/heldout-136-financial-statement.txt` ADDRESS: “962 Pollard Stravenue Apt. 479” (1)
- `text/heldout-138-financial-statement.txt` PERSON: “Zoe H. Lucas” (1)
- `text/heldout-138-financial-statement.txt` COMPANY: “Collins PLC” (1)
- `text/heldout-140-financial-statement.txt` ID_NUMBER: “X5083723” (1)
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “123.100.102.62” (1)
- `text/heldout-142-fix-protocol.txt` EMAIL: “test@example.com” (2)
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “Test_Username” (1)
- `text/heldout-142-fix-protocol.txt` SECRET: “Test_Password” (1)
- `text/heldout-144-fix-protocol.txt` PERSON: “Carina Johansson” (1)
- `text/heldout-145-fix-protocol.txt` PERSON: “Pauline Martin-Thibault” (1)
- `text/heldout-146-fpml.txt` COMPANY: “ABC Corporation” (1)
- `text/heldout-147-fpml.txt` PERSON: “Ryan D. Clayton” (1)
- `text/heldout-150-fpml.txt` PERSON: “Iker T. Milla” (1)
- `text/heldout-151-health-insurance-claim-form.txt` COMPANY: “Anytown General Hospital” (1)
- `text/heldout-151-health-insurance-claim-form.txt` COMPANY: “ABC Health Insurance” (1)
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “MD-123456” (1)
- `text/heldout-152-health-insurance-claim-form.txt` COMPANY: “Anytown Medical Center” (1)
- `text/heldout-152-health-insurance-claim-form.txt` PERSON: “Jane Smith” (1)
- `text/heldout-152-health-insurance-claim-form.txt` PHONE: “0012345678” (1)
- `text/heldout-153-health-insurance-claim-form.txt` PERSON: “Timothy Marshall” (2)
- `text/heldout-153-health-insurance-claim-form.txt` ID_NUMBER: “346-93-3732” (1)
- `text/heldout-154-health-insurance-claim-form.txt` PERSON: “Camilla G. Karz” (1)
- `text/heldout-154-health-insurance-claim-form.txt` COMPANY: “AlphaMed Laboratories” (1)
- `text/heldout-155-health-insurance-claim-form.txt` PERSON: “Fabrizio” (1)
- `text/heldout-155-health-insurance-claim-form.txt` PERSON: “Beth Jackson” (1)
- `text/heldout-157-insurance-claim-form.txt` PERSON: “Dr. Jane Smith” (1)
- `text/heldout-158-insurance-claim-form.txt` ID_NUMBER: “750-38-3379” (1)
- `text/heldout-158-insurance-claim-form.txt` EMAIL: “editha.beckmann@email.com” (1)
- `text/heldout-160-insurance-claim-form.txt` PERSON: “John Doe” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “Luxury Hotels London” (1)
- `text/heldout-164-insurance-policy.txt` ID_NUMBER: “GB24RBAL16357958515028” (1)
- `text/heldout-165-insurance-policy.txt` ID_NUMBER: “2023-BP-12854” (1)
- `text/heldout-167-investment-prospectus.txt` COMPANY: “Technology and Innovation Fund” (1)
- `text/heldout-170-investment-prospectus.txt` ADDRESS: “6789, 123 Fake Street
   Anytown, US” (1)
- `text/heldout-172-isda-definition.txt` COMPANY: “London Court of International Arbitration” (2)
- `text/heldout-173-isda-definition.txt` ID_NUMBER: “GB24CAHL67306072373596” (1)
- `text/heldout-174-isda-definition.txt` ID_NUMBER: “E88-7198-265-04” (1)
- `text/heldout-176-it-support-ticket.txt` PERSON: “Véronique P. Mallet” (1)
- `text/heldout-176-it-support-ticket.txt` EMAIL: “veronique.mallet@example.com” (1)
- `text/heldout-177-it-support-ticket.txt` COMPANY: “IT Support Team” (1)
- `text/heldout-181-loan-agreement.txt` ID_NUMBER: “2285235482941386” (1)
- `text/heldout-186-loan-application.txt` PERSON: “12345678” (1)
- `text/heldout-187-loan-application.txt` COMPANY: “Financial Details
Bank” (1)
- `text/heldout-187-loan-application.txt` COMPANY: “TD Bank” (1)
- `text/heldout-189-loan-application.txt` PERSON: “John Doe” (1)
- `text/heldout-189-loan-application.txt` DATE_OF_BIRTH: “01/01/1980” (1)
- `text/heldout-189-loan-application.txt` ID_NUMBER: “123-45-6789” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Community Group” (1)
- `text/heldout-191-mortgage-amortization-schedule.txt` PERSON: “Richard Davis-Collins” (1)
- `text/heldout-196-mortgage-contract.txt` COMPANY: “DFMVDEWF223” (1)
- `text/heldout-202-mt940.txt` ID_NUMBER: “191234567890” (17)
- `text/heldout-203-mt940.txt` ID_NUMBER: “1234567890” (9)
- `text/heldout-203-mt940.txt` PERSON: “JBROWN” (5)
- `text/heldout-203-mt940.txt` ID_NUMBER: “4567890123” (4)
- `text/heldout-204-mt940.txt` PERSON: “MARK SIMPSON-TAYLOR” (1)
- `text/heldout-204-mt940.txt` PERSON: “MARK SIMP” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “4567845674567456” (1)
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220520123456” (1)
- `text/heldout-206-payment-confirmation.txt` PERSON: “Joaquín Ureña-Vilalta” (2)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “7fde39a0-1d2a-4e03-8f4a-d33ae2bf8f5c” (1)
- `text/heldout-207-payment-confirmation.txt` COMPANY: “Venmo” (2)
- `text/heldout-207-payment-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-208-payment-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-208-payment-confirmation.txt` EMAIL: “email@company.com” (2)
- `text/heldout-208-payment-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-210-payment-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-210-payment-confirmation.txt` COMPANY: “ABC Company Ltd” (1)
- `text/heldout-211-pension-plan-agreement.txt` ID_NUMBER: “Jp-58502” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Hon” (1)
- `text/heldout-213-pension-plan-agreement.txt` PERSON: “AMANDO VALENCIA” (1)
- `text/heldout-213-pension-plan-agreement.txt` ID_NUMBER: “Ua12597-R” (1)
- `text/heldout-216-policyholder-s-report.txt` COMPANY: “EXAMPLE INSURANCE” (2)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “987654321” (1)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Premier Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Elite Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Supreme Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` SECRET: “de42714ebBf36ea6Ce53” (1)
- `text/heldout-220-policyholder-s-report.txt` PHONE: “0800 012 3456” (1)
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[contact information]” (3)
- `text/heldout-229-product-disclosure-statement.txt` ID_NUMBER: “GB69ATOF66777186070880” (1)
- `text/heldout-240-regulatory-compliance-guide.txt` ONLINE_ID: “185.131.138.249” (1)
- `text/heldout-241-regulatory-filing.txt` DATE_OF_BIRTH: “December 24, 1977” (1)
- `text/heldout-244-regulatory-filing.txt` ID_NUMBER: “CID-878308” (1)
- `text/heldout-247-renewal-reminder.txt` EMAIL: “[company_email@companyname.com]” (1)
- `text/heldout-247-renewal-reminder.txt` EMAIL: “company_email@companyname.com” (1)
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “XMYTGBDH508” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ADDRESS: “45 High Street, London, UK” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ID_NUMBER: “489290094” (1)
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862004760” (1)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “O1234567890” (2)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “1234567890” (2)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC LIMITED” (1)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC CORPORATE” (1)
- `text/heldout-277-tax-assessment-notice.txt` PERSON: “John Doe” (1)
- `text/heldout-277-tax-assessment-notice.txt` ID_NUMBER: “456789012” (1)
- `text/heldout-278-tax-assessment-notice.txt` ID_NUMBER: “FR4855184678937477672584491” (1)
- `text/heldout-278-tax-assessment-notice.txt` ID_NUMBER: “213-76554-56” (1)
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-280-tax-assessment-notice.txt` COMPANY: “HMRC” (2)
- `text/heldout-280-tax-assessment-notice.txt` ID_NUMBER: “2023-001234-UK” (2)
- `text/heldout-280-tax-assessment-notice.txt` ID_NUMBER: “12345678” (1)
- `text/heldout-281-tax-return.txt` PERSON: “John A. Smith” (1)
- `text/heldout-283-tax-return.txt` COMPANY: “Artistic Project for XYZ Ltd.” (1)
- `text/heldout-283-tax-return.txt` COMPANY: “Artistic Project for ABC Ltd.” (1)
- `text/heldout-284-tax-return.txt` PERSON: “Letizia P. Staglieno” (1)
- `text/heldout-285-tax-return.txt` COMPANY: “Other Partners” (1)
- `text/heldout-287-trade-confirmation.txt` ID_NUMBER: “EMP406537” (1)
- `text/heldout-288-trade-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-289-trade-confirmation.txt` PERSON: “Isabel Kambs” (1)
- `text/heldout-290-trade-confirmation.txt` EMAIL: “Jane.Smith@sellerco.com” (2)
- `text/heldout-290-trade-confirmation.txt` COMPANY: “First National Bank
Bank” (1)
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Second National Bank
Bank” (1)
- `text/heldout-291-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-291-transaction-confirmation.txt` ID_NUMBER: “12345678” (1)
- `text/heldout-292-transaction-confirmation.txt` ID_NUMBER: “444” (1)
- `text/heldout-294-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-294-transaction-confirmation.txt` ID_NUMBER: “12345678” (1)
- `text/heldout-294-transaction-confirmation.txt` EMAIL: “info@abcv insurance.com” (1)
- `text/heldout-297-xbrl.txt` COMPANY: “ABC_Corporation” (1)

**Over-redacted**

- `text/heldout-001-annual-report.txt` PERSON: “Stakeholders” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “Diversity and Inclusion Task Force” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “task force” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “task force” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “employee resource groups” is not on the answer key
- `text/heldout-003-annual-report.txt` COMPANY: “Our dedicated team” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` PERSON: “management” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-008-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-009-audit-report.txt` COMPANY: “Management's Responsibility

Management” is not on the answer key
- `text/heldout-009-audit-report.txt` COMPANY: “American Institute of Certified Public Accountants” is not on the answer key
- `text/heldout-009-audit-report.txt` COMPANY: “Management” is not on the answer key
- `text/heldout-010-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-010-audit-report.txt` COMPANY: “local community organizations” is not on the answer key
- `text/heldout-010-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-010-audit-report.txt` COMPANY: “company” is not on the answer key
- `text/heldout-012-bai-format.txt` DATE_OF_BIRTH: “2023-02-15” is not on the answer key
- `text/heldout-012-bai-format.txt` DATE_OF_BIRTH: “2023-02-13” is not on the answer key
- `text/heldout-013-bai-format.txt` DATE_OF_BIRTH: “2023-02-17” is not on the answer key
- `text/heldout-013-bai-format.txt` DATE_OF_BIRTH: “2023-02-19” is not on the answer key
- `text/heldout-014-bai-format.txt` SECRET: “SFTTDECT202” is not on the answer key
- `text/heldout-014-bai-format.txt` DATE_OF_BIRTH: “20220315” is not on the answer key
- `text/heldout-014-bai-format.txt` SECRET: “TA” is not on the answer key
- `text/heldout-014-bai-format.txt` SECRET: “TRC” is not on the answer key
- `text/heldout-014-bai-format.txt` SECRET: “USD” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “BAI022” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “62D” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “BAI022” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “59F:GB” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “52A” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “54A” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “59F:GB” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “63F” is not on the answer key
- `text/heldout-015-bai-format.txt` SECRET: “Cash_Concentration
:13A” is not on the answer key
- `text/heldout-015-bai-format.txt` ADDRESS: “London” is not on the answer key
- `text/heldout-019-bank-statement.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-019-bank-statement.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-020-bank-statement.txt` SECRET: “Bank Name” is not on the answer key
- `text/heldout-020-bank-statement.txt` ONLINE_ID: “Bank Address” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “Shipper” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “shipper” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “shipper” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “carrier” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` COMPANY: “carrier” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` COMPANY: “Shipper” is not on the answer key
- `text/heldout-024-bill-of-lading.txt` SECRET: “XYZ-EQ-001” is not on the answer key
- `text/heldout-024-bill-of-lading.txt` SECRET: “XYZ-EQ-500” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` COMPANY: “MV Sea Eagle” is not on the answer key
- `text/heldout-027-business-plan.txt` COMPANY: “London-based tech startup” is not on the answer key
- `text/heldout-027-business-plan.txt` COMPANY: “home security company” is not on the answer key
- `text/heldout-027-business-plan.txt` COMPANY: “retail stores” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “community partnerships” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “reputable suppliers” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “local schools” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “community centers” is not on the answer key
- `text/heldout-028-business-plan.txt` COMPANY: “environmental organizations” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “restaurants” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “food-related businesses” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “hospitality management schools and universities” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “top-tier restaurants” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “hospitality management schools and universities” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “colleges and universities” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-032-compliance-certificate.txt` ONLINE_ID: “394-Martha-Ramp” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Labor Rights” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “company” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “company” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Equal Opportunity Committee” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` ADDRESS: “City, State, Postal Code” is not on the answer key
- `text/heldout-034-compliance-certificate.txt` COMPANY: “National Institute of Standards and Technology” is not on the answer key
- `text/heldout-034-compliance-certificate.txt` COMPANY: “NIST” is not on the answer key
- `text/heldout-035-compliance-certificate.txt` COMPANY: “industry” is not on the answer key
- `text/heldout-036-corporate-governance-guidelines.txt` COMPANY: “research institutions” is not on the answer key
- `text/heldout-036-corporate-governance-guidelines.txt` COMPANY: “startups” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “The Board of Directors” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Compensation Committee” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Compensation Committee” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “The Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “The Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “The Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “The Board” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “The Board” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “organization” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “CEO” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “board members” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “CEO” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “CEO” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “CEO” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “Board Members” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “board of directors” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “management” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` PERSON: “employee” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` PERSON: “manager” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “compliance officer” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “compliance officer” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “management team” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-040-corporate-governance-guidelines.txt` COMPANY: “company” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs
Corporation” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “INTERNAL REVENUE” is not on the answer key
- `text/heldout-050-credit-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-051-credit-card-application.txt` ADDRESS: “Street address” is not on the answer key
- `text/heldout-051-credit-card-application.txt` DATE_OF_BIRTH: “Yes” is not on the answer key
- `text/heldout-052-credit-card-application.txt` COMPANY: “musicians and music enthusiasts” is not on the answer key
- `text/heldout-052-credit-card-application.txt` COMPANY: “music events” is not on the answer key
- `text/heldout-053-credit-card-application.txt` ADDRESS: “Address” is not on the answer key
- `text/heldout-054-credit-card-application.txt` SECRET: “api” is not on the answer key
- `text/heldout-054-credit-card-application.txt` SECRET: “sk\_live\_Ba2WeZEl0NroHt2z3D8HaUNT” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “2022-03-14T12” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “2d9f3b0e-f31e-4d04-9e8e-9cc276f3b333” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “2022-03-14T15” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “3e0g4c1f-g42f-5h05-0i8f-0jkl78mnop00” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ONLINE_ID: “Investor Local LatLng” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ONLINE_ID: “1” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “0x8f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “101.143160” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “0x23e1e388e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “101.143160” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “0x9f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` SECRET: “0x1f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` SECRET: “BC1QRZGE3E9N2S92Q7JYLAE3FR3CGJAMMN” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “2022-03-14T12” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “34” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` SECRET: “e4f5g6h7-8i9j0-1k2l3-4m5n6-7b8c9d0e” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` EMAIL: “tb1q9gjvavzgnm7eu3p9y9u3ek94e39999” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “2022-03-13T10” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “29:15Z” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` SECRET: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` DATE_OF_BIRTH: “2022-03-12T09” is not on the answer key
- `text/heldout-064-cryptocurrency-transaction-report.txt` SECRET: “3.12345678” is not on the answer key
- `text/heldout-064-cryptocurrency-transaction-report.txt` SECRET: “1.09876543” is not on the answer key
- `text/heldout-064-cryptocurrency-transaction-report.txt` SECRET: “0.25689101” is not on the answer key
- `text/heldout-064-cryptocurrency-transaction-report.txt` SECRET: “2.98712345” is not on the answer key
- `text/heldout-064-cryptocurrency-transaction-report.txt` SECRET: “5.62617234” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` SECRET: “00123456” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “00012345” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` SECRET: “00000123” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` SECRET: “00000012” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` SECRET: “00000001” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` SECRET: “1BvBMSEYstWetqTFn5A” is not on the answer key
- `text/heldout-066-csv.txt` ONLINE_ID: “9.3194175 N” is not on the answer key
- `text/heldout-066-csv.txt` SECRET: “76.559561 E” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “Address” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Alice” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Anna” is not on the answer key
- `text/heldout-069-csv.txt` PERSON: “Syd” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` SECRET: “EUR” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` SECRET: “0.8674” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` SECRET: “0.6129” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` SECRET: “JPY” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` ONLINE_ID: “-43.591313” is not on the answer key
- `text/heldout-071-currency-exchange-rate-sheet.txt` ONLINE_ID: “63.440134” is not on the answer key
- `text/heldout-072-currency-exchange-rate-sheet.txt` ONLINE_ID: “55.079215” is not on the answer key
- `text/heldout-072-currency-exchange-rate-sheet.txt` ONLINE_ID: “70.908111” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` ADDRESS: “Name” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` ADDRESS: “Street Address” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` SECRET: “CAD” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` SECRET: “JPY” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` SECRET: “CHF” is not on the answer key
- `text/heldout-073-currency-exchange-rate-sheet.txt` SECRET: “AUD” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “Local LatLng” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “59” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ONLINE_ID: “628758” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “167.532272
Street Address” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “local latlng” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` ADDRESS: “street address” is not on the answer key
- `text/heldout-077-customer-agreement.txt` COMPANY: “Province of Ontario” is not on the answer key
- `text/heldout-077-customer-agreement.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-077-customer-agreement.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-077-customer-agreement.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` COMPANY: “Tenant” is not on the answer key
- `text/heldout-078-customer-agreement.txt` PERSON: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` PERSON: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` PERSON: “Landlord” is not on the answer key
- `text/heldout-078-customer-agreement.txt` PERSON: “Landlord” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “MARKETING STRATEGIES” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Customer” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` PERSON: “Customer” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “RIGHTS” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-080-customer-agreement.txt` ADDRESS: “business address” is not on the answer key
- `text/heldout-080-customer-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-080-customer-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-080-customer-agreement.txt` PERSON: “Customer” is not on the answer key
- `text/heldout-080-customer-agreement.txt` PERSON: “Customer” is not on the answer key
- `text/heldout-084-customer-support-conversational-log.txt` SECRET: “username” is not on the answer key
- `text/heldout-084-customer-support-conversational-log.txt` PERSON: “I” is not on the answer key
- `text/heldout-085-customer-support-conversational-log.txt` ONLINE_ID: “74.4284035” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “Dispute Review Board” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “Justinshire” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “RESOLUTION POLICY” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “parties” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “external entities” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “Our retired judges” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “mediator” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “arbitrator” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “retired judges” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “mediator” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` COMPANY: “arbitrator” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` SECRET: “Swift BIC Code” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` ADDRESS: “Street Address” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “customers” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “external entities” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` PERSON: “mediator” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` PERSON: “mediator” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “regulatory body” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “Confidentiality” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-089-dispute-resolution-policy.txt` COMPANY: “business relationship” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “parties” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “our company” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “external entities” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “neutral third party” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` PERSON: “mediator” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` PERSON: “mediator” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-092-edi.txt` SECRET: “TE” is not on the answer key
- `text/heldout-092-edi.txt` COMPANY: “IEA” is not on the answer key
- `text/heldout-093-edi.txt` SECRET: “202108121530” is not on the answer key
- `text/heldout-093-edi.txt` DATE_OF_BIRTH: “19650101” is not on the answer key
- `text/heldout-094-edi.txt` DATE_OF_BIRTH: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` DATE_OF_BIRTH: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` DATE_OF_BIRTH: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` DATE_OF_BIRTH: “210101:0930” is not on the answer key
- `text/heldout-094-edi.txt` SECRET: “BGM” is not on the answer key
- `text/heldout-094-edi.txt` SECRET: “215” is not on the answer key
- `text/heldout-094-edi.txt` DATE_OF_BIRTH: “20220101” is not on the answer key
- `text/heldout-094-edi.txt` SECRET: “NAD” is not on the answer key
- `text/heldout-094-edi.txt` SECRET: “NAD” is not on the answer key
- `text/heldout-094-edi.txt` SECRET: “SU” is not on the answer key
- `text/heldout-094-edi.txt` COMPANY: “supplier” is not on the answer key
- `text/heldout-094-edi.txt` ADDRESS: “street address” is not on the answer key
- `text/heldout-095-edi.txt` SECRET: “300” is not on the answer key
- `text/heldout-095-edi.txt` COMPANY: “UNH” is not on the answer key
- `text/heldout-095-edi.txt` SECRET: “98A” is not on the answer key
- `text/heldout-095-edi.txt` ONLINE_ID: “123456” is not on the answer key
- `text/heldout-095-edi.txt` PHONE: “789123” is not on the answer key
- `text/heldout-095-edi.txt` COMPANY: “NAD” is not on the answer key
- `text/heldout-095-edi.txt` ADDRESS: “street address” is not on the answer key
- `text/heldout-095-edi.txt` PERSON: “shipper” is not on the answer key
- `text/heldout-097-email.txt` PERSON: “She” is not on the answer key
- `text/heldout-098-email.txt` COMPANY: “Healthcare” is not on the answer key
- `text/heldout-099-email.txt` COMPANY: “leading tech companies” is not on the answer key
- `text/heldout-100-email.txt` ADDRESS: “City, State” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “United Kingdom” is not on the answer key
- `text/heldout-101-employment-contract.txt` DATE_OF_BIRTH: “11:23:56” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Delaware corporation” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Employee herein” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Delaware corporation” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “State of Delaware” is not on the answer key
- `text/heldout-105-employment-contract.txt` COMPANY: “Delaware corporation” is not on the answer key
- `text/heldout-105-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-105-employment-contract.txt` COMPANY: “The Company” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-105-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-106-financial-aid-application.txt` DATE_OF_BIRTH: “June 2017” is not on the answer key
- `text/heldout-106-financial-aid-application.txt` COMPANY: “College/University” is not on the answer key
- `text/heldout-106-financial-aid-application.txt` COMPANY: “N/A
- Major: Computer Science” is not on the answer key
- `text/heldout-106-financial-aid-application.txt` PERSON: “Dean” is not on the answer key
- `text/heldout-107-financial-aid-application.txt` COMPANY: “Company Name]” is not on the answer key
- `text/heldout-107-financial-aid-application.txt` COMPANY: “Field of Study]” is not on the answer key
- `text/heldout-107-financial-aid-application.txt` COMPANY: “University Name]” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` COMPANY: “emergency fund” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` COMPANY: “last employer” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-108-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-109-financial-aid-application.txt` EMAIL: “mailto” is not on the answer key
- `text/heldout-109-financial-aid-application.txt` PERSON: “I” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` COMPANY: “Financial Support Program” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` DATE_OF_BIRTH: “Date” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ADDRESS: “Mailing Address” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` COMPANY: “various school clubs” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` COMPANY: “reputable university” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` ID_NUMBER: “43298715” is not on the answer key
- `text/heldout-112-financial-data-feed.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-112-financial-data-feed.txt` DATE_OF_BIRTH: “2025-01-01” is not on the answer key
- `text/heldout-113-financial-data-feed.txt` DATE_OF_BIRTH: “2027-03-14” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` SECRET: “REMY22Q1” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “3 Villagatan” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “City>Varberg” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ONLINE_ID: “43333” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “entity” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` DATE_OF_BIRTH: “past year” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “entity” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “Directors and Officers” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “entity's directors” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “officers” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “directors and officers” is not on the answer key
- `text/heldout-116-financial-disclosure-statement.txt` COMPANY: “entity” is not on the answer key
- `text/heldout-117-financial-disclosure-statement.txt` COMPANY: “environmental organizations” is not on the answer key
- `text/heldout-117-financial-disclosure-statement.txt` COMPANY: “Environmental Organizations” is not on the answer key
- `text/heldout-117-financial-disclosure-statement.txt` COMPANY: “leading environmental organizations” is not on the answer key
- `text/heldout-118-financial-disclosure-statement.txt` COMPANY: “Wholly owned” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` SECRET: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-121-financial-forecast.txt` COMPANY: “businesses” is not on the answer key
- `text/heldout-121-financial-forecast.txt` COMPANY: “digital marketing industry” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “industry peers” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-123-financial-forecast.txt` COMPANY: “company” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “new and existing customers” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “high-risk customers” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “relevant authorities” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` ADDRESS: “98.608882” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` SECRET: “government-issued ID” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` SECRET: “proof of address” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “high-risk countries” is not on the answer key
- `text/heldout-127-financial-regulatory-compliance-report.txt` COMPANY: “relevant authorities” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` COMPANY: “The organization” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` COMPANY: “subject matter experts” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` EMAIL: “mailto” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-131-financial-risk-assessment.txt` PERSON: “she” is not on the answer key
- `text/heldout-131-financial-risk-assessment.txt` PERSON: “her” is not on the answer key
- `text/heldout-131-financial-risk-assessment.txt` PERSON: “she” is not on the answer key
- `text/heldout-132-financial-risk-assessment.txt` COMPANY: “ESG” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-133-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` COMPANY: “suppliers” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` COMPANY: “supplier” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` COMPANY: “client base” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “business” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` COMPANY: “company” is not on the answer key
- `text/heldout-135-financial-risk-assessment.txt` ONLINE_ID: “phone number” is not on the answer key
- `text/heldout-136-financial-statement.txt` COMPANY: “Canada” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “Richardshire” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “leading biotechnology firm” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “company” is not on the answer key
- `text/heldout-141-fix-protocol.txt` SECRET: “TradeID123456” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “55=20210318-14” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “32:15
56” is not on the answer key
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “554” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “Tag_Value_Pair_List
101” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “49” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “20765” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “22” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “12” is not on the answer key
- `text/heldout-142-fix-protocol.txt` SECRET: “1137” is not on the answer key
- `text/heldout-143-fix-protocol.txt` SECRET: “NSENF0011G” is not on the answer key
- `text/heldout-143-fix-protocol.txt` SECRET: “54” is not on the answer key
- `text/heldout-144-fix-protocol.txt` SECRET: “ISIN-GB00B1234567” is not on the answer key
- `text/heldout-144-fix-protocol.txt` SECRET: “54” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ONLINE_ID: “547” is not on the answer key
- `text/heldout-147-fpml.txt` SECRET: “USD” is not on the answer key
- `text/heldout-147-fpml.txt` SECRET: “EUR” is not on the answer key
- `text/heldout-147-fpml.txt` SECRET: “USD” is not on the answer key
- `text/heldout-148-fpml.txt` SECRET: “430” is not on the answer key
- `text/heldout-148-fpml.txt` SECRET: “430” is not on the answer key
- `text/heldout-148-fpml.txt` SECRET: “USD” is not on the answer key
- `text/heldout-149-fpml.txt` DATE_OF_BIRTH: “2022-03-01” is not on the answer key
- `text/heldout-149-fpml.txt` DATE_OF_BIRTH: “2023-03-01” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “Apt. 7” is not on the answer key
- `text/heldout-150-fpml.txt` DATE_OF_BIRTH: “2027-03-01” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` ADDRESS: “CA 12345, USA” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` ADDRESS: “CA 12345” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` COMPANY: “former business partner” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` PERSON: “My doctor” is not on the answer key
- `text/heldout-161-insurance-policy.txt` ADDRESS: “II” is not on the answer key
- `text/heldout-163-insurance-policy.txt` COMPANY: “INSURANCE” is not on the answer key
- `text/heldout-163-insurance-policy.txt` COMPANY: “Policyholder” is not on the answer key
- `text/heldout-163-insurance-policy.txt` COMPANY: “Policyholder” is not on the answer key
- `text/heldout-163-insurance-policy.txt` PERSON: “Insurer” is not on the answer key
- `text/heldout-164-insurance-policy.txt` COMPANY: “INSURANCE” is not on the answer key
- `text/heldout-164-insurance-policy.txt` COMPANY: “COVERAGE” is not on the answer key
- `text/heldout-164-insurance-policy.txt` COMPANY: “Insured Pet” is not on the answer key
- `text/heldout-164-insurance-policy.txt` COMPANY: “Insured Pet” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insurer” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-165-insurance-policy.txt` COMPANY: “Insured” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “technology and innovation-driven companies” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “high-growth companies” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “technology and innovation-driven companies” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “high-growth companies” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “technology and innovation-driven companies” is not on the answer key
- `text/heldout-167-investment-prospectus.txt` COMPANY: “technology and innovation-driven companies” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “healthcare companies” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “healthcare companies” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “businesses” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “healthcare companies” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “healthcare providers” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “research institutions” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “headquarters” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` COMPANY: “healthcare companies” is not on the answer key
- `text/heldout-168-investment-prospectus.txt` EMAIL: “info@hor” is not on the answer key
- `text/heldout-169-investment-prospectus.txt` COMPANY: “investment committee” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` EMAIL: “info@tuckers.

   Local” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` PHONE: “86.649371” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` PHONE: “85.982249” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` COMPANY: “venture capital firms” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Counterparty Name” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Counterparty Jurisdiction” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Your Company Jurisdiction” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` SECRET: “ELIGIBILITY” is not on the answer key
- `text/heldout-173-isda-definition.txt` ADDRESS: “Zip Code” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty Name” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “State of Incorporation” is not on the answer key
- `text/heldout-173-isda-definition.txt` ADDRESS: “Address” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “Counterparty” is not on the answer key
- `text/heldout-173-isda-definition.txt` COMPANY: “State of New York” is not on the answer key
- `text/heldout-174-isda-definition.txt` COMPANY: “Parties” is not on the answer key
- `text/heldout-174-isda-definition.txt` COMPANY: “State of New York” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Sales department” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` SECRET: “Priority” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Sales department” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “IT team” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Exchange” is not on the answer key
- `text/heldout-179-it-support-ticket.txt` COMPANY: “internal system” is not on the answer key
- `text/heldout-179-it-support-ticket.txt` COMPANY: “IT team” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` SECRET: “password” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` SECRET: “strong password” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` SECRET: “A-Z” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` SECRET: “a-z” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` PERSON: “user” is not on the answer key
- `text/heldout-181-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-181-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-182-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “State of Delaware” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Honda Civic” is not on the answer key
- `text/heldout-183-loan-agreement.txt` ADDRESS: “-83” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-183-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “State of Delaware” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-184-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-185-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-186-loan-application.txt` COMPANY: “UK government” is not on the answer key
- `text/heldout-189-loan-application.txt` SECRET: “650” is not on the answer key
- `text/heldout-189-loan-application.txt` COMPANY: “nonprofit organization” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “community members” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “adult education classes” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia community” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “community members” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “community center” is not on the answer key
- `text/heldout-190-loan-application.txt` COMPANY: “community members” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` ONLINE_ID: “777.46” is not on the answer key
- `text/heldout-196-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Lender” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-198-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “laws” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “State of Delaware” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “Federal Reserve” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “London Interbank” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “57F” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “59F” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “BNKCUSUS33XXX0000123456LAURENCE” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “MORENO51501” is not on the answer key
- `text/heldout-201-mt940.txt` COMPANY: “PAUL ESTATES” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “DBTTRFACT220518USD-1200” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “CAD12345” is not on the answer key
- `text/heldout-201-mt940.txt` ONLINE_ID: “67
:61:20220520” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “64” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “65” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “66” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “67B” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “68C” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “69” is not on the answer key
- `text/heldout-201-mt940.txt` SECRET: “70” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “87654321” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “GB2LXXX” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “CA001234567890” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “62F” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “CAD12345” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “63F” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “CAD3456” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “78” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “CAD8901” is not on the answer key
- `text/heldout-202-mt940.txt` SECRET: “CAD12345” is not on the answer key
- `text/heldout-203-mt940.txt` ONLINE_ID: “USD:123456.78” is not on the answer key
- `text/heldout-203-mt940.txt` ONLINE_ID: “USD:12345.67” is not on the answer key
- `text/heldout-204-mt940.txt` SECRET: “MTXDRFTXXX000000000MLNXMONT0000000000” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “86” is not on the answer key
- `text/heldout-204-mt940.txt` SECRET: “87F” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “20220922163436” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “62F” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “A” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “64A” is not on the answer key
- `text/heldout-205-mt940.txt` ONLINE_ID: “123.45” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “I” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “20220519123456” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “78.90” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “456.78” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “D” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “20220517123456” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “1234.56” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “20220516123456” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “12345.67” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220520” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “123456.78” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “66” is not on the answer key
- `text/heldout-205-mt940.txt` SECRET: “202205” is not on the answer key
- `text/heldout-206-payment-confirmation.txt` DATE_OF_BIRTH: “2022-03-15” is not on the answer key
- `text/heldout-207-payment-confirmation.txt` ID_NUMBER: “478236951” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “hereina Governmental Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Board of Trustees” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` DATE_OF_BIRTH: “________, 20_____” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Employer” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Employer” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Employer” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “State of Illinois” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “Springfield Municipal Employees” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` PERSON: “Participant” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Committee” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Local Government Pension Scheme” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` SECRET: “name” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “Insurance Policyholder” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance company” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance company” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance company” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance company” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance company” is not on the answer key
- `text/heldout-218-policyholder-s-report.txt` COMPANY: “insurance companies” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` SECRET: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` SECRET: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` SECRET: “KPEEUSZS411” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-221-privacy-policy.txt` ADDRESS: “street address” is not on the answer key
- `text/heldout-222-privacy-policy.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-222-privacy-policy.txt` ADDRESS: “Street Address” is not on the answer key
- `text/heldout-223-privacy-policy.txt` COMPANY: “premier location-based services provider” is not on the answer key
- `text/heldout-223-privacy-policy.txt` ADDRESS: “latitude, longitude” is not on the answer key
- `text/heldout-223-privacy-policy.txt` ONLINE_ID: “IP address” is not on the answer key
- `text/heldout-223-privacy-policy.txt` COMPANY: “privacy” is not on the answer key
- `text/heldout-223-privacy-policy.txt` ONLINE_ID: “IP address” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “customers” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “identity verification service providers” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “contractors” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “third parties” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “names” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “addresses” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “email addresses” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “Company Name” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “address]
[city, state/province, postal code” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “country” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “The IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “The IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-227-product-disclosure-statement.txt` COMPANY: “Our hedge funds” is not on the answer key
- `text/heldout-227-product-disclosure-statement.txt` COMPANY: “private companies” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “technology sector” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “software companies” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “software company” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Tech Innovators Investment” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “technology sector” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “software companies” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “emerging technologies” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Individual companies” is not on the answer key
- `text/heldout-230-product-disclosure-statement.txt` COMPANY: “Fund” is not on the answer key
- `text/heldout-230-product-disclosure-statement.txt` COMPANY: “The Fund” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Bank” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “___________” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “___________” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “student housing development project” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “student housing development project” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “study lounges” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “senior housing facility” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` COMPANY: “senior housing facility” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Bank” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “banking corporation” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` ADDRESS: “residential address” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “student housing project” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` PERSON: “Borrower” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “state-of-the-art student housing facility” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “UK-based insurance company” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council's (FRC)” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “health plans” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “healthcare clearinghouses” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “healthcare providers” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Covered entities” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “covered entities” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HITECH” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “business associates of covered entities” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Media companies” is not on the answer key
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Media companies” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “aviation industry” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “aviation authority” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “Pilot Training Standards” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “Pilot training” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “Pilots” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “Ground school” is not on the answer key
- `text/heldout-241-regulatory-filing.txt` COMPANY: “community” is not on the answer key
- `text/heldout-241-regulatory-filing.txt` SECRET: “password” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “esteemed Regulatory Authorities” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “our organization” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` ADDRESS: “name” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “our organization” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “regulatory authorities” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “The Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “The Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “The Company” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` COMPANY: “The Company” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2023-06-15” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2023-06-16” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2043-06-15” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` DATE_OF_BIRTH: “13:02:23” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` ADDRESS: “Street Address” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-250-renewal-reminder.txt` COMPANY: “Your policy” is not on the answer key
- `text/heldout-250-renewal-reminder.txt` SECRET: “LOYAL10” is not on the answer key
- `text/heldout-251-safety-data-sheet.txt` SECRET: “GHS05” is not on the answer key
- `text/heldout-251-safety-data-sheet.txt` SECRET: “GHS07” is not on the answer key
- `text/heldout-252-safety-data-sheet.txt` SECRET: “31.45” is not on the answer key
- `text/heldout-254-safety-data-sheet.txt` ADDRESS: “Emergency” is not on the answer key
- `text/heldout-254-safety-data-sheet.txt` PERSON: “physician” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “state of Delaware” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` ADDRESS: “[City], DE 19999” is not on the answer key
- `text/heldout-257-securities-prospectus.txt` COMPANY: “leading technology company” is not on the answer key
- `text/heldout-258-securities-prospectus.txt` COMPANY: “UK-based manufacturing company” is not on the answer key
- `text/heldout-258-securities-prospectus.txt` COMPANY: “leading manufacturer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “RISK” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “UK-based manufacturing company” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-260-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Register of Members of the Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Register of Members of the Company” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “The Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “respective officers, directors” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “directors” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “employees” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “________________” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Transferee” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Transferee” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Transferee” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` ONLINE_ID: “phone number” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company's board of directors” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Board” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “________________” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Board of Directors” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Board of Directors” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` COMPANY: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` COMPANY: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` COMPANY: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “________________” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “________________________” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “Province of Ontario” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “Client” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “clients” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “Client's supply chain” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “Client” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` COMPANY: “Client's supply chain” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` COMPANY: “State of New York” is not on the answer key
- `text/heldout-271-swift-message.txt` SECRET: “CDRCGB22GPT” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CHASUS33CHAS” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CHASMTON” is not on the answer key
- `text/heldout-271-swift-message.txt` SECRET: “71A” is not on the answer key
- `text/heldout-271-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-272-swift-message.txt` ONLINE_ID: “NY washedenx” is not on the answer key
- `text/heldout-272-swift-message.txt` ONLINE_ID: “28C:52D” is not on the answer key
- `text/heldout-272-swift-message.txt` ONLINE_ID: “59:20220920” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “ABCDEFGHIJKLMNOP
:77S:USDGBP” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “16R” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “1” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “32B” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “CRED” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “33B” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “37B” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “ABCDEFGHIJ
:38B:USD
:39B:ABCDEFGHIJKL
:40B:GBP
:57F” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “ABCDEFGHIJKLMNOP” is not on the answer key
- `text/heldout-272-swift-message.txt` ONLINE_ID: “70:20220921” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “71A” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “ABCDEFGHIJKLMNOP” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “77S” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “GBPUSD” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “16R” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “32B” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “33B” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “38B” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “39B” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “40B:USD” is not on the answer key
- `text/heldout-272-swift-message.txt` SECRET: “57F” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “BCSMNY33” is not on the answer key
- `text/heldout-273-swift-message.txt` DATE_OF_BIRTH: “20220315” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “53A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “56A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “57A::ABBYGB2L” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “58A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “GBP
:59” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “60F” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “72” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “77S” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “78” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-274-swift-message.txt` SECRET: “98A” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “25” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “WFIBGB2LXXX” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “1234567890” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “73B” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “77A” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “108” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “CCYGBP” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “132” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “138” is not on the answer key
- `text/heldout-275-swift-message.txt` SECRET: “142” is not on the answer key
- `text/heldout-276-tax-assessment-notice.txt` COMPANY: “UNITED KINGDOM” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` COMPANY: “Toyota” is not on the answer key
- `text/heldout-283-tax-return.txt` COMPANY: “UNITED KINGDOM SELF-ASSESSMENT” is not on the answer key
- `text/heldout-283-tax-return.txt` ID_NUMBER: “OVMEUSXS349” is not on the answer key
- `text/heldout-284-tax-return.txt` COMPANY: “S corporations, trusts” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2023-02-14” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2024-05-25” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` ADDRESS: “NW1 2LB
United Kingdom” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` SECRET: “951” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` DATE_OF_BIRTH: “today” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “Tenant” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` PERSON: “landlord” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “jurisdiction” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` ADDRESS: “Property Address” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` ADDRESS: “address” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-292-transaction-confirmation.txt` COMPANY: “Landlord” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` DATE_OF_BIRTH: “2003-09-28” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` DATE_OF_BIRTH: “2003-09-28” is not on the answer key
- `text/heldout-295-transaction-confirmation.txt` DATE_OF_BIRTH: “001234” is not on the answer key
- `text/heldout-296-xbrl.txt` SECRET: “tbx” is not on the answer key
- `text/heldout-296-xbrl.txt` SECRET: “xbrli” is not on the answer key
- `text/heldout-297-xbrl.txt` SECRET: “gaap_20X-03-31_usd” is not on the answer key
- `text/heldout-297-xbrl.txt` SECRET: “gaap_20X-03-31_usd” is not on the answer key
- `text/heldout-298-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-298-xbrl.txt` DATE_OF_BIRTH: “time-2022-03-31” is not on the answer key
- `text/heldout-298-xbrl.txt` SECRET: “xbrli” is not on the answer key
- `text/heldout-298-xbrl.txt` DATE_OF_BIRTH: “time-2022-03-31” is not on the answer key
- `text/heldout-298-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-298-xbrl.txt` ID_NUMBER: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “xbrldi” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “currency” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “GBP” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “currency” is not on the answer key
- `text/heldout-299-xbrl.txt` SECRET: “GB3” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “xbrl-linkbase-2003-12-31” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “iso4217” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “iso4217-2003-09-30” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “xbrli” is not on the answer key
- `text/heldout-300-xbrl.txt` SECRET: “arcrole” is not on the answer key

### phi4

**Missed**

- `text/heldout-004-annual-report.txt` COMPANY: “2 024 Inc.” (1)
- `text/heldout-012-bai-format.txt` PERSON: “Micaela R. Barranco” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “976481327” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “2345 River Road, Apt. 091” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “San Francisco, CA 94112” (1)
- `text/heldout-028-business-plan.txt` COMPANY: “Eco-Friendly Cleaning Services” (2)
- `text/heldout-030-business-plan.txt` COMPANY: “TAOC” (7)
- `text/heldout-031-compliance-certificate.txt` ADDRESS: “60 Hövelgasse” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Uber Eats” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Amazon” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Netflix” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Apple” (1)
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Your Bank” (1)
- `text/heldout-068-csv.txt` PERSON: “John” (1)
- `text/heldout-068-csv.txt` PERSON: “Jane” (1)
- `text/heldout-068-csv.txt` PERSON: “Bob” (1)
- `text/heldout-068-csv.txt` PERSON: “Charlie” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1990-03-03” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1988-11-11” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1992-07-07” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1989-12-12” (1)
- `text/heldout-068-csv.txt` DATE_OF_BIRTH: “1991-09-09” (1)
- `text/heldout-084-customer-support-conversational-log.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-091-edi.txt` PERSON: “Shipper Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Postal Code” (1)
- `text/heldout-091-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Postal Code” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Postal Code” (1)
- `text/heldout-094-edi.txt` ID_NUMBER: “W6843547” (1)
- `text/heldout-095-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-096-email.txt` COMPANY: “at TimeMaster

P” (1)
- `text/heldout-101-employment-contract.txt` ADDRESS: “8 Renshof” (1)
- `text/heldout-107-financial-aid-application.txt` ADDRESS: “6092 Sutton Field, New Ericfurt, postal code 29575” (1)
- `text/heldout-128-financial-regulatory-compliance-report.txt` PERSON: “Livia F. d' Heripon” (1)
- `text/heldout-137-financial-statement.txt` COMPANY: “International Trade Company” (1)
- `text/heldout-158-insurance-claim-form.txt` COMPANY: “XYZ Engineering Services” (1)
- `text/heldout-158-insurance-claim-form.txt` COMPANY: “ABC Construction Services” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “ABC Travel Agency” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “XYZ Airlines” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “Luxury Hotels London” (1)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “Horizon Healthcare Innovation Fund” (3)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “MedHealth Innovations” (1)
- `text/heldout-168-investment-prospectus.txt` COMPANY: “Biosynth Labs” (1)
- `text/heldout-170-investment-prospectus.txt` COMPANY: “The Teton Valley Technology Startup Fund” (2)
- `text/heldout-172-isda-definition.txt` COMPANY: “London Court of International Arbitration” (2)
- `text/heldout-172-isda-definition.txt` PERSON: “Alicia Salomón Vizcaíno” (1)
- `text/heldout-177-it-support-ticket.txt` COMPANY: “IT Support Team” (1)
- `text/heldout-186-loan-application.txt` COMPANY: “National Debtline” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia Community Initiative” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia Community Center” (3)
- `text/heldout-204-mt940.txt` ID_NUMBER: “4567845674567456” (1)
- `text/heldout-206-payment-confirmation.txt` COMPANY: “ABC Online Store” (1)
- `text/heldout-207-payment-confirmation.txt` COMPANY: “Venmo” (3)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Premier Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Elite Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Supreme Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` SECRET: “de42714ebBf36ea6Ce53” (1)
- `text/heldout-223-privacy-policy.txt` COMPANY: “GeoTrack” (4)
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ University” (3)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Ofcom” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Canadian Radio-television and Telecommunications Commission (CRTC)” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Advertising Standards Authority (ASA)” (1)
- `text/heldout-273-swift-message.txt` ADDRESS: “065 Elizabeth Plains, Apt. 84659” (1)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “O1234567890” (2)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “1234567890” (2)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “C1479015350371735835064” (1)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “3261217076” (1)
- `text/heldout-280-tax-assessment-notice.txt` COMPANY: “HMRC” (2)
- `text/heldout-290-trade-confirmation.txt` PERSON: “John Doe” (1)
- `text/heldout-290-trade-confirmation.txt` PERSON: “Jane Smith” (1)

**Over-redacted**

- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “31st December 2021” is not on the answer key
- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “31st December 2021” is not on the answer key
- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “Q2 2021” is not on the answer key
- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “Q4 2021” is not on the answer key
- `text/heldout-006-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-006-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-011-bai-format.txt` ID_NUMBER: “220223USD 12,500.00” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASTOR UREÑA4” is not on the answer key
- `text/heldout-011-bai-format.txt` ADDRESS: “CHEMIN DA COSTA, PASCAL” is not on the answer key
- `text/heldout-011-bai-format.txt` ID_NUMBER: “220223USD 12,500.00” is not on the answer key
- `text/heldout-011-bai-format.txt` DATE_OF_BIRTH: “20230222” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-011-bai-format.txt` SECRET: “XXXXXXXXXXXXXXXX” is not on the answer key
- `text/heldout-016-bank-statement.txt` DATE_OF_BIRTH: “06/22/202R1” is not on the answer key
- `text/heldout-019-bank-statement.txt` DATE_OF_BIRTH: “31 Dec 2é2015” is not on the answer key
- `text/heldout-020-bank-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` COMPANY: “Operations Manager” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` DATE_OF_BIRTH: “01/06/2023” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` COMPANY: “MV Ocean Titan” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` PHONE: “B/L No.: ABCD123456” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “Serial Number: EC-2345-6789” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` DATE_OF_BIRTH: “31st August 2022” is not on the answer key
- `text/heldout-035-compliance-certificate.txt` DATE_OF_BIRTH: “March 1, 2023” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` DATE_OF_BIRTH: “31/03/2022” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` DATE_OF_BIRTH: “January 1, 2021” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-046-credit-application.txt` SECRET: “[CVV]” is not on the answer key
- `text/heldout-047-credit-application.txt` DATE_OF_BIRTH: “07/05/1989” is not on the answer key
- `text/heldout-047-credit-application.txt` DATE_OF_BIRTH: “07/05/1989” is not on the answer key
- `text/heldout-048-credit-application.txt` DATE_OF_BIRTH: “12th June 2023” is not on the answer key
- `text/heldout-048-credit-application.txt` DATE_OF_BIRTH: “1st January 2023” is not on the answer key
- `text/heldout-052-credit-card-application.txt` DATE_OF_BIRTH: “Date of Birth” is not on the answer key
- `text/heldout-052-credit-card-application.txt` EMAIL: “Email Address” is not on the answer key
- `text/heldout-052-credit-card-application.txt` PHONE: “Mobile Phone Number” is not on the answer key
- `text/heldout-052-credit-card-application.txt` ADDRESS: “Street Address” is not on the answer key
- `text/heldout-052-credit-card-application.txt` ADDRESS: “Postal Code” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023 - 01/31/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ONLINE_ID: “Order #123456” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023 - 01/31/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` ONLINE_ID: “www.yourbank.com/payments” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` ID_NUMBER: “**** **** **** 4205” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023 - 01/31/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/05/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/25/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023 - 31/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “28/02/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “£25.00 or stated balance, whichever is greater” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “15/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “£90.00 (previously £0.00)” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “125.35” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “125.35” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “14.20” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “54.99” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “90.00” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “45.78” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “83.12” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` ID_NUMBER: “24.55” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy” is not on the answer key
- `text/heldout-061-cryptocurrency-transaction-report.txt` SECRET: “bc1qar0srrr7xfkvy5l643lydnw93hlwdgp” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “3d567f8a-4a5d-4b6c-a9d0-e345687921f0” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “BC1QRZGE3E9N2S92Q7JYLAE3FR3CGJAMMN” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “e4f5g6h7-8i9j0-1k2l3-4m5n6-7b8c9d0e” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “tb1q9gjvavzgnm7eu3p9y9u3ek94e39999” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “p9o0i1j2-3k4l5-6m7n8-9o0a1-2b3c4d5” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` ONLINE_ID: “1BvBMSEYstWetqTFn5A” is not on the answer key
- `text/heldout-066-csv.txt` DATE_OF_BIRTH: “1986-03-28” is not on the answer key
- `text/heldout-072-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “2023-00-00” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “July 01,  ing 2024” is not on the answer key
- `text/heldout-076-customer-agreement.txt` DATE_OF_BIRTH: “July 13, 2021” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “28th day of February, 2024” is not on the answer key
- `text/heldout-077-customer-agreement.txt` ID_NUMBER: “$2,000” is not on the answer key
- `text/heldout-077-customer-agreement.txt` ID_NUMBER: “$2,000” is not on the answer key
- `text/heldout-080-customer-agreement.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-081-customer-support-conversational-log.txt` DATE_OF_BIRTH: “March 17th” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` ID_NUMBER: “ABJCDEDI492” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “220309” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “20230309” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “20230309” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “20230310” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “20230315” is not on the answer key
- `text/heldout-091-edi.txt` SECRET: “Freight Bill Number” is not on the answer key
- `text/heldout-091-edi.txt` SECRET: “Additional Reference” is not on the answer key
- `text/heldout-091-edi.txt` DATE_OF_BIRTH: “20230309” is not on the answer key
- `text/heldout-097-email.txt` COMPANY: “Facebook” is not on the answer key
- `text/heldout-097-email.txt` COMPANY: “Twitter” is not on the answer key
- `text/heldout-097-email.txt` COMPANY: “Instagram” is not on the answer key
- `text/heldout-099-email.txt` ONLINE_ID: “[Insert Link]” is not on the answer key
- `text/heldout-099-email.txt` ONLINE_ID: “[Insert Link]” is not on the answer key
- `text/heldout-099-email.txt` ONLINE_ID: “[Insert Link]” is not on the answer key
- `text/heldout-099-email.txt` ONLINE_ID: “[Insert Link]” is not on the answer key
- `text/heldout-099-email.txt` ONLINE_ID: “[Insert Link]” is not on the answer key
- `text/heldout-099-email.txt` PERSON: “[Your Name]” is not on the answer key
- `text/heldout-099-email.txt` PERSON: “[Your Title]” is not on the answer key
- `text/heldout-100-email.txt` DATE_OF_BIRTH: “Thursday, 1st of April” is not on the answer key
- `text/heldout-100-email.txt` DATE_OF_BIRTH: “10:00 am EST” is not on the answer key
- `text/heldout-101-employment-contract.txt` ADDRESS: “[Employee Address]” is not on the answer key
- `text/heldout-102-employment-contract.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-103-employment-contract.txt` DATE_OF_BIRTH: “1st day of January, 19XX” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Delaware corporation” is not on the answer key
- `text/heldout-104-employment-contract.txt` DATE_OF_BIRTH: “August 5, 2022” is not on the answer key
- `text/heldout-105-employment-contract.txt` DATE_OF_BIRTH: “2nd day of February, 2002” is not on the answer key
- `text/heldout-105-employment-contract.txt` DATE_OF_BIRTH: “February 15, 2023” is not on the answer key
- `text/heldout-105-employment-contract.txt` DATE_OF_BIRTH: “January 10, 2024” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` DATE_OF_BIRTH: “[Date of Birth]” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` PHONE: “[Contact Number]” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` EMAIL: “[Email Address]” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ID_NUMBER: “**** **** **** 568” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` ID_NUMBER: “43298715” is not on the answer key
- `text/heldout-117-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£2,500,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£150,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£50,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£200,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£1,800,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£720,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£100,000” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “£2,147,367” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “28%” is not on the answer key
- `text/heldout-124-financial-forecast.txt` ID_NUMBER: “2.3 years” is not on the answer key
- `text/heldout-125-financial-forecast.txt` DATE_OF_BIRTH: “May 15” is not on the answer key
- `text/heldout-125-financial-forecast.txt` DATE_OF_BIRTH: “June 10” is not on the answer key
- `text/heldout-125-financial-forecast.txt` DATE_OF_BIRTH: “July 20” is not on the answer key
- `text/heldout-125-financial-forecast.txt` DATE_OF_BIRTH: “May 15” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “January 1st, 2022” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “December 31st, 2022” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “January 10th, 2022” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` DATE_OF_BIRTH: “March 10, 2023” is not on the answer key
- `text/heldout-138-financial-statement.txt` COMPANY: “Collins” is not on the answer key
- `text/heldout-138-financial-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-138-financial-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-138-financial-statement.txt` DATE_OF_BIRTH: “December 34, 2020” is not on the answer key
- `text/heldout-138-financial-statement.txt` DATE_OF_BIRTH: “December 1, 2021” is not on the answer key
- `text/heldout-143-fix-protocol.txt` DATE_OF_BIRTH: “20220315-12:34:56.789” is not on the answer key
- `text/heldout-143-fix-protocol.txt` DATE_OF_BIRTH: “20220315-12:34:56.789” is not on the answer key
- `text/heldout-143-fix-protocol.txt` ID_NUMBER: “125” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “ISIN-GB00B1234567” is not on the answer key
- `text/heldout-146-fpml.txt` ID_NUMBER: “20220001” is not on the answer key
- `text/heldout-146-fpml.txt` DATE_OF_BIRTH: “2022-01-01” is not on the answer key
- `text/heldout-147-fpml.txt` DATE_OF_BIRTH: “2023-03-16” is not on the answer key
- `text/heldout-147-fpml.txt` DATE_OF_BIRTH: “2023-06-16” is not on the answer key
- `text/heldout-147-fpml.txt` DATE_OF_BIRTH: “2023-09-16” is not on the answer key
- `text/heldout-147-fpml.txt` DATE_OF_BIRTH: “2023-12-16” is not on the answer key
- `text/heldout-147-fpml.txt` SECRET: “10000000.00” is not on the answer key
- `text/heldout-148-fpml.txt` DATE_OF_BIRTH: “2022-09-01T09:30:00” is not on the answer key
- `text/heldout-148-fpml.txt` ID_NUMBER: “430” is not on the answer key
- `text/heldout-148-fpml.txt` ID_NUMBER: “430” is not on the answer key
- `text/heldout-148-fpml.txt` ID_NUMBER: “3M Libor” is not on the answer key
- `text/heldout-148-fpml.txt` DATE_OF_BIRTH: “2022-12-01” is not on the answer key
- `text/heldout-148-fpml.txt` DATE_OF_BIRTH: “2025-12-01” is not on the answer key
- `text/heldout-148-fpml.txt` ID_NUMBER: “6M” is not on the answer key
- `text/heldout-148-fpml.txt` ID_NUMBER: “10000000” is not on the answer key
- `text/heldout-150-fpml.txt` DATE_OF_BIRTH: “2022-03-01T12:34:56+00:00” is not on the answer key
- `text/heldout-150-fpml.txt` DATE_OF_BIRTH: “2022-03-01T12:34:56+00:00” is not on the answer key
- `text/heldout-150-fpml.txt` DATE_OF_BIRTH: “2022-03-01” is not on the answer key
- `text/heldout-150-fpml.txt` DATE_OF_BIRTH: “2027-03-01” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “54321” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/10/2022” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/11/2022” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/01/2023” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/05/2023” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/10/2023” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-15” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-155-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/15/2023” is not on the answer key
- `text/heldout-155-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/21/2023” is not on the answer key
- `text/heldout-157-insurance-claim-form.txt` DATE_OF_BIRTH: “05/16/2022” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` DATE_OF_BIRTH: “06/15/2023” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` DATE_OF_BIRTH: “06/20/2023” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “March 1, 2022” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “March 1, 2023 - March 15, 2023” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “February 20, 2023” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “February 22, 2023” is not on the answer key
- `text/heldout-163-insurance-policy.txt` DATE_OF_BIRTH: “August 25, 2023” is not on the answer key
- `text/heldout-164-insurance-policy.txt` DATE_OF_BIRTH: “January 1, 2023” is not on the answer key
- `text/heldout-169-investment-prospectus.txt` ONLINE_ID: “www.humanitarianfund.org” is not on the answer key
- `text/heldout-169-investment-prospectus.txt` ONLINE_ID: “www.humanitarianfund.org” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Counterparty Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` ADDRESS: “[Counterparty Address]” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Your Company Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` ADDRESS: “[Your Company Address]” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` DATE_OF_BIRTH: “03/14/2023” is not on the answer key
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` ONLINE_ID: “https://accounts.example.com/password-reset” is not on the answer key
- `text/heldout-182-loan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-182-loan-agreement.txt` ID_NUMBER: “Five Hundred Thousand Dollars ($500,000.00)” is not on the answer key
- `text/heldout-182-loan-agreement.txt` ID_NUMBER: “Five Hundred Thousand Dollars ($500,000.00)” is not on the answer key
- `text/heldout-186-loan-application.txt` DATE_OF_BIRTH: “Date: 13/05/2023” is not on the answer key
- `text/heldout-188-loan-application.txt` DATE_OF_BIRTH: “01/03/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “05/01/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “05/15/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “05/29/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “06/12/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “06/26/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “07/10/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “07/24/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “08/07/2023” is not on the answer key
- `text/heldout-191-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “08/21/2023” is not on the answer key
- `text/heldout-194-mortgage-amortization-schedule.txt` SECRET: “Credit Card Security Code:” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` ID_NUMBER: “£200,000” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “the Borrower” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “the Lender” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “the Lender” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “The Lender” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “the Borrower” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` COMPANY: “the Borrower” is not on the answer key
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “1234567890/CA001234567890” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220519123456” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220518123456” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220517123456” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220516123456” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “20220520” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “123456.78” is not on the answer key
- `text/heldout-205-mt940.txt` DATE_OF_BIRTH: “202205” is not on the answer key
- `text/heldout-207-payment-confirmation.txt` DATE_OF_BIRTH: “2022-09-14” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` ID_NUMBER: “123” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-209-payment-confirmation.txt` DATE_OF_BIRTH: “22 Jun 1977” is not on the answer key
- `text/heldout-210-payment-confirmation.txt` ID_NUMBER: “INV-2022-12345” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “Springfield” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “City” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “City” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “ERISA” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “City” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Address]” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Contact Information]” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[contact information]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Contact Information]” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-224-privacy-policy.txt` EMAIL: “[contact information]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` PERSON: “names” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “addresses” is not on the answer key
- `text/heldout-225-privacy-policy.txt` EMAIL: “email addresses” is not on the answer key
- `text/heldout-225-privacy-policy.txt` DATE_OF_BIRTH: “[date]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` ADDRESS: “______________________” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` ADDRESS: “______________________” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “BORROWER” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Bank” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FRC” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “GAAP” is not on the answer key
- `text/heldout-239-regulatory-compliance-guide.txt` DATE_OF_BIRTH: “June” is not on the answer key
- `text/heldout-242-regulatory-filing.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-243-regulatory-filing.txt` COMPANY: “AlphaHeat Heated Jacket” is not on the answer key
- `text/heldout-243-regulatory-filing.txt` DATE_OF_BIRTH: “November 15, 2021” is not on the answer key
- `text/heldout-243-regulatory-filing.txt` COMPANY: “AlphaHeat Heated Jacket” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “31 March 2022” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2023-06-15” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2023-06-16” is not on the answer key
- `text/heldout-246-renewal-reminder.txt` DATE_OF_BIRTH: “2043-06-15” is not on the answer key
- `text/heldout-248-renewal-reminder.txt` DATE_OF_BIRTH: “01 June 2023” is not on the answer key
- `text/heldout-250-renewal-reminder.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` ID_NUMBER: “$50,000,000” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` DATE_OF_BIRTH: “December 31, 2022” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` DATE_OF_BIRTH: “December 31, 2032” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` ID_NUMBER: “$1,000” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` ID_NUMBER: “US1234567890” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` DATE_OF_BIRTH: “12th day of March, 2021” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “SHAREHOLDER” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` ADDRESS: “located at ________________________” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` ADDRESS: “located at ________________________” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-269-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “thirty (30) days” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “thirty (30) days” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CDRCGB22GPT” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CHASUS33” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CHASMTON” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-272-swift-message.txt` ADDRESS: “MT300 NY washedenx” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKLMNOP” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKLMNOP” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKLMNOP” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “0123456789” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “WFIBGB2LXXX” is not on the answer key
- `text/heldout-275-swift-message.txt` DATE_OF_BIRTH: “I20221231” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “1234567890” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “CDR00001” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “CDR00001” is not on the answer key
- `text/heldout-275-swift-message.txt` COMPANY: “CCYGBP” is not on the answer key
- `text/heldout-276-tax-assessment-notice.txt` COMPANY: “REPUBLIC OF UNITED KINGDOM” is not on the answer key
- `text/heldout-277-tax-assessment-notice.txt` DATE_OF_BIRTH: “January 1, 2023” is not on the answer key
- `text/heldout-277-tax-assessment-notice.txt` DATE_OF_BIRTH: “2023-2024” is not on the answer key
- `text/heldout-278-tax-assessment-notice.txt` DATE_OF_BIRTH: “15th of March, 2023” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` DATE_OF_BIRTH: “01/04/2023 - 31/03/2024” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “AB12 CD34” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` DATE_OF_BIRTH: “01/11/2023” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax-evasion” is not on the answer key
- `text/heldout-280-tax-assessment-notice.txt` DATE_OF_BIRTH: “31st January 2024” is not on the answer key
- `text/heldout-281-tax-return.txt` ID_NUMBER: “$25,000 (401(k) RMD)” is not on the answer key
- `text/heldout-281-tax-return.txt` ID_NUMBER: “$33,300” is not on the answer key
- `text/heldout-281-tax-return.txt` ID_NUMBER: “$4,084” is not on the answer key
- `text/heldout-281-tax-return.txt` ID_NUMBER: “$3,500” is not on the answer key
- `text/heldout-281-tax-return.txt` ID_NUMBER: “$584” is not on the answer key
- `text/heldout-283-tax-return.txt` ID_NUMBER: “OVMEUSXS349” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “01/04/2021” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “15/07/2021” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “01/04/2021” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “01/07/2021” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “01/10/2021” is not on the answer key
- `text/heldout-283-tax-return.txt` DATE_OF_BIRTH: “01/01/2022” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2023-02-14” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2024-05-25” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “12/04/2023” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “12/04/2023” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “26/04/2023” is not on the answer key
- `text/heldout-289-trade-confirmation.txt` DATE_OF_BIRTH: “2023-02-14” is not on the answer key
- `text/heldout-289-trade-confirmation.txt` DATE_OF_BIRTH: “2023-02-15” is not on the answer key
- `text/heldout-289-trade-confirmation.txt` DATE_OF_BIRTH: “2024-03-20” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` DATE_OF_BIRTH: “March 15, 2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` DATE_OF_BIRTH: “June 10, 2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` EMAIL: “[Your Email]” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` PHONE: “[Your Phone Number]” is not on the answer key
- `text/heldout-291-transaction-confirmation.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` DATE_OF_BIRTH: “[Redacted]” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` DATE_OF_BIRTH: “2003-09-28” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` ID_NUMBER: “[Redacted]” is not on the answer key
- `text/heldout-293-transaction-confirmation.txt` DATE_OF_BIRTH: “2003-09-28” is not on the answer key
- `text/heldout-295-transaction-confirmation.txt` ID_NUMBER: “[Insert account number here]” is not on the answer key
- `text/heldout-295-transaction-confirmation.txt` ID_NUMBER: “[Insert sort code here]” is not on the answer key
- `text/heldout-299-xbrl.txt` DATE_OF_BIRTH: “2021-01-01” is not on the answer key
- `text/heldout-299-xbrl.txt` DATE_OF_BIRTH: “2021-01-01” is not on the answer key
- `text/heldout-299-xbrl.txt` DATE_OF_BIRTH: “2021-01-01” is not on the answer key

### gemma4:e4b

**Missed**

- `text/heldout-003-annual-report.txt` ID_NUMBER: “996238618” (1)
- `text/heldout-004-annual-report.txt` ID_NUMBER: “X-758472-D” (1)
- `text/heldout-004-annual-report.txt` PHONE: “488-430-9265” (1)
- `text/heldout-010-audit-report.txt` SECRET: “_wfy65KIEsPmYvs^1z” (1)
- `text/heldout-012-bai-format.txt` ID_NUMBER: “205932082” (1)
- `text/heldout-012-bai-format.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “976481327” (1)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “1234567890” (23)
- `text/heldout-014-bai-format.txt` ID_NUMBER: “123456” (3)
- `text/heldout-015-bai-format.txt` ID_NUMBER: “GB98 EFGH1234567890123456” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “San Francisco, CA 94112” (1)
- `text/heldout-026-business-plan.txt` ADDRESS: “79730 Justin Plaza, Jessicaberg” (1)
- `text/heldout-031-compliance-certificate.txt` ID_NUMBER: “853” (1)
- `text/heldout-033-compliance-certificate.txt` ID_NUMBER: “K8181639” (1)
- `text/heldout-033-compliance-certificate.txt` ADDRESS: “447 James Freeway” (1)
- `text/heldout-043-corporate-tax-return.txt` ID_NUMBER: “P2891850” (1)
- `text/heldout-045-corporate-tax-return.txt` ID_NUMBER: “12-3456789” (2)
- `text/heldout-045-corporate-tax-return.txt` ID_NUMBER: “987654321” (2)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “11-22-33” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Uber Eats” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Amazon” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Netflix” (1)
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Apple” (1)
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “921” (1)
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Your Bank” (1)
- `text/heldout-058-credit-card-statement.txt` ONLINE_ID: “18fd:ca22:8edb:4” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN4” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN5” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN6” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN7” (1)
- `text/heldout-072-currency-exchange-rate-sheet.txt` ID_NUMBER: “EMP672995” (1)
- `text/heldout-072-currency-exchange-rate-sheet.txt` ADDRESS: “591 Barrera Orchard” (1)
- `text/heldout-075-currency-exchange-rate-sheet.txt` ADDRESS: “348 Industrigränd, Apt. 1” (1)
- `text/heldout-084-customer-support-conversational-log.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-087-dispute-resolution-policy.txt` PERSON: “Paulino Cuenca-Contreras” (1)
- `text/heldout-087-dispute-resolution-policy.txt` ADDRESS: “77263 Cathy Landing” (1)
- `text/heldout-091-edi.txt` PERSON: “Shipper Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Postal Code” (1)
- `text/heldout-091-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Address Line 1” (1)
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Postal Code” (1)
- `text/heldout-092-edi.txt` ID_NUMBER: “MXAL72620669578837” (2)
- `text/heldout-092-edi.txt` ADDRESS: “Los Angeles*CA*90001” (1)
- `text/heldout-092-edi.txt` PHONE: “1234567890” (1)
- `text/heldout-094-edi.txt` ID_NUMBER: “W6843547” (1)
- `text/heldout-095-edi.txt` PERSON: “Consignee Name” (1)
- `text/heldout-106-financial-aid-application.txt` ID_NUMBER: “***-**-1234” (1)
- `text/heldout-107-financial-aid-application.txt` ID_NUMBER: “J-526012-P” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4220” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4221” (1)
- `text/heldout-133-financial-risk-assessment.txt` ID_NUMBER: “EMP769291” (2)
- `text/heldout-142-fix-protocol.txt` ADDRESS: “1 Hussain lake” (1)
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “Test_Username” (1)
- `text/heldout-142-fix-protocol.txt` SECRET: “Test_Password” (1)
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “24034022099218196691” (1)
- `text/heldout-145-fix-protocol.txt` ADDRESS: “Thomas Heights Apt.” (1)
- `text/heldout-147-fpml.txt` ID_NUMBER: “ZKQI65019016425115” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456-20220901” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456” (2)
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “123456789” (1)
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “MD-123456” (1)
- `text/heldout-157-insurance-claim-form.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-159-insurance-claim-form.txt` ID_NUMBER: “L-123456789” (1)
- `text/heldout-160-insurance-claim-form.txt` ID_NUMBER: “TP-1234567” (1)
- `text/heldout-165-insurance-policy.txt` ID_NUMBER: “2023-BP-12854” (1)
- `text/heldout-166-investment-prospectus.txt` ADDRESS: “5986 Williams Meadow, Apt. 956” (1)
- `text/heldout-172-isda-definition.txt` COMPANY: “London Court of International Arbitration” (2)
- `text/heldout-173-isda-definition.txt` ADDRESS: “15598 Laura Corner, Apt. 1592” (1)
- `text/heldout-174-isda-definition.txt` ID_NUMBER: “E88-7198-265-04” (1)
- `text/heldout-176-it-support-ticket.txt` ADDRESS: “81402 Jennifer Extension” (1)
- `text/heldout-177-it-support-ticket.txt` COMPANY: “IT Support Team” (1)
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft Support” (1)
- `text/heldout-194-mortgage-amortization-schedule.txt` ADDRESS: “1940 Hendricks Heights” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “12345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1312345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1412345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1512345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1612345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “1712345678901234567890” (1)
- `text/heldout-201-mt940.txt` ID_NUMBER: “18123456789” (1)
- `text/heldout-202-mt940.txt` ID_NUMBER: “191234567890” (17)
- `text/heldout-204-mt940.txt` ID_NUMBER: “1234567890123456” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “4567845674567456” (1)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “338” (1)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “7fde39a0-1d2a-4e03-8f4a-d33ae2bf8f5c” (1)
- `text/heldout-211-pension-plan-agreement.txt` ID_NUMBER: “Jp-58502” (1)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Hon” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “987654321” (1)
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “456123789” (1)
- `text/heldout-246-renewal-reminder.txt` ONLINE_ID: “f466:e5ca:ba7b:12ce:8df0:4592:b6dc:f100” (1)
- `text/heldout-252-safety-data-sheet.txt` ADDRESS: “33500 Caitlyn Fort, Apt. 2” (1)
- `text/heldout-266-supply-chain-management-agreement.txt` ID_NUMBER: “489290094” (1)
- `text/heldout-271-swift-message.txt` ID_NUMBER: “522932ABC12” (1)
- `text/heldout-272-swift-message.txt` PERSON: “SMITH, JOHN DOE” (1)
- `text/heldout-272-swift-message.txt` COMPANY: “ABC BANK NEW YORK” (3)
- `text/heldout-272-swift-message.txt` ID_NUMBER: “CDRCUS66XXX1234567890ABCDEFGHIJK” (1)
- `text/heldout-272-swift-message.txt` ID_NUMBER: “OOFFXXX0010123456ABCDEFGHIJ” (1)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “O1234567890” (2)
- `text/heldout-273-swift-message.txt` ID_NUMBER: “1234567890” (2)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “C1479015350371735835064” (1)
- `text/heldout-274-swift-message.txt` ADDRESS: “1/6 Putzweg, 43295, Füssen” (1)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “GB22ABBY09090909090909” (1)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “1234567890” (16)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “3261217076” (1)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC LIMITED” (1)
- `text/heldout-275-swift-message.txt` COMPANY: “ABC CORPORATE” (1)
- `text/heldout-275-swift-message.txt` ID_NUMBER: “GB29WFIB1234567890” (1)
- `text/heldout-276-tax-assessment-notice.txt` ID_NUMBER: “5476-6d60-7b3” (1)
- `text/heldout-277-tax-assessment-notice.txt` ID_NUMBER: “456789012” (1)
- `text/heldout-278-tax-assessment-notice.txt` ID_NUMBER: “213-76554-56” (1)
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-280-tax-assessment-notice.txt` ID_NUMBER: “2023-001234-UK” (2)
- `text/heldout-284-tax-return.txt` ID_NUMBER: “S45347676” (1)
- `text/heldout-287-trade-confirmation.txt` ID_NUMBER: “EMP406537” (1)
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “GB00B1234567” (1)
- `text/heldout-289-trade-confirmation.txt` ID_NUMBER: “P28546430” (1)
- `text/heldout-290-trade-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-290-trade-confirmation.txt` ID_NUMBER: “987654321” (1)
- `text/heldout-292-transaction-confirmation.txt` ID_NUMBER: “444” (1)
- `text/heldout-294-transaction-confirmation.txt` PERSON: “John Smith” (1)
- `text/heldout-294-transaction-confirmation.txt` COMPANY: “ABC Insurance Company” (4)
- `text/heldout-294-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-294-transaction-confirmation.txt` EMAIL: “info@abcv insurance.com” (2)
- `text/heldout-295-transaction-confirmation.txt` ONLINE_ID: “23b8:3adf:8e87:51e6:9314:3ed1:1544:4e64” (1)

**Over-redacted**

- `text/heldout-001-annual-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-007-audit-report.txt` COMPANY: “Independent Auditor” is not on the answer key
- `text/heldout-007-audit-report.txt` COMPANY: “Independent Auditor” is not on the answer key
- `text/heldout-007-audit-report.txt` ADDRESS: “EC1A 7BA” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASTOR UREÑA” is not on the answer key
- `text/heldout-011-bai-format.txt` ADDRESS: “4 CHEMIN DA COSTA, PASCAL” is not on the answer key
- `text/heldout-019-bank-statement.txt` DATE_OF_BIRTH: “01 Dec 2015” is not on the answer key
- `text/heldout-019-bank-statement.txt` DATE_OF_BIRTH: “01 Dec 2015” is not on the answer key
- `text/heldout-019-bank-statement.txt` DATE_OF_BIRTH: “31 Dec 23 2015” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “The Art of Cooking” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “The Art of Cooking” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Corporate Governance Guidelines” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Corporate Governance Guidelines” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company's” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company's” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company's” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company's” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “company policies” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “The company” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “The Board of Directors” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Compensation Committee” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “the company's” is not on the answer key
- `text/heldout-037-corporate-governance-guidelines.txt` COMPANY: “Compensation Committee” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “Corporate Governance Guidelines” is not on the answer key
- `text/heldout-039-corporate-governance-guidelines.txt` COMPANY: “Corporate Governance Guidelines” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-044-corporate-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/10/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/15/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/20/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/25/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/30/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “02/25/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/05/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/25/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “31/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “28/02/2023” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5A” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “United States Dollar” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Euro” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “British Pound” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Canadian Dollar” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Japanese Yen” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Indian Rupee” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Australian Dollar” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Chinese Yank” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` COMPANY: “Swiss Franc” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-079-customer-agreement.txt` COMPANY: “Marketing Partner” is not on the answer key
- `text/heldout-080-customer-agreement.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “Dispute Review Board” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “Dispute Review Board” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` COMPANY: “DRB” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` DATE_OF_BIRTH: “March 1, 2023” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-090-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-093-edi.txt` PERSON: “JONES^JAMES” is not on the answer key
- `text/heldout-093-edi.txt` DATE_OF_BIRTH: “19650101” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` COMPANY: “VDA” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` ID_NUMBER: “5201740824234” is not on the answer key
- `text/heldout-094-edi.txt` COMPANY: “VDA” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-100-email.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “company” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-101-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-102-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “EM” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “EM” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “em” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Employee” is not on the answer key
- `text/heldout-103-employment-contract.txt` PERSON: “Em” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company's” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company's” is not on the answer key
- `text/heldout-104-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “3 Villagatan” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-130-financial-regulatory-compliance-report.txt` COMPANY: “[Company” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “Richardshire” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “GB00B1234567” is not on the answer key
- `text/heldout-148-fpml.txt` COMPANY: “3M Libor” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “5/5 Gesche-Trüb-Platz” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “Apt. 7” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/15/2022” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “Dr” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-15” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-157-insurance-claim-form.txt` DATE_OF_BIRTH: “05/15/2022” is not on the answer key
- `text/heldout-157-insurance-claim-form.txt` DATE_OF_BIRTH: “05/16/2022” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` DATE_OF_BIRTH: “06/15/2023” is not on the answer key
- `text/heldout-158-insurance-claim-form.txt` DATE_OF_BIRTH: “06/20/2023” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “January 1, 2022” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “January 1, 2022” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “January 15, 2022” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “February 15, 2022” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` DATE_OF_BIRTH: “March 1, 2022” is not on the answer key
- `text/heldout-159-insurance-claim-form.txt` COMPANY: “[Insurance Company Name]” is not on the answer key
- `text/heldout-163-insurance-policy.txt` DATE_OF_BIRTH: “August 25, 2023” is not on the answer key
- `text/heldout-164-insurance-policy.txt` DATE_OF_BIRTH: “January 1, 2023” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` DATE_OF_BIRTH: “March 31,” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` DATE_OF_BIRTH: “2023” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Counterparty Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Your Company Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Credit Support Provider” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Credit Support User” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Credit Support User” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “Credit Support Provider” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “The Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “The Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “The Firm” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “the Counterparty” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “ISDA Master Agreement” is not on the answer key
- `text/heldout-175-isda-definition.txt` COMPANY: “Benchmark Administrator” is not on the answer key
- `text/heldout-175-isda-definition.txt` COMPANY: “Benchmark Administrator” is not on the answer key
- `text/heldout-176-it-support-ticket.txt` COMPANY: “EaseUS Data Recovery Wizard” is not on the answer key
- `text/heldout-176-it-support-ticket.txt` COMPANY: “Stellar Data Recovery” is not on the answer key
- `text/heldout-176-it-support-ticket.txt` COMPANY: “Data Recovery Team” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Sales” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Sales” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` ONLINE_ID: “https://accounts.example.com/password-reset” is not on the answer key
- `text/heldout-182-loan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-182-loan-agreement.txt` ID_NUMBER: “$500,000.00” is not on the answer key
- `text/heldout-182-loan-agreement.txt` ID_NUMBER: “$500,000.00” is not on the answer key
- `text/heldout-182-loan-agreement.txt` ID_NUMBER: “Fourteen Thousand Three Hundred Twenty-One Dollars and Fifty-E” is not on the answer key
- `text/heldout-186-loan-application.txt` ONLINE_ID: “https://www.nationaldebtline.org/” is not on the answer key
- `text/heldout-186-loan-application.txt` ONLINE_ID: “https://www.moneyadviceservice.org.uk/” is not on the answer key
- `text/heldout-188-loan-application.txt` DATE_OF_BIRTH: “01/03/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “02/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “03/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “04/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “05/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “06/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “07/01/2023” is not on the answer key
- `text/heldout-192-mortgage-amortization-schedule.txt` DATE_OF_BIRTH: “08/01/2023” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` ADDRESS: “_______________________” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Lender” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-200-mortgage-contract.txt` COMPANY: “Borrower” is not on the answer key
- `text/heldout-201-mt940.txt` PERSON: “LAURENCE T. MORENO” is not on the answer key
- `text/heldout-201-mt940.txt` ADDRESS: “51501 PAUL ESTATES, APT. 08925” is not on the answer key
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-204-mt940.txt` DATE_OF_BIRTH: “20220922163436” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220519123456” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220518123456” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220517123456” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220516123456” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “20220520” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “202205” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` DATE_OF_BIRTH: “January 1, 2022” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` DATE_OF_BIRTH: “January 15, 2022” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “PLAN” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` COMPANY: “Plan” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Local Government Pension Scheme Regulations 2013” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Trustee Act 2000” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Address]” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Contact Information]” is not on the answer key
- `text/heldout-217-policyholder-s-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-221-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-222-privacy-policy.txt` COMPANY: “Company Name” is not on the answer key
- `text/heldout-222-privacy-policy.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-224-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[address]” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities (IEO)” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities (IEO)” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-228-product-disclosure-statement.txt` COMPANY: “Technology Investment Disclosure” is not on the answer key
- `text/heldout-231-real-estate-loan-agreement.txt` COMPANY: “INTERNATIONAL REAL ESTATE INVESTMENT LOAN AGREEMENT” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Bank” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “Bank” is not on the answer key
- `text/heldout-235-real-estate-loan-agreement.txt` COMPANY: “bank” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Insurance companies” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FRC” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Health Insurance Portability and Accountability Act” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HITECH” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Health Information Technology for Economic and Clinical Health” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HITECH” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “HIPAA” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-238-regulatory-compliance-guide.txt` COMPANY: “Healthcare organizations” is not on the answer key
- `text/heldout-240-regulatory-compliance-guide.txt` COMPANY: “Aviation Compliance Guide” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-248-renewal-reminder.txt` DATE_OF_BIRTH: “01 June 2023” is not on the answer key
- `text/heldout-249-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-250-renewal-reminder.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “ISSUER” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “SHAREHOLDER” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-261-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “SHAREHOLDER” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “shareholder” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-263-shareholder-agreement.txt` COMPANY: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “SHAREHOLDER” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company's” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company's” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company's” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-264-shareholder-agreement.txt` PERSON: “Shareholder” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Corporation” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-265-shareholder-agreement.txt` COMPANY: “Shareholders” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-267-supply-chain-management-agreement.txt` COMPANY: “Supplier” is not on the answer key
- `text/heldout-269-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “CHASUS33” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “0123456789” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax-evasion” is not on the answer key
- `text/heldout-281-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` ADDRESS: “NW1 2LB” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` DATE_OF_BIRTH: “March 15, 2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key

### qwen3.6:27b

**Missed**

- `text/heldout-111-financial-data-feed.txt` COMPANY: “LONDON_EXCHANGE” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4220” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4221” (1)
- `text/heldout-114-financial-data-feed.txt` COMPANY: “Rémy” (1)
- `text/heldout-204-mt940.txt` ID_NUMBER: “C214-1412-519” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Premier Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Elite Health Insurance” (2)
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “Supreme Health Insurance” (2)
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[contact information]” (3)
- `text/heldout-247-renewal-reminder.txt` PHONE: “+XXX-XXX-XXXX” (2)
- `text/heldout-274-swift-message.txt` ID_NUMBER: “1234567890” (16)

**Over-redacted**

- `text/heldout-001-annual-report.txt` DATE_OF_BIRTH: “2022” is not on the answer key
- `text/heldout-001-annual-report.txt` DATE_OF_BIRTH: “2022” is not on the answer key
- `text/heldout-001-annual-report.txt` DATE_OF_BIRTH: “2022” is not on the answer key
- `text/heldout-001-annual-report.txt` DATE_OF_BIRTH: “2022” is not on the answer key
- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “31st December 2021” is not on the answer key
- `text/heldout-002-annual-report.txt` DATE_OF_BIRTH: “31st December 2021” is not on the answer key
- `text/heldout-003-annual-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-006-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-006-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-007-audit-report.txt` DATE_OF_BIRTH: “31 December 2021” is not on the answer key
- `text/heldout-007-audit-report.txt` DATE_OF_BIRTH: “31 December 2021” is not on the answer key
- `text/heldout-007-audit-report.txt` ADDRESS: “London, EC1A 7BA” is not on the answer key
- `text/heldout-008-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-009-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-009-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-009-audit-report.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-009-audit-report.txt` COMPANY: “American Institute of Certified Public Accountants” is not on the answer key
- `text/heldout-011-bai-format.txt` DATE_OF_BIRTH: “220223” is not on the answer key
- `text/heldout-011-bai-format.txt` DATE_OF_BIRTH: “220223” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASTOR UREÑA” is not on the answer key
- `text/heldout-011-bai-format.txt` ADDRESS: “4 CHEMIN DA COSTA” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASCAL” is not on the answer key
- `text/heldout-011-bai-format.txt` DATE_OF_BIRTH: “220223” is not on the answer key
- `text/heldout-011-bai-format.txt` DATE_OF_BIRTH: “20230222” is not on the answer key
- `text/heldout-020-bank-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “UKI-2023-001” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “VT-2309” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “ECOM-23-09-UK” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` DATE_OF_BIRTH: “15th March, 2023” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “MOBL-23-001234” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` ID_NUMBER: “CL-123456” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` DATE_OF_BIRTH: “24 Sep 1980” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` DATE_OF_BIRTH: “01/06/2023” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` PERSON: “MV Ocean Titan” is not on the answer key
- `text/heldout-023-bill-of-lading.txt` ID_NUMBER: “007-UKE-23” is not on the answer key
- `text/heldout-024-bill-of-lading.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-024-bill-of-lading.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “ABCD123456” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` DATE_OF_BIRTH: “15th June, 2023” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “EC-2345-6789” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC's” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “"The Art of Cooking"” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “"The Art of Cooking"” is not on the answer key
- `text/heldout-032-compliance-certificate.txt` DATE_OF_BIRTH: “March 15, 2023” is not on the answer key
- `text/heldout-035-compliance-certificate.txt` DATE_OF_BIRTH: “March 1, 2023” is not on the answer key
- `text/heldout-042-corporate-tax-return.txt` ID_NUMBER: “LMUGDEES017” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` DATE_OF_BIRTH: “31st of March 2022” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` DATE_OF_BIRTH: “31/03/2022” is not on the answer key
- `text/heldout-044-corporate-tax-return.txt` COMPANY: “United States of America” is not on the answer key
- `text/heldout-044-corporate-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “UNITED STATES OF AMERICA” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “DEPARTMENT OF THE TREASURY” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “INTERNAL REVENUE SERVICE” is not on the answer key
- `text/heldout-047-credit-application.txt` DATE_OF_BIRTH: “07/05/1989” is not on the answer key
- `text/heldout-047-credit-application.txt` ID_NUMBER: “EQREUSUQ895” is not on the answer key
- `text/heldout-047-credit-application.txt` DATE_OF_BIRTH: “07/05/1989” is not on the answer key
- `text/heldout-048-credit-application.txt` DATE_OF_BIRTH: “12th June 2023” is not on the answer key
- `text/heldout-048-credit-application.txt` COMPANY: “Visa” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/10/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/15/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/20/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/25/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/30/2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Netflix” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/10/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Spotify” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/15/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “New York Times” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/20/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “The Economist” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/25/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Amazon Prime” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “01/30/2023” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Duolingo Plus” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` DATE_OF_BIRTH: “02/25/2023” is not on the answer key
- `text/heldout-058-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-058-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` ID_NUMBER: “**** **** **** 4205” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/31/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/05/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “02/25/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/05/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/08/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/12/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/15/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/20/2023” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` DATE_OF_BIRTH: “01/22/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “01/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “31/01/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “28/02/2023” is not on the answer key
- `text/heldout-060-credit-card-statement.txt` DATE_OF_BIRTH: “15/01/2023” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x8f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x23e1e388e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x9f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x1f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5A” is not on the answer key
- `text/heldout-066-csv.txt` DATE_OF_BIRTH: “1986-03-28 12:05:04” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Doe” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “62701” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Smith” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “60601” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Johnson” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “90001” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Alice” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Williams” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “10001” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Brown” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “30301” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Anna” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “White” is not on the answer key
- `text/heldout-068-csv.txt` ADDRESS: “77001” is not on the answer key
- `text/heldout-070-csv.txt` DATE_OF_BIRTH: “2022-01-01 10:30:00” is not on the answer key
- `text/heldout-070-csv.txt` DATE_OF_BIRTH: “2022-01-01 10:31:00” is not on the answer key
- `text/heldout-070-csv.txt` DATE_OF_BIRTH: “2022-01-01 10:32:00” is not on the answer key
- `text/heldout-074-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “July 01,  ing 2024” is not on the answer key
- `text/heldout-075-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “2022-03-01” is not on the answer key
- `text/heldout-076-customer-agreement.txt` DATE_OF_BIRTH: “July 13, 2021” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “28th day of February, 2024” is not on the answer key
- `text/heldout-085-customer-support-conversational-log.txt` DATE_OF_BIRTH: “2022-03-15 14:32:15” is not on the answer key
- `text/heldout-086-dispute-resolution-policy.txt` DATE_OF_BIRTH: “March 1, 2023” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` ID_NUMBER: “ABJCDEDI492” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Country” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Country” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Country” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “Freight Bill Number” is not on the answer key
- `text/heldout-092-edi.txt` DATE_OF_BIRTH: “20230101” is not on the answer key
- `text/heldout-092-edi.txt` DATE_OF_BIRTH: “20230101” is not on the answer key
- `text/heldout-092-edi.txt` DATE_OF_BIRTH: “20230131” is not on the answer key
- `text/heldout-092-edi.txt` DATE_OF_BIRTH: “20230101” is not on the answer key
- `text/heldout-093-edi.txt` PERSON: “JONES^JAMES^^^^L” is not on the answer key
- `text/heldout-093-edi.txt` DATE_OF_BIRTH: “19650101” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-096-email.txt` PERSON: “[Your Name]” is not on the answer key
- `text/heldout-097-email.txt` ONLINE_ID: “Facebook” is not on the answer key
- `text/heldout-097-email.txt` ONLINE_ID: “Twitter” is not on the answer key
- `text/heldout-097-email.txt` ONLINE_ID: “Instagram” is not on the answer key
- `text/heldout-102-employment-contract.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-103-employment-contract.txt` DATE_OF_BIRTH: “1st day of January, 19XX” is not on the answer key
- `text/heldout-104-employment-contract.txt` DATE_OF_BIRTH: “August 5, 2022” is not on the answer key
- `text/heldout-105-employment-contract.txt` DATE_OF_BIRTH: “2nd day of February, 2002” is not on the answer key
- `text/heldout-109-financial-aid-application.txt` DATE_OF_BIRTH: “May 2022” is not on the answer key
- `text/heldout-109-financial-aid-application.txt` DATE_OF_BIRTH: “Fall 2022” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ID_NUMBER: “**** **** **** 568” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “3 Villagatan” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “Varberg” is not on the answer key
- `text/heldout-114-financial-data-feed.txt` ADDRESS: “43333” is not on the answer key
- `text/heldout-117-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-118-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-118-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-118-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` DATE_OF_BIRTH: “March 31, 2721” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` DATE_OF_BIRTH: “March 31, 2721” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` DATE_OF_BIRTH: “March 31, 2721” is not on the answer key
- `text/heldout-120-financial-disclosure-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-126-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “January - December 2021” is not on the answer key
- `text/heldout-126-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “January - December 2021” is not on the answer key
- `text/heldout-129-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “2021” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` DATE_OF_BIRTH: “15:56, March 10, 2023” is not on the answer key
- `text/heldout-136-financial-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-136-financial-statement.txt` ADDRESS: “Toronto, ON M1R 3V8” is not on the answer key
- `text/heldout-137-financial-statement.txt` DATE_OF_BIRTH: “December 31, 2021” is not on the answer key
- `text/heldout-139-financial-statement.txt` DATE_OF_BIRTH: “June 30, 3022” is not on the answer key
- `text/heldout-141-fix-protocol.txt` ID_NUMBER: “TradeID123456” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “ISIN-GB00B1234567” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-144-fix-protocol.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “5/5 Gesche-Trüb-Platz” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “Apt. 7” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “London” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “GB” is not on the answer key
- `text/heldout-150-fpml.txt` ADDRESS: “GB” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “54321” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/10/2022” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/11/2022” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “dr” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` DATE_OF_BIRTH: “05/15/2022” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` PERSON: “Dr” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-15” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-154-health-insurance-claim-form.txt` DATE_OF_BIRTH: “2022-03-16” is not on the answer key
- `text/heldout-155-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/15/2023” is not on the answer key
- `text/heldout-155-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/21/2023” is not on the answer key
- `text/heldout-161-insurance-policy.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-164-insurance-policy.txt` DATE_OF_BIRTH: “January 1, 2023” is not on the answer key
- `text/heldout-169-investment-prospectus.txt` ADDRESS: “0747 Taylor” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` DATE_OF_BIRTH: “March 31,” is not on the answer key
- `text/heldout-170-investment-prospectus.txt` DATE_OF_BIRTH: “2023” is not on the answer key
- `text/heldout-173-isda-definition.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-173-isda-definition.txt` PERSON: “[Counterparty Name]” is not on the answer key
- `text/heldout-173-isda-definition.txt` ADDRESS: “[Address]” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` ID_NUMBER: “IT-2023-0456” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` DATE_OF_BIRTH: “03/14/2023” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` ONLINE_ID: “<https://zoom.us/download>” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` ONLINE_ID: “<https://zoom.us/download>” is not on the answer key
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft Outlook” is not on the answer key
- `text/heldout-182-loan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-184-loan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-185-loan-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-186-loan-application.txt` DATE_OF_BIRTH: “13/05/2023” is not on the answer key
- `text/heldout-196-mortgage-contract.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-197-mortgage-contract.txt` DATE_OF_BIRTH: “1st day of August, 2021” is not on the answer key
- `text/heldout-199-mortgage-contract.txt` DATE_OF_BIRTH: “1st day of August, 2023” is not on the answer key
- `text/heldout-201-mt940.txt` ID_NUMBER: “BNKCUSUS33XXX” is not on the answer key
- `text/heldout-201-mt940.txt` ID_NUMBER: “0000123456” is not on the answer key
- `text/heldout-201-mt940.txt` PERSON: “LAURENCE T. MORENO” is not on the answer key
- `text/heldout-201-mt940.txt` ADDRESS: “51501 PAUL ESTATES, APT. 08925” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “87654321” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “GB2LXXX” is not on the answer key
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “1234567890” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “CA001234567890” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-207-payment-confirmation.txt` ID_NUMBER: “478236951” is not on the answer key
- `text/heldout-207-payment-confirmation.txt` DATE_OF_BIRTH: “2022-09-14 15:36:45” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` ID_NUMBER: “123” is not on the answer key
- `text/heldout-209-payment-confirmation.txt` DATE_OF_BIRTH: “22 Jun 1977” is not on the answer key
- `text/heldout-210-payment-confirmation.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-210-payment-confirmation.txt` ID_NUMBER: “INV-2022-12345” is not on the answer key
- `text/heldout-211-pension-plan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-213-pension-plan-agreement.txt` DATE_OF_BIRTH: “January 1, 2023” is not on the answer key
- `text/heldout-213-pension-plan-agreement.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “Springfield Municipal Employees' Reterms” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Local Government Pension Scheme” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Trustee Act 2000” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-223-privacy-policy.txt` ONLINE_ID: “IP address” is not on the answer key
- `text/heldout-223-privacy-policy.txt` ONLINE_ID: “IP address” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “Investment in Entertainment Opportunities” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-226-product-disclosure-statement.txt` COMPANY: “IEO” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Bank” is not on the answer key
- `text/heldout-233-real-estate-loan-agreement.txt` DATE_OF_BIRTH: “1st day of August, 2021” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` ADDRESS: “-63.653143, -39.337449” is not on the answer key
- `text/heldout-234-real-estate-loan-agreement.txt` ADDRESS: “-63.653143, -39.337449” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FRC” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “GAAP” is not on the answer key
- `text/heldout-239-regulatory-compliance-guide.txt` DATE_OF_BIRTH: “June” is not on the answer key
- `text/heldout-243-regulatory-filing.txt` ID_NUMBER: “2021-0087” is not on the answer key
- `text/heldout-243-regulatory-filing.txt` DATE_OF_BIRTH: “November 15, 2021” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “01 January 2021” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “31 December 2021” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “31 December 2021” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “31 December 2021” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` DATE_OF_BIRTH: “31 March 2022” is not on the answer key
- `text/heldout-248-renewal-reminder.txt` DATE_OF_BIRTH: “01 June 2023” is not on the answer key
- `text/heldout-250-renewal-reminder.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-257-securities-prospectus.txt` COMPANY: “New York Stock Exchange” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` ID_NUMBER: “US1234567890” is not on the answer key
- `text/heldout-262-shareholder-agreement.txt` DATE_OF_BIRTH: “12th day of March, 2021” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client's” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client's” is not on the answer key
- `text/heldout-266-supply-chain-management-agreement.txt` PERSON: “Client” is not on the answer key
- `text/heldout-268-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-269-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2” is not on the answer key
- `text/heldout-270-supply-chain-management-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862USD0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CDRCGB22GPT” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CHASUS33CHAS” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CHASMTON” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862USD0000000000” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “CDR0123456789” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “BCSMNY33” is not on the answer key
- `text/heldout-273-swift-message.txt` DATE_OF_BIRTH: “20220315” is not on the answer key
- `text/heldout-274-swift-message.txt` ID_NUMBER: “AAAAZPSS” is not on the answer key
- `text/heldout-274-swift-message.txt` ID_NUMBER: “ABBYGB2L” is not on the answer key
- `text/heldout-274-swift-message.txt` DATE_OF_BIRTH: “20230315” is not on the answer key
- `text/heldout-274-swift-message.txt` DATE_OF_BIRTH: “20230315” is not on the answer key
- `text/heldout-274-swift-message.txt` DATE_OF_BIRTH: “20230316” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “WFIBGB2LXXX” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “I20221231” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “1234567890” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “CDR00001” is not on the answer key
- `text/heldout-275-swift-message.txt` ID_NUMBER: “CDR00001” is not on the answer key
- `text/heldout-277-tax-assessment-notice.txt` COMPANY: “CITY OF TORONTO” is not on the answer key
- `text/heldout-278-tax-assessment-notice.txt` DATE_OF_BIRTH: “15th of March, 2023” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` DATE_OF_BIRTH: “31/03/2024” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “AB12 CD34” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` DATE_OF_BIRTH: “01/11/2023” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ONLINE_ID: “https://www.gov.uk/vehicle-tax-evasion” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` COMPANY: “Vehicle Tax Office” is not on the answer key
- `text/heldout-281-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-283-tax-return.txt` ID_NUMBER: “OVMEUSXS349” is not on the answer key
- `text/heldout-284-tax-return.txt` COMPANY: “United States Internal Revenue Service” is not on the answer key
- `text/heldout-285-tax-return.txt` ID_NUMBER: “12-3456789” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2023-02-14” is not on the answer key
- `text/heldout-286-trade-confirmation.txt` DATE_OF_BIRTH: “2024-05-25” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “12/04/2023” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “12/04/2023” is not on the answer key
- `text/heldout-287-trade-confirmation.txt` DATE_OF_BIRTH: “26/04/2023” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “001-BIO-2023-04-19-001” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` DATE_OF_BIRTH: “18/04/2023” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` DATE_OF_BIRTH: “20/04/2023” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` COMPANY: “Euroclear UK & Ireland” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` DATE_OF_BIRTH: “March 15, 2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` DATE_OF_BIRTH: “June 10, 2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-291-transaction-confirmation.txt` DATE_OF_BIRTH: “01/04/2023” is not on the answer key
- `text/heldout-294-transaction-confirmation.txt` DATE_OF_BIRTH: “01/03/2023” is not on the answer key
- `text/heldout-294-transaction-confirmation.txt` PERSON: “[Name]” is not on the answer key
- `text/heldout-295-transaction-confirmation.txt` ID_NUMBER: “001234” is not on the answer key
- `text/heldout-295-transaction-confirmation.txt` DATE_OF_BIRTH: “01/10/2022” is not on the answer key

### gemma4:31b

**Missed**

- `text/heldout-019-bank-statement.txt` ADDRESS: “2345 River Road, Apt. 091” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “San Francisco, CA 94112” (1)
- `text/heldout-058-credit-card-statement.txt` ONLINE_ID: “18fd:ca22:8edb:4” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN4” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN5” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN6” (1)
- `text/heldout-064-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN7” (1)
- `text/heldout-071-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “1994-01-31” (4)
- `text/heldout-078-customer-agreement.txt` ADDRESS: “789 Oxford Street, London, United Kingdom” (1)
- `text/heldout-092-edi.txt` ID_NUMBER: “MXAL72620669578837” (2)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4220” (1)
- `text/heldout-111-financial-data-feed.txt` ONLINE_ID: “a741:45da:c53e:2f8:835a:e766:162b:4221” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456-20220901” (1)
- `text/heldout-148-fpml.txt` ID_NUMBER: “TRA123456” (2)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “338” (1)
- `text/heldout-206-payment-confirmation.txt` ID_NUMBER: “7fde39a0-1d2a-4e03-8f4a-d33ae2bf8f5c” (1)
- `text/heldout-221-privacy-policy.txt` ADDRESS: “Noëllelaan 0” (1)
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[contact information]” (3)
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Student Housing” (1)
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ University” (3)
- `text/heldout-241-regulatory-filing.txt` DATE_OF_BIRTH: “December 24, 1977” (1)
- `text/heldout-278-tax-assessment-notice.txt` ID_NUMBER: “213-76554-56” (1)
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “GB00B1234567” (1)

**Over-redacted**

- `text/heldout-009-audit-report.txt` COMPANY: “American Institute of Certified Public Accountants” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASTOR UREÑA” is not on the answer key
- `text/heldout-011-bai-format.txt` ADDRESS: “4 CHEMIN DA COSTA” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASCAL” is not on the answer key
- `text/heldout-011-bai-format.txt` COMPANY: “CORPORATE CUSTOMER” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC's” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-029-business-plan.txt` COMPANY: “LCC” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “"The Art of Cooking"” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “"The Art of Cooking"” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` COMPANY: “The University” is not on the answer key
- `text/heldout-032-compliance-certificate.txt` ONLINE_ID: “https://pii-entity.com/394-Martha-Ramp” is not on the answer key
- `text/heldout-034-compliance-certificate.txt` COMPANY: “National Institute of Standards and Technology” is not on the answer key
- `text/heldout-034-compliance-certificate.txt` COMPANY: “NIST” is not on the answer key
- `text/heldout-042-corporate-tax-return.txt` ID_NUMBER: “LMUGDEES017” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-044-corporate-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “DEPARTMENT OF THE TREASURY” is not on the answer key
- `text/heldout-045-corporate-tax-return.txt` COMPANY: “INTERNAL REVENUE SERVICE” is not on the answer key
- `text/heldout-047-credit-application.txt` ID_NUMBER: “EQREUSUQ895” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Starbucks” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` ID_NUMBER: “**** 1234” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Netflix” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Spotify” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “New York Times” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “The Economist” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Amazon Prime” is not on the answer key
- `text/heldout-057-credit-card-statement.txt` COMPANY: “Duolingo Plus” is not on the answer key
- `text/heldout-058-credit-card-statement.txt` COMPANY: “Netflix” is not on the answer key
- `text/heldout-059-credit-card-statement.txt` ID_NUMBER: “4205” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x8f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x23e1e388e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x9f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-062-cryptocurrency-transaction-report.txt` ID_NUMBER: “0x1f933868e1e983388e8090666928768a76847b5c” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “3d567f8a-4a5d-4b6c-a9d0-e345687921f0” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “BC1QRZGE3E9N2S92Q7JYLAE3FR3CGJAMMN” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “e4f5g6h7-8i9j0-1k2l3-4m5n6-7b8c9d0e” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “tb1q9gjvavzgnm7eu3p9y9u3ek94e39999” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “p9o0i1j2-3k4l5-6m7n8-9o0a1-2b3c4d5” is not on the answer key
- `text/heldout-063-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” is not on the answer key
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5A” is not on the answer key
- `text/heldout-066-csv.txt` DATE_OF_BIRTH: “1986-03-28” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Doe” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Smith” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Johnson” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Alice” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Williams” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Brown” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “Anna” is not on the answer key
- `text/heldout-068-csv.txt` PERSON: “White” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` ID_NUMBER: “ABJCDEDI492” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-091-edi.txt` COMPANY: “Carrier” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster Pro” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-103-employment-contract.txt` COMPANY: “Company” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ID_NUMBER: “**** **** **** 568” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` COMPANY: “AAPL” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` COMPANY: “GOOGL” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` COMPANY: “AAPL” is not on the answer key
- `text/heldout-111-financial-data-feed.txt` COMPANY: “GOOGL” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “PIMIUSPB765” is not on the answer key
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “Test_Account” is not on the answer key
- `text/heldout-144-fix-protocol.txt` COMPANY: “ABC” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “54321” is not on the answer key
- `text/heldout-169-investment-prospectus.txt` ADDRESS: “0747 Taylor” is not on the answer key
- `text/heldout-174-isda-definition.txt` COMPANY: “ISDA” is not on the answer key
- `text/heldout-174-isda-definition.txt` COMPANY: “ISDA” is not on the answer key
- `text/heldout-176-it-support-ticket.txt` COMPANY: “EaseUS Data Recovery Wizard” is not on the answer key
- `text/heldout-176-it-support-ticket.txt` COMPANY: “Stellar Data Recovery” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Outlook” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Outlook” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Outlook” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Exchange” is not on the answer key
- `text/heldout-178-it-support-ticket.txt` COMPANY: “Exchange” is not on the answer key
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft Outlook” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` COMPANY: “Google Chrome” is not on the answer key
- `text/heldout-201-mt940.txt` PERSON: “LAURENCE T. MORENO” is not on the answer key
- `text/heldout-201-mt940.txt` ADDRESS: “51501 PAUL ESTATES, APT. 08925” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “87654321/GB2LXXX” is not on the answer key
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “1234567890/CA001234567890” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` ID_NUMBER: “123” is not on the answer key
- `text/heldout-213-pension-plan-agreement.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-214-pension-plan-agreement.txt` COMPANY: “Springfield Municipal Employees'” is not on the answer key
- `text/heldout-215-pension-plan-agreement.txt` COMPANY: “Local Government Pension Scheme Regulations 2013” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FCA” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “FRC” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “GAAP” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-256-securities-prospectus.txt` COMPANY: “Issuer” is not on the answer key
- `text/heldout-257-securities-prospectus.txt` COMPANY: “New York Stock Exchange” is not on the answer key
- `text/heldout-258-securities-prospectus.txt` ADDRESS: “Manchester, UK” is not on the answer key
- `text/heldout-259-securities-prospectus.txt` ID_NUMBER: “US1234567890” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862USD0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “/CDRCGB22GPT” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “/CHASUS33CHAS” is not on the answer key
- `text/heldout-271-swift-message.txt` COMPANY: “/CHASMTON” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “/1/2533072862USD0000000000” is not on the answer key
- `text/heldout-272-swift-message.txt` ADDRESS: “NY washedenx” is not on the answer key
- `text/heldout-272-swift-message.txt` COMPANY: “Washedenx” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “BCSMNY33” is not on the answer key
- `text/heldout-274-swift-message.txt` COMPANY: “ABBYGB2L” is not on the answer key
- `text/heldout-277-tax-assessment-notice.txt` COMPANY: “CITY OF TORONTO” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “AB12 CD34” is not on the answer key
- `text/heldout-279-tax-assessment-notice.txt` COMPANY: “Vehicle Tax Office” is not on the answer key
- `text/heldout-281-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-283-tax-return.txt` ID_NUMBER: “OVMEUSXS349” is not on the answer key
- `text/heldout-284-tax-return.txt` COMPANY: “United States Internal Revenue Service” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` COMPANY: “Euroclear UK & Ireland” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key

### gpt-oss

**Missed**

- `text/heldout-012-bai-format.txt` ID_NUMBER: “123456789” (1)
- `text/heldout-015-bai-format.txt` ADDRESS: “295 Ashleyhof” (4)
- `text/heldout-019-bank-statement.txt` ADDRESS: “2345 River Road, Apt. 091” (1)
- `text/heldout-019-bank-statement.txt` ADDRESS: “San Francisco, CA 94112” (1)
- `text/heldout-025-bill-of-lading.txt` ADDRESS: “841 Rios Estate, Suite 427” (2)
- `text/heldout-025-bill-of-lading.txt` ADDRESS: “Unit 7, Riverside Park
Southampton, SO15 3TG
UK” (1)
- `text/heldout-042-corporate-tax-return.txt` PERSON: “Ida” (1)
- `text/heldout-044-corporate-tax-return.txt` ADDRESS: “154” (1)
- `text/heldout-045-corporate-tax-return.txt` ID_NUMBER: “987654321” (2)
- `text/heldout-048-credit-application.txt` PERSON: “John Doe” (1)
- `text/heldout-048-credit-application.txt` ID_NUMBER: “11-22-33” (1)
- `text/heldout-052-credit-card-application.txt` COMPANY: “RockStar Musician Card” (2)
- `text/heldout-053-credit-card-application.txt` ID_NUMBER: “014” (1)
- `text/heldout-065-cryptocurrency-transaction-report.txt` ID_NUMBER: “1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2” (7)
- `text/heldout-068-csv.txt` PERSON: “John” (1)
- `text/heldout-068-csv.txt` PERSON: “Jane” (1)
- `text/heldout-068-csv.txt` PERSON: “Bob” (1)
- `text/heldout-068-csv.txt` PERSON: “Charlie” (1)
- `text/heldout-071-currency-exchange-rate-sheet.txt` DATE_OF_BIRTH: “1994-01-31” (4)
- `text/heldout-092-edi.txt` ADDRESS: “347 Beth Stream” (2)
- `text/heldout-092-edi.txt` ADDRESS: “Los Angeles*CA*90001” (1)
- `text/heldout-101-employment-contract.txt` ADDRESS: “8 Renshof” (1)
- `text/heldout-107-financial-aid-application.txt` ID_NUMBER: “J-526012-P” (1)
- `text/heldout-114-financial-data-feed.txt` COMPANY: “Rémy” (1)
- `text/heldout-122-financial-forecast.txt` ADDRESS: “290 Timothy Route” (1)
- `text/heldout-132-financial-risk-assessment.txt` PERSON: “Boman” (10)
- `text/heldout-142-fix-protocol.txt` ONLINE_ID: “Test_Username” (1)
- `text/heldout-142-fix-protocol.txt` SECRET: “Test_Password” (1)
- `text/heldout-154-health-insurance-claim-form.txt` ADDRESS: “33133” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “ABC Travel Agency” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “XYZ Airlines” (1)
- `text/heldout-160-insurance-claim-form.txt` COMPANY: “Luxury Hotels London” (1)
- `text/heldout-160-insurance-claim-form.txt` ID_NUMBER: “TP-1234567” (1)
- `text/heldout-165-insurance-policy.txt` COMPANY: “THE PACIFIC COAST INSURANCE COMPANY” (1)
- `text/heldout-165-insurance-policy.txt` COMPANY: “The Pacific Coast Insurance Company” (1)
- `text/heldout-165-insurance-policy.txt` ID_NUMBER: “2023-BP-12854” (1)
- `text/heldout-166-investment-prospectus.txt` ADDRESS: “5986 Williams Meadow, Apt. 956” (1)
- `text/heldout-176-it-support-ticket.txt` ADDRESS: “81402 Jennifer Extension” (1)
- `text/heldout-179-it-support-ticket.txt` COMPANY: “Microsoft Support” (1)
- `text/heldout-190-loan-application.txt` ADDRESS: “4896 Edward Junction
Lake Cynthia, 53799” (1)
- `text/heldout-190-loan-application.txt` COMPANY: “Lake Cynthia Community Center” (3)
- `text/heldout-207-payment-confirmation.txt` COMPANY: “Venmo” (3)
- `text/heldout-212-pension-plan-agreement.txt` PERSON: “Hon” (1)
- `text/heldout-221-privacy-policy.txt` ADDRESS: “Noëllelaan 0” (1)
- `text/heldout-223-privacy-policy.txt` COMPANY: “GeoTrack” (4)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Ofcom” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Canadian Radio-television and Telecommunications Commission (CRTC)” (1)
- `text/heldout-239-regulatory-compliance-guide.txt` COMPANY: “Advertising Standards Authority (ASA)” (1)
- `text/heldout-241-regulatory-filing.txt` DATE_OF_BIRTH: “December 24, 1977” (1)
- `text/heldout-273-swift-message.txt` ADDRESS: “065 Elizabeth Plains, Apt. 84659” (1)
- `text/heldout-279-tax-assessment-notice.txt` ID_NUMBER: “1234567890” (1)
- `text/heldout-280-tax-assessment-notice.txt` COMPANY: “HMRC” (3)
- `text/heldout-280-tax-assessment-notice.txt` ID_NUMBER: “2023-001234-UK” (2)
- `text/heldout-285-tax-return.txt` ADDRESS: “70578 Manning Grove” (1)
- `text/heldout-290-trade-confirmation.txt` ADDRESS: “Grain Elevator #3, 1234 Wheat Fields Lane, Kansas City, MO 64116” (1)
- `text/heldout-291-transaction-confirmation.txt` COMPANY: “XYZ Insurance” (4)
- `text/heldout-291-transaction-confirmation.txt` ID_NUMBER: “123456789” (1)

**Over-redacted**

- `text/heldout-001-annual-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-001-annual-report.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-003-annual-report.txt` COMPANY: “XY” is not on the answer key
- `text/heldout-011-bai-format.txt` PERSON: “PASTOR UREÑA4” is not on the answer key
- `text/heldout-011-bai-format.txt` ADDRESS: “CHEMIN DA COSTA, PASCAL” is not on the answer key
- `text/heldout-012-bai-format.txt` ADDRESS: “Mountain View” is not on the answer key
- `text/heldout-012-bai-format.txt` ADDRESS: “CA” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “UKI-2023-001” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “VT-2309” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “ECOM-23-09-UK” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` DATE_OF_BIRTH: “15th March, 2023” is not on the answer key
- `text/heldout-021-bill-of-lading.txt` ID_NUMBER: “MOBL-23-001234” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` PERSON: “[Consignee Name]” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` ADDRESS: “[Consignee Address]” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` PERSON: “[Vessel Name]” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` PERSON: “[Voyage Number]” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` PERSON: “[Port of Discharge]” is not on the answer key
- `text/heldout-022-bill-of-lading.txt` PERSON: “[Carrier Name]” is not on the answer key
- `text/heldout-025-bill-of-lading.txt` ID_NUMBER: “EC-2345-6789” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “The Art of Cooking” is not on the answer key
- `text/heldout-030-business-plan.txt` COMPANY: “The Art of Cooking” is not on the answer key
- `text/heldout-031-compliance-certificate.txt` DATE_OF_BIRTH: “31st August 2022” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-033-compliance-certificate.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-038-corporate-governance-guidelines.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-043-corporate-tax-return.txt` COMPANY: “HM Revenue and Customs” is not on the answer key
- `text/heldout-048-credit-application.txt` DATE_OF_BIRTH: “12th June 2023” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “**** 1234 (Last 4 digits)” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` COMPANY: “Starbucks” is not on the answer key
- `text/heldout-056-credit-card-statement.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` DATE_OF_BIRTH: “July 13, 2021” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-076-customer-agreement.txt` PERSON: “Software” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “1st day of March, 2023” is not on the answer key
- `text/heldout-077-customer-agreement.txt` DATE_OF_BIRTH: “28th day of February, 2024” is not on the answer key
- `text/heldout-080-customer-agreement.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-087-dispute-resolution-policy.txt` ID_NUMBER: “ABJCDEDI492” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-088-dispute-resolution-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “00501” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “000001175” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “1175” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “005010X223A1” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Shipper Country” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Consignee Country” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier City” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier State or Province” is not on the answer key
- `text/heldout-091-edi.txt` ADDRESS: “Carrier Country” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “Freight Bill Number” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “Additional Reference” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “1175” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “1175” is not on the answer key
- `text/heldout-091-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-093-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-093-edi.txt` DATE_OF_BIRTH: “19650101” is not on the answer key
- `text/heldout-093-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-093-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-093-edi.txt` ID_NUMBER: “123456” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-096-email.txt` COMPANY: “TimeMaster” is not on the answer key
- `text/heldout-100-email.txt` DATE_OF_BIRTH: “Thursday, 1st of April” is not on the answer key
- `text/heldout-100-email.txt` DATE_OF_BIRTH: “10:00 am EST” is not on the answer key
- `text/heldout-100-email.txt` COMPANY: “[Company Name] Team” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ID_NUMBER: “**** **** **** 568” is not on the answer key
- `text/heldout-110-financial-aid-application.txt` ID_NUMBER: “568” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “1BvBMSEY StuartRH323” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “SWIFT_BIC_CODE: PIMIUSPB765” is not on the answer key
- `text/heldout-119-financial-disclosure-statement.txt` ID_NUMBER: “SWIFT_BIC_CODE: PIMIUSPB765” is not on the answer key
- `text/heldout-128-financial-regulatory-compliance-report.txt` DATE_OF_BIRTH: “January 10th, 2022” is not on the answer key
- `text/heldout-134-financial-risk-assessment.txt` DATE_OF_BIRTH: “March 10, 2023” is not on the answer key
- `text/heldout-140-financial-statement.txt` COMPANY: “Richardshire's leading biotechnology firm” is not on the answer key
- `text/heldout-141-fix-protocol.txt` ID_NUMBER: “TradeID123456” is not on the answer key
- `text/heldout-144-fix-protocol.txt` COMPANY: “ABC” is not on the answer key
- `text/heldout-151-health-insurance-claim-form.txt` ID_NUMBER: “54321” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/01/2023” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/05/2023” is not on the answer key
- `text/heldout-152-health-insurance-claim-form.txt` DATE_OF_BIRTH: “03/10/2023” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-153-health-insurance-claim-form.txt` DATE_OF_BIRTH: “MM/DD/YYYY” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “March 1, 2023 - March 15, 2023” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “February 20, 2023” is not on the answer key
- `text/heldout-160-insurance-claim-form.txt` DATE_OF_BIRTH: “February 22, 2023” is not on the answer key
- `text/heldout-164-insurance-policy.txt` ID_NUMBER: “12345678” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Counterparty Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` ADDRESS: “[Counterparty Address]” is not on the answer key
- `text/heldout-171-isda-definition.txt` COMPANY: “[Your Company Name]” is not on the answer key
- `text/heldout-171-isda-definition.txt` ADDRESS: “[Your Company Address]” is not on the answer key
- `text/heldout-172-isda-definition.txt` COMPANY: “United Kingdom mail” is not on the answer key
- `text/heldout-177-it-support-ticket.txt` ID_NUMBER: “IT-2023-0456” is not on the answer key
- `text/heldout-180-it-support-ticket.txt` ONLINE_ID: “https://accounts.example.com/password-reset” is not on the answer key
- `text/heldout-182-loan-agreement.txt` DATE_OF_BIRTH: “1st day of January, 2023” is not on the answer key
- `text/heldout-186-loan-application.txt` ONLINE_ID: “https://www.nationaldebtline.org/” is not on the answer key
- `text/heldout-201-mt940.txt` ID_NUMBER: “BNKCUSUS33XXX0000123456” is not on the answer key
- `text/heldout-201-mt940.txt` PERSON: “LAURENCE T. MORENO” is not on the answer key
- `text/heldout-201-mt940.txt` COMPANY: “PAUL ESTATES” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “87654321” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “GB2LXXX” is not on the answer key
- `text/heldout-202-mt940.txt` COMPANY: “ABC TRAVEL LTD” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “1234567890” is not on the answer key
- `text/heldout-202-mt940.txt` ID_NUMBER: “CA001234567890” is not on the answer key
- `text/heldout-204-mt940.txt` ID_NUMBER: “MTXDRFTXXX000000000MLNXMONT0000000000” is not on the answer key
- `text/heldout-205-mt940.txt` ID_NUMBER: “BBBBGB2LXXX” is not on the answer key
- `text/heldout-207-payment-confirmation.txt` ID_NUMBER: “478236951” is not on the answer key
- `text/heldout-208-payment-confirmation.txt` ID_NUMBER: “123” is not on the answer key
- `text/heldout-209-payment-confirmation.txt` PERSON: “[Your Name]” is not on the answer key
- `text/heldout-209-payment-confirmation.txt` PERSON: “[Your Position]” is not on the answer key
- `text/heldout-209-payment-confirmation.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-213-pension-plan-agreement.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-216-policyholder-s-report.txt` ONLINE_ID: “Sincer01” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` ID_NUMBER: “KPEEUSZS411” is not on the answer key
- `text/heldout-219-policyholder-s-report.txt` COMPANY: “[Your Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` DATE_OF_BIRTH: “[date]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-225-privacy-policy.txt` ADDRESS: “[address]” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Tech Innovators Investment” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Tech Innovators Investment” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Tech Innovators Investment” is not on the answer key
- `text/heldout-229-product-disclosure-statement.txt` COMPANY: “Tech Innovators Investment” is not on the answer key
- `text/heldout-232-real-estate-loan-agreement.txt` COMPANY: “__________ Bank” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial” is not on the answer key
- `text/heldout-236-regulatory-compliance-guide.txt` COMPANY: “Financial Reporting Council's (FRC)” is not on the answer key
- `text/heldout-241-regulatory-filing.txt` PERSON: “Sir/Madam” is not on the answer key
- `text/heldout-241-regulatory-filing.txt` PERSON: “She” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “Regulatory Authorities” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` COMPANY: “regulatory authorities” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` PERSON: “[Your Name]” is not on the answer key
- `text/heldout-244-regulatory-filing.txt` PERSON: “[Your Position]” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` PERSON: “[Name]” is not on the answer key
- `text/heldout-245-regulatory-filing.txt` PERSON: “[Title]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-247-renewal-reminder.txt` COMPANY: “[Company Name]” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CDRCGB22GPT” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CHASUS33CHAS” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “CHASMTON” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “2533072862” is not on the answer key
- `text/heldout-271-swift-message.txt` ID_NUMBER: “0000000000” is not on the answer key
- `text/heldout-272-swift-message.txt` PERSON: “washedenx” is not on the answer key
- `text/heldout-272-swift-message.txt` PERSON: “Washedenx” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJKL” is not on the answer key
- `text/heldout-272-swift-message.txt` ID_NUMBER: “ABCDEFGHIJ” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “0123456789” is not on the answer key
- `text/heldout-273-swift-message.txt` ID_NUMBER: “BCSMNY33” is not on the answer key
- `text/heldout-274-swift-message.txt` ID_NUMBER: “AAAAZPSS” is not on the answer key
- `text/heldout-274-swift-message.txt` ID_NUMBER: “ABBYGB2L” is not on the answer key
- `text/heldout-280-tax-assessment-notice.txt` DATE_OF_BIRTH: “31st January 2024” is not on the answer key
- `text/heldout-280-tax-assessment-notice.txt` ADDRESS: “BX9 1AS” is not on the answer key
- `text/heldout-281-tax-return.txt` COMPANY: “Internal Revenue Service” is not on the answer key
- `text/heldout-283-tax-return.txt` ID_NUMBER: “OVMEUSXS349” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` ID_NUMBER: “001-BIO-2023-04-19-001” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` DATE_OF_BIRTH: “18/04/2023” is not on the answer key
- `text/heldout-288-trade-confirmation.txt` DATE_OF_BIRTH: “20/04/2023” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Buyer” is not on the answer key
- `text/heldout-290-trade-confirmation.txt` COMPANY: “Seller” is not on the answer key
- `text/heldout-294-transaction-confirmation.txt` PERSON: “[Name]” is not on the answer key

### Likely answer-key gaps

The answer key was made by the data generator, not checked by a person. Text that at least 3 of the 5 models redacted but the key does not list is more likely a gap in the key than a mistake by all of them. These are worth a look before trusting the over-redaction figures.

| Document | Text redacted | Category | Models |
|---|---:|---:|---:|
| text/heldout-043-corporate-tax-return.txt | “HM Revenue and Customs” | COMPANY | 5 |
| text/heldout-119-financial-disclosure-statement.txt | “1BvBMSEY StuartRH323” | ID_NUMBER | 5 |
| text/heldout-202-mt940.txt | “ABC TRAVEL LTD” | COMPANY | 5 |
| text/heldout-205-mt940.txt | “BBBBGB2LXXX” | ID_NUMBER | 5 |
| text/heldout-236-regulatory-compliance-guide.txt | “Financial” | COMPANY | 5 |
| text/heldout-065-cryptocurrency-transaction-report.txt | “1BvBMSEYstWetqTFn5A” | ID_NUMBER | 4 |
| text/heldout-087-dispute-resolution-policy.txt | “ABJCDEDI492” | ID_NUMBER | 4 |
| text/heldout-088-dispute-resolution-policy.txt | “[Company Name]” | COMPANY | 4 |
| text/heldout-110-financial-aid-application.txt | “**** **** **** 568” | ID_NUMBER | 4 |
| text/heldout-119-financial-disclosure-statement.txt | “PIMIUSPB765” | ID_NUMBER | 4 |
| text/heldout-151-health-insurance-claim-form.txt | “54321” | ID_NUMBER | 4 |
| text/heldout-182-loan-agreement.txt | “1st day of January, 2023” | DATE_OF_BIRTH | 4 |
| text/heldout-201-mt940.txt | “LAURENCE T. MORENO” | PERSON | 4 |
| text/heldout-204-mt940.txt | “0000000000” | ID_NUMBER | 4 |
| text/heldout-208-payment-confirmation.txt | “123” | ID_NUMBER | 4 |
| text/heldout-232-real-estate-loan-agreement.txt | “__________ Bank” | COMPANY | 4 |
| text/heldout-236-regulatory-compliance-guide.txt | “FCA” | COMPANY | 4 |
| text/heldout-236-regulatory-compliance-guide.txt | “Financial Reporting Council” | COMPANY | 4 |
| text/heldout-236-regulatory-compliance-guide.txt | “FRC” | COMPANY | 4 |
| text/heldout-281-tax-return.txt | “Internal Revenue Service” | COMPANY | 4 |
| text/heldout-283-tax-return.txt | “OVMEUSXS349” | ID_NUMBER | 4 |
| text/heldout-290-trade-confirmation.txt | “Buyer” | COMPANY | 4 |
| text/heldout-290-trade-confirmation.txt | “Seller” | COMPANY | 4 |
| text/heldout-011-bai-format.txt | “PASTOR UREÑA” | PERSON | 3 |
| text/heldout-038-corporate-governance-guidelines.txt | “[Company Name]” | COMPANY | 3 |
| text/heldout-044-corporate-tax-return.txt | “Internal Revenue Service” | COMPANY | 3 |
| text/heldout-048-credit-application.txt | “12th June 2023” | DATE_OF_BIRTH | 3 |
| text/heldout-056-credit-card-statement.txt | “**** 1234” | ID_NUMBER | 3 |
| text/heldout-057-credit-card-statement.txt | “**** 1234” | ID_NUMBER | 3 |
| text/heldout-059-credit-card-statement.txt | “02/05/2023” | DATE_OF_BIRTH | 3 |
| text/heldout-059-credit-card-statement.txt | “02/25/2023” | DATE_OF_BIRTH | 3 |
| text/heldout-060-credit-card-statement.txt | “28/02/2023” | DATE_OF_BIRTH | 3 |
| text/heldout-076-customer-agreement.txt | “July 13, 2021” | DATE_OF_BIRTH | 3 |
| text/heldout-077-customer-agreement.txt | “1st day of March, 2023” | DATE_OF_BIRTH | 3 |
| text/heldout-077-customer-agreement.txt | “28th day of February, 2024” | DATE_OF_BIRTH | 3 |
| text/heldout-080-customer-agreement.txt | “[Company Name]” | COMPANY | 3 |
| text/heldout-091-edi.txt | “Freight Bill Number” | ID_NUMBER | 3 |
| text/heldout-093-edi.txt | “19650101” | DATE_OF_BIRTH | 3 |
| text/heldout-154-health-insurance-claim-form.txt | “2022-03-15” | DATE_OF_BIRTH | 3 |
| text/heldout-154-health-insurance-claim-form.txt | “2022-03-16” | DATE_OF_BIRTH | 3 |
| text/heldout-164-insurance-policy.txt | “January 1, 2023” | DATE_OF_BIRTH | 3 |
| text/heldout-171-isda-definition.txt | “[Counterparty Name]” | COMPANY | 3 |
| text/heldout-171-isda-definition.txt | “[Your Company Name]” | COMPANY | 3 |
| text/heldout-180-it-support-ticket.txt | “https://accounts.example.com/password-reset” | ONLINE_ID | 3 |
| text/heldout-201-mt940.txt | “51501 PAUL ESTATES, APT. 08925” | ADDRESS | 3 |
| text/heldout-213-pension-plan-agreement.txt | “Internal Revenue Service” | COMPANY | 3 |
| text/heldout-219-policyholder-s-report.txt | “KPEEUSZS411” | ID_NUMBER | 3 |
| text/heldout-225-privacy-policy.txt | “[Company Name]” | COMPANY | 3 |
| text/heldout-236-regulatory-compliance-guide.txt | “GAAP” | COMPANY | 3 |
| text/heldout-247-renewal-reminder.txt | “[Company Name]” | COMPANY | 3 |
| text/heldout-248-renewal-reminder.txt | “01 June 2023” | DATE_OF_BIRTH | 3 |
| text/heldout-250-renewal-reminder.txt | “01/04/2023” | DATE_OF_BIRTH | 3 |
| text/heldout-259-securities-prospectus.txt | “US1234567890” | ID_NUMBER | 3 |
| text/heldout-269-supply-chain-management-agreement.txt | “1st day of March, 2” | DATE_OF_BIRTH | 3 |
| text/heldout-271-swift-message.txt | “0000000000” | ID_NUMBER | 3 |
| text/heldout-271-swift-message.txt | “CDRCGB22GPT” | ID_NUMBER | 3 |
| text/heldout-271-swift-message.txt | “CHASMTON” | ID_NUMBER | 3 |
| text/heldout-273-swift-message.txt | “0123456789” | ID_NUMBER | 3 |
| text/heldout-273-swift-message.txt | “BCSMNY33” | ID_NUMBER | 3 |
| text/heldout-274-swift-message.txt | “ABBYGB2L” | ID_NUMBER | 3 |

…and 4 more.

## Skipped

- `text/heldout-093-edi.txt` with phi4: Response status code does not indicate success: 500 (Internal Server Error).
- `text/heldout-173-isda-definition.txt` with phi4: The request was canceled due to the configured HttpClient.Timeout of 300 seconds elapsing.
- `text/heldout-260-securities-prospectus.txt` with phi4: The request was canceled due to the configured HttpClient.Timeout of 300 seconds elapsing.
- `text/heldout-068-csv.txt` with gemma4:e4b: Expected end of string, but instead reached end of data. LineNumber: 305 | BytePositionInLine: 156.
- `text/heldout-183-loan-agreement.txt` with gemma4:e4b: Expected end of string, but instead reached end of data. LineNumber: 5 | BytePositionInLine: 7069.
- `text/heldout-093-edi.txt` with gemma4:31b: Response status code does not indicate success: 500 (Internal Server Error).

## Settings used

- Temperature 0, seed 42, context 8192 tokens, chunks of about 4800 characters with 400 overlap.
- Reasoning (think): off (gpt-oss cannot switch it off, so it runs at its lowest level, low). Model kept loaded for 60m; each model is unloaded when its turn ends.
- Run on macOS 27.0.1, 18 cores; Ollama 0.35.0; endpoint http://localhost:11434; started 2026-10-02 18:53.
- Categories and their modes: PERSON (redact), PHONE (redact), EMAIL (redact), ADDRESS (redact), ID_NUMBER (redact), ONLINE_ID (redact), AGE (redact), DATE_OF_BIRTH (redact), GENDER (redact), COMPANY (redact), COMPANY_ID (redact), DOMAIN (redact), CONTEXTUAL (redact), LOCATION (flag only), SECRET (redact).
- Categories on: PERSON, PHONE, EMAIL, ADDRESS, ID_NUMBER, ONLINE_ID, AGE, DATE_OF_BIRTH, GENDER, COMPANY, COMPANY_ID, DOMAIN, CONTEXTUAL, LOCATION, SECRET (pronouns included).
- Flag-only (reported, not redacted, so they count as missed here): LOCATION.
- Models: rules only; GLiNER only; rules + GLiNER; phi4 (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (all flags accepted) (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (correct flags accepted) (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); gemma4:e4b (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (all flags accepted) (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (correct flags accepted) (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); qwen3.6:27b (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (all flags accepted) (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (correct flags accepted) (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); gemma4:31b (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (all flags accepted) (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (correct flags accepted) (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gpt-oss (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (all flags accepted) (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (correct flags accepted) (20.9B · MXFP4 · 128K ctx, digest 17052f91).

## What combining two models could achieve (estimate)

If a document were redacted with *both* models of a pair and everything either found were removed, an item would leak only if both missed it. This estimate counts the items missed by both, from the missed lists in this run, so it is a ceiling on the recall of a union: it ignores that the second model's different wording may also redact the same text partly. The over-redaction figure is the most extra redactions that could add up (the two models' counts summed). Only documents scored by both models are counted. The top pairs by estimated recall are shown.

| Pair | Better model alone (recall) | Both together (estimated recall) | Most extra over-redactions |
|---|---:|---:|---:|
| qwen3.6:27b + gpt-oss | 98.0% | 99.9% | 577 |
| qwen3.6:27b + gemma4:31b | 98.0% | 99.7% | 511 |
| phi4 + gemma4:31b | 98.0% | 99.7% | 677 |
| phi4 + qwen3.6:27b | 98.0% | 99.6% | 899 |
| gemma4:31b + gpt-oss | 98.0% | 99.5% | 345 |
| gemma4:e4b + gpt-oss | 94.5% | 98.9% | 704 |
| gemma4:e4b + qwen3.6:27b | 98.0% | 98.9% | 853 |
| gemma4:e4b + gemma4:31b | 97.9% | 98.8% | 630 |

## Beyond one model: combining models, and a model of our own

A finished product would not have to rely on one model. The table above shows each model's strengths and gaps, and they are not the same gaps, so combining models is a real option. There are four common ways. **Union:** run two models and redact whatever either finds. Recall rises, because an item has to be missed by both to leak, but over-redaction and run time add up. This suits a tool where a leak costs far more than an extra black box. **Agreement:** with three or more models, redact only what at least two agree on, or send the disagreements to a person; this cuts over-redaction but gives up some recall, and the app's manual review screen is already the right place for the disagreements. **Cascade:** a small, fast model reads everything, and a larger one is used only on documents or passages where the small one is unsure or found something odd. **Specialists:** reliable patterns such as emails, phone numbers, postcodes and ID formats are better found by fixed rules, which are fast and never forget, leaving the model for names, companies and the contextual judgement calls where only a language model does well.

The price of any combination is time and memory. On one machine the models run one after another, so the time is roughly the sum of the models used, and each must be loaded in turn. The estimate above is a ceiling on what a union could gain, worked out from this run's own misses; the real gain also depends on how many extra over-redactions the second model adds, which this report can only bound. Before choosing a combination, rerun the evaluation with the combination itself, since this harness scores any detector the same way.

**Could we make our own model?** Yes, in three different senses, from least to most effort. *Tune what we have* (the instructions, category descriptions and examples in the configuration): cheap, already done once, and it moved results noticeably. *Fine-tune an existing open model* on examples of documents with the sensitive items marked: this is a well-trodden technique (a light-weight method called LoRA adjusts a small fraction of the weights) and a machine like this one can plausibly train a small or mid-sized model, which can then be loaded into Ollama like any other. The hard part is not the training but the **data**: it needs hundreds to thousands of carefully marked documents that look like the clients' real ones, and the synthetic corpus here, which is deliberately simple and invented, is a useful start but could teach a model the style of our test documents rather than real documents. *Train a dedicated, much smaller entity-recognition model* (the kind used for names and places in classic language-processing tools): fast, runs on an ordinary CPU, and very good at the plain categories, but weaker than a language model at the judgement calls such as contextual identifiers. Training a large language model from scratch is not realistic for this project.

Our recommendation is to treat this as a staged question rather than a yes or no. The first step is the one under way: measure the off-the-shelf models and the combinations of them. If a gap remains in a specific category (for example contextual identifiers or place names), the next step is a small fine-tune aimed at that gap, using synthetic data plus a modest set of real, client-approved, hand-marked documents, and scored with this same harness on documents the model never saw in training. Costs to plan for are the marking effort, keeping any real client data local and out of the repository, checking that each base model's licence allows this use, and repeating the exercise whenever the base model changes. None of this needs to block the prototype: the evaluation harness is the part that makes every one of these options measurable.

## How to read this, and its limits

- **Synthetic corpus.** The documents are invented and every sensitive item is known exactly. The scores compare models fairly with each other; they are not a promise about a client's real documents, which are messier.
- **Strict recall.** An item counts as caught only when its text is gone from the redacted text. A partial redaction (for example only the surname of a full name) leaves the rest visible and counts as a miss.
- **Precision** counts a redaction as correct if it overlaps anything on the answer key, whatever its label; label accuracy is reported separately. Text the key does not list but which a person might also want removed counts against precision.
- **Scans** are read by OCR first. Words OCR misreads can't be found by the model, so scan scores mix model and OCR quality.
- Results with a local model can vary slightly between runs and machines; the temperature and seed are fixed to keep this small.
