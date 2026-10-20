# Possible investigations

Ideas that are worth a look but are not part of the current work. Nothing here is committed to. Each entry says what it is, why it might help, what a test would be and what it would need.
The recommendations that came out of the final evaluation are in [FINAL_FINDINGS.md](FINAL_FINDINGS.md) section 11.

Standing constraints apply to every item: local processing only (no document text leaves the machine), synthetic data only, original documents never changed.

---

## 1. A decision model (Jev-style) as a gate on candidate redactions

**Added:** 5 October 2026. **Status:** not started. Needs a model download, which needs agreement first.

**What it is.** A "System One" decision model answers typed questions about some text (yes or no with a probability, a choice among options, or a score) and does not generate text.
TypeSafe AI's **Jev** (introduced September 2026) is the original: [announcement](https://typesafe.ai/blog/introducing-system-one-models-and-jev). It is a **hosted API only**, so it cannot be used here.
AutoTrust's **JEV-9B** and **JEV-27B** ([Hugging Face](https://huggingface.co/autotrust/JEV-9B)) are independent open-weight copies, distilled from Jev onto Qwen models. They are not TypeSafe's product.

**Why it does not fit as a detector.** It cannot produce text, and redaction needs the exact string found in the document. It is not a candidate for the `--models` list.

**Where it might fit.** As a gate on candidates that other detectors have already found: "is this a real personal item, and of what type?", with a calibrated probability. That would address the weakest point in the findings (over-redaction: precision 74% to 92% depending on the model) and give the reviewer a ranked queue. For example:

| Probability | Action |
|---|---|
| 0.90 and above | redact automatically |
| 0.60 to 0.90 | send to the reviewer |
| below 0.60 | leave in the text |

It could replace or support the clean-up rules (FR74), and it would help with the open question of how a reviewer handles disagreements between models.

**A cheap test.** The saved dataset already records every redaction each model made and whether it was right or wrong (`spans.csv`, `outcomes.csv`). Ask the model about a sample of those candidates and measure whether its probability separates the right ones from the wrong ones (precision and recall at each threshold, compared with the clean-up rules). Thousands of short questions; no new detection run. Do it on a quiet machine after an evaluation run has finished.

**Needs before starting.**
- Check the licence of the JEV model weights themselves (the training corpus is Apache-2.0; the model and its Qwen base may differ).
- Check the download size (probably 15 to 20 GB for the 9B model) and how to run it locally (it may need Python and PyTorch, not Ollama). GLiNER-style .NET integration would be a further step, only if the test is promising.
- Ask for agreement before downloading.

**Cautions.** The speed and cost claims are the vendor's own and are not verified here. The models are new and the open ones are third-party copies. The test above is what would show whether it helps on our documents.
