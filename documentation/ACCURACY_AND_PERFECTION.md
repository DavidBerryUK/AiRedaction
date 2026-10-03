# Can redaction be 100% accurate?

**Status:** Position paper, based on the [final findings](FINAL_FINDINGS.md) (dataset `final-20261003`, 3 October 2026).
**The question:** how likely is it that we can get a fully, 100% accurate redaction process working?

---

## 1. The short answer

A process that is **100% accurate on every real document is very unlikely**, and we should not promise it. A process that is **measured, very accurate, and honest about what is left** is realistic, and is a stronger thing to put in front of a client, because it can be demonstrated and defended.

---

## 2. What our own data shows

- **100% on a test set is easy, and misleading.** Taking the union of three models removed every sensitive item on the held-out key (0 missed of 1,679), but about a quarter of what it removed was wrong (precision 76%). Recall can always be bought by redacting more.
- **Accurate with usable precision has a ceiling.** The best measured result is two models agreeing, with a person deciding the disagreements: **99.3 to 99.8% recall at 93 to 96% precision**. That is the "ideal reviewer" best case; a real reviewer will make some mistakes, and that has not been measured.
- **Zero misses does not prove 100%.** With 0 misses in 1,679 items, the most we can say with 95% confidence is that the true miss rate is below about **0.18%** (the "rule of three": 3 divided by the sample size). To show a rate below 0.01% would need about **30,000** error-free items, and they would need to be realistic documents.

---

## 3. Why 100% is not reachable

1. **"Sensitive" is partly a judgement.** Job titles, ages, project codenames and "Head of People" are decided differently by different clients. We also found that the answer key itself was wrong in places, and two careful people would disagree on some items. Perfection against a fixed key is not meaningful for those items.
2. **Some leaks are indirect.** "The 2019 data breach at the Bristol depot" identifies someone without naming them. No list of rules will catch every fact of that kind.
3. **Real documents are messier than ours.** Bad scans, handwriting, tables and unusual formats are not in the test set. Our scores are for clean, synthetic documents.
4. **Manual redaction is not perfect either.** A claim of "more accurate than a person doing it by hand" is defensible and measurable. "Perfect" is not.

---

## 4. What can be made close to perfect

- **Structured identifiers** (email addresses, phone numbers, IBANs, NHS and National Insurance numbers, card numbers) are essentially 100% with fixed rules, and with check digits that can be proved.
- **The output file.** A redaction that is irreversible and verified: no recoverable text in a PDF, nothing hidden in a Word file, a scan read again to confirm. All 1,684 redacted files in the final evaluation passed these checks.
- **The process.** Every redaction records its source, anything uncertain goes to a person, and nothing leaves the machine.

---

## 5. What we should claim instead

> Layered detection finds about 98% automatically and about 99.5% with human review of the disagreements, on documents it was not tuned on. Every miss rate is measured with a confidence range, every result can be inspected, and the remaining risk is stated.

This is a stronger position than "100%": it says what is true, shows the evidence, and names the limits.

---

## 6. How to push the measured figure higher

1. **Add the deterministic clean-up rules** (masked values, placeholders, generic defined terms, dates of birth only near birth context): a large precision gain, mainly for phi4 and qwen3.6:27b.
2. **Client term lists and policy options**, so policy items are decisions a client makes and not model errors.
3. **Measure real reviewers**, so the review figure stops being a best case.
4. **Grow the held-out set** toward real, messy documents (Word, PDF, scans, UK letters and forms), since that is where the real figure will come from.
5. **Check the answer key** by hand on a random sample, so the figures can be quoted outside the project.

---

## 7. Related

- [FINAL_FINDINGS.md](FINAL_FINDINGS.md): every figure above, and what can and cannot be claimed.
- [METHODOLOGY.md](METHODOLOGY.md): how the process works and why each choice was made.
