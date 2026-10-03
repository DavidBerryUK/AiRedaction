# External corpus: Gretel (finance documents)

180 English documents from the **test split** of [gretelai/synthetic_pii_finance_multilingual](https://huggingface.co/datasets/gretelai/synthetic_pii_finance_multilingual)
(Apache 2.0, fully synthetic), with 554 labelled sensitive items (distinct text per document). Built by `tools/eval/import_external.py`.

**Choice of documents.** Up to 3 per document type, drawn at random (seed 20261003), from the test documents that are *not* in
`tests/HeldOutCorpus`. No quality filter was applied. The documents come from the same generator as the held-out corpus, so they test new documents
but not a new generator; see the Nemotron corpus for that.

**Answer key.** The generator's own labels, mapped to this project's categories (the same mapping as the held-out corpus), plus any social-security-shaped number in the text that the generator left unlabelled (the dataset's own labels are kept in `ground-truth-original`). They have **not** been otherwise audited,
and the held-out audit found that this generator leaves many real items unlabelled, so a redaction of a genuine but unlabelled item counts as
over-redaction here and precision is understated. Generic dates and times, coordinates and bank identifier codes are not on the key.
Only the categories in `judgedCategories` are judged. Do not tune rules or prompts against this corpus.

**Limits.** Text only (no Word, PDF or image versions). Attribution: Gretel AI, Apache 2.0.
