# Dataset archives

The evaluation datasets that the results explorer shows, kept in git as compressed archives (the `datasets/` folder itself is ignored because its SQLite cache is large and can be rebuilt).

| Archive | What it is |
|---|---|
| `final-20261004.tar.gz` | The second evaluation: 5 models over 750 documents (the first 338, plus 232 Nemotron and 180 Gretel), 17,250 results. **The one to show.** Includes `cleanup-effect.md` and `report.md`. |
| `final-20261003.tar.gz` | The first evaluation: 5 models over 338 documents, 7,774 results. Kept for comparison. |

Each archive holds the CSV files, `document-text.jsonl` and `run.json` of one dataset (format: [RESULTS_DATASET_FORMAT.md](../documentation/RESULTS_DATASET_FORMAT.md)). It does not hold the SQLite database the explorer reads from; that is built automatically the first time a dataset is opened.

To restore them, from the repository root:

```bash
./tools/restore-datasets.sh
```

Then start the web app (`./run-web.sh`) and open the **📊 Results** page. The documents are in `in/` and the answer keys in `tests/`, both in git.

Answer-key notes (also shown in the explorer's header): held-out is audited, Nemotron is checked (30 documents by hand and a scan; not audited), Gretel is generator-labelled and not audited, formats is hand-written.
