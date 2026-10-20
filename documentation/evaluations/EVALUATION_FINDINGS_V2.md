# Version 2 findings: effect of the phase 1 accuracy changes

**Version:** 2 (follows [version 1](EVALUATION_FINDINGS_V1.md))
**Measured on:** phi4 only, 38 documents, report `eval/eval-20261002-1637.md`

## What changed

| Change | What it does |
|---|---|
| Rules layer (FR70) | Emails, phone numbers, postcodes, NHS, National Insurance, IBAN (check digit), sort code and account numbers, card numbers (Luhn), IP addresses, gender words and titles, and pronouns when switched on, are found by fixed rule alongside the model |
| Spacing-tolerant matching (FR71) | The model's answers are matched, placed and propagated treating any run of spaces or line breaks as equal |
| Empty-reply guard (FR72) | An empty answer for text that plainly has names or identifiers is asked again with a reminder, then goes to `llm.fallbackModel` if one is set |
| Answer key corrected | Gender words and pronouns in the test documents, and the partial account number "4821", are now marked; the scorer also tolerates OCR's "name @site" |

## Result for phi4

| | Version 1 | Version 2 |
|---|---:|---:|
| Recall | 95.3% | **97.5%** |
| Precision | 95.9% | **97.6%** |
| F1 | 95.6% | **97.6%** |
| Items missed | 16 | **9** |
| Over-redactions | 13 | **8** |
| Must-keep items damaged | 2 of 31 | 2 of 31 |
| Time per document | 9.3 s | 9.9 s |

**Read with care.** The answer key grew from 337 to 358 occurrences (the gender words and "4821" were added), so this is not a perfectly like-for-like comparison, and a repeat of the same run varied by about 0.3 points (97.2% to 97.5% in two runs of slightly different code). The other nine models have not been re-run yet.

## What the change taught us

1. **The rules did most of the work.** The gender-word and structured-item misses are gone, which was the part of the version 1 estimate that held up.
2. **The line-wrap explanation was only partly right.** Matching is now spacing-tolerant, but reading phi4's raw reply showed it never lists "Fernleigh Surgery" at all, so those misses (now 5 of phi4's 9) are the model leaving a company out, not a matching fault. Joining wrapped lines before sending the text was tried and removed: it made no difference beyond run-to-run noise.
3. **What remains is mostly the hard category.** Of the 9 misses, 5 are "Fernleigh Surgery", 2 are "Kestrel" (a project codename) and the rest are contextual identifiers and the mixed document. These point to the next steps from version 1: a second model, the second-pass check and human review of disagreements.
4. **Over-redactions are small and policy-shaped:** ages in passing, "in January" read as a date of birth, and job titles.

## Added after this run (not yet measured)

An **organisation-name rule**: capitalised words ending in a suffix such as Ltd, PLC, Surgery, School, Council or Credit Union (the list is `rules.organisationSuffixes` in the config) are marked as COMPANY by rule. It targets the names every model missed in the PDF and scan versions ("Fernleigh Surgery", "Brightwater Analytics Ltd"). Its effect on the scores, including any extra over-redaction, will show in the next evaluation run. A name directly above an organisation on the next line (a signature block) may be joined into one company redaction: still redacted, but labelled as a company.

## Next

Re-run all ten models to see whether the rules lift the other models as much, then build phase 2 (a second model with agreement scoring, and the second-pass check).
