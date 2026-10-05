# What the clean-up rules would do

Worked out from the spans saved in `final-20261004` with no model run: each model's saved answers have each rule applied, and are scored against the answer keys again. Models: gemma4:31b, gemma4:e4b, gpt-oss, phi4, qwen3.6:27b.

**Read this first.** The rules were chosen after looking at the held-out and formats documents, so the gain shown for those is an upper estimate. The `external-*` corpora (and the combined `unseen` table) were added afterwards and were never looked at when the rules were written, so they are the fair test. The figure that matters most is the right redactions a rule would take away, because that is the risk.

## Held-out (300 documents; rules written after seeing these)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 98.0% | 91.7% | 94.8% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 98.0% | 91.9% | 94.8% | +0.2 pts | 0.0 pts | 3 | 0 |
| gemma4:31b | masked values | 98.0% | 91.7% | 94.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 98.0% | 92.3% | 95.1% | +0.6 pts | 0.0 pts | 11 | 0 |
| gemma4:31b | dates need birth wording | 98.0% | 91.8% | 94.8% | +0.1 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 98.1% | 91.7% | 94.8% | 0.0 pts | +0.1 pts | 0 | -2 |
| gemma4:31b | all rules together | 98.1% | 92.5% | 95.2% | +0.8 pts | +0.1 pts | 15 | -2 |
| gemma4:e4b | no rules (as run) | 88.0% | 74.2% | 80.5% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 87.8% | 75.6% | 81.2% | +1.4 pts | -0.2 pts | 37 | 3 |
| gemma4:e4b | masked values | 88.0% | 74.2% | 80.5% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:e4b | generic terms | 88.0% | 80.8% | 84.2% | +6.6 pts | 0.0 pts | 157 | 0 |
| gemma4:e4b | dates need birth wording | 87.7% | 76.5% | 81.7% | +2.3 pts | -0.3 pts | 60 | 5 |
| gemma4:e4b | IPv6 addresses (adds) | 88.2% | 74.2% | 80.6% | +0.1 pts | +0.2 pts | 0 | -4 |
| gemma4:e4b | all rules together | 87.7% | 85.5% | 86.6% | +11.3 pts | -0.2 pts | 256 | 4 |
| gpt-oss | no rules (as run) | 94.7% | 87.3% | 90.8% |  |  |  |  |
| gpt-oss | placeholders in brackets | 94.5% | 89.6% | 92.0% | +2.3 pts | -0.2 pts | 46 | 3 |
| gpt-oss | masked values | 94.7% | 87.3% | 90.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 94.7% | 87.9% | 91.2% | +0.6 pts | 0.0 pts | 12 | 0 |
| gpt-oss | dates need birth wording | 94.7% | 87.9% | 91.2% | +0.6 pts | 0.0 pts | 13 | 0 |
| gpt-oss | IPv6 addresses (adds) | 94.7% | 87.3% | 90.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 94.5% | 90.9% | 92.7% | +3.6 pts | -0.2 pts | 71 | 3 |
| phi4 | no rules (as run) | 94.1% | 74.3% | 83.0% |  |  |  |  |
| phi4 | placeholders in brackets | 93.9% | 76.0% | 84.0% | +1.7 pts | -0.2 pts | 47 | 3 |
| phi4 | masked values | 94.1% | 77.2% | 84.8% | +2.9 pts | 0.0 pts | 79 | 0 |
| phi4 | generic terms | 94.1% | 77.6% | 85.0% | +3.3 pts | 0.0 pts | 88 | 0 |
| phi4 | dates need birth wording | 93.7% | 79.9% | 86.2% | +5.6 pts | -0.4 pts | 147 | 7 |
| phi4 | IPv6 addresses (adds) | 94.1% | 74.3% | 83.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 93.5% | 89.8% | 91.6% | +15.5 pts | -0.6 pts | 360 | 10 |
| qwen3.6:27b | no rules (as run) | 98.0% | 81.7% | 89.1% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 98.0% | 81.9% | 89.2% | +0.2 pts | 0.0 pts | 5 | 0 |
| qwen3.6:27b | masked values | 98.0% | 81.7% | 89.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 98.0% | 82.1% | 89.4% | +0.4 pts | 0.0 pts | 10 | 0 |
| qwen3.6:27b | dates need birth wording | 97.7% | 88.1% | 92.7% | +6.4 pts | -0.3 pts | 146 | 5 |
| qwen3.6:27b | IPv6 addresses (adds) | 98.2% | 81.7% | 89.2% | 0.0 pts | +0.1 pts | 0 | -2 |
| qwen3.6:27b | all rules together | 97.9% | 88.9% | 93.1% | +7.2 pts | -0.2 pts | 161 | 3 |

## external-nemotron (232 documents; not seen when the rules were written)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 98.4% | 91.6% | 94.9% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 98.4% | 91.6% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | masked values | 98.4% | 91.6% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 98.4% | 91.7% | 94.9% | +0.1 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | dates need birth wording | 98.4% | 91.6% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 98.4% | 91.6% | 94.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | all rules together | 98.4% | 91.7% | 94.9% | +0.1 pts | 0.0 pts | 1 | 0 |
| gemma4:e4b | no rules (as run) | 88.6% | 84.6% | 86.5% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 88.6% | 84.6% | 86.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | masked values | 88.6% | 84.6% | 86.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | generic terms | 88.6% | 84.6% | 86.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | dates need birth wording | 88.5% | 89.5% | 89.0% | +4.9 pts | -0.1 pts | 60 | 1 |
| gemma4:e4b | IPv6 addresses (adds) | 88.6% | 84.6% | 86.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | all rules together | 88.5% | 89.5% | 89.0% | +4.9 pts | -0.1 pts | 60 | 1 |
| gpt-oss | no rules (as run) | 96.1% | 92.0% | 94.0% |  |  |  |  |
| gpt-oss | placeholders in brackets | 96.1% | 92.0% | 94.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | masked values | 96.1% | 92.0% | 94.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 96.1% | 92.1% | 94.1% | +0.1 pts | 0.0 pts | 1 | 0 |
| gpt-oss | dates need birth wording | 96.1% | 92.5% | 94.3% | +0.5 pts | 0.0 pts | 6 | 0 |
| gpt-oss | IPv6 addresses (adds) | 96.1% | 92.0% | 94.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 96.1% | 92.6% | 94.3% | +0.6 pts | 0.0 pts | 7 | 0 |
| phi4 | no rules (as run) | 94.8% | 82.5% | 88.2% |  |  |  |  |
| phi4 | placeholders in brackets | 94.8% | 82.5% | 88.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | masked values | 94.8% | 82.5% | 88.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | generic terms | 94.8% | 82.5% | 88.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | dates need birth wording | 94.7% | 92.0% | 93.3% | +9.5 pts | -0.1 pts | 125 | 1 |
| phi4 | IPv6 addresses (adds) | 94.8% | 82.5% | 88.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 94.7% | 92.0% | 93.3% | +9.5 pts | -0.1 pts | 125 | 1 |
| qwen3.6:27b | no rules (as run) | 98.8% | 82.4% | 89.8% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 98.8% | 82.4% | 89.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | masked values | 98.8% | 82.4% | 89.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 98.8% | 82.4% | 89.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | dates need birth wording | 98.7% | 89.1% | 93.7% | +6.7 pts | -0.1 pts | 96 | 1 |
| qwen3.6:27b | IPv6 addresses (adds) | 98.8% | 82.4% | 89.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | all rules together | 98.7% | 89.1% | 93.7% | +6.7 pts | -0.1 pts | 96 | 1 |

## external-gretel (180 documents; not seen when the rules were written)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 95.6% | 70.4% | 81.1% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 95.6% | 70.4% | 81.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | masked values | 95.6% | 70.4% | 81.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 95.6% | 71.2% | 81.6% | +0.9 pts | 0.0 pts | 13 | 0 |
| gemma4:31b | dates need birth wording | 95.6% | 70.4% | 81.1% | +0.1 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 95.6% | 70.4% | 81.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | all rules together | 95.6% | 71.3% | 81.7% | +0.9 pts | 0.0 pts | 14 | 0 |
| gemma4:e4b | no rules (as run) | 92.3% | 57.9% | 71.2% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 91.9% | 59.3% | 72.1% | +1.3 pts | -0.4 pts | 30 | 3 |
| gemma4:e4b | masked values | 92.3% | 57.9% | 71.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | generic terms | 92.1% | 62.6% | 74.6% | +4.7 pts | -0.1 pts | 94 | 1 |
| gemma4:e4b | dates need birth wording | 92.3% | 59.9% | 72.6% | +1.9 pts | 0.0 pts | 40 | 0 |
| gemma4:e4b | IPv6 addresses (adds) | 92.3% | 57.9% | 71.2% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | all rules together | 91.8% | 66.6% | 77.2% | +8.7 pts | -0.5 pts | 165 | 4 |
| gpt-oss | no rules (as run) | 90.9% | 68.4% | 78.1% |  |  |  |  |
| gpt-oss | placeholders in brackets | 90.7% | 69.1% | 78.5% | +0.7 pts | -0.1 pts | 11 | 1 |
| gpt-oss | masked values | 90.9% | 68.4% | 78.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 90.9% | 68.4% | 78.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | dates need birth wording | 90.9% | 69.0% | 78.4% | +0.5 pts | 0.0 pts | 8 | 0 |
| gpt-oss | IPv6 addresses (adds) | 90.9% | 68.4% | 78.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 90.7% | 69.7% | 78.8% | +1.2 pts | -0.1 pts | 19 | 1 |
| phi4 | no rules (as run) | 92.5% | 59.3% | 72.3% |  |  |  |  |
| phi4 | placeholders in brackets | 91.6% | 60.7% | 73.0% | +1.4 pts | -0.9 pts | 33 | 7 |
| phi4 | masked values | 92.5% | 59.3% | 72.3% | 0.0 pts | 0.0 pts | 1 | 0 |
| phi4 | generic terms | 92.5% | 61.6% | 74.0% | +2.3 pts | 0.0 pts | 46 | 0 |
| phi4 | dates need birth wording | 92.5% | 64.3% | 75.9% | +5.0 pts | 0.0 pts | 96 | 0 |
| phi4 | IPv6 addresses (adds) | 92.5% | 59.3% | 72.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 91.6% | 68.9% | 78.7% | +9.6 pts | -0.9 pts | 174 | 7 |
| qwen3.6:27b | no rules (as run) | 96.1% | 60.0% | 73.9% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 96.1% | 59.9% | 73.8% | 0.0 pts | 0.0 pts | -1 | 0 |
| qwen3.6:27b | masked values | 96.1% | 60.0% | 73.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 96.1% | 60.5% | 74.3% | +0.5 pts | 0.0 pts | 11 | 0 |
| qwen3.6:27b | dates need birth wording | 96.1% | 63.7% | 76.6% | +3.7 pts | 0.0 pts | 72 | 0 |
| qwen3.6:27b | IPv6 addresses (adds) | 96.1% | 60.0% | 73.9% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | all rules together | 96.1% | 64.2% | 77.0% | +4.3 pts | 0.0 pts | 82 | 0 |

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
| gpt-oss | no rules (as run) | 96.9% | 96.5% | 96.7% |  |  |  |  |
| gpt-oss | placeholders in brackets | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | masked values | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | dates need birth wording | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | IPv6 addresses (adds) | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 96.9% | 96.5% | 96.7% | 0.0 pts | 0.0 pts | 0 | 0 |
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

## Unseen documents only (the fair test; 412 documents)

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 97.3% | 81.3% | 88.6% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 97.3% | 81.3% | 88.6% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | masked values | 97.3% | 81.3% | 88.6% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 97.3% | 81.8% | 88.9% | +0.5 pts | 0.0 pts | 14 | 0 |
| gemma4:31b | dates need birth wording | 97.3% | 81.3% | 88.6% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 97.3% | 81.3% | 88.6% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | all rules together | 97.3% | 81.8% | 88.9% | +0.6 pts | 0.0 pts | 15 | 0 |
| gemma4:e4b | no rules (as run) | 90.1% | 70.4% | 79.0% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 89.9% | 71.3% | 79.5% | +0.9 pts | -0.2 pts | 30 | 3 |
| gemma4:e4b | masked values | 90.1% | 70.4% | 79.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | generic terms | 90.0% | 73.3% | 80.8% | +2.9 pts | -0.1 pts | 94 | 1 |
| gemma4:e4b | dates need birth wording | 90.0% | 73.5% | 80.9% | +3.1 pts | -0.1 pts | 100 | 1 |
| gemma4:e4b | IPv6 addresses (adds) | 90.1% | 70.4% | 79.0% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:e4b | all rules together | 89.8% | 77.8% | 83.4% | +7.4 pts | -0.3 pts | 225 | 5 |
| gpt-oss | no rules (as run) | 94.0% | 80.6% | 86.8% |  |  |  |  |
| gpt-oss | placeholders in brackets | 93.9% | 81.0% | 87.0% | +0.4 pts | -0.1 pts | 11 | 1 |
| gpt-oss | masked values | 94.0% | 80.6% | 86.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 94.0% | 80.6% | 86.8% | 0.0 pts | 0.0 pts | 1 | 0 |
| gpt-oss | dates need birth wording | 94.0% | 81.1% | 87.1% | +0.5 pts | 0.0 pts | 14 | 0 |
| gpt-oss | IPv6 addresses (adds) | 94.0% | 80.6% | 86.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 93.9% | 81.6% | 87.3% | +1.0 pts | -0.1 pts | 26 | 1 |
| phi4 | no rules (as run) | 93.9% | 70.9% | 80.8% |  |  |  |  |
| phi4 | placeholders in brackets | 93.5% | 71.8% | 81.2% | +0.9 pts | -0.4 pts | 33 | 7 |
| phi4 | masked values | 93.9% | 70.9% | 80.8% | 0.0 pts | 0.0 pts | 1 | 0 |
| phi4 | generic terms | 93.9% | 72.2% | 81.6% | +1.4 pts | 0.0 pts | 46 | 0 |
| phi4 | dates need birth wording | 93.8% | 77.9% | 85.1% | +7.1 pts | -0.1 pts | 221 | 1 |
| phi4 | IPv6 addresses (adds) | 93.9% | 70.9% | 80.8% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 93.5% | 80.7% | 86.6% | +9.8 pts | -0.4 pts | 299 | 8 |
| qwen3.6:27b | no rules (as run) | 97.7% | 71.3% | 82.5% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 97.7% | 71.3% | 82.5% | 0.0 pts | 0.0 pts | -1 | 0 |
| qwen3.6:27b | masked values | 97.7% | 71.3% | 82.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 97.7% | 71.7% | 82.7% | +0.3 pts | 0.0 pts | 11 | 0 |
| qwen3.6:27b | dates need birth wording | 97.7% | 76.5% | 85.8% | +5.1 pts | -0.1 pts | 168 | 1 |
| qwen3.6:27b | IPv6 addresses (adds) | 97.7% | 71.3% | 82.5% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | all rules together | 97.7% | 76.8% | 86.0% | +5.4 pts | -0.1 pts | 178 | 1 |

## All corpora

| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| gemma4:31b | no rules (as run) | 97.7% | 86.5% | 91.7% |  |  |  |  |
| gemma4:31b | placeholders in brackets | 97.7% | 86.5% | 91.8% | +0.1 pts | 0.0 pts | 3 | 0 |
| gemma4:31b | masked values | 97.7% | 86.5% | 91.7% | 0.0 pts | 0.0 pts | 0 | 0 |
| gemma4:31b | generic terms | 97.7% | 86.9% | 92.0% | +0.5 pts | 0.0 pts | 25 | 0 |
| gemma4:31b | dates need birth wording | 97.7% | 86.5% | 91.8% | 0.0 pts | 0.0 pts | 2 | 0 |
| gemma4:31b | IPv6 addresses (adds) | 97.8% | 86.5% | 91.8% | 0.0 pts | +0.1 pts | 0 | -2 |
| gemma4:31b | all rules together | 97.8% | 87.1% | 92.1% | +0.6 pts | +0.1 pts | 30 | -2 |
| gemma4:e4b | no rules (as run) | 89.5% | 73.7% | 80.9% |  |  |  |  |
| gemma4:e4b | placeholders in brackets | 89.3% | 74.8% | 81.4% | +1.1 pts | -0.2 pts | 67 | 6 |
| gemma4:e4b | masked values | 89.5% | 73.8% | 80.9% | 0.0 pts | 0.0 pts | 1 | 0 |
| gemma4:e4b | generic terms | 89.5% | 78.0% | 83.3% | +4.3 pts | 0.0 pts | 251 | 1 |
| gemma4:e4b | dates need birth wording | 89.3% | 76.5% | 82.4% | +2.7 pts | -0.2 pts | 166 | 6 |
| gemma4:e4b | IPv6 addresses (adds) | 89.6% | 73.8% | 80.9% | 0.0 pts | +0.1 pts | 0 | -4 |
| gemma4:e4b | all rules together | 89.3% | 82.4% | 85.7% | +8.7 pts | -0.2 pts | 487 | 9 |
| gpt-oss | no rules (as run) | 94.6% | 84.7% | 89.4% |  |  |  |  |
| gpt-oss | placeholders in brackets | 94.5% | 85.8% | 89.9% | +1.1 pts | -0.1 pts | 57 | 4 |
| gpt-oss | masked values | 94.6% | 84.7% | 89.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | generic terms | 94.6% | 85.0% | 89.5% | +0.3 pts | 0.0 pts | 13 | 0 |
| gpt-oss | dates need birth wording | 94.6% | 85.2% | 89.6% | +0.5 pts | 0.0 pts | 27 | 0 |
| gpt-oss | IPv6 addresses (adds) | 94.6% | 84.7% | 89.4% | 0.0 pts | 0.0 pts | 0 | 0 |
| gpt-oss | all rules together | 94.5% | 86.6% | 90.4% | +1.9 pts | -0.1 pts | 97 | 4 |
| phi4 | no rules (as run) | 94.4% | 74.2% | 83.1% |  |  |  |  |
| phi4 | placeholders in brackets | 94.1% | 75.4% | 83.7% | +1.2 pts | -0.3 pts | 80 | 10 |
| phi4 | masked values | 94.4% | 75.5% | 83.9% | +1.2 pts | 0.0 pts | 80 | 0 |
| phi4 | generic terms | 94.4% | 76.3% | 84.4% | +2.1 pts | 0.0 pts | 134 | 0 |
| phi4 | dates need birth wording | 94.2% | 80.3% | 86.7% | +6.1 pts | -0.2 pts | 370 | 8 |
| phi4 | IPv6 addresses (adds) | 94.4% | 74.2% | 83.1% | 0.0 pts | 0.0 pts | 0 | 0 |
| phi4 | all rules together | 93.9% | 85.8% | 89.7% | +11.6 pts | -0.5 pts | 661 | 18 |
| qwen3.6:27b | no rules (as run) | 98.0% | 77.0% | 86.3% |  |  |  |  |
| qwen3.6:27b | placeholders in brackets | 98.0% | 77.1% | 86.3% | +0.1 pts | 0.0 pts | 4 | 0 |
| qwen3.6:27b | masked values | 98.0% | 77.0% | 86.3% | 0.0 pts | 0.0 pts | 0 | 0 |
| qwen3.6:27b | generic terms | 98.0% | 77.3% | 86.5% | +0.3 pts | 0.0 pts | 21 | 0 |
| qwen3.6:27b | dates need birth wording | 97.8% | 82.5% | 89.5% | +5.5 pts | -0.2 pts | 328 | 7 |
| qwen3.6:27b | IPv6 addresses (adds) | 98.1% | 77.0% | 86.3% | 0.0 pts | +0.1 pts | 0 | -2 |
| qwen3.6:27b | all rules together | 97.9% | 83.0% | 89.8% | +6.0 pts | -0.1 pts | 353 | 5 |

## Does the starting point match the saved results?

The first row of each model (no rules) is rescored here and must equal the saved result. Differences below would mean the simulation is wrong.

All match.

## What each rule removes, over all five models

| Rule | Redactions removed that were wrong | Redactions removed that were right (the cost) |
|---|---:|---:|
| a template placeholder (text in brackets, or a date template) | 232 | 22 |
| a masked or blank value | 85 | 5 |
| a generic term for a role in a contract | 510 | 5 |
| a date with no birth wording near it | 922 | 26 |

## Right redactions a rule would lose (examples to check)

| Rule | Category | Text | Times (all models) | Models | Example document |
|---|---|---|---:|---:|---|
| birth-date-context | DATE_OF_BIRTH | `1994-01-31` | 12 | 3 | finance-sample/heldout-071-currency-exchange-rate-sheet.txt |
| masked-value | LOCATION | `__________` | 4 | 1 | finance-sample/heldout-232-real-estate-loan-agreement.txt |
| bracketed-placeholder | COMPANY | `[Company Name]` | 3 | 3 | external-gretel/ext-gretel-048-customer-agreement.txt |
| birth-date-context | DATE_OF_BIRTH | `December 24, 1977` | 3 | 3 | finance-sample/heldout-241-regulatory-filing.txt |
| bracketed-placeholder | CONTEXTUAL | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| generic-term | PERSON | `Consignee` | 3 | 3 | finance-sample/heldout-095-edi.txt |
| bracketed-placeholder | SECRET | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| birth-date-context | DATE_OF_BIRTH | `1966-10-15` | 3 | 3 | external-nemotron/ext-nemotron-065-disability-disability-insurance-form.txt |
| bracketed-placeholder | CONTEXTUAL | `[Company Contact Information]` | 3 | 1 | external-gretel/ext-gretel-135-privacy-policy.txt |
| bracketed-placeholder | COMPANY | `[Company Contact Information]` | 3 | 1 | external-gretel/ext-gretel-135-privacy-policy.txt |
| bracketed-placeholder | EMAIL | `[contact information]` | 3 | 1 | finance-sample/heldout-225-privacy-policy.txt |
| birth-date-context | DATE_OF_BIRTH | `202205` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| birth-date-context | DATE_OF_BIRTH | `20220520` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| birth-date-context | DATE_OF_BIRTH | `20220520123456` | 2 | 1 | finance-sample/heldout-205-mt940.txt |
| bracketed-placeholder | DATE_OF_BIRTH | `MM/DD/YYYY` | 2 | 1 | external-gretel/ext-gretel-068-financial-aid-application.txt |
| bracketed-placeholder | ADDRESS | `[Your Company Address]` | 2 | 2 | external-gretel/ext-gretel-095-isda-definition.txt |
| birth-date-context | DATE_OF_BIRTH | `2019` | 1 | 1 | text/10-contextual-profile.txt |
| generic-term | COMPANY | `Company` | 1 | 1 | external-gretel/ext-gretel-095-isda-definition.txt |
| birth-date-context | DATE_OF_BIRTH | `1954` | 1 | 1 | external-nemotron/ext-nemotron-174-product-customer-review-form.txt |
| masked-value | COMPANY | `_______________` | 1 | 1 | external-gretel/ext-gretel-123-mortgage-contract.txt |
| generic-term | PERSON | `Company` | 1 | 1 | external-gretel/ext-gretel-056-edi.txt |
