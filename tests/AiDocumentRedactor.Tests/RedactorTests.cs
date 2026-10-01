using AiDocumentRedactor.Core;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for turning spans into redacted text.</summary>
public class RedactorTests
{
    const string T = "[REDACTED:{type}]";
    /// <summary>Shorthand for a test span.</summary>
    static DetectedEntity E(string type, int s, int l) => new(type, s, l, 1, "test");

    /// <summary>Spans become [REDACTED:TYPE] and their positions in the redacted text are recorded.</summary>
    [Fact]
    public void Replaces_spans_with_typed_placeholders_and_tracks_positions()
    {
        var r = Redactor.Apply("Mail bob@x.com now", [E("EMAIL", 5, 9)], T);
        Assert.Equal("Mail [REDACTED:EMAIL] now", r.RedactedText);
        var e = Assert.Single(r.Edits);
        Assert.Equal(5, e.RedactedStart);
        Assert.Equal("[REDACTED:EMAIL]".Length, e.RedactedLength);
    }

    /// <summary>When spans overlap, the longest one wins.</summary>
    [Fact]
    public void Overlapping_spans_keep_the_longest()
    {
        var r = Redactor.Apply("Sarah Jones here", [E("PERSON", 0, 5), E("PERSON", 0, 11)], T);
        Assert.Equal("[REDACTED:PERSON] here", r.RedactedText);
    }

    /// <summary>Spans outside the text are ignored.</summary>
    [Fact]
    public void Out_of_range_spans_are_ignored_and_original_survives_nowhere()
    {
        var r = Redactor.Apply("abc", [E("PERSON", 2, 10)], T);
        Assert.Equal("abc", r.RedactedText);
    }

    /// <summary>Touching spans and spans at the start or end of the text are handled.</summary>
    [Fact]
    public void Handles_adjacent_spans_and_text_boundaries()
    {
        var r = Redactor.Apply("AABB", [E("PERSON", 0, 2), E("COMPANY", 2, 2)], T);
        Assert.Equal("[REDACTED:PERSON][REDACTED:COMPANY]", r.RedactedText);
    }

    /// <summary>A flagged span stays in the text, is recorded as a Flagged edit with its position, and is dropped if a real redaction covers it.</summary>
    [Fact]
    public void Flagged_spans_stay_in_the_text_and_are_recorded()
    {
        var r = Redactor.Apply("Ann is head of compliance today", [new DetectedEntity("PERSON", 0, 3, 1, "t"), new DetectedEntity("CONTEXTUAL", 7, 18, 1, "t", Flag: true)], T);
        Assert.Equal("[REDACTED:PERSON] is head of compliance today", r.RedactedText);
        var flagged = Assert.Single(r.Edits, e => e.Status == EditStatus.Flagged);
        Assert.Equal("head of compliance", r.RedactedText.Substring(flagged.RedactedStart, flagged.RedactedLength));

        var covered = Redactor.Apply("Ann Lee says", [new DetectedEntity("PERSON", 0, 7, 1, "t"), new DetectedEntity("CONTEXTUAL", 4, 3, 1, "t", Flag: true)], T);
        Assert.DoesNotContain(covered.Edits, e => e.Status == EditStatus.Flagged);
    }
}
