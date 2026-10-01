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
    static Block T(params string[][] rows) => new(BlockKind.Table, "", rows);

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
            P("Your new salary will be paid into the account ending 4821. If any of these details are wrong, please call me on [[PHONE|07700 900123]] or write to [[EMAIL|marcus.delaney@brightwater-analytics.example]]."),
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
            P("Reply from [[PERSON|Aisha Rahman]], [[COMPANY|Fernleigh Parcels Ltd]]: Mr [[PERSON|Kowalczyk]], sorry for the delay. We will redeliver tomorrow."),
        ]),

        new("04-meeting-notes", "Meeting notes", ["md", "docx"],
        [
            H("Project [[CONTEXTUAL|Kestrel]] - steering meeting"),
            P("Attendees: [[PERSON|Priya Natarajan]] ([[COMPANY|Corvid Logistics PLC]]), [[PERSON|Marcus Delaney]], [[PERSON|Jonas Eriksen]] ([[COMPANY|Nordlys Consulting AS]])."),
            P("[[PERSON|Jonas]] said the new depot manager, a woman [[AGE|in her forties]], would start in January. Actions: [[PERSON|Priya]] to send the draft to [[EMAIL|jonas.eriksen@nordlys.example]]. Next meeting at the [[COMPANY|Nordlys]] office."),
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
            P("Dear Colleague, thank you for seeing this [[AGE|61]]-year-old [[GENDER|man]] who reports exertional chest tightness. He works as a bus driver for [[COMPANY|Eastway Coaches Ltd]]. Please contact [[PERSON|Dr Helen Okafor]] at [[COMPANY|Fernleigh Surgery]] with your findings."),
        ]) { ScanKinds = ["clean", "degraded"] },

        new("10-contextual-profile", "Profile with indirect identifiers", ["txt"],
        [
            P("Our regional director is the only female partner at the Bristol office of [[COMPANY|Corvid Logistics PLC]]. She joined after the [[CONTEXTUAL|2019 data breach at the Bristol depot]] and earns [[CONTEXTUAL|GBP 92,000]]. Before that she was [[CONTEXTUAL|head of compliance at Harbourside Credit Union]]."),
            P("Her predecessor, [[PERSON|Gareth Lloyd]], now teaches at [[CONTEXTUAL|Merriweather Grammar School]]."),
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
            Comment = "Check with [[PERSON|Dev Patel]] before sending - his number is [[PHONE|07700 900999]]",
            TrackedDeletion = "[[SECRET|Tr1cky-Gl4cier!]] ",
            MetadataAuthor = "[[PERSON|Priya Natarajan]]",
            SplitRuns = true,
        },
    ];
}
