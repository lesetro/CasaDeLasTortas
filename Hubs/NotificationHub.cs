using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CasaDeLasTortas.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real.
    ///
    /// GRUPOS AUTOMÁTICOS (asignados al conectar según el rol del JWT):
    ///   "Admin"     → administradores
    ///   "Vendedor"  → vendedores
    ///   "Comprador" → compradores
    ///
    /// EVENTOS QUE EMITE EL SERVIDOR:
    ///   → Admin:
    ///       "NuevoComprobanteSubido"   comprador subió un comprobante para revisar
    ///       "PagoRechazado"            admin rechazó un pago (copia al admin para log)
    ///       "EntregaConfirmada"        comprador confirmó recepción → liberar fondos
    ///       "NotificacionAdmin"        mensaje general al grupo Admin
    ///   → Vendedor:
    ///       "NuevoPedido"              llegó un nuevo pedido
    ///       "PagoConfirmado"           admin aprobó el comprobante del comprador
    ///       "FondosLiberados"          admin transfirió fondos al vendedor
    ///       "StockBajo"                torta con stock bajo
    ///   → Comprador:
    ///       "PagoVerificado"           admin aprobó su comprobante
    ///       "PagoRechazado"            admin rechazó su comprobante
    ///       "PedidoEnPreparacion"      vendedor empezó a preparar
    ///       "PedidoListo"              pedido listo para retirar
    ///       "NuevaTorta"               nueva torta publicada
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        // Mapa userId → conexiones activas (en producción usar Redis)
        private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
        private static readonly object _lock = new();

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════════
        // CICLO DE VIDA
        // ══════════════════════════════════════════════════════════════

        public override async Task OnConnectedAsync()
        {
            var userId     = GetUserId();
            var userRole   = GetUserRole();
            var connId     = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                // Registrar conexión
                lock (_lock)
                {
                    if (!_userConnections.ContainsKey(userId))
                        _userConnections[userId] = new HashSet<string>();
                    _userConnections[userId].Add(connId);
                }

                // Agregar al grupo del rol (Comprador / Vendedor / Admin)
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(connId, userRole);

                    // Los admins tienen su propio grupo específico para recibir
                    // notificaciones de nuevos comprobantes y liberaciones pendientes
                    if (userRole == "Admin")
                        await Groups.AddToGroupAsync(connId, "AdminPanel");
                }

                _logger.LogInformation(
                    "Usuario {UserId} ({Role}) conectado. ConnectionId: {ConnId}",
                    userId, userRole, connId);

                await Clients.Caller.SendAsync("OnConnected", new
                {
                    message    = "Conectado al servidor de notificaciones",
                    userId,
                    userRole,
                    connectionId = connId,
                    timestamp  = DateTime.UtcNow
                });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var connId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    if (_userConnections.TryGetValue(userId, out var conns))
                    {
                        conns.Remove(connId);
                        if (conns.Count == 0)
                            _userConnections.Remove(userId);
                    }
                }

                if (exception != null)
                    _logger.LogWarning(exception,
                        "Usuario {UserId} desconectado con error. ConnId: {ConnId}", userId, connId);
                else
                    _logger.LogInformation(
                        "Usuario {UserId} desconectado. ConnId: {ConnId}", userId, connId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ══════════════════════════════════════════════════════════════
        // MÉTODOS LLAMADOS POR LOS CLIENTES
        // ══════════════════════════════════════════════════════════════

        [HubMethodName("JoinGroup")]
        public async Task JoinGroupAsync(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Usuario {UserId} se unió al grupo {Group}", GetUserId(), groupName);
            await Clients.Caller.SendAsync("GroupJoined", new
            {
                group = groupName, timestamp = DateTime.UtcNow
            });
        }

        [HubMethodName("LeaveGroup")]
        public async Task LeaveGroupAsync(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("GroupLeft", new
            {
                group = groupName, timestamp = DateTime.UtcNow
            });
        }

        [HubMethodName("Ping")]
        public async Task PingAsync()
        {
            await Clients.Caller.SendAsync("Pong", new { timestamp = DateTime.UtcNow });
        }

        [HubMethodName("SendPrivateMessage")]
        public async Task SendPrivateMessageAsync(string toUserId, string message)
        {
            var fromUserId = GetUserId();
            await Clients.User(toUserId).SendAsync("ReceivePrivateMessage", new
            {
                fromUserId,
                fromUserName = GetUserName(),
                message,
                timestamp    = DateTime.UtcNow
            });
            await Clients.Caller.SendAsync("MessageSent", new
            {
                toUserId, timestamp = DateTime.UtcNow
            });
        }

        // ══════════════════════════════════════════════════════════════
        // NOTIFICACIONES — FLUJO DE PAGOS (llamadas desde servicios del backend)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// El comprador subió un comprobante de transferencia.
        /// → Notifica al grupo Admin para que vaya a revisarlo.
        /// → Notifica al comprador confirmando recepción.
        /// </summary>
        public async Task NotificarComprobanteSubido(
            int    pagoId,
            string compradorUserId,
            string compradorNombre,
            decimal monto,
            string  numeroOrden)
        {
            // Al panel de admin
            await Clients.Group("AdminPanel").SendAsync("NuevoComprobanteSubido", new
            {
                tipo           = "nuevo_comprobante",
                pagoId,
                compradorNombre,
                monto,
                numeroOrden,
                mensaje        = $"Nuevo comprobante de {compradorNombre} — Orden #{numeroOrden} — ${monto:N0}",
                timestamp      = DateTime.UtcNow
            });

            // Al comprador: confirmación de que llegó
            await Clients.User(compradorUserId).SendAsync("ComprobanteRecibido", new
            {
                pagoId,
                numeroOrden,
                mensaje   = "Tu comprobante fue recibido. Lo revisaremos en 1 día hábil.",
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Comprobante de pago {PagoId} subido por comprador {UserId} — ${Monto:N0}",
                pagoId, compradorUserId, monto);
        }

        /// <summary>
        /// Admin aprobó el comprobante.
        /// → Notifica al comprador que su pago fue verificado.
        /// → Notifica a cada vendedor involucrado que pueden preparar.
        /// </summary>
        public async Task NotificarPagoAprobado(
            int    pagoId,
            string compradorUserId,
            string compradorNombre,
            string numeroOrden,
            decimal monto,
            IEnumerable<string> vendedorUserIds)
        {
            // Al comprador
            await Clients.User(compradorUserId).SendAsync("PagoVerificado", new
            {
                tipo        = "pago_verificado",
                pagoId,
                numeroOrden,
                monto,
                mensaje     = $"¡Pago verificado! Tu pedido #{numeroOrden} pasó a preparación.",
                timestamp   = DateTime.UtcNow
            });

            // A cada vendedor de la orden
            foreach (var vendedorId in vendedorUserIds)
            {
                await Clients.User(vendedorId).SendAsync("PagoConfirmado", new
                {
                    tipo           = "pago_confirmado",
                    pagoId,
                    compradorNombre,
                    numeroOrden,
                    monto,
                    mensaje        = $"Pago confirmado para la orden #{numeroOrden}. ¡Podés empezar la preparación!",
                    timestamp      = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "Pago {PagoId} aprobado. Notificado a comprador {CompradorId} y {VendCount} vendedor(es)",
                pagoId, compradorUserId, vendedorUserIds.Count());
        }

        /// <summary>
        /// Admin rechazó el comprobante.
        /// → Notifica al comprador con el motivo.
        /// </summary>
        public async Task NotificarPagoRechazado(
            int    pagoId,
            string compradorUserId,
            string numeroOrden,
            string motivo)
        {
            await Clients.User(compradorUserId).SendAsync("PagoRechazado", new
            {
                tipo        = "pago_rechazado",
                pagoId,
                numeroOrden,
                motivo,
                mensaje     = $"Tu comprobante de la orden #{numeroOrden} fue rechazado: {motivo}",
                timestamp   = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Pago {PagoId} rechazado. Notificado al comprador {UserId}. Motivo: {Motivo}",
                pagoId, compradorUserId, motivo);
        }

        /// <summary>
        /// Vendedor marcó el pedido como "Listo para retirar".
        /// → Notifica al comprador.
        /// </summary>
        public async Task NotificarPedidoListo(
            int    detalleId,
            string compradorUserId,
            string nombreTorta,
            string numeroOrden)
        {
            await Clients.User(compradorUserId).SendAsync("PedidoListo", new
            {
                tipo       = "pedido_listo",
                detalleId,
                nombreTorta,
                numeroOrden,
                mensaje    = $"¡Tu pedido #{numeroOrden} está listo para retirar!",
                timestamp  = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Pedido {DetalleId} listo. Notificado al comprador {UserId}",
                detalleId, compradorUserId);
        }

        /// <summary>
        /// Comprador confirmó la entrega.
        /// → Notifica al Admin para que proceda a liberar fondos.
        /// → Notifica al vendedor que el comprador confirmó.
        /// </summary>
        public async Task NotificarEntregaConfirmada(
            int    ventaId,
            string compradorNombre,
            string numeroOrden,
            string vendedorUserId,
            decimal montoALiberar)
        {
            // Al admin
            await Clients.Group("AdminPanel").SendAsync("EntregaConfirmada", new
            {
                tipo           = "entrega_confirmada",
                ventaId,
                compradorNombre,
                numeroOrden,
                montoALiberar,
                mensaje        = $"Entrega confirmada — Orden #{numeroOrden} — Liberar ${montoALiberar:N0} al vendedor",
                timestamp      = DateTime.UtcNow
            });

            // Al vendedor
            await Clients.User(vendedorUserId).SendAsync("EntregaConfirmada", new
            {
                tipo           = "entrega_confirmada",
                ventaId,
                compradorNombre,
                numeroOrden,
                mensaje        = $"El comprador confirmó la recepción del pedido #{numeroOrden}",
                timestamp      = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Entrega confirmada — Orden #{Orden}. Admin notificado. Monto a liberar: ${Monto:N0}",
                numeroOrden, montoALiberar);
        }

        /// <summary>
        /// Admin liberó los fondos al vendedor.
        /// → Notifica al vendedor con el monto acreditado.
        /// </summary>
        public async Task NotificarFondosLiberados(
            int     liberacionId,
            string  vendedorUserId,
            string  vendedorNombre,
            string  numeroOrden,
            decimal montoLiberado,
            decimal comisionDescontada)
        {
            await Clients.User(vendedorUserId).SendAsync("FondosLiberados", new
            {
                tipo               = "fondos_liberados",
                liberacionId,
                numeroOrden,
                montoLiberado,
                comisionDescontada,
                monto              = montoLiberado,   // alias usado por AppVendedor.vue
                mensaje            = $"💸 Fondos acreditados — Orden #{numeroOrden} — ${montoLiberado:N0} transferidos a tu cuenta",
                timestamp          = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Fondos liberados al vendedor {Nombre} ({UserId}) — ${Monto:N0} — Orden #{Orden}",
                vendedorNombre, vendedorUserId, montoLiberado, numeroOrden);
        }

        /// <summary>
        /// Disputa abierta por un comprador.
        /// → Notifica al Admin.
        /// </summary>
        public async Task NotificarDisputaAbierta(
            int    disputaId,
            string compradorNombre,
            string numeroOrden,
            string motivo)
        {
            await Clients.Group("AdminPanel").SendAsync("DisputaAbierta", new
            {
                tipo            = "disputa_abierta",
                disputaId,
                compradorNombre,
                numeroOrden,
                motivo,
                mensaje         = $"🚨 Nueva disputa — Orden #{numeroOrden} — {compradorNombre}: {motivo}",
                timestamp       = DateTime.UtcNow
            });

            _logger.LogWarning(
                "Disputa {DisputaId} abierta para orden #{Orden} por {Comprador}",
                disputaId, numeroOrden, compradorNombre);
        }

        // ══════════════════════════════════════════════════════════════
        // NOTIFICACIONES DE PRODUCTOS
        // ══════════════════════════════════════════════════════════════

        /// <summary>Nueva torta publicada → notifica a todos los compradores.</summary>
        public async Task NotificarNuevaTorta(
            int     tortaId,
            string  nombreTorta,
            decimal precio,
            string  categoria)
        {
            await Clients.Group("Comprador").SendAsync("NuevaTorta", new
            {
                tipo       = "nueva_torta",
                tortaId,
                nombreTorta,
                precio,
                categoria,
                mensaje    = $"Nueva torta disponible: {nombreTorta}",
                timestamp  = DateTime.UtcNow
            });
        }

        /// <summary>Stock bajo de una torta → notifica al vendedor dueño.</summary>
        public async Task NotificarStockBajo(
            int    tortaId,
            string vendedorUserId,
            string nombreTorta,
            int    stockActual)
        {
            await Clients.User(vendedorUserId).SendAsync("StockBajo", new
            {
                tipo       = "stock_bajo",
                tortaId,
                nombreTorta,
                stockActual,
                message    = $"Stock bajo de '{nombreTorta}': quedan {stockActual} unidades",
                nivel      = stockActual == 0 ? "critico" : "advertencia",
                timestamp  = DateTime.UtcNow
            });
        }

        // ══════════════════════════════════════════════════════════════
        // MÉTODOS HEREDADOS / GENERALES
        // ══════════════════════════════════════════════════════════════

        /// <summary>Notificación de pago genérica (compatibilidad con código anterior).</summary>
        public async Task NotificarNuevoPago(
            int    pagoId,
            string compradorId,
            string vendedorId,
            decimal monto)
        {
            await Clients.User(vendedorId).SendAsync("NuevoPago", new
            {
                pagoId, compradorId, monto,
                message   = $"Nuevo pago recibido por ${monto:N2}",
                timestamp = DateTime.UtcNow
            });
            await Clients.User(compradorId).SendAsync("PagoCreado", new
            {
                pagoId, vendedorId, monto,
                message   = $"Pago de ${monto:N2} registrado exitosamente",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>Cambio de estado genérico de pago.</summary>
        public async Task NotificarCambioEstadoPago(
            int    pagoId,
            string usuarioId,
            string nuevoEstado,
            string mensaje)
        {
            await Clients.User(usuarioId).SendAsync("EstadoPagoCambiado", new
            {
                pagoId, nuevoEstado, message = mensaje, timestamp = DateTime.UtcNow
            });
        }

        /// <summary>Notificación a todos los usuarios.</summary>
        public async Task NotificarATodos(string titulo, string mensaje, string tipo = "info")
        {
            await Clients.All.SendAsync("NotificacionGeneral", new
            {
                titulo, mensaje, nivel = tipo, timestamp = DateTime.UtcNow
            });
        }

        /// <summary>Notificación a un grupo por nombre.</summary>
        public async Task NotificarAGrupo(
            string grupo, string titulo, string mensaje, string tipo = "info")
        {
            await Clients.Group(grupo).SendAsync("NotificacionGrupo", new
            {
                grupo, titulo, mensaje, nivel = tipo, timestamp = DateTime.UtcNow
            });
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ══════════════════════════════════════════════════════════════

        private string GetUserId()    => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        private string GetUserName()  => Context.User?.FindFirst(ClaimTypes.Name)?.Value            ?? "Desconocido";
        private string GetUserRole()  => Context.User?.FindFirst(ClaimTypes.Role)?.Value            ?? string.Empty;
        private string GetUserEmail() => Context.User?.FindFirst(ClaimTypes.Email)?.Value           ?? string.Empty;

        // ══════════════════════════════════════════════════════════════
        // MÉTODOS ESTÁTICOS DE CONSULTA (usados por otros servicios)
        // ══════════════════════════════════════════════════════════════

        public static bool IsUserConnected(string userId)
        {
            lock (_lock)
                return _userConnections.ContainsKey(userId)
                    && _userConnections[userId].Count > 0;
        }

        public static IEnumerable<string> GetUserConnections(string userId)
        {
            lock (_lock)
                return _userConnections.TryGetValue(userId, out var conns)
                    ? conns.ToList()
                    : Enumerable.Empty<string>();
        }

        public static object GetConnectionStats()
        {
            lock (_lock)
                return new
                {
                    totalUsers       = _userConnections.Count,
                    totalConnections = _userConnections.Values.Sum(c => c.Count),
                    timestamp        = DateTime.UtcNow
                };
        }
    }
}