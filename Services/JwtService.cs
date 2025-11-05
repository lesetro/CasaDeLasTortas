using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CasaDeLasTortas.Models.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CasaDeLasTortas.Services
{
    public interface IJwtService
    {
        string GenerateToken(Persona persona);
        ClaimsPrincipal? ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        public string GenerateToken(Persona persona)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada"))
                );

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, persona.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, persona.Email),
                    new Claim(ClaimTypes.Name, persona.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, persona.Id.ToString()),
                    new Claim(ClaimTypes.Role, persona.Rol),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("PersonaId", persona.Id.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"] ?? "24")),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation($"Token generado para usuario: {persona.Email}");

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar token para: {persona.Email}");
                throw;
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada"));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token inválido");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario desde un token
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                
                if (principal == null)
                {
                    return null;
                }

                var userIdClaim = principal.FindFirst("PersonaId") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener UserId del token");
                return null;
            }
        }
    }
}