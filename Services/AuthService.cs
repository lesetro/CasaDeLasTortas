using CasaDeLasTortas.Data;
using CasaDeLasTortas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CasaDeLasTortas.Services
{
    public interface IAuthService
    {
        Task<Persona?> ValidateCredentialsAsync(string email, string password);
        Task<Persona> RegisterAsync(Persona persona, string password);
        Task<Persona?> GetUserByIdAsync(int id);
        Task<Persona?> GetUserByEmailAsync(string email);
        Task<bool> UpdatePasswordAsync(int personaId, string currentPassword, string newPassword);
        Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword);
        Task<bool> IsEmailAvailableAsync(string email);
        Task UpdateLastAccessAsync(int personaId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        public async Task<Persona?> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                var persona = await _context.Personas
                    .Include(p => p.Vendedor)
                    .Include(p => p.Comprador)
                    .FirstOrDefaultAsync(p => p.Email == email && p.Activo);

                if (persona == null)
                {
                    return null;
                }

                // Verificar contraseña
                if (!VerifyPassword(password, persona.PasswordHash))
                {
                    return null;
                }

                // Actualizar último acceso
                persona.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                return persona;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar credenciales para {email}");
                throw;
            }
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<Persona> RegisterAsync(Persona persona, string password)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await _context.Personas
                    .FirstOrDefaultAsync(p => p.Email == persona.Email);

                if (existingUser != null)
                {
                    throw new InvalidOperationException("El email ya está registrado");
                }

                // Hash de la contraseña
                persona.PasswordHash = HashPassword(password);
                persona.FechaRegistro = DateTime.Now;
                persona.Activo = true;

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario registrado: {persona.Email}");

                return persona;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar usuario: {persona.Email}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public async Task<Persona?> GetUserByIdAsync(int id)
        {
            return await _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        public async Task<Persona?> GetUserByEmailAsync(string email)
        {
            return await _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Email == email && p.Activo);
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            try
            {
                var persona = await _context.Personas.FindAsync(personaId);

                if (persona == null)
                {
                    return false;
                }

                // Verificar contraseña actual
                if (!VerifyPassword(currentPassword, persona.PasswordHash))
                {
                    return false;
                }

                // Actualizar contraseña
                persona.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña actualizada para usuario: {persona.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar contraseña del usuario: {personaId}");
                throw;
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario (alias de UpdatePasswordAsync)
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            return await UpdatePasswordAsync(personaId, currentPassword, newPassword);
        }

        /// <summary>
        /// Verifica si un email está disponible
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var existingUser = await _context.Personas
                .FirstOrDefaultAsync(p => p.Email == email);
            
            return existingUser == null;
        }

        /// <summary>
        /// Actualiza la fecha del último acceso
        /// </summary>
        public async Task UpdateLastAccessAsync(int personaId)
        {
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona != null)
            {
                persona.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Genera un hash de la contraseña
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifica una contraseña contra su hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}