using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using ShopWave.Api;
using ShopWave.Security;
using System.Security.Cryptography.X509Certificates;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("Omgevingsvariabele JWT_SECRET_KEY ontbreekt.");

const string Issuer   = "shopwave-api";
const string Audience = "shopwave-client";

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = Issuer,
            ValidAudience            = Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Les 12: CORS-origins dynamisch uit de configuratie (appsettings.{Environment}.json).
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopWavePolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window      = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueLimit  = 0;
        limiterOptions.QueueProcessingOrder =
            System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature? feature =
                context.Features.Get<
                    Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

            if (feature != null)
            {
                Console.Error.WriteLine($"[FOUT] {feature.Error.Message}");
            }

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Er is een fout opgetreden.");
        });
    });
}

TokenBlacklist    tokenBlacklist    = new TokenBlacklist();
TwoFactorService  twoFactorService  = new TwoFactorService();
AccountRepository accountRepository = new AccountRepository(twoFactorService);
JwtTokenService   jwtTokenService   = new JwtTokenService(secretKey, Issuer, Audience);

accountRepository.Register("alice@shopwave.be", "wachtwoord123");
accountRepository.Register("admin@shopwave.be", "admin123");

List<string> orderDatabase = new List<string>
{
    "alice@shopwave.be|Laptop|999.99",
    "bob@shopwave.be|Muis|29.99",
    "alice@shopwave.be|Toetsenbord|79.99",
    "admin@shopwave.be|Server|4999.99"
};

app.UseHsts();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options",        "DENY");
    context.Response.Headers.Append("X-XSS-Protection",       "1; mode=block");
    await next();
});

app.UseAuthentication();

app.UseCors("ShopWavePolicy");

app.Use(async (context, next) =>
{
    string authHeader = context.Request.Headers["Authorization"].ToString();
    string token      = authHeader.Replace("Bearer ", string.Empty);

    if (tokenBlacklist.IsRevoked(token))
    {
        context.Response.StatusCode = 401;
        return;
    }

    await next();
});

app.UseAuthorization();

app.UseRateLimiter();

app.MapGet("/", () => "ShopWave API actief op HTTPS met JWT");

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

app.MapGet("/orders/zoek", HandleZoek);

app.MapGet("/orders/zoek-product", HandleZoekProduct);

app.MapGet("/crash", HandleCrash);

app.MapPost("/register", HandleRegister);

app.MapPost("/login", HandleLogin)
   .RequireRateLimiting("login");

app.MapPost("/verify", HandleVerify);

app.MapGet("/me", HandleMe).RequireAuthorization();

app.MapGet("/orders/{email}", HandleOrders)
   .RequireAuthorization();

app.MapGet("/admin/orders", HandleAdminOrders)
   .RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPost("/logout", HandleLogout).RequireAuthorization();

app.Run();

IResult HandleZoek(string email)
{
    // VEILIG: de zoekopdracht staat volledig los van de "query-structuur"
    // In een echte SQL-database gebruik je SqlCommand met Parameters.AddWithValue("@email", email)
    // De database behandelt @email altijd als waarde, nooit als SQL-code
    Console.WriteLine($"[VEILIG] Zoeken op e-mail: {email}");

    List<string> results = orderDatabase
        .Where(order => order.StartsWith(email + "|", StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Results.Ok(new { Results = results });
}

IResult HandleZoekProduct(string product)
{
    Console.WriteLine($"[VEILIG] Zoeken op product: {product}");

    List<string> results = orderDatabase
        .Where(order =>
        {
            string[] fields  = order.Split('|');
            bool     matched = false;

            if (fields.Length >= 2)
            {
                matched = fields[1].Equals(product, StringComparison.OrdinalIgnoreCase);
            }

            return matched;
        })
        .ToList();

    return Results.Ok(new { Results = results });
}

IResult HandleCrash()
{
    throw new InvalidOperationException(
        "Verbinding mislukt op SHOPWAVE-DB-01 (192.168.1.50:3306). " +
        "Connection string: Server=192.168.1.50;Uid=shopwave_admin;Pwd=ShopW@ve2024!");
}

IResult HandleRegister(RegisterRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (!request.Email.Contains("@") || !request.Email.Contains("."))
    {
        return Results.BadRequest(new { Error = "Ongeldig e-mailadres." });
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { Error = "Wachtwoord is verplicht." });
    }

    if (request.Password.Length < 8)
    {
        return Results.BadRequest(new { Error = "Wachtwoord moet minstens 8 tekens bevatten." });
    }

    if (request.Password.Length > 128)
    {
        return Results.BadRequest(new { Error = "Wachtwoord mag maximaal 128 tekens bevatten." });
    }

    accountRepository.Register(request.Email, request.Password);
    return Results.Ok(new { Message = "Geregistreerd." });
}

IResult HandleLogin(LoginRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (!request.Email.Contains("@"))
    {
        return Results.BadRequest(new { Error = "Ongeldig e-mailadres." });
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { Error = "Wachtwoord is verplicht." });
    }

    string result = accountRepository.Login(request.Email, request.Password);
    return Results.Ok(new { Status = result });
}

IResult HandleVerify(VerifyRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new { Error = "E-mailadres is verplicht." });
    }

    if (string.IsNullOrWhiteSpace(request.Code))
    {
        return Results.BadRequest(new { Error = "2FA-code is verplicht." });
    }

    if (request.Code.Length != 6)
    {
        return Results.BadRequest(new { Error = "2FA-code moet exact 6 cijfers bevatten." });
    }

    if (!request.Code.All(char.IsDigit))
    {
        return Results.BadRequest(new { Error = "2FA-code moet enkel cijfers bevatten." });
    }

    string result = accountRepository.VerifyTwoFactor(request.Email, request.Code);

    if (result != "Inloggen geslaagd.")
    {
        return Results.Unauthorized();
    }

    string role  = DetermineRole(request.Email);
    string token = jwtTokenService.GenerateToken(request.Email, role);

    return Results.Ok(new { Token = token });
}

string DetermineRole(string email)
{
    string role;

    if (email == "admin@shopwave.be")
    {
        role = "admin";
    }
    else
    {
        role = "user";
    }

    return role;
}

IResult HandleMe(HttpContext context)
{
    string email = string.Empty;
    string role  = string.Empty;

    System.Security.Claims.Claim emailClaim = context.User.FindFirst(
        System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

    System.Security.Claims.Claim roleClaim = context.User.FindFirst(
        System.Security.Claims.ClaimTypes.Role);

    if (emailClaim != null)
    {
        email = emailClaim.Value;
    }

    if (roleClaim != null)
    {
        role = roleClaim.Value;
    }

    return Results.Ok(new { Email = email, Role = role });
}

IResult HandleOrders(string email)
{
    X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate("ShopWave");
    OrderSigner      signer      = new OrderSigner(certificate);
    string           orderData   = $"{email} | Laptop | 999.99 EUR";
    string           signature   = signer.Sign(orderData);

    return Results.Ok(new { Order = orderData, Signature = signature });
}

IResult HandleAdminOrders()
{
    return Results.Ok(new
    {
        Orders = new[]
        {
            new { OrderId = "ORD-001", Customer = "alice@shopwave.be",  Total = 999.99  },
            new { OrderId = "ORD-002", Customer = "bob@shopwave.be",    Total = 249.50  },
            new { OrderId = "ORD-003", Customer = "carol@shopwave.be",  Total = 1499.00 }
        }
    });
}

IResult HandleLogout(HttpContext context)
{
    string authHeader = context.Request.Headers["Authorization"].ToString();
    string token      = authHeader.Replace("Bearer ", string.Empty);

    tokenBlacklist.Revoke(token);

    return Results.Ok(new { Message = "Uitgelogd." });
}

record LoginRequest(string Email, string Password);
record VerifyRequest(string Email, string Code);
record RegisterRequest(string Email, string Password);
