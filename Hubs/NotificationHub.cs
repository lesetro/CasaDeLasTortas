using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CasaDeLasTortas.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real
    /// Permite comunicación bidireccional entre servidor y clientes
    /// </summary>
    [Authorize] // Solo usuarios autenticados pueden conectarse
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        // Diccionario estático para rastrear conexiones por usuario
        // En producción, usar Redis o similar para escalabilidad
        private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
        private static readonly object _lock = new();

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // ==================== EVENTOS DE CICLO DE VIDA ====================

        /// <summary>
        /// Se ejecuta cuando un cliente se conecta al hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                // Agregar conexión al diccionario
                lock (_lock)
                {
                    if (!_userConnections.ContainsKey(userId))
                    {
                        _userConnections[userId] = new HashSet<string>();
                    }
                    _userConnections[userId].Add(connectionId);
                }

                _logger.LogInformation(
                    "Usuario {UserId} conectado. ConnectionId: {ConnectionId}. Total conexiones: {Count}",
                    userId, connectionId, _userConnections[userId].Count
                );

                // Notificar al usuario que está conectado
                await Clients.Caller.SendAsync("OnConnected", new
                {
                    message = "Conectado exitosamente al servidor de notificaciones",
                    userId,
                    connectionId,
                    timestamp = DateTime.UtcNow
                });

                // Agregar al grupo de su rol
                var userRole = GetUserRole();
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(connectionId, userRole);
                    _logger.LogInformation("Usuario {UserId} agregado al grupo {Role}", userId, userRole);
                }
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Se ejecuta cuando un cliente se desconecta
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    if (_userConnections.ContainsKey(userId))
                    {
                        _userConnections[userId].Remove(connectionId);
                        if (_userConnections[userId].Count == 0)
                        {
                            _userConnections.Remove(userId);
                        }
                    }
                }

                if (exception != null)
                {
                    _logger.LogWarning(exception,
                        "Usuario {UserId} desconectado con error. ConnectionId: {ConnectionId}",
                        userId, connectionId
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "Usuario {UserId} desconectado normalmente. ConnectionId: {ConnectionId}",
                        userId, connectionId
                    );
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ==================== MÉTODOS PÚBLICOS (LLAMADOS POR CLIENTES) ====================

        /// <summary>
        /// Permite al cliente unirse a un grupo específico
        /// </summary>
        /// <param name="groupName">Nombre del grupo (ej: "Vendedores", "Compradores")</param>
        [HubMethodName("JoinGroup")]
        public async Task JoinGroupAsync(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "Usuario {UserId} se unió al grupo {GroupName}",
                GetUserId(), groupName
            );

            await Clients.Caller.SendAsync("GroupJoined", new
            {
                group = groupName,
                message = $"Te has unido al grupo {groupName}",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Permite al cliente salir de un grupo
        /// </summary>
        [HubMethodName("LeaveGroup")]
        public async Task LeaveGroupAsync(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation(
                "Usuario {UserId} salió del grupo {GroupName}",
                GetUserId(), groupName
            );

            await Clients.Caller.SendAsync("GroupLeft", new
            {
                group = groupName,
                message = $"Has salido del grupo {groupName}",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Envia un mensaje directo a otro usuario
        /// </summary>
        [HubMethodName("SendPrivateMessage")]
        public async Task SendPrivateMessageAsync(string toUserId, string message)
        {
            var fromUserId = GetUserId();
            var fromUserName = GetUserName();

            if (string.IsNullOrEmpty(toUserId))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "ID de usuario destino es requerido",
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            // Enviar mensaje al destinatario
            await Clients.User(toUserId).SendAsync("ReceivePrivateMessage", new
            {
                fromUserId,
                fromUserName,
                message,
                timestamp = DateTime.UtcNow
            });

            // Confirmar al remitente
            await Clients.Caller.SendAsync("MessageSent", new
            {
                toUserId,
                message = "Mensaje enviado correctamente",
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Mensaje privado de {FromUserId} a {ToUserId}",
                fromUserId, toUserId
            );
        }

        /// <summary>
        /// Ping para mantener la conexión viva
        /// </summary>
        [HubMethodName("Ping")]
        public async Task PingAsync()
        {
            await Clients.Caller.SendAsync("Pong", new
            {
                message = "Pong",
                timestamp = DateTime.UtcNow
            });
        }

        // ==================== MÉTODOS PARA NOTIFICACIONES ESPECÍFICAS ====================

        /// <summary>
        /// Notifica sobre un nuevo pago creado
        /// (llamado desde el servidor, no desde el cliente)
        /// </summary>
        public async Task NotificarNuevoPago(int pagoId, string compradorId, string vendedorId, decimal monto)
        {
            // Notificar al vendedor
            await Clients.User(vendedorId).SendAsync("NuevoPago", new
            {
                tipo = "nuevo_pago",
                pagoId,
                compradorId,
                monto,
                message = $"Nuevo pago recibido por ${monto:N2}",
                timestamp = DateTime.UtcNow
            });

            // Notificar al comprador
            await Clients.User(compradorId).SendAsync("PagoCreado", new
            {
                tipo = "pago_creado",
                pagoId,
                vendedorId,
                monto,
                message = $"Pago de ${monto:N2} registrado exitosamente",
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Notificación de pago {PagoId} enviada a vendedor {VendedorId} y comprador {CompradorId}",
                pagoId, vendedorId, compradorId
            );
        }

        /// <summary>
        /// Notifica cambio de estado de un pago
        /// </summary>
        public async Task NotificarCambioEstadoPago(int pagoId, string usuarioId, string nuevoEstado, string mensaje)
        {
            await Clients.User(usuarioId).SendAsync("EstadoPagoCambiado", new
            {
                tipo = "estado_pago_cambiado",
                pagoId,
                nuevoEstado,
                message = mensaje,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Notificación de cambio de estado del pago {PagoId} a {NuevoEstado} enviada a {UsuarioId}",
                pagoId, nuevoEstado, usuarioId
            );
        }

        /// <summary>
        /// Notifica sobre una nueva torta publicada (a todos los compradores)
        /// </summary>
        public async Task NotificarNuevaTorta(int tortaId, string nombreTorta, decimal precio, string categoría)
        {
            await Clients.Group("Comprador").SendAsync("NuevaTorta", new
            {
                tipo = "nueva_torta",
                tortaId,
                nombreTorta,
                precio,
                categoría,
                message = $"Nueva torta disponible: {nombreTorta}",
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Notificación de nueva torta {TortaId} enviada al grupo Comprador",
                tortaId
            );
        }

        /// <summary>
        /// Notifica cuando el stock de una torta está bajo
        /// </summary>
        public async Task NotificarStockBajo(int tortaId, string vendedorId, string nombreTorta, int stockActual)
        {
            await Clients.User(vendedorId).SendAsync("StockBajo", new
            {
                tipo = "stock_bajo",
                tortaId,
                nombreTorta,
                stockActual,
                message = $"Stock bajo de {nombreTorta}: solo quedan {stockActual} unidades",
                timestamp = DateTime.UtcNow,
                nivel = stockActual == 0 ? "critico" : "advertencia"
            });

            _logger.LogInformation(
                "Notificación de stock bajo de torta {TortaId} enviada a vendedor {VendedorId}",
                tortaId, vendedorId
            );
        }

        /// <summary>
        /// Envía una notificación general a todos los usuarios
        /// </summary>
        public async Task NotificarATodos(string titulo, string mensaje, string tipo = "info")
        {
            await Clients.All.SendAsync("NotificacionGeneral", new
            {
                tipo = "notificacion_general",
                titulo,
                mensaje,
                nivel = tipo, // info, warning, error, success
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notificación general enviada a todos: {Titulo}", titulo);
        }

        /// <summary>
        /// Envía notificación a un grupo específico (por rol)
        /// </summary>
        public async Task NotificarAGrupo(string grupo, string titulo, string mensaje, string tipo = "info")
        {
            await Clients.Group(grupo).SendAsync("NotificacionGrupo", new
            {
                tipo = "notificacion_grupo",
                grupo,
                titulo,
                mensaje,
                nivel = tipo,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notificación enviada al grupo {Grupo}: {Titulo}", grupo, titulo);
        }

        // ==================== MÉTODOS AUXILIARES PRIVADOS ====================

        /// <summary>
        /// Obtiene el ID del usuario actual del contexto
        /// </summary>
        private string GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el nombre del usuario actual
        /// </summary>
        private string GetUserName()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario Desconocido";
        }

        /// <summary>
        /// Obtiene el rol del usuario actual
        /// </summary>
        private string GetUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el email del usuario actual
        /// </summary>
        private string GetUserEmail()
        {
            return Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Verifica si un usuario está conectado actualmente
        /// </summary>
        public static bool IsUserConnected(string userId)
        {
            lock (_lock)
            {
                return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
            }
        }

        /// <summary>
        /// Obtiene todas las conexiones de un usuario
        /// </summary>
        public static IEnumerable<string> GetUserConnections(string userId)
        {
            lock (_lock)
            {
                return _userConnections.ContainsKey(userId) 
                    ? _userConnections[userId].ToList() 
                    : Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Obtiene estadísticas de conexiones
        /// </summary>
        public static object GetConnectionStats()
        {
            lock (_lock)
            {
                return new
                {
                    totalUsers = _userConnections.Count,
                    totalConnections = _userConnections.Values.Sum(c => c.Count),
                    timestamp = DateTime.UtcNow
                };
            }
        }
    }
}