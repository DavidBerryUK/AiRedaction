# Story-001: Results Explorer

**Status:** In progress (proof of concept). Built so far: the dataset format, the interim dataset and validator, the SQLite import, and the explorer page with leaderboard, results grid, documents and difficulty ratings, document view, categories, combinations, methodology, CSV export and help dialogs. Still to build: the live "try with another model" run.
**Related:** [Story-002: Ultimate Evaluator](Story-002-Ultimate-Evaluator.md) (produces the data this story reads), [HOW_TO_RUN_EVALUATION.md](../HOW_TO_RUN_EVALUATION.md), [EVALUATION_FINDINGS_V5.md](../EVALUATION_FINDINGS_V5.md)

## Summary

As a person demonstrating or assessing the redaction tool, I want an interactive page in the web app where I can explore every evaluation result, so that I can quickly see which models and documents perform well or badly, why, and what happens if I try a different model on a document.

This is a proof-of-concept explorer for demonstrations and quality assurance. It is not part of the end-user redaction workflow.

## Decisions made

| Question | Decision |
|---|---|
| Live "try this document with model X" | **In scope.** Live runs are saved and added to the results |
| Where it lives | **A new page** (`/explorer`) in the existing web app, separate from the redaction page |
| Access | **Same access token** as the rest of the app (proof of concept, no separate login) |
| Human review of the answer key | **Later** (out of scope; see Future work) |
| Storage | The versioned run files and CSV are the record; **SQLite is a derived store** for the UI |
| Versions | **One final dataset only**, produced by Story-002 with every result scored against the same code and the same answer key. Earlier runs (v1 to v6) are not shown in the explorer. The history and reasoning are told in a separate methodology page instead |
| Which models | **Local Ollama models only.** No online or hosted models (such as Claude) are used for live runs or evaluation: documents never leave the machine, and there are no per-token costs |

## Data model

The dataset is a folder of CSV files, plus one JSON Lines file holding the extracted text of each document, so the coloured view needs neither the original file nor a repeat of the extraction. The exact files, columns and checks are in [RESULTS_DATASET_FORMAT.md](../RESULTS_DATASET_FORMAT.md). Documents are referenced by a path relative to the `in/` folder.

Planned order of work: (1) agree the format, (2) make an interim dataset from the existing runs with a converter, (3) build the explorer against it, (4) change the evaluator to write the final dataset in the same format, with a validator that checks both.

- **Source of truth:** the versioned run files in `documentation/evaluation-reports` (and `eval/`), which are never edited by the explorer.
- **Import:** a step that reads the run files and the answer keys and builds a SQLite database (`eval/explorer.db`, git-ignored). It can be deleted and rebuilt at any time.
- **CSV export:** long-format CSVs (one row per document × model × run, and one per entity outcome) so the data opens in Excel and can be diffed in git.
- **Live results:** written to SQLite when run, and appended to `live-results.csv`. Rebuilding replays the CSV, so live runs are not lost.
- **Tables (outline):** `runs`, `documents`, `models`, `results` (document × model × run), `entities` (the answer key), `outcomes` (entity × result: found, missed, flagged, over-redacted), `edits` (spans with their source: rule, model, GLiNER).
- **Every row records** the model, Ollama version, settings that affect results (think level, GLiNER on or off and threshold, rules on or off), answer-key version, and `source` (`batch` or `live`).

## Acceptance criteria

### Browse and filter
- [ ] The page is reachable from the app header and requires the existing access token.
- [ ] A results grid shows one row per document × model, with recall, precision, F1, missed, over-redacted, flags and time.
- [ ] The grid can be filtered by run, model, corpus, document type, format, category, and live or batch, and sorted by any column.
- [ ] Any filtered view can be exported to CSV.

### Best and worst
- [ ] A leaderboard ranks models by a chosen measure (recall, precision, F1, time) for the current filter.
- [ ] A "worst documents" list for a chosen model links to each document.

### One document, all models
- [ ] A document page lists how each model did on that document.
- [ ] The document text is shown with spans coloured as correct, missed, over-redacted and flagged, with a switch to view each model's result.
- [ ] The page shows the source of each span (rule, model, GLiNER) and the answer-key entry it matched.

### Problem documents
- [ ] Each document has a difficulty rating (Easy, Moderate, Hard, Problem) based on misses and over-redactions across the models.
- [ ] Documents can be sorted and filtered by rating, including "missed by every model".
- [ ] The documents that fall in the Problem band can be listed together with the items that caused it, to show where the answer key itself may be wrong.

### Categories
- [ ] A category × model heatmap shows recall and precision per cell.
- [ ] Clicking a cell lists the missed or over-redacted items with their surrounding text.
- [ ] A "most-missed items across all models" list is available.

### Try with another model (live)
- [ ] From a document page the user can choose any model installed in the local Ollama and run the real pipeline on that document. No online or hosted model can be selected, and the page makes no calls to external services.
- [ ] Progress is shown and the run can be cancelled.
- [ ] The result is scored against the answer key and shown beside the batch results for the same document.
- [ ] The live run is saved with its model, Ollama version, settings and a `live` tag, and appended to `live-results.csv`.
- [ ] A warning shows when a live run's settings differ from the batch run it is compared with, and when the machine was busy.
- [ ] Runs are queued one at a time and the previous model is unloaded first, to avoid running out of memory.
- [ ] A document with no answer key (an upload) can be run and viewed, and is labelled "no key, unscored".

### Combine and compare
- [ ] The user can tick two or three models, choose a voting strategy, and see the combined recall and precision.
- [ ] The page header shows the single dataset in use: code version (git commit), answer-key version, date, Ollama version and models. Live runs are always compared with this dataset.

### Methodology page
- [ ] A "How we got here" page in the explorer explains the process and the reasons for each choice, for example why the prompt is worded as it is, why rules run before the models, why the verbatim check exists, why `think` is off for most models, why GLiNER only flags, how the answer key was built and audited, and what the limits are.
- [ ] Each decision links to the evidence behind it (the relevant findings document), but the page does not show earlier runs as selectable datasets.
- [ ] The same content is kept as a document in `documentation` (the page renders it), so it can be read and reviewed without running the app.

## Tasks

1. Define the SQLite schema and the CSV formats.
2. Build the importer (run files and answer keys to SQLite and CSV), with a command to rebuild.
3. Add the `/explorer` page, navigation, and access-token check.
4. Results grid with filters, sorting and CSV export.
5. Leaderboard and worst-documents views.
6. Document page with the coloured text view and per-model switch.
7. Difficulty rating and the problem-documents list.
8. Category heatmap and drill-down.
9. Live "try with model X": queue, progress, scoring, save to SQLite and CSV.
10. Combination explorer.
11. Methodology page and its source document.
12. Tests for the importer, scoring queries and the live-run recording.
13. Update the specification and the how-to documents.

## Dependencies

- **Story-002 provides the final dataset.** The explorer is built to read that format. During development the importer can be tried on the existing held-out and formats runs, but those are only test data and are not shown as results.
- Answer keys (existing).
- The existing redaction pipeline and `EvalScoring`, to run and score live runs.
- Ollama, for the live runs only. The rest of the page works without it.
- `Microsoft.Data.Sqlite`.
- The existing web app access token.

## Out of scope

- Reviewing or correcting the answer key from the page (see Future work).
- Editing or deleting batch run data.
- Per-user accounts, or sharing live runs between users. Live results are local to the machine.
- Running the full evaluation from the page (see Story-002).
- Online or hosted models of any kind (they would send document text off the machine and cost tokens).

## Future work

- **Answer-key review:** confirm or overturn the audit decisions from the page, to build the human-checked sample the V5 findings call for.
- Promote a live run into a formal, versioned run.
- Comparing two dataset versions, if the tool is later re-evaluated after changes (for example a new final run after a new rule).
- "Run all missing models" for a filtered set of documents.

## Notes

- The explorer shows document text, and live runs use machine time, so it sits behind the same access token as the rest of the app.
- Timings from live runs are unreliable when the machine is busy (as seen in the formats run), so they are flagged and not used for rankings by default.
- The audited held-out key was built from model agreement, so the explorer should show the key version beside any score.
