# External corpus: Nemotron-PII

232 documents from the **test split** of [nvidia/Nemotron-PII](https://huggingface.co/datasets/nvidia/Nemotron-PII) (CC-BY-4.0, fully synthetic,
attribution to NVIDIA required), with 873 labelled sensitive items (distinct text per document). Built by `tools/eval/import_external.py`.

**Choice of documents.** A seeded random draw (seed 20261003), the same number from each of the 29 domains (banking, credit, mortgage, insurance,
healthcare, public safety, elections, ...), length 150 to 5,000 characters. No quality filter was applied. A different generator and labeller from
the held-out corpus, so this is the more independent test.

**Answer key.** The dataset's labels, mapped to this project's categories (`tools/eval/import_external.py`). The spans' stated offsets do not always
match the text, so each item is taken from the span's own text and kept only if that text occurs in the document.

**Key check (3 October 2026).** 30 random documents (about 90 key items) were read against their keys by hand, and every document was scanned for emails,
phone numbers, card numbers, IP addresses and social-security-shaped numbers missing from the key. All 102 emails, 27 card numbers and 52 phone numbers
found were labelled. 4 of the 26 social-security-shaped numbers were not (one was in the hand-read sample, the other three were found by the scan), so a rule
now adds any such number to the key; `ground-truth-original` holds the dataset's own labels. Internal addresses such as 192.168.1.100 are left off the key on
purpose. Apart from that one kind of gap, no missed sensitive item was found in the hand-read documents. Names, addresses and company names cannot be
scanned for automatically, so the check on those is the 30-document reading only. This is a small sample, not an audit.
Generic dates and times, cities, countries, URLs and coordinates are not on the key. Gender, age, job title, race, religion and similar are not
judged either way. Do not tune rules or prompts against this corpus.

**Limits.** Text only (no Word, PDF or image versions). Many are short, plain passages. Attribution: NVIDIA, CC-BY-4.0.
