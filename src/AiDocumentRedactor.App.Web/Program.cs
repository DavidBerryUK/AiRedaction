// Local web host for the Redaction Demo UI. Binds to 127.0.0.1 only and requires a per-launch token,
// so documents are only reachable from this machine. The UI itself lives in AiDocumentRedactor.App.Ui.
using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.App.Web.Components;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Ocr;

// ---- configuration (single JSON file, FR12) ----
// Reads the value after a command-line flag, e.g. --config path. Returns null if the flag is absent.
string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
var configPath = Arg("--config") ?? "redactor.config.json";
RedactorOptions options;
try
{
    options = RedactorOptions.Load(configPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Config error: {ex.Message}");
    return 2;
}
if (Arg("--output") is { } o)
{
    options.Output.Directory = Path.GetFullPath(o);
}
else
{
    options.Output.Directory = Path.GetFullPath(options.Output.Directory);
}

if (!string.IsNullOrWhiteSpace(options.Report.Directory))
{
    options.Report.Directory = Path.GetFullPath(options.Report.Directory);
}

var inputRoot = Path.GetFullPath(Arg("--input") ?? options.Input.Directory);

// ---- local-only web host: loopback binding, per-launch token ----
var port = int.TryParse(Arg("--port"), out var pt) ? pt : 5199;
var token = Environment.GetEnvironmentVariable("REDACTOR_TOKEN") is { Length: > 0 } t ? t : Guid.NewGuid().ToString("N");

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");   // never 0.0.0.0
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// OCR engine for scanned PDFs and images (local, models load on first use). Null if switched off in the config.
IOcrEngine? ocr = options.Ocr.Enabled ? new RapidOcrEngine() : null;

// One session per browser (found by a cookie), so several browsers or people never share documents or results.
// Create the one shared session that holds results and runs redactions (detectors are created per model on demand).
builder.Services.AddSingleton(sp => new SessionRegistry(() =>
{
    var llm = options.Llm;
    IModelCatalog catalog = new OllamaModelCatalog(OllamaDetector.CreateClient(llm));
    return new RedactionSession(options, inputRoot, DocumentFormats.Readers(options, ocr), DocumentFormats.Writers(options, ocr), catalog,
        opts => opts.Llm.Provider == "ollama" ? new OllamaDetector(OllamaDetector.CreateClient(opts.Llm), opts, GlinerDetector.Create(opts)) : new NoOpDetector(),
        new ReviewStore(Path.GetFullPath(Path.Combine(".cache", "review"))))   // review changes are kept as offsets only
    {
        ConfigPath = Path.GetFullPath(configPath),
        OcrEngineName = ocr?.Name
    };
}));
// Components get the session of their own browser: the root component sets the holder, everything below asks for the session.
builder.Services.AddScoped<SessionHolder>();
builder.Services.AddScoped(sp => sp.GetRequiredService<SessionHolder>().Current ?? throw new InvalidOperationException("No session for this browser."));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// Security gate on every request: loopback host names only, a valid access token/cookie, then no-cache and strict security headers.
app.Use(async (ctx, next) =>
{
    // DNS-rebinding defence: only answer to loopback host names.
    if (ctx.Request.Host.Host is not ("127.0.0.1" or "localhost"))
    {
        ctx.Response.StatusCode = 421;
        return;
    }
    // Access token: first visit carries ?t=..., then a cookie. Other local processes/pages cannot guess it.
    if (ctx.Request.Cookies["rd_token"] != token)
    {
        if (ctx.Request.Query["t"] == token)
        {
            ctx.Response.Cookies.Append("rd_token", token, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Path = "/" });
            ctx.Response.Redirect(ctx.Request.Path.HasValue ? ctx.Request.Path.Value! : "/");
            return;
        }
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsync("Access token required. Use the URL printed at startup.");
        return;
    }
    // Which session this browser uses: a random id in an HttpOnly cookie (a missing or malformed one is replaced).
    var sid = ctx.Request.Cookies["rd_session"];
    if (!SessionRegistry.IsValidId(sid))
    {
        sid = SessionRegistry.NewId();
        ctx.Response.Cookies.Append("rd_session", sid, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Path = "/" });
    }
    ctx.Items["rd_session"] = sid;
    ctx.Response.Headers["Cache-Control"] = "no-store";
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data: blob:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'; frame-src 'self'; object-src 'self'; frame-ancestors 'self'";
    await next();
});

// Serves a viewable file (PDF / scan image) from the input folder to the in-app viewer. The path is checked so
// nothing outside the input folder can be read; the token middleware above still applies.
app.MapGet("/files/{**path}", (string path) =>
    DocumentFiles.Resolve(inputRoot, path) is { } full
        ? Results.File(full, DocumentFiles.ContentType(full), enableRangeProcessing: true)
        : Results.NotFound());

// One page of the original PDF as a PNG (an image file is returned as it is), for the page view with highlights.
app.MapGet("/pages/{page:int}/{**path}", async (int page, string path) =>
    DocumentFiles.Resolve(inputRoot, path) is { } full && page >= 0
        ? Results.File(PageRenderer.RenderPng(await File.ReadAllBytesAsync(full), full.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase), page, 110), "image/png")
        : Results.NotFound());

// One page of a redacted result as a PNG, rendered in memory from the same bytes that would be saved.
app.MapGet("/results/{id:guid}/page/{page:int}", async (Guid id, int page, HttpContext ctx, SessionRegistry sessions) =>
    await sessions.Get((string)ctx.Items["rd_session"]!).RenderRedactedAsync(id) is { } r && page >= 0
        ? Results.File(PageRenderer.RenderPng(r.Bytes, r.ContentType == "application/pdf", page, 110), "image/png")
        : Results.NotFound());

// Renders a redacted PDF or image result in memory for the viewer (never written to disk). Same token protection as everything else.
app.MapGet("/results/{id:guid}/redacted", async (Guid id, HttpContext ctx, SessionRegistry sessions) =>
    await sessions.Get((string)ctx.Items["rd_session"]!).RenderRedactedAsync(id) is { } r ? Results.File(r.Bytes, r.ContentType) : Results.NotFound());

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Print the URL (with token) to open in the browser, then run until stopped.
Console.WriteLine($"Redaction Demo — open http://127.0.0.1:{port}/?t={token}");
Console.WriteLine($"Source: {inputRoot}   Output: {options.Output.Directory}   Model: {options.Llm.Model} at {options.Llm.Endpoint}");
app.Run();
return 0;
