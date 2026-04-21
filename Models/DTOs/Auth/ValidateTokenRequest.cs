namespace CasaDeLasTortas.Models.DTOs.Auth
{
    /// <summary>
    /// Request para validar un token JWT
    /// Usado en: AccountController.ValidateToken()
    /// </summary>
    public class ValidateTokenRequest
    {
        /// <summary>Token JWT a validar</summary>
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de validación de token
    /// </summary>
    public class ValidateTokenResponse
    {
        public bool Valid { get; set; }
        public string? Message { get; set; }
        public ValidatedUserInfo? User { get; set; }
    }

    /// <summary>
    /// Información del usuario extraída del token
    /// </summary>
    public class ValidatedUserInfo
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Rol { get; set; }
    }
}