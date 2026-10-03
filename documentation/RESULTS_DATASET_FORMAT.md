# Results dataset format

**Status:** v0.1. Implemented by the interim converter (`--export-dataset`) and the validator (`--validate-dataset`) in the evaluation project; the final evaluator (Story-002) will write the same files.
**Used by:** [Story-001 Results Explorer](stories/Story-001-Results-Explorer.md) (reads it), [Story-002 Ultimate Evaluator](stories/Story-002-Ultimate-Evaluator.md) (writes it), and a converter that makes an interim dataset from the existing saved runs.

The dataset is a **folder of plain files**. It is the durable record of an evaluation. The explorer's SQLite database is built from it and can be deleted and rebuilt at any time.

```
dataset-<id>/
  run.json              what was run: code, settings, models, machine, key versions
  documents.csv         one row per document
  document-text.jsonl   one line per document: the extracted text the detectors saw
  entities.csv          the answer key, one row per item (and must-keep and ignore items)
  results.csv           one row per document × detector configuration × repeat
  spans.csv             one row per redaction or flag a detector produced
  outcomes.csv          one row per judged fact: caught, missed, over-redacted, and so on
```

## 1. Rules for every file

| Rule | Detail |
|---|---|
| Encoding | UTF-8 without a byte-order mark, LF line endings |
| CSV | RFC 4180: header row, comma separator, fields quoted when they contain a comma, quote or line break |
| Empty | An empty cell means "unknown or not applicable", never zero |
| Booleans | `true` or `false` |
| Numbers | Invariant culture (a full stop for decimals), no thousands separators |
| Dates and times | ISO 8601, UTC (`2026-10-03T14:05:00Z`) |
| Offsets | `start` and `length` count **UTF-16 code units** (the .NET string index) into the text in `document-text.jsonl`. JavaScript uses the same unit. Tools in other languages must convert |
| Identifiers | Stable strings. A document's id is its file path relative to the input folder (`in/`), so it is unique even when several files (text, Word, PDF, scans) share one answer key. Other ids are built as shown below |
| Extra columns | Readers must ignore columns they do not know, so columns can be added without breaking them |
| Version | `run.json` carries `formatVersion`. A change that removes or renames a column raises the major number |

## 2. `run.json`

One object describing the run. Fields marked * are required.

| Field | Meaning |
|---|---|
| `formatVersion`* | This format's version, for example `0.1` |
| `datasetId`* | Short id used for the folder name |
| `status`* | `final` or `interim`. An interim dataset is made from earlier runs and is shown with a banner in the explorer |
| `createdAt`*, `elapsedSeconds` | When it was made, and the evaluator's run time |
| `gitCommit`, `gitDirty` | The code that produced it. A final dataset has `gitDirty: false` |
| `ollamaVersion`, `machine` | Environment (operating system, cores, memory) |
| `machineBusy` | Whether the evaluator saw other heavy processes running |
| `repeats` | How many times each configuration was run (1 if not repeated) |
| `models`* | List of `{name, digest, parameterSize, quantization, sizeBytes}` for each Ollama model |
| `gliner` | `{model, threshold, labels, soloAction}` when GLiNER was used |
| `sources`* | One entry per saved or fresh run that went into the dataset: `{file, corpus, started, elapsedSeconds, ollamaVersion, machine, settingsHash, settings}`. `settings` holds the effect-bearing settings (temperature, seed, context size, chunk size and overlap, `think`, timeout, rules on or off, categories and their modes, organisation suffixes, GLiNER threshold). `results.csv` carries the `settingsHash` of its source |
| `corpora`* | List of `{corpus, documentCount, keyFiles, keyVersion, keyChecksum, keyNote}`. `keyVersion` says which answer key scored the run (`audited` or `hand-written`), and the checksum covers the key files |
| `warnings` | How many warnings the converter or evaluator raised while building the dataset |
| `notes` | Free text |

## 3. `documents.csv`

| Column | Meaning |
|---|---|
| `doc_id`* | The file's path relative to the input folder (`in/`), for example `pdf/01-employee-record.pdf` or `finance-sample/heldout-001-annual-report.txt` |
| `key_id`* | The answer key that applies. Several documents can share one (the same letter as text, Word, PDF and scans) |
| `corpus`* | `formats` or `heldout` |
| `format_group`* | Text, Word, PDF with text, or a kind of scan |
| `doc_type` | The kind of document (from the answer key's title) |
| `chars` | Length of the extracted text |
| `text_hash`* | SHA-256 (hex) of the UTF-8 bytes of the text in `document-text.jsonl` |
| `entity_count`, `occurrence_count` | Size of the answer key for this document |

## 4. `document-text.jsonl`

One JSON object per line:

```json
{"doc_id": "heldout-197-mortgage-contract", "text_hash": "…", "text": "…"}
```

`text` is exactly the text the detectors received, after extraction (and OCR for scans). All offsets in `spans.csv` and `outcomes.csv` refer to it. The explorer checks `text_hash` against `documents.csv` before showing a coloured view and warns on a mismatch.

## 5. `entities.csv` (the answer key)

One row per key item, plus rows for what must be kept and what the key cannot decide.

| Column | Meaning |
|---|---|
| `entity_id`* | `<key_id>#<n>` for items to remove, `<key_id>#keep<n>` for must-keep items, `<key_id>#ignore<n>` for undecidable ones |
| `key_id`* | The answer key. A key with no items has no rows, and its documents have `entity_count` 0 |
| `role`* | `entity` (must be removed), `must_preserve` (must survive) or `ignore` (cannot be decided, counted neither right nor wrong) |
| `type` | Category, such as PERSON. Empty for `must_preserve` and `ignore` |
| `text`* | The exact text |
| `occurrences` | How many times it appears in the document |
| `where` | Where the key places it (for example `body`) |
| `origin` | `original`, `added` (by the audit) or `hand` (written by hand) |
| `judged` | `false` when the key does not judge this category (a redaction of it counts as unjudged) |

## 6. `results.csv`

One row per document × configuration × repeat. A **configuration** is a detector setup: `rules`, `gliner`, a model name (alone), or a model with a variant.

| Column | Meaning |
|---|---|
| `result_id`* | `<doc_id>\|<config>\|<repeat>` |
| `doc_id`*, `config`*, `repeat`* | |
| `model` | The Ollama model, if any |
| `variant`* | `plain`, `with-gliner`, `with-gliner-all-flags-accepted` or `with-gliner-correct-flags-accepted` |
| `status`* | `ok`, `timeout`, `error` or `skipped`. A timed-out document is a row, not a gap |
| `error` | Message when the status is not `ok` |
| `source` | `batch` or `live` |
| `detect_seconds`, `gliner_seconds`, `write_seconds` | Timings |
| `prompt_tokens`, `output_tokens` | |
| `discarded` | Model answers dropped by the verbatim guard |
| `present`, `caught` | Occurrences on the key, and how many were removed |
| `entities_present`, `entities_fully_caught` | Same, counted as items |
| `lost_to_extraction` | Key items that never appeared in the text (for example OCR misreads) |
| `edits`, `true_positives`, `type_correct` | Redactions made, those overlapping the key, and those with the right category |
| `unjudged` | Redactions that could not be judged |
| `flags_raised`, `flags_correct` | Items only GLiNER found (flagged, or redacted in the "accepted" variants), and how many of them were really sensitive. Empty for setups without GLiNER |
| `preserve_total`, `preserve_broken` | Must-keep items and how many were wrongly removed |
| `output_ok`, `output_error` | Whether the written file passed its safety checks (empty if not tried) |
| `settings_hash` | Allows a live run to be compared with the batch settings |

Recall, precision and F1 are not stored. They are calculated from these counts, so the formulas live in one place.

## 7. `spans.csv`

What each detector produced, as positions.

| Column | Meaning |
|---|---|
| `span_id`* | `<result_id>#<n>` |
| `result_id`* | |
| `type`*, `start`*, `length`* | Category and position in the text |
| `confidence` | 0 to 1, if the detector gave one |
| `source`* | Where the span came from, as recorded by the detector: for example `rule`, `llm`, `llm-variant`, `gliner`, `gliner-only` (only GLiNER found it), or a label with `+gliner` added where both agreed |
| `flag`* | `true` if left in the text for review, `false` if redacted |

## 8. `outcomes.csv`

One row for each judged fact, so category and per-item questions need no re-scoring.

| Column | Meaning |
|---|---|
| `outcome_id`* | `<result_id>#o<n>` |
| `result_id`*, `doc_id`* | |
| `kind`* | `caught`, `missed`, `lost_to_extraction`, `over_redaction`, `unjudged`, `preserve_broken`, `flag_correct`, `flag_wrong` (items GLiNER found alone that were left in the text for review) or `key_extra` (a further occurrence of a key text beyond the count the key gives: not scored for recall, but a redaction of it still matches the key) |
| `entity_id` | The key item involved, if any |
| `span_id` | The span involved, if any |
| `type` | Category |
| `text` | The text concerned |
| `start`, `length` | Position, where known |

A `caught` or `missed` row is written for every occurrence of every key item, so recall by category, document or model can be totalled directly. The totals in `results.csv` decide how many occurrences count as caught; the occurrences most covered by redactions are the ones shown as caught. A redaction that matched the key has a span but no outcome row.

## 9. Derived by the explorer, not stored

- Recall, precision, F1, the 95% ranges.
- Combination strategies (union, agreement, voting) from the spans of two or more configurations.
- Difficulty ratings for documents, and the leaderboards.
- Live runs add rows to `results.csv`-style files (`source = live`) in a separate `live-results` set, so a batch dataset is never edited.

## 10. Checks (a validator)

A validator, used in a test for both the interim and the final dataset, fails if:
- a required column or file is missing, or a value has the wrong type;
- an id points to nothing (a span with no result, an outcome with no entity or document);
- a span lies outside the text length, or a `text_hash` does not match;
- a final dataset has `gitDirty: true`, or a `results.csv` row is missing for a document in a corpus (unless its status says why).

## 11. The interim dataset

`dotnet run --project src/AiDocumentRedactor.Eval -- --export-dataset <run.scores.json[.gz]>[,<more>] --id interim-20261003` makes a dataset from saved runs without running any model, and `--validate-dataset <dir>` checks any dataset. The first interim dataset was made from the held-out run (300 documents) and the formats run (38 documents), both saved as `documentation/evaluation-reports/*.scores.json.gz`.

| Interim field | Where it comes from | Gap |
|---|---|---|
| `results.csv` | The saved per-document rows, rescored against the answer keys as they are now | `repeat` is 1; `machineBusy` unknown; timings are as saved |
| `spans.csv` | The saved spans; the combinations with GLiNER are rebuilt from the model's and GLiNER's spans with the same agreement rules | Earlier runs have no spans, so they are not converted |
| `entities.csv` | The answer keys in `tests/`, the audited key for the held-out corpus | `origin` is `added` only for items the audit added |
| `outcomes.csv` | Built by the scorer in the same pass as the totals | |
| `document-text.jsonl` | The documents read again from the corpus files (with OCR for scans) | See the check below |
| `run.json` | Settings, models, Ollama version and machine from the saved runs | `gitCommit` is empty and `status` is `interim` |
| Failed rows | The run's "skipped" list: a timeout or an error becomes a row with that status for the model and its GLiNER variants | |

**The check on the text.** Every rescored row is compared with the saved row: the number of redactions and the must-keep count must match. For the first interim dataset all 6,063 comparable rows matched, including every scanned and Word document, which shows the text read again is the text the models saw. (The "correct flags accepted" variant depends on the answer key, which changed after the run, so it is not compared.) The totals also match the audited report exactly.

**The input folder.** The text is read from the corpus files the run used, and `in/` is compared with them. Nine files in `in/` differ from the corpus copies: six Word files and three degraded scans (regenerated at some point). Their text in `in/` may therefore not match the stored text, which matters for live runs.

## 11b. Live results

Results made from the explorer's document page ("Try with another model") are written to a `live/` folder inside the dataset, as `results.csv`, `spans.csv` and `outcomes.csv` in the same format as the dataset's own. The batch files are never changed. Live rows have `source` = `live`, a `config` such as `llama3.1:8b (live 1)` (the number counts live runs of that model on that document) and `repeat` 1. The explorer's database import adds them to the same tables, skipping any row that names an unknown document or repeats an id, and the leaderboard, ratings and combinations leave them out unless "Include live runs" is ticked. A live run that times out or fails is recorded as a row with that status. A live run refuses to start if the input copy of the document reads differently from the stored text, and warns if the answer key or the settings differ from those the batch results were made with.

## 12. Sensitivity

`document-text.jsonl` holds the full text of every document. For the synthetic corpus that is harmless. If real documents are ever evaluated, the dataset is as sensitive as the documents and must be stored and shared accordingly. The explorer reads it only behind the access token.

## 13. Open questions

1. A single `config` string, or separate columns for detector, model and variant? (Now: both, with `config` as the setup's name.)
2. Should combination strategies ever be stored? (Now: no, derived by the explorer.)
3. Is one JSON Lines file for all text acceptable at the final size, or should it be split per corpus? (The interim file is 0.5 MB for 338 documents.)
4. Size: the interim `spans.csv` and `outcomes.csv` are about 22 MB and 26 MB. Fine for SQLite and local use, large for git, so generated datasets are not committed; the final one may need compressing.
5. The nine documents in `in/` that differ from the corpus copies: should `in/` be refreshed from the corpus, or the corpus from `in/`?
