# Story-003: View a previous run

**Status:** Design agreed, ready to build (no code yet). Developed on branch `story-003-view-previous-run`.
**Related:** [Story-001: Results Explorer](Story-001-Results-Explorer.md), [Story-002: Ultimate Evaluator](Story-002-Ultimate-Evaluator.md)

## Summary

As a person reviewing redaction work, I want to open the results of a run that has already been done and see them in the same **Redacted** and **Edits** panels I use for a live run, so that I can inspect what was redacted without running the models again.

## Why

At present the Redacted and Edits panels only show a run made in the current session. A run that has already finished (a saved or earlier run) cannot be looked at again without repeating it, which takes time and may give different results. Reusing the existing panels keeps the review experience the same for live and past runs.

## Where previous runs come from (decided)

A previous run is a result recorded by the evaluation program `AiDocumentRedactor.Eval`, which has run hundreds of evaluations. They are held in the results dataset (see [RESULTS_DATASET_FORMAT.md](../RESULTS_DATASET_FORMAT.md)), archived in `datasets-archive/` and read into the explorer's SQLite store (Story-001).

One previous run = one row of `results.csv` (a document, a detector configuration such as a model or rules or GLiNER, and a repeat). What the dataset holds for it:

- The text the detectors saw: `document-text.jsonl`.
- Every redaction or flag it produced: `spans.csv` (category, position, confidence, source, and whether it was redacted or only flagged). These become the Edits.
- The judged outcomes (caught, missed, over-redacted): `outcomes.csv`.

**The redacted text is not stored.** It must be rebuilt by applying the spans to the document text, using the same replacement rules as a live run. Flagged spans stay in the text, as in a live run. A check that the rebuilt text matches a live run of the same spans belongs in the acceptance criteria.

## How a previous run is picked (decided)

The Redacted panel already has one **result button per model** (the tabs at the top of the panel). Each time the user runs a model on the selected document, a new button appears, showing the model name and its edit count. This story extends that, so no new picker is needed:

1. When the user **picks a document**, the app checks which datasets hold results for it (matched on the document's path relative to the input folder, the dataset's `doc_id`).
2. For every model with a stored result, the app **pre-populates a result button**, in the same row as the buttons for live runs, before the user runs anything.
3. Each pre-populated button carries a **preview**: a short hover or inline text showing what the run is, without opening it. Proposed content: model, edit count, the dataset and run date, and that it is from a previous run. The existing buttons already show model, count and timing on hover; the previous-run preview follows the same pattern.
4. A previous-run button is **visibly distinct** from a live one (for example a "previous run" marker or different style) so the two are never confused.
5. Clicking it loads that run into the Redacted and Edits panels, with the previous-run notice below. The user can still run a model live, and its new button appears beside them.
6. If a model has both a stored and a live result, both are shown and labelled, not merged.

**Several results for one model (decided): show the latest.** Where the datasets hold more than one result for the same document and model (repeats, or the same document in more than one dataset such as the 3 and 4 October runs), only the **most recent** is shown, one button per model. Recency is the run's date and time. Older results are not shown.

**Decided:** a dataset also holds variants per document (rules only, GLiNER alone, "with GLiNER" and its accepted-flags forms). The proposal is that a model's button shows its latest **plain** result (the model on its own, matching what the user gets when running a model live today), and that rules-only, GLiNER-only and combined variants are not shown in this story.



## What the user sees

- The **Redacted** panel shows the redacted text from the previous run.
- A clear, always-visible notice at the top of the Redacted panel says that this is **part of a previous run**, so it cannot be mistaken for the result of a run just made. It should say which run it is (date and time of the run, the model or detector configuration, and the dataset it came from).
- The **Edits (n)** panel lists every edit from that run, with the same category, confidence grade and status shown for a live run. The count in the heading matches the number of edits listed.
- The Original panel shows the source document alongside, as in a live run, where it is still available.

## Acceptance criteria

- [ ] The Redacted text is rebuilt from the dataset's document text and spans, and the document text is verified against its `text_hash` before display; a mismatch shows a message, not wrong highlights.
- [ ] Picking a document with stored results shows one pre-populated result button per stored model, without running anything.
- [ ] At most one stored button per model is shown, and it is the most recent result for that document and model.
- [ ] Each such button has preview text (model, edit count, dataset and date, "previous run").
- [ ] A document with no stored results looks and behaves exactly as today.
- [ ] Clicking a pre-populated button loads that run into the Redacted and Edits panels.
- [ ] Running a model live still adds its own button next to the previous-run ones.
- [ ] The Redacted panel displays a clear "previous run" notice with the identity (date/time, model) of the run, for as long as that run is shown.
- [ ] The notice is not shown for a live run, and disappears when the user starts a new run or returns to the live result.
- [ ] Every edit from the previous run appears in the Edits panel, and the heading count equals the number listed.
- [ ] Edit highlighting in the Redacted and Original panels works as for a live run.
- [ ] Manual redaction, accept/reject and save/export are disabled while a previous run is shown.
- [ ] If the original file has moved or changed, the Original panel says so.
- [ ] The source document content never changes: the original file is never written, and the Original panel always shows the document as loaded from disk, never text taken from a dataset.
- [ ] Viewing a previous run never changes it, never re-runs a model, and never modifies or deletes the original documents or saved results.
- [ ] If the saved run is missing or unreadable, the user sees a plain message and the live view is left unchanged.
- [ ] Local-only: nothing is sent off the machine; the same access token protects the view.

## Further decisions

| Question | Decision |
|---|---|
| Which variant per model | The latest **plain** result. Rules-only, GLiNER-only and combined variants are not shown |
| Many buttons | The previous-run buttons sit in their own labelled group, scrolling if the row is too long |
| Read-only or editable? | **Read-only.** Manual redaction and accept/reject are disabled, so a past run is a faithful record |
| Original panel | If the source file has moved or changed, the panel says so rather than showing misaligned highlights |
| Save or export | **Not allowed** from a previous run, in this story |

## Out of scope

- Comparing two runs side by side.
- Editing or re-scoring a previous run.
- Changing how Eval produces or stores results. The dataset already holds the spans needed.
- Showing the answer key or scoring in the panels (a possible later addition, since `outcomes.csv` has it).

## Notes

- Follows the standing constraints: synthetic documents only, local-only processing.
- The design is agreed. Build on this branch.
