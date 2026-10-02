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

It shows where it is as it goes: a header per model (`=== Model 2 of 10: gemma4:e4b ===`) and a line per document that names the model and the document before it starts, then adds the result when it finishes, for example `[model 2/10 gemma4:e4b] document 3 of 34: text/context-text-03.txt ... caught 7/7, 1 over, 6.3s`. When it finishes it prints the report path, by default `eval/eval-<date>-<time>.md`, with a `.json` file beside it. The report is rewritten after every model, so an interrupted run still leaves results for the models that finished.

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

### What else the report contains

- **Detail per model:** documents scored and failed, total and median time, the slowest document, tokens used, whole items fully caught, label accuracy and items lost to OCR.
- **Missed and over-redacted text:** `run-eval.sh` adds `--show-text` because the corpus is synthetic, so the report lists exactly what each model missed or over-redacted. Add `--no-text` to leave it out.
- **What combining two models could achieve:** an estimate, from the missed lists, of the recall if two models were run together and everything either found were removed.
- **Beyond one model:** a short discussion of combining models and of training a model of our own.
- **Settings used:** includes the reasoning (think) setting, the Ollama version, the machine and the categories with their modes.
- Each model is **unloaded when its turn ends**, so every model starts cold and the timings are comparable.

### Combining runs

Every report also writes a full `.scores.json` beside it. To rebuild one report from several runs, for example after re-running a single model:

```bash
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll --merge eval/first.scores.json,eval/second.scores.json --out eval/merged.md
```

A model in a later file replaces the same model from earlier files, including its failures. (Runs made before this feature have no `.scores.json` and cannot be merged this way.)

### What the Summary columns mean

The report explains these itself, under the Summary table, in plain language:

- **Recall:** of everything that should have been hidden, how much was hidden. The most important number, because a miss is a leak.
- **Precision:** of everything hidden, how much really needed hiding. The rest is over-redaction.
- **F1:** one score that blends the two; it is high only when both are high.
- **Must-keep items damaged (preserved):** ordinary text that had to survive (dates, titles, amounts) but was wrongly removed. Zero is ideal.

### Things to keep in mind

- The corpus is synthetic. Scores compare models fairly with each other, but a client's real documents will be messier.
- **Thinking models** (the qwen3.x family) write a long chain of reasoning before answering. Left on, they were 10-20 times slower and often returned an empty or cut-off reply, so documents failed. `llm.think` in `redactor.config.json` is set to `false` to switch the reasoning off; a model with no thinking mode ignores it. Set it to `null` to leave each model's own default.
- Ollama keeps each model in memory for a while after it is used, so the previous model may still show in `ollama ps`; it is idle and harmless (`ollama stop <model>` unloads it).
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

## Running on the held-out corpus, and what takes how long

`tests/HeldOutCorpus` holds 300 synthetic finance documents (see its README) that nothing in this project was tuned against. To run on it:

```bash
./run-eval.sh --corpus tests/HeldOutCorpus --gliner --models phi4,gpt-oss,qwen3.6:27b --no-write
```

`--no-write` skips writing and verifying redacted files (these documents are text only), and `--gliner` adds the second-opinion rows. Measured on a 10-document sample, a document takes about 6–7 s with phi4, about 6 s with gpt-oss and about 15 s with qwen3.6:27b (the first document includes loading the model), and GLiNER adds about 0.06 s. For 300 documents that is roughly **35–45 minutes for phi4, 30–40 for gpt-oss and 70–90 for qwen3.6:27b**, so about **2½ hours for these three**; all five configured models would take about 4 hours. The rows without a model take under a minute.

New in the report: 95% ranges on recall and precision, *Recall by document type* (collapsed when long), the no-model baseline rows (*rules only*, and with `--gliner` also *GLiNER only* and *rules + GLiNER*; `--no-baselines` skips them), a row for **correct flags accepted** (an ideal reviewer), a **GLiNER time** column, and, when the report shows document text, a **Likely answer-key gaps** list: text most models redacted that the answer key does not list.

## Scoring combinations of detectors for real

Every run now saves each detector's spans (positions only, no text) in its `.scores.json`. To score combinations without running any model again:

```bash
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll --combine eval/<run>.scores.json --models phi4,qwen3.6:27b --out eval/combos.md
```

It scores each model alone, their union, agreement (all agree, majority, or two-or-more with singles flagged), and, if the run had `--gliner`, the union with GLiNER's flags and a vote with GLiNER counting as one detector, each with the flags left in the text and with an ideal reviewer accepting only the correct flags.
