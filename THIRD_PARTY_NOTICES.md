# Third-party notices

This repository contains test documents and answer keys derived from two public datasets, and it refers to models and libraries that are not part of it. All documents in the repository are synthetic: no real individuals are represented.

## Data included in this repository

### Nemotron-PII (NVIDIA), CC BY 4.0

- **Source:** [nvidia/Nemotron-PII](https://huggingface.co/datasets/nvidia/Nemotron-PII), test split. Copyright NVIDIA. Licensed under [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/).
- **Where it is used here:** `in/external-nemotron/` (232 documents) and `tests/ExternalNemotronCorpus/` (their answer keys), and the evaluation datasets in `datasets-archive/` that score them.
- **Changes made:** a seeded random sample of 232 documents was taken (8 from each of 29 domains, length 150 to 5,000 characters); the dataset's labels were mapped to this project's categories and the answer keys written in this project's format; social-security-shaped numbers that the dataset left unlabelled were added to the keys (the dataset's own labels are kept in `ground-truth-original`). Document text is unchanged. See `tests/ExternalNemotronCorpus/README.md` and `tools/eval/import_external.py`.
- NVIDIA does not endorse this project or its results.

### Synthetic PII Finance Multilingual (Gretel AI), Apache 2.0

- **Source:** [gretelai/synthetic_pii_finance_multilingual](https://huggingface.co/datasets/gretelai/synthetic_pii_finance_multilingual), English test split. Licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).
- **Where it is used here:** `in/finance-sample/` (300 documents) and `tests/HeldOutCorpus/`, and `in/external-gretel/` (180 documents) and `tests/ExternalGretelCorpus/`, and the datasets in `datasets-archive/` that score them.
- **Changes made:** documents were selected (the held-out set by document type and quality, the external set by a seeded random draw excluding documents already used); the generator's labels were mapped to this project's categories; the held-out answer key was then corrected by a documented rule-based audit (`tests/HeldOutCorpus/AUDIT.md`), with the generator's original key kept in `ground-truth-original`. Document text is unchanged.

## Models and libraries not included

- **Language models** (run locally through [Ollama](https://ollama.com), downloaded separately and not part of this repository): gemma4, phi4, qwen3.6 and gpt-oss. Each is under its own licence and terms.
- **GLiNER** (optional second opinion, run in .NET with ONNX Runtime): the model files are downloaded separately into `models/` (git-ignored) from `knowledgator/gliner-pii-edge-v1.0` on Hugging Face.
- **NuGet packages** (CommunityToolkit.Mvvm, DocumentFormat.OpenXml, Markdig, Microsoft.Data.Sqlite, Microsoft.ML.OnnxRuntime, PDFtoImage, PdfPig, RapidOcrNet, SkiaSharp, and the test packages) are restored by `dotnet restore` under their own licences and are not redistributed here.

## The project's own licence

This repository does not yet state a licence for its own code and documents. Until one is added, the default copyright rules apply.
