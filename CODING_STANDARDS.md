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
- Follow Microsoft's [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Razor/Blazor** for the UI (ASP.NET Core host, loopback-only, no platform-specific UI code)

## Project Structure

```
src/
├── AiDocumentRedactor.Cli/          # Console entry point, config, DI wiring
├── AiDocumentRedactor.App.ViewModels/ # UI logic (view-models, session, results). Plain net10.0, no UI-framework dependency
├── AiDocumentRedactor.App.Ui/       # Razor class library: all components, CSS and JS
├── AiDocumentRedactor.App.Web/      # Local ASP.NET Core (Blazor Server) host: loopback-only
├── AiDocumentRedactor.Core/         # Models, interfaces, pipeline orchestration
├── AiDocumentRedactor.Documents/    # text, docx, pdf readers/writers; page rendering and box redaction
├── AiDocumentRedactor.Ocr/          # Local OCR (Tesseract/RapidOCR) behind IOcrEngine interface
└── AiDocumentRedactor.Detection/    # LlmDetector (Ollama), prompt builder, span locator/merger

tests/        - Test projects
documentation/ - Project documentation (SPECIFICATION.md, CODING_STANDARDS.md)
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
| Fields | camelCase (private) | `_documentCache` |
| Parameters | camelCase | `documentPath` |
| Constants | PascalCase | `MaxRetryCount` |
| Static readonly | PascalCase | `DefaultTimeout` |

## File Organization

- One class per file (except partial classes)
- File name matches class name
- Use file-scoped namespaces
- Order usings: System first, then external libraries, then project usings

## Code Style

### Use explicit types over var when type is not obvious:
```csharp
// Good
string filePath = GetPath();

// OK when type is clear
var list = new List<string>();
```

### Use expression-bodied members for simple methods:
```csharp
public string GetName() => _name;

public int GetCount() => _items.Count;
```

### Use pattern matching:
```csharp
public bool IsDocument(string path) => path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
```

### Use null-coalescing for null checks:
```csharp
public string GetPath() => _cachedPath ??= Path.Combine(_config.OutputDir, "output.pdf");
```

### Always use brackets for if statements:
```csharp
// Good
if (condition)
{
    DoSomething();
}

// Bad
if (condition)
    DoSomething();
```

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
var types = [EntityTypes.Person, EntityTypes.Email];
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
    private readonly IEnumerable<IDocumentReader> _readers = readers;
    private readonly IEntityDetector _detector = detector;
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
- Use `ConfigureAwait(false)` in library code to avoid deadlocks
- Prefer `Task.Run` for CPU-bound work on background threads

## Documentation

- Public types and members must have XML documentation
- Use `<summary>`, `<param>`, `<returns>` tags
- Document exceptions with `<exception>` tag

## Testing

- Unit tests in `tests/` project
- Test method naming: `MethodName_State_ExpectedResult`
- Use xUnit as the test framework

## Exception Handling

- Use specific exception types
- Don't swallow exceptions silently
- Provide meaningful error messages

## Configuration

- Use `IOptions<T>` pattern for configuration
- Configuration files in root: `*.json`
- Environment-specific configs: `appsettings.Development.json`, `appsettings.Production.json`

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
_cachedPath ??= Path.Combine(_config.OutputDir, "output.pdf");

// Good for null checks
var value = options?.Value ?? defaultValue;
```
