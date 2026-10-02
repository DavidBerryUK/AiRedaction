#!/usr/bin/env python3
"""Spike: how well does a GLiNER PII model do on our corpus, before any application work?

Runs one GLiNER model over the plain-text documents in tests/TestCorpus/text, marks the spans it finds at several confidence
thresholds, and scores them against the answer key the same strict way the evaluation does (an answer-key occurrence counts as
caught only when ALL of its characters are covered; spacing may differ). It reports recall, precision (a predicted span is
correct if it overlaps anything on the key), recall by category, and the items missed, so they can be compared with the chat models.

This is a throw-away measurement, not the integration. Documents stay on this machine; the model is downloaded once from
Hugging Face and then runs offline.

Setup (once):   python3 -m venv .venv-gliner && source .venv-gliner/bin/activate && pip install gliner
Run:            python tools/gliner-spike/spike.py [--model knowledgator/gliner-pii-edge-v1.0] [--only text/] [--thresholds 0.3,0.5,0.7]
"""
import argparse, json, re, time
from collections import defaultdict
from pathlib import Path

# Our categories as plain-language labels for the model (the real integration will keep these in the config).
LABELS = {
    "PERSON": "person name", "PHONE": "phone number", "EMAIL": "email address", "ADDRESS": "address",
    "ID_NUMBER": "identification number", "ONLINE_ID": "ip address", "AGE": "age", "DATE_OF_BIRTH": "date of birth",
    "GENDER": "gender", "COMPANY": "organization", "COMPANY_ID": "company registration number", "DOMAIN": "website",
    "CONTEXTUAL": "job title", "SECRET": "password",
}

def pattern(text):
    body = r"\s+".join(re.escape(p) for p in text.split())
    lead = r"(?<![\w])" if text[:1].isalnum() else ""
    trail = r"(?![\w])" if text[-1:].isalnum() else ""
    return re.compile(lead + body + trail)

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--model", default="knowledgator/gliner-pii-edge-v1.0")
    ap.add_argument("--corpus", default="tests/TestCorpus")
    ap.add_argument("--only", default="text/")
    ap.add_argument("--thresholds", default="0.3,0.4,0.5,0.6,0.7,0.8")
    a = ap.parse_args()
    from gliner import GLiNER
    model = GLiNER.from_pretrained(a.model)
    root = Path(a.corpus)
    labels = list(dict.fromkeys(LABELS.values()))
    docs = []
    for f in sorted(root.rglob("*")):
        rel = f.relative_to(root).as_posix()
        if f.is_file() and rel.startswith(a.only) and f.suffix in (".txt", ".md", ".csv", ".json") and "ground-truth" not in rel:
            gt = root / "ground-truth" / (f.stem + ".json")
            if gt.exists():
                docs.append((rel, f.read_text(encoding="utf-8"), json.loads(gt.read_text(encoding="utf-8"))))
    print(f"{len(docs)} documents, model {a.model}")
    preds = {}
    t0 = time.time()
    for rel, text, _ in docs:
        spans = []
        step, size = 1200, 1500   # GLiNER reads limited-length windows, so slide over the document
        for s in range(0, max(1, len(text)), step):
            chunk = text[s:s + size]
            for e in model.predict_entities(chunk, labels, threshold=min(map(float, a.thresholds.split(",")))):
                spans.append((s + e["start"], s + e["end"], e["label"], e["score"]))
            if s + size >= len(text):
                break
        preds[rel] = spans
    secs = (time.time() - t0) / max(1, len(docs))
    print(f"{secs:.2f} s per document on this machine\n")
    print("threshold  recall   precision  F1      missed  over")
    for th in map(float, a.thresholds.split(",")):
        present = caught = edits = tp = 0
        bycat = defaultdict(lambda: [0, 0])
        missed = []
        for rel, text, gt in docs:
            cover = [(s, e) for s, e, _, sc in preds[rel] if sc >= th]
            key = []
            for ent in gt["entities"]:
                if ent.get("where", "body") != "body":
                    continue
                for m in pattern(ent["text"]).finditer(text):
                    key.append((m.start(), m.end(), ent["type"], ent["text"]))
            for s, e, typ, txt in key:
                present += 1
                bycat[typ][0] += 1
                ok = all(any(cs <= i < ce for cs, ce in cover) for i in range(s, e))
                caught += ok
                bycat[typ][1] += ok
                if not ok:
                    missed.append((rel, typ, txt))
            edits += len(cover)
            tp += sum(1 for cs, ce in cover if any(cs < e and s < ce for s, e, _, _ in key))
        rec = caught / present if present else 1
        pre = tp / edits if edits else 1
        f1 = 2 * rec * pre / (rec + pre) if rec + pre else 0
        print(f"{th:<10} {rec:6.1%}  {pre:8.1%}  {f1:6.1%}  {present - caught:6}  {edits - tp:4}")
        if abs(th - 0.5) < 1e-9:
            detail = (bycat, missed)
    if "detail" in dir():
        bycat, missed = detail
        print("\nRecall by category at threshold 0.5")
        for k, (p, c) in sorted(bycat.items()):
            print(f"  {k:14} {c}/{p}")
        print("\nMissed at threshold 0.5")
        for rel, typ, txt in missed:
            print(f"  {rel} {typ}: {txt}")

if __name__ == "__main__":
    main()
