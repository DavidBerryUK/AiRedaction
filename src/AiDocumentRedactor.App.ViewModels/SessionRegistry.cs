using System.Collections.Concurrent;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Gives each browser its own <see cref="RedactionSession"/>, found by a random id kept in that browser's cookie. Two browsers
/// never see or change each other's documents, results or review. A page refresh in the same browser keeps its session.</summary>
public class SessionRegistry(Func<RedactionSession> create)
{
    readonly ConcurrentDictionary<string, RedactionSession> sessions = new();

    /// <summary>The session for this id, created the first time it is asked for.</summary>
    public RedactionSession Get(string id) => sessions.GetOrAdd(id, _ => create());

    /// <summary>True if the text looks like an id this registry hands out (32 hex characters), so junk cookies are never used as keys.</summary>
    public static bool IsValidId(string? id) => id is { Length: 32 } && id.All(Uri.IsHexDigit);

    /// <summary>A new random session id.</summary>
    public static string NewId() => Guid.NewGuid().ToString("N");
}

/// <summary>Per-circuit holder that tells components which session belongs to their browser. The root component sets it before any
/// child component asks for a <see cref="RedactionSession"/>.</summary>
public class SessionHolder
{
    /// <summary>The session for the current browser.</summary>
    public RedactionSession? Current { get; set; }
}
