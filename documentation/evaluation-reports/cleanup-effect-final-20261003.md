# What the clean-up rules would do

Worked out from the spans saved in `final-20261003` with no model run: each model's saved answers have each rule applied, and are scored against the answer keys again. Models: gemma4:31b, gemma4:e4b, gpt-oss, phi4, qwen3.6:27b.

**Read this first.** The rules were chosen after looking at these same documents, so the gain shown is an upper estimate. A fair test needs documents the rules were not based on. The figure that matters most is the second table: how many *right* redactions a rule would take away, because that is the risk.

## Held-out (300 documents)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 98.0% | 91.9% | 94.9% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 98.0% | 92.1% | 94.9% | +0.2 pts | 0.0 pts | 3 | 0 |
| gemma4:31b | masked values | 98.0% | 91.9% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 98.0% | 92.5% | 95.2% | +0.6 pts | 0.0 pts | 11 | 0 |
| gemma4:31b | dates need birth wording | 98.0% | 92.0% | 94.9% | +0.1 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 98.1% | 91.9% | 94.9% | 0.0 pts | +0.1 pts | 0 | -2 |
| gemma4:31b | all rules together | 98.1% | 92.7% | 95.3% | +0.8 pts | +0.1 pts | 15 | -2 |
| gemma4:e4b | no rules (as run) | 88.0% | 74.2% | 80.5% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 87.8% | 75.6% | 81.2% | +1.4 pts | -0.2 pts | 37 | 3 |
| gemma4:e4b | masked values | 88.0% | 74.2% | 80.5% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:e4b | generic terms | 88.0% | 80.8% | 84.2% | +6.6 pts | 0.0 pts | 157 | 0 |
| gemma4:e4b | dates need birth wording | 87.7% | 76.5% | 81.7% | +2.3 pts | -0.3 pts | 60 | 5 |
| gemma4:e4b | IPv6 addresses (adds) | 88.2% | 74.2% | 80.6% | +0.1 pts | +0.2 pts | 0 | -4 |
| gemma4:e4b | all rules together | 87.7% | 85.5% | 86.6% | +11.3 pts | -0.2 pts | 256 | 4 |
| gpt-oss | no rules (as run) | 94.3% | 87.3% | 90.6% |  |  |  |  |
| gpt-oss | placeholders in brackets | 94.1% | 89.7% | 91.9% | +2.4 pts | -0.2 pts | 49 | 3 |
| gpt-oss | masked values | 94.3% | 87.3% | 90.6% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 94.3% | 87.5% | 90.8% | +0.2 pts | 0.0 pts | 4 | 0 |
| gpt-oss | dates need birth wording | 94.3% | 87.7% | 90.9% | +0.4 pts | 0.0 pts | 8 | 0 |
| gpt-oss | IPv6 addresses (adds) | 94.3% | 87.3% | 90.6% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 94.1% | 90.3% | 92.2% | +3.0 pts | -0.2 pts | 60 | 3 |
| phi4 | no rules (as run) | 94.1% | 74.3% | 83.0% |  |  |  |  |
| phi4 | placeholders in brackets | 93.9% | 76.0% | 84.0% | +1.7 pts | -0.2 pts | 47 | 3 |
| phi4 | masked values | 94.1% | 77.2% | 84.8% | +2.9 pts | 0.0 pts | 79 | 0 |
| phi4 | generic terms | 94.1% | 77.6% | 85.0% | +3.3 pts | 0.0 pts | 88 | 0 |
| phi4 | dates need birth wording | 93.7% | 79.9% | 86.2% | +5.6 pts | -0.4 pts | 147 | 7 |
| phi4 | IPv6 addresses (adds) | 94.1% | 74.3% | 83.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 93.5% | 89.8% | 91.6% | +15.5 pts | -0.6 pts | 360 | 10 |
| qwen3.6:27b | no rules (as run) | 98.0% | 81.6% | 89.0% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 98.0% | 81.8% | 89.2% | +0.2 pts | 0.0 pts | 5 | 0 |
| qwen3.6:27b | masked values | 98.0% | 81.6% | 89.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 98.0% | 82.0% | 89.3% | +0.4 pts | 0.0 pts | 10 | 0 |
| qwen3.6:27b | dates need birth wording | 97.7% | 87.9% | 92.6% | +6.4 pts | -0.3 pts | 146 | 5 |
| qwen3.6:27b | IPv6 addresses (adds) | 98.2% | 81.6% | 89.1% | 0.0 pts | +0.1 pts | 0 | -2 |
| qwen3.6:27b | all rules together | 97.9% | 88.7% | 93.0% | +7.1 pts | -0.2 pts | 161 | 3 |

## Formats (38 documents)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 99.2% | 91.7% | 95.3% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | masked values | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | dates need birth wording | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | all rules together | 99.2% | 91.7% | 95.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | no rules (as run) | 93.3% | 94.9% | 94.1% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 93.3% | 94.9% | 94.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | masked values | 93.3% | 94.9% | 94.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | generic terms | 93.3% | 94.9% | 94.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | dates need birth wording | 93.3% | 96.6% | 94.9% | +1.7 pts | 0.0 pts | 6 | 0 |
| gemma4:e4b | IPv6 addresses (adds) | 93.3% | 94.9% | 94.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | all rules together | 93.3% | 96.6% | 94.9% | +1.7 pts | 0.0 pts | 6 | 0 |
| gpt-oss | no rules (as run) | 98.9% | 98.0% | 98.4% |  |  |  |  |
| gpt-oss | placeholders in brackets | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | masked values | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | dates need birth wording | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | IPv6 addresses (adds) | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 98.9% | 98.0% | 98.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | no rules (as run) | 98.3% | 97.7% | 98.0% |  |  |  |  |
| phi4 | placeholders in brackets | 98.3% | 97.7% | 98.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | masked values | 98.3% | 97.7% | 98.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | generic terms | 98.3% | 97.7% | 98.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | dates need birth wording | 98.3% | 98.2% | 98.3% | +0.6 pts | 0.0 pts | 2 | 0 |
| phi4 | IPv6 addresses (adds) | 98.3% | 97.7% | 98.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 98.3% | 98.2% | 98.3% | +0.6 pts | 0.0 pts | 2 | 0 |
| qwen3.6:27b | no rules (as run) | 99.4% | 89.5% | 94.2% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 99.4% | 89.5% | 94.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | masked values | 99.4% | 89.5% | 94.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 99.4% | 89.5% | 94.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | dates need birth wording | 99.2% | 92.8% | 95.9% | +3.3 pts | -0.3 pts | 14 | 1 |
| qwen3.6:27b | IPv6 addresses (adds) | 99.4% | 89.5% | 94.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | all rules together | 99.2% | 92.8% | 95.9% | +3.3 pts | -0.3 pts | 14 | 1 |

## Both corpora

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 98.2% | 91.9% | 94.9% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 98.2% | 92.0% | 95.0% | +0.1 pts | 0.0 pts | 3 | 0 |
| gemma4:31b | masked values | 98.2% | 91.9% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 98.2% | 92.4% | 95.2% | +0.5 pts | 0.0 pts | 11 | 0 |
| gemma4:31b | dates need birth wording | 98.2% | 91.9% | 95.0% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 98.3% | 91.9% | 95.0% | 0.0 pts | +0.1 pts | 0 | -2 |
| gemma4:31b | all rules together | 98.3% | 92.5% | 95.3% | +0.6 pts | +0.1 pts | 15 | -2 |
| gemma4:e4b | no rules (as run) | 88.9% | 77.2% | 82.7% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 88.8% | 78.5% | 83.3% | +1.3 pts | -0.1 pts | 37 | 3 |
| gemma4:e4b | masked values | 88.9% | 77.3% | 82.7% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:e4b | generic terms | 88.9% | 83.0% | 85.9% | +5.8 pts | 0.0 pts | 157 | 0 |
| gemma4:e4b | dates need birth wording | 88.7% | 79.5% | 83.8% | +2.3 pts | -0.2 pts | 66 | 5 |
| gemma4:e4b | IPv6 addresses (adds) | 89.1% | 77.3% | 82.8% | 0.0 pts | +0.2 pts | 0 | -4 |
| gemma4:e4b | all rules together | 88.7% | 87.3% | 88.0% | +10.1 pts | -0.2 pts | 262 | 4 |
| gpt-oss | no rules (as run) | 95.1% | 89.0% | 92.0% |  |  |  |  |
| gpt-oss | placeholders in brackets | 94.9% | 91.1% | 93.0% | +2.1 pts | -0.1 pts | 49 | 3 |
| gpt-oss | masked values | 95.1% | 89.0% | 92.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 95.1% | 89.2% | 92.0% | +0.2 pts | 0.0 pts | 4 | 0 |
| gpt-oss | dates need birth wording | 95.1% | 89.3% | 92.1% | +0.3 pts | 0.0 pts | 8 | 0 |
| gpt-oss | IPv6 addresses (adds) | 95.1% | 89.0% | 92.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 94.9% | 91.6% | 93.2% | +2.6 pts | -0.1 pts | 60 | 3 |
| phi4 | no rules (as run) | 94.8% | 77.6% | 85.3% |  |  |  |  |
| phi4 | placeholders in brackets | 94.7% | 79.1% | 86.2% | +1.5 pts | -0.1 pts | 47 | 3 |
| phi4 | masked values | 94.8% | 80.2% | 86.9% | +2.6 pts | 0.0 pts | 79 | 0 |
| phi4 | generic terms | 94.8% | 80.5% | 87.1% | +2.9 pts | 0.0 pts | 88 | 0 |
| phi4 | dates need birth wording | 94.5% | 82.6% | 88.2% | +5.0 pts | -0.3 pts | 149 | 7 |
| phi4 | IPv6 addresses (adds) | 94.8% | 77.6% | 85.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 94.3% | 91.2% | 92.7% | +13.6 pts | -0.5 pts | 362 | 10 |
| qwen3.6:27b | no rules (as run) | 98.3% | 82.9% | 89.9% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 98.3% | 83.0% | 90.0% | +0.2 pts | 0.0 pts | 5 | 0 |
| qwen3.6:27b | masked values | 98.3% | 82.9% | 89.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 98.3% | 83.2% | 90.1% | +0.3 pts | 0.0 pts | 10 | 0 |
| qwen3.6:27b | dates need birth wording | 98.0% | 88.8% | 93.1% | +5.9 pts | -0.3 pts | 160 | 6 |
| qwen3.6:27b | IPv6 addresses (adds) | 98.4% | 82.9% | 90.0% | 0.0 pts | +0.1 pts | 0 | -2 |
| qwen3.6:27b | all rules together | 98.1% | 89.4% | 93.5% | +6.5 pts | -0.2 pts | 175 | 4 |

## Does the starting point match the saved results?

The first row of each model (no rules) is rescored here and must equal the saved result. Differences below would mean the simulation is wrong.

All match.

## What each rule removes, over all five models

| Rule | Redactions removed that were wrong | Redactions removed that were right (the cost) |
|---|---:|---:|
| a template placeholder (text in brackets, or a date template) | 153 | 9 |
| a masked or blank value | 84 | 4 |
| a generic term for a role in a contract | 305 | 2 |
| a date with no birth wording near it | 394 | 22 |

## Right redactions a rule would lose (examples to check)

| Rule | Category | Text | Times (all models) | Models | Example document |
|---|---|---|---:|---:|---|
| birth-date-context | DATE_OF_BIRTH | `1994-01-31` | 12 | 3 | finance-sample/heldout-071-currency-exchange-rate-sheet.txt |
| masked-value | LOCATION | `__________` | 4 | 1 | finance-sample/heldout-232-real-estate-loan-agreement.txt |
| bracketed-placeholder | SECRET | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| bracketed-placeholder | EMAIL | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| birth-date-context | DATE_OF_BIRTH | `December 24, 1977` | 3 | 3 | finance-sample/heldout-241-regulatory-filing.txt |
| bracketed-placeholder | CONTEXTUAL | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| generic-term | PERSON | `Consignee` | 2 | 2 | finance-sample/heldout-095-edi.txt |
| birth-date-context | DATE_OF_BIRTH | `202205` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| birth-date-context | DATE_OF_BIRTH | `20220520` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| birth-date-context | DATE_OF_BIRTH | `20220520123456` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| birth-date-context | DATE_OF_BIRTH | `2019` | 1 | 1 | text/10-contextual-profile.txt |
