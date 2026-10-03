# Versioned evaluation reports

Each report here is a fixed, named copy of an evaluation run, kept so results can be compared over time. The working folder `eval/` holds every run as it happens; **only the runs below are the official versions**. Each has its Markdown report and a full `.scores.json` (which `--merge` can rebuild a report from).

| Version | Run | Models | What it measures | Findings |
|---|---|---|---|---|
| **v1** | 2026-10-02 14:53 | 10 | **Baseline** before any accuracy changes. 38 documents, 337 occurrences on the answer key | [Version 1 findings](../EVALUATION_FINDINGS_V1.md) |
| **v2** | 2026-10-02 16:37 | phi4 only | After phase 1 (rules layer, spacing-tolerant matching, empty-reply guard) and a corrected answer key (358 occurrences) | [Version 2 findings](../EVALUATION_FINDINGS_V2.md) |
| **v3** | 2026-10-02 17:15 | 5 | Phase 1 plus the **organisation-name rule**, the five kept models, full corpus | [Version 3 findings](../EVALUATION_FINDINGS_V3.md) |
| **v4** | 2026-10-02 18:04 | phi4, qwen3.6:27b, each alone and with GLiNER | **Agreement scoring** with GLiNER (flags left in the text, and flags accepted). Not written to files, so no output-safety row | [Version 4 findings](../EVALUATION_FINDINGS_V4.md) |
| **v5** | 2026-10-02 18:53 | 5 models and no-model baselines, each with and without GLiNER | **Held-out run**: 300 synthetic finance documents the system was not shaped around, scored against the generator's own answer key (kept for provenance; the key turned out to be incomplete) | [Version 5 findings](../EVALUATION_FINDINGS_V5.md) |
| **v5b** | rescored 2026-10-03 | same | The v5 run **rescored against the audited answer key** (no model re-run), with an audit log and three combination reports. This is the held-out result to use | [Version 5 findings](../EVALUATION_FINDINGS_V5.md) |
| **v6** | 2026-10-03 13:26 | 5 models, with GLiNER | The 38-document format corpus (Word, PDF, scans, text), written and verified. **Timings in this run are unreliable** (the machine was busy) | [Version 5 findings, section 8](../EVALUATION_FINDINGS_V5.md) |

The large `.scores.json` files of v5 and v6 are gzip-compressed (`.scores.json.gz`); the evaluation reads them directly (`--combine`, `--rescore`).

**Comparing versions.** v1 used the older answer key (337 occurrences); v2 and v3 use the corrected key (358), so v1 figures are close to, but not exactly, like-for-like. Every report records the Ollama version, the model digests and the settings used.

**Adding a version.** Run `./run-eval.sh`, copy the report and its `.scores.json` here with the next version number, date and a short label, add a row above, and write the findings.
