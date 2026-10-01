# How to run the Evaluation App

The Evaluation App measures how well each local model redacts. It runs the models over the synthetic test corpus, compares what they redacted with an answer key, and writes a **Markdown report** (plus a JSON file of the raw scores). It is a command-line tool, not part of the web app: a full run takes a long time, so it is meant to be started and left alone.

## Quick launch

From the repository root, one command builds, checks that Ollama is running, runs the evaluation with the models switched on in the config, and opens the Markdown report when it finishes:

```bash
./run-eval.sh
```

It accepts the same options as the program (`--models`, `--only`, `--no-write`, `--show-text`, `--out`) plus `--no-open` (do not open the report). For a quick trial:

```bash
./run-eval.sh --models phi4 --only text/ --no-write
```

`Ctrl+C` stops it; the report for the models that finished is kept. The manual steps below do the same thing in separate commands.

## What you need

- .NET 10 SDK and Ollama running (as for the Web App)
- The models you want to compare, downloaded with `ollama pull <name>`
- The test corpus in `tests/TestCorpus` (part of the repository)

## Choose the models

Models are listed in `redactor.config.json`, in the `evaluation.models` section. Each has a switch:

```json
"evaluation": {
  "models": [
    { "name": "phi4",       "include": true },
    { "name": "gemma4:e4b", "include": true },
    { "name": "qwen3.6:27b", "include": false }
  ]
}
```

Only models with `"include": true` are run. A model that is switched on but not installed is skipped with a note (`ollama pull <name>` to fetch it). To run a different set once, without editing the file, use `--models phi4,gemma4:e4b`.

## Run it

From the **repository root**:

```bash
dotnet build
```

```bash
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll
```

It prints each document and model as it goes, for example `text/01-hr-letter.txt: caught 16/16, 2 over, 10.9s`. When it finishes it prints the report path, by default `eval/eval-<date>-<time>.md`, with a `.json` file beside it. The report is rewritten after every model, so an interrupted run still leaves results for the models that finished.

### Options

| Option | Meaning |
|---|---|
| `--config <file>` | settings (default `redactor.config.json`) |
| `--corpus <folder>` | test corpus (default `tests/TestCorpus`) |
| `--models a,b` | models to run, instead of the config list |
| `--out <file>` | report path (default `eval/eval-<date>-<time>.md`) |
| `--only <text>` | only corpus files whose path contains this text, for a quick trial (for example `--only text/`) |
| `--no-write` | score the text only; skip writing and verifying the redacted files (much faster) |
| `--show-text` | list the missed and over-redacted strings in the report (sensible only on the synthetic corpus) |
| `--help` | show the options |

A quick trial of one model on the plain-text documents, without writing files:

```bash
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll --models phi4 --only text/ --no-write
```

### How long it takes

Every model reads every document: 28 files across text, Word, PDF and three kinds of scan. Small models take a minute or two per model, large ones much longer, and writing each redacted file (with the PDF and OCR safety checks) adds time. With all seven configured models expect it to run for a long time; start it and leave it. Use `--no-write` or `--only` while you are testing settings.

## Reading the report

The report is a Markdown file you can open in any viewer or paste into a document. It contains:

- **Summary:** one row per model with **recall** (share of sensitive items removed, where a miss is a leak), **precision** (share of redactions that were correct), F1, items missed, over-redactions, must-keep items damaged, time per document, output tokens per second and model size.
- **Headline findings:** plain-English highlights generated from the numbers.
- **Recall by category** and **by document format**, and **precision by category**.
- **Per document:** recall, misses and over-redactions for each file and model.
- **Timings:** seconds for every document and model (model time, plus write-and-verify time unless `--no-write`), with totals and averages. The program runs every document with one model before loading the next, so each model is loaded once.
- **Output safety:** whether each redacted file was written and passed the tool's own checks (no text layer in PDFs, nothing recoverable in Word files, OCR re-read of scans). Not present when `--no-write` is used.
- **Settings used** and **how to read this, and its limits**.

By default the report contains **no document text**, only counts, categories and file names.

### Things to keep in mind

- The corpus is synthetic. Scores compare models fairly with each other, but a client's real documents will be messier.
- Recall is strict: a partial redaction (for example only a surname) counts as a miss.
- Scans are read by OCR first, so scan scores mix model quality and OCR quality. Items OCR could not read at all are reported separately.
- Some models may not produce valid structured output; those documents are listed under **Skipped** rather than scored.

## Troubleshooting

| Problem | Fix |
|---|---|
| "Cannot reach Ollama" | start Ollama; check `llm.endpoint` in the config |
| "Not installed: …" | `ollama pull <name>`, or remove it from `--models` |
| "No models to run" | switch at least one model on in `evaluation.models`, or use `--models` |
| No ground-truth folder | run from the repository root, or pass `--corpus` |
| Scans are skipped | OCR is switched off (`ocr.enabled` in the config) |
