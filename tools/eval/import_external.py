"""Builds two external test corpora from public synthetic-PII datasets on Hugging Face.

  gretel    gretelai/synthetic_pii_finance_multilingual (Apache 2.0), English test split, with the 300 documents already used
            in tests/HeldOutCorpus left out.
  nemotron  nvidia/Nemotron-PII (CC-BY-4.0), test split.

Both are fully synthetic. Documents are chosen by a seeded random draw (not by quality), so the sample is not shaped by us.
Each document comes with labelled spans, which are mapped to this project's answer-key format (text-based, so only the text of
a span matters, not its offset).

Needs pyarrow (pip install pyarrow) and the two parquet files already downloaded (see --data). Run from the repository root:
  python3 tools/eval/import_external.py --data datasets/external
"""
import argparse
import ast
import csv
import json
import random
import re
from pathlib import Path

SEED = 20261003
JUDGED = ["PERSON", "COMPANY", "ADDRESS", "EMAIL", "PHONE", "DATE_OF_BIRTH", "ID_NUMBER", "ONLINE_ID", "SECRET"]

# Labels that are not sensitive under this project's policy (generic dates and times, coordinates, bank identifier codes, cities and
# countries, ...) are left off the key, so a model that redacts them is counted as over-redacting. Labels with no category here
# (gender, age, job title, religion, ...) are not judged either way.
GRETEL_MAP = {
    "name": "PERSON", "first_name": "PERSON", "last_name": "PERSON", "company": "COMPANY", "street_address": "ADDRESS",
    "email": "EMAIL", "phone_number": "PHONE", "date_of_birth": "DATE_OF_BIRTH",
    "ssn": "ID_NUMBER", "passport_number": "ID_NUMBER", "driver_license_number": "ID_NUMBER", "iban": "ID_NUMBER", "bban": "ID_NUMBER",
    "bank_routing_number": "ID_NUMBER", "credit_card_number": "ID_NUMBER", "credit_card_security_code": "ID_NUMBER",
    "account_pin": "ID_NUMBER", "employee_id": "ID_NUMBER", "customer_id": "ID_NUMBER",
    "ipv4": "ONLINE_ID", "ipv6": "ONLINE_ID", "user_name": "ONLINE_ID", "password": "SECRET", "api_key": "SECRET",
}
NEMOTRON_MAP = {
    "first_name": "PERSON", "last_name": "PERSON", "company_name": "COMPANY", "street_address": "ADDRESS", "email": "EMAIL",
    "phone_number": "PHONE", "fax_number": "PHONE", "date_of_birth": "DATE_OF_BIRTH",
    "ssn": "ID_NUMBER", "account_number": "ID_NUMBER", "customer_id": "ID_NUMBER", "credit_debit_card": "ID_NUMBER", "cvv": "ID_NUMBER",
    "pin": "ID_NUMBER", "bank_routing_number": "ID_NUMBER", "employee_id": "ID_NUMBER", "medical_record_number": "ID_NUMBER",
    "health_plan_beneficiary_number": "ID_NUMBER", "certificate_license_number": "ID_NUMBER", "tax_id": "ID_NUMBER",
    "license_plate": "ID_NUMBER", "vehicle_identifier": "ID_NUMBER", "device_identifier": "ID_NUMBER", "unique_id": "ID_NUMBER",
    "biometric_identifier": "ID_NUMBER",
    "user_name": "ONLINE_ID", "ipv4": "ONLINE_ID", "ipv6": "ONLINE_ID", "mac_address": "ONLINE_ID",
    "password": "SECRET", "api_key": "SECRET", "http_cookie": "SECRET",
}


def slug(s):
    return re.sub(r"[^a-z0-9]+", "-", s.lower()).strip("-")[:40]


def entities(text, spans, mapping):
    """Distinct (type, text) items from the spans, using each span's own text and keeping only text that is in the document."""
    counts = {}
    for s in spans:
        t = mapping.get(s["label"])
        value = str(s["text"] if s.get("text") is not None else text[s["start"]:s["end"]]).strip()
        if t is None or len(value) < 2 or value not in text:
            continue
        counts[(t, value)] = counts.get((t, value), 0) + 1
    return [{"where": "body", "type": t, "text": v, "occurrences": n} for (t, v), n in counts.items()]


def write_corpus(root, input_dir, name, title, readme, docs):
    for sub in ("text", "ground-truth"):
        (root / sub).mkdir(parents=True, exist_ok=True)
    input_dir.mkdir(parents=True, exist_ok=True)
    manifest = []
    total = 0
    for i, d in enumerate(docs, 1):
        doc_id = f"{name}-{i:03d}-{slug(d['kind'])}"
        for target in (root / "text" / f"{doc_id}.txt", input_dir / f"{doc_id}.txt"):
            with open(target, "w", encoding="utf-8", newline="") as f:
                f.write(d["text"])
        key = {"id": doc_id, "title": f"{d['kind']} ({title})", "formats": ["txt"], "scanKinds": [], "entities": d["entities"],
               "mustPreserve": [], "judgedCategories": JUDGED}
        with open(root / "ground-truth" / f"{doc_id}.json", "w", encoding="utf-8", newline="") as f:
            f.write(json.dumps(key, indent=1, ensure_ascii=False) + "\n")
        manifest.append([doc_id, d["source_id"], d["kind"], d["group"], len(d["text"]), len(d["entities"])])
        total += len(d["entities"])
    with open(root / "manifest.csv", "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f)
        w.writerow(["doc_id", "source_id", "document_type", "group", "characters", "key_items"])
        w.writerows(manifest)
    (root / "README.md").write_text(readme.format(n=len(docs), items=total), encoding="utf-8")
    return total


GRETEL_README = """# External corpus: Gretel (finance documents)

{n} English documents from the **test split** of [gretelai/synthetic_pii_finance_multilingual](https://huggingface.co/datasets/gretelai/synthetic_pii_finance_multilingual)
(Apache 2.0, fully synthetic), with {items} labelled sensitive items (distinct text per document). Built by `tools/eval/import_external.py`.

**Choice of documents.** Up to 3 per document type, drawn at random (seed {seed}), from the test documents that are *not* in
`tests/HeldOutCorpus`. No quality filter was applied. The documents come from the same generator as the held-out corpus, so they test new documents
but not a new generator; see the Nemotron corpus for that.

**Answer key.** The generator's own labels, mapped to this project's categories (the same mapping as the held-out corpus). They have **not** been audited,
and the held-out audit found that this generator leaves many real items unlabelled, so a redaction of a genuine but unlabelled item counts as
over-redaction here and precision is understated. Generic dates and times, coordinates and bank identifier codes are not on the key.
Only the categories in `judgedCategories` are judged. Do not tune rules or prompts against this corpus.

**Limits.** Text only (no Word, PDF or image versions). Attribution: Gretel AI, Apache 2.0.
"""

NEMOTRON_README = """# External corpus: Nemotron-PII

{n} documents from the **test split** of [nvidia/Nemotron-PII](https://huggingface.co/datasets/nvidia/Nemotron-PII) (CC-BY-4.0, fully synthetic,
attribution to NVIDIA required), with {items} labelled sensitive items (distinct text per document). Built by `tools/eval/import_external.py`.

**Choice of documents.** A seeded random draw (seed {seed}), the same number from each of the 29 domains (banking, credit, mortgage, insurance,
healthcare, public safety, elections, ...), length 150 to 5,000 characters. No quality filter was applied. A different generator and labeller from
the held-out corpus, so this is the more independent test.

**Answer key.** The dataset's labels, mapped to this project's categories (`tools/eval/import_external.py`). The spans' stated offsets do not always
match the text, so each item is taken from the span's own text and kept only if that text occurs in the document. Not audited by a person.
Generic dates and times, cities, countries, URLs and coordinates are not on the key. Gender, age, job title, race, religion and similar are not
judged either way. Do not tune rules or prompts against this corpus.

**Limits.** Text only (no Word, PDF or image versions). Many are short, plain passages. Attribution: NVIDIA, CC-BY-4.0.
"""


def main():
    import pyarrow.parquet as pq

    ap = argparse.ArgumentParser()
    ap.add_argument("--data", default="datasets/external")
    ap.add_argument("--gretel-per-type", type=int, default=3)
    ap.add_argument("--nemotron-per-domain", type=int, default=8)
    args = ap.parse_args()
    data = Path(args.data)
    rng = random.Random(SEED)

    held = {p.read_text(encoding="utf-8") for p in Path("tests/HeldOutCorpus/text").glob("*.txt")}
    g = pq.read_table(data / "gretel/English_test.parquet").to_pylist()
    fresh = [r for r in g if r["generated_text"] not in held and 150 <= len(r["generated_text"]) <= 5000]
    print(f"Gretel: {len(g)} English test documents, {sum(r['generated_text'] in held for r in g)} already in the held-out corpus, {len(fresh)} usable.")
    by_type = {}
    for r in fresh:
        by_type.setdefault(r["document_type"], []).append(r)
    gdocs = []
    for kind in sorted(by_type):
        for r in rng.sample(by_type[kind], min(args.gretel_per_type, len(by_type[kind]))):
            gdocs.append({"text": r["generated_text"], "kind": kind, "group": r["domain"], "source_id": str(r["__index_level_0__"]),
                          "entities": entities(r["generated_text"], json.loads(r["pii_spans"]), GRETEL_MAP)})

    n = pq.read_table(data / "nemotron/test.parquet").to_pylist()
    ok = [r for r in n if 150 <= len(r["text"]) <= 5000]
    by_domain = {}
    for r in ok:
        by_domain.setdefault(r["domain"], []).append(r)
    ndocs = []
    for dom in sorted(by_domain):
        for r in rng.sample(by_domain[dom], min(args.nemotron_per_domain, len(by_domain[dom]))):
            ndocs.append({"text": r["text"], "kind": f"{dom} - {r['document_type']}", "group": dom, "source_id": r["uid"],
                          "entities": entities(r["text"], ast.literal_eval(r["spans"]), NEMOTRON_MAP)})
    print(f"Nemotron: {len(n)} test documents, {len(ok)} of a usable length, {len(by_domain)} domains.")

    for name, root, inp, title, readme, docs in (
        ("ext-gretel", Path("tests/ExternalGretelCorpus"), Path("in/external-gretel"), "external finance document", GRETEL_README, gdocs),
        ("ext-nemotron", Path("tests/ExternalNemotronCorpus"), Path("in/external-nemotron"), "external synthetic document", NEMOTRON_README, ndocs),
    ):
        for old in list(root.glob("text/*")) + list(root.glob("ground-truth/*")) + (list(inp.glob("*.txt")) if inp.exists() else []):
            old.unlink()
        items = write_corpus(root, inp, name, title, readme.replace("{seed}", str(SEED)), docs)
        print(f"{name}: {len(docs)} documents, {items} key items -> {root} and {inp}")


if __name__ == "__main__":
    main()
