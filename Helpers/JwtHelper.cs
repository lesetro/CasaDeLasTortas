using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Helpers
{
    /// <summary>
    /// Helper para generar, validar y manipular tokens JWT
    /// JSON Web Tokens para autenticación stateless
    /// </summary>
    public class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationHours;

        /// <summary>
        /// Constructor del helper JWT
        /// </summary>
        /// <param name="secretKey">Clave secreta para firmar tokens (debe ser >= 32 caracteres)</param>
        /// <param name="issuer">Emisor del token (generalmente el nombre de tu app)</param>
        /// <param name="audience">Audiencia del token (quién puede usarlo)</param>
        /// <param name="expirationHours">Horas de validez del token</param>
        public JwtHelper(string secretKey, string issuer, string audience, int expirationHours = 8)
        {
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            {
                throw new ArgumentException("La clave secreta debe tener al menos 32 caracteres");
            }

            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _expirationHours = expirationHours;
        }

        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        /// <param name="persona">Usuario para el cual generar el token</param>
        /// <returns>Token JWT como string</returns>
        public string GenerarToken(Persona persona)
{
    // Crear los claims (información del usuario dentro del token)
    var claims = new List<Claim>
    {
        // Claim estándar de identificador de usuario - CORREGIDO: IdPersona → Id
        new Claim(ClaimTypes.NameIdentifier, persona.Id.ToString()),
        
        // Claim estándar de nombre - CORREGIDO: Eliminado Apellido ya que no existe
        new Claim(ClaimTypes.Name, persona.Nombre),
        
        // Claim estándar de email
        new Claim(ClaimTypes.Email, persona.Email),
        
        // Claim estándar de rol
        new Claim(ClaimTypes.Role, persona.Rol),
        
        // Claims personalizados - CORREGIDO: Eliminado Apellido
        new Claim("Nombre", persona.Nombre),
        new Claim("Activo", persona.Activo.ToString()),
        
        // Claim de tiempo de emisión (issued at)
        new Claim(JwtRegisteredClaimNames.Iat, 
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        
        // Identificador único del token
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    
    

            // Agregar avatar si existe
            if (!string.IsNullOrEmpty(persona.Avatar))
            {
                claims.Add(new Claim("Avatar", persona.Avatar));
            }

            // Crear la clave de seguridad
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            
            // Crear las credenciales de firma
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(_expirationHours),
                signingCredentials: credentials
            );

            // Serializar el token a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>ClaimsPrincipal si es válido, null si no</returns>
        public ClaimsPrincipal? ValidarToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true, // Verifica que no haya expirado
                    ClockSkew = TimeSpan.Zero // Sin tolerancia de tiempo
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extrae el ID del usuario de un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>ID del usuario o null si no se puede extraer</returns>
        public int? ObtenerUserIdDeToken(string token)
        {
            var principal = ValidarToken(token);
            if (principal == null) return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            return int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }

        /// <summary>
        /// Extrae el rol del usuario de un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Rol del usuario o null</returns>
        public string? ObtenerRolDeToken(string token)
        {
            var principal = ValidarToken(token);
            return principal?.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Extrae el email del usuario de un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Email del usuario o null</returns>
        public string? ObtenerEmailDeToken(string token)
        {
            var principal = ValidarToken(token);
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Verifica si un token ha expirado
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>True si ha expirado, False si aún es válido</returns>
        public bool TokenHaExpirado(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; // Si no se puede leer, considerarlo expirado
            }
        }

        /// <summary>
        /// Obtiene la fecha de expiración de un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Fecha de expiración o null</returns>
        public DateTime? ObtenerFechaExpiracion(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decodifica un token sin validarlo (útil para debugging)
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Diccionario con los claims del token</returns>
        public Dictionary<string, string> DecodificarTokenSinValidar(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.Claims.ToDictionary(
                    c => c.Type,
                    c => c.Value
                );
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Extrae el token Bearer de un header Authorization
        /// </summary>
        /// <param name="authorizationHeader">Header completo (ej: "Bearer eyJhbGc...")</param>
        /// <returns>Token sin el prefijo Bearer o null</returns>
        public static string? ExtraerTokenDeHeader(string? authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            // Verificar que tenga el formato "Bearer {token}"
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        /// <summary>
        /// Verifica si un usuario tiene un rol específico según su token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="rolRequerido">Rol a verificar</param>
        /// <returns>True si el usuario tiene el rol</returns>
        public bool UsuarioTieneRol(string token, string rolRequerido)
        {
            var rol = ObtenerRolDeToken(token);
            return rol?.Equals(rolRequerido, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Verifica si un usuario tiene alguno de los roles especificados
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="rolesPermitidos">Lista de roles permitidos</param>
        /// <returns>True si el usuario tiene al menos uno de los roles</returns>
        public bool UsuarioTieneAlgunRol(string token, params string[] rolesPermitidos)
        {
            var rol = ObtenerRolDeToken(token);
            if (string.IsNullOrEmpty(rol)) return false;

            return rolesPermitidos.Any(r => 
                r.Equals(rol, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Calcula cuánto tiempo falta para que expire un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>TimeSpan con el tiempo restante, o TimeSpan.Zero si ya expiró</returns>
        public TimeSpan TiempoRestanteToken(string token)
        {
            var fechaExpiracion = ObtenerFechaExpiracion(token);
            if (fechaExpiracion == null) return TimeSpan.Zero;

            var tiempoRestante = fechaExpiracion.Value - DateTime.UtcNow;
            return tiempoRestante > TimeSpan.Zero ? tiempoRestante : TimeSpan.Zero;
        }
    }
}