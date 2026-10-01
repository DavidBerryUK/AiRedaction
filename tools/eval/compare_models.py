#!/usr/bin/env python3
"""Run the redactor with one or more Ollama models over ./in and score against tests/TestCorpus/ground-truth.
Interim harness (Phase 1); replaced by a C# evaluation harness in Phase 4. Scores text-format outputs only.
Usage: compare_models.py <workdir> model [model ...]
Recall = share of ground-truth body strings that no longer appear in the output.
Over-redaction = mustPreserve strings that vanished + placeholders beyond the expected count (a proxy for false positives)."""
import glob, json, os, re, subprocess, sys, time, collections

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
work = os.path.abspath(sys.argv[1]); models = sys.argv[2:]
os.makedirs(work, exist_ok=True)
cfg = open(os.path.join(ROOT, "redactor.config.json")).read().replace('"provider": "none"', '"provider": "ollama"')
cfgp = os.path.join(work, "eval.config.json"); open(cfgp, "w").write(cfg)
dll = os.path.join(ROOT, "src/AiDocumentRedactor.Cli/bin/Debug/net10.0/AiDocumentRedactor.Cli.dll")

def run(model):
    out = os.path.join(work, "out-" + model.replace(":", "_").replace("/", "_"))
    subprocess.run(["rm", "-rf", out])
    t = time.time()
    p = subprocess.run(["dotnet", dll, "--config", cfgp, "--input", os.path.join(ROOT, "in"), "--output", out, "--model", model],
                       capture_output=True, text=True)
    secs = time.time() - t
    tok = re.search(r"Tokens: (\d+) in / (\d+) out; discarded non-verbatim: (\d+)", p.stdout)
    return out, secs, tok.groups() if tok else ("?", "?", "?"), p

def score(out):
    files = {}
    for f in glob.glob(out + "/*/*-redacted.*"):
        files.setdefault(os.path.basename(f).split("-redacted")[0], []).append(f)
    tot, miss = collections.Counter(), collections.Counter()
    over, placeholders, expected, misses = 0, 0, 0, []
    for gt in sorted(glob.glob(os.path.join(ROOT, "tests/TestCorpus/ground-truth/*.json"))):
        g = json.load(open(gt)); fs = files.get(g["id"])
        for f in fs or []:
            t = open(f).read()
            placeholders += len(re.findall(r"\[REDACTED:[A-Z_]+\]", t))
            for e in g["entities"]:
                if e["where"] != "body": continue
                tot[e["type"]] += 1; expected += e["occurrences"]
                if e["text"] in t: miss[e["type"]] += 1; misses.append((os.path.basename(f), e["type"], e["text"]))
            for m in g["mustPreserve"]:
                if m not in t: over += 1; misses.append((os.path.basename(f), "OVER-REDACTED", m))
    return tot, miss, over, max(0, placeholders - expected), misses, len(files)

results = {}
for m in models:
    print(f"== {m}", flush=True)
    out, secs, tok, p = run(m)
    if p.returncode == 2: print("  not runnable:", p.stderr.strip() or p.stdout.strip()[-200:]); continue
    tot, miss, over, extra, misses, n = score(out)
    T, M = sum(tot.values()), sum(miss.values())
    results[m] = dict(secs=secs, tok=tok, tot=tot, miss=miss, over=over, extra=extra, misses=misses, n=n, recall=(T - M) / T if T else 0)
    print(f"  recall {T-M}/{T} = {100*(T-M)/T:.0f}%  | {secs:.0f}s for {n} docs | tokens {tok[0]} in/{tok[1]} out | discarded {tok[2]} | over-redacted {over} | extra placeholders ~{extra}", flush=True)

types = sorted({t for r in results.values() for t in r["tot"]})
print("\n| Model | Recall | Secrets | Person | Company | Contextual | Time (s) | Tokens out | Over-redacted | Extra placeholders |")
print("|---|---|---|---|---|---|---|---|---|---|")
def cell(r, t): return f"{r['tot'][t]-r['miss'][t]}/{r['tot'][t]}" if r["tot"][t] else "-"
for m, r in results.items():
    print(f"| {m} | {100*r['recall']:.0f}% | {cell(r,'SECRET')} | {cell(r,'PERSON')} | {cell(r,'COMPANY')} | {cell(r,'CONTEXTUAL')} | {r['secs']:.0f} | {r['tok'][1]} | {r['over']} | {r['extra']} |")
json.dump({m: {k: (dict(v) if isinstance(v, collections.Counter) else v) for k, v in r.items()} for m, r in results.items()}, open(os.path.join(work, "results.json"), "w"), indent=1)
