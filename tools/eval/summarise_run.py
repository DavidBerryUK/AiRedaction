"""Summarises a results dataset as Markdown tables, from the CSV files alone (no model, no database).

  python3 tools/eval/summarise_run.py datasets/final-20261004
  python3 tools/eval/summarise_run.py datasets/final-20261004 --compare datasets/final-20261003

Per corpus and setup: recall and precision with 95% Wilson ranges, items missed, over-redactions and seconds per document.
Recall is the share of key occurrences removed (caught / present). Precision is true positives / (true positives + over-redactions);
redactions the key does not judge are left out of both. With --compare, the documents both datasets share are compared setup by setup,
which shows run-to-run variation (the models are run at temperature 0 with a fixed seed, so any difference is real).
"""
import argparse
import collections
import csv
import math
import sys
from pathlib import Path

PLAIN = "plain"
CORPUS_ORDER = ["heldout", "external-nemotron", "external-gretel", "formats"]


def wilson(k, n, z=1.96):
    if n == 0:
        return (float("nan"), float("nan"))
    p = k / n
    d = 1 + z * z / n
    centre = (p + z * z / (2 * n)) / d
    half = z * math.sqrt(p * (1 - p) / n + z * z / (4 * n * n)) / d
    return (centre - half, centre + half)


def pct(x):
    return "n/a" if x != x else f"{100 * x:.1f}%"


def load(folder):
    folder = Path(folder)
    docs = {r["doc_id"]: r for r in csv.DictReader(open(folder / "documents.csv", encoding="utf-8"))}
    results = list(csv.DictReader(open(folder / "results.csv", encoding="utf-8")))
    over = collections.Counter()
    for o in csv.DictReader(open(folder / "outcomes.csv", encoding="utf-8")):
        if o["kind"] == "over_redaction":
            over[o["result_id"]] += 1
    return docs, results, over


def i(r, k):
    return int(r[k] or 0)


def table(docs, results, over, variant, corpus):
    groups = collections.defaultdict(list)
    failures = collections.Counter()
    for r in results:
        if docs[r["doc_id"]]["corpus"] != corpus or r["variant"] != variant:
            continue
        if r["status"] != "ok":
            failures[r["config"]] += 1
            continue
        groups[r["config"]].append(r)
    rows = []
    for config, rs in groups.items():
        present = sum(i(r, "present") for r in rs)
        caught = sum(i(r, "caught") for r in rs)
        tp = sum(i(r, "true_positives") for r in rs)
        ov = sum(over[r["result_id"]] for r in rs)
        secs = sum(float(r["detect_seconds"] or 0) + float(r["write_seconds"] or 0) for r in rs) / len(rs)
        rl, rh = wilson(caught, present)
        pl, ph = wilson(tp, tp + ov)
        rec = caught / present if present else float("nan")
        prec = tp / (tp + ov) if tp + ov else float("nan")
        f1 = 2 * rec * prec / (rec + prec) if rec == rec and prec == prec and rec + prec else float("nan")
        rows.append((config, len(rs), rec, rl, rh, prec, pl, ph, f1, present - caught, present, ov, secs, failures[config]))
    rows.sort(key=lambda x: -(x[8] if x[8] == x[8] else -1))
    out = ["| Setup | Docs | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Seconds per doc | Failed |",
           "|---|---:|---:|---:|---:|---:|---:|---:|---:|"]
    for c, n, rec, rl, rh, prec, pl, ph, f1, miss, present, ov, secs, fail in rows:
        out.append(f"| {c} | {n} | {pct(rec)} ({100 * rl:.1f}–{100 * rh:.1f}) | {pct(prec)} ({100 * pl:.1f}–{100 * ph:.1f}) | {pct(f1)} | {miss} of {present} | {ov} | {secs:.1f} | {fail} |")
    return out


def compare(docs_a, res_a, over_a, docs_b, res_b, over_b, variant):
    """Same documents, same setup, two datasets: recall and precision side by side, and how many documents changed."""
    shared = set(docs_a) & set(docs_b)
    index = lambda results: {(r["doc_id"], r["config"]): r for r in results if r["variant"] == variant and r["status"] == "ok" and r["doc_id"] in shared}
    a, b = index(res_a), index(res_b)
    configs = sorted({c for _, c in a} & {c for _, c in b})
    out = [f"{len(shared)} documents are in both datasets.", "",
           "| Setup | Docs | Recall: old | Recall: new | Precision: old | Precision: new | Documents with a different outcome |",
           "|---|---:|---:|---:|---:|---:|---:|"]
    for c in configs:
        keys = [k for k in a if k[1] == c and k in b]
        def stats(index, over):
            present = sum(i(index[k], "present") for k in keys)
            caught = sum(i(index[k], "caught") for k in keys)
            tp = sum(i(index[k], "true_positives") for k in keys)
            ov = sum(over[index[k]["result_id"]] for k in keys)
            return caught / present if present else float("nan"), tp / (tp + ov) if tp + ov else float("nan")
        ra, pa = stats(a, over_a)
        rb, pb = stats(b, over_b)
        changed = sum(1 for k in keys if (i(a[k], "caught"), over_a[a[k]["result_id"]]) != (i(b[k], "caught"), over_b[b[k]["result_id"]]))
        out.append(f"| {c} | {len(keys)} | {pct(ra)} | {pct(rb)} | {pct(pa)} | {pct(pb)} | {changed} |")
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dataset")
    ap.add_argument("--compare")
    ap.add_argument("--variant", default=PLAIN, help="plain (default) or one of the with-gliner variants")
    args = ap.parse_args()
    docs, results, over = load(args.dataset)
    corpora = [c for c in CORPUS_ORDER if any(d["corpus"] == c for d in docs.values())]
    corpora += sorted({d["corpus"] for d in docs.values()} - set(corpora))
    print(f"# Summary of {Path(args.dataset).name} ({args.variant})\n")
    for corpus in corpora:
        n = sum(1 for d in docs.values() if d["corpus"] == corpus)
        print(f"## {corpus} ({n} documents)\n")
        print("\n".join(table(docs, results, over, args.variant, corpus)) + "\n")
    if args.compare:
        docs_b, res_b, over_b = load(args.compare)
        print(f"## Compared with {Path(args.compare).name}\n")
        print("\n".join(compare(docs_b, res_b, over_b, docs, results, over, args.variant)))


if __name__ == "__main__":
    sys.exit(main())
