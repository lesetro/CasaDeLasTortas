# 📚 HELPERS - GUÍA DE ESTUDIO COMPLETA

## 🎯 ¿Qué son los Helpers?

Los **Helpers** (ayudantes) son clases utilitarias que contienen métodos estáticos reutilizables para tareas comunes que se repiten en múltiples partes de la aplicación. Son como "herramientas" que simplifican operaciones complejas.

**Analogía**: Imagina que los Helpers son como una caja de herramientas. En lugar de construir un destornillador cada vez que necesitas atornillar algo, simplemente tomas el destornillador de la caja. Los Helpers funcionan igual: en lugar de escribir el mismo código una y otra vez, usas métodos predefinidos.

---

## 📄 ARCHIVO 1: PaginacionHelper.cs

### 🤔 ¿Por qué existe este archivo?

Imagina que tienes 10,000 tortas en tu base de datos. Si intentas mostrarlas todas en una página web, la página:
1. Tardaría mucho en cargar
2. Consumiría mucha memoria
3. Sería imposible de navegar

**Solución**: La paginación divide los datos en "páginas" más pequeñas (ej: 10 tortas por página).

### 🏗️ Estructura del Archivo

```
PaginacionHelper (clase estática)
├── CrearPaginacion() ──────→ Para listas en memoria
├── CrearPaginacionAsync() ─→ Para consultas de base de datos
├── ValidarParametros() ────→ Valida números de página
├── CalcularTotalPaginas() ─→ Calcula cuántas páginas hay
├── CalcularSkip() ─────────→ Calcula desde dónde empezar
├── CrearMetadata() ────────→ Información adicional de paginación
├── Paginar() (extensión) ──→ Atajo para paginar IQueryable
├── Paginar() (extensión) ──→ Atajo para paginar List
└── GenerarUrlsNavegacion() → URLs para botones siguiente/anterior
```

### 📖 MÉTODO 1: CrearPaginacion()

**¿Qué hace?**
Toma una lista completa de datos que ya está en memoria y la divide en páginas.

**¿Cuándo usarlo?**
Cuando ya tienes todos los datos cargados en una `List<T>`.

**Ejemplo práctico**:
```csharp
// Tienes 100 tortas en memoria
var todasLasTortas = new List<Torta>(); // Imagina que tiene 100 tortas

// Quieres mostrar la página 2, con 10 tortas por página
var resultado = PaginacionHelper.CrearPaginacion(todasLasTortas, pageNumber: 2, pageSize: 10);

// resultado.Items tendrá las tortas 11-20
// resultado.PaginaActual = 2
// resultado.TotalPaginas = 10  (100 tortas / 10 por página)
// resultado.TienePaginaAnterior = true
// resultado.TienePaginaSiguiente = true
```

**¿Cómo funciona internamente?**

```csharp
// PASO 1: Validar parámetros
if (pageNumber < 1) pageNumber = 1;  // No puede ser negativo
if (pageSize < 1) pageSize = 10;     // Mínimo 1
if (pageSize > 100) pageSize = 100;  // Máximo 100 (seguridad)

// PASO 2: Calcular totales
var totalItems = source.Count;  // Contar todos los elementos
var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
// Ejemplo: 95 items / 10 por página = 9.5 → ceil(9.5) = 10 páginas

// PASO 3: Usar Skip() y Take() de LINQ
var items = source
    .Skip((pageNumber - 1) * pageSize)  // Saltar elementos previos
    .Take(pageSize)                      // Tomar solo los de esta página
    .ToList();

// Ejemplo para página 2, tamaño 10:
// Skip((2-1) * 10) = Skip(10)  → Salta los primeros 10
// Take(10)                     → Toma los siguientes 10
// Resultado: elementos 11-20
```

**Fórmula clave**:
```
Índice inicial = (númeroPágina - 1) × tamañoPágina
Página 1: (1-1) × 10 = 0  → elementos 0-9
Página 2: (2-1) × 10 = 10 → elementos 10-19
Página 3: (3-1) × 10 = 20 → elementos 20-29
```

---

### 📖 MÉTODO 2: CrearPaginacionAsync()

**¿Qué hace?**
Igual que el anterior, pero para consultas de base de datos (`IQueryable<T>`).

**¿Por qué es diferente?**
- Usa `async/await` para no bloquear el hilo
- Ejecuta el `COUNT` y `SELECT` en la **base de datos**, no en memoria
- **Más eficiente**: Solo trae los datos de la página actual desde la BD

**Ejemplo práctico**:
```csharp
// Tienes una consulta a la base de datos (aún no ejecutada)
IQueryable<Torta> query = _context.Tortas.Where(t => t.Disponible);

// Paginar directamente desde la base de datos
var resultado = await PaginacionHelper.CrearPaginacionAsync(query, pageNumber: 3, pageSize: 15);

// SQL ejecutado:
// SELECT COUNT(*) FROM Tortas WHERE Disponible = 1;        ← Cuenta total
// SELECT * FROM Tortas WHERE Disponible = 1 LIMIT 15 OFFSET 30;  ← Trae página 3
```

**Diferencia con CrearPaginacion()**:
```csharp
// ❌ MALO: Trae TODOS los datos a memoria primero
var todasLasTortas = await _context.Tortas.ToListAsync();  // 10,000 tortas en memoria
var pagina = PaginacionHelper.CrearPaginacion(todasLasTortas, 1, 10);

// ✅ BUENO: Solo trae 10 tortas desde la base de datos
var pagina = await PaginacionHelper.CrearPaginacionAsync(
    _context.Tortas,  // IQueryable, no ejecutado aún
    1, 
    10
);
```

---

### 📖 MÉTODO 3: ValidarParametros()

**¿Qué hace?**
Asegura que los números de página y tamaño sean válidos.

**¿Por qué es necesario?**
Los usuarios podrían enviar valores incorrectos:
- `pageNumber = -5` ❌
- `pageSize = 0` ❌
- `pageSize = 99999` ❌ (ataque de negación de servicio)

**Ejemplo**:
```csharp
var (pageNumber, pageSize) = PaginacionHelper.ValidarParametros(-2, 500);
// Resultado: pageNumber = 1 (corregido)
//            pageSize = 100 (máximo permitido)
```

---

### 📖 MÉTODO 4: CalcularTotalPaginas()

**¿Qué hace?**
Calcula cuántas páginas se necesitan para mostrar todos los elementos.

**Fórmula**:
```
TotalPáginas = ⌈TotalElementos / TamañoPágina⌉
```
`⌈⌉` = Ceiling (redondear hacia arriba)

**Ejemplos**:
```csharp
CalcularTotalPaginas(100, 10) = ⌈100/10⌉ = 10 páginas
CalcularTotalPaginas(95, 10)  = ⌈95/10⌉  = ⌈9.5⌉ = 10 páginas
CalcularTotalPaginas(5, 10)   = ⌈5/10⌉   = ⌈0.5⌉ = 1 página
CalcularTotalPaginas(0, 10)   = ⌈0/10⌉   = 0 páginas
```

**¿Por qué Ceiling y no Floor?**
Porque si tienes 95 elementos y 10 por página, necesitas 10 páginas (no 9), aunque la última solo tenga 5 elementos.

---

### 📖 MÉTODO 5: CalcularSkip()

**¿Qué hace?**
Calcula cuántos elementos debe saltar LINQ para llegar a la página deseada.

**Fórmula**:
```
Skip = (NúmeroPágina - 1) × TamañoPágina
```

**Ejemplos visuales**:
```
Elementos: [A, B, C, D, E, F, G, H, I, J, K, L] (12 elementos)
TamañoPágina = 3

Página 1: Skip(0) → [A, B, C]
Página 2: Skip(3) → [D, E, F]
Página 3: Skip(6) → [G, H, I]
Página 4: Skip(9) → [J, K, L]
```

---

### 📖 MÉTODO 6: CrearMetadata()

**¿Qué hace?**
Genera información adicional útil para mostrar controles de paginación en la UI.

**Ejemplo de uso en API**:
```csharp
var metadata = PaginacionHelper.CrearMetadata(totalItems: 95, pageNumber: 3, pageSize: 10);

// Resultado:
{
    "totalItems": 95,
    "pageNumber": 3,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": true,  ← Mostrar botón "Anterior"
    "hasNextPage": true,       ← Mostrar botón "Siguiente"
    "firstItemIndex": 21,      ← "Mostrando 21-30 de 95"
    "lastItemIndex": 30
}
```

**Uso en la interfaz**:
```html
<!-- Deshabilitar botón "Anterior" si no hay página previa -->
<button disabled="@(!metadata.hasPreviousPage)">Anterior</button>

<!-- Mostrar información -->
<span>Mostrando @metadata.firstItemIndex - @metadata.lastItemIndex de @metadata.totalItems</span>

<!-- Deshabilitar botón "Siguiente" si no hay más páginas -->
<button disabled="@(!metadata.hasNextPage)">Siguiente</button>
```

---

### 📖 MÉTODOS 7 y 8: Paginar() (extensiones)

**¿Qué son las extensiones?**
Son métodos que "agregan" funcionalidad a tipos existentes. Permiten escribir:
```csharp
// En lugar de:
var resultado = PaginacionHelper.CrearPaginacion(lista, 2, 10);

// Puedes escribir:
var resultado = lista.Paginar(2, 10);  // Más limpio
```

**Cómo funcionan**:
```csharp
// El keyword "this" hace que sea una extensión
public static IQueryable<T> Paginar<T>(
    this IQueryable<T> query,  // ← "this" lo convierte en extensión
    int pageNumber,
    int pageSize)
{
    // Implementación...
}
```

**Ejemplo de uso**:
```csharp
// Sin extensión (verbose)
var tortas = await PaginacionHelper.CrearPaginacionAsync(
    _context.Tortas, 
    pageNumber, 
    pageSize
);

// Con extensión (limpio)
var tortas = await _context.Tortas.Paginar(pageNumber, pageSize).ToListAsync();
```

---

### 📖 MÉTODO 9: GenerarUrlsNavegacion()

**¿Qué hace?**
Genera URLs para los botones de navegación de paginación.

**Ejemplo**:
```csharp
var urls = PaginacionHelper.GenerarUrlsNavegacion(
    baseUrl: "/api/tortas?categoria=chocolate",
    pageNumber: 3,
    totalPages: 10
);

// Resultado:
{
    "first": "/api/tortas?categoria=chocolate&pagina=1",
    "previous": "/api/tortas?categoria=chocolate&pagina=2",
    "current": "/api/tortas?categoria=chocolate&pagina=3",
    "next": "/api/tortas?categoria=chocolate&pagina=4",
    "last": "/api/tortas?categoria=chocolate&pagina=10"
}
```

**Uso en API RESTful**:
```json
{
    "data": [...],
    "links": {
        "first": "...",
        "prev": "...",
        "self": "...",
        "next": "...",
        "last": "..."
    }
}
```

---

### 🎓 CASOS DE USO REALES

#### Caso 1: Listar tortas con paginación
```csharp
public async Task<IActionResult> Index(int pagina = 1)
{
    const int TAMANIO_PAGINA = 12;
    
    var query = _context.Tortas
        .Where(t => t.Disponible)
        .OrderBy(t => t.Nombre);
    
    var resultado = await PaginacionHelper.CrearPaginacionAsync(
        query,
        pagina,
        TAMANIO_PAGINA
    );
    
    return View(resultado);
}
```

#### Caso 2: API con metadata
```csharp
[HttpGet]
public async Task<IActionResult> GetTortas([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
{
    var query = _context.Tortas.Where(t => t.Disponible);
    var datos = await PaginacionHelper.CrearPaginacionAsync(query, pagina, tamano);
    var metadata = PaginacionHelper.CrearMetadata(datos.TotalItems, pagina, tamano);
    
    return Ok(new {
        data = datos.Items,
        pagination = metadata
    });
}
```

---

## 📄 ARCHIVO 2: JwtHelper.cs

### 🤔 ¿Qué es JWT y por qué existe?

**JWT** = JSON Web Token

**Problema a resolver**:
En aplicaciones web tradicionales, la sesión se guarda en el servidor (stateful). Esto causa problemas:
1. **Escalabilidad**: Si tienes 3 servidores, necesitas sesión compartida
2. **Memoria**: Cada usuario conectado consume memoria del servidor
3. **APIs móviles**: Las apps móviles no manejan cookies bien

**Solución JWT** (Stateless):
El servidor NO guarda nada. Todo está en el token que el cliente envía en cada petición.

**Analogía del Pasaporte**:
```
Sesión tradicional (Stateful):
- Entras a un país y te dan un ticket
- El país guarda tu info en una base de datos
- Cada vez que pasas un checkpoint, consultan la BD

JWT (Stateless):
- Te dan un pasaporte con toda tu info + sello oficial
- No necesitan consultar nada, solo verifican el sello
- El pasaporte expira después de X tiempo
```

### 🔐 Anatomía de un JWT

Un JWT tiene 3 partes separadas por puntos:
```
eyJhbGc.eyJzdWI.SflKxwRJ
  ↑       ↑       ↑
Header  Payload  Signature
```

**1. Header** (Encabezado):
```json
{
    "alg": "HS256",    ← Algoritmo de firma
    "typ": "JWT"       ← Tipo de token
}
```

**2. Payload** (Carga útil - Los Claims):
```json
{
    "sub": "123",              ← User ID
    "name": "Juan Pérez",
    "email": "juan@email.com",
    "role": "Comprador",
    "exp": 1735689600,         ← Fecha de expiración (timestamp Unix)
    "iat": 1735660800          ← Fecha de emisión
}
```

**3. Signature** (Firma):
```
HMACSHA256(
    base64UrlEncode(header) + "." + base64UrlEncode(payload),
    secret_key
)
```

La firma asegura que nadie haya modificado el token.

---

### 🏗️ Estructura del Archivo JwtHelper.cs

```
JwtHelper (clase instanciable)
├── Constructor() ──────────→ Configura secretKey, issuer, audience
├── GenerarToken() ────────→ Crea un JWT para un usuario
├── ValidarToken() ────────→ Verifica si un JWT es válido
├── ObtenerUserIdDeToken() → Extrae el ID del usuario
├── ObtenerRolDeToken() ───→ Extrae el rol
├── ObtenerEmailDeToken() ─→ Extrae el email
├── TokenHaExpirado() ─────→ Verifica si expiró
├── ObtenerFechaExpiracion()→ Fecha de vencimiento
├── DecodificarTokenSinValidar()→ Lee el token (debug)
├── ExtraerTokenDeHeader() → Quita "Bearer " del header
├── UsuarioTieneRol() ─────→ Verifica permisos
├── UsuarioTieneAlgunRol() → Verifica múltiples roles
└── TiempoRestanteToken() ─→ Tiempo hasta expiración
```

---

### 📖 MÉTODO 1: Constructor

**¿Qué hace?**
Inicializa el helper con la configuración necesaria.

**Parámetros**:
```csharp
public JwtHelper(
    string secretKey,        // Clave secreta para firmar (min 32 chars)
    string issuer,           // Quién emite el token (tu app)
    string audience,         // Para quién es el token (tu app/API)
    int expirationHours = 8  // Cuánto dura el token
)
```

**¿Qué es cada cosa?**

1. **secretKey**: Como la llave de tu casa. Si alguien la tiene, puede falsificar tokens.
   ```
   ❌ MALO: "abc123"  (muy corta)
   ✅ BUENO: "MiSuperClaveSecreta123456789!@#"
   ```

2. **issuer**: Identificador de quién crea el token
   ```
   Ejemplos: "CasaDeLasTortas", "https://tuapp.com"
   ```

3. **audience**: Quién puede usar el token
   ```
   Ejemplos: "CasaDeLasTortas-API", "https://api.tuapp.com"
   ```

4. **expirationHours**: Tiempo de vida del token
   ```
   Sesiones cortas: 1-2 horas
   Sesiones normales: 8-24 horas
   Remember me: 30 días
   ```

**Uso**:
```csharp
// En Program.cs (Dependency Injection)
builder.Services.AddSingleton<JwtHelper>(provider =>
{
    return new JwtHelper(
        secretKey: builder.Configuration["Jwt:SecretKey"],
        issuer: "CasaDeLasTortas",
        audience: "CasaDeLasTortas-Users",
        expirationHours: 8
    );
});
```

---

### 📖 MÉTODO 2: GenerarToken()

**¿Qué hace?**
Crea un JWT con toda la información del usuario.

**Proceso paso a paso**:

**PASO 1: Crear Claims (información del usuario)**
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, "123"),     // ID del usuario
    new Claim(ClaimTypes.Name, "Juan Pérez"),        // Nombre completo
    new Claim(ClaimTypes.Email, "juan@email.com"),   // Email
    new Claim(ClaimTypes.Role, "Comprador"),         // Rol
    new Claim("Activo", "True"),                     // Custom claim
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único del token
};
```

**¿Qué son los Claims?**
Son pares clave-valor que describen al usuario. Piensa en ellos como etiquetas:
```
Usuario: Juan Pérez
├── ID: 123
├── Email: juan@email.com
├── Rol: Comprador
└── Activo: True
```

**PASO 2: Crear la clave de firma**
```csharp
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
```
Convierte tu clave secreta en bytes para usarla en criptografía.

**PASO 3: Crear credenciales de firma**
```csharp
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
```
Define **cómo** se firmará el token (usando HMAC-SHA256).

**PASO 4: Crear el token**
```csharp
var token = new JwtSecurityToken(
    issuer: "CasaDeLasTortas",           // Quién lo emite
    audience: "CasaDeLasTortas-Users",   // Para quién es
    claims: claims,                       // Información del usuario
    notBefore: DateTime.UtcNow,          // Válido desde ahora
    expires: DateTime.UtcNow.AddHours(8), // Expira en 8 horas
    signingCredentials: credentials       // Cómo se firma
);
```

**PASO 5: Serializar a string**
```csharp
return new JwtSecurityTokenHandler().WriteToken(token);
// Resultado: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM..."
```

**Ejemplo completo**:
```csharp
var persona = await _context.Personas.FindAsync(123);
var token = jwtHelper.GenerarToken(persona);

// token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
// El cliente guarda este token y lo envía en cada petición
```

---

### 📖 MÉTODO 3: ValidarToken()

**¿Qué hace?**
Verifica que un token sea legítimo y no haya sido manipulado.

**Validaciones que hace**:
1. ✅ La firma es correcta (no fue alterado)
2. ✅ El issuer es el esperado
3. ✅ El audience es el correcto
4. ✅ No ha expirado

**Ejemplo de uso**:
```csharp
var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

var principal = jwtHelper.ValidarToken(token);

if (principal == null)
{
    // Token inválido o expirado
    return Unauthorized();
}

// Token válido, extraer información
var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var role = principal.FindFirst(ClaimTypes.Role)?.Value;
```

**¿Qué retorna?**
- `ClaimsPrincipal`: Objeto con todos los claims si el token es válido
- `null`: Si el token es inválido o expiró

**Casos de fallo**:
```csharp
// ❌ Token modificado
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.HACKED.signature"
→ La firma no coincide → null

// ❌ Token expirado
expires: DateTime.UtcNow.AddHours(-1)  // Expiró hace 1 hora
→ Lifetime validation fails → null

// ❌ Issuer incorrecto
issuer: "OtraApp" (esperaba "CasaDeLasTortas")
→ Issuer validation fails → null
```

---

### 📖 MÉTODOS 4-6: Extraer información del token

Estos métodos facilitan obtener datos específicos del usuario sin tener que navegar por los claims manualmente.

**ObtenerUserIdDeToken()**:
```csharp
var token = "eyJ...";
var userId = jwtHelper.ObtenerUserIdDeToken(token);
// userId = 123
```

**ObtenerRolDeToken()**:
```csharp
var rol = jwtHelper.ObtenerRolDeToken(token);
// rol = "Comprador"
```

**ObtenerEmailDeToken()**:
```csharp
var email = jwtHelper.ObtenerEmailDeToken(token);
// email = "juan@email.com"
```

**Uso en un middleware**:
```csharp
public class JwtMiddleware
{
    public async Task InvokeAsync(HttpContext context, JwtHelper jwtHelper)
    {
        var token = context.Request.Headers["Authorization"].ToString();
        token = JwtHelper.ExtraerTokenDeHeader(token); // Quita "Bearer "
        
        if (!string.IsNullOrEmpty(token))
        {
            var userId = jwtHelper.ObtenerUserIdDeToken(token);
            if (userId.HasValue)
            {
                // Adjuntar usuario al contexto
                context.Items["UserId"] = userId.Value;
            }
        }
        
        await _next(context);
    }
}
```

---

### 📖 MÉTODO 7: TokenHaExpirado()

**¿Qué hace?**
Verifica si el token ya pasó su fecha de expiración.

**Ejemplo**:
```csharp
var token = "eyJ...";

if (jwtHelper.TokenHaExpirado(token))
{
    // Redirigir a login
    return RedirectToAction("Login");
}
```

**Cómo funciona**:
```csharp
// Lee el token sin validar la firma (solo para ver la fecha)
var jwtToken = tokenHandler.ReadJwtToken(token);

// Compara la fecha de expiración con la hora actual
return jwtToken.ValidTo < DateTime.UtcNow;

// Ejemplo:
// ValidTo = 2024-01-15 10:00:00 UTC
// UtcNow  = 2024-01-15 11:00:00 UTC
// → 10:00 < 11:00 → true (expirado)
```

---

### 📖 MÉTODO 8: ExtraerTokenDeHeader()

**¿Qué hace?**
Los tokens JWT se envían en el header `Authorization` con el formato:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Este método quita el prefijo `"Bearer "` para obtener solo el token.

**Ejemplo**:
```csharp
var header = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
var token = JwtHelper.ExtraerTokenDeHeader(header);
// token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Uso en una API**:
```csharp
[HttpGet]
public IActionResult MisDatos()
{
    var authHeader = Request.Headers["Authorization"].ToString();
    var token = JwtHelper.ExtraerTokenDeHeader(authHeader);
    
    if (string.IsNullOrEmpty(token))
    {
        return Unauthorized();
    }
    
    var userId = _jwtHelper.ObtenerUserIdDeToken(token);
    // Usar userId para obtener datos...
}
```

---

### 📖 MÉTODOS 9-10: Verificación de roles

**UsuarioTieneRol()**: Verifica si el usuario tiene un rol específico
```csharp
var token = "eyJ...";

if (jwtHelper.UsuarioTieneRol(token, "Admin"))
{
    // El usuario es admin
    // Permitir acceso a panel de administración
}
else
{
    return Forbid(); // 403 Forbidden
}
```

**UsuarioTieneAlgunRol()**: Verifica si el usuario tiene al menos uno de varios roles
```csharp
if (jwtHelper.UsuarioTieneAlgunRol(token, "Admin", "Vendedor"))
{
    // El usuario es Admin O Vendedor
    // Permitir editar tortas
}
```

**Equivalente a [Authorize(Roles = "...")]**:
```csharp
// En lugar de:
[Authorize(Roles = "Admin,Vendedor")]
public IActionResult EditarTorta() { }

// Puedes hacer manualmente:
public IActionResult EditarTorta()
{
    var token = JwtHelper.ExtraerTokenDeHeader(Request.Headers["Authorization"]);
    
    if (!jwtHelper.UsuarioTieneAlgunRol(token, "Admin", "Vendedor"))
    {
        return Forbid();
    }
    
    // Lógica de edición...
}
```

---

### 🎓 FLUJO COMPLETO DE AUTENTICACIÓN JWT

**1. Login (Generar Token)**:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDTO model)
{
    // 1. Validar credenciales
    var persona = await _context.Personas
        .FirstOrDefaultAsync(p => p.Email == model.Email);
    
    if (persona == null || !BCrypt.Verify(model.Password, persona.PasswordHash))
    {
        return Unauthorized("Credenciales inválidas");
    }
    
    // 2. Generar token JWT
    var token = _jwtHelper.GenerarToken(persona);
    
    // 3. Retornar token al cliente
    return Ok(new { token });
}
```

**2. Cliente guarda el token**:
```javascript
// Frontend (JavaScript)
const response = await fetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
});

const { token } = await response.json();

// Guardar en localStorage
localStorage.setItem('authToken', token);
```

**3. Cliente envía el token en cada petición**:
```javascript
const token = localStorage.getItem('authToken');

fetch('/api/tortas', {
    headers: {
        'Authorization': `Bearer ${token}`
    }
});
```

**4. Servidor valida el token**:
```csharp
[HttpGet]
[Authorize]  // ASP.NET valida el token automáticamente
public IActionResult GetTortas()
{
    // Si llegó aquí, el token es válido
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Usar userId para filtrar datos...
}
```

---

### ⚙️ CONFIGURACIÓN EN PROGRAM.CS

```csharp
// 1. Agregar servicios de autenticación JWT
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
            ValidIssuer = "CasaDeLasTortas",
            ValidateAudience = true,
            ValidAudience = "CasaDeLasTortas-Users",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 2. Registrar JwtHelper como servicio
builder.Services.AddSingleton<JwtHelper>(provider =>
{
    return new JwtHelper(
        secretKey: builder.Configuration["Jwt:SecretKey"],
        issuer: "CasaDeLasTortas",
        audience: "CasaDeLasTortas-Users",
        expirationHours: 8
    );
});

// 3. Agregar middleware de autenticación
app.UseAuthentication();  // Debe ir ANTES de UseAuthorization
app.UseAuthorization();
```

---

### 🔒 SEGURIDAD Y BUENAS PRÁCTICAS

**1. Longitud de la SecretKey**:
```csharp
❌ MALO: "abc123"
✅ BUENO: "UnaClaveMuyLargaYSegura123456789!@#$%"
```
Mínimo 256 bits (32 caracteres).

**2. Nunca exponer la SecretKey**:
```csharp
// ❌ NUNCA en código
var secretKey = "MiClaveSecreta123";

// ✅ En appsettings.json (y gitignore)
var secretKey = builder.Configuration["Jwt:SecretKey"];
```

**3. HTTPS obligatorio en producción**:
Los tokens se envían en cada petición. Sin HTTPS, pueden ser interceptados.

**4. Tiempo de expiración razonable**:
```
Sesiones cortas (banking): 15-30 minutos
Sesiones normales: 8-24 horas
Long-lived tokens: Usar refresh tokens
```

**5. Validar SIEMPRE en el servidor**:
```csharp
// ❌ NUNCA confíes solo en el frontend
// El cliente puede modificar el token en JavaScript

// ✅ Valida en el servidor
[Authorize]  // ASP.NET valida automáticamente
public IActionResult AccionProtegida() { }
```

---

### 🎯 RESUMEN COMPARATIVO

| Característica | Sesiones (Cookies) | JWT |
|----------------|-------------------|-----|
| **Almacenamiento** | Servidor | Cliente |
| **Escalabilidad** | Requiere sesión compartida | Stateless, fácil de escalar |
| **Tamaño** | Solo un ID (~36 bytes) | Token completo (~200-500 bytes) |
| **Seguridad** | HttpOnly cookies | Puede ser robado de localStorage |
| **Invalidación** | Fácil (borra en servidor) | Difícil (debe expirar) |
| **Mobile/API** | Limitado | Ideal |

---

### 📚 CONCEPTOS CLAVE PARA RECORDAR

1. **PaginacionHelper**: Divide datos grandes en páginas manejables
2. **Skip() y Take()**: LINQ para saltar y tomar elementos
3. **IQueryable vs List**: Consulta diferida vs datos en memoria
4. **JWT**: Autenticación sin estado (stateless)
5. **Claims**: Información del usuario dentro del token
6. **Firma digital**: Asegura que el token no fue modificado
7. **Expiración**: Los tokens tienen tiempo de vida limitado
8. **Bearer Token**: Formato estándar para enviar JWTs

---

**✅ Archivos creados para estudio académico**