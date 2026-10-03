# Results dataset format

**Status:** Draft v0.1 (design only, nothing built yet)
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
| Identifiers | Stable strings, lower case, no spaces. Documents use the document id; other ids are built as shown below |
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
| `settings`* | The effect-bearing settings: temperature, seed, context size, chunk size and overlap, `think` per model, timeout, rules on or off, categories and their modes, organisation suffixes |
| `settingsHash` | Hash of the full configuration used |
| `corpora`* | List of `{corpus, documentCount, keyVersion, keyChecksum, keyNote}`. `keyVersion` says which answer key scored the run (for example `audited`) |
| `notes` | Free text |

## 3. `documents.csv`

| Column | Meaning |
|---|---|
| `doc_id`* | Document id (the answer key's id) |
| `file`* | Path relative to the input folder (`in/`), for example `pdf/01-employee-record.pdf` |
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
| `entity_id`* | `<doc_id>#<n>` |
| `doc_id`* | |
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
| `flags_raised`, `flags_correct` | GLiNER-only flags and how many were really sensitive |
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
| `source`* | `rule`, `model`, `gliner`, or a combined label such as `model+gliner` |
| `flag`* | `true` if left in the text for review, `false` if redacted |

## 8. `outcomes.csv`

One row for each judged fact, so category and per-item questions need no re-scoring.

| Column | Meaning |
|---|---|
| `outcome_id`* | |
| `result_id`*, `doc_id`* | |
| `kind`* | `caught`, `missed`, `lost_to_extraction`, `over_redaction`, `unjudged`, `preserve_broken`, `flag_correct` or `flag_wrong` |
| `entity_id` | The key item involved, if any |
| `span_id` | The span involved, if any |
| `type` | Category |
| `text` | The text concerned |
| `start`, `length` | Position, where known |

A `caught` or `missed` row is written for every occurrence of every key item, so recall by category, document or model can be totalled directly.

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

## 11. Making the interim dataset from existing runs

| Interim field | Source in the existing saved runs | Gap |
|---|---|---|
| `results.csv` | The per-document scores | `repeat` is 1; `machineBusy` unknown |
| `spans.csv` | Saved spans (held-out and formats runs only) | Earlier runs have none, so they are not converted |
| `entities.csv` | The answer keys, using the audited key for the held-out corpus | `origin` available only for audited items |
| `outcomes.csv` | Rebuilt by scoring the saved spans against the key (the existing rescoring does this) | |
| `document-text.jsonl` | Re-extracted from `in/` with the current extractor | Must match what the detectors saw; the hash cannot prove this for old runs, so it is recorded as a known risk |
| `run.json` | Settings, models, Ollama version and machine from the run files | `gitCommit` is empty, `status` is `interim` |
| Timeout rows | Inferred from the missing document (phi4 on one CSV file) | |

## 12. Sensitivity

`document-text.jsonl` holds the full text of every document. For the synthetic corpus that is harmless. If real documents are ever evaluated, the dataset is as sensitive as the documents and must be stored and shared accordingly. The explorer reads it only behind the access token.

## 13. Open questions

1. A single `config` string, or separate columns for detector, model and variant? (Draft: both, with `config` as a convenience.)
2. Should combination strategies ever be stored? (Draft: no, derived.)
3. Is one JSON Lines file for all text acceptable at the final size, or should it be split per corpus?
4. Should the interim converter re-extract text from `in/`, or is the extractor too unreliable for a hash check to be trusted?
