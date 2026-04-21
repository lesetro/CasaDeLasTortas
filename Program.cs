using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Repositories;
using CasaDeLasTortas.Services;
using CasaDeLasTortas.Helpers;
using CasaDeLasTortas.Hubs;
using CasaDeLasTortas.Middleware;
using CasaDeLasTortas.Models.Options;  

var builder = WebApplication.CreateBuilder(args);

// ==================== BASE DE DATOS ====================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection no está configurada en appsettings.json");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
            mySqlOptions.CommandTimeout(60);
        }
    )
);

// ==================== AUTENTICACIÓN JWT ====================
// 
// Solo JWT como esquema por defecto

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey   = jwtSettings["SecretKey"] ?? jwtSettings["Key"];
var issuer      = jwtSettings["Issuer"];
var audience    = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:SecretKey debe tener al menos 32 caracteres en appsettings.json");

if (string.IsNullOrEmpty(issuer))
    throw new InvalidOperationException("Jwt:Issuer debe estar configurado en appsettings.json");

if (string.IsNullOrEmpty(audience))
    throw new InvalidOperationException("Jwt:Audience debe estar configurado en appsettings.json");

var key = Encoding.UTF8.GetBytes(secretKey);

// JWT es ahora el esquema por defecto
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.SaveToken              = true;
    options.RequireHttpsMetadata   = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = true,
        ValidIssuer              = issuer,
        ValidateAudience         = true,
        ValidAudience            = audience,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };

    // Soporte para SignalR: token por query string en /hubs/*
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogWarning("⚠️ Error de autenticación JWT: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        // Manejar Challenge (cuando no hay token o es inválido)
        OnChallenge = context =>
        {
            // Si es petición API o AJAX → devolver JSON 401
            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"success\":false,\"message\":\"No autenticado. Token JWT requerido.\"}");
            }
            
            // Si es petición MVC normal → redirigir al login
            context.HandleResponse();
            context.Response.Redirect("/Account/Login");
            return Task.CompletedTask;
        },
        // Manejar Forbidden (cuando no tiene permisos)
        OnForbidden = context =>
        {
            // Si es petición API o AJAX → devolver JSON 403
            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"success\":false,\"message\":\"Acceso denegado. No tiene permisos.\"}");
            }
            
            // Si es petición MVC normal → redirigir a AccessDenied
            context.Response.Redirect("/Account/AccessDenied");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==================== CORS ====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
        policy.WithOrigins(
                "http://localhost:5000", "https://localhost:5001",
                "http://localhost:5169", "https://localhost:7000",
                "http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());

    options.AddPolicy("Production", policy =>
        policy.WithOrigins(
                "https://tudominio.com",
                "https://www.tudominio.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ==================== REGISTRO DE SERVICIOS ====================

builder.Services.AddHttpContextAccessor();

// ── Repositorios existentes ────────────────────────────────────────────────────
builder.Services.AddScoped<IPersonaRepository,     PersonaRepository>();
builder.Services.AddScoped<IVendedorRepository,    VendedorRepository>();
builder.Services.AddScoped<ICompradorRepository,   CompradorRepository>();
builder.Services.AddScoped<ITortaRepository,       TortaRepository>();
builder.Services.AddScoped<IImagenTortaRepository, ImagenTortaRepository>();
builder.Services.AddScoped<IPagoRepository,        PagoRepository>();
builder.Services.AddScoped<IVentaRepository,       VentaRepository>();
builder.Services.AddScoped<IDetalleVentaRepository,DetalleVentaRepository>();

// ── Repositorios 
builder.Services.AddScoped<ILiberacionRepository,    LiberacionRepository>();
builder.Services.AddScoped<IDisputaRepository,       DisputaRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

// ── Unit of Work ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Servicios de infraestructura ──────────────────────────────────────────────
builder.Services.AddScoped<IFileService,   FileService>();
builder.Services.AddScoped<IAuthService,   AuthService>();
builder.Services.AddScoped<IJwtService,    JwtService>();
builder.Services.AddScoped<ICarritoService,CarritoService>();

// ── Servicios NUEVOS 
builder.Services.AddScoped<IPagoService,        PagoService>();
builder.Services.AddScoped<ILiberacionService,  LiberacionService>();

// ── JwtHelper (singleton) ─────────────────────────────────────────────────────
builder.Services.AddSingleton<JwtHelper>(provider =>
{
    var expirationHours = jwtSettings.GetValue<int>("ExpirationHours", 8);
    return new JwtHelper(
        secretKey:       secretKey,
        issuer:          issuer,
        audience:        audience,
        expirationHours: expirationHours
    );
});

// ── Configuración de Plataforma (options pattern) ────────────────────────────
builder.Services.Configure<PlataformaPagosOptions>(
    builder.Configuration.GetSection("PlataformaPagos"));

// ── FileStorage options ───────────────────────────────────────────────────────
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

// ==================== SIGNALR ====================

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval     = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout          = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval         = TimeSpan.FromSeconds(15);
    options.EnableDetailedErrors      = builder.Environment.IsDevelopment();
});

// ==================== CONTROLADORES + JSON ====================

var jsonOptions = (Microsoft.AspNetCore.Mvc.JsonOptions opts) =>
{
    opts.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
};

builder.Services.AddControllersWithViews().AddJsonOptions(jsonOptions);
builder.Services.AddControllers().AddJsonOptions(jsonOptions);

// ==================== SESIÓN / CACHÉ ====================
//  Mantenemos Session para el carrito (usa cookie de SESIÓN, no de AUTH)
//

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout               = TimeSpan.FromMinutes(30);
    options.Cookie.Name               = ".CasaDeLasTortas.Session";
    options.Cookie.HttpOnly           = true;
    options.Cookie.IsEssential        = true;
    options.Cookie.SecurePolicy       = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddHttpClient();

// ==================== LOGGING ====================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning);

// ==================== CONSTRUIR APP ====================

var app = builder.Build();

// ── Migraciones + Seed de BD ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services  = scope.ServiceProvider;
    var dbLogger  = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context    = services.GetRequiredService<ApplicationDbContext>();
        var canConnect = await context.Database.CanConnectAsync();

        if (!canConnect)
        {
            dbLogger.LogError(
                "No se pudo conectar a la base de datos. Verificá la cadena de conexión.");
            if (app.Environment.IsDevelopment())
                throw new InvalidOperationException("No se pudo conectar a la base de datos");
        }
        else
        {
            dbLogger.LogInformation("✅ Conexión a base de datos exitosa.");

            if (app.Environment.IsDevelopment())
            {
                dbLogger.LogInformation("🚀 Inicializando BD con datos de prueba...");
                await DbInitializer.Initialize(context);
                dbLogger.LogInformation("✅ BD inicializada con datos de prueba.");
            }
        }
    }
    catch (Exception ex)
    {
        var dbLogger2 = services.GetRequiredService<ILogger<Program>>();
        dbLogger2.LogError(ex, "❌ Error al inicializar la BD: {Message}", ex.Message);
        if (app.Environment.IsDevelopment()) throw;
    }
}

// ==================== PIPELINE HTTP ====================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors(app.Environment.IsDevelopment() ? "AllowDevelopment" : "Production");
app.UseSession();

//  ORDEN IMPORTANTE del pipeline de autenticación:
// 1. UseAuthentication() - Configura el esquema JWT por defecto
// 2. JwtMiddleware - Extrae token de Header/Query/Cookie y establece context.User
// 3. UseAuthorization() - Verifica [Authorize] basado en context.User
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

// ==================== ENDPOINTS ====================

app.MapControllers();

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notifications");

// ── Endpoints de diagnóstico ──────────────────────────────────────────────────
app.MapGet("/api/test", () => Results.Ok(new
{
    message   = "API funciona correctamente",
    timestamp = DateTime.UtcNow,
    endpoint  = "/api/test"
})).AllowAnonymous();

app.MapGet("/api/auth/test", () => Results.Ok(new
{
    message   = "Auth API está funcionando",
    timestamp = DateTime.UtcNow,
    endpoint  = "/api/auth/test"
})).AllowAnonymous();

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status      = "Healthy",
            timestamp   = DateTime.UtcNow,
            database    = "Connected",
            environment = app.Environment.EnvironmentName,
            authScheme  = "JWT"  //  Indicamos que usamos 100% JWT
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status      = "Unhealthy",
            timestamp   = DateTime.UtcNow,
            database    = "Error",
            error       = ex.Message,
            environment = app.Environment.EnvironmentName
        }, statusCode: 503);
    }
}).AllowAnonymous();

// ==================== INICIO ====================

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("═══════════════════════════════════════════════════════════");
startupLogger.LogInformation("🍰 Casa de las Tortas — Aplicación iniciada");
startupLogger.LogInformation("🔐 Autenticación: 100% JWT (sin cookies de auth)");
startupLogger.LogInformation("📍 Entorno: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("🌐 URLs: {Urls}",
    builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000");
startupLogger.LogInformation("═══════════════════════════════════════════════════════════");

app.Run();

