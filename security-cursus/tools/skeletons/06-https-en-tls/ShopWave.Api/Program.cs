// STARTCODE voor oefening 1 van les 6. Dit is de API zoals ze na de theorie is:
// ze luistert al op poort 5001, maar zonder certificaat en zonder endpoints.
// Die vul jij aan.
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        // jouw code hier: laad het certificaat en activeer HTTPS
    });
});

WebApplication app = builder.Build();

// jouw endpoints hier

app.Run();
