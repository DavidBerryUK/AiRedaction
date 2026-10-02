# Redaction evaluation report

Run on 2026-10-02 18:04 (macOS 27.0.1, 18 cores); took 00:15:30. 38 documents, 152 items on the answer key, 6 models. Everything ran on this machine through local models.

## Summary

| Model | Size | Recall | Precision | F1 | Sensitive items missed | Over-redactions | Must-keep items damaged | Time per document | Output tokens/s |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| phi4 | 14.7B · 9.1 GB | 98.3% | 97.7% | 98.0% | 6 of 358 | 8 of 341 | 2 of 31 | 0:08.9 | 48 |
| phi4 + GLiNER | ? | 98.3% | 97.7% | 98.0% | 6 of 358 | 8 of 341 | 2 of 31 | 0:09.0 | 48 |
| phi4 + GLiNER (flags accepted) | ? | 99.2% | 87.9% | 93.2% | 3 of 358 | 47 of 387 | 5 of 31 | 0:09.0 | 48 |
| qwen3.6:27b | 27.3B · 17.8 GB | 99.4% | 89.5% | 94.2% | 2 of 358 | 41 of 392 | 3 of 31 | 0:15.4 | 37 |
| qwen3.6:27b + GLiNER | ? | 99.4% | 89.5% | 94.2% | 2 of 358 | 41 of 392 | 3 of 31 | 0:15.4 | 37 |
| qwen3.6:27b + GLiNER (flags accepted) | ? | 100.0% | 83.5% | 91.0% | 0 of 358 | 70 of 424 | 5 of 31 | 0:15.4 | 37 |

### What the columns mean

- **Recall** answers: *of everything that should have been hidden, how much did the model hide?* If a document has 100 sensitive items and the model hides 95, recall is 95%. The other 5 are leaks, so for a redaction tool this is the most important number. It is strict: hiding only the surname of "Jane Smith" leaves the first name visible and counts as a miss.
- **Precision** answers: *of everything the model hid, how much really needed hiding?* If it hides 100 things and 90 were sensitive, precision is 90%. The other 10 are over-redactions: harmless, but they make the document harder to read.
- **F1** is a single score that blends recall and precision. It is high only when both are high, so a model cannot score well by hiding everything (perfect recall, poor precision) or by hiding almost nothing (high precision, poor recall). Use it for a quick ranking, but look at recall first.
- **Sensitive items missed** is the count behind recall ("3 of 120" means 3 sensitive items were left visible).
- **Over-redactions** is the count behind precision ("8 of 130" means 8 of the 130 redactions covered text that did not need hiding).
- **Must-keep items damaged** (also called *preserved*) checks the opposite risk. Each test document contains ordinary text that must survive, such as dates, job titles, amounts, product names and general places. This counts how many of those were wrongly removed. "0 of 40" is ideal, and each one damaged is information the reader needed that is now gone.
- **Time per document** is the average wall-clock time to redact one document, and **Output tokens/s** is how fast the model writes its answer (a hardware and model-size measure).

## Detail per model

Where each model's time and effort went, and how many documents it could not process. *Items fully caught* counts whole items (a full name is one item) rather than every occurrence; *label accuracy* is the share of correct redactions given the right category; *lost to OCR* counts items the scan reader never produced, which no model could have found. For rows that include **GLiNER**, *flagged for review* counts things only GLiNER found, which are left in the text for a person to check, with how many of them really were sensitive. A row marked *(flags accepted)* shows the result if the reviewer accepted every flag.

| Model | Documents scored | Documents failed | Total model time | Median document | Slowest document | Prompt tokens | Output tokens | Items fully caught | Label accuracy | Lost to OCR | Flagged for review (really sensitive) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| phi4 | 38 | 0 | 0:05:41 | 8.1 s | 28.6 s (csv/06-customer-list.csv) | 29,241 | 16,306 | 322 of 328 | 97.9% | 0 | – |
| phi4 + GLiNER | 38 | 0 | 0:05:42 | 8.1 s | 28.6 s (csv/06-customer-list.csv) | 29,241 | 16,306 | 322 of 328 | 97.9% | 0 | 46 (7) |
| phi4 + GLiNER (flags accepted) | 38 | 0 | 0:05:42 | 8.1 s | 28.6 s (csv/06-customer-list.csv) | 29,241 | 16,306 | 325 of 328 | 96.8% | 0 | 46 (7) |
| qwen3.6:27b | 38 | 0 | 0:09:47 | 14.9 s | 33.0 s (csv/06-customer-list.csv) | 30,415 | 21,548 | 326 of 328 | 99.4% | 0 | – |
| qwen3.6:27b + GLiNER | 38 | 0 | 0:09:48 | 14.9 s | 33.0 s (csv/06-customer-list.csv) | 30,415 | 21,548 | 326 of 328 | 99.4% | 0 | 32 (3) |
| qwen3.6:27b + GLiNER (flags accepted) | 38 | 0 | 0:09:48 | 14.9 s | 33.0 s (csv/06-customer-list.csv) | 30,415 | 21,548 | 328 of 328 | 98.9% | 0 | 32 (3) |

## Headline findings

- **Best at finding sensitive data:** qwen3.6:27b + GLiNER (flags accepted), removing 100.0% of items (0 missed) with 83.5% precision.
- **Best balance (F1):** phi4 at 98.0%. **Fastest:** phi4 at about 9.0 s per document.
- **Over-redaction check:** qwen3.6:27b + GLiNER (flags accepted) damaged 5 of 31 must-keep items.

## Recall by category

| Category | Items | phi4 | phi4 + GLiNER | phi4 + GLiNER (flags accepted) | qwen3.6:27b | qwen3.6:27b + GLiNER | qwen3.6:27b + GLiNER (flags accepted) |
|---|---:|---:|---:|---:|---:|---:|---:|
| ADDRESS | 29 | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) | 100.0% (29/29) |
| AGE | 16 | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) | 100.0% (16/16) |
| COMPANY | 56 | 100.0% (56/56) | 100.0% (56/56) | 100.0% (56/56) | 100.0% (56/56) | 100.0% (56/56) | 100.0% (56/56) |
| COMPANY_ID | 8 | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) |
| CONTEXTUAL | 6 | 33.3% (2/6) | 33.3% (2/6) | 66.7% (4/6) | 66.7% (4/6) | 66.7% (4/6) | 100.0% (6/6) |
| DATE_OF_BIRTH | 14 | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) | 100.0% (14/14) |
| DOMAIN | 2 | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| EMAIL | 31 | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) |
| GENDER | 35 | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) | 100.0% (35/35) |
| ID_NUMBER | 22 | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) |
| ONLINE_ID | 2 | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) | 100.0% (2/2) |
| PERSON | 100 | 98.0% (98/100) | 98.0% (98/100) | 99.0% (99/100) | 100.0% (100/100) | 100.0% (100/100) | 100.0% (100/100) |
| PHONE | 30 | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) |
| SECRET | 7 | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) | 100.0% (7/7) |

## Recall by document format

| Format | Documents | Items | phi4 | phi4 + GLiNER | phi4 + GLiNER (flags accepted) | qwen3.6:27b | qwen3.6:27b + GLiNER | qwen3.6:27b + GLiNER (flags accepted) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| PDF (text layer) | 4 | 52 | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) | 100.0% (52/52) |
| Plain text | 20 | 136 | 96.3% (131/136) | 96.3% (131/136) | 97.8% (133/136) | 99.3% (135/136) | 99.3% (135/136) | 100.0% (136/136) |
| Scan: clean image | 3 | 39 | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) |
| Scan: degraded image | 3 | 39 | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) | 100.0% (39/39) |
| Scan: image-only PDF | 2 | 26 | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) | 100.0% (26/26) |
| Word | 6 | 66 | 98.5% (65/66) | 98.5% (65/66) | 100.0% (66/66) | 98.5% (65/66) | 98.5% (65/66) | 100.0% (66/66) |

## Precision by redaction category

| Category | phi4 | phi4 + GLiNER | phi4 + GLiNER (flags accepted) | qwen3.6:27b | qwen3.6:27b + GLiNER | qwen3.6:27b + GLiNER (flags accepted) |
|---|---:|---:|---:|---:|---:|---:|
| ADDRESS | 100.0% (29/29) | 100.0% (29/29) | 93.5% (29/31) | 93.5% (29/31) | 93.5% (29/31) | 87.9% (29/33) |
| AGE | 80.0% (16/20) | 80.0% (16/20) | 76.2% (16/21) | 88.9% (16/18) | 88.9% (16/18) | 84.2% (16/19) |
| COMPANY | 100.0% (57/57) | 100.0% (57/57) | 78.9% (60/76) | 96.6% (56/58) | 96.6% (56/58) | 77.3% (58/75) |
| COMPANY_ID | 100.0% (4/4) | 100.0% (4/4) | 100.0% (4/4) | 100.0% (8/8) | 100.0% (8/8) | 100.0% (8/8) |
| CONTEXTUAL | 0.0% (0/1) | 0.0% (0/1) | 12.5% (2/16) | 11.1% (2/18) | 11.1% (2/18) | 13.0% (3/23) |
| DATE_OF_BIRTH | 87.5% (14/16) | 87.5% (14/16) | 82.4% (14/17) | 51.7% (15/29) | 51.7% (15/29) | 50.0% (15/30) |
| DOMAIN | 100.0% (2/2) | 100.0% (2/2) | 66.7% (2/3) | 100.0% (2/2) | 100.0% (2/2) | 66.7% (2/3) |
| EMAIL | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) | 100.0% (31/31) |
| GENDER | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) | 86.8% (33/38) | 86.8% (33/38) | 86.8% (33/38) |
| ID_NUMBER | 100.0% (27/27) | 100.0% (27/27) | 100.0% (27/27) | 100.0% (22/22) | 100.0% (22/22) | 100.0% (22/22) |
| ONLINE_ID | 100.0% (2/2) | 100.0% (2/2) | 66.7% (2/3) | 100.0% (2/2) | 100.0% (2/2) | 66.7% (2/3) |
| PERSON | 98.9% (92/93) | 98.9% (92/93) | 95.9% (94/98) | 100.0% (98/98) | 100.0% (98/98) | 97.0% (98/101) |
| PHONE | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) | 100.0% (30/30) |
| SECRET | 100.0% (7/7) | 100.0% (7/7) | 87.5% (7/8) | 100.0% (7/7) | 100.0% (7/7) | 87.5% (7/8) |

Of the correct redactions, the share given the right category label: phi4 97.9%; phi4 + GLiNER 97.9%; phi4 + GLiNER (flags accepted) 96.8%; qwen3.6:27b 99.4%; qwen3.6:27b + GLiNER 99.4%; qwen3.6:27b + GLiNER (flags accepted) 98.9%.

## Per document

| Document | Format | Items | phi4 (recall · missed · over) | phi4 + GLiNER (recall · missed · over) | phi4 + GLiNER (flags accepted) (recall · missed · over) | qwen3.6:27b (recall · missed · over) | qwen3.6:27b + GLiNER (recall · missed · over) | qwen3.6:27b + GLiNER (flags accepted) (recall · missed · over) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| csv/06-customer-list.csv | Plain text | 20 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| docx/01-hr-letter.docx | Word | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 4 |
| docx/02-services-agreement.docx | Word | 14 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| docx/04-meeting-notes.docx | Word | 12 | 91.7% · 1 · 1 | 91.7% · 1 · 1 | 100.0% · 0 · 1 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 100.0% · 0 · 0 |
| docx/09-gp-referral.docx | Word | 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| docx/11-hard-negatives.docx | Word | 0 | – · 0 · 2 | – · 0 · 2 | – · 0 · 6 | – · 0 · 3 | – · 0 · 3 | – · 0 · 5 |
| docx/12-hygiene-docx.docx | Word | 10 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| json/07-app-config.json | Plain text | 4 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| markdown/03-customer-email.md | Plain text | 10 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 2 |
| markdown/04-meeting-notes.md | Plain text | 12 | 91.7% · 1 · 1 | 91.7% · 1 · 1 | 100.0% · 0 · 1 | 91.7% · 1 · 0 | 91.7% · 1 · 0 | 100.0% · 0 · 0 |
| pdf/01-hr-letter.pdf | PDF (text layer) | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 4 |
| pdf/02-services-agreement.pdf | PDF (text layer) | 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| pdf/08-invoice.pdf | PDF (text layer) | 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| pdf/09-gp-referral.pdf | PDF (text layer) | 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| scans/01-hr-letter-scan-clean.png | Scan: clean image | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 4 |
| scans/01-hr-letter-scan-degraded.jpg | Scan: degraded image | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 4 |
| scans/01-hr-letter-scan.pdf | Scan: image-only PDF | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 4 | 100.0% · 0 · 4 | 100.0% · 0 · 5 |
| scans/08-invoice-scan-clean.png | Scan: clean image | 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/08-invoice-scan-degraded.jpg | Scan: degraded image | 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/08-invoice-scan.pdf | Scan: image-only PDF | 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| scans/09-gp-referral-scan-clean.png | Scan: clean image | 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| scans/09-gp-referral-scan-degraded.jpg | Scan: degraded image | 13 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 2 | 100.0% · 0 · 2 | 100.0% · 0 · 2 |
| text/01-hr-letter.txt | Plain text | 17 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 3 | 100.0% · 0 · 3 | 100.0% · 0 · 3 |
| text/03-customer-email.txt | Plain text | 10 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 | 100.0% · 0 · 1 |
| text/05-incident-report.txt | Plain text | 9 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/10-contextual-profile.txt | Plain text | 10 | 80.0% · 2 · 1 | 80.0% · 2 · 1 | 80.0% · 2 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/11-hard-negatives.txt | Plain text | 0 | – · 0 · 2 | – · 0 · 2 | – · 0 · 6 | – · 0 · 3 | – · 0 · 3 | – · 0 · 5 |
| text/context-text-01.txt | Plain text | 7 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |
| text/context-text-02.txt | Plain text | 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |
| text/context-text-03.txt | Plain text | 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/context-text-04.txt | Plain text | 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 3 |
| text/context-text-05.txt | Plain text | 8 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-06.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 3 | – · 0 · 0 | – · 0 · 0 | – · 0 · 3 |
| text/context-text-07.txt | Plain text | 6 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 1 |
| text/context-text-08.txt | Plain text | 0 | – · 0 · 1 | – · 0 · 1 | – · 0 · 4 | – · 0 · 0 | – · 0 · 0 | – · 0 · 5 |
| text/context-text-09.txt | Plain text | 8 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 0 |
| text/context-text-10.txt | Plain text | 0 | – · 0 · 0 | – · 0 · 0 | – · 0 · 3 | – · 0 · 3 | – · 0 · 3 | – · 0 · 6 |
| text/context-text-11-mixed.txt | Plain text | 5 | 60.0% · 2 · 0 | 60.0% · 2 · 0 | 80.0% · 1 · 2 | 100.0% · 0 · 0 | 100.0% · 0 · 0 | 100.0% · 0 · 2 |

## Timings

Seconds for each document and model. The first figure is the model finding the sensitive items. Models are run one after another, every document with one model before the next model is loaded, so a model is loaded into memory once.

| Document | phi4 | phi4 + GLiNER | phi4 + GLiNER (flags accepted) | qwen3.6:27b | qwen3.6:27b + GLiNER | qwen3.6:27b + GLiNER (flags accepted) |
|---|---:|---:|---:|---:|---:|---:|
| csv/06-customer-list.csv | 28.6 | 28.6 | 28.6 | 33.0 | 33.0 | 33.0 |
| docx/01-hr-letter.docx | 14.3 | 14.3 | 14.3 | 27.9 | 27.9 | 27.9 |
| docx/02-services-agreement.docx | 10.6 | 10.7 | 10.7 | 20.3 | 20.3 | 20.3 |
| docx/04-meeting-notes.docx | 7.4 | 7.5 | 7.5 | 13.2 | 13.2 | 13.2 |
| docx/09-gp-referral.docx | 9.5 | 9.5 | 9.5 | 18.6 | 18.6 | 18.6 |
| docx/11-hard-negatives.docx | 1.6 | 1.6 | 1.6 | 4.9 | 4.9 | 4.9 |
| docx/12-hygiene-docx.docx | 6.5 | 6.5 | 6.5 | 12.5 | 12.6 | 12.6 |
| json/07-app-config.json | 4.5 | 4.6 | 4.6 | 8.6 | 8.6 | 8.6 |
| markdown/03-customer-email.md | 7.5 | 7.5 | 7.5 | 14.2 | 14.3 | 14.3 |
| markdown/04-meeting-notes.md | 7.5 | 7.5 | 7.5 | 14.7 | 14.8 | 14.8 |
| pdf/01-hr-letter.pdf | 15.5 | 15.6 | 15.6 | 30.2 | 30.2 | 30.2 |
| pdf/02-services-agreement.pdf | 9.7 | 9.7 | 9.7 | 18.0 | 18.1 | 18.1 |
| pdf/08-invoice.pdf | 12.5 | 12.6 | 12.6 | 17.1 | 17.1 | 17.1 |
| pdf/09-gp-referral.pdf | 8.8 | 8.8 | 8.8 | 19.1 | 19.1 | 19.1 |
| scans/01-hr-letter-scan-clean.png | 16.7 | 16.8 | 16.8 | 30.7 | 30.7 | 30.7 |
| scans/01-hr-letter-scan-degraded.jpg | 16.6 | 16.7 | 16.7 | 28.5 | 28.5 | 28.5 |
| scans/01-hr-letter-scan.pdf | 16.0 | 16.0 | 16.0 | 30.8 | 30.8 | 30.8 |
| scans/08-invoice-scan-clean.png | 11.8 | 11.8 | 11.8 | 16.9 | 17.0 | 17.0 |
| scans/08-invoice-scan-degraded.jpg | 11.8 | 11.9 | 11.9 | 16.9 | 17.0 | 17.0 |
| scans/08-invoice-scan.pdf | 11.9 | 11.9 | 11.9 | 17.2 | 17.2 | 17.2 |
| scans/09-gp-referral-scan-clean.png | 9.2 | 9.3 | 9.3 | 17.6 | 17.6 | 17.6 |
| scans/09-gp-referral-scan-degraded.jpg | 9.1 | 9.1 | 9.1 | 17.7 | 17.7 | 17.7 |
| text/01-hr-letter.txt | 13.7 | 13.8 | 13.8 | 30.1 | 30.1 | 30.1 |
| text/03-customer-email.txt | 8.1 | 8.1 | 8.1 | 14.9 | 14.9 | 14.9 |
| text/05-incident-report.txt | 9.1 | 9.1 | 9.1 | 15.3 | 15.4 | 15.4 |
| text/10-contextual-profile.txt | 8.0 | 8.0 | 8.0 | 11.2 | 11.2 | 11.2 |
| text/11-hard-negatives.txt | 1.7 | 1.8 | 1.8 | 5.1 | 5.1 | 5.1 |
| text/context-text-01.txt | 3.3 | 3.4 | 3.4 | 8.5 | 8.5 | 8.5 |
| text/context-text-02.txt | 4.2 | 4.3 | 4.3 | 6.9 | 6.9 | 6.9 |
| text/context-text-03.txt | 4.4 | 4.4 | 4.4 | 9.1 | 9.1 | 9.1 |
| text/context-text-04.txt | 4.2 | 4.2 | 4.2 | 9.1 | 9.1 | 9.1 |
| text/context-text-05.txt | 5.9 | 5.9 | 5.9 | 9.2 | 9.2 | 9.2 |
| text/context-text-06.txt | 0.3 | 0.4 | 0.4 | 2.0 | 2.0 | 2.0 |
| text/context-text-07.txt | 5.2 | 5.2 | 5.2 | 7.1 | 7.1 | 7.1 |
| text/context-text-08.txt | 6.8 | 6.8 | 6.8 | 1.9 | 1.9 | 1.9 |
| text/context-text-09.txt | 6.1 | 6.1 | 6.1 | 9.1 | 9.1 | 9.1 |
| text/context-text-10.txt | 6.6 | 6.6 | 6.6 | 9.0 | 9.1 | 9.1 |
| text/context-text-11-mixed.txt | 5.9 | 6.0 | 6.0 | 10.2 | 10.2 | 10.2 |
| **Total** | **341.3** | **342.2** | **342.2** | **587.2** | **588.1** | **588.1** |
| Average per document | 9.0 | 9.0 | 9.0 | 15.5 | 15.5 | 15.5 |

## What was missed and over-redacted
*Contains text from the documents.*

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

### phi4 + GLiNER

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

### phi4 + GLiNER (flags accepted)

**Missed**

- `text/10-contextual-profile.txt` CONTEXTUAL: “2019 data breach at the Bristol depot” (1)
- `text/10-contextual-profile.txt` CONTEXTUAL: “GBP 92,000” (1)
- `text/context-text-11-mixed.txt` PERSON: “Paris” (1)

**Over-redacted**

- `csv/06-customer-list.csv` ADDRESS: “date_of_birth” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/04-meeting-notes.docx` DATE_OF_BIRTH: “in January” is not on the answer key
- `docx/09-gp-referral.docx` CONTEXTUAL: “bus driver” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “HMRC” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “committee” is not on the answer key
- `docx/11-hard-negatives.docx` AGE: “42” is not on the answer key
- `docx/11-hard-negatives.docx` CONTEXTUAL: “revenue” is not on the answer key
- `markdown/03-customer-email.md` DATE_OF_BIRTH: “Tuesday” is not on the answer key
- `markdown/03-customer-email.md` AGE: “61” is not on the answer key
- `markdown/04-meeting-notes.md` DATE_OF_BIRTH: “in January” is not on the answer key
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
- `text/10-contextual-profile.txt` PERSON: “regional director” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “HMRC” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “committee” is not on the answer key
- `text/11-hard-negatives.txt` AGE: “42” is not on the answer key
- `text/11-hard-negatives.txt` CONTEXTUAL: “revenue” is not on the answer key
- `text/context-text-01.txt` COMPANY: “Kestrel” is not on the answer key
- `text/context-text-01.txt` COMPANY: “design team” is not on the answer key
- `text/context-text-02.txt` CONTEXTUAL: “Kestrel travel” is not on the answer key
- `text/context-text-02.txt` COMPANY: “office” is not on the answer key
- `text/context-text-03.txt` COMPANY: “finance team” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Regional expansion plan” is not on the answer key
- `text/context-text-04.txt` COMPANY: “suppliers” is not on the answer key
- `text/context-text-04.txt` COMPANY: “regional office” is not on the answer key
- `text/context-text-06.txt` COMPANY: “Committee” is not on the answer key
- `text/context-text-06.txt` COMPANY: “committee” is not on the answer key
- `text/context-text-06.txt` ADDRESS: “entrance” is not on the answer key
- `text/context-text-07.txt` DOMAIN: “Parcels” is not on the answer key
- `text/context-text-08.txt` PERSON: “children” is not on the answer key
- `text/context-text-08.txt` CONTEXTUAL: “a school trip” is not on the answer key
- `text/context-text-08.txt` COMPANY: “apple tree” is not on the answer key
- `text/context-text-10.txt` COMPANY: “offsite” is not on the answer key
- `text/context-text-10.txt` PERSON: “Coaches” is not on the answer key
- `text/context-text-10.txt` AGE: “nine” is not on the answer key
- `text/context-text-11-mixed.txt` SECRET: “MIXED” is not on the answer key
- `text/context-text-11-mixed.txt` COMPANY: “Project Kestrel” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “HMRC”
- `docx/11-hard-negatives.docx` should have been kept: “42”
- `text/11-hard-negatives.txt` should have been kept: “HMRC”
- `text/11-hard-negatives.txt` should have been kept: “42”
- `text/context-text-08.txt` should have been kept: “apple”

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

### qwen3.6:27b + GLiNER

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

### qwen3.6:27b + GLiNER (flags accepted)

**Missed**

- nothing

**Over-redacted**

- `csv/06-customer-list.csv` ADDRESS: “date_of_birth” is not on the answer key
- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “14 October 2025” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Senior Data Scientist” is not on the answer key
- `docx/01-hr-letter.docx` DATE_OF_BIRTH: “1 November” is not on the answer key
- `docx/01-hr-letter.docx` CONTEXTUAL: “Head of People” is not on the answer key
- `docx/09-gp-referral.docx` GENDER: “he” is not on the answer key
- `docx/09-gp-referral.docx` CONTEXTUAL: “bus driver” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “HMRC” is not on the answer key
- `docx/11-hard-negatives.docx` DATE_OF_BIRTH: “March” is not on the answer key
- `docx/11-hard-negatives.docx` COMPANY: “committee” is not on the answer key
- `docx/11-hard-negatives.docx` CONTEXTUAL: “revenue” is not on the answer key
- `docx/12-hygiene-docx.docx` GENDER: “his” is not on the answer key
- `docx/12-hygiene-docx.docx` DATE_OF_BIRTH: “31 December” is not on the answer key
- `markdown/03-customer-email.md` DATE_OF_BIRTH: “Tuesday” is not on the answer key
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
- `scans/01-hr-letter-scan.pdf` ONLINE_ID: “Tel” is not on the answer key
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
- `text/05-incident-report.txt` PERSON: “engineer” is not on the answer key
- `text/10-contextual-profile.txt` CONTEXTUAL: “regional director” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “HMRC” is not on the answer key
- `text/11-hard-negatives.txt` DATE_OF_BIRTH: “March” is not on the answer key
- `text/11-hard-negatives.txt` COMPANY: “committee” is not on the answer key
- `text/11-hard-negatives.txt` CONTEXTUAL: “revenue” is not on the answer key
- `text/context-text-01.txt` COMPANY: “Kestrel” is not on the answer key
- `text/context-text-01.txt` COMPANY: “design team” is not on the answer key
- `text/context-text-02.txt` CONTEXTUAL: “Kestrel travel” is not on the answer key
- `text/context-text-02.txt` COMPANY: “office” is not on the answer key
- `text/context-text-03.txt` COMPANY: “finance team” is not on the answer key
- `text/context-text-04.txt` COMPANY: “Regional expansion plan” is not on the answer key
- `text/context-text-04.txt` COMPANY: “suppliers” is not on the answer key
- `text/context-text-04.txt` COMPANY: “regional office” is not on the answer key
- `text/context-text-06.txt` COMPANY: “Committee” is not on the answer key
- `text/context-text-06.txt` COMPANY: “committee” is not on the answer key
- `text/context-text-06.txt` ADDRESS: “entrance” is not on the answer key
- `text/context-text-07.txt` DOMAIN: “Parcels” is not on the answer key
- `text/context-text-08.txt` PERSON: “children” is not on the answer key
- `text/context-text-08.txt` COMPANY: “Amazon” is not on the answer key
- `text/context-text-08.txt` COMPANY: “apple tree” is not on the answer key
- `text/context-text-10.txt` COMPANY: “offsite” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-10.txt` PERSON: “Coaches” is not on the answer key
- `text/context-text-10.txt` AGE: “nine” is not on the answer key
- `text/context-text-10.txt` ADDRESS: “Victoria Station” is not on the answer key
- `text/context-text-11-mixed.txt` SECRET: “MIXED” is not on the answer key
- `text/context-text-11-mixed.txt` COMPANY: “Project Kestrel” is not on the answer key
- `docx/11-hard-negatives.docx` should have been kept: “HMRC”
- `text/11-hard-negatives.txt` should have been kept: “HMRC”
- `text/context-text-08.txt` should have been kept: “apple”
- `text/context-text-08.txt` should have been kept: “Amazon”
- `text/context-text-10.txt` should have been kept: “Victoria”

## Settings used

- Temperature 0, seed 42, context 8192 tokens, chunks of about 4800 characters with 400 overlap.
- Reasoning (think): off (gpt-oss cannot switch it off, so it runs at its lowest level, low). Model kept loaded for 60m; each model is unloaded when its turn ends.
- Run on macOS 27.0.1, 18 cores; Ollama 0.35.0; endpoint http://localhost:11434; started 2026-10-02 18:04.
- Categories and their modes: PERSON (redact), PHONE (redact), EMAIL (redact), ADDRESS (redact), ID_NUMBER (redact), ONLINE_ID (redact), AGE (redact), DATE_OF_BIRTH (redact), GENDER (redact), COMPANY (redact), COMPANY_ID (redact), DOMAIN (redact), CONTEXTUAL (redact), LOCATION (flag only), SECRET (redact).
- Categories on: PERSON, PHONE, EMAIL, ADDRESS, ID_NUMBER, ONLINE_ID, AGE, DATE_OF_BIRTH, GENDER, COMPANY, COMPANY_ID, DOMAIN, CONTEXTUAL, LOCATION, SECRET (pronouns included).
- Flag-only (reported, not redacted, so they count as missed here): LOCATION.
- Models: phi4 (14.7B · Q4_K_M · 16K ctx, digest ac896e5b); phi4 + GLiNER; phi4 + GLiNER (flags accepted); qwen3.6:27b (27.3B · Q4_K_M · 256K ctx, digest bcbdbd4b); qwen3.6:27b + GLiNER; qwen3.6:27b + GLiNER (flags accepted).

## What combining two models could achieve (estimate)

If a document were redacted with *both* models of a pair and everything either found were removed, an item would leak only if both missed it. This estimate counts the items missed by both, from the missed lists in this run, so it is a ceiling on the recall of a union: it ignores that the second model's different wording may also redact the same text partly. The over-redaction figure is the most extra redactions that could add up (the two models' counts summed). Only documents scored by both models are counted. The top pairs by estimated recall are shown.

| Pair | Better model alone (recall) | Both together (estimated recall) | Most extra over-redactions |
|---|---:|---:|---:|
| phi4 + qwen3.6:27b + GLiNER (flags accepted) | 100.0% | 100.0% | 78 |
| phi4 + GLiNER + qwen3.6:27b + GLiNER (flags accepted) | 100.0% | 100.0% | 78 |
| phi4 + GLiNER (flags accepted) + qwen3.6:27b | 99.4% | 100.0% | 88 |
| phi4 + GLiNER (flags accepted) + qwen3.6:27b + GLiNER | 99.4% | 100.0% | 88 |
| qwen3.6:27b + qwen3.6:27b + GLiNER (flags accepted) | 100.0% | 100.0% | 111 |
| qwen3.6:27b + GLiNER + qwen3.6:27b + GLiNER (flags accepted) | 100.0% | 100.0% | 111 |
| phi4 + GLiNER (flags accepted) + qwen3.6:27b + GLiNER (flags accepted) | 100.0% | 100.0% | 117 |
| phi4 + qwen3.6:27b | 99.4% | 99.4% | 49 |

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
