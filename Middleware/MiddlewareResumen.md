# 🔀 MIDDLEWARE - GUÍA DE ESTUDIO COMPLETA

## 🎯 ¿Qué es un Middleware?

### Definición Simple

Un **Middleware** es un componente de software que se ejecuta **entre** la petición HTTP del cliente y la respuesta del servidor.

**Analogía del Filtro de Seguridad**:
```
Imagina un edificio de oficinas:

Cliente → [Puerta giratoria] → [Detector de metales] → [Registro de visitantes] → Oficina
          ↑                     ↑                     ↑
          Middleware 1          Middleware 2          Middleware 3
```

Cada middleware:
1. **Examina** la petición entrante
2. **Decide** si dejarla pasar o rechazarla
3. **Modifica** la petición si es necesario
4. **Pasa** al siguiente middleware
5. **Puede interceptar** la respuesta de salida

---

## 🔄 Pipeline de Middlewares en ASP.NET Core

### ¿Qué es el Pipeline?

El **pipeline** es la secuencia ordenada de middlewares por donde pasa cada petición.

```
Petición HTTP
    ↓
┌───────────────────────┐
│ Middleware 1: Logging │  ← Registra la petición
└─────────↓─────────────┘
          ↓
┌───────────────────────┐
│ Middleware 2: CORS    │  ← Verifica origen permitido
└─────────↓─────────────┘
          ↓
┌───────────────────────┐
│ Middleware 3: JWT     │  ← Valida token (NUESTRO MIDDLEWARE)
└─────────↓─────────────┘
          ↓
┌───────────────────────┐
│ Middleware 4: Router  │  ← Encuentra el controlador
└─────────↓─────────────┘
          ↓
┌───────────────────────┐
│ Middleware 5: MVC     │  ← Ejecuta el controlador
└─────────↓─────────────┘
          ↓
Respuesta HTTP
```

**Importante**: El orden de los middlewares **importa mucho**.

---

## 📖 ANATOMÍA DE UN MIDDLEWARE

### Estructura Básica

```csharp
public class MiMiddleware
{
    // 1. Siguiente middleware en el pipeline
    private readonly RequestDelegate _next;
    
    // 2. Constructor - recibe el siguiente middleware
    public MiMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    // 3. Método que se ejecuta en cada petición
    public async Task InvokeAsync(HttpContext context)
    {
        // ========== ANTES de pasar al siguiente middleware ==========
        // Aquí puedes examinar/modificar la petición
        Console.WriteLine($"Petición entrante: {context.Request.Path}");
        
        // ========== PASAR al siguiente middleware ==========
        await _next(context);  // ← Punto crítico
        
        // ========== DESPUÉS de que el siguiente middleware respondió ==========
        // Aquí puedes examinar/modificar la respuesta
        Console.WriteLine($"Respuesta saliente: {context.Response.StatusCode}");
    }
}
```

### ¿Qué es RequestDelegate?

```csharp
public delegate Task RequestDelegate(HttpContext context);
```

Es simplemente una función que:
- Recibe un `HttpContext`
- Retorna un `Task` (es asíncrono)

**Representa el siguiente paso en el pipeline**.

---

## 📄 ARCHIVO: JwtMiddleware.cs

### 🤔 ¿Por qué existe este middleware?

**Problema**: Cada controlador necesita:
1. Extraer el token JWT
2. Validarlo
3. Verificar si expiró
4. Buscar al usuario en la BD
5. Adjuntar el usuario al contexto

**Sin middleware** (repetitivo):
```csharp
[HttpGet]
public async Task<IActionResult> MisDatos()
{
    // Repetir en CADA acción
    var token = Request.Headers["Authorization"]...;
    var userId = ValidarToken(token);
    var usuario = await _context.Personas.FindAsync(userId);
    
    // Lógica real del controlador...
}
```

**Con middleware** (automático):
```csharp
[HttpGet]
public IActionResult MisDatos()
{
    // El usuario ya está disponible gracias al middleware
    var usuario = JwtMiddleware.ObtenerUsuarioActual(HttpContext);
    
    // Lógica real del controlador...
}
```

---

## 🏗️ ESTRUCTURA DEL JwtMiddleware.cs

```
JwtMiddleware
│
├── 🔧 CONSTRUCTOR
│   └── RequestDelegate _next
│
├── 🔄 InvokeAsync()  ← Método principal
│   ├── 1. Extraer token
│   ├── 2. Validar token
│   ├── 3. Adjuntar usuario
│   └── 4. Continuar pipeline
│
├── 📥 ExtraerToken()
│   ├── Header Authorization (API)
│   ├── Cookie (Web)
│   └── Query string (SignalR)
│
├── 👤 AdjuntarUsuarioAlContexto()
│   ├── Validar token con JWT
│   ├── Verificar algoritmo
│   ├── Consultar usuario en BD
│   ├── Verificar que esté activo
│   └── Adjuntar a HttpContext.Items
│
└── 🛠️ HELPERS ESTÁTICOS
    ├── ObtenerUsuarioActual()
    ├── ObtenerUsuarioIdActual()
    └── ObtenerRolActual()
```

---

## 📖 MÉTODO 1: InvokeAsync()

Este es el **corazón** del middleware. Se ejecuta en **cada petición HTTP**.

```csharp
public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
{
    // PASO 1: Intentar obtener el token
    var token = ExtraerToken(context);

    // PASO 2: Si hay token, validarlo
    if (!string.IsNullOrEmpty(token))
    {
        await AdjuntarUsuarioAlContexto(context, token, unitOfWork);
    }

    // PASO 3: Continuar con el siguiente middleware
    await _next(context);
    
    // NOTA: Si no hay token, la petición continúa de todas formas
    // Los controladores con [Authorize] rechazarán la petición
}
```

**¿Por qué no rechazar si no hay token?**
Porque algunas rutas son públicas:
- `/home/index`
- `/account/login`
- `/account/register`

El middleware JWT solo **valida** el token si existe. La decisión de **requerir** autenticación es de `[Authorize]`.

---

## 📖 MÉTODO 2: ExtraerToken()

Busca el token JWT en **3 lugares diferentes**.

### Lugar 1: Header Authorization (API)

```csharp
var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
// authHeader = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

if (authHeader != null && authHeader.StartsWith("Bearer "))
{
    return authHeader.Substring("Bearer ".Length).Trim();
    // return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**¿Por qué "Bearer"?**
Es el estándar OAuth 2.0 para tokens de portador:
```
Authorization: Bearer {token}
```

**Ejemplo de petición API**:
```http
GET /api/tortas HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Lugar 2: Cookie (Web)

```csharp
if (context.Request.Cookies.TryGetValue("auth_token", out var cookieToken))
{
    return cookieToken;
}
```

**¿Cuándo se usa?**
En aplicaciones web tradicionales con vistas Razor. El token se guarda en una cookie HttpOnly cuando el usuario hace login:

```csharp
// En AccountController.Login
Response.Cookies.Append("auth_token", token, new CookieOptions
{
    HttpOnly = true,   // No accesible desde JavaScript (seguridad XSS)
    Secure = true,     // Solo HTTPS
    SameSite = SameSiteMode.Strict,
    Expires = DateTime.UtcNow.AddHours(8)
});
```

### Lugar 3: Query String (SignalR)

```csharp
var queryToken = context.Request.Query["access_token"].FirstOrDefault();
```

**¿Por qué en query string?**
SignalR (WebSockets) no puede enviar headers personalizados en la conexión inicial:

```javascript
// Cliente SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications?access_token=' + token)
    .build();
```

**Resumen de lugares**:
```
Petición API móvil     → Header "Authorization: Bearer ..."
Petición web (navegador) → Cookie "auth_token"
Conexión SignalR        → Query "?access_token=..."
```

---

## 📖 MÉTODO 3: AdjuntarUsuarioAlContexto()

Este método es el **más importante**. Valida el token y adjunta el usuario.

### PASO 1: Obtener la clave secreta

```csharp
var secretKey = _configuration["Jwt:SecretKey"];
// secretKey = "MiClaveSecretaSuperSegura123456789!@#"

var key = Encoding.UTF8.GetBytes(secretKey);
// Convierte a bytes para usarla en criptografía
```

**¿De dónde viene?**
Del archivo `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "MiClaveSecretaSuperSegura123456789!@#",
    "Issuer": "CasaDeLasTortas",
    "Audience": "CasaDeLasTortas-Users"
  }
}
```

### PASO 2: Configurar parámetros de validación

```csharp
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,  // ✅ Verificar la firma
    IssuerSigningKey = new SymmetricSecurityKey(key),
    
    ValidateIssuer = true,            // ✅ Verificar el emisor
    ValidIssuer = "CasaDeLasTortas",
    
    ValidateAudience = true,          // ✅ Verificar la audiencia
    ValidAudience = "CasaDeLasTortas-Users",
    
    ValidateLifetime = true,          // ✅ Verificar que no expiró
    ClockSkew = TimeSpan.Zero         // Sin tolerancia de tiempo
};
```

**¿Qué valida cada opción?**

1. **IssuerSigningKey**: Verifica que el token fue firmado con nuestra clave secreta
   ```
   Token falsificado con otra clave → Firma inválida → Rechazado
   ```

2. **Issuer**: Verifica que el token fue emitido por nuestra aplicación
   ```
   Token de otra app → Issuer diferente → Rechazado
   ```

3. **Audience**: Verifica que el token es para nuestra aplicación
   ```
   Token destinado a otra API → Audience diferente → Rechazado
   ```

4. **Lifetime**: Verifica que no haya expirado
   ```
   Token creado hace 9 horas (expira en 8) → Expirado → Rechazado
   ```

5. **ClockSkew**: Tolerancia de tiempo para sincronización de relojes
   ```
   ClockSkew = TimeSpan.Zero  → Sin tolerancia
   ClockSkew = TimeSpan.FromMinutes(5) → Permite 5 minutos de diferencia
   ```

### PASO 3: Validar el token

```csharp
var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
```

**¿Qué retorna?**
- `principal`: ClaimsPrincipal con todos los claims del usuario
- `validatedToken`: El token validado (objeto JwtSecurityToken)

**Si falla**: Lanza una excepción (`SecurityTokenException`, `SecurityTokenExpiredException`, etc.)

### PASO 4: Verificar el algoritmo de firma (seguridad adicional)

```csharp
if (validatedToken is JwtSecurityToken jwtToken)
{
    var isValidAlgorithm = jwtToken.Header.Alg.Equals(
        SecurityAlgorithms.HmacSha256,
        StringComparison.InvariantCultureIgnoreCase
    );

    if (!isValidAlgorithm)
    {
        return; // Rechazar
    }
}
```

**¿Por qué verificar el algoritmo?**
Para prevenir ataques de "algorithm confusion":
```
Atacante crea token con algoritmo "none":
{
  "alg": "none",  ← Sin firma
  "typ": "JWT"
}

Si no verificamos, podría aceptarse un token sin firma
```

### PASO 5: Extraer el ID del usuario

```csharp
var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
// userIdClaim.Value = "123"

if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
{
    return; // Token sin ID de usuario válido
}
```

**ClaimTypes.NameIdentifier**:
Es el claim estándar para el ID del usuario. Se configuró al generar el token:
```csharp
// En JwtHelper.GenerarToken()
claims.Add(new Claim(ClaimTypes.NameIdentifier, persona.IdPersona.ToString()));
```

### PASO 6: Verificar usuario en base de datos

```csharp
var persona = await unitOfWork.PersonaRepository.GetByIdAsync(userId);

if (persona == null)
{
    // Usuario no existe (fue eliminado después de emitir el token)
    return;
}

if (!persona.Activo)
{
    // Usuario fue desactivado
    return;
}
```

**¿Por qué consultar la BD?**
Aunque el token sea válido, el usuario podría:
1. Haber sido eliminado
2. Haber sido desactivado por un admin
3. Haber cambiado su rol

**Validación en dos capas**:
```
Token válido ✅ + Usuario activo en BD ✅ = Autorizado ✅
Token válido ✅ + Usuario inactivo en BD ❌ = No autorizado ❌
```

### PASO 7: Adjuntar al contexto

```csharp
context.Items["User"] = persona;
context.Items["UserId"] = userId;
context.Items["UserRole"] = persona.Rol;

// Opcional: Para compatibilidad con [Authorize]
context.User = principal;
```

**¿Qué es HttpContext.Items?**
Un diccionario para compartir datos entre middlewares y controladores en la **misma petición**:

```csharp
// En el middleware
context.Items["UserId"] = 123;

// En el controlador
var userId = (int)HttpContext.Items["UserId"];
// userId = 123
```

**Diferencia con Session**:
```
HttpContext.Items  → Dura UNA petición
HttpContext.Session → Dura múltiples peticiones (hasta que cierre sesión)
```

---

## 📖 MÉTODOS AUXILIARES ESTÁTICOS

Facilitan el acceso a los datos del usuario desde cualquier controlador.

### ObtenerUsuarioActual()

```csharp
public static Persona? ObtenerUsuarioActual(HttpContext context)
{
    return context.Items["User"] as Persona;
}
```

**Uso en controlador**:
```csharp
[HttpGet]
public IActionResult MisDatos()
{
    var usuario = JwtMiddleware.ObtenerUsuarioActual(HttpContext);
    
    if (usuario == null)
    {
        return Unauthorized();
    }
    
    return Ok(new {
        nombre = usuario.Nombre,
        email = usuario.Email
    });
}
```

### ObtenerUsuarioIdActual()

```csharp
public static int? ObtenerUsuarioIdActual(HttpContext context)
{
    return context.Items["UserId"] as int?;
}
```

**Uso**:
```csharp
var userId = JwtMiddleware.ObtenerUsuarioIdActual(HttpContext);
// userId = 123 (o null si no está autenticado)
```

### ObtenerRolActual()

```csharp
public static string? ObtenerRolActual(HttpContext context)
{
    return context.Items["UserRole"] as string;
}
```

**Uso**:
```csharp
var rol = JwtMiddleware.ObtenerRolActual(HttpContext);
// rol = "Vendedor"

if (rol == "Admin")
{
    // Lógica específica para admin
}
```

---

## ⚙️ CONFIGURACIÓN EN PROGRAM.CS

### Orden Correcto de Middlewares

```csharp
var app = builder.Build();

// 1. HTTPS Redirection (siempre primero en producción)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 2. Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// 3. Routing (encuentra la ruta)
app.UseRouting();

// 4. CORS (si aplica)
app.UseCors();

// 5. Autenticación (ANTES de autorización)
app.UseAuthentication();  // ← ASP.NET Core valida automáticamente

// 6. NUESTRO MIDDLEWARE JWT (opcional si usamos UseAuthentication)
app.UseJwtMiddleware();

// 7. Autorización (verifica permisos)
app.UseAuthorization();

// 8. Endpoints (MVC, API, SignalR)
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
```

**Orden crítico**:
```
❌ MALO:
app.UseAuthorization();     // Intenta verificar permisos
app.UseAuthentication();    // Usuario aún no autenticado
→ Resultado: Siempre unauthorized

✅ BUENO:
app.UseAuthentication();    // Autentica usuario
app.UseAuthorization();     // Verifica permisos
→ Resultado: Funciona correctamente
```

### Registrar el Middleware

**Opción 1: UseMiddleware (sin extensión)**:
```csharp
app.UseMiddleware<JwtMiddleware>();
```

**Opción 2: Con método de extensión (más limpio)**:
```csharp
// En JwtMiddleware.cs
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}

// En Program.cs
app.UseJwtMiddleware();  // ✨ Más elegante
```

---

## 🔍 FLUJO COMPLETO DE UNA PETICIÓN

### Ejemplo: GET /api/tortas

```
1. Cliente envía petición:
   GET /api/tortas HTTP/1.1
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

2. UseRouting → Identifica la ruta: TortaController.Index()

3. UseAuthentication → Valida el token automáticamente

4. UseJwtMiddleware → (NUESTRO MIDDLEWARE)
   ├─ Extrae token del header "Authorization"
   ├─ Valida firma, issuer, audience, expiración
   ├─ Consulta usuario en BD: Persona ID=123
   ├─ Verifica que esté activo: ✅
   └─ Adjunta a HttpContext.Items["User"]

5. UseAuthorization → Verifica [Authorize] en el controlador

6. TortaController.Index()
   ├─ var usuario = JwtMiddleware.ObtenerUsuarioActual(HttpContext);
   ├─ var tortas = await _repo.GetAllAsync();
   └─ return Ok(tortas);

7. Respuesta:
   HTTP/1.1 200 OK
   Content-Type: application/json
   [{ "id": 1, "nombre": "Torta de Chocolate", ... }]
```

---

## 🛡️ SEGURIDAD

### 1. Inyección de Dependencias por Método

```csharp
public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
                                                    ↑
                                            Inyección por método
```

**¿Por qué no en el constructor?**
Los middlewares son **singleton** (una sola instancia). Si inyectamos servicios **scoped** (como DbContext) en el constructor, tendremos problemas:

```csharp
// ❌ MALO: DbContext scoped en constructor singleton
private readonly IUnitOfWork _unitOfWork;

public JwtMiddleware(RequestDelegate next, IUnitOfWork unitOfWork)
{
    _next = next;
    _unitOfWork = unitOfWork;  // ← PROBLEMA
}

// ✅ BUENO: Inyectar en InvokeAsync
public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
{
    // unitOfWork se crea para cada petición
}
```

### 2. Manejo de Excepciones

```csharp
try
{
    var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
    // ...
}
catch (SecurityTokenExpiredException)
{
    // Token expirado - no adjuntamos usuario
    // La petición continúa, [Authorize] rechazará
}
catch (SecurityTokenException ex)
{
    // Token inválido - no adjuntamos usuario
}
catch (Exception ex)
{
    // Error inesperado - registrar y continuar
    _logger.LogError(ex, "Error al validar token");
}
```

**Principio**: Fallar silenciosamente
- No lanzamos excepciones
- No retornamos 401 Unauthorized
- Simplemente no adjuntamos el usuario
- Dejamos que `[Authorize]` maneje el rechazo

### 3. Verificación del Algoritmo

```csharp
var isValidAlgorithm = jwtToken.Header.Alg.Equals(
    SecurityAlgorithms.HmacSha256,
    StringComparison.InvariantCultureIgnoreCase
);
```

Previene ataques donde el atacante cambia el algoritmo a "none" o "RS256" para falsificar tokens.

### 4. ClockSkew = TimeSpan.Zero

```csharp
ClockSkew = TimeSpan.Zero  // Sin tolerancia
```

Por defecto, ASP.NET permite 5 minutos de diferencia. Nosotros lo ponemos en cero para mayor seguridad.

---

## 🎓 CASOS DE USO

### Caso 1: Obtener Usuario en Controlador

```csharp
[HttpGet("mi-perfil")]
[Authorize]
public IActionResult MiPerfil()
{
    // El middleware ya validó el token y adjuntó el usuario
    var usuario = JwtMiddleware.ObtenerUsuarioActual(HttpContext);
    
    return Ok(new {
        id = usuario.IdPersona,
        nombre = usuario.Nombre,
        email = usuario.Email,
        rol = usuario.Rol
    });
}
```

### Caso 2: Verificar Permisos Manualmente

```csharp
[HttpPut("tortas/{id}")]
public async Task<IActionResult> ActualizarTorta(int id, TortaViewModel model)
{
    var torta = await _unitOfWork.TortaRepository.GetByIdAsync(id);
    
    // Verificar que el usuario sea el dueño
    var userId = JwtMiddleware.ObtenerUsuarioIdActual(HttpContext);
    if (torta.VendedorId != userId)
    {
        return Forbid(); // 403 Forbidden
    }
    
    // Actualizar torta...
}
```

### Caso 3: Usar en SignalR Hub

```csharp
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // El middleware ya validó el token
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort(); // Rechazar conexión sin token
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnConnectedAsync();
    }
}
```

---

## 🔄 ALTERNATIVA: UseAuthentication de ASP.NET Core

ASP.NET Core ya tiene autenticación JWT integrada. Nuestro `JwtMiddleware` personalizado es **opcional**.

**Configuración alternativa**:
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"];
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// En el pipeline
app.UseAuthentication();  // ← Hace lo mismo que nuestro middleware
app.UseAuthorization();
```

**Diferencia**:
```
UseAuthentication        → Autenticación genérica de ASP.NET Core
JwtMiddleware (custom)   → Personalizado con lógica adicional (verificar en BD, etc.)
```

**Ventaja del middleware custom**:
- Control total sobre la validación
- Lógica de negocio personalizada
- Adjuntar datos adicionales al contexto

---

## 📚 CONCEPTOS CLAVE PARA RECORDAR

1. **Middleware**: Componente que intercepta peticiones HTTP
2. **Pipeline**: Secuencia ordenada de middlewares
3. **RequestDelegate**: Representa el siguiente middleware
4. **InvokeAsync**: Método que se ejecuta en cada petición
5. **HttpContext.Items**: Diccionario para compartir datos en una petición
6. **ClaimsPrincipal**: Objeto con los claims del usuario autenticado
7. **SecurityTokenException**: Excepción al validar un token inválido
8. **Dependency Injection**: Scoped services se inyectan en InvokeAsync, no en constructor

---

**✅ Middleware JwtMiddleware.cs completo con validación JWT y seguridad robusta**