# Redaction Demo (AI Document Redactor)

A prototype that finds and removes sensitive data (names, addresses, phone numbers, company details, secrets and more) from documents using **local language models through Ollama**. Nothing leaves the machine, and redaction is permanent: the sensitive data is removed from the output, not hidden.

It reads text files, Word documents, PDFs and scans (with local OCR), and writes redacted copies to a separate folder. The original files are never changed or deleted.

## What is in the repository

| Part | What it is |
|---|---|
| **Web App** | interactive review tool: run a redaction, see every edit with a confidence grade, redact by hand, view PDF pages, compare models |
| **Evaluation App** | command-line tool that scores models on synthetic test corpora and writes Markdown reports |
| **Command-line runner** | redacts a whole folder in one go (`src/AiDocumentRedactor.Cli`) |
| **Test corpus** | invented documents in many formats, with answer keys (`tests/TestCorpus`, `tests/HeldOutCorpus`) |
| **Results Explorer** | web-based interface for exploring evaluation results (see `src/AiDocumentRedactor.ResultsExplorer`) |

## Documentation

- [How to run the Web App](documentation/HOW_TO_RUN_WEB_APP.md)
- [How to run the Evaluation App](documentation/HOW_TO_RUN_EVALUATION.md)
- [**Final findings: one evaluation, every model, every document**](documentation/FINAL_FINDINGS.md) (the account to quote; the version findings below record how the approach was developed)
- [Can redaction be 100% accurate? (position paper)](documentation/ACCURACY_AND_PERFECTION.md)
- [Version 1 findings: evaluation results and the road to production](documentation/EVALUATION_FINDINGS_V1.md)
- [Version 2 findings: effect of the phase 1 accuracy changes](documentation/EVALUATION_FINDINGS_V2.md)
- [Version 3 findings: five models after phase 1 and the organisation rule](documentation/EVALUATION_FINDINGS_V3.md)
- [Versioned evaluation reports (v1, v2, v3)](documentation/evaluation-reports/README.md)
- [Held-out corpus: 300 synthetic finance documents with answer keys](tests/HeldOutCorpus/README.md)
- [Version 5 findings: held-out test, corrected answer key, combinations](documentation/EVALUATION_FINDINGS_V5.md)
- [Version 4 findings: agreement with GLiNER](documentation/EVALUATION_FINDINGS_V4.md)
- [Phase 2 plan: layered detection with GLiNER, and benchmarking it](documentation/PHASE_2_PLAN.md)
- [Specification](documentation/SPECIFICATION.md): requirements, approach, security, risks and the delivery plan
- [Coding standards](CODING_STANDARDS.md)

## Quick evaluation commands

Run a quick trial of one model on plain-text documents:

```bash
./run-eval.sh --models phi4 --only text/ --no-write
```

Run on the held-out corpus (300 finance documents):

```bash
./run-eval.sh --corpus tests/HeldOutCorpus --models phi4,gemma4:e4b --no-write
```

Combine multiple saved runs:

```bash
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll \
  --merge eval/first.scores.json,eval/second.scores.json --out eval/merged.md
```

## Quick start

One command each, from the repository root:

```bash
./run-web.sh      # start the Web App and open it in your browser
./run-eval.sh     # run the evaluation and open the report (takes a while)
```

The scripts check .NET and Ollama, build, and start. The manual steps are below and in the guides.

You need the .NET 10 SDK, [Ollama](https://ollama.com) running, and a model:

```bash
ollama pull phi4
```

```bash
dotnet build
```

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet src/AiDocumentRedactor.App.Web/bin/Debug/net10.0/AiDocumentRedactor.App.Web.dll --config redactor.config.json --port 5199
```

Open the address it prints (it includes an access token). Put documents in the `in` folder; redacted files go to `out`. Full steps are in [How to run the Web App](documentation/HOW_TO_RUN_WEB_APP.md).

## Settings

All settings are in `redactor.config.json`: folders, which categories of sensitive data are on, the model, OCR and PDF options, and which models the evaluation compares. The Web App also lets you change categories for a session without touching the file.

## Tests

```bash
dotnet test
```

## Project layout

```
src/      Core, Detection, Documents, Ocr, App.ViewModels, App.Ui, App.Web, Cli, Eval
tests/    unit tests and the synthetic test corpus
tools/    the corpus generator
documentation/   specification and how-to guides
in/       the source test documents (synthetic, in source control; never modified by the tools)
out/      the redacted output (not in source control)
```

## Licences and attribution

Test documents and answer keys are derived from public datasets (NVIDIA Nemotron-PII, CC BY 4.0; Gretel AI, Apache 2.0). See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
