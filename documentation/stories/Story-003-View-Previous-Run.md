# Story-003: View a previous run

**Status:** Draft (design only, no code yet). Developed on branch `story-003-view-previous-run`.
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

## What the user sees

- The **Redacted** panel shows the redacted text from the previous run.
- A clear, always-visible notice at the top of the Redacted panel says that this is **part of a previous run**, so it cannot be mistaken for the result of a run just made. It should say which run it is (date and time of the run, the model or detector configuration, and the dataset it came from).
- The **Edits (n)** panel lists every edit from that run, with the same category, confidence grade and status shown for a live run. The count in the heading matches the number of edits listed.
- The Original panel shows the source document alongside, as in a live run, where it is still available.

## Acceptance criteria

- [ ] The Redacted text is rebuilt from the dataset's document text and spans, and the document text is verified against its `text_hash` before display; a mismatch shows a message, not wrong highlights.
- [ ] A user can choose a previous run for a document and have it load into the Redacted and Edits panels.
- [ ] The Redacted panel displays a clear "previous run" notice with the identity (date/time, model) of the run, for as long as that run is shown.
- [ ] The notice is not shown for a live run, and disappears when the user starts a new run or returns to the live result.
- [ ] Every edit from the previous run appears in the Edits panel, and the heading count equals the number listed.
- [ ] Edit highlighting in the Redacted and Original panels works as for a live run.
- [ ] Viewing a previous run never changes it, never re-runs a model, and never modifies or deletes the original documents or saved results.
- [ ] If the saved run is missing or unreadable, the user sees a plain message and the live view is left unchanged.
- [ ] Local-only: nothing is sent off the machine; the same access token protects the view.

## Open questions (to agree before building)

| Question | Notes |
|---|---|
| How is a run picked? | A per-document "previous runs" list or dropdown, or opened from the explorer results grid. |
| Read-only or editable? | Proposal: read-only, with manual redaction and accept/reject disabled, so a past run is a faithful record. |
| Does the Original panel need the source file? | If the original has moved or changed, the panel should say so rather than show misaligned highlights. |
| Is saving or exporting from a previous run allowed? | Proposal: no, in this story. |

## Out of scope

- Comparing two runs side by side.
- Editing or re-scoring a previous run.
- Changing how Eval produces or stores results. The dataset already holds the spans needed.
- Showing the answer key or scoring in the panels (a possible later addition, since `outcomes.csv` has it).

## Notes

- Follows the standing constraints: synthetic documents only, local-only processing.
- No code is written until the design and open questions above are agreed.
