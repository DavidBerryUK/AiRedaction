# Story-003: Implementation plan

**Status:** Plan for approval. No code written yet. Branch: `story-003-view-previous-run`.
**Story:** [Story-003: View a previous run](Story-003-View-Previous-Run.md) (the agreed design this plan builds)
**Standards:** [CODING_STANDARDS.md](../../CODING_STANDARDS.md)

## 1. Goal in one paragraph

When the user picks a document, look in the evaluation datasets for stored results. For each model that has one, add a **previous-run result button** to the Redacted panel's existing button row, with preview text. Clicking it shows that run in the Redacted and Edits panels, read-only, under a clear "previous run" notice. Only the latest plain result per model is shown. The redacted text is not stored, so it is rebuilt from the dataset's saved text and spans using the same code a live run uses.

## 2. What the code already gives us

| Need | What exists | Where |
|---|---|---|
| Stored results per document | SQLite tables `documents`, `results`, `spans`, `texts` built from each dataset folder | `Explorer/ResultsDatabase.cs`, `Explorer/Dataset/Schema.cs` |
| Reading them | `ExplorerService.DocumentAsync(docId)` (text plus every result row), `ConfigViewAsync` (markers) | `Explorer/ExplorerService.cs` lines 185, 211 |
| Listing datasets, newest first | `ExplorerCatalog.ListAsync()`, `OpenAsync(id)` | `Explorer/ExplorerCatalog.cs` lines 48, 79 |
| Rebuilding redacted text | `Redactor.Apply(text, spans, template)`. The same call Eval uses to score results | `Core/Redactor.cs` line 40 |
| Placeholder format | `options.Redaction.PlaceholderTemplate`, default `[REDACTED:{type}]` | `Core/Options.cs` line 229 |
| Matching a document to a dataset row | `RelativePath` with `\` changed to `/` equals the dataset `doc_id` | `DocumentListViewModel.cs`, `RedactionSession.FileUrl` |
| Detecting a changed original | SHA-256 of the text (`DatasetWriter.HashText`), and the precedent in `LiveRunner` | `Explorer/Dataset/DatasetWriter.cs`, `Eval/LiveRunner.cs` |
| The result buttons, panels, Edits count | `RedactionApp.razor` segments row, Redacted body, Edits aside | `Ui/RedactionApp.razor` lines ~185 to 345 |

## 3. Findings that shape the plan

1. **No dataset is on disk.** The datasets are only archived (`datasets-archive/*.tar.gz`). The feature needs `tools/restore-datasets.sh` run first. Without it, no previous-run buttons appear, and the app must behave exactly as it does today.
2. **The first open of a dataset is slow.** The SQLite file is built on demand (about 100 MB of CSV for the larger dataset) the first time a dataset is opened, and it is not built at startup.
3. **There is no per-result date.** Only the dataset's `createdAt` exists. "Latest" therefore means the newest **dataset** that has a result for that document and model. All stored repeats are 1.
4. **The queries we need do not exist.** `GridRow` and `ConfigViewAsync` carry no result id, and `ConfigViewAsync` filters by config, not by result. A new query is needed.
5. **`ViewModels` cannot see `Explorer`.** It references only `Core`. The session must reach stored results through an interface it owns, implemented elsewhere.
6. **Live runs replace results by model name** and evict old ones. Stored results kept in the same list would be overwritten or evicted by a live run of the same model, which breaks "both shown, not merged".
7. **Most review actions do not check `CanReview`** (reject, accept flag, restore, change category, undo, redo, remove area, save, "Redact anyway"). Read-only needs explicit gating.
8. **The Razor components cannot be unit tested** (the test project has no bUnit and no UI reference). Logic must live in `ViewModels` or `Explorer`, with the `.razor` kept thin.
9. **Checked: the final datasets match `in/` exactly.** `RESULTS_DATASET_FORMAT.md` (section 11) records nine files that differ from the corpus copies the older interim dataset was built from. On 7 October 2026 every document in both final datasets was re-read from `in/` with the app's own readers (including OCR) and its text hash compared with the dataset's: `final-20261003` 338 of 338 match, `final-20261004` 750 of 750 match, none missing, none unreadable. So stored results from the final datasets are valid for every document in them. The interim dataset (older, built from other copies) is not used by this feature.
10. **`RedactionSession.cs` is 1,078 lines and `RedactionApp.razor` is 688**, over the 400-line guideline. New session logic goes in a new partial class file.

## 4. Design

### 4.0 Principle: the documents in `in/` never change

The documents in `in/` are fixed, so a result stored against one stays valid. Previous-run code only **reads**. It never writes to the input folder, never alters the loaded document (`OriginalText`, `OriginalDoc`, word boxes), and never puts dataset text into the Original panel. The dataset text is used for one thing only: the hash check that proves it matches the file on disk. Because `in/` is fixed, a mismatch is not expected in normal use. It means a data fault (a dataset built from a different copy, or a reader that now extracts differently), so the check stays as a safety net: the stored run is shown as unavailable with a plain reason, and we never substitute the dataset's text for the file's. A test asserts the input file bytes and the session's loaded original are identical before and after loading and viewing a previous run.

### 4.1 Data access: a new query in Explorer

Add to `ExplorerService` a method that returns, for one `doc_id`, the stored plain result of each model:

- Filter: `model <> ''`, `variant = 'plain'`, `status = 'ok'`, `source = 'batch'` (live explorer runs are excluded; they are not "previous evaluation runs").
- Return per model: result id, model, edit count, the spans (type, start, length, confidence, source, flag), the document text, the text hash, and timings and tokens (for the footer).

Add a new type, `StoredRun`, for the returned data. Put the "latest per model across datasets" logic in a small new class, `PreviousRunFinder` (in Explorer): walk `ExplorerCatalog.ListAsync()` newest first, open each, ask for the document, and keep the first result seen for each model. Each `StoredRun` carries the dataset id and the dataset's `createdAt`.

No new tables and no change to the dataset format.

### 4.2 The seam between the session and Explorer

In `ViewModels`, define `IPreviousRunSource` with one method: given a document's relative path, return the stored runs. `RedactionSession` takes it through an init property, like `ConfigPath` and `OcrEngineName`. It is optional (null means the feature is off). `Web/Program.cs` registers an adapter, in a new small class in `App.Web` that wraps `PreviousRunFinder`. This keeps `ViewModels` free of any dependency on `Explorer`, and keeps `Program.cs` minimal.

### 4.3 Rebuilding the result (the riskiest part)

A new `PreviousRunBuilder` in `ViewModels` turns a `StoredRun` into a `ModelResult`:

1. Map each saved span to `DetectedEntity(type, start, length, confidence, source, flag)`. A missing confidence becomes 0.
2. Call `Redactor.Apply(originalText, spans, options.Redaction.PlaceholderTemplate)`. This gives the redacted text and the edits with positions. Flagged spans stay in the text as `Flagged` edits, exactly as in a live run.
3. Wrap it in a `ModelResult` with the model, the dataset's timings and tokens, and a new `PreviousRun` marker (section 4.4).

Edit ids come from `Apply` (numbered by position), not from the dataset's span ids. That is fine because they are only used to link the highlight to the list.

**Guard:** the builder verifies that the hash of the text the dataset saved matches the text the session loaded for the document (`S.OriginalText`). On a mismatch, no result is built, and the result is shown as unavailable (section 4.6).

### 4.4 Keeping previous runs apart from live runs

- Add `PreviousRunInfo(DatasetId, DatasetCreatedAt, SourceModel)` and an optional `PreviousRun` property on `ModelResult`, with `IsPreviousRun`.
- The session keeps previous runs in **their own dictionary**, keyed by document path, separate from live `results`. They are never touched by live-run replacement or the `MaxResultsPerDocument` eviction.
- `ResultsForSelected` stays live-only. A new `PreviousRunsForSelected` lists the stored ones. `ActiveResult` and `SelectResult` search both lists, so every existing panel that reads `ActiveResult` works unchanged.
- `ConfidenceGrader`: a previous run is graded on its own, as if it were the only model that ran. It does **not** vote for live runs, and live runs do not vote for it. (Confirm in the grader code when building; the aim is that a previous run's grades do not change when live runs are added.)
- Loading: in `SelectAsync`, once the document has loaded, ask the source for stored runs and build the results. This must not block the page. Show the document immediately and fill the buttons when the lookup returns (section 4.7).

### 4.5 Read-only

Add `IsReadOnlyView => ActiveResult is { IsPreviousRun: true }` in the session and fold it into `CanReview`. Then explicitly guard the actions that do not check `CanReview` today: `RejectEdit`, `AcceptFlag`, `RestoreEdit`, `ChangeType`, `RemoveArea`, `Undo`, `Redo`, `SaveAsync`, `UseAsOutputAsync`, the "Redact anyway" path, and `AddManual` and `AddArea`. A guarded action does nothing and changes nothing. A previous run is never written to the output folder and never persisted by `ReviewStore`.

### 4.6 UI changes (`RedactionApp.razor`)

- **Button group.** After the live buttons, a labelled group "Previous runs" (own CSS class, scrolls horizontally when long). Each button shows the model and edit count, a small "previous" marker, and distinct styling. Same `role="tab"` pattern as the live buttons.
- **Preview text.** A `title` tooltip in the same style as the live buttons: model, edit count, dataset id, dataset date, and "previous run". If the original does not match, the button is disabled and the tooltip says why.
- **Notice.** A banner between the pane header and the redacted text: "This is part of a previous run: <model>, <dataset>, <date>. It is a stored record and cannot be edited." Shown only while a previous run is active.
- **Review tools.** Hide Undo, Redo, "Unsaved changes" and "Use as output / Save changes" while a previous run is active.
- **Edits panel.** Unchanged for the count and list. The per-edit action panel and "Redact anyway" are hidden because `CanReview` is false.
- **Footer.** Uses the stored timings and tokens, and says "previous run" instead of "saved to output".
- **Moved original.** When the hash does not match, the Original panel shows a short message that the file differs from the one the stored run used, so no previous-run highlights are shown.
- **Pages (PDFs and scans).** Page view is supported for previous runs. The Pages/Text toggle stays available, and the redacted page images come from the stored run: the page route (`/results/{id}/page/{n}`) and `RenderRedactedAsync` look in the previous-run list as well as the live results, then draw black boxes from the rebuilt edits and the word boxes of the loaded document, as for a live run. This is safe because a previous-run button is only enabled when the stored text matches the loaded file (section 4.3), so the edit positions line up with the page words. `RenderStamp` and the render cache must cover previous runs too. Page view is **not** offered when the text does not match (the button is already disabled).
- **Banner for model info.** The "model not installed" banner reads `Info` from the live model list. A previous run's model may not be installed, so the banner must not show for a previous run.

### 4.7 Latency and failure

- The lookup runs in the background after the document appears. The buttons fill in when it finishes. If the user changes document first, the late result is discarded.
- The first open of each dataset builds its SQLite file, which can take a while. Proposal: begin warming the datasets in the background when the session starts, so the cost is paid once, off the request path. A small "Looking for previous runs…" indicator shows while it runs.
- Any failure (no datasets folder, a bad dataset, a locked file) is caught, written to the log, and results in no previous-run buttons. It never stops the page, and live runs are unaffected.

## 5. Build order

Each step leaves the app building and the tests passing.

| # | Step | Tests (xUnit, synthetic data) |
|---|---|---|
| 1 | `StoredRun` and the new `ExplorerService` query (the dataset check is done, see finding 9) | Using `DatasetTests.MakeCorpus` and `InterimConverter`: returns the plain `ok` batch result per model with its spans; ignores variants, baselines, live rows, and non-`ok` rows |
| 2 | `PreviousRunFinder`: latest per model across datasets | Two datasets with the same document and model: the newer wins; a model only in the older dataset is still returned; no datasets gives an empty list |
| 3 | `PreviousRunBuilder` in `ViewModels` | **Rebuilt text and edits equal `Redactor.Apply` and a live `ModelResult` for the same spans** (reuse `FakeModel` from `SessionTests`); flagged spans stay in the text; hash mismatch refuses |
| 4 | `ModelResult` marker, session dictionary, `PreviousRunsForSelected`, `ActiveResult`/`SelectResult` across both lists, `IPreviousRunSource` wiring in `SelectAsync` | A live re-run of the same model leaves the stored one in place; eviction never removes it; changing document discards a late lookup; no source means nothing changes |
| 5 | Read-only gating and source-unchanged guarantee | Each guarded action leaves state, review store and output folder unchanged for a previous run |
| 6 | Adapter in `App.Web` and registration in `Program.cs` | Adapter returns runs for a path with `\` separators; honours `--datasets` |
| 7 | Page view: let `RenderRedactedAsync`, `RenderStamp` and the render cache serve previous runs; keep the Pages/Text toggle | A previous-run result renders a PDF page to PNG with boxes over exactly the edited words; a mismatching document is refused; a live re-run does not reuse a stored render |
| 8 | UI: button group, preview, notice, hidden tools, footer, Original message, text-only | Manual check in the browser (no component test setup exists); see section 6 |
| 9 | Background warm-up and the "looking" indicator | Warm-up failure is swallowed and logged |
| 10 | Documentation: a short section in `HOW_TO_RUN_WEB_APP.md` (restore the datasets, the `--datasets` flag), a status update in the story | n/a |

## 6. Verification

- `dotnet test` passes, with the new tests above.
- Manual check, run from Visual Studio or `./run-web.sh` after `tools/restore-datasets.sh`:
  1. Pick a document that is in the dataset: previous-run buttons appear, one per model, with preview text.
  2. Click one: the Redacted panel shows the notice, the Edits count matches the list, highlights line up in both panels.
  3. Try to edit, reject, save: nothing changes. Undo, Redo and Save are hidden.
  4. Run a model live: its own button appears beside the stored ones, and the stored one is unchanged.
  5. Pick a document with no stored results: identical to today.
  6. On a PDF and on a scan with stored results: switch to page view, and the redacted pages show black boxes over the same words listed in the Edits panel. Switch back to text view and the highlights agree.
  7. Pick one of the nine documents that differ: the button is disabled with the reason, and the Original panel explains.
  8. Run with no `datasets` folder: no buttons, no errors.
- Compare one stored run on screen with the same document and model in the explorer page, to check the edit counts agree.

## 7. Risks

| Risk | Mitigation |
|---|---|
| Rebuilt text differs from what was scored | Same `Redactor.Apply`, same template, and the step 3 test compares against a live result |
| Dataset text differs from the file now on disk | Hash check, disabled button, clear message |
| First dataset open is slow | Background warm-up, and the lookup never blocks the page |
| Stored runs leak into live behaviour (voting, saving, eviction) | Separate dictionary, read-only gating, tests for each |
| `RedactionSession.cs` grows further | New partial class file for all previous-run logic |
| Page images drawn from the wrong words if the text differs | Hash check gates the button; test that a mismatch is refused; manual check on a PDF and a scan |
| No component tests for the Razor changes | Logic lives in `ViewModels` and `Explorer`, `.razor` kept thin, manual checklist in section 6 |
| `ConfidenceGrader` surprises on a single stored result | Read the grader at step 4 and add a test before wiring the UI |

## 8. Decisions this plan makes (change them if you disagree)

1. **"Latest" is decided by dataset date**, since results have no timestamp of their own.
2. **Live explorer results (`source = live`) are not shown** as previous runs. Only batch evaluation results are.
3. **Page view is supported for PDFs and scans**, only when the stored text matches the loaded file.
4. **A text mismatch disables the button** with a reason, as a safety net, rather than showing misaligned highlights. `in/` is fixed, so this should not happen in normal use.
5. **Stored runs do not vote** in confidence grading with live runs.
6. **Warm the datasets in the background** at session start.

## 9. Out of scope (as in the story)

Comparing runs, saving a redacted file from a stored run, editing or re-scoring a stored run, exporting from one, variant results (rules, GLiNER, combined), showing answer-key scoring, and any change to how Eval writes datasets.
