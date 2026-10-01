namespace AiDocumentRedactor.App.Ui;

/// <summary>A highlighted span inside a document pane (offsets into the pane's text).</summary>
public record PaneMark(int Id, int Start, int Length, string Type, bool Live = false, string? Conf = null, bool Flagged = false, bool Rejected = false);

/// <summary>Where the document was split into chunks for the model; drawn as a labelled line in the pane.</summary>
public record ChunkMarker(int Number, int Start);

/// <summary>A rectangle drawn by hand on a page (PDF points, origin bottom-left), shown in the page view and listed among the edits.</summary>
public record PageArea(int Id, int Page, double X, double Y, double Width, double Height);
