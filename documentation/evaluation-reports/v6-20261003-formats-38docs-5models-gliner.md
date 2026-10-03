# Redaction evaluation report

Run on 2026-10-03 13:26 (macOS 27.0.1, 18 cores); took 01:47:33. 38 documents, 152 items on the answer key, 23 models. Everything ran on this machine through local models.

## Summary

| Model | Size | Recall (95% range) | Precision (95% range) | F1 | Sensitive items missed | Over-redactions | Must-keep items damaged | Time per document | Output tokens/s |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| rules only | – | 55.0% (49.8–60.1) | 100.0% (98.0–100.0) | 71.0% | 161 of 358 | 0 of 190 | 0 of 31 | 0:00.0 | 0 |
| GLiNER only | – | 86.6% (82.7–89.7) | 85.1% (81.0–88.4) | 85.8% | 48 of 358 | 53 of 355 | 9 of 31 | 0:00.0 | 0 |
| rules + GLiNER | – | 95.8% (93.2–97.4) | 86.3% (82.6–89.4) | 90.8% | 15 of 358 | 53 of 388 | 9 of 31 | 0:00.0 | 0 |
| phi4 | 14.7B · 9.1 GB | 98.2% (96.2–99.2) | 97.5% (95.2–98.7) | 97.9% | 6 of 338 | 8 of 321 | 2 of 31 | 1:32.4 | 4 |
| phi4 + GLiNER | 14.7B · 9.1 GB | 98.2% (96.2–99.2) | 97.5% (95.2–98.7) | 97.9% | 6 of 338 | 8 of 321 | 2 of 31 | 1:32.4 | 4 |
| phi4 + GLiNER (all flags accepted) | 14.7B · 9.1 GB | 99.1% (97.4–99.7) | 87.4% (83.6–90.4) | 92.9% | 3 of 338 | 46 of 366 | 5 of 31 | 1:32.4 | 4 |
| phi4 + GLiNER (correct flags accepted) | 14.7B · 9.1 GB | 99.1% (97.4–99.7) | 97.6% (95.3–98.8) | 98.3% | 3 of 338 | 8 of 328 | 2 of 31 | 1:32.4 | 4 |
| gemma4:e4b | 7.5B · 6.6 GB | 93.3% (90.2–95.5) | 94.9% (92.0–96.8) | 94.1% | 24 of 358 | 17 of 332 | 0 of 31 | 0:18.3 | 26 |
| gemma4:e4b + GLiNER | 7.5B · 6.6 GB | 93.3% (90.2–95.5) | 94.9% (92.0–96.8) | 94.1% | 24 of 358 | 17 of 332 | 0 of 31 | 0:18.4 | 26 |
| gemma4:e4b + GLiNER (all flags accepted) | 7.5B · 6.6 GB | 98.6% (96.8–99.4) | 86.4% (82.6–89.4) | 92.1% | 5 of 358 | 53 of 389 | 4 of 31 | 0:18.4 | 26 |
| gemma4:e4b + GLiNER (correct flags accepted) | 7.5B · 6.6 GB | 98.6% (96.8–99.4) | 95.2% (92.4–97.0) | 96.9% | 5 of 358 | 17 of 353 | 0 of 31 | 0:18.4 | 26 |
| qwen3.6:27b | 27.3B · 17.8 GB | 99.4% (98.0–99.8) | 89.5% (86.1–92.2) | 94.2% | 2 of 358 | 41 of 392 | 3 of 31 | 0:33.7 | 17 |
| qwen3.6:27b + GLiNER | 27.3B · 17.8 GB | 99.4% (98.0–99.8) | 89.5% (86.1–92.2) | 94.2% | 2 of 358 | 41 of 392 | 3 of 31 | 0:33.7 | 17 |
| qwen3.6:27b + GLiNER (all flags accepted) | 27.3B · 17.8 GB | 100.0% (98.9–100.0) | 83.5% (79.7–86.7) | 91.0% | 0 of 358 | 70 of 424 | 5 of 31 | 0:33.7 | 17 |
| qwen3.6:27b + GLiNER (correct flags accepted) | 27.3B · 17.8 GB | 100.0% (98.9–100.0) | 89.6% (86.2–92.3) | 94.5% | 0 of 358 | 41 of 395 | 3 of 31 | 0:33.7 | 17 |
| gemma4:31b | 30.7B · 20.4 GB | 99.2% (97.6–99.7) | 91.7% (88.5–94.1) | 95.3% | 3 of 358 | 32 of 386 | 3 of 31 | 0:11.9 | 34 |
| gemma4:31b + GLiNER | 30.7B · 20.4 GB | 99.2% (97.6–99.7) | 91.7% (88.5–94.1) | 95.3% | 3 of 358 | 32 of 386 | 3 of 31 | 0:11.9 | 34 |
| gemma4:31b + GLiNER (all flags accepted) | 30.7B · 20.4 GB | 99.7% (98.4–100.0) | 85.6% (81.9–88.7) | 92.1% | 1 of 358 | 60 of 417 | 5 of 31 | 0:11.9 | 34 |
| gemma4:31b + GLiNER (correct flags accepted) | 30.7B · 20.4 GB | 99.7% (98.4–100.0) | 91.8% (88.6–94.1) | 95.6% | 1 of 358 | 32 of 389 | 3 of 31 | 0:11.9 | 34 |
| gpt-oss | 20.9B · 13.8 GB | 98.9% (97.2–99.6) | 97.4% (95.2–98.6) | 98.1% | 4 of 358 | 9 of 348 | 0 of 31 | 0:07.4 | 106 |
| gpt-oss + GLiNER | 20.9B · 13.8 GB | 98.9% (97.2–99.6) | 97.4% (95.2–98.6) | 98.1% | 4 of 358 | 9 of 348 | 0 of 31 | 0:07.4 | 106 |
| gpt-oss + GLiNER (all flags accepted) | 20.9B · 13.8 GB | 99.4% (98.0–99.8) | 87.7% (84.1–90.6) | 93.2% | 2 of 358 | 48 of 391 | 4 of 31 | 0:07.4 | 106 |
| gpt-oss + GLiNER (correct flags accepted) | 20.9B · 13.8 GB | 99.4% (98.0–99.8) | 97.4% (95.2–98.6) | 98.4% | 2 of 358 | 9 of 352 | 0 of 31 | 0:07.4 | 106 |

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
| rules only | 38 | 0 | 0:00:00 | 0.0 s | 0.0 s (csv/06-customer-list.csv) | 0 | 0 | 190 of 328 | 97.9% | 0 | – | – |
| GLiNER only | 38 | 0 | 0:00:00 | 0.0 s | 0.0 s (csv/06-customer-list.csv) | 0 | 0 | 281 of 328 | 75.2% | 0 | – | 0.7 s |
| rules + GLiNER | 38 | 0 | 0:00:00 | 0.0 s | 0.0 s (csv/06-customer-list.csv) | 0 | 0 | 314 of 328 | 90.4% | 0 | – | 0.7 s |
| phi4 | 37 | 1 | 0:56:59 | 68.0 s | 267.7 s (scans/01-hr-letter-scan-clean.png) | 28,448 | 14,879 | 303 of 309 | 97.8% | 0 | – | – |
| phi4 + GLiNER | 37 | 0 | 0:57:00 | 68.0 s | 267.7 s (scans/01-hr-letter-scan-clean.png) | 28,448 | 14,879 | 303 of 309 | 97.8% | 0 | 45 (7) | 0.9 s |
| phi4 + GLiNER (all flags accepted) | 37 | 0 | 0:57:00 | 68.0 s | 267.7 s (scans/01-hr-letter-scan-clean.png) | 28,448 | 14,879 | 306 of 309 | 96.6% | 0 | 45 (7) | 0.9 s |
| phi4 + GLiNER (correct flags accepted) | 37 | 0 | 0:57:00 | 68.0 s | 267.7 s (scans/01-hr-letter-scan-clean.png) | 28,448 | 14,879 | 306 of 309 | 96.6% | 0 | 45 (7) | 0.9 s |
| gemma4:e4b | 38 | 0 | 0:11:39 | 11.7 s | 93.4 s (docx/02-services-agreement.docx) | 30,369 | 17,865 | 311 of 328 | 98.4% | 0 | – | – |
| gemma4:e4b + GLiNER | 38 | 0 | 0:11:40 | 11.7 s | 93.5 s (docx/02-services-agreement.docx) | 30,369 | 17,865 | 311 of 328 | 98.4% | 0 | 57 (21) | 0.9 s |
| gemma4:e4b + GLiNER (all flags accepted) | 38 | 0 | 0:11:40 | 11.7 s | 93.5 s (docx/02-services-agreement.docx) | 30,369 | 17,865 | 324 of 328 | 97.9% | 0 | 57 (21) | 0.9 s |
| gemma4:e4b + GLiNER (correct flags accepted) | 38 | 0 | 0:11:40 | 11.7 s | 93.5 s (docx/02-services-agreement.docx) | 30,369 | 17,865 | 324 of 328 | 97.9% | 0 | 57 (21) | 0.9 s |
| qwen3.6:27b | 38 | 0 | 0:21:21 | 13.0 s | 269.6 s (docx/01-hr-letter.docx) | 30,415 | 21,548 | 326 of 328 | 99.4% | 0 | – | – |
| qwen3.6:27b + GLiNER | 38 | 0 | 0:21:22 | 13.0 s | 269.6 s (docx/01-hr-letter.docx) | 30,415 | 21,548 | 326 of 328 | 99.4% | 0 | 32 (3) | 0.9 s |
| qwen3.6:27b + GLiNER (all flags accepted) | 38 | 0 | 0:21:22 | 13.0 s | 269.6 s (docx/01-hr-letter.docx) | 30,415 | 21,548 | 328 of 328 | 98.9% | 0 | 32 (3) | 0.9 s |
| qwen3.6:27b + GLiNER (correct flags accepted) | 38 | 0 | 0:21:22 | 13.0 s | 269.6 s (docx/01-hr-letter.docx) | 30,415 | 21,548 | 328 of 328 | 98.9% | 0 | 32 (3) | 0.9 s |
| gemma4:31b | 38 | 0 | 0:07:32 | 12.6 s | 34.9 s (csv/06-customer-list.csv) | 30,521 | 15,424 | 325 of 328 | 99.4% | 0 | – | – |
| gemma4:31b + GLiNER | 38 | 0 | 0:07:33 | 12.6 s | 34.9 s (csv/06-customer-list.csv) | 30,521 | 15,424 | 325 of 328 | 99.4% | 0 | 31 (3) | 0.9 s |
| gemma4:31b + GLiNER (all flags accepted) | 38 | 0 | 0:07:33 | 12.6 s | 34.9 s (csv/06-customer-list.csv) | 30,521 | 15,424 | 327 of 328 | 98.9% | 0 | 31 (3) | 0.9 s |
| gemma4:31b + GLiNER (correct flags accepted) | 38 | 0 | 0:07:33 | 12.6 s | 34.9 s (csv/06-customer-list.csv) | 30,521 | 15,424 | 327 of 328 | 98.9% | 0 | 31 (3) | 0.9 s |
| gpt-oss | 38 | 0 | 0:04:41 | 6.5 s | 29.4 s (docx/01-hr-letter.docx) | 31,327 | 29,861 | 324 of 328 | 97.1% | 0 | – | – |
| gpt-oss + GLiNER | 38 | 0 | 0:04:42 | 6.5 s | 29.4 s (docx/01-hr-letter.docx) | 31,327 | 29,861 | 324 of 328 | 97.1% | 0 | 43 (4) | 1.0 s |
| gpt-oss + GLiNER (all flags accepted) | 38 | 0 | 0:04:42 | 6.5 s | 29.4 s (docx/01-hr-letter.docx) | 31,327 | 29,861 | 326 of 328 | 96.2% | 0 | 43 (4) | 1.0 s |
| gpt-oss + GLiNER (correct flags accepted) | 38 | 0 | 0:04:42 | 6.5 s | 29.4 s (docx/01-hr-letter.docx) | 31,327 | 29,861 | 326 of 328 | 96.2% | 0 | 43 (4) | 1.0 s |

## Headline findings

- **Best at finding sensitive data:** qwen3.6:27b, removing 99.4% of items (2 missed) with 89.5% precision.
- **Best balance (F1):** gpt-oss at 98.1%. **Fastest:** gpt-oss at about 7.4 s per document.
- **Weakest category for qwen3.6:27b:** CONTEXTUAL (66.7%, 2 of 6 missed).
- **Hardest format:** Word (98.5% recall).
- **Over-redaction check:** qwen3.6:27b damaged 3 of 31 must-keep items.

## Recall by category

| Category | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| ADDRESS | 29 | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) |
| AGE | 16 | 12.5% (2/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) |
| COMPANY | 56 | 80.4% (45/56) | 96.4% (54/56) | 100.0% (52/52) | 89.3% (50/56) | 100.0% (56/56) | 100.0% (56/56) | 100.0% (56/56) |
| COMPANY_ID | 8 | 0.0% (0/8) | 75.0% (6/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) |
| CONTEXTUAL | 6 | 33.3% (2/6) | 83.3% (5/6) | 33.3% (2/6) | 33.3% (2/6) | 66.7% (4/6) | 50.0% (3/6) | 33.3% (2/6) |
| DATE_OF_BIRTH | 14 | 0.0% (0/14) | 85.7% (12/14) | 100.0% (10/10) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) |
| DOMAIN | 2 | 0.0% (0/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| EMAIL | 31 | 100.0% (31/31) | 93.5% (29/31) | 100.0% (27/27) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) |
| GENDER | 35 | 100.0% (35/35) | 60.0% (21/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) |
| ID_NUMBER | 22 | 100.0% (22/22) | 59.1% (13/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) |
| ONLINE_ID | 2 | 50.0% (1/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| PERSON | 100 | 0.0% (0/100) | 92.0% (92/100) | 97.9% (94/96) | 86.0% (86/100) | 100.0% (100/100) | 100.0% (100/100) | 100.0% (100/100) |
| PHONE | 30 | 100.0% (30/30) | 80.0% (24/30) | 100.0% (26/26) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) |
| SECRET | 7 | 0.0% (0/7) | 71.4% (5/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) |

## Recall by document format

| Format | Documents | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| PDF (text layer) | 4 | 52 | 65.4% (34/52) | 86.5% (45/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) |
| Plain text | 20 | 136 | 38.2% (52/136) | 87.5% (119/136) | 95.7% (111/116) | 83.1% (113/136) | 99.3% (135/136) | 98.5% (134/136) | 97.8% (133/136) |
| Scan: clean image | 3 | 39 | 69.2% (27/39) | 82.1% (32/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) |
| Scan: degraded image | 3 | 39 | 69.2% (27/39) | 84.6% (33/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) |
| Scan: image-only PDF | 2 | 26 | 73.1% (19/26) | 88.5% (23/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) |
| Word | 6 | 66 | 57.6% (38/66) | 87.9% (58/66) | 98.5% (65/66) | 98.5% (65/66) | 98.5% (65/66) | 98.5% (65/66) | 98.5% (65/66) |

## Recall by document type

Where each model is strong or weak, by the kind of document. Cells show recall and (items caught/items present); a dash means the type has no sensitive items on the answer key.

<details><summary>Show the table (23 document types)</summary>

| Document type | Documents | Items | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Application config (JSON) | 1 | 4 | 25.0% (1/4) | 25.0% (1/4) | 100.0% (4/4) | 100.0% (4/4) | 100.0% (4/4) | 100.0% (4/4) | 100.0% (4/4) |
| Context: company names that are also ordinary words (companies) | 1 | 6 | 0.0% (0/6) | 100.0% (6/6) | 100.0% (6/6) | 0.0% (0/6) | 100.0% (6/6) | 100.0% (6/6) | 100.0% (6/6) |
| Context: first names that are also places | 1 | 0 | – | – | – | – | – | – | – |
| Context: Jordan the country | 1 | 2 | 50.0% (1/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| Context: Jordan the person | 1 | 6 | 16.7% (1/6) | 100.0% (6/6) | 100.0% (6/6) | 50.0% (3/6) | 100.0% (6/6) | 100.0% (6/6) | 100.0% (6/6) |
| Context: MIXED - the same words as a person and as a place in one document (demo) | 1 | 5 | 0.0% (0/5) | 60.0% (3/5) | 60.0% (3/5) | 40.0% (2/5) | 100.0% (5/5) | 100.0% (5/5) | 100.0% (5/5) |
| Context: names that are also ordinary words (people) | 1 | 8 | 0.0% (0/8) | 100.0% (8/8) | 100.0% (8/8) | 0.0% (0/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) |
| Context: ordinary words that are also company names | 1 | 0 | – | – | – | – | – | – | – |
| Context: ordinary words that are also names | 1 | 0 | – | – | – | – | – | – | – |
| Context: Paris the city | 1 | 2 | 50.0% (1/2) | 50.0% (1/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| Context: Paris the person | 1 | 7 | 14.3% (1/7) | 85.7% (6/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) |
| Context: places that are also first names (people) | 1 | 8 | 0.0% (0/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) |
| Customer email thread | 2 | 20 | 50.0% (10/20) | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) |
| Customer list (CSV) | 1 | 20 | 55.0% (11/20) | 90.0% (18/20) | – | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) | 100.0% (20/20) |
| Document with nothing to redact | 2 | 0 | – | – | – | – | – | – | – |
| HR letter | 6 | 102 | 70.6% (72/102) | 87.3% (89/102) | 100.0% (102/102) | 100.0% (102/102) | 100.0% (102/102) | 100.0% (102/102) | 100.0% (102/102) |
| Incident report with credentials | 1 | 9 | 33.3% (3/9) | 66.7% (6/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) | 100.0% (9/9) |
| Invoice | 4 | 36 | 77.8% (28/36) | 91.7% (33/36) | 100.0% (36/36) | 100.0% (36/36) | 100.0% (36/36) | 100.0% (36/36) | 100.0% (36/36) |
| Meeting notes | 2 | 24 | 33.3% (8/24) | 100.0% (24/24) | 91.7% (22/24) | 91.7% (22/24) | 91.7% (22/24) | 91.7% (22/24) | 91.7% (22/24) |
| Profile with indirect identifiers | 1 | 10 | 70.0% (7/10) | 60.0% (6/10) | 80.0% (8/10) | 80.0% (8/10) | 100.0% (10/10) | 90.0% (9/10) | 80.0% (8/10) |
| Referral letter | 4 | 52 | 61.5% (32/52) | 84.6% (44/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) |
| Services agreement | 2 | 27 | 55.6% (15/27) | 81.5% (22/27) | 100.0% (27/27) | 100.0% (27/27) | 100.0% (27/27) | 100.0% (27/27) | 100.0% (27/27) |
| Word document with hidden content | 1 | 10 | 60.0% (6/10) | 70.0% (7/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) | 100.0% (10/10) |

</details>

## Precision by redaction category

| Category | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|
| ADDRESS | 100.0% (29/29) | 84.8% (28/33) | 100.0% (29/29) | 100.0% (29/29) | 93.5% (29/31) | 90.6% (29/32) | 100.0% (29/29) |
| AGE | – | 80.0% (12/15) | 80.0% (16/20) | 88.9% (16/18) | 88.9% (16/18) | 88.9% (16/18) | 88.9% (16/18) |
| COMPANY | 100.0% (40/40) | 73.3% (63/86) | 100.0% (53/53) | 100.0% (50/50) | 96.6% (56/58) | 96.6% (57/59) | 100.0% (56/56) |
| COMPANY_ID | – | 100.0% (24/24) | 100.0% (4/4) | 100.0% (6/6) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (2/2) |
| CONTEXTUAL | – | 26.3% (5/19) | 0.0% (0/1) | 10.0% (1/10) | 11.1% (2/18) | 10.5% (2/19) | 0.0% (0/4) |
| DATE_OF_BIRTH | – | 92.3% (12/13) | 83.3% (10/12) | 70.0% (14/20) | 51.7% (15/29) | 100.0% (14/14) | 100.0% (14/14) |
| DOMAIN | – | 92.9% (13/14) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| EMAIL | 100.0% (31/31) | 100.0% (16/16) | 100.0% (27/27) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) |
| GENDER | 100.0% (37/37) | 100.0% (16/16) | 100.0% (22/22) | 100.0% (21/21) | 86.8% (33/38) | 83.3% (35/42) | 100.0% (23/23) |
| ID_NUMBER | 100.0% (22/22) | 100.0% (4/4) | 100.0% (27/27) | 100.0% (24/24) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (28/28) |
| ONLINE_ID | 100.0% (1/1) | 66.7% (2/3) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 66.7% (2/3) | 75.0% (3/4) |
| PERSON | – | 95.8% (92/96) | 98.9% (88/89) | 100.0% (82/82) | 100.0% (98/98) | 100.0% (99/99) | 98.0% (98/100) |
| PHONE | 100.0% (30/30) | 100.0% (8/8) | 100.0% (26/26) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) |
| SECRET | – | 87.5% (7/8) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) |

Of the correct redactions, the share given the right category label: rules only 97.9%; GLiNER only 75.2%; phi4 97.8%; gemma4:e4b 98.4%; qwen3.6:27b 99.4%; gemma4:31b 99.4%; gpt-oss 97.1%.

## Per document

| Document | Format | Items | rules only (recall · missed · over) | GLiNER only (recall · missed · over) | phi4 (recall · missed · over) | gemma4:e4b (recall · missed · over) | qwen3.6:27b (recall · missed · over) | gemma4:31b (recall · missed · over) | gpt-oss (recall · missed · over) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| csv/06-customer-list.csv | Plain text | 20 | 55.0% · 9 · 0 | 90.0% · 2 · 1 | – | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| docx/01-hr-letter.docx | Word | 17 | 70.6% · 5 · 0 | 94.1% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 3 |
| docx/02-services-agreement.docx | Word | 14 | 57.1% · 6 · 0 | 85.7% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| docx/04-meeting-notes.docx | Word | 12 | 33.3% · 8 · 0 | 100.0% · 0 · 0 | 91.7% · 1 · 1 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 91.7% · 1 · 0 |
| docx/09-gp-referral.docx | Word | 13 | 61.5% · 5 · 0 | 84.6% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| docx/11-hard-negatives.docx | Word | 0 | – · 0 · 0 | – · 0 · 4 | – · 0 · 2 | – · 0 · 0 | – · 0 · 3 | – · 0 · 2 | – · 0 · 0 |
| docx/12-hygiene-docx.docx | Word | 10 | 60.0% · 4 · 0 | 70.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 0 |
| json/07-app-config.json | Plain text | 4 | 25.0% · 3 · 0 | 25.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| markdown/03-customer-email.md | Plain text | 10 | 50.0% · 5 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 1 |
| markdown/04-meeting-notes.md | Plain text | 12 | 33.3% · 8 · 0 | 100.0% · 0 · 0 | 91.7% · 1 · 1 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 91.7% · 1 · 0 |
| pdf/01-hr-letter.pdf | PDF (text layer) | 17 | 70.6% · 5 · 0 | 94.1% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| pdf/02-services-agreement.pdf | PDF (text layer) | 13 | 53.8% · 6 · 0 | 76.9% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| pdf/08-invoice.pdf | PDF (text layer) | 9 | 77.8% · 2 · 0 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| pdf/09-gp-referral.pdf | PDF (text layer) | 13 | 61.5% · 5 · 0 | 84.6% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 1 |
| scans/01-hr-letter-scan-clean.png | Scan: clean image | 17 | 70.6% · 5 · 0 | 76.5% · 4 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 1 |
| scans/01-hr-letter-scan-degraded.jpg | Scan: degraded image | 17 | 70.6% · 5 · 0 | 76.5% · 4 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 1 |
| scans/01-hr-letter-scan.pdf | Scan: image-only PDF | 17 | 70.6% · 5 · 0 | 88.2% · 2 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 4 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| scans/08-invoice-scan-clean.png | Scan: clean image | 9 | 77.8% · 2 · 0 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/08-invoice-scan-degraded.jpg | Scan: degraded image | 9 | 77.8% · 2 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/08-invoice-scan.pdf | Scan: image-only PDF | 9 | 77.8% · 2 · 0 | 88.9% · 1 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/09-gp-referral-scan-clean.png | Scan: clean image | 13 | 61.5% · 5 · 0 | 84.6% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| scans/09-gp-referral-scan-degraded.jpg | Scan: degraded image | 13 | 61.5% · 5 · 0 | 84.6% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| text/01-hr-letter.txt | Plain text | 17 | 70.6% · 5 · 0 | 94.1% · 1 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 2 | 100.0% · 0 · 0 |
| text/03-customer-email.txt | Plain text | 10 | 50.0% · 5 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/05-incident-report.txt | Plain text | 9 | 33.3% · 6 · 0 | 66.7% · 3 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/10-contextual-profile.txt | Plain text | 10 | 70.0% · 3 · 0 | 60.0% · 4 · 2 | 80.0% · 2 · 1 | 80.0% · 2 · 0 | 100.0% · 0 · 0 | 90.0% · 1 · 2 | 80.0% · 2 · 0 |
| text/11-hard-negatives.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 4 | – · 0 · 2 | – · 0 · 0 | – · 0 · 3 | – · 0 · 2 | – · 0 · 0 |
| text/context-text-01.txt | Plain text | 7 | 14.3% · 6 · 0 | 85.7% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-02.txt | Plain text | 2 | 50.0% · 1 · 0 | 50.0% · 1 · 4 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-03.txt | Plain text | 6 | 16.7% · 5 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 50.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-04.txt | Plain text | 2 | 50.0% · 1 · 0 | 100.0% · 0 · 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-05.txt | Plain text | 8 | 0.0% · 8 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 0.0% · 8 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-06.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 3 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/context-text-07.txt | Plain text | 6 | 0.0% · 6 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 0.0% · 6 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-08.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 5 | – · 0 · 1 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 0 |
| text/context-text-09.txt | Plain text | 8 | 0.0% · 8 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-10.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 11 | – · 0 · 0 | – · 0 · 0 | – · 0 · 3 | – · 0 · 3 | – · 0 · 0 |
| text/context-text-11-mixed.txt | Plain text | 5 | 0.0% · 5 · 0 | 60.0% · 2 · 2 | 60.0% · 2 · 0 | 40.0% · 3 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |

## Timings

Seconds for each document and model. The first figure is the model finding the sensitive items; the second is writing and verifying the redacted file (PDF render, OCR re-read of scans, and so on). Models are run one after another, every document with one model before the next model is loaded, so a model is loaded into memory once.

| Document | rules only | GLiNER only | phi4 | gemma4:e4b | qwen3.6:27b | gemma4:31b | gpt-oss |
|---|---:|---:|---:|---:|---:|---:|---:|
| csv/06-customer-list.csv | 0.0 | 0.0 | – | 30.9 + 0.0 | 167.2 + 0.0 | 34.9 + 0.0 | 13.6 + 0.0 |
| docx/01-hr-letter.docx | 0.0 | 0.0 | 248.3 + 0.0 | 79.8 + 0.0 | 269.6 + 0.0 | 20.5 + 0.0 | 29.4 + 0.0 |
| docx/02-services-agreement.docx | 0.0 | 0.0 | 68.0 + 0.0 | 93.4 + 0.0 | 202.7 + 0.0 | 16.9 + 0.0 | 11.3 + 0.0 |
| docx/04-meeting-notes.docx | 0.0 | 0.0 | 21.8 + 0.0 | 50.8 + 0.0 | 134.8 + 0.0 | 9.9 + 0.0 | 13.7 + 0.0 |
| docx/09-gp-referral.docx | 0.0 | 0.0 | 28.1 + 0.0 | 76.3 + 0.0 | 96.1 + 0.0 | 15.3 + 0.0 | 6.0 + 0.0 |
| docx/11-hard-negatives.docx | 0.0 | 0.0 | 4.7 + 0.0 | 2.4 + 0.0 | 4.2 + 0.0 | 3.0 + 0.0 | 0.3 + 0.0 |
| docx/12-hygiene-docx.docx | 0.0 | 0.0 | 19.5 + 0.0 | 47.6 + 0.0 | 10.8 + 0.0 | 9.4 + 0.0 | 21.0 + 0.0 |
| json/07-app-config.json | 0.0 | 0.0 | 13.4 + 0.0 | 24.6 + 0.0 | 7.5 + 0.0 | 9.5 + 0.0 | 3.8 + 0.0 |
| markdown/03-customer-email.md | 0.0 | 0.0 | 46.5 + 0.0 | 8.7 + 0.0 | 12.7 + 0.0 | 13.7 + 0.0 | 9.0 + 0.0 |
| markdown/04-meeting-notes.md | 0.0 | 0.0 | 97.6 + 0.0 | 7.8 + 0.0 | 12.9 + 0.0 | 10.5 + 0.0 | 17.4 + 0.0 |
| pdf/01-hr-letter.pdf | 0.0 | 0.0 | 229.0 + 0.0 | 21.3 + 0.0 | 26.5 + 0.0 | 20.5 + 0.0 | 8.3 + 0.0 |
| pdf/02-services-agreement.pdf | 0.0 | 0.0 | 131.3 + 0.0 | 12.3 + 0.0 | 15.8 + 0.0 | 14.7 + 0.0 | 8.9 + 0.0 |
| pdf/08-invoice.pdf | 0.0 | 0.0 | 194.5 + 0.0 | 13.1 + 0.0 | 14.6 + 0.0 | 12.9 + 0.0 | 7.2 + 0.0 |
| pdf/09-gp-referral.pdf | 0.0 | 0.0 | 141.5 + 0.0 | 11.3 + 0.0 | 16.6 + 0.0 | 14.6 + 0.0 | 8.0 + 0.0 |
| scans/01-hr-letter-scan-clean.png | 0.0 | 0.0 | 267.7 + 0.4 | 23.2 + 0.4 | 26.8 + 0.4 | 20.7 + 0.4 | 7.4 + 0.4 |
| scans/01-hr-letter-scan-degraded.jpg | 0.0 | 0.0 | 256.9 + 0.2 | 22.5 + 0.2 | 25.2 + 0.2 | 18.7 + 0.2 | 7.2 + 0.2 |
| scans/01-hr-letter-scan.pdf | 0.0 | 0.0 | 198.5 + 0.5 | 23.9 + 0.5 | 26.8 + 0.5 | 20.6 + 0.5 | 14.7 + 0.5 |
| scans/08-invoice-scan-clean.png | 0.0 | 0.0 | 157.9 + 0.4 | 11.8 + 0.4 | 14.6 + 0.4 | 12.9 + 0.4 | 5.5 + 0.4 |
| scans/08-invoice-scan-degraded.jpg | 0.0 | 0.0 | 137.6 + 0.2 | 13.2 + 0.2 | 14.7 + 0.2 | 12.8 + 0.2 | 6.0 + 0.2 |
| scans/08-invoice-scan.pdf | 0.0 | 0.0 | 149.3 + 0.5 | 11.8 + 0.5 | 14.7 + 0.5 | 12.9 + 0.5 | 6.1 + 0.5 |
| scans/09-gp-referral-scan-clean.png | 0.0 | 0.0 | 129.5 + 0.4 | 11.3 + 0.4 | 15.0 + 0.4 | 14.5 + 0.4 | 6.5 + 0.4 |
| scans/09-gp-referral-scan-degraded.jpg | 0.0 | 0.0 | 95.8 + 0.2 | 11.3 + 0.2 | 15.1 + 0.2 | 12.9 + 0.2 | 6.5 + 0.2 |
| text/01-hr-letter.txt | 0.0 | 0.0 | 184.3 + 0.0 | 24.1 + 0.0 | 25.9 + 0.0 | 20.4 + 0.0 | 7.8 + 0.0 |
| text/03-customer-email.txt | 0.0 | 0.0 | 121.4 + 0.0 | 11.7 + 0.0 | 12.8 + 0.0 | 12.6 + 0.0 | 4.8 + 0.0 |
| text/05-incident-report.txt | 0.0 | 0.0 | 139.8 + 0.0 | 13.0 + 0.0 | 13.0 + 0.0 | 12.3 + 0.0 | 7.5 + 0.0 |
| text/10-contextual-profile.txt | 0.0 | 0.0 | 123.7 + 0.0 | 6.6 + 0.0 | 9.7 + 0.0 | 11.0 + 0.0 | 10.2 + 0.0 |
| text/11-hard-negatives.txt | 0.0 | 0.0 | 25.8 + 0.0 | 0.5 + 0.0 | 4.3 + 0.0 | 3.0 + 0.0 | 0.3 + 0.0 |
| text/context-text-01.txt | 0.0 | 0.0 | 50.4 + 0.0 | 3.6 + 0.0 | 7.2 + 0.0 | 6.1 + 0.0 | 2.4 + 0.0 |
| text/context-text-02.txt | 0.0 | 0.0 | 14.7 + 0.0 | 3.6 + 0.0 | 5.9 + 0.0 | 5.5 + 0.0 | 2.5 + 0.0 |
| text/context-text-03.txt | 0.0 | 0.0 | 11.5 + 0.0 | 4.5 + 0.0 | 7.9 + 0.0 | 6.7 + 0.0 | 5.6 + 0.0 |
| text/context-text-04.txt | 0.0 | 0.0 | 10.9 + 0.0 | 5.1 + 0.0 | 7.8 + 0.0 | 5.9 + 0.0 | 4.5 + 0.0 |
| text/context-text-05.txt | 0.0 | 0.0 | 15.9 + 0.0 | 0.6 + 0.0 | 7.8 + 0.0 | 6.8 + 0.0 | 3.9 + 0.0 |
| text/context-text-06.txt | 0.0 | 0.0 | 0.7 + 0.0 | 0.6 + 0.0 | 1.7 + 0.0 | 2.0 + 0.0 | 0.3 + 0.0 |
| text/context-text-07.txt | 0.0 | 0.0 | 14.0 + 0.0 | 0.6 + 0.0 | 6.2 + 0.0 | 5.7 + 0.0 | 3.2 + 0.0 |
| text/context-text-08.txt | 0.0 | 0.0 | 18.0 + 0.0 | 0.6 + 0.0 | 1.7 + 0.0 | 2.0 + 0.0 | 0.3 + 0.0 |
| text/context-text-09.txt | 0.0 | 0.0 | 16.7 + 0.0 | 4.0 + 0.0 | 7.9 + 0.0 | 6.6 + 0.0 | 3.3 + 0.0 |
| text/context-text-10.txt | 0.0 | 0.0 | 18.0 + 0.0 | 4.9 + 0.0 | 7.8 + 0.0 | 6.5 + 0.0 | 4.2 + 0.0 |
| text/context-text-11-mixed.txt | 0.0 | 0.0 | 16.0 + 0.0 | 6.0 + 0.0 | 8.9 + 0.0 | 7.5 + 0.0 | 2.8 + 0.0 |
| **Total** | **0.0 + 0.0** | **0.7 + 0.0** | **3419.2 + 2.8** | **699.1 + 2.7** | **1281.5 + 2.7** | **452.8 + 2.7** | **281.5 + 2.8** |
| Average per document | 0.0 | 0.0 | 92.4 | 18.4 | 33.7 | 11.9 | 7.4 |

## What was missed and over-redacted
*Contains text from the documents.*

### rules only

**Missed**

- `csv/06-customer-list.csv` PERSON: “Eleanor Whitcombe” (1)
- `csv/06-customer-list.csv` DATE_OF_BIRTH: “1986-03-12” (1)
- `csv/06-customer-list.csv` PERSON: “Tomasz Kowalczyk” (1)
- `csv/06-customer-list.csv` DATE_OF_BIRTH: “1964-07-30” (1)
- `csv/06-customer-list.csv` PERSON: “Aisha Rahman” (1)
- `csv/06-customer-list.csv` DATE_OF_BIRTH: “1991-11-02” (1)
- `csv/06-customer-list.csv` PERSON: “Jonas Eriksen” (1)
- `csv/06-customer-list.csv` DATE_OF_BIRTH: “1978-01-19” (1)
- `csv/06-customer-list.csv` COMPANY: “Nordlys Consulting AS” (1)
- `docx/01-hr-letter.docx` PERSON: “Eleanor Whitcombe” (1)
- `docx/01-hr-letter.docx` PERSON: “Whitcombe” (1)
- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “12 March 1986” (1)
- `docx/01-hr-letter.docx` AGE: “39” (1)
- `docx/01-hr-letter.docx` PERSON: “Marcus Delaney” (1)
- `docx/02-services-agreement.docx` COMPANY_ID: “09876543” (1)
- `docx/02-services-agreement.docx` COMPANY_ID: “GB 123 4567 89” (1)
- `docx/02-services-agreement.docx` PERSON: “Priya Natarajan” (1)
- `docx/02-services-agreement.docx` PERSON: “Marcus Delaney” (1)
- `docx/02-services-agreement.docx` DOMAIN: “www.corvid-logistics.example” (1)
- `docx/02-services-agreement.docx` PERSON: “Marcus Delaney” (1)
- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `docx/04-meeting-notes.docx` PERSON: “Priya Natarajan” (1)
- `docx/04-meeting-notes.docx` PERSON: “Marcus Delaney” (1)
- `docx/04-meeting-notes.docx` PERSON: “Jonas Eriksen” (1)
- `docx/04-meeting-notes.docx` COMPANY: “Nordlys Consulting AS” (1)
- `docx/04-meeting-notes.docx` PERSON: “Jonas” (1)
- `docx/04-meeting-notes.docx` PERSON: “Priya” (1)
- `docx/04-meeting-notes.docx` COMPANY: “Nordlys” (1)
- `docx/09-gp-referral.docx` PERSON: “Tomasz Kowalczyk” (1)
- `docx/09-gp-referral.docx` AGE: “61” (2)
- `docx/09-gp-referral.docx` DATE_OF_BIRTH: “30 July 1964” (1)
- `docx/09-gp-referral.docx` PERSON: “Dr Helen Okafor” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Gareth Lloyd” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Priya Natarajan” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Dev Patel” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Priya Natarajan” (1)
- `json/07-app-config.json` SECRET: “Tr1cky-Gl4cier!” (1)
- `json/07-app-config.json` SECRET: “sk-test-4f9a1c7e2b8d4a6f9c3e5b7a1d2c4e6f” (1)
- `json/07-app-config.json` PERSON: “Dev Patel” (1)
- `markdown/03-customer-email.md` PERSON: “Tomasz Kowalczyk” (2)
- `markdown/03-customer-email.md` SECRET: “Winter-Harbour-42!” (1)
- `markdown/03-customer-email.md` PERSON: “Aisha Rahman” (1)
- `markdown/03-customer-email.md` PERSON: “Kowalczyk” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` PERSON: “Priya Natarajan” (1)
- `markdown/04-meeting-notes.md` PERSON: “Marcus Delaney” (1)
- `markdown/04-meeting-notes.md` PERSON: “Jonas Eriksen” (1)
- `markdown/04-meeting-notes.md` COMPANY: “Nordlys Consulting AS” (1)
- `markdown/04-meeting-notes.md` PERSON: “Jonas” (1)
- `markdown/04-meeting-notes.md` PERSON: “Priya” (1)
- `markdown/04-meeting-notes.md` COMPANY: “Nordlys” (1)
- `pdf/01-hr-letter.pdf` PERSON: “Eleanor Whitcombe” (1)
- `pdf/01-hr-letter.pdf` PERSON: “Whitcombe” (1)
- `pdf/01-hr-letter.pdf` DATE_OF_BIRTH: “12 March 1986” (1)
- `pdf/01-hr-letter.pdf` AGE: “39” (1)
- `pdf/01-hr-letter.pdf` PERSON: “Marcus Delaney” (1)
- `pdf/02-services-agreement.pdf` COMPANY_ID: “09876543” (1)
- `pdf/02-services-agreement.pdf` COMPANY_ID: “GB 123 4567 89” (1)
- `pdf/02-services-agreement.pdf` PERSON: “Priya Natarajan” (1)
- `pdf/02-services-agreement.pdf` PERSON: “Marcus Delaney” (1)
- `pdf/02-services-agreement.pdf` DOMAIN: “www.corvid-logistics.example” (1)
- `pdf/02-services-agreement.pdf` PERSON: “Marcus Delaney” (1)
- `pdf/08-invoice.pdf` COMPANY_ID: “GB 987 6543 21” (1)
- `pdf/08-invoice.pdf` PERSON: “Priya Natarajan” (1)
- `pdf/09-gp-referral.pdf` PERSON: “Tomasz Kowalczyk” (1)
- `pdf/09-gp-referral.pdf` AGE: “61” (2)
- `pdf/09-gp-referral.pdf` DATE_OF_BIRTH: “30 July 1964” (1)
- `pdf/09-gp-referral.pdf` PERSON: “Dr Helen Okafor” (1)
- `scans/01-hr-letter-scan-clean.png` PERSON: “Eleanor Whitcombe” (1)
- `scans/01-hr-letter-scan-clean.png` PERSON: “Whitcombe” (1)
- `scans/01-hr-letter-scan-clean.png` DATE_OF_BIRTH: “12 March 1986” (1)
- `scans/01-hr-letter-scan-clean.png` AGE: “39” (1)
- `scans/01-hr-letter-scan-clean.png` PERSON: “Marcus Delaney” (1)
- `scans/01-hr-letter-scan-degraded.jpg` PERSON: “Eleanor Whitcombe” (1)
- `scans/01-hr-letter-scan-degraded.jpg` PERSON: “Whitcombe” (1)
- `scans/01-hr-letter-scan-degraded.jpg` DATE_OF_BIRTH: “12 March 1986” (1)
- `scans/01-hr-letter-scan-degraded.jpg` AGE: “39” (1)
- `scans/01-hr-letter-scan-degraded.jpg` PERSON: “Marcus Delaney” (1)
- `scans/01-hr-letter-scan.pdf` PERSON: “Eleanor Whitcombe” (1)
- `scans/01-hr-letter-scan.pdf` PERSON: “Whitcombe” (1)
- `scans/01-hr-letter-scan.pdf` DATE_OF_BIRTH: “12 March 1986” (1)
- `scans/01-hr-letter-scan.pdf` AGE: “39” (1)
- `scans/01-hr-letter-scan.pdf` PERSON: “Marcus Delaney” (1)
- `scans/08-invoice-scan-clean.png` COMPANY_ID: “GB 987 6543 21” (1)
- `scans/08-invoice-scan-clean.png` PERSON: “Priya Natarajan” (1)
- `scans/08-invoice-scan-degraded.jpg` COMPANY_ID: “GB 987 6543 21” (1)
- `scans/08-invoice-scan-degraded.jpg` PERSON: “Priya Natarajan” (1)
- `scans/08-invoice-scan.pdf` COMPANY_ID: “GB 987 6543 21” (1)
- `scans/08-invoice-scan.pdf` PERSON: “Priya Natarajan” (1)
- `scans/09-gp-referral-scan-clean.png` PERSON: “Tomasz Kowalczyk” (1)
- `scans/09-gp-referral-scan-clean.png` AGE: “61” (2)
- `scans/09-gp-referral-scan-clean.png` DATE_OF_BIRTH: “30 July 1964” (1)
- `scans/09-gp-referral-scan-clean.png` PERSON: “Dr Helen Okafor” (1)
- `scans/09-gp-referral-scan-degraded.jpg` PERSON: “Tomasz Kowalczyk” (1)
- `scans/09-gp-referral-scan-degraded.jpg` AGE: “61” (2)
- `scans/09-gp-referral-scan-degraded.jpg` DATE_OF_BIRTH: “30 July 1964” (1)
- `scans/09-gp-referral-scan-degraded.jpg` PERSON: “Dr Helen Okafor” (1)
- `text/01-hr-letter.txt` PERSON: “Eleanor Whitcombe” (1)
- `text/01-hr-letter.txt` PERSON: “Whitcombe” (1)
- `text/01-hr-letter.txt` DATE_OF_BIRTH: “12 March 1986” (1)
- `text/01-hr-letter.txt` AGE: “39” (1)
- `text/01-hr-letter.txt` PERSON: “Marcus Delaney” (1)
- `text/03-customer-email.txt` PERSON: “Tomasz Kowalczyk” (2)
- `text/03-customer-email.txt` SECRET: “Winter-Harbour-42!” (1)
- `text/03-customer-email.txt` PERSON: “Aisha Rahman” (1)
- `text/03-customer-email.txt` PERSON: “Kowalczyk” (1)
- `text/05-incident-report.txt` PERSON: “Dev Patel” (2)
- `text/05-incident-report.txt` SECRET: “Server=db01.internal;User Id=svc_reports;Password=Tr1cky-Gl4cier!” (1)
- `text/05-incident-report.txt` SECRET: “AKIAIOSFODNN7EXAMPLE” (1)
- `text/05-incident-report.txt` ONLINE_ID: “@dpatel_dev” (1)
- `text/05-incident-report.txt` SECRET: “eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJkZW1vIn0.c2lnbmF0dXJl” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)
- `text/10-contextual-profile.txt` PERSON: “Gareth Lloyd” (1)
- `text/context-text-01.txt` PERSON: “Jane” (3)
- `text/context-text-01.txt` PERSON: “Paris” (3)
- `text/context-text-02.txt` PERSON: “Jane” (1)
- `text/context-text-03.txt` PERSON: “Jordan” (4)
- `text/context-text-03.txt` PERSON: “Dana” (1)
- `text/context-text-04.txt` PERSON: “Dana” (1)
- `text/context-text-05.txt` PERSON: “Will” (2)
- `text/context-text-05.txt` PERSON: “Mark” (2)
- `text/context-text-05.txt` PERSON: “Rose” (1)
- `text/context-text-05.txt` PERSON: “Bill” (1)
- `text/context-text-05.txt` PERSON: “Grace” (1)
- `text/context-text-05.txt` PERSON: “Hope” (1)
- `text/context-text-07.txt` COMPANY: “Apple” (2)
- `text/context-text-07.txt` COMPANY: “Shell” (2)
- `text/context-text-07.txt` COMPANY: “Amazon” (1)
- `text/context-text-07.txt` COMPANY: “Target” (1)
- `text/context-text-09.txt` PERSON: “Georgia” (2)
- `text/context-text-09.txt` PERSON: “Chelsea” (2)
- `text/context-text-09.txt` PERSON: “Florence” (2)
- `text/context-text-09.txt` PERSON: “Victoria” (2)
- `text/context-text-11-mixed.txt` PERSON: “Paris” (2)
- `text/context-text-11-mixed.txt` PERSON: “Jane” (1)
- `text/context-text-11-mixed.txt` PERSON: “Jordan” (1)
- `text/context-text-11-mixed.txt` PERSON: “Georgia” (1)

**Over-redacted**

- nothing

### GLiNER only

**Missed**

- `csv/06-customer-list.csv` EMAIL: “t.kowalczyk@example.org” (1)
- `csv/06-customer-list.csv` EMAIL: “j.eriksen@nordlys.example” (1)
- `docx/01-hr-letter.docx` ID_NUMBER: “QQ 12 34 56 C” (1)
- `docx/02-services-agreement.docx` COMPANY_ID: “GB 123 4567 89” (1)
- `docx/02-services-agreement.docx` ID_NUMBER: “GB82 WEST 1234 5698 7654 32” (1)
- `docx/09-gp-referral.docx` GENDER: “man” (1)
- `docx/09-gp-referral.docx` GENDER: “He” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Gareth Lloyd” (1)
- `docx/12-hygiene-docx.docx` PERSON: “Dev Patel” (1)
- `docx/12-hygiene-docx.docx` GENDER: “his” (1)
- `json/07-app-config.json` SECRET: “Tr1cky-Gl4cier!” (1)
- `json/07-app-config.json` SECRET: “sk-test-4f9a1c7e2b8d4a6f9c3e5b7a1d2c4e6f” (1)
- `json/07-app-config.json` PERSON: “Dev Patel” (1)
- `pdf/01-hr-letter.pdf` ID_NUMBER: “QQ 12 34 56 C” (1)
- `pdf/02-services-agreement.pdf` COMPANY_ID: “GB 123 4567 89” (1)
- `pdf/02-services-agreement.pdf` PERSON: “Marcus Delaney” (1)
- `pdf/02-services-agreement.pdf` PERSON: “Marcus Delaney” (1)
- `pdf/08-invoice.pdf` ID_NUMBER: “GB82 WEST 1234 5698 7654 32” (1)
- `pdf/09-gp-referral.pdf` GENDER: “man” (1)
- `pdf/09-gp-referral.pdf` GENDER: “He” (1)
- `scans/01-hr-letter-scan-clean.png` PHONE: “0113 496 0123” (1)
- `scans/01-hr-letter-scan-clean.png` DATE_OF_BIRTH: “12 March 1986” (1)
- `scans/01-hr-letter-scan-clean.png` ID_NUMBER: “QQ 12 34 56 C” (1)
- `scans/01-hr-letter-scan-clean.png` PHONE: “07700 900123” (1)
- `scans/01-hr-letter-scan-degraded.jpg` PHONE: “0113 496 0123” (1)
- `scans/01-hr-letter-scan-degraded.jpg` DATE_OF_BIRTH: “12 March 1986” (1)
- `scans/01-hr-letter-scan-degraded.jpg` ID_NUMBER: “QQ 12 34 56 C” (1)
- `scans/01-hr-letter-scan-degraded.jpg` PHONE: “07700 900123” (1)
- `scans/01-hr-letter-scan.pdf` ID_NUMBER: “QQ 12 34 56 C” (1)
- `scans/01-hr-letter-scan.pdf` PHONE: “07700 900123” (1)
- `scans/08-invoice-scan-clean.png` COMPANY: “Fernleigh Parcels Ltd” (1)
- `scans/08-invoice-scan.pdf` ID_NUMBER: “GB82 WEST 1234 5698 7654 32” (1)
- `scans/09-gp-referral-scan-clean.png` GENDER: “man” (1)
- `scans/09-gp-referral-scan-clean.png` GENDER: “He” (1)
- `scans/09-gp-referral-scan-degraded.jpg` GENDER: “man” (1)
- `scans/09-gp-referral-scan-degraded.jpg` GENDER: “He” (1)
- `text/01-hr-letter.txt` ID_NUMBER: “QQ 12 34 56 C” (1)
- `text/05-incident-report.txt` PERSON: “Dev Patel” (1)
- `text/05-incident-report.txt` COMPANY: “Brightwater Analytics Ltd” (1)
- `text/05-incident-report.txt` PHONE: “01632 960001” (1)
- `text/10-contextual-profile.txt` GENDER: “She” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)
- `text/10-contextual-profile.txt` GENDER: “she” (1)
- `text/10-contextual-profile.txt` GENDER: “Her” (1)
- `text/context-text-01.txt` GENDER: “her” (1)
- `text/context-text-02.txt` GENDER: “her” (1)
- `text/context-text-11-mixed.txt` PERSON: “Paris” (2)

**Over-redacted**

- `csv/06-customer-list.csv` ADDRESS: “date_of_birth” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/09-gp-referral.docx` CONTEXTUAL: “bus driver” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “HMRC” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “committee” is not on the answer key
- `docx/11-hard-negatives.docx` CONTEXTUAL: “revenue” is not on the answer key
- `markdown/03-customer-email.md` DATE_OF_BIRTH: “Tuesday” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `pdf/09-gp-referral.pdf` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan.pdf` ONLINE_ID: “Tel” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/09-gp-referral-scan-clean.png` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/09-gp-referral-scan-degraded.jpg` CONTEXTUAL: “bus driver” is not on the answer key
- `text/01-hr-letter.txt` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key
- `text/05-incident-report.txt` PERSON: “engineer” is not on the answer key
- `text/10-contextual-profile.txt` CONTEXTUAL: “regional director” is not on the answer key
- `text/10-contextual-profile.txt` COMPANY: “Bristol office” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “HMRC” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “committee” is not on the answer key
- `text/11-hard-negatives.txt` CONTEXTUAL: “revenue” is not on the answer key
- `text/context-text-01.txt` COMPANY: “Kestrel” is not on the answer key
- `text/context-text-01.txt` COMPANY: “design team” is not on the answer key
- `text/context-text-02.txt` CONTEXTUAL: “Kestrel travel” is not on the answer key
- `text/context-text-02.txt` COMPANY: “office” is not on the answer key
- `text/context-text-02.txt` COMPANY: “France” is not on the answer key
- `text/context-text-03.txt` COMPANY: “finance team” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Regional expansion plan” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Jordan” is not on the answer key
- `text/context-text-04.txt` COMPANY: “suppliers” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Jordan” is not on the answer key
- `text/context-text-04.txt` COMPANY: “regional office” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Jordan” is not on the answer key
- `text/context-text-06.txt` COMPANY: “Committee” is not on the answer key
- `text/context-text-06.txt` COMPANY: “committee” is not on the answer key
- `text/context-text-06.txt` ADDRESS: “entrance” is not on the answer key
- `text/context-text-07.txt` DOMAIN: “Parcels” is not on the answer key
- `text/context-text-08.txt` PERSON: “children” is not on the answer key
- `text/context-text-08.txt` COMPANY: “Amazon” is not on the answer key
- `text/context-text-08.txt` COMPANY: “apple tree” is not on the answer key
- `text/context-text-10.txt` COMPANY: “offsite” is not on the answer key
- `text/context-text-10.txt` PERSON: “Chelsea” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Georgia” is not on the answer key
- `text/context-text-10.txt` PERSON: “Coaches” is not on the answer key
- `text/context-text-10.txt` COMPANY: “Chelsea” is not on the answer key
- `text/context-text-10.txt` AGE: “nine” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-11-mixed.txt` SECRET: “MIXED” is not on the answer key
- `text/context-text-11-mixed.txt` COMPANY: “Project Kestrel” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “HMRC”
- `text/11-hard-negatives.txt` should have been kept: “HMRC”
- `text/context-text-02.txt` should have been kept: “France”
- `text/context-text-04.txt` should have been kept: “Jordan”
- `text/context-text-08.txt` should have been kept: “apple”
- `text/context-text-08.txt` should have been kept: “Amazon”
- `text/context-text-10.txt` should have been kept: “Chelsea”
- `text/context-text-10.txt` should have been kept: “Victoria”
- `text/context-text-10.txt` should have been kept: “Georgia”

### phi4

**Missed**

- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)
- `text/context-text-11-mixed.txt` PERSON: “Paris” (1)
- `text/context-text-11-mixed.txt` PERSON: “Jordan” (1)

**Over-redacted**

- `docx/04-meeting-notes.docx` DATE_OF_BIRTH: “in January” is not on the answer key
- `docx/11-hard-negatives.docx` AGE: “42” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `markdown/04-meeting-notes.md` DATE_OF_BIRTH: “in January” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key
- `text/10-contextual-profile.txt` PERSON: “regional director” is not on the answer key
- `text/11-hard-negatives.txt` AGE: “42” is not on the answer key
- `text/context-text-08.txt` CONTEXTUAL: “a school trip” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “42”
- `text/11-hard-negatives.txt` should have been kept: “42”

### gemma4:e4b

**Missed**

- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)
- `text/context-text-03.txt` PERSON: “Jordan” (3)
- `text/context-text-05.txt` PERSON: “Will” (2)
- `text/context-text-05.txt` PERSON: “Mark” (2)
- `text/context-text-05.txt` PERSON: “Rose” (1)
- `text/context-text-05.txt` PERSON: “Bill” (1)
- `text/context-text-05.txt` PERSON: “Grace” (1)
- `text/context-text-05.txt` PERSON: “Hope” (1)
- `text/context-text-07.txt` COMPANY: “Apple” (2)
- `text/context-text-07.txt` COMPANY: “Shell” (2)
- `text/context-text-07.txt` COMPANY: “Amazon” (1)
- `text/context-text-07.txt` COMPANY: “Target” (1)
- `text/context-text-11-mixed.txt` PERSON: “Paris” (2)
- `text/context-text-11-mixed.txt` PERSON: “Jordan” (1)

**Over-redacted**

- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Head of People” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `pdf/01-hr-letter.pdf` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan.pdf` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Head of People” is not on the answer key
- `text/01-hr-letter.txt` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key

### qwen3.6:27b

**Missed**

- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)

**Over-redacted**

- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “1 November” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Head of People” is not on the answer key
- `docx/09-gp-referral.docx` GENDER: “he” is not on the answer key
- `docx/09-gp-referral.docx` CONTEXTUAL: “bus driver” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “HMRC” is not on the answer key
- `docx/11-hard-negatives.docx` DATE_OF_BIRTH: “March” is not on the answer key
- `docx/12-hygiene-docx.docx` GENDER: “his” is not on the answer key
- `docx/12-hygiene-docx.docx` DATE_OF_BIRTH: “31 December” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `pdf/01-hr-letter.pdf` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `pdf/01-hr-letter.pdf` DATE_OF_BIRTH: “1 November” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Head of People” is not on the answer key
- `pdf/09-gp-referral.pdf` GENDER: “he” is not on the answer key
- `pdf/09-gp-referral.pdf` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` DATE_OF_BIRTH: “1 November” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` DATE_OF_BIRTH: “1 November” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan.pdf` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan.pdf` DATE_OF_BIRTH: “1 November” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/09-gp-referral-scan-clean.png` GENDER: “he” is not on the answer key
- `scans/09-gp-referral-scan-clean.png` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/09-gp-referral-scan-degraded.jpg` GENDER: “he” is not on the answer key
- `scans/09-gp-referral-scan-degraded.jpg` CONTEXTUAL: “bus driver” is not on the answer key
- `text/01-hr-letter.txt` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `text/01-hr-letter.txt` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `text/01-hr-letter.txt` CONTEXTUAL: “Head of People” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “HMRC” is not on the answer key
- `text/11-hard-negatives.txt` DATE_OF_BIRTH: “March” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “HMRC”
- `text/11-hard-negatives.txt` should have been kept: “HMRC”
- `text/context-text-10.txt` should have been kept: “Victoria”

### gemma4:31b

**Missed**

- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)

**Over-redacted**

- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Head of People” is not on the answer key
- `docx/09-gp-referral.docx` GENDER: “he” is not on the answer key
- `docx/09-gp-referral.docx` CONTEXTUAL: “bus driver” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “HMRC” is not on the answer key
- `docx/12-hygiene-docx.docx` GENDER: “his” is not on the answer key
- `json/07-app-config.json` ONLINE_ID: “svc_reports” is not on the answer key
- `markdown/03-customer-email.md` GENDER: “I'm” is not on the answer key
- `markdown/03-customer-email.md` GENDER: “I'm” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `pdf/01-hr-letter.pdf` CONTEXTUAL: “Head of People” is not on the answer key
- `pdf/09-gp-referral.pdf` GENDER: “he” is not on the answer key
- `pdf/09-gp-referral.pdf` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `scans/01-hr-letter-scan.pdf` CONTEXTUAL: “Head of People” is not on the answer key
- `scans/09-gp-referral-scan-clean.png` GENDER: “he” is not on the answer key
- `scans/09-gp-referral-scan-clean.png` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/09-gp-referral-scan-degraded.jpg` GENDER: “he” is not on the answer key
- `scans/09-gp-referral-scan-degraded.jpg` CONTEXTUAL: “bus driver” is not on the answer key
- `text/01-hr-letter.txt` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `text/01-hr-letter.txt` CONTEXTUAL: “Head of People” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key
- `text/10-contextual-profile.txt` CONTEXTUAL: “regional director” is not on the answer key
- `text/10-contextual-profile.txt` ADDRESS: “Bristol office” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “HMRC” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “HMRC”
- `text/11-hard-negatives.txt` should have been kept: “HMRC”
- `text/context-text-10.txt` should have been kept: “Victoria”

### gpt-oss

**Missed**

- `docx/04-meeting-notes.docx` CONTEXTUAL: “Kestrel” (1)
- `markdown/04-meeting-notes.md` CONTEXTUAL: “Kestrel” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)

**Over-redacted**

- `docx/01-hr-letter.docx` CONTEXTUAL: “promotion” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “promotion to Senior Data Scientist” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Head of People” is not on the answer key
- `json/07-app-config.json` ONLINE_ID: “svc_reports” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `pdf/09-gp-referral.pdf` CONTEXTUAL: “bus driver” is not on the answer key
- `scans/01-hr-letter-scan-clean.png` PERSON: “Head of People” is not on the answer key
- `scans/01-hr-letter-scan-degraded.jpg` PERSON: “Head of People” is not on the answer key
- `text/03-customer-email.txt` AGE: “61” is not on the answer key

### Likely answer-key gaps

The answer key was made by the data generator, not checked by a person. Text that at least 3 of the 5 models redacted but the key does not list is more likely a gap in the key than a mistake by all of them. These are worth a look before trusting the over-redaction figures.

| Document | Text redacted | Category | Models |
|---|---:|---:|---:|
| markdown/03-customer-email.md | “61” | AGE | 5 |
| text/03-customer-email.txt | “61” | AGE | 5 |
| docx/01-hr-letter.docx | “Head of People” | CONTEXTUAL | 4 |
| scans/01-hr-letter-scan-clean.png | “Head of People” | CONTEXTUAL | 4 |
| scans/01-hr-letter-scan-degraded.jpg | “Head of People” | CONTEXTUAL | 4 |
| docx/01-hr-letter.docx | “Senior Data Scientist” | CONTEXTUAL | 3 |
| pdf/01-hr-letter.pdf | “Senior Data Scientist” | CONTEXTUAL | 3 |
| pdf/09-gp-referral.pdf | “bus driver” | CONTEXTUAL | 3 |
| scans/01-hr-letter-scan-clean.png | “Senior Data Scientist” | CONTEXTUAL | 3 |
| scans/01-hr-letter-scan-degraded.jpg | “Senior Data Scientist” | CONTEXTUAL | 3 |
| scans/01-hr-letter-scan.pdf | “Senior Data Scientist” | CONTEXTUAL | 3 |
| scans/01-hr-letter-scan.pdf | “Head of People” | CONTEXTUAL | 3 |

## Output safety

Each redacted file was also written and passed through the tool's own checks (no text layer in PDFs, nothing recoverable in Word files, OCR re-read of scans). A file that fails is refused rather than written.

| Model | Files written and verified | Refused or failed |
|---|---:|---:|
| phi4 | 37 of 37 | none |
| gemma4:e4b | 38 of 38 | none |
| qwen3.6:27b | 38 of 38 | none |
| gemma4:31b | 38 of 38 | none |
| gpt-oss | 38 of 38 | none |

## Skipped

- `csv/06-customer-list.csv` with phi4: The request was canceled due to the configured HttpClient.Timeout of 300 seconds elapsing.

## Settings used

- Temperature 0, seed 42, context 8192 tokens, chunks of about 4800 characters with 400 overlap.
- Reasoning (think): off (gpt-oss cannot switch it off, so it runs at its lowest level, low). Model kept loaded for 60m; each model is unloaded when its turn ends.
- Run on macOS 27.0.1, 18 cores; Ollama 0.35.1; endpoint http://localhost:11434; started 2026-10-03 13:26.
- Categories and their modes: PERSON (redact), PHONE (redact), EMAIL (redact), ADDRESS (redact), ID_NUMBER (redact), ONLINE_ID (redact), AGE (redact), DATE_OF_BIRTH (redact), GENDER (redact), COMPANY (redact), COMPANY_ID (redact), DOMAIN (redact), CONTEXTUAL (redact), LOCATION (flag only), SECRET (redact).
- Categories on: PERSON, PHONE, EMAIL, ADDRESS, ID_NUMBER, ONLINE_ID, AGE, DATE_OF_BIRTH, GENDER, COMPANY, COMPANY_ID, DOMAIN, CONTEXTUAL, LOCATION, SECRET (pronouns included).
- Flag-only (reported, not redacted, so they count as missed here): LOCATION.
- Models: rules only; GLiNER only; rules + GLiNER; phi4 (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (all flags accepted) (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER (correct flags accepted) (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); gemma4:e4b (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (all flags accepted) (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); gemma4:e4b + GLiNER (correct flags accepted) (7.5B · Q4_K_M · 128K ctx, digest dc35e8d9); qwen3.6:27b (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (all flags accepted) (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER (correct flags accepted) (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); gemma4:31b (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (all flags accepted) (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gemma4:31b + GLiNER (correct flags accepted) (30.7B · Q4_K_M · 256K ctx, digest 17ba34c0); gpt-oss (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (all flags accepted) (20.9B · MXFP4 · 128K ctx, digest 17052f91); gpt-oss + GLiNER (correct flags accepted) (20.9B · MXFP4 · 128K ctx, digest 17052f91).

## What combining two models could achieve (estimate)

If a document were redacted with *both* models of a pair and everything either found were removed, an item would leak only if both missed it. This estimate counts the items missed by both, from the missed lists in this run, so it is a ceiling on the recall of a union: it ignores that the second model's different wording may also redact the same text partly. The over-redaction figure is the most extra redactions that could add up (the two models' counts summed). Only documents scored by both models are counted. The top pairs by estimated recall are shown.

| Pair | Better model alone (recall) | Both together (estimated recall) | Most extra over-redactions |
|---|---:|---:|---:|
| qwen3.6:27b + gpt-oss | 99.4% | 99.4% | 50 |
| gemma4:e4b + qwen3.6:27b | 99.4% | 99.4% | 58 |
| qwen3.6:27b + gemma4:31b | 99.4% | 99.4% | 73 |
| phi4 + qwen3.6:27b | 99.4% | 99.4% | 49 |
| gemma4:31b + gpt-oss | 99.2% | 99.2% | 41 |
| gemma4:e4b + gemma4:31b | 99.2% | 99.2% | 49 |
| phi4 + gemma4:31b | 99.1% | 99.1% | 40 |
| gemma4:e4b + gpt-oss | 98.9% | 98.9% | 26 |

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
