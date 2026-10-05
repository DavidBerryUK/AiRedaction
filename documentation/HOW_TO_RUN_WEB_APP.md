# How to run the Web App

The Web App is the interactive Redaction Demo: pick a document, press **Redact**, and review exactly what was redacted. It runs on your own machine, only listens on `127.0.0.1`, and sends nothing anywhere. All detection is done by a local model through Ollama.

## Quick launch (recommended for demos)

From the repository root, one command builds, checks that Ollama is running, starts the app and opens it in your browser with the access token already filled in:

```bash
./run-web.sh
```

Stop it with `Ctrl+C`. Options: `--port 5200` (use another port), `--input <folder>`, `--output <folder>`, `--config <file>`, and `--no-open` (do not open the browser). If Ollama is not running, or the port is busy, the script says so and what to do. The manual steps below do the same thing in separate commands.

## What you need

| Requirement | Check |
|---|---|
| .NET 10 SDK | `dotnet --version` shows 10.x |
| Ollama, running | `ollama list` works (start the Ollama app or run `ollama serve`) |
| At least one model | `ollama pull phi4` (the default) |
| Some documents | put them in the `in` folder (any sub-folders) |

The test corpus in `tests/TestCorpus` can be copied into `in` to try the app straight away:

```bash
cp -R tests/TestCorpus/{text,markdown,csv,json,docx,pdf,scans} in/
```

## Start it

Run these from the **repository root** (the folder containing `redactor.config.json`). Building first and running the built program keeps the relative `in` and `out` folders pointing at the root.

```bash
dotnet build
```

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet src/AiDocumentRedactor.App.Web/bin/Debug/net10.0/AiDocumentRedactor.App.Web.dll --config redactor.config.json --port 5199
```

The program prints a line like this:

```
Redaction Demo — open http://127.0.0.1:5199/?t=1966a29de6bb48909fff904098d1507a
```

Open **that exact address** in your browser. The `t=` part is a one-time access token created at each start; without it the app answers "Access token required". After the first visit the browser keeps a cookie, so a refresh works.

Stop it with `Ctrl+C`.

### Options

| Option | Meaning |
|---|---|
| `--config <file>` | settings file (default `redactor.config.json`) |
| `--input <folder>` | where the source documents are (default `input.directory` in the config) |
| `--output <folder>` | where redacted files are saved (default `output.directory`) |
| `--port <number>` | port to listen on (default 5199) |

`ASPNETCORE_ENVIRONMENT=Development` is needed so the page's style and script files are served when running from a build folder.

## Using the app

1. **Pick a document** in the left list. Search, sort and the type filter help find it. Statuses show what has been processed.
2. **Press Redact.** The button names the model that will run. The toolbar shows a time forecast, with a warning for very large documents. Progress is shown at the bottom.
3. **Review.** The original is on the left with every finding highlighted, the redacted version in the middle, and a list of every edit on the right with a **High / Medium / Low** confidence badge. Click an edit to jump to it.
4. **Correct it.** Select text in the original to redact it by hand (with or without a model), or select an edit to reject it, change its category or remove it. Undo and Redo are available, and **Unsaved changes** shows until you save.
5. **PDFs and scans:** the **Pages** button shows the real pages with highlights; **Draw box** blacks out a signature or logo.
6. **Save.** The first result is saved to the output folder automatically (`name-redacted.ext`). **Use as output** or **Save changes** writes a different or corrected result.

### Buttons in the top bar

- **🔍 Prompt:** shows exactly what is sent to the model and what came back.
- **☰ Categories:** switch kinds of sensitive data on or off, or set them to *Flag only*, for this session. Nothing is written to the config file.
- **⚙ Models:** see which models are downloaded and choose which ones also run to grade confidence.
- **Next run:** the model used by the next Redact. You can switch model at any time and run again; each model keeps its own result tab.

Each browser gets its own session, so two people (or two browsers) never see each other's work.

## Settings

Everything is in `redactor.config.json`: folders, which categories are on, the model, chunk sizes, OCR and PDF options, and the warning limits. See section 7.5a of `SPECIFICATION.md`. A wrong or unknown setting stops the app at start with a message listing every problem.

## Troubleshooting

| Problem | Fix |
|---|---|
| "Access token required" | use the full address printed at start, including `?t=...` |
| Banner says the model is not installed | `ollama pull <name>` then press the ⟳ button next to the model picker |
| "Cannot reach Ollama" | start Ollama; check `llm.endpoint` in the config |
| Page has no styling | start with `ASPNETCORE_ENVIRONMENT=Development` as shown above |
| Redact button is greyed out | the hint text next to it says why (no document, unreadable file, model missing) |
| A scan takes a few seconds to open | it is being read with OCR; this happens once per file per session |
| Port already in use | add `--port 5200` |

## Optional: the GLiNER second opinion

The app can run a small second detector (GLiNER) next to the language model. Where both find something, it is marked as agreed; something **only GLiNER found** is flagged for review and left in the text, with its reason and score, and you can accept it (**Redact this**) or dismiss it. It is off by default.

1. Put the model files in `models/gliner-pii-edge/` (not part of the repository; they are about 50 MB): `model_quint8.onnx`, `tokenizer.json`, `gliner_config.json`. They come from `knowledgator/gliner-pii-edge-v1.0` on Hugging Face (Apache 2.0), downloaded once; the app never downloads anything while it runs.
2. **For the demo, use the switch:** open **☰ Categories** and turn on **Second opinion (GLiNER)**. It lasts for that browser session only and is never written to the config (the *Reset to config file* button turns it back off). If the model files are missing the switch is greyed out and says where to put them. A second choice sets what happens to something only GLiNER finds: *Flag for review* (the default) or *Redact*. To have it on from the start, add `"gliner": { "enabled": true }` in `redactor.config.json` instead (other settings, such as the threshold, what happens to GLiNER-only finds, and the label wording per category, are described in the specification, requirement FR73).
3. Start the app as usual. If GLiNER is on and the model files are missing, a run stops with a message saying so.
4. In the edit list, each edit shows where it came from (Rule, AI, AI + GLiNER, GLiNER only, and so on). Use the filter *Flagged for review only* to see the GLiNER flags.

## The results explorer

Beside the redaction screen, the web app has a **results explorer** at `/explorer` (the **📊 Results** button in the top bar). On a fresh checkout the `datasets` folder is empty: run `./tools/restore-datasets.sh` first, which unpacks the two evaluation datasets kept in git in `datasets-archive/` (see its [README](../datasets-archive/README.md)). The explorer reads a dataset from the `datasets` folder (see [HOW_TO_RUN_EVALUATION.md](HOW_TO_RUN_EVALUATION.md) for making one; use `--datasets <folder>` to read another folder) and uses the same access token as the rest of the app. The first time a dataset is opened a SQLite database is built beside it (a few seconds); it is rebuilt automatically if the dataset changes and can be deleted at any time.

| Tab | What it shows |
|---|---|
| Leaderboard | Every setup's recall, precision and F1 with 95% ranges, over the documents that match the filters |
| Results | One row per setup per document, sortable, filterable, with CSV export of the rows shown |
| Documents | Every document rated Easy, Moderate, Hard or Problem by how the models did; open one to see its text with each setup's work drawn on it |
| Categories | A heat-map of recall, misses and over-redactions by category and setup, a drill-down to the items, and the items models miss or wrongly redact again and again |
| Combine models | Scores any combination of the saved detectors (union, agreement, flags, an ideal reviewer) without running a model |
| How we got here | The methodology document |

Every control and column has an ⓘ button that explains it in technical and plain terms with examples.

**Try with another model.** On a document's page in the explorer, "Choose a model…" lists the models installed in your local Ollama. "Run on this document" runs the real pipeline (rules, the model, and optionally GLiNER), scores it against the answer key the same way as the evaluation, and adds it to that document's results as a *live* result, saved in the dataset's `live` folder. Only local models can be chosen, and the run can be cancelled. Live results are left out of the leaderboard and ratings unless "Include live runs" is ticked. The answer keys are read from `tests` (use `--corpus-root <folder>` to change that), and the documents from the input folder.

