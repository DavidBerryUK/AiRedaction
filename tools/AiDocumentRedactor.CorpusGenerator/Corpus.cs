namespace CorpusGenerator;

/// <summary>All data below is invented. Emails use example.* domains, phones use Ofcom drama ranges,
/// IPs use documentation ranges, keys/cards/IBANs are published example values.</summary>
public static class Corpus
{
    /// <summary>A heading block.</summary>
    static Block H(string t) => new(BlockKind.Heading, t);
    /// <summary>A paragraph block (\n gives a line break).</summary>
    static Block P(string t) => new(BlockKind.Para, t);
    /// <summary>A table block from rows of cells.</summary>
    static Block T(params string[][] rows) => new(BlockKind.Table, string.Empty, rows);

    /// <summary>The 12 test documents. Each is written once, with the formats it should be rendered to.</summary>
    public static List<DocDef> All() =>
    [
        new("01-hr-letter", "HR letter", ["txt", "docx", "pdf", "scan"],
        [
            H("[[COMPANY|Brightwater Analytics Ltd]]"),
            P("[[ADDRESS|Unit 4, Kingfisher House, 22 Wharf Road, Leeds LS1 4ZZ]]"),
            P("Tel: [[PHONE|0113 496 0123]]  |  [[EMAIL|people@brightwater-analytics.example]]"),
            P("14 October 2025"),
            P("[[GENDER|Ms]] [[PERSON|Eleanor Whitcombe]]\n[[ADDRESS|14 Marlowe Court, Headingley, Leeds LS6 2QT]]"),
            P("Dear [[GENDER|Ms]] [[PERSON|Whitcombe]],"),
            H("Confirmation of your promotion"),
            P("Further to our meeting last week, I am delighted to confirm your promotion to Senior Data Scientist with effect from 1 November. Our records show you were born on [[DATE_OF_BIRTH|12 March 1986]], so you are [[AGE|39]] years old, and your National Insurance number is [[ID_NUMBER|QQ 12 34 56 C]]."),
            P("Your new salary will be paid into the account ending [[ID_NUMBER|4821]]. If any of these details are wrong, please call me on [[PHONE|07700 900123]] or write to [[EMAIL|marcus.delaney@brightwater-analytics.example]]."),
            P("Yours sincerely,"),
            P("[[PERSON|Marcus Delaney]]\nHead of People, [[COMPANY|Brightwater Analytics]]"),
        ]) { ScanKinds = ["clean", "degraded", "pdf"] },

        new("02-services-agreement", "Services agreement", ["docx", "pdf"],
        [
            H("Services Agreement"),
            P("This agreement is made between [[COMPANY|Corvid Logistics PLC]] (company number [[COMPANY_ID|09876543]], VAT number [[COMPANY_ID|GB 123 4567 89]]) of [[ADDRESS|Harbourside Works, 7 Quay Lane, Bristol BS1 5TT]] (the Client) and [[COMPANY|Brightwater Analytics Ltd]] (the Supplier)."),
            T(["Item", "Detail"],
              ["Client contact", "[[PERSON|Priya Natarajan]], [[EMAIL|priya.natarajan@corvid-logistics.example]]"],
              ["Supplier contact", "[[PERSON|Marcus Delaney]], [[PHONE|020 7946 0958]]"],
              ["Payment to", "IBAN [[ID_NUMBER|GB82 WEST 1234 5698 7654 32]]"],
              ["Client website", "[[DOMAIN|www.corvid-logistics.example]]"]),
            P("The Supplier will deliver the services described in Schedule 1. Fees are payable within thirty days of invoice."),
        ]) { Header = "[[COMPANY|Corvid Logistics PLC]] - Confidential", Footer = "Prepared by [[PERSON|Marcus Delaney]] - [[EMAIL|legal@brightwater-analytics.example]]" },

        new("03-customer-email", "Customer email thread", ["txt", "md"],
        [
            P("From: [[PERSON|Tomasz Kowalczyk]] <[[EMAIL|tomasz.kowalczyk@example.org]]>\nTo: Support\nSubject: Order 55821 not delivered"),
            P("Hi, I'm [[PERSON|Tomasz Kowalczyk]]. My parcel was meant to arrive at [[ADDRESS|3 Orchard Close, Norwich NR2 3AB]] on Tuesday. I'm 61 and housebound, so I really can't collect it. You can reach me on [[PHONE|07700 900456]]. My account password is [[SECRET|Winter-Harbour-42!]] if that helps you find it."),
            P("Reply from [[PERSON|Aisha Rahman]], [[COMPANY|Fernleigh Parcels Ltd]]: [[GENDER|Mr]] [[PERSON|Kowalczyk]], sorry for the delay. We will redeliver tomorrow."),
        ]),

        new("04-meeting-notes", "Meeting notes", ["md", "docx"],
        [
            H("Project [[CONTEXTUAL|Kestrel]] - steering meeting"),
            P("Attendees: [[PERSON|Priya Natarajan]] ([[COMPANY|Corvid Logistics PLC]]), [[PERSON|Marcus Delaney]], [[PERSON|Jonas Eriksen]] ([[COMPANY|Nordlys Consulting AS]])."),
            P("[[PERSON|Jonas]] said the new depot manager, a [[GENDER|woman]] [[AGE|in her forties]], would start in January. Actions: [[PERSON|Priya]] to send the draft to [[EMAIL|jonas.eriksen@nordlys.example]]. Next meeting at the [[COMPANY|Nordlys]] office."),
        ]),

        new("05-incident-report", "Incident report with credentials", ["txt"],
        [
            H("Incident 2025-117: exposed credentials"),
            P("Reported by [[PERSON|Dev Patel]] on behalf of [[COMPANY|Brightwater Analytics Ltd]]. An engineer pasted the following into a public chat: the database connection string was [[SECRET|Server=db01.internal;User Id=svc_reports;Password=Tr1cky-Gl4cier!]] and the cloud key was [[SECRET|AKIAIOSFODNN7EXAMPLE]]."),
            P("The request came from [[ONLINE_ID|203.0.113.45]] using the handle [[ONLINE_ID|@dpatel_dev]]. The bearer token [[SECRET|eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJkZW1vIn0.c2lnbmF0dXJl]] has been revoked. Call [[PERSON|Dev Patel]] on [[PHONE|01632 960001]] with questions."),
        ]),

        new("06-customer-list", "Customer list (CSV)", ["csv"], [])
        {
            RawText = "name,email,phone,date_of_birth,company\n" +
                "[[PERSON|Eleanor Whitcombe]],[[EMAIL|e.whitcombe@example.net]],[[PHONE|07700 900111]],[[DATE_OF_BIRTH|1986-03-12]],[[COMPANY|Brightwater Analytics Ltd]]\n" +
                "[[PERSON|Tomasz Kowalczyk]],[[EMAIL|t.kowalczyk@example.org]],[[PHONE|07700 900222]],[[DATE_OF_BIRTH|1964-07-30]],[[COMPANY|Fernleigh Parcels Ltd]]\n" +
                "[[PERSON|Aisha Rahman]],[[EMAIL|a.rahman@example.com]],[[PHONE|07700 900333]],[[DATE_OF_BIRTH|1991-11-02]],[[COMPANY|Fernleigh Parcels Ltd]]\n" +
                "[[PERSON|Jonas Eriksen]],[[EMAIL|j.eriksen@nordlys.example]],[[PHONE|07700 900444]],[[DATE_OF_BIRTH|1978-01-19]],[[COMPANY|Nordlys Consulting AS]]\n",
        },

        new("07-app-config", "Application config (JSON)", ["json"], [])
        {
            RawText = "{\n  \"service\": \"reporting\",\n  \"owner\": \"[[EMAIL|ops@brightwater-analytics.example]]\",\n" +
                "  \"database\": \"Server=db01.internal;Database=reports;User Id=svc_reports;Password=[[SECRET|Tr1cky-Gl4cier!]]\",\n" +
                "  \"apiKey\": \"[[SECRET|sk-test-4f9a1c7e2b8d4a6f9c3e5b7a1d2c4e6f]]\",\n" +
                "  \"supportContact\": \"[[PERSON|Dev Patel]]\",\n  \"retries\": 3\n}\n",
        },

        new("08-invoice", "Invoice", ["pdf", "scan"],
        [
            H("INVOICE 2025-0831"),
            P("[[COMPANY|Fernleigh Parcels Ltd]]\n[[ADDRESS|Unit 9, Mill Lane Trading Estate, Norwich NR6 6XY]]\nVAT: [[COMPANY_ID|GB 987 6543 21]]"),
            P("Bill to: [[COMPANY|Corvid Logistics PLC]], attn [[PERSON|Priya Natarajan]], [[ADDRESS|Harbourside Works, 7 Quay Lane, Bristol BS1 5TT]]"),
            T(["Description", "Qty", "Amount"], ["Courier services, September", "1", "GBP 1,250.00"], ["Fuel surcharge", "1", "GBP 85.40"]),
            P("Pay by bank transfer to IBAN [[ID_NUMBER|GB82 WEST 1234 5698 7654 32]]. Queries: [[EMAIL|accounts@fernleigh-parcels.example]], [[PHONE|01603 740555]]."),
        ]) { ScanKinds = ["clean", "degraded", "pdf"] },

        new("09-gp-referral", "Referral letter", ["pdf", "docx", "scan"],
        [
            H("Referral to Cardiology"),
            P("Patient: [[PERSON|Tomasz Kowalczyk]], [[GENDER|male]], [[AGE|61]], born [[DATE_OF_BIRTH|30 July 1964]]\nNHS number: [[ID_NUMBER|943 476 5919]]\n[[ADDRESS|3 Orchard Close, Norwich NR2 3AB]]  Tel [[PHONE|07700 900456]]"),
            P("Dear Colleague, thank you for seeing this [[AGE|61]]-year-old [[GENDER|man]] who reports exertional chest tightness. [[GENDER|He]] works as a bus driver for [[COMPANY|Eastway Coaches Ltd]]. Please contact [[PERSON|Dr Helen Okafor]] at [[COMPANY|Fernleigh Surgery]] with your findings."),
        ]) { ScanKinds = ["clean", "degraded"] },

        new("10-contextual-profile", "Profile with indirect identifiers", ["txt"],
        [
            P("Our regional director is the only [[GENDER|female]] partner at the Bristol office of [[COMPANY|Corvid Logistics PLC]]. [[GENDER|She]] joined after the [[CONTEXTUAL|2019 data breach at the Bristol depot]] and earns [[CONTEXTUAL|GBP 92,000]]. Before that [[GENDER|she]] was [[CONTEXTUAL|head of compliance at Harbourside Credit Union]]."),
            P("[[GENDER|Her]] predecessor, [[PERSON|Gareth Lloyd]], now teaches at [[CONTEXTUAL|Merriweather Grammar School]]."),
        ]),

        new("11-hard-negatives", "Document with nothing to redact", ["txt", "docx"],
        [
            H("Quarterly style guide"),
            P("Please mark the boxes clearly and save the file as a Microsoft Word document. Submit returns to HMRC by the deadline. The server room in London is cold in winter. Prices rose 4% in March and the will of the committee was clear. See page 12 for 42 worked examples."),
            P("Use the term revenue, not income, and keep paragraphs short."),
        ]) { MustPreserve = ["Microsoft Word", "HMRC", "London", "mark", "42"] },

        new("12-hygiene-docx", "Word document with hidden content", ["docx"],
        [
            H("Draft termination notice"),
            P("This letter confirms that [[PERSON|Gareth Lloyd]] will leave [[COMPANY|Corvid Logistics PLC]] on 31 December. Contact [[EMAIL|hr@corvid-logistics.example]] for details."),
            P("Please return company property to [[ADDRESS|Harbourside Works, 7 Quay Lane, Bristol BS1 5TT]]."),
        ])
        {
            Header = "Strictly private - [[COMPANY|Corvid Logistics PLC]]",
            Footer = "Ref [[PERSON|Priya Natarajan]]",
            Comment = "Check with [[PERSON|Dev Patel]] before sending - [[GENDER|his]] number is [[PHONE|07700 900999]]",
            TrackedDeletion = "[[SECRET|Tr1cky-Gl4cier!]] ",
            MetadataAuthor = "[[PERSON|Priya Natarajan]]",
            SplitRuns = true,
        },

        // ---- Context tests: the same word is sometimes a person or company and sometimes an ordinary word or a place. Each pair has one document
        // for each sense, so the answer key stays exact (a word is never both in the same document). ----
        new("context-text-01", "Context: Paris the person", ["txt"],
        [
            H("Project Kestrel meeting notes"),
            P("[[PERSON|Jane]] and [[PERSON|Paris]] were in a meeting discussing the project. [[PERSON|Paris]] leads the design team and [[PERSON|Jane]] manages the budget."),
            P("After the meeting [[PERSON|Paris]] sent the notes to [[PERSON|Jane]] and asked [[GENDER|her]] to confirm the timetable before Friday."),
        ]),

        new("context-text-02", "Context: Paris the city", ["txt"],
        [
            H("Project Kestrel travel note"),
            P("[[PERSON|Jane]] was with [[GENDER|her]] team in Paris discussing the project. The team took the train to Paris on Monday and stayed in the city for three days."),
            P("The workshop in Paris went well, and the group returned to the office on Thursday. France has a good rail network."),
        ]) { MustPreserve = ["Paris", "France"] },

        new("context-text-03", "Context: Jordan the person", ["txt"],
        [
            H("Onboarding update"),
            P("[[PERSON|Jordan]] joined the finance team last week. [[PERSON|Dana]] showed [[PERSON|Jordan]] the reporting system and [[PERSON|Jordan]] asked a lot of good questions."),
            P("Please send [[PERSON|Jordan]] the access form at [[EMAIL|jordan.reeve@northfield-partners.example]]."),
        ]),

        new("context-text-04", "Context: Jordan the country", ["txt"],
        [
            H("Regional expansion plan"),
            P("[[PERSON|Dana]] travelled to Jordan to meet suppliers. The flight to Jordan takes about five hours and the team plans to open a regional office in Amman next spring."),
            P("Please send the visa forms for the Jordan trip to [[EMAIL|dana.whitlock@northfield-partners.example]]."),
        ]) { MustPreserve = ["Jordan", "Amman"] },

        new("context-text-05", "Context: names that are also ordinary words (people)", ["txt"],
        [
            H("Team lunch"),
            P("[[PERSON|Will]] booked the table and [[PERSON|Mark]] paid. [[PERSON|Rose]] brought the cake and [[PERSON|Bill]] cut it. [[PERSON|Grace]] and [[PERSON|Hope]] arrived late but were forgiven."),
            P("[[PERSON|Mark]] thanked everyone and [[PERSON|Will]] promised to organise the next one."),
        ]),

        new("context-text-06", "Context: ordinary words that are also names", ["txt"],
        [
            H("Committee minutes"),
            P("The committee will approve the budget at the meeting on 14 May. Please mark each page you have read. A rose bush was planted by the entrance."),
            P("The bill for the repairs was paid with grace and a little hope that costs will fall."),
        ]) { MustPreserve = ["will", "mark", "rose", "bill", "grace", "hope", "May"] },

        new("context-text-07", "Context: company names that are also ordinary words (companies)", ["txt"],
        [
            H("Supplier review"),
            P("Our laptops come from [[COMPANY|Apple]] and our fuel cards from [[COMPANY|Shell]]. Parcels are shipped by [[COMPANY|Amazon]] and uniforms are bought from [[COMPANY|Target]]."),
            P("The contract with [[COMPANY|Apple]] renews in June and [[COMPANY|Shell]] has offered a better rate."),
        ]),

        new("context-text-08", "Context: ordinary words that are also company names", ["txt"],
        [
            H("Weekend notes"),
            P("The children ate an apple and collected a shell on the beach. Later they paddled down the Amazon on a school trip and practised on a target in the garden."),
            P("The apple tree needs pruning and the shell path needs raking."),
        ]) { MustPreserve = ["apple", "shell", "Amazon", "target"] },

        new("context-text-09", "Context: places that are also first names (people)", ["txt"],
        [
            H("Planning session"),
            P("[[PERSON|Georgia]] and [[PERSON|Chelsea]] reviewed the plan with [[PERSON|Florence]] and [[PERSON|Victoria]]. [[PERSON|Chelsea]] will present it on Tuesday."),
            P("[[PERSON|Florence]] asked [[PERSON|Georgia]] to share the slides with [[PERSON|Victoria]]."),
        ]),

        new("context-text-10", "Context: first names that are also places", ["txt"],
        [
            H("Offsite itinerary"),
            P("The offsite starts in Chelsea in London, moves to Florence in Italy, stops near Victoria Station, and finishes in Georgia."),
            P("Coaches leave Chelsea at nine and the train from Victoria Station is booked."),
        ]) { MustPreserve = ["Chelsea", "London", "Florence", "Italy", "Victoria", "Georgia"] },

        // MIXED: the same words are a person in one sentence and a place in the next, inside ONE document. For the demo. The scorer matches words as
        // strings, so it cannot tell the two uses apart; its numbers for this one document are approximate (no keep-list is set for that reason).
        new("context-text-11-mixed", "Context: MIXED - the same words as a person and as a place in one document (demo)", ["txt"],
        [
            H("MIXED: Project Kestrel visit notes"),
            P("[[PERSON|Paris]] and [[PERSON|Jane]] spent Monday in Paris discussing the project. [[PERSON|Paris]] presented the budget and the team then had dinner in Paris."),
            P("[[PERSON|Jordan]] flew to Jordan on Tuesday to meet suppliers. The visit to Jordan was arranged by [[PERSON|Georgia]], who then travelled to Georgia for the follow-up meeting."),
        ]),
    ];
}
