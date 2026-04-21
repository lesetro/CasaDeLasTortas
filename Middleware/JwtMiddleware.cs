using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Middleware
{
    /// <summary>
    /// JwtMiddleware - Versión 100% JWT
    /// ═══════════════════════════════════════════════════════════════
    /// Este middleware extrae el token JWT de múltiples fuentes y
    /// establece el HttpContext.User para que tanto MVC como API
    /// puedan usar User.Identity, User.IsInRole(), etc.
    /// 
    /// FUENTES DE TOKEN (en orden de prioridad):
    /// 1. Header Authorization: Bearer {token}
    /// 2. Query String: ?access_token={token} (para SignalR)
    /// 3. Cookie: authToken={token} (para compatibilidad con vistas)
    /// 
    /// NOTA: Aunque soportamos leer token desde cookie, esto NO es
    /// autenticación por cookie. El token JWT sigue siendo la fuente
    /// de verdad, solo que puede estar almacenado en una cookie.
    /// ═══════════════════════════════════════════════════════════════
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService)
        {
            // Si ya está autenticado (por el middleware de JWT Bearer), no hacer nada
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Intentar extraer y validar token
            var token = ExtractToken(context);

            if (!string.IsNullOrEmpty(token))
            {
                AttachUserToContext(context, jwtService, token);
            }

            await _next(context);
        }

        /// <summary>
        /// Extrae el token JWT de múltiples fuentes
        /// </summary>
        private string? ExtractToken(HttpContext context)
        {
            // 1. Header Authorization (prioridad más alta)
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("🔑 Token extraído desde Authorization Header");
                    return token;
                }
            }

            // 2. Query String (para SignalR y casos especiales)
            var queryToken = context.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                _logger.LogDebug("🔑 Token extraído desde Query String");
                return queryToken;
            }

            // 3. Cookie (para vistas CSHTML que guardan token en cookie)
            //  NOTA: Esto permite que las vistas MVC funcionen con JWT
            // El frontend puede guardar el JWT en una cookie llamada "authToken"
            var cookieToken = context.Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogDebug("🔑 Token extraído desde Cookie 'authToken'");
                return cookieToken;
            }

            return null;
        }

        /// <summary>
        /// Valida el token y establece HttpContext.User
        /// </summary>
        private void AttachUserToContext(HttpContext context, IJwtService jwtService, string token)
        {
            try
            {
                var principal = jwtService.ValidateToken(token);
                
                if (principal != null && principal.Identity?.IsAuthenticated == true)
                {
                    // ✅ CLAVE: Establecer context.User para que MVC lo reconozca
                    // Esto hace que User.Identity.IsAuthenticated, User.IsInRole(), etc. funcionen
                    context.User = principal;
                    
                    // Log informativo (solo en desarrollo para no llenar logs)
                    var userId   = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                    var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
                    
                    _logger.LogDebug(
                        "✅ Usuario autenticado via JWT: ID={UserId}, Nombre={UserName}, Rol={UserRole}", 
                        userId, userName, userRole
                    );
                }
                else
                {
                    _logger.LogDebug("⚠️ Token presente pero principal no autenticado");
                }
            }
            catch (Exception ex)
            {
                // Token inválido o expirado - el usuario quedará sin autenticar
                // Esto NO es un error crítico, es normal que tokens expiren
                _logger.LogDebug("⚠️ Token inválido o expirado: {Message}", ex.Message);
            }
        }
    }
}