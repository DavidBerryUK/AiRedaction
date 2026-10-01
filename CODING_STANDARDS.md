# Coding Standards

This document defines the coding standards and conventions for the AiDocumentRedactor project.

## Overview

This is a C#/.NET project that uses local AI (Ollama) for document redaction. The coding standards prioritize:

- **Clarity over cleverness** - Code should be easy to understand
- **Type safety** - Leverage C#'s type system
- **Testability** - Code should be easy to test
- **Local-first** - Minimize external dependencies; no document content leaves the machine

## Language & Framework

- **C# 12** with modern features (file-scoped namespaces, target-typed new, expression-bodied members, pattern matching)
- **.NET 10** as the target framework (net10.0 for cross-platform compatibility)
- Follow Microsoft's C# Coding Conventions:
  - Use `var` for local variables (see *Use `var`* below)
  - Use `this.` prefix for member access when there's ambiguity, but not otherwise
  - Use `nameof()` operator instead of string literals when referencing identifiers
  - Use `string.Empty` instead of `""` for empty strings
  - Use `const` for compile-time constants and `static readonly` for runtime constants
  - Use `async`/`await` for all I/O operations (`File.ReadAllBytesAsync`, not `File.ReadAllBytes`). Synchronous file access is acceptable only for loading the config at start-up, tiny state files, and inside a `Task.Run` block doing CPU-bound work on bytes that were already read asynchronously
  - Do not use `ConfigureAwait(false)`: ASP.NET Core and modern .NET have no context that causes the old deadlocks, and in the UI layer code must resume on the component's context
  - Prefer `Task.Run` for CPU-bound work on background threads
- **Razor/Blazor** for the UI (ASP.NET Core host, loopback-only, no platform-specific UI code)

## Project Structure

```
src/
├── AiDocumentRedactor.Cli/          # Command-line runner: Program.cs plus small helpers (config, paths, pipeline setup)
├── AiDocumentRedactor.Eval/         # Evaluation command: scores models on the test corpus and writes a Markdown report
├── AiDocumentRedactor.App.ViewModels/ # UI logic (view-models, session, results). Plain net10.0, no UI-framework dependency
├── AiDocumentRedactor.App.Ui/       # Razor class library: all components, CSS and JS
├── AiDocumentRedactor.App.Web/      # Local ASP.NET Core (Blazor Server) host: loopback-only
├── AiDocumentRedactor.Core/         # Models, interfaces, pipeline orchestration
├── AiDocumentRedactor.Documents/    # text, docx, pdf readers/writers; page rendering and box redaction
├── AiDocumentRedactor.Ocr/          # Local OCR (RapidOCR) behind the IOcrEngine interface
└── AiDocumentRedactor.Detection/    # LlmDetector (Ollama), prompt builder, span locator/merger

tests/        - Test projects
documentation/ - Project documentation (SPECIFICATION.md); CODING_STANDARDS.md is in the repository root
tools/        - Helper tools
in/           - Input documents (for testing)
out/          - Output documents and run reports
```

## Naming Conventions

| Item | Style | Example |
|------|-------|---------|
| Classes | PascalCase | `DocumentProcessor` |
| Interfaces | IPascalCase | `IDocumentReader` |
| Methods | PascalCase | `ProcessDocument()` |
| Properties | PascalCase | `DocumentPath` |
| Fields | camelCase (private), **no underscore prefix** | `documentCache` |
| Parameters | camelCase | `documentPath` |
| Constants | PascalCase | `MaxRetryCount` |
| Static readonly | PascalCase | `DefaultTimeout` |

## File Organization

- One main class per file, and the file name matches it (`RedactionPipeline.cs` holds `RedactionPipeline`)
- Small related types may share a file: records, enums, interfaces and tiny helper classes that belong with the main type or form a closely related set (for example the option classes in `Options.cs`, or the interfaces in `Interfaces.cs`). Name the file after the main type or the group (`Options.cs`, `Models.cs`)
- Give a type its own file when it grows past about 100 lines, has its own tests, or is used on its own elsewhere
- Keep a file under about 400 lines; split a larger class by responsibility (partial classes are fine for this)
- Private fields have no underscore prefix. Use `this.` only where a parameter has the same name as a field.
- Use file-scoped namespaces
- Order usings: System first, then external libraries, then project usings

## Code Style

### Use `var`
Use `var` for local variables wherever the language allows it, whether or not the type is obvious from the right-hand side: `var filePath = GetPath();`, `var list = new List<string>();`, `var doc = await reader.ReadAsync(...)`. Use an explicit type only when `var` cannot work (for example a field, a parameter, `string? x = null;` that is assigned later, or a target-typed collection such as `string[] types = [...]`).

**Enforced by tooling:** `.editorconfig` sets the rule. Run `dotnet format style --diagnostics IDE0007` to convert a file or the solution.

### Use expression-bodied members for simple methods:
```csharp
public string GetName() => name;

public int GetCount() => items.Count;
```

### Use pattern matching:
```csharp
public bool IsDocument(string path) => path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
```

### Use null-coalescing for null checks:
```csharp
public string GetPath() => cachedPath ??= Path.Combine(config.OutputDir, "output.pdf");
```

### Braces: always, and always on a new line
Every `if`, `else`, `for`, `foreach`, `while`, `using`, `try`, `catch` and `finally` body has braces, **even when it is a single statement**, and every opening brace goes on its own line. Methods, properties and types follow the same rule.

```csharp
// Good
if (detector is OllamaDetector o)
{
    Console.WriteLine($"Tokens: {o.PromptTokens} in / {o.OutputTokens} out");
}

catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}

// Bad
if (detector is OllamaDetector o)
    Console.WriteLine($"Tokens: {o.PromptTokens} in / {o.OutputTokens} out");

catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 2; }
```

Exceptions: expression-bodied members and lambdas (`=> ...`) and object/collection initializers (`new X { A = 1 }`) may stay on one line.

**Enforced by tooling:** `.editorconfig` in the repository root sets these rules. To fix a file or the whole solution, run `dotnet format whitespace` and `dotnet format style --diagnostics IDE0011`. Razor files (`.razor`) are not covered by these tools; the same rule applies to their `@code` blocks and to `@if` / `@foreach` markup blocks, by hand. Two exceptions in markup: a short conditional placed inline in the middle of a line of markup (`<td>@if (x) { <span>…</span> }</td>`), and anything inside a `<pre>` element, where whitespace is visible.

### Use records for immutable data types:
```csharp
// Good
public record DetectedEntity(string Type, int Start, int Length, double Confidence, string Source);
public record RedactionEdit(int Id, string Type, int OriginalStart, ...);

// Use class for mutable objects
public class RedactorOptions { public string Model { get; set; } = ""; }
```

### Use collection initializers for arrays and lists:
```csharp
// Good
string[] types = [EntityTypes.Person, EntityTypes.Email];
var list = new List<string> { "a", "b", "c" };

// Bad
var list = new List<string>();
list.Add("a");
list.Add("b");
```

### Use primary constructors for simple classes:
```csharp
// Good
public class RedactionPipeline(
    IEnumerable<IDocumentReader> readers,
    IEntityDetector detector)
{
    // Use constructor parameters directly
}

// Bad - unnecessary boilerplate
public class RedactionPipeline(IEnumerable<IDocumentReader> readers, IEntityDetector detector)
{
    private readonly IEnumerable<IDocumentReader> readers = readers;
    private readonly IEntityDetector detector = detector;
}
```

### Use using declarations instead of using statements:
```csharp
// Good
using var ms = new MemoryStream();
using var doc = WordprocessingDocument.Open(ms, true);

// Bad
using (var ms = new MemoryStream()) { ... }
```

**Exception:** keep a `using` block when the object must be disposed *before the end of the method*, for example a PDF document, canvas or Word package that has to be closed (flushed) before its output stream is read. Add a short comment saying why.

### Use null-forgiving operator when you know better than the compiler:
```csharp
// Good
if (doc.MainDocumentPart! is { } main) { ... }

// Only use when you are certain the value is not null
```

### Use top-level statements for entry points:
```csharp
// Good - Program.cs
// No namespace, no class, no Main() method needed
var options = RedactorOptions.Load(configPath);

// Bad
namespace MyApp { class Program { static async Task Main() { ... } } }
```

### Async methods:
- Use `async`/`await` for all I/O operations
- Return `Task` or `Task<T>` from async methods
- Do not add `ConfigureAwait(false)` (see above); a plain `await` is the standard
- Prefer `Task.Run` for CPU-bound work on background threads

## Documentation

- Public types and members must have an XML `<summary>`, written in plain English
- Say what the parameters and the return value mean **inside the summary** when it is not obvious. `<param>` and `<returns>` tags are not required
- Add an `<exception>` tag only where a caller is expected to handle a specific exception
- Private members need a short comment only when the reason is not obvious from the code

## Testing

- Unit tests in `tests/` using xUnit
- Test method names are short sentences that say what is expected, with underscores, e.g. `Reads_text_and_word_positions`, `A_missed_item_is_a_leak`
- Tests use synthetic data only (`tests/TestCorpus`); never add real documents

## Exception Handling

- Use specific exception types
- Don't swallow exceptions silently
- Provide meaningful error messages

## Configuration

- All settings live in one JSON file, `redactor.config.json`, read into `RedactorOptions` (unknown keys are an error)
- Command-line flags may override individual values (config file < flag)
- Nothing configurable is hard-coded; a new setting gets a property with a default, a line in the specification, and a test

## Language Features

### Pattern matching over type checks:
```csharp
// Good
if (result is { Edits: { Count: > 0 } }) { ... }

// Good for simple checks
if (result.Edits.Count > 0) { ... }
```

### Prefer LINQ for collections:
```csharp
// Good
var active = edits.Where(e => e.Status == EditStatus.Active).ToList();

// Good for simple cases
var found = text.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
```

### Use the null-coalescing assignment operator:
```csharp
// Good
cachedPath ??= Path.Combine(config.OutputDir, "output.pdf");

// Good for null checks
var value = options?.Value ?? defaultValue;
```

## Separation of Concerns

### Single Responsibility Principle
The `Program.cs` file should be kept minimal and focused. Complex logic should be extracted into dedicated helper classes that each handle a single responsibility:

1. **Configuration Management**: Handle loading and validation of configuration settings
2. **Input/Output Processing**: Manage directory validation, file enumeration, and path calculations
3. **Pipeline Setup**: Configure the redaction pipeline components (readers, writers, detectors)
4. **Execution Logic**: Handle the main processing loop and result reporting

### Example Refactoring Approach
```csharp
// Instead of complex logic in Program.cs:
var configPath = Arg("--config") ?? "redactor.config.json";
RedactorOptions options;
try { options = RedactorOptions.Load(configPath); }
catch (Exception ex) { Console.Error.WriteLine($"Config error: {ex.Message}"); return 2; }

// Extract to a dedicated class:
public class ConfigurationLoader
{
    public static RedactorOptions? LoadConfiguration(string[] args)
    {
        var configPath = CommandLine.Arg(args, "--config") ?? "redactor.config.json";
        try
        {
            return RedactorOptions.Load(configPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Config error: {ex.Message}");
            return null;
        }
    }
}
```

### Class Naming Convention for Helpers
- Helper classes should be named descriptively to indicate their purpose
- Use suffixes like `Manager`, `Processor`, `Validator`, or `Builder` to clarify responsibilities
- Keep helper classes focused on a single domain of functionality

This approach improves:
- Code readability and maintainability
- Testability of individual components
- Reusability across different parts of the application
- Team collaboration by clearly separating concerns
