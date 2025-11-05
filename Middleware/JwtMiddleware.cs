using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Middleware
{
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
            var token = ExtractToken(context);

            if (!string.IsNullOrEmpty(token))
            {
                AttachUserToContext(context, jwtService, token);
            }

            await _next(context);
        }

        private string ExtractToken(HttpContext context)
        {
            // 1. Intentar desde Header Authorization
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation("🔑 Token extraído desde Authorization Header");
                return token;
            }

            // 2. Intentar desde Query String (para SignalR)
            var queryToken = context.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                _logger.LogInformation("🔑 Token extraído desde Query String");
                return queryToken;
            }

            // 3. Intentar desde Cookie
            var cookieToken = context.Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogInformation("🔑 Token extraído desde Cookie");
                return cookieToken;
            }

            return null;
        }

        private void AttachUserToContext(HttpContext context, IJwtService jwtService, string token)
        {
            try
            {
                var principal = jwtService.ValidateToken(token);
                
                if (principal != null && principal.Identity.IsAuthenticated)
                {
                    context.User = principal;
                    
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                    var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
                    
                    _logger.LogInformation(
                        "✅ Usuario autenticado: ID={UserId}, Nombre={UserName}, Rol={UserRole}", 
                        userId, userName, userRole
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Token inválido o expirado");
                // No hacer nada - el usuario quedará sin autenticar
            }
        }
    }
}