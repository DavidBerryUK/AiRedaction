# Slide Deck Questions

Answer under each question (short answers are fine). Skip anything that does not apply and a sensible default will be used and noted.

Template: https://docs.google.com/presentation/d/1Mt1PE1xm-QlML0My8dSW9L8oCu2f240WVdD3Hef0Oj8/edit

Goals: (a) self promotion, (b) showcase use of local models, (c) show the use of a quick prototype to gather results.

---

## 1. Audience and setting

1. Who is the audience? (Answer Digital colleagues, clients, meetup or conference, leadership, recruiters)

   **Answer:**
   Answer Digital Colleagues

2. Live talk or a deck people read on their own? If live, how long is the slot and is there Q&A?

   **Answer:**

   Read on their own initially,

3. What is the audience's technical level? Do they know what an LLM, Ollama or a "local model" is?

   **Answer:**

   Mixed, will have awareness of LLM and Local models, Ollama may be new to most people

4. Is there a date or event that sets the deadline?

   **Answer:**

   No deadline, I will share in slack as soon as I'm happy with it, hopefully before end of week

5. Follow the company template's look and tone strictly, or is there room to be playful?

   **Answer:**

   This is an informal presentation, just used slides as we should, but nothing strict

---

## 2. Purpose and message

6. If people remember one sentence, what should it be?

   **Answer:**
   Answer can build AI projects using AI as a 'worker', not just for automating workflow

7. What do you want them to do afterwards? (ask you about it, try local models, use you on a project, look at the repo)

   **Answer:**
   ask me about it, download the project from github, view the evaluation data

8. What is the self-promotion aimed at? (your AI skills, .NET/Blazor, delivery speed, your way of working with Claude)

   **Answer:**
   building project that usings local models, and building a prototype using claude

9. Is the repo public or private? Can the deck link to it and name Answer Digital?

   **Answer:**
   public, can definitely link to it

10. Is there a client or business use case behind this (for example NHS data, where redaction really matters), or is it a pure exploration?

    **Answer:**
    It will be helpful to new client 'Trade Remedies Authority' which has just entered the discovery phase

---

## 3. The story: local models

11. Why local models? (privacy, cost, offline, sensitive documents that cannot go to a cloud API)

    **Answer:**

12. Which models did you test, and on what hardware (GPU, RAM)?

    **Answer:**

13. What is your headline finding? Which model won, and how close did local get to what you would expect from a cloud model?

    **Answer:**

14. Did anything surprise you? (a small model beating a big one, timeouts, odd failure modes)

    **Answer:**

15. What would you tell someone who has never run a local model? (setup effort, gotchas, speed)

    **Answer:**

16. Do you want a cloud comparison, or should the deck stay local-only?

    **Answer:**

---

## 4. The story: quick prototype and gathering results

17. How long did the prototype take, and how much was built with Claude Code? Do you want to say so openly?

    **Answer:**

    5 days, using stories and plans, no direct coding

18. The Eval project ran hundreds of evaluations. Roughly how many runs, documents and models? Is there a figure you are proud of?

    **Answer:**

    started with 11 llm models but trimmed down, about 5 long runs, Claude evaluated results and helped shape the next run and model usage

19. Do you want to tell the "prototype, then measure" story: building the Eval harness and the Explorer and letting the data pick the winners?

    **Answer:**

    exported as much data as possible, first starting off with stats, then moving onto actual output documents and recording edits, allows for accurate comparisons and replayes in the UI application

20. Which lessons should the deck show? (agree the design first, small stories, tests, restorable datasets)

    **Answer:**

    smaller stories, though its not a complex app

21. Any false starts or things that did not work? Audiences usually like these.

    **Answer:**

    Just started with small prototype to test single categories, then got appropriate sample documents from hugging face, then build up into an evaluation suite

---

## 5. What the tool does

22. Live demo, recorded clip, or screenshots only?

    **Answer:**
    screen shots only

23. Which features are the stars? (redacted view, Edits list, PDF page view, Previous runs buttons, Chart dialog, Categories dialog)

    **Answer:**
    redaction view showing past runs, the models dialog, the categories dialog, the prompt dialog, the 'explorer' main screen

24. How much of "how it finds things" should be explained? (rules, the AI prompt, GLiNER second opinion, custom terms)

    **Answer:**
    explain at a high level

25. Do you want an architecture slide, and how detailed?

    **Answer:**
    simple slide only , e.g. calls model, reads document from, stores results here

---

## 6. Metrics and terminology

26. Which numbers go on the slides? Suggested: recall, precision, missed, over-redactions, time per document (same wording as the Explorer).

    **Answer:**
    all please

27. Should the main chart be the per-model comparison from the Chart dialog, redrawn cleanly for the slide?

    **Answer:**
    accuracy - e.g. which models can we recommend to use based on the results found

28. What is the "answer key" and who made it? Audiences will ask how you know the results are right.

    **Answer:**
    We can use local models, keep private, we can evaluate safely and auto mark the results

---

## 7. Screenshots and data safety

29. Are the documents in `in/` synthetic or made up? If any are real, they must not be shown.

    **Answer:**
    all download from hugging face, no real documents

30. Should screenshots be captured from the running app (including dark mode if wanted)?

    **Answer:**
    running app

31. Should screenshots be cropped and annotated with callouts?

    **Answer:**
    both

32. Do you want a before and after of one document (original beside redacted) as a hero image?

    **Answer:**
    yes

---

## 8. Structure and format

33. Roughly how many slides? (8, 12, 20)

    **Answer:**

    12-20 ish, what makes a good deck

34. Build directly in the Google Slides copy, or draft the content in a document first and paste it in?

    **Answer:**

    build directly in google slides

35. Speaker notes on each slide?

    **Answer:**

    no

36. Closing slide with contact details, repo link or a QR code?

    **Answer:**

    dave.berry@answerdigital.com

---

## 9. Tone

37. Which tone? (confident and technical, light and story-led, a mix)

    **Answer:**

    confidant, light, not deeply technical

38. Anything to avoid? (naming competitors, accuracy claims you cannot back up)

    **Answer:**
    nothing

---

## Anything else

**Notes:**
