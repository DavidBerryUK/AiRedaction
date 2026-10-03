# Answer-key audit

Built from `v5-heldout.scores.json`. Compared the answer key of `/Users/davidberry/sources/CSharpe/AiDocumentRedactor/tests/HeldOutCorpus` with what at least 3 of 5 models (phi4, gemma4:e4b, qwen3.6:27b, gemma4:31b, gpt-oss) agree on.

## Redacted by most models but not on the key

| Decision | Items | Meaning |
|---|---:|---|
| Add | 242 | sensitive and missing from the key: added |
| Reject | 58 | not sensitive (placeholder, public body, reference code, masked or generic text): a real over-redaction, left as it is |
| Ignore | 167 | cannot be decided by a rule, or a category the key does not judge: left out of scoring |

## On the key but missed by most models

| Decision | Items | Meaning |
|---|---:|---|
| Keep | 4 | a real item: stays on the key, and the models really did miss it |
| Remove | 34 | not sensitive (placeholder, generic or lower-case phrase, city alone, masked digits): removed from the key |

## Rules used

- Placeholders (`[Company Name]`, `____`), text with no letters or digits, generic words (Company, Firm, Borrower…), and public bodies (HMRC, FCA, IRS, GAAP…) are never sensitive.
- Categories this key never labels (GENDER, AGE, CONTEXTUAL, DOMAIN, COMPANY_ID) are not judged at all.
- Emails and phone numbers are added. Passwords and keys of 8 or more characters are added. IP addresses, wallet addresses and similar machine identifiers are added; web addresses are not.
- Identifier numbers are added when they have 6 or more digits (or are a sort code); bank identifier codes (BIC), securities numbers (ISIN), short reference codes (ABC017) and masked numbers are not.
- Addresses are added unless they are only a city and country (a place, which this project flags). Names are added when they look like names. Company names are added only with a company marker (Ltd, PLC, Bank, Trust…) or as a coded name; other company-like words are left out of scoring.
- Dates are added as a date of birth only when the year is 2008 or earlier. A missed key item is removed when it is a placeholder, a generic word, an all-lower-case phrase, a city alone, the last digits of a masked number, or a company-like phrase with no company marker.

## The decisions

| Document | Text | Category | Models | Decision |
|---|---|---|---:|---|
| text/heldout-003-annual-report.txt | `YZ Corporation` | COMPANY | 5 | Add |
| text/heldout-004-annual-report.txt | `2 024 Inc.` | COMPANY | 4 | Add |
| text/heldout-011-bai-format.txt | `00472329181` | PHONE | 3 | Add |
| text/heldout-011-bai-format.txt | `BAI02100001` | ID_NUMBER | 5 | Add |
| text/heldout-011-bai-format.txt | `PASTOR UREÑA` | PERSON | 3 | Add |
| text/heldout-012-bai-format.txt | `123456789` | ID_NUMBER | 3 | Add |
| text/heldout-012-bai-format.txt | `94043` | ADDRESS | 4 | Add |
| text/heldout-012-bai-format.txt | `USD Bank` | COMPANY | 5 | Add |
| text/heldout-014-bai-format.txt | `123456` | ID_NUMBER | 4 | Add |
| text/heldout-014-bai-format.txt | `1234567890` | ID_NUMBER | 4 | Add |
| text/heldout-015-bai-format.txt | `EC2V 1LT` | ADDRESS | 4 | Add |
| text/heldout-015-bai-format.txt | `GB29 ABCD1234567890123456` | ID_NUMBER | 5 | Add |
| text/heldout-015-bai-format.txt | `GB98 EFGH1234567890123456` | ID_NUMBER | 4 | Add |
| text/heldout-016-bank-statement.txt | `4251 Nathan St, Patrickton` | ADDRESS | 5 | Add |
| text/heldout-016-bank-statement.txt | `Royal Bank` | COMPANY | 5 | Add |
| text/heldout-016-bank-statement.txt | `United Bank of Canada` | COMPANY | 5 | Add |
| text/heldout-017-bank-statement.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-018-bank-statement.txt | `123-456-789` | ID_NUMBER | 5 | Add |
| text/heldout-018-bank-statement.txt | `123456789` | ID_NUMBER | 5 | Add |
| text/heldout-018-bank-statement.txt | `456 Elm Street, Toronto, ON, M5H 2L7` | ADDRESS | 5 | Add |
| text/heldout-018-bank-statement.txt | `First Federal Bank of Canada` | COMPANY | 5 | Add |
| text/heldout-019-bank-statement.txt | `123456789` | ID_NUMBER | 5 | Add |
| text/heldout-020-bank-statement.txt | `1-800-123-4567` | PHONE | 5 | Add |
| text/heldout-021-bill-of-lading.txt | `35 High Street,        London, NW1 5UH        United Kingdom` | ADDRESS | 3 | Add |
| text/heldout-021-bill-of-lading.txt | `ABC Enterprises` | COMPANY | 5 | Add |
| text/heldout-025-bill-of-lading.txt | `Unit 7, Riverside Park Southampton, SO15 3TG UK` | ADDRESS | 3 | Add |
| text/heldout-027-business-plan.txt | `Identify Potential Partners` | COMPANY | 5 | Add |
| text/heldout-030-business-plan.txt | `Culinary School` | COMPANY | 5 | Add |
| text/heldout-033-compliance-certificate.txt | `Milagros` | PERSON | 4 | Add |
| text/heldout-040-corporate-governance-guidelines.txt | `NFMUGBGD530` | COMPANY | 5 | Add |
| text/heldout-043-corporate-tax-return.txt | `08-32-10` | ID_NUMBER | 5 | Add |
| text/heldout-043-corporate-tax-return.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-044-corporate-tax-return.txt | `154` | ADDRESS | 4 | Add |
| text/heldout-045-corporate-tax-return.txt | `12-3456789` | ID_NUMBER | 4 | Add |
| text/heldout-045-corporate-tax-return.txt | `987654321` | ID_NUMBER | 3 | Add |
| text/heldout-048-credit-application.txt | `11-22-33` | ID_NUMBER | 3 | Add |
| text/heldout-048-credit-application.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-055-credit-card-application.txt | `Danielring 1` | ADDRESS | 5 | Add |
| text/heldout-057-credit-card-statement.txt | `+44 973 771 5733` | PHONE | 5 | Add |
| text/heldout-057-credit-card-statement.txt | `PO Box 542, Chapman Tunnel EH12 5DR, United Kingdom` | ADDRESS | 5 | Add |
| text/heldout-057-credit-card-statement.txt | `Your Bank` | COMPANY | 3 | Add |
| text/heldout-058-credit-card-statement.txt | `18fd:ca22:8edb:4` | ONLINE_ID | 3 | Add |
| text/heldout-062-cryptocurrency-transaction-report.txt | `6 Graham` | ADDRESS | 5 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2` | ID_NUMBER | 3 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3` | ID_NUMBER | 3 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN4` | ID_NUMBER | 3 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN5` | ID_NUMBER | 3 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN6` | ID_NUMBER | 3 | Add |
| text/heldout-064-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN7` | ID_NUMBER | 3 | Add |
| text/heldout-065-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2` | ID_NUMBER | 4 | Add |
| text/heldout-068-csv.txt | `123 Main St` | ADDRESS | 4 | Add |
| text/heldout-068-csv.txt | `1985-05-15` | DATE_OF_BIRTH | 4 | Add |
| text/heldout-068-csv.txt | `1988-11-11` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-068-csv.txt | `1989-12-12` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-068-csv.txt | `1990-03-03` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-068-csv.txt | `1991-09-09` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-068-csv.txt | `1992-07-07` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-068-csv.txt | `234 Cedar St` | ADDRESS | 4 | Add |
| text/heldout-068-csv.txt | `321 Maple St` | ADDRESS | 4 | Add |
| text/heldout-068-csv.txt | `456 Elm St` | ADDRESS | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5555` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5556` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5557` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5558` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5559` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `555-555-5560` | PHONE | 4 | Add |
| text/heldout-068-csv.txt | `654 Pine St` | ADDRESS | 4 | Add |
| text/heldout-068-csv.txt | `789 Oak St` | ADDRESS | 4 | Add |
| text/heldout-069-csv.txt | `012-345-6789` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `123-456-7890` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `234-567-8901` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `345-678-9012` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `456-789-0123` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `567-890-1234` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `678-901-2345` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `789-012-3456` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `890-123-4567` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `901-234-5678` | PHONE | 5 | Add |
| text/heldout-069-csv.txt | `Alice` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Anderson` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Brown` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Davis` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Doe` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Eve` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Frank` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Grace` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Heidi` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Johnson` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Miller` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Smith` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Taylor` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Thomas` | PERSON | 5 | Add |
| text/heldout-069-csv.txt | `Williams` | PERSON | 5 | Add |
| text/heldout-071-currency-exchange-rate-sheet.txt | `1994-01-31` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-084-customer-support-conversational-log.txt | `123456789` | ID_NUMBER | 3 | Add |
| text/heldout-086-dispute-resolution-policy.txt | `098 Herbert Passage DH7N 5PP Justinshire` | ADDRESS | 3 | Add |
| text/heldout-090-dispute-resolution-policy.txt | `980-30-0925` | ID_NUMBER | 5 | Add |
| text/heldout-091-edi.txt | `Carrier Address Line 1` | ADDRESS | 3 | Add |
| text/heldout-091-edi.txt | `Carrier Postal Code` | ADDRESS | 3 | Add |
| text/heldout-091-edi.txt | `Consignee Address Line 1` | ADDRESS | 3 | Add |
| text/heldout-091-edi.txt | `Consignee Name` | PERSON | 3 | Add |
| text/heldout-091-edi.txt | `Consignee Postal Code` | ADDRESS | 3 | Add |
| text/heldout-091-edi.txt | `Shipper Address Line 1` | ADDRESS | 3 | Add |
| text/heldout-091-edi.txt | `Shipper Name` | PERSON | 3 | Add |
| text/heldout-091-edi.txt | `Shipper Postal Code` | ADDRESS | 3 | Add |
| text/heldout-092-edi.txt | `1234567890` | PHONE | 4 | Add |
| text/heldout-092-edi.txt | `Los Angeles*CA*90001` | ADDRESS | 3 | Add |
| text/heldout-094-edi.txt | `Acme Inc.` | COMPANY | 3 | Add |
| text/heldout-094-edi.txt | `W6843547` | ID_NUMBER | 3 | Add |
| text/heldout-095-edi.txt | `456 Park Lane` | ADDRESS | 5 | Add |
| text/heldout-095-edi.txt | `789 Elm Street` | ADDRESS | 5 | Add |
| text/heldout-095-edi.txt | `Consignee Name` | PERSON | 3 | Add |
| text/heldout-098-email.txt | `AIzaZkjcPJM9Ht` | SECRET | 5 | Add |
| text/heldout-101-employment-contract.txt | `NW1 2TP` | ADDRESS | 3 | Add |
| text/heldout-106-financial-aid-application.txt | `High School` | COMPANY | 5 | Add |
| text/heldout-107-financial-aid-application.txt | `6092 Sutton Field, New Ericfurt, postal code 29575` | ADDRESS | 3 | Add |
| text/heldout-107-financial-aid-application.txt | `High School` | COMPANY | 5 | Add |
| text/heldout-109-financial-aid-application.txt | `Anytown High School` | COMPANY | 5 | Add |
| text/heldout-109-financial-aid-application.txt | `High School` | COMPANY | 5 | Add |
| text/heldout-119-financial-disclosure-statement.txt | `0x742d35C443742d35333333333333333333333333` | ID_NUMBER | 5 | Add |
| text/heldout-119-financial-disclosure-statement.txt | `0x98765432109876543210987654321098` | ID_NUMBER | 5 | Add |
| text/heldout-119-financial-disclosure-statement.txt | `1FfmbHfn4B2w3r3n429E49453212c272` | ID_NUMBER | 5 | Add |
| text/heldout-119-financial-disclosure-statement.txt | `Cryptocurrency Holdings` | COMPANY | 5 | Add |
| text/heldout-119-financial-disclosure-statement.txt | `MW99999999999999999999999999999999` | ID_NUMBER | 5 | Add |
| text/heldout-131-financial-risk-assessment.txt | `Idrottsstigen` | ADDRESS | 5 | Add |
| text/heldout-138-financial-statement.txt | `391 Bryce Square, Apt. 42076 London, UK, EC3N 2GR` | ADDRESS | 5 | Add |
| text/heldout-142-fix-protocol.txt | `Test_Password` | SECRET | 3 | Add |
| text/heldout-142-fix-protocol.txt | `Test_Username` | ONLINE_ID | 3 | Add |
| text/heldout-146-fpml.txt | `1 Churchill Place, London E14 5HP, United Kingdom` | ADDRESS | 5 | Add |
| text/heldout-148-fpml.txt | `TRA123456` | ID_NUMBER | 3 | Add |
| text/heldout-148-fpml.txt | `TRA123456-20220901` | ID_NUMBER | 3 | Add |
| text/heldout-150-fpml.txt | `EC3V 3PD` | ADDRESS | 5 | Add |
| text/heldout-151-health-insurance-claim-form.txt | `0012345678` | ID_NUMBER | 5 | Add |
| text/heldout-151-health-insurance-claim-form.txt | `ABC Health Insurance` | COMPANY | 5 | Add |
| text/heldout-151-health-insurance-claim-form.txt | `MD-123456` | ID_NUMBER | 4 | Add |
| text/heldout-152-health-insurance-claim-form.txt | `0012345678` | PHONE | 5 | Add |
| text/heldout-153-health-insurance-claim-form.txt | `1234567890` | ID_NUMBER | 5 | Add |
| text/heldout-154-health-insurance-claim-form.txt | `33133` | ADDRESS | 3 | Add |
| text/heldout-154-health-insurance-claim-form.txt | `3567 Sunshine St., 15.916498 N, -59.876838 E` | ADDRESS | 4 | Add |
| text/heldout-157-insurance-claim-form.txt | `123456789` | ID_NUMBER | 4 | Add |
| text/heldout-159-insurance-claim-form.txt | `L-123456789` | ID_NUMBER | 4 | Add |
| text/heldout-159-insurance-claim-form.txt | `PO Box 1234 Anytown, USA 12345-67` | ADDRESS | 5 | Add |
| text/heldout-160-insurance-claim-form.txt | `TP-1234567` | ID_NUMBER | 3 | Add |
| text/heldout-165-insurance-policy.txt | `2023-BP-12854` | ID_NUMBER | 3 | Add |
| text/heldout-170-investment-prospectus.txt | `6789, 123 Fake Street    Anytown, US` | ADDRESS | 3 | Add |
| text/heldout-183-loan-agreement.txt | `JHMES3H62LA025372` | ID_NUMBER | 3 | Add |
| text/heldout-187-loan-application.txt | `Financial Details Bank` | COMPANY | 5 | Add |
| text/heldout-187-loan-application.txt | `TD Bank` | COMPANY | 5 | Add |
| text/heldout-188-loan-application.txt | `020 1234 5678` | PHONE | 5 | Add |
| text/heldout-188-loan-application.txt | `AB123456C` | ID_NUMBER | 5 | Add |
| text/heldout-190-loan-application.txt | `Community Group` | COMPANY | 5 | Add |
| text/heldout-196-mortgage-contract.txt | `DFMVDEWF223` | COMPANY | 5 | Add |
| text/heldout-201-mt940.txt | `12345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `1312345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `1412345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `1512345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `1612345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `1712345678901234567890` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `18123456789` | ID_NUMBER | 4 | Add |
| text/heldout-201-mt940.txt | `51501 PAUL ESTATES, APT. 08925` | ADDRESS | 4 | Add |
| text/heldout-201-mt940.txt | `LAURENCE T. MORENO` | PERSON | 4 | Add |
| text/heldout-202-mt940.txt | `191234567890` | ID_NUMBER | 4 | Add |
| text/heldout-202-mt940.txt | `OOFFXXXGB2LXXX0123456789` | ID_NUMBER | 3 | Add |
| text/heldout-203-mt940.txt | `1234567890` | ID_NUMBER | 5 | Add |
| text/heldout-203-mt940.txt | `2234567890123456` | ID_NUMBER | 4 | Add |
| text/heldout-203-mt940.txt | `4567890123` | ID_NUMBER | 5 | Add |
| text/heldout-203-mt940.txt | `JBRO` | PERSON | 5 | Add |
| text/heldout-203-mt940.txt | `JBROWN` | PERSON | 5 | Add |
| text/heldout-204-mt940.txt | `0000000000` | ID_NUMBER | 4 | Add |
| text/heldout-204-mt940.txt | `1234567890123456` | ID_NUMBER | 4 | Add |
| text/heldout-204-mt940.txt | `4567845674567456` | ID_NUMBER | 3 | Add |
| text/heldout-204-mt940.txt | `ABC Bank` | COMPANY | 5 | Add |
| text/heldout-204-mt940.txt | `MARK SIMP` | PERSON | 4 | Add |
| text/heldout-205-mt940.txt | `1234567890` | ID_NUMBER | 5 | Add |
| text/heldout-205-mt940.txt | `20220520123456` | ID_NUMBER | 5 | Add |
| text/heldout-205-mt940.txt | `5243611ABC` | ID_NUMBER | 5 | Add |
| text/heldout-206-payment-confirmation.txt | `7fde39a0-1d2a-4e03-8f4a-d33ae2bf8f5c` | ID_NUMBER | 3 | Add |
| text/heldout-208-payment-confirmation.txt | `123 Maple Street, Anytown, USA` | ADDRESS | 5 | Add |
| text/heldout-208-payment-confirmation.txt | `123456789` | ID_NUMBER | 5 | Add |
| text/heldout-208-payment-confirmation.txt | `First National Bank` | COMPANY | 5 | Add |
| text/heldout-210-payment-confirmation.txt | `ABC Company Ltd` | COMPANY | 5 | Add |
| text/heldout-212-pension-plan-agreement.txt | `Hon` | PERSON | 3 | Add |
| text/heldout-216-policyholder-s-report.txt | `1-800-EXAMPLE-INS` | PHONE | 3 | Add |
| text/heldout-219-policyholder-s-report.txt | `123456789` | ID_NUMBER | 4 | Add |
| text/heldout-219-policyholder-s-report.txt | `456123789` | ID_NUMBER | 4 | Add |
| text/heldout-219-policyholder-s-report.txt | `987654321` | ID_NUMBER | 4 | Add |
| text/heldout-219-policyholder-s-report.txt | `de42714ebBf36ea6Ce53` | SECRET | 4 | Add |
| text/heldout-219-policyholder-s-report.txt | `Elite Health Insurance` | COMPANY | 3 | Add |
| text/heldout-219-policyholder-s-report.txt | `Premier Health Insurance` | COMPANY | 3 | Add |
| text/heldout-219-policyholder-s-report.txt | `Supreme Health Insurance` | COMPANY | 3 | Add |
| text/heldout-227-product-disclosure-statement.txt | `Luxor Capital Management Ltd.` | COMPANY | 5 | Add |
| text/heldout-234-real-estate-loan-agreement.txt | `XYZ Financial Corporation` | COMPANY | 5 | Add |
| text/heldout-239-regulatory-compliance-guide.txt | `Canadian Advertising Standards Council` | COMPANY | 3 | Add |
| text/heldout-240-regulatory-compliance-guide.txt | `Ground School` | COMPANY | 5 | Add |
| text/heldout-241-regulatory-filing.txt | `December 24, 1977` | DATE_OF_BIRTH | 3 | Add |
| text/heldout-247-renewal-reminder.txt | `+XXX-XXX-XXXX` | PHONE | 4 | Add |
| text/heldout-259-securities-prospectus.txt | `ABC Corp.` | COMPANY | 3 | Add |
| text/heldout-262-shareholder-agreement.txt | `XMYTGBDH508` | COMPANY | 5 | Add |
| text/heldout-271-swift-message.txt | `0000000000` | ID_NUMBER | 3 | Add |
| text/heldout-271-swift-message.txt | `522932ABC12` | ID_NUMBER | 4 | Add |
| text/heldout-271-swift-message.txt | `CHASUS3325330728` | ID_NUMBER | 3 | Add |
| text/heldout-272-swift-message.txt | `CDRCUS66XXX1234567890ABCDEFGHIJK` | ID_NUMBER | 3 | Add |
| text/heldout-272-swift-message.txt | `OOFFXXX0010123456ABCDEFGHIJ` | ID_NUMBER | 3 | Add |
| text/heldout-273-swift-message.txt | `0123456789` | ID_NUMBER | 3 | Add |
| text/heldout-273-swift-message.txt | `065 ELIZABETH PLAINS, APT. 84659/CITY/CA/91010-1234/US` | ADDRESS | 4 | Add |
| text/heldout-273-swift-message.txt | `1234567890` | ID_NUMBER | 3 | Add |
| text/heldout-273-swift-message.txt | `O1234567890` | ID_NUMBER | 3 | Add |
| text/heldout-274-swift-message.txt | `1234567890` | ID_NUMBER | 3 | Add |
| text/heldout-274-swift-message.txt | `3261217076` | ID_NUMBER | 3 | Add |
| text/heldout-274-swift-message.txt | `GB22ABBY09090909090909` | ID_NUMBER | 4 | Add |
| text/heldout-275-swift-message.txt | `GB29WFIB1234567890` | ID_NUMBER | 4 | Add |
| text/heldout-276-tax-assessment-notice.txt | `5476-6d60-7b3` | ID_NUMBER | 4 | Add |
| text/heldout-277-tax-assessment-notice.txt | `456789012` | ID_NUMBER | 4 | Add |
| text/heldout-278-tax-assessment-notice.txt | `213-76554-56` | ID_NUMBER | 3 | Add |
| text/heldout-279-tax-assessment-notice.txt | `1234567890` | ID_NUMBER | 3 | Add |
| text/heldout-280-tax-assessment-notice.txt | `12-34-56` | ID_NUMBER | 5 | Add |
| text/heldout-280-tax-assessment-notice.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-280-tax-assessment-notice.txt | `2023-001234-UK` | ID_NUMBER | 3 | Add |
| text/heldout-281-tax-return.txt | `123-45-6789` | ID_NUMBER | 5 | Add |
| text/heldout-282-tax-return.txt | `12-3456789` | ID_NUMBER | 5 | Add |
| text/heldout-282-tax-return.txt | `S Corporation` | COMPANY | 5 | Add |
| text/heldout-285-tax-return.txt | `Other Partners` | COMPANY | 5 | Add |
| text/heldout-287-trade-confirmation.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-287-trade-confirmation.txt | `60-12-34` | ID_NUMBER | 5 | Add |
| text/heldout-288-trade-confirmation.txt | `12345678A` | ID_NUMBER | 5 | Add |
| text/heldout-288-trade-confirmation.txt | `GB00B1234567` | ID_NUMBER | 3 | Add |
| text/heldout-290-trade-confirmation.txt | `123456789` | ID_NUMBER | 5 | Add |
| text/heldout-290-trade-confirmation.txt | `987654321` | ID_NUMBER | 5 | Add |
| text/heldout-290-trade-confirmation.txt | `First National Bank Bank` | COMPANY | 5 | Add |
| text/heldout-290-trade-confirmation.txt | `Grain Elevator #3, 1234 Wheat Fields Lane, Kansas City, MO 64116` | ADDRESS | 3 | Add |
| text/heldout-290-trade-confirmation.txt | `Second National Bank Bank` | COMPANY | 5 | Add |
| text/heldout-291-transaction-confirmation.txt | `11-11-11` | ID_NUMBER | 5 | Add |
| text/heldout-291-transaction-confirmation.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-291-transaction-confirmation.txt | `123456789` | ID_NUMBER | 3 | Add |
| text/heldout-291-transaction-confirmation.txt | `ABC Bank` | COMPANY | 5 | Add |
| text/heldout-294-transaction-confirmation.txt | `11-11-11` | ID_NUMBER | 5 | Add |
| text/heldout-294-transaction-confirmation.txt | `12345678` | ID_NUMBER | 5 | Add |
| text/heldout-294-transaction-confirmation.txt | `123456789` | ID_NUMBER | 4 | Add |
| text/heldout-294-transaction-confirmation.txt | `HSBC Bank` | COMPANY | 5 | Add |
| text/heldout-294-transaction-confirmation.txt | `info@abcv insurance.com` | EMAIL | 4 | Add |
| text/heldout-298-xbrl.txt | `Liliana` | PERSON | 5 | Add |
| text/heldout-298-xbrl.txt | `Proietti-Trapani` | PERSON | 5 | Add |
| text/heldout-038-corporate-governance-guidelines.txt | `[Company Name]` | COMPANY | 3 | Reject |
| text/heldout-043-corporate-tax-return.txt | `HM Revenue and Customs` | COMPANY | 5 | Reject |
| text/heldout-044-corporate-tax-return.txt | `Internal Revenue Service` | COMPANY | 3 | Reject |
| text/heldout-047-credit-application.txt | `EQREUSUQ895` | ID_NUMBER | 3 | Reject |
| text/heldout-048-credit-application.txt | `12th June 2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-056-credit-card-statement.txt | `**** 1234` | ID_NUMBER | 3 | Reject |
| text/heldout-057-credit-card-statement.txt | `**** 1234` | ID_NUMBER | 3 | Reject |
| text/heldout-059-credit-card-statement.txt | `02/05/2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-059-credit-card-statement.txt | `02/25/2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-060-credit-card-statement.txt | `28/02/2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-065-cryptocurrency-transaction-report.txt | `1BvBMSEYstWetqTFn5A` | ID_NUMBER | 4 | Reject |
| text/heldout-076-customer-agreement.txt | `July 13, 2021` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-077-customer-agreement.txt | `1st day of March, 2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-077-customer-agreement.txt | `28th day of February, 2024` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-080-customer-agreement.txt | `[Company Name]` | COMPANY | 3 | Reject |
| text/heldout-087-dispute-resolution-policy.txt | `ABJCDEDI492` | ID_NUMBER | 4 | Reject |
| text/heldout-088-dispute-resolution-policy.txt | `[Company Name]` | COMPANY | 4 | Reject |
| text/heldout-091-edi.txt | `Freight Bill Number` | ID_NUMBER | 3 | Reject |
| text/heldout-093-edi.txt | `19650101` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-119-financial-disclosure-statement.txt | `1BvBMSEY StuartRH323` | ID_NUMBER | 5 | Reject |
| text/heldout-119-financial-disclosure-statement.txt | `PIMIUSPB765` | ID_NUMBER | 4 | Reject |
| text/heldout-151-health-insurance-claim-form.txt | `54321` | ID_NUMBER | 4 | Reject |
| text/heldout-154-health-insurance-claim-form.txt | `2022-03-15` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-154-health-insurance-claim-form.txt | `2022-03-16` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-164-insurance-policy.txt | `January 1, 2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-171-isda-definition.txt | `[Counterparty Name]` | COMPANY | 3 | Reject |
| text/heldout-171-isda-definition.txt | `[Your Company Name]` | COMPANY | 3 | Reject |
| text/heldout-180-it-support-ticket.txt | `https://accounts.example.com/password-reset` | ONLINE_ID | 4 | Reject |
| text/heldout-182-loan-agreement.txt | `1st day of January, 2023` | DATE_OF_BIRTH | 4 | Reject |
| text/heldout-205-mt940.txt | `BBBBGB2LXXX` | ID_NUMBER | 5 | Reject |
| text/heldout-208-payment-confirmation.txt | `123` | ID_NUMBER | 4 | Reject |
| text/heldout-213-pension-plan-agreement.txt | `Internal Revenue Service` | COMPANY | 3 | Reject |
| text/heldout-219-policyholder-s-report.txt | `KPEEUSZS411` | ID_NUMBER | 3 | Reject |
| text/heldout-225-privacy-policy.txt | `[Company Name]` | COMPANY | 3 | Reject |
| text/heldout-232-real-estate-loan-agreement.txt | `__________ Bank` | COMPANY | 4 | Reject |
| text/heldout-236-regulatory-compliance-guide.txt | `FCA` | COMPANY | 4 | Reject |
| text/heldout-236-regulatory-compliance-guide.txt | `Financial` | COMPANY | 5 | Reject |
| text/heldout-236-regulatory-compliance-guide.txt | `Financial Reporting Council` | COMPANY | 4 | Reject |
| text/heldout-236-regulatory-compliance-guide.txt | `FRC` | COMPANY | 4 | Reject |
| text/heldout-236-regulatory-compliance-guide.txt | `GAAP` | COMPANY | 3 | Reject |
| text/heldout-247-renewal-reminder.txt | `[Company Name]` | COMPANY | 3 | Reject |
| text/heldout-248-renewal-reminder.txt | `01 June 2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-250-renewal-reminder.txt | `01/04/2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-259-securities-prospectus.txt | `US1234567890` | ID_NUMBER | 3 | Reject |
| text/heldout-269-supply-chain-management-agreement.txt | `1st day of March, 2` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-271-swift-message.txt | `CDRCGB22GPT` | ID_NUMBER | 3 | Reject |
| text/heldout-271-swift-message.txt | `CHASMTON` | ID_NUMBER | 3 | Reject |
| text/heldout-273-swift-message.txt | `BCSMNY33` | ID_NUMBER | 3 | Reject |
| text/heldout-274-swift-message.txt | `ABBYGB2L` | ID_NUMBER | 3 | Reject |
| text/heldout-275-swift-message.txt | `WFIBGB2LXXX` | ID_NUMBER | 3 | Reject |
| text/heldout-279-tax-assessment-notice.txt | `AB12 CD34` | ID_NUMBER | 3 | Reject |
| text/heldout-279-tax-assessment-notice.txt | `https://www.gov.uk/vehicle-tax` | ONLINE_ID | 5 | Reject |
| text/heldout-279-tax-assessment-notice.txt | `https://www.gov.uk/vehicle-tax-evasion` | ONLINE_ID | 5 | Reject |
| text/heldout-281-tax-return.txt | `Internal Revenue Service` | COMPANY | 4 | Reject |
| text/heldout-283-tax-return.txt | `OVMEUSXS349` | ID_NUMBER | 4 | Reject |
| text/heldout-290-trade-confirmation.txt | `Buyer` | COMPANY | 4 | Reject |
| text/heldout-290-trade-confirmation.txt | `March 15, 2023` | DATE_OF_BIRTH | 3 | Reject |
| text/heldout-290-trade-confirmation.txt | `Seller` | COMPANY | 4 | Reject |
| text/heldout-004-annual-report.txt | `Customer Support Manager` | CONTEXTUAL | 4 | Ignore |
| text/heldout-004-annual-report.txt | `Senior Data Analyst` | CONTEXTUAL | 4 | Ignore |
| text/heldout-004-annual-report.txt | `Senior Software Engineer` | CONTEXTUAL | 4 | Ignore |
| text/heldout-006-audit-report.txt | `Accounts Payable clerk` | CONTEXTUAL | 3 | Ignore |
| text/heldout-006-audit-report.txt | `Professional Institute of Auditors` | COMPANY | 3 | Ignore |
| text/heldout-008-audit-report.txt | `American Institute of Certified Public Accountants` | COMPANY | 4 | Ignore |
| text/heldout-016-bank-statement.txt | `Canadian Tire` | COMPANY | 4 | Ignore |
| text/heldout-016-bank-statement.txt | `Esso` | COMPANY | 4 | Ignore |
| text/heldout-016-bank-statement.txt | `Loblaws` | COMPANY | 4 | Ignore |
| text/heldout-016-bank-statement.txt | `Tim Hortons` | COMPANY | 4 | Ignore |
| text/heldout-020-bank-statement.txt | `Mr.` | GENDER | 3 | Ignore |
| text/heldout-021-bill-of-lading.txt | `MV Ocean Titan` | COMPANY | 3 | Ignore |
| text/heldout-021-bill-of-lading.txt | `Operations Manager` | CONTEXTUAL | 4 | Ignore |
| text/heldout-023-bill-of-lading.txt | `his` | GENDER | 5 | Ignore |
| text/heldout-024-bill-of-lading.txt | `Shipping Manager` | CONTEXTUAL | 3 | Ignore |
| text/heldout-037-corporate-governance-guidelines.txt | `Chief Human Resources Officer` | CONTEXTUAL | 4 | Ignore |
| text/heldout-037-corporate-governance-guidelines.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-039-corporate-governance-guidelines.txt | `his` | GENDER | 5 | Ignore |
| text/heldout-041-corporate-tax-return.txt | `1234567890` | COMPANY_ID | 3 | Ignore |
| text/heldout-042-corporate-tax-return.txt | `LMUGDEES017` | COMPANY_ID | 4 | Ignore |
| text/heldout-044-corporate-tax-return.txt | `Department of the Treasury` | COMPANY | 3 | Ignore |
| text/heldout-048-credit-application.txt | `HSBC` | COMPANY | 3 | Ignore |
| text/heldout-056-credit-card-statement.txt | `Spotify` | COMPANY | 3 | Ignore |
| text/heldout-057-credit-card-statement.txt | `www.yourbank.com/payments` | DOMAIN | 4 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Fashion Hub` | COMPANY | 3 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Fresh Food Market` | COMPANY | 3 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Gourmet Delights` | COMPANY | 3 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Medicine Depot` | COMPANY | 3 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Shopper's Paradise` | COMPANY | 3 | Ignore |
| text/heldout-059-credit-card-statement.txt | `Speedy Fuel` | COMPANY | 3 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Amazon` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `British Airways` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Sainsbury's` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Shell` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Starbucks` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Tesco` | COMPANY | 4 | Ignore |
| text/heldout-060-credit-card-statement.txt | `Uber` | COMPANY | 4 | Ignore |
| text/heldout-068-csv.txt | `Female` | GENDER | 4 | Ignore |
| text/heldout-068-csv.txt | `Male` | GENDER | 4 | Ignore |
| text/heldout-069-csv.txt | `32` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `35` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `36` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `38` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `39` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `42` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `45` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `48` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `50` | AGE | 5 | Ignore |
| text/heldout-069-csv.txt | `Female` | GENDER | 5 | Ignore |
| text/heldout-069-csv.txt | `Male` | GENDER | 5 | Ignore |
| text/heldout-074-currency-exchange-rate-sheet.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-074-currency-exchange-rate-sheet.txt | `www.exchangerates.com` | DOMAIN | 5 | Ignore |
| text/heldout-079-customer-agreement.txt | `12345678` | COMPANY_ID | 5 | Ignore |
| text/heldout-082-customer-support-conversational-log.txt | `RoyalBankUK` | COMPANY | 5 | Ignore |
| text/heldout-087-dispute-resolution-policy.txt | `Rent-a-Judge` | COMPANY | 5 | Ignore |
| text/heldout-091-edi.txt | `Carrier Name` | COMPANY | 4 | Ignore |
| text/heldout-091-edi.txt | `company1` | COMPANY | 5 | Ignore |
| text/heldout-091-edi.txt | `company2` | COMPANY | 5 | Ignore |
| text/heldout-092-edi.txt | `TestCustomer` | COMPANY | 5 | Ignore |
| text/heldout-092-edi.txt | `TestVendor` | COMPANY | 5 | Ignore |
| text/heldout-092-edi.txt | `TestVendor Name` | COMPANY | 4 | Ignore |
| text/heldout-093-edi.txt | `Lab` | COMPANY | 3 | Ignore |
| text/heldout-093-edi.txt | `XYZHealth` | COMPANY | 3 | Ignore |
| text/heldout-093-edi.txt | `XYZHosp` | COMPANY | 3 | Ignore |
| text/heldout-095-edi.txt | `Billing Co.` | COMPANY | 5 | Ignore |
| text/heldout-097-email.txt | `Development Officer` | CONTEXTUAL | 3 | Ignore |
| text/heldout-097-email.txt | `her` | GENDER | 5 | Ignore |
| text/heldout-097-email.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-099-email.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-101-employment-contract.txt | `her` | GENDER | 4 | Ignore |
| text/heldout-101-employment-contract.txt | `his` | GENDER | 4 | Ignore |
| text/heldout-105-employment-contract.txt | `her` | GENDER | 5 | Ignore |
| text/heldout-105-employment-contract.txt | `HER` | GENDER | 4 | Ignore |
| text/heldout-105-employment-contract.txt | `she` | GENDER | 5 | Ignore |
| text/heldout-126-financial-regulatory-compliance-report.txt | `CNB` | COMPANY | 4 | Ignore |
| text/heldout-131-financial-risk-assessment.txt | `her` | GENDER | 5 | Ignore |
| text/heldout-131-financial-risk-assessment.txt | `she` | GENDER | 5 | Ignore |
| text/heldout-140-financial-statement.txt | `Chief Financial Officer (CFO)` | CONTEXTUAL | 3 | Ignore |
| text/heldout-141-fix-protocol.txt | `JPM` | COMPANY | 5 | Ignore |
| text/heldout-141-fix-protocol.txt | `RJF` | COMPANY | 5 | Ignore |
| text/heldout-144-fix-protocol.txt | `BROKER1` | COMPANY | 4 | Ignore |
| text/heldout-144-fix-protocol.txt | `TARGET1` | COMPANY | 4 | Ignore |
| text/heldout-158-insurance-claim-form.txt | `Local Seismic Monitoring Agency` | COMPANY | 4 | Ignore |
| text/heldout-161-insurance-policy.txt | `NFIP` | COMPANY | 5 | Ignore |
| text/heldout-164-insurance-policy.txt | `12345678` | COMPANY_ID | 5 | Ignore |
| text/heldout-164-insurance-policy.txt | `PetCare Cover` | COMPANY | 5 | Ignore |
| text/heldout-167-investment-prospectus.txt | `S&P 500 Index` | COMPANY | 4 | Ignore |
| text/heldout-169-investment-prospectus.txt | `Humanitarian Aid Fund` | COMPANY | 3 | Ignore |
| text/heldout-169-investment-prospectus.txt | `www.humanitarianfund.org` | DOMAIN | 5 | Ignore |
| text/heldout-170-investment-prospectus.txt | `TVTSF` | COMPANY | 4 | Ignore |
| text/heldout-176-it-support-ticket.txt | `she` | GENDER | 5 | Ignore |
| text/heldout-176-it-support-ticket.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-177-it-support-ticket.txt | `Zoom` | COMPANY | 3 | Ignore |
| text/heldout-180-it-support-ticket.txt | `He` | GENDER | 5 | Ignore |
| text/heldout-180-it-support-ticket.txt | `his` | GENDER | 5 | Ignore |
| text/heldout-186-loan-application.txt | `HSBC` | COMPANY | 4 | Ignore |
| text/heldout-186-loan-application.txt | `https://www.moneyadviceservice.org.uk/` | DOMAIN | 3 | Ignore |
| text/heldout-186-loan-application.txt | `https://www.nationaldebtline.org/` | DOMAIN | 4 | Ignore |
| text/heldout-186-loan-application.txt | `The Money Advice Service` | COMPANY | 4 | Ignore |
| text/heldout-187-loan-application.txt | `Equifax Canada` | COMPANY | 5 | Ignore |
| text/heldout-187-loan-application.txt | `Scotiabank` | COMPANY | 4 | Ignore |
| text/heldout-187-loan-application.txt | `Senior Software Engineer` | CONTEXTUAL | 3 | Ignore |
| text/heldout-188-loan-application.txt | `IT Manager` | CONTEXTUAL | 5 | Ignore |
| text/heldout-189-loan-application.txt | `Credit.org` | COMPANY | 5 | Ignore |
| text/heldout-189-loan-application.txt | `https://www.credit.org/` | DOMAIN | 3 | Ignore |
| text/heldout-189-loan-application.txt | `https://www.fcaa.org/` | DOMAIN | 3 | Ignore |
| text/heldout-189-loan-application.txt | `https://www.ftc.gov/search?q=credit+counseling` | DOMAIN | 3 | Ignore |
| text/heldout-189-loan-application.txt | `https://www.nfcc.org/` | DOMAIN | 3 | Ignore |
| text/heldout-189-loan-application.txt | `Software Engineer` | CONTEXTUAL | 3 | Ignore |
| text/heldout-195-mortgage-amortization-schedule.txt | `Jumbo Mortgage` | COMPANY | 3 | Ignore |
| text/heldout-202-mt940.txt | `ABC TRAVEL LTD` | COMPANY | 5 | Ignore |
| text/heldout-203-mt940.txt | `ABC BANK` | COMPANY | 5 | Ignore |
| text/heldout-211-pension-plan-agreement.txt | `Seanville` | COMPANY | 4 | Ignore |
| text/heldout-212-pension-plan-agreement.txt | `her` | GENDER | 3 | Ignore |
| text/heldout-212-pension-plan-agreement.txt | `his` | GENDER | 3 | Ignore |
| text/heldout-213-pension-plan-agreement.txt | `59½` | AGE | 3 | Ignore |
| text/heldout-213-pension-plan-agreement.txt | `her` | GENDER | 4 | Ignore |
| text/heldout-213-pension-plan-agreement.txt | `his` | GENDER | 4 | Ignore |
| text/heldout-214-pension-plan-agreement.txt | `Springfield Municipal Employees' Retirement Plan` | COMPANY | 4 | Ignore |
| text/heldout-215-pension-plan-agreement.txt | `Local Government Employees' Pension Scheme` | COMPANY | 4 | Ignore |
| text/heldout-215-pension-plan-agreement.txt | `LOCAL GOVERNMENT EMPLOYEES' PENSION SCHEME` | COMPANY | 4 | Ignore |
| text/heldout-215-pension-plan-agreement.txt | `Local Government Pension Committee` | COMPANY | 4 | Ignore |
| text/heldout-215-pension-plan-agreement.txt | `Local Government Pension Scheme Advisory Board` | COMPANY | 4 | Ignore |
| text/heldout-227-product-disclosure-statement.txt | `Luxor` | COMPANY | 5 | Ignore |
| text/heldout-227-product-disclosure-statement.txt | `Luxor's` | COMPANY | 3 | Ignore |
| text/heldout-230-product-disclosure-statement.txt | `Bloomberg Commodity Index` | COMPANY | 5 | Ignore |
| text/heldout-236-regulatory-compliance-guide.txt | `Data Protection Act 2018` | COMPANY | 3 | Ignore |
| text/heldout-236-regulatory-compliance-guide.txt | `GDPR` | COMPANY | 3 | Ignore |
| text/heldout-236-regulatory-compliance-guide.txt | `General Data Protection Regulation` | COMPANY | 3 | Ignore |
| text/heldout-236-regulatory-compliance-guide.txt | `International Financial Reporting Standards` | COMPANY | 3 | Ignore |
| text/heldout-236-regulatory-compliance-guide.txt | `UK Generally Accepted Accounting Practice` | COMPANY | 5 | Ignore |
| text/heldout-241-regulatory-filing.txt | `Community Engagement Officer` | CONTEXTUAL | 3 | Ignore |
| text/heldout-241-regulatory-filing.txt | `She` | GENDER | 5 | Ignore |
| text/heldout-244-regulatory-filing.txt | `his` | GENDER | 5 | Ignore |
| text/heldout-245-regulatory-filing.txt | `1234567` | COMPANY_ID | 4 | Ignore |
| text/heldout-257-securities-prospectus.txt | `NYSE` | COMPANY | 3 | Ignore |
| text/heldout-266-supply-chain-management-agreement.txt | `ABC` | COMPANY | 3 | Ignore |
| text/heldout-268-supply-chain-management-agreement.txt | `her` | GENDER | 5 | Ignore |
| text/heldout-271-swift-message.txt | `CHASUS33` | COMPANY | 4 | Ignore |
| text/heldout-273-swift-message.txt | `ABC BANK` | COMPANY | 4 | Ignore |
| text/heldout-275-swift-message.txt | `ABC001` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC002` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC003` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC004` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC005` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC006` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC007` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC008` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC009` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC010` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC011` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC012` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC013` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC014` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC015` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC016` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC017` | COMPANY | 3 | Ignore |
| text/heldout-275-swift-message.txt | `ABC018` | COMPANY | 3 | Ignore |
| text/heldout-276-tax-assessment-notice.txt | `Her Majesty's Revenue and Customs` | COMPANY | 4 | Ignore |
| text/heldout-280-tax-assessment-notice.txt | `Her Majesty's Revenue and Customs` | COMPANY | 4 | Ignore |
| text/heldout-281-tax-return.txt | `United States Department of the Treasury` | COMPANY | 4 | Ignore |
| text/heldout-285-tax-return.txt | `12-3456789` | COMPANY_ID | 3 | Ignore |
| text/heldout-287-trade-confirmation.txt | `NatWest` | COMPANY | 3 | Ignore |
| text/heldout-287-trade-confirmation.txt | `Quantum` | COMPANY | 5 | Ignore |
| text/heldout-290-trade-confirmation.txt | `Buyer Co.` | COMPANY | 4 | Ignore |
| text/heldout-290-trade-confirmation.txt | `Seller Co.` | COMPANY | 4 | Ignore |
| text/heldout-294-transaction-confirmation.txt | `Claims Manager` | CONTEXTUAL | 3 | Ignore |
| text/heldout-019-bank-statement.txt | `2345 River Road, Apt. 091` | ADDRESS | 3 | Keep |
| text/heldout-019-bank-statement.txt | `San Francisco, CA 94112` | ADDRESS | 4 | Keep |
| text/heldout-111-financial-data-feed.txt | `a741:45da:c53e:2f8:835a:e766:162b:4220` | ONLINE_ID | 3 | Keep |
| text/heldout-111-financial-data-feed.txt | `a741:45da:c53e:2f8:835a:e766:162b:4221` | ONLINE_ID | 3 | Keep |
| text/heldout-016-bank-statement.txt | `--------------` | PERSON | 5 | Remove |
| text/heldout-016-bank-statement.txt | `---------------------` | COMPANY | 5 | Remove |
| text/heldout-058-credit-card-statement.txt | `Netflix` | COMPANY | 4 | Remove |
| text/heldout-108-financial-aid-application.txt | `emergency fund` | COMPANY | 4 | Remove |
| text/heldout-108-financial-aid-application.txt | `Emergency Fund` | COMPANY | 4 | Remove |
| text/heldout-110-financial-aid-application.txt | `[Date of Birth]` | DATE_OF_BIRTH | 4 | Remove |
| text/heldout-110-financial-aid-application.txt | `568` | ID_NUMBER | 4 | Remove |
| text/heldout-120-financial-disclosure-statement.txt | `London, UK` | ADDRESS | 5 | Remove |
| text/heldout-122-financial-forecast.txt | `Real Estate Investment Trusts (REITs)` | COMPANY | 5 | Remove |
| text/heldout-139-financial-statement.txt | `Educational Institution` | COMPANY | 5 | Remove |
| text/heldout-140-financial-statement.txt | `Richardshire` | COMPANY | 3 | Remove |
| text/heldout-171-isda-definition.txt | `Firm` | COMPANY | 4 | Remove |
| text/heldout-179-it-support-ticket.txt | `external recipient` | PERSON | 5 | Remove |
| text/heldout-179-it-support-ticket.txt | `external recipients` | PERSON | 5 | Remove |
| text/heldout-216-policyholder-s-report.txt | `date of birth` | DATE_OF_BIRTH | 5 | Remove |
| text/heldout-217-policyholder-s-report.txt | `[Company Address]` | ADDRESS | 3 | Remove |
| text/heldout-220-policyholder-s-report.txt | `address` | ADDRESS | 5 | Remove |
| text/heldout-221-privacy-policy.txt | `[contact information]` | ADDRESS | 4 | Remove |
| text/heldout-225-privacy-policy.txt | `addresses` | ADDRESS | 4 | Remove |
| text/heldout-232-real-estate-loan-agreement.txt | `student housing` | COMPANY | 4 | Remove |
| text/heldout-232-real-estate-loan-agreement.txt | `STUDENT HOUSING` | COMPANY | 5 | Remove |
| text/heldout-235-real-estate-loan-agreement.txt | `_______________ Bank` | COMPANY | 3 | Remove |
| text/heldout-238-regulatory-compliance-guide.txt | `health plans` | COMPANY | 5 | Remove |
| text/heldout-238-regulatory-compliance-guide.txt | `healthcare providers` | COMPANY | 5 | Remove |
| text/heldout-240-regulatory-compliance-guide.txt | `certified aviation maintenance technician` | PERSON | 4 | Remove |
| text/heldout-255-safety-data-sheet.txt | `High-Reach Cleaning Solution` | COMPANY | 5 | Remove |
| text/heldout-257-securities-prospectus.txt | `London, United Kingdom` | ADDRESS | 5 | Remove |
| text/heldout-258-securities-prospectus.txt | `Manchester, UK` | ADDRESS | 4 | Remove |
| text/heldout-261-shareholder-agreement.txt | `address` | ADDRESS | 5 | Remove |
| text/heldout-261-shareholder-agreement.txt | `date of birth` | DATE_OF_BIRTH | 5 | Remove |
| text/heldout-262-shareholder-agreement.txt | `Corporation` | COMPANY | 4 | Remove |
| text/heldout-263-shareholder-agreement.txt | `Corporation` | COMPANY | 3 | Remove |
| text/heldout-265-shareholder-agreement.txt | `Corporation` | COMPANY | 3 | Remove |
| text/heldout-284-tax-return.txt | `United States Internal Revenue Service` | COMPANY | 3 | Remove |
