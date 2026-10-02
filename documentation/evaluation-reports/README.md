# Versioned evaluation reports

Each report here is a fixed, named copy of an evaluation run, kept so results can be compared over time. The working folder `eval/` holds every run as it happens; **only the runs below are the official versions**. Each has its Markdown report and a full `.scores.json` (which `--merge` can rebuild a report from).

| Version | Run | Models | What it measures | Findings |
|---|---|---|---|---|
| **v1** | 2026-10-02 14:53 | 10 | **Baseline** before any accuracy changes. 38 documents, 337 occurrences on the answer key | [Version 1 findings](../EVALUATION_FINDINGS_V1.md) |
| **v2** | 2026-10-02 16:37 | phi4 only | After phase 1 (rules layer, spacing-tolerant matching, empty-reply guard) and a corrected answer key (358 occurrences) | [Version 2 findings](../EVALUATION_FINDINGS_V2.md) |
| **v3** | 2026-10-02 17:15 | 5 | Phase 1 plus the **organisation-name rule**, the five kept models, full corpus | [Version 3 findings](../EVALUATION_FINDINGS_V3.md) |
| **v4** | 2026-10-02 18:04 | phi4, qwen3.6:27b, each alone and with GLiNER | **Agreement scoring** with GLiNER (flags left in the text, and flags accepted). Not written to files, so no output-safety row | [Version 4 findings](../EVALUATION_FINDINGS_V4.md) |

**Comparing versions.** v1 used the older answer key (337 occurrences); v2 and v3 use the corrected key (358), so v1 figures are close to, but not exactly, like-for-like. Every report records the Ollama version, the model digests and the settings used.

**Adding a version.** Run `./run-eval.sh`, copy the report and its `.scores.json` here with the next version number, date and a short label, add a row above, and write the findings.
