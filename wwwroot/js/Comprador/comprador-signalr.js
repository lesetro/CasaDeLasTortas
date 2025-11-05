// comprador-signalr.js - Cliente SignalR para notificaciones en tiempo real del comprador

class CompradorSignalRClient {
    constructor() {
        this.connection = null;
        this.compradorId = null;
    }
    
    async inicializar(compradorId) {
        this.compradorId = compradorId;
        
        if (!window.signalR) {
            console.error('SignalR no está disponible');
            return;
        }
        
        try {
            // Crear conexión
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/notifications?access_token=' + window.authToken)
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60000) {
                            return Math.random() * 10000; // Retry entre 0-10 segundos
                        } else {
                            return null; // Dejar de reintentar después de 1 minuto
                        }
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();
            
            // Configurar manejadores de eventos
            this.configurarEventos();
            
            // Iniciar conexión
            await this.connection.start();
            console.log('✓ SignalR conectado - Comprador');
            
            // Unirse al grupo de compradores
            await this.connection.invoke('JoinGroup', 'Compradores');
            console.log('✓ Unido al grupo de Compradores');
            
        } catch (error) {
            console.error('Error conectando SignalR:', error);
            toastr.error('Error al conectar con el servidor de notificaciones');
        }
    }
    
    configurarEventos() {
        // Evento: Estado del Pedido Actualizado
        this.connection.on('EstadoPedidoActualizado', (data) => {
            console.log(' Estado del pedido actualizado:', data);
            
            let mensaje = '';
            let tipo = 'info';
            
            switch(data.nuevoEstado) {
                case 'Completado':
                    mensaje = `¡Tu pedido #${data.pedidoId} ha sido entregado!`;
                    tipo = 'success';
                    this.reproducirSonido('/sounds/order-completed.mp3');
                    break;
                case 'EnPreparacion':
                    mensaje = `El vendedor está preparando tu pedido #${data.pedidoId}`;
                    tipo = 'info';
                    break;
                case 'Cancelado':
                    mensaje = `Tu pedido #${data.pedidoId} ha sido cancelado`;
                    tipo = 'warning';
                    break;
                default:
                    mensaje = `Estado actualizado del pedido #${data.pedidoId}: ${data.nuevoEstado}`;
            }
            
            toastr[tipo](mensaje, 'Actualización de Pedido', {
                timeOut: 10000,
                progressBar: true,
                closeButton: true,
                onclick: function() {
                    // Navegar al historial de compras
                    window.location.hash = '#historial';
                }
            });
            
            // Emitir evento personalizado para que Vue lo escuche
            window.dispatchEvent(new CustomEvent('estadoPedidoActualizado', { detail: data }));
        });
        
        // Evento: Mensaje del Vendedor
        this.connection.on('MensajeVendedor', (data) => {
            console.log(' Mensaje del vendedor:', data);
            
            toastr.info(
                data.mensaje,
                ` Mensaje de ${data.nombreVendedor}`,
                {
                    timeOut: 15000,
                    progressBar: true,
                    closeButton: true
                }
            );
        });
        
        // Evento: Nueva Torta Disponible (según preferencias)
        this.connection.on('NuevaTortaDisponible', (data) => {
            console.log('🎂 Nueva torta disponible:', data);
            
            toastr.info(
                `Nueva torta "${data.nombre}" disponible por ${data.vendedor}`,
                '¡Nueva Torta!',
                {
                    timeOut: 10000,
                    progressBar: true,
                    closeButton: true,
                    onclick: function() {
                        // Navegar al catálogo
                        window.location.hash = '#catalogo';
                    }
                }
            );
        });
        
        // Evento: Oferta Especial
        this.connection.on('OfertaEspecial', (data) => {
            console.log(' Oferta especial:', data);
            
            toastr.warning(
                `¡Oferta especial! ${data.descripcion}`,
                ' ¡Oferta Limitada!',
                {
                    timeOut: 12000,
                    progressBar: true,
                    closeButton: true
                }
            );
        });
        
        // Eventos de reconexión
        this.connection.onreconnecting((error) => {
            console.warn(' Reconectando SignalR...', error);
            toastr.warning('Reconectando con el servidor...', 'Conexión', {
                timeOut: 0,
                extendedTimeOut: 0
            });
        });
        
        this.connection.onreconnected((connectionId) => {
            console.log('✓ Reconectado a SignalR:', connectionId);
            toastr.success('Conexión restablecida', 'Conexión');
        });
        
        this.connection.onclose((error) => {
            console.error(' Conexión SignalR cerrada:', error);
            toastr.error('Se perdió la conexión con el servidor', 'Error de Conexión', {
                timeOut: 0,
                extendedTimeOut: 0,
                closeButton: true
            });
        });
    }
    
    reproducirSonido(url) {
        try {
            const audio = new Audio(url);
            audio.volume = 0.3;
            audio.play().catch(error => {
                console.log('No se pudo reproducir el sonido:', error);
            });
        } catch (error) {
            console.log('Error reproduciendo sonido:', error);
        }
    }
    
    async enviarMensaje(metodo, ...args) {
        if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
            console.error('SignalR no está conectado');
            return;
        }
        
        try {
            return await this.connection.invoke(metodo, ...args);
        } catch (error) {
            console.error('Error invocando método SignalR:', error);
            throw error;
        }
    }
    
    async detener() {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log('✓ SignalR desconectado');
            } catch (error) {
                console.error('Error deteniendo SignalR:', error);
            }
        }
    }
}

// Crear instancia global
window.compradorSignalR = new CompradorSignalRClient();

// Auto-inicializar cuando tengamos el compradorId
// (se llamará desde CompradorDashboard después de cargar los datos del comprador)