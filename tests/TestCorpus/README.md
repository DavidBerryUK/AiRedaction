# Test corpus (synthetic)

**All data is invented.** Emails use `example.*` domains, phone numbers are Ofcom drama ranges, IPs are
documentation ranges (203.0.113.x), and the NI number, IBAN, NHS number, AWS key and tokens are published
example values. No real person, company or credential appears anywhere. Do not add real data here.

Regenerate (deterministic) with:

    dotnet run --project tools/AiDocumentRedactor.CorpusGenerator -- tests/TestCorpus

Each source document is authored once with inline markup `[[TYPE|text]]` in
`tools/AiDocumentRedactor.CorpusGenerator/Corpus.cs`, so `ground-truth/<id>.json` is exact by construction.

| Id | Content | Formats | Tests |
|----|---------|---------|-------|
| 01-hr-letter | HR letter: names, address, phone, email, DOB, age, NI number, gender titles | txt, docx, pdf, scans (clean, degraded, image-only pdf) | Core PII in every format incl. OCR |
| 02-services-agreement | Contract with table, VAT/company numbers, IBAN, domain; header and footer text | docx, pdf | Tables, headers/footers, company identifiers |
| 03-customer-email | Email thread, partial names ("Mr Kowalczyk"), password in prose | txt, md | Name variants, prose secrets |
| 04-meeting-notes | Attendees, code name, "in her forties" | md, docx | Approximate age, contextual identifiers |
| 05-incident-report | Connection string, AWS key, bearer token, IP, handle | txt | Secrets and online identifiers |
| 06-customer-list | Name/email/phone/DOB/company rows | csv | Structured data, syntax safety |
| 07-app-config | Connection string, API key | json | Secrets in JSON, still-valid JSON after redaction |
| 08-invoice | Company, VAT, IBAN, contact | pdf, scans | Financial identifiers on scans |
| 09-gp-referral | Patient details, NHS number, age, gender | pdf, docx, scans | Sensitive health-style letter |
| 10-contextual-profile | Indirect identifiers: role, salary, school, event | txt | Tier 4 (contextual) |
| 11-hard-negatives | Nothing to redact: Microsoft Word, HMRC, London, "mark", "42" | txt, docx | Over-redaction / allow-list |
| 12-hygiene-docx | Hidden content: comment, tracked deletion, header, footer, author metadata, text split across runs | docx | Scrubbing and run mapping |

Ground-truth fields: `entities[].where` is `body`, `header`, `footer`, `comment`, `tracked-deletion` or
`metadata`; `mustPreserve` lists strings that must survive redaction. Everything in `entities` must be
absent from a redacted output.
