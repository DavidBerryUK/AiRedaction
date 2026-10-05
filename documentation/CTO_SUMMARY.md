# Redaction Demo: evidence summary for the CTO

**What it is.** A prototype that finds and removes personal and company-identifying information from documents, using only AI models that run on the machine itself (no document text leaves it, no per-use cost). It was built to show whether accurate, provable redaction is achievable, with a view to the production version.

**The short answer.** On documents it was never shaped around, the best single model removes about **98 in every 100** sensitive items and is right about **92 times in 100** when it removes something. With two models checking each other and a person deciding the disagreements, the measured best case is about **99.3% recall at 95.8% precision**. No model reached 100%, and the remaining misses are describable. All data so far is synthetic text; real documents are the next step.

## The evidence

| | Documents | What it is | gemma4:31b recall / precision |
|---|---:|---|---|
| Held-out finance set | 300 | Unseen by the rules; key audited | 98.0% / 91.7% |
| **Nemotron set** | **232** | **Different generator and labeller; key checked** | **98.4% / 91.6%** |
| Gretel set | 180 | New documents; key not audited (precision understated) | 95.6% / 70.4% |

- **Replicated.** The headline held on a second, independent set of documents, with the five models in the same order. Recall is the share of sensitive items removed; precision is the share of removals that were right.
- **Five local models compared** over 750 documents in one run on one version of the code, with timings. The larger models (gemma4:31b, qwen3.6:27b) find the most (about 98%); gemma4:31b is far more precise. The smallest, gemma4:e4b, is about 4 times faster but finds only about 88%.
- **Where it over-redacts is fixable.** Simple rules (placeholders, masked values, generic terms like "Borrower", dates without birth wording) raised precision by 5 to 10 points for three models on documents they were never based on, at a cost of at most 0.4 points of recall. Across five models: 743 wrong redactions removed, 15 right ones lost. They are off by default.
- **Every redacted file written passed its safety check** (3,743 of 3,743: no recoverable text, nothing hidden in the file).
- **Every figure can be inspected.** A results explorer in the web app shows each document with what each model found, missed and wrongly removed, and lets you try another local model on any document.

## Honest limits

- **Synthetic documents only**, mostly text. There are no real, messy documents, and few Word, PDF or scan examples. The figures may not carry over to real ones.
- **The answer keys are not human-audited.** The held-out key was corrected by written rules, the Nemotron key was checked on 30 documents and by a scan, and the Gretel key was not audited. A person should check a sample before figures are quoted outside the project.
- **The reviewed figure (99.3%) assumes the person is right every time.** That has not been measured.
- **No setup is perfect.** The most-missed items are indirect identifiers, ID numbers with no recognisable shape, and policy questions (is a project codename sensitive?) that a client's term list would settle.
- **One of the models (gpt-oss) gives different answers on repeat runs** (48 of 338 documents changed), so it needs care as the second model for agreement.
- **7 of 3,750 model-and-document runs failed** (timeouts, server errors, cut-off replies). They are recorded, not hidden.

## Recommended next steps

1. Run it on a sample of **real or realistic documents** (Word, PDF, scans), with a hand-checked key. This is the largest gap.
2. **Measure the reviewer:** have two or three people review the disagreements and record how often they get them right.
3. **Audit the answer keys** by hand on a random sample, so the figures can be quoted externally.
4. **Make client policy configurable** (term lists for codenames, job titles, public bodies) so judgement calls are the client's decisions, not model errors.
5. Investigate a **calibrated validation step** that scores each candidate redaction, to cut over-redaction and rank what the reviewer sees (listed in `INVESTIGATIONS.md`).

**Where to look.** Full account: `documentation/FINAL_FINDINGS.md` (section 13 is the latest run). Reasoning behind each design choice: `documentation/METHODOLOGY.md`. Data: `datasets-archive/` (restore with `./tools/restore-datasets.sh`), shown in the web app's **📊 Results** page.
