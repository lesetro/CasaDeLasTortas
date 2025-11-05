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

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURACIÓN DE BASE DE DATOS ====================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada en appsettings.json");
}

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

// ==================== CONFIGURACIÓN DE AUTENTICACIÓN JWT ====================

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey debe tener al menos 32 caracteres en appsettings.json");
}

if (string.IsNullOrEmpty(issuer))
{
    throw new InvalidOperationException("Jwt:Issuer debe estar configurado en appsettings.json");
}

if (string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("Jwt:Audience debe estar configurado en appsettings.json");
}

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Error de autenticación JWT");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==================== CONFIGURACIÓN DE CORS ====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:5001", 
                "http://localhost:5169",
                "https://localhost:7000",
                "http://localhost:3000",
                "https://localhost:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://tudominio.com",
                "https://www.tudominio.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ==================== REGISTRO DE SERVICIOS ====================

builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IVendedorRepository, VendedorRepository>();
builder.Services.AddScoped<ICompradorRepository, CompradorRepository>();
builder.Services.AddScoped<ITortaRepository, TortaRepository>();
builder.Services.AddScoped<IImagenTortaRepository, ImagenTortaRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JwtHelper
builder.Services.AddSingleton<JwtHelper>(provider =>
{
    var expirationHours = jwtSettings.GetValue<int>("ExpirationHours", 8);
    return new JwtHelper(
        secretKey: secretKey,
        issuer: issuer,
        audience: audience,
        expirationHours: expirationHours
    );
});

// ==================== CONFIGURACIÓN DE SIGNALR ====================

builder.Services.AddSignalR();

// ==================== CONFIGURACIÓN DE CONTROLADORES ====================

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configuración explícita para API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ==================== CONFIGURACIÓN ADICIONAL ====================

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".CasaDeLasTortas.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddHttpClient();

// ==================== LOGGING ====================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// ==================== CONSTRUIR APLICACIÓN ====================

var app = builder.Build();

// ==================== INICIALIZAR BASE DE DATOS ====================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbLogger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        dbLogger.LogInformation("Verificando conexión a base de datos...");
        
        var canConnect = await context.Database.CanConnectAsync();
        
        if (!canConnect)
        {
            dbLogger.LogError("No se pudo conectar a la base de datos. Verifique la cadena de conexión.");
            if (app.Environment.IsDevelopment())
            {
                throw new InvalidOperationException("No se pudo conectar a la base de datos");
            }
        }
        else
        {
            dbLogger.LogInformation("✅ Conexión a base de datos exitosa.");
            
            if (app.Environment.IsDevelopment())
            {
                dbLogger.LogInformation("🚀 Inicializando base de datos DESARROLLO...");
                await DbInitializer.InitializeAsync(context, isDevelopment: true);
                dbLogger.LogInformation("✅ Base de datos DESARROLLO inicializada con datos de prueba.");
            }
            else
            {
                dbLogger.LogInformation("🏭 Inicializando base de datos PRODUCCIÓN...");
                await DbInitializer.InitializeAsync(context, isDevelopment: false);
                dbLogger.LogInformation("✅ Base de datos PRODUCCIÓN inicializada.");
            }
        }
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "❌ Error al inicializar la base de datos: {Message}", ex.Message);
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// ==================== CONFIGURACIÓN DEL PIPELINE HTTP ====================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// CORS 
app.UseCors(app.Environment.IsDevelopment() ? "AllowDevelopment" : "Production");

app.UseSession();
app.UseAuthentication();


//app.UseMiddleware<JwtMiddleware>();

app.UseAuthorization();

// ==================== MAPEO DE ENDPOINTS ====================

// 🔥 CRÍTICO: MapControllers debe estar presente para los controladores API
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notifications");

// ==================== ENDPOINTS DE PRUEBA ====================

app.MapGet("/api/test", () => Results.Ok(new { 
    message = "API funciona correctamente", 
    timestamp = DateTime.UtcNow,
    endpoint = "/api/test"
})).AllowAnonymous();

app.MapGet("/api/auth/test", () => Results.Ok(new { 
    message = "Auth API está funcionando", 
    timestamp = DateTime.UtcNow,
    endpoint = "/api/auth/test"
})).AllowAnonymous();

app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            database = "Connected",
            environment = app.Environment.EnvironmentName
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "Unhealthy",
            timestamp = DateTime.UtcNow,
            database = "Error",
            error = ex.Message,
            environment = app.Environment.EnvironmentName
        }, statusCode: 503);
    }
}).AllowAnonymous();

// ==================== INICIO DE APLICACIÓN ====================

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("🍰 Casa de las Tortas - Aplicación iniciada");
logger.LogInformation("Entorno: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("URLs: {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000");


app.Run();