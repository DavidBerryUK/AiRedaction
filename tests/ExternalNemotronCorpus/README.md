# External corpus: Nemotron-PII

232 documents from the **test split** of [nvidia/Nemotron-PII](https://huggingface.co/datasets/nvidia/Nemotron-PII) (CC-BY-4.0, fully synthetic,
attribution to NVIDIA required), with 869 labelled sensitive items (distinct text per document). Built by `tools/eval/import_external.py`.

**Choice of documents.** A seeded random draw (seed 20261003), the same number from each of the 29 domains (banking, credit, mortgage, insurance,
healthcare, public safety, elections, ...), length 150 to 5,000 characters. No quality filter was applied. A different generator and labeller from
the held-out corpus, so this is the more independent test.

**Answer key.** The dataset's labels, mapped to this project's categories (`tools/eval/import_external.py`). The spans' stated offsets do not always
match the text, so each item is taken from the span's own text and kept only if that text occurs in the document. Not audited by a person.
Generic dates and times, cities, countries, URLs and coordinates are not on the key. Gender, age, job title, race, religion and similar are not
judged either way. Do not tune rules or prompts against this corpus.

**Limits.** Text only (no Word, PDF or image versions). Many are short, plain passages. Attribution: NVIDIA, CC-BY-4.0.
