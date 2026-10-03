# Story-002: Ultimate Evaluator

**Status:** Built and run once (3 October 2026): dataset `final-20261003`, 338 documents, 7,774 results, 4 h 18 min, machine idle, code commit `6062ee0` (finished on `19bfc11` with no scoring code changed). One pass, no repeats, by decision. Six model-and-document failures are recorded in the dataset.
**Related:** [Story-001: Results Explorer](Story-001-Results-Explorer.md) (reads the data this story produces)

## Summary

As the person responsible for the evidence behind the tool, I want one command that runs every model, every corpus and every variant under recorded, repeatable conditions, so that a single complete, versioned dataset exists for the explorer and for the findings reports.

## Why

The evidence so far came from several separate runs with different settings, a busy machine (unreliable timings), a model timeout, and a corrected answer key applied afterwards. A final run needs to remove those weaknesses.

## One final version

The result is a **single dataset**, not a series of versions. Every model, corpus and variant is run against **the same code and the same answer key**, so each number can be compared with every other. Earlier runs (v1 to v6) were stages while the techniques were being refined, and they are not part of it. The reasoning behind the technique is told in the methodology page and document (Story-001), not by showing old runs.

## Ideas for acceptance criteria

- [ ] The run records the git commit of the code and refuses to start if there are uncommitted changes, so the dataset can be tied to exact code.
- [ ] The run records the answer-key version (and a checksum of the key files).
- [ ] Only local Ollama models are used. The evaluator refuses to run a model that is not served by the local Ollama, so no document text leaves the machine and no token costs arise.
- [ ] One command runs all configured models over the tuned, held-out and formats corpora, with rules-only and GLiNER variants and the combination strategies.
- [ ] The run records the Ollama version, hardware, settings and answer-key version, and warns if the machine is busy.
- [ ] Repeats (for example three) are supported so run-to-run variation is measured.
- [ ] A larger model timeout is set, and a timed-out document is recorded as a failure, not dropped.
- [ ] The run is resumable if interrupted.
- [ ] Output is the versioned run file, the CSV, the SQLite database for the explorer, and the markdown report, stamped with a version number.
- [ ] Never modifies or deletes the original documents.

## Notes

- Follows the standing constraints: synthetic documents only, local-only processing.
- Depends on the data format agreed in Story-001.
