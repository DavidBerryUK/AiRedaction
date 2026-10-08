# Slide Deck Outline: AI Document Redactor

Status: **draft for approval. Nothing is built yet.**

## Brief (from your answers)

- **Audience:** Answer Digital colleagues. Mixed technical level. They know LLMs and local models; Ollama is probably new.
- **Format:** read on their own first (shared in Slack, no live slot), so each slide must stand alone without speaker notes. No speaker notes, as you asked.
- **Tone:** confident, light, not deeply technical. Informal, so the template is used loosely.
- **One sentence to remember:** Answer can build AI projects using AI as a "worker", not just for automating workflow.
- **Calls to action:** ask me about it, download the project from GitHub (public), view the evaluation data.
- **Self-promotion angle:** building a project that uses local models, and building the prototype with Claude (5 days, stories and plans, no direct coding).
- **Business hook:** useful for the new client, Trade Remedies Authority (discovery phase).
- **Visuals:** screenshots only, from the running app, cropped and annotated. Synthetic documents only (all from Hugging Face). Hero before/after image.
- **Length:** 15 slides. That is long enough to tell the story and short enough to read in about 10 minutes.

## Slide-by-slide

| # | Slide | Content | Visual |
|---|-------|---------|--------|
| 1 | **Title** | "Redacting documents with local AI models: a 5-day prototype built with Claude." Dave Berry, Answer Digital. | Template title layout |
| 2 | **The one-liner** | AI as a worker, not just workflow automation. Three short points: a real working app, local models, built in 5 days. | Big text only |
| 3 | **The problem** | Sensitive documents need names, addresses, IDs and more removed. Manual redaction is slow and error-prone. Relevant to Trade Remedies Authority discovery. | Simple icon row |
| 4 | **Why local models** | Documents never leave the machine, no per-token cost, nothing sent to a cloud API, and it can be tested safely. A quick note that Ollama is how you run models locally. | Simple graphic |
| 5 | **What I built** | The redactor in one picture: the Explorer main screen, annotated. | Screenshot: Explorer main screen |
| 6 | **Hero: before and after** | One synthetic document, original beside redacted. | Screenshot pair |
| 7 | **How it finds things** | High level only: rules for the obvious patterns, an AI model driven by a prompt, a second opinion model (GLiNER), and custom terms. | Simple diagram or screenshot: Categories dialog |
| 8 | **Architecture** | One simple flow: document in, model called locally, results stored. | Simple 3-box diagram |
| 9 | **Prototype first** | Started with a small prototype for single categories, found realistic sample documents on Hugging Face, then grew it into an evaluation suite. | Timeline strip |
| 10 | **Measure everything** | Exported as much data as possible: stats first, then the actual output documents and recorded edits. That allows accurate comparisons and replays in the UI. Claude analysed each run and helped choose the next one. | Screenshot: Previous runs view |
| 11 | **The evaluation** | Started with 11 models, trimmed down, about 5 long runs. Numbers to confirm (see below). | Number tiles |
| 12 | **The results** | Which models can we recommend, based on the data. Recall, precision, missed, over-redactions and time per document, using the Explorer's wording. | Redrawn clean chart plus screenshot: models dialog |
| 13 | **Marking the results automatically** | Because the models are local and private, we can evaluate safely and mark results automatically against the answer key, then replay any run. | Screenshot: prompt dialog or redaction view |
| 14 | **How it was built** | 5 days, stories and plans, no direct coding, smaller stories. Claude built it, I steered. Honest note: it is not a complex app. | Simple process graphic |
| 15 | **Try it / ask me** | Link to the public GitHub repo, how to view the evaluation data, contact dave.berry@answerdigital.com. | Repo link and QR code |

If you want it shorter, slides 3, 9 and 13 are the easiest to merge or drop (giving 12).

## Screenshots to capture (running app, synthetic documents only)

1. Explorer main screen (slides 5, 12).
2. Redaction view showing past runs (slides 6, 10).
3. Original beside redacted for one chosen document (slide 6, hero).
4. Models dialog (slide 12).
5. Categories dialog (slide 7).
6. Prompt dialog (slide 13).

Each is cropped. Callouts are added on slides 5, 6, 10 and 12. I will check every screenshot for anything that looks like a real person's data before using it.

## Open items and assumptions

Your answers to questions 11 to 16 were blank, so this is what I will do unless you say otherwise:

- **Local models story:** I will use the reasons in slide 4 (privacy, cost, no cloud API). Please confirm.
- **Numbers on slides 11 and 12:** I will pull the model names, document counts, run counts, scores and times from the final datasets in the Explorer so they match exactly. I need to know if you want your hardware (GPU and RAM) shown. Say if so and give the spec.
- **Cloud comparison:** none, local only.
- **Surprises and lessons for first-time local users:** none on the slides unless you add one or two lines for me.
- **The answer key (Q28):** I read your answer as "local models keep it private, so we can evaluate safely and mark automatically". I do not have who made the answer key. If the answer key came from the Hugging Face dataset labels, I will say so on slide 13. Please confirm.
- **Template:** I still need to check I can reach your Google Slides copy. If I can not edit it directly, I will build the content and screenshots locally and give you the slides to paste in.
- **Palette:** the chart on slide 12 will use the template's colours.

## Next step

Tell me what to change, and approve. Then I will check access to the Google Slides copy, capture the screenshots, and build the deck.
