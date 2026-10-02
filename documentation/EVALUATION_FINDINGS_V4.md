# Version 4 findings: agreement with GLiNER

**Version:** 4 (follows [version 3](EVALUATION_FINDINGS_V3.md))
**Report:** [v4-20261002-1804-gliner-agreement-2-models](evaluation-reports/v4-20261002-1804-gliner-agreement-2-models.md)
**Run:** phi4 and qwen3.6:27b, 38 documents, 358 occurrences, each scored alone, with GLiNER, and with GLiNER's flags accepted. Files were not written, so no output-safety check was run.

## What was built

GLiNER (`gliner-pii-edge`, quantised, short everyday labels, threshold 0.3) now runs in-process as a second opinion next to the language model and the rules. Where both find something, the main span is marked agreed. Something **only GLiNER found is flagged for review and left in the text**, and a reviewer decides. Adding GLiNER cannot remove a redaction (see requirement FR73 and the [phase 2 plan](PHASE_2_PLAN.md)).

## Results

| Row | Recall | Precision | Missed | Over-redactions | Must-keep damaged | Flagged for review (really sensitive) |
|---|---:|---:|---:|---:|---:|---:|
| phi4 | 98.3% | 97.7% | 6 | 8 | 2 of 31 | – |
| phi4 + GLiNER (flags left in text) | 98.3% | 97.7% | 6 | 8 | 2 of 31 | 46 (7) |
| phi4 + GLiNER, **if every flag is accepted** | 99.2% | 87.9% | 3 | 47 | 5 of 31 | 46 (7) |
| qwen3.6:27b | 99.4% | 89.5% | 2 | 41 | 3 of 31 | – |
| qwen3.6:27b + GLiNER (flags left in text) | 99.4% | 89.5% | 2 | 41 | 3 of 31 | 32 (3) |
| qwen3.6:27b + GLiNER, **if every flag is accepted** | **100.0%** | 83.5% | **0** | 70 | 5 of 31 | 32 (3) |

## What this shows

1. **GLiNER's flags are a safety net, not a redactor.** With its flags left in the text, nothing changes (by design). The flags hold the missed items: for qwen3.6:27b, the two "Kestrel" occurrences were found and flagged, so a reviewer who accepts those reaches **100% recall (0 missed)**. For phi4, three of its six misses were among the flags.
2. **The review load is small but noisy.** About one flag per document (46 across 38 for phi4, 32 for qwen), and **only 7 of 46 (15%) and 3 of 32 (9%) were really sensitive**. Accepting everything would cost 39 or 29 extra over-redactions, so the flags need a person, not a rule. A reviewer who accepts only the right ones adds the missed items without the over-redactions.
3. **No cost in time.** GLiNER added about 0.1 s per document; the models' timings are unchanged.
4. **The cheapest route to a high recall figure is qwen3.6:27b with review of GLiNER's flags**, while phi4 stays the better automatic redactor (98.3% recall at 97.7% precision, and a small review load).

## Caveats

- The threshold (0.3) and label wording were chosen after looking at this same corpus, so the flag counts and the 100% are optimistic; a held-out set is still needed (version 3, section 5).
- "100%" assumes the reviewer accepts exactly the right flags. A person can also accept a wrong one or miss a right one, which this run does not measure.
- Only two models were run, and files were not written.
- A flagged item counts as "really sensitive" if it overlaps something on the answer key; the key does not list judgement-based items such as job titles, so some flags the key calls wrong might be accepted by a client's own policy.

## Next

1. A **held-out set** and a larger corpus, then re-measure the flag precision with a threshold chosen on the tuning set only.
2. **Show flags in the review screen with the reason** ("found by GLiNER only, confidence 0.62"): the source and confidence are already carried on each span; the screen does not show them yet.
3. **Per-category thresholds** for GLiNER (its noise differs a lot by category), and a second look at the large model.
4. The **second-pass check** of the redacted text (phase 2, last item).
