using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace CasaDeLasTortas.Services
{
    public interface INotificationService
    {
        Task EnviarAsync(string? token, string titulo, string cuerpo);
    }

    /// <summary>
    /// Envía notificaciones push a un dispositivo vía Firebase Cloud Messaging.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task EnviarAsync(string? token, string titulo, string cuerpo)
        {
            // Sin token o sin Firebase inicializado → no hacemos nada (no rompe el flujo)
            if (string.IsNullOrWhiteSpace(token) || FirebaseApp.DefaultInstance == null)
                return;

            try
            {
                var message = new Message
                {
                    Token = token,
                    Notification = new Notification { Title = titulo, Body = cuerpo }
                };
                var id = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Push enviado: {Id}", id);
            }
            catch (Exception ex)
            {
                // Token inválido/expirado u otro error → solo log (no interrumpe la operación)
                _logger.LogWarning(ex, "No se pudo enviar la notificación push");
            }
        }
    }
}
