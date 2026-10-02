# Held-out corpus (finance documents)

300 English documents from the **test split** of [gretelai/synthetic_pii_finance_multilingual](https://huggingface.co/datasets/gretelai/synthetic_pii_finance_multilingual) (Apache 2.0, fully synthetic: no real
individuals are represented), covering 60 document types, with 938 labelled sensitive items (distinct text per document).
Imported by `tools/AiDocumentRedactor.HeldOutImporter`; to rebuild, run it from the repository root.

**Why it exists.** No rule, threshold, prompt or answer key in this project was written after looking at these documents, so scores on them
show how the system does on documents it has not been shaped around. Do not tune against it: if you do, build a fresh one.

**How the answer key was made.** The dataset labels each sensitive span. They are mapped to this project's categories: names to PERSON,
company to COMPANY, street addresses to ADDRESS, emails to EMAIL, phone numbers to PHONE, dates of birth to DATE_OF_BIRTH, national, passport,
licence, bank, card, employee and customer numbers to ID_NUMBER, IP addresses and user names to ONLINE_ID, passwords and API keys to SECRET.
Generic dates and times, coordinates and bank identifier codes are **not** on the key (they are not sensitive under this project's policy), so a
model that redacts them counts as over-redacting.

**Limits.** These are generated finance documents (loan applications, policies, contracts, emails, support tickets, statements and machine
formats such as SWIFT, FIX and XBRL), not letters, medical notes or scans. The labels come from the generator and were not checked by a
person, so a few may be missing or wrong; treat small differences between models with caution. Text only (no Word, PDF or image versions).

Documents by type: Annual Report (5); Audit Report (5); BAI Format (5); Bank Statement (5); Bill of Lading (5); Business Plan (5); Compliance Certificate (5); Corporate Governance Guidelines (5); Corporate Tax Return (5); Credit Application (5); Credit Card Application (5); Credit Card Statement (5); Cryptocurrency Transaction Report (5); CSV (5); Currency Exchange Rate Sheet (5); Customer Agreement (5); Customer support conversational log (5); Dispute Resolution Policy (5); EDI (5); Email (5); Employment Contract (5); Financial Aid Application (5); Financial Data Feed (5); Financial Disclosure Statement (5); Financial Forecast (5); Financial Regulatory Compliance Report (5); Financial Risk Assessment (5); Financial Statement (5); FIX Protocol (5); FpML (5); Health Insurance Claim Form (5); Insurance Claim Form (5); Insurance Policy (5); Investment Prospectus (5); ISDA Definition (5); IT support ticket (5); Loan Agreement (5); Loan Application (5); Mortgage Amortization Schedule (5); Mortgage Contract (5); MT940 (5); Payment Confirmation (5); Pension Plan Agreement (5); Policyholder's Report (5); Privacy Policy (5); Product Disclosure Statement (5); Real Estate Loan Agreement (5); Regulatory Compliance Guide (5); Regulatory Filing (5); Renewal Reminder (5); Safety Data Sheet (5); Securities Prospectus (5); Shareholder Agreement (5); Supply Chain Management Agreement (5); SWIFT Message (5); Tax Assessment Notice (5); Tax Return (5); Trade Confirmation (5); Transaction Confirmation (5); XBRL (5).