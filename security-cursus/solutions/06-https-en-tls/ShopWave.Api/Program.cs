using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHsts(options =>
{
    options.MaxAge            = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("localhost");
        listenOptions.UseHttps(certificate);
    });
});

WebApplication app = builder.Build();

app.UseHsts();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options",        "DENY");
    context.Response.Headers.Append("X-XSS-Protection",       "1; mode=block");
    await next();
});

app.MapGet("/", () => "ShopWave API actief op HTTPS");

app.MapGet("/certificaat", () =>
{
    X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("ShopWave");

    return new
    {
        Subject    = cert.Subject,
        Issuer     = cert.Issuer,
        ValidUntil = cert.NotAfter.ToString("yyyy-MM-dd"),
        SelfSigned = cert.Subject == cert.Issuer
    };
});

app.MapGet("/onveilig/inlog", () =>
{
    // In productie zou dit endpoint via HTTP bereikbaar zijn. Dan is de response onversleuteld.
    return "email=alice@shopwave.be&password=wachtwoord123";
});

app.MapGet("/veilig/certificaatinfo", () =>
{
    X509Certificate2 cert = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    return new
    {
        Subject    = cert.Subject,
        Issuer     = cert.Issuer,
        SelfSigned = cert.Subject == cert.Issuer
    };
});

app.MapGet("/headers", (HttpContext context) =>
{
    Dictionary<string, string> headers = new Dictionary<string, string>();

    foreach (var header in context.Response.Headers)
    {
        headers[header.Key.ToLower()] = header.Value.ToString();
    }

    return headers;
});

app.Run();
