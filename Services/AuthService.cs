using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
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
        Task<Persona?> LoginAsync(string email, string password, bool mergeCarrito = true);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);

        // ── NUEVO ──
        Task<bool> ResetPasswordDirectAsync(string email, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly ICarritoService _carritoService;

        public AuthService(
            ApplicationDbContext context, 
            ILogger<AuthService> logger,
            ICarritoService carritoService)
        {
            _context = context;
            _logger = logger;
            _carritoService = carritoService;
        }

        // ══════════════════════════════════════════════════════
        //  NUEVO: Resetear contraseña directamente por email
        // ══════════════════════════════════════════════════════
        public async Task<bool> ResetPasswordDirectAsync(string email, string newPassword)
        {
            try
            {
                var persona = await _context.Personas
                    .FirstOrDefaultAsync(p => p.Email == email && p.Activo);

                if (persona == null)
                    return false;

                persona.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña reseteada para: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña para {Email}", email);
                throw;
            }
        }
        // ══════════════════════════════════════════════════════


        public async Task<Persona?> LoginAsync(string email, string password, bool mergeCarrito = true)
        {
            try
            {
                _logger.LogInformation("Intento de login para: {Email}", email);

                var persona = await ValidateCredentialsAsync(email, password);
                
                if (persona == null)
                {
                    _logger.LogWarning("Credenciales inválidas para: {Email}", email);
                    return null;
                }

                if (mergeCarrito && persona.Rol == "Comprador" && persona.Comprador != null)
                {
                    await FusionarCarritoAnonimo(persona.Comprador.Id);
                }

                _logger.LogInformation("Login exitoso para: {Email}", email);
                return persona;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoginAsync para: {Email}", email);
                throw;
            }
        }

        private async Task FusionarCarritoAnonimo(int compradorId)
        {
            try
            {
                _logger.LogInformation("Fusionando carrito anónimo para comprador {CompradorId}", compradorId);
                
                var carritoAnonimo = _carritoService.ObtenerCarrito();

                if (carritoAnonimo.Items.Any())
                {
                    _logger.LogInformation("Fusionando {Count} items del carrito anónimo", 
                        carritoAnonimo.Items.Count);

                    await _carritoService.FusionarCarritos(carritoAnonimo, compradorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al fusionar carrito para comprador {CompradorId}", compradorId);
            }
        }

        public async Task<Persona?> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                var persona = await _context.Personas
                    .Include(p => p.Vendedor)
                    .Include(p => p.Comprador)
                    .FirstOrDefaultAsync(p => p.Email == email && p.Activo);

                if (persona == null)
                    return null;

                if (!VerifyPassword(password, persona.PasswordHash))
                    return null;

                persona.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                return persona;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar credenciales para {Email}", email);
                throw;
            }
        }

        public async Task<Persona> RegisterAsync(Persona persona, string password)
        {
            try
            {
                var existingUser = await _context.Personas
                    .FirstOrDefaultAsync(p => p.Email == persona.Email);

                if (existingUser != null)
                    throw new InvalidOperationException("El email ya está registrado");

                persona.PasswordHash = HashPassword(password);
                persona.FechaRegistro = DateTime.Now;
                persona.Activo = true;

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado: {Email}", persona.Email);
                return persona;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario: {Email}", persona.Email);
                throw;
            }
        }

        public async Task<Persona?> GetUserByIdAsync(int id)
        {
            return await _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
        }

        public async Task<Persona?> GetUserByEmailAsync(string email)
        {
            return await _context.Personas
                .Include(p => p.Vendedor)
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Email == email && p.Activo);
        }

        public async Task<bool> UpdatePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            try
            {
                var persona = await _context.Personas.FindAsync(personaId);

                if (persona == null)
                    return false;

                if (!VerifyPassword(currentPassword, persona.PasswordHash))
                    return false;

                persona.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña actualizada para usuario: {Email}", persona.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar contraseña del usuario: {PersonaId}", personaId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            return await UpdatePasswordAsync(personaId, currentPassword, newPassword);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var existingUser = await _context.Personas
                .FirstOrDefaultAsync(p => p.Email == email);
            
            return existingUser == null;
        }

        public async Task UpdateLastAccessAsync(int personaId)
        {
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona != null)
            {
                persona.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

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