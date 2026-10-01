namespace AiDocumentRedactor.App.Ui;

/// <summary>A highlighted span inside a document pane (offsets into the pane's text).</summary>
public record PaneMark(int Id, int Start, int Length, string Type, bool Live = false, string? Conf = null, bool Flagged = false);

/// <summary>Where the document was split into chunks for the model; drawn as a labelled line in the pane.</summary>
public record ChunkMarker(int Number, int Start);
