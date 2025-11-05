// vendedor-signalr.js - Cliente SignalR para notificaciones en tiempo real del vendedor

class VendedorSignalRClient {
    constructor() {
        this.connection = null;
        this.vendedorId = null;
    }
    
    async inicializar(vendedorId) {
        this.vendedorId = vendedorId;
        
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
            console.log('✓ SignalR conectado - Vendedor');
            
            // Unirse al grupo de vendedores
            await this.connection.invoke('JoinGroup', 'Vendedores');
            console.log('✓ Unido al grupo de Vendedores');
            
        } catch (error) {
            console.error('Error conectando SignalR:', error);
            toastr.error('Error al conectar con el servidor de notificaciones');
        }
    }
    
    configurarEventos() {
        // Evento: Nuevo Pedido (EL MÁS IMPORTANTE)
        this.connection.on('NuevoPedido', (data) => {
            console.log('📦 Nuevo pedido recibido:', data);
            
            // Reproducir sonido
            this.reproducirSonido('/sounds/new-order.mp3');
            
            // Mostrar notificación toast
            toastr.success(
                `${data.compradorNombre} pidió ${data.tortaNombre} por $${data.monto}`,
                '🎂 ¡Nuevo Pedido!',
                {
                    timeOut: 15000,
                    progressBar: true,
                    closeButton: true,
                    onclick: function() {
                        // Navegar a la vista de pedidos
                        window.location.hash = '#pedidos';
                    }
                }
            );
            
            // Emitir evento personalizado para que Vue lo escuche
            window.dispatchEvent(new CustomEvent('nuevoPedido', { detail: data }));
        });
        
        // Evento: Pedido Cancelado
        this.connection.on('PedidoCancelado', (data) => {
            console.log('❌ Pedido cancelado:', data);
            
            toastr.warning(
                `${data.compradorNombre} canceló su pedido de ${data.tortaNombre}`,
                'Pedido Cancelado',
                {
                    timeOut: 10000
                }
            );
            
            window.dispatchEvent(new CustomEvent('pedidoCancelado', { detail: data }));
        });
        
        // Evento: Stock Bajo
        this.connection.on('StockBajo', (data) => {
            console.log('⚠️ Stock bajo:', data);
            
            toastr.warning(
                `La torta "${data.nombreTorta}" tiene solo ${data.stockActual} unidades`,
                '⚠️ Stock Bajo',
                {
                    timeOut: 0,
                    extendedTimeOut: 0,
                    closeButton: true
                }
            );
        });
        
        // Evento: Nueva Calificación
        this.connection.on('NuevaCalificacion', (data) => {
            console.log('⭐ Nueva calificación:', data);
            
            const estrellas = '⭐'.repeat(data.calificacion);
            toastr.info(
                `${data.compradorNombre} te calificó con ${estrellas} (${data.calificacion}/5)`,
                'Nueva Calificación',
                {
                    timeOut: 10000
                }
            );
        });
        
        // Evento: Mensaje Privado (Chat)
        this.connection.on('ReceiveChatMessage', (data) => {
            console.log('💬 Nuevo mensaje:', data);
            
            toastr.info(
                data.message.substring(0, 50) + '...',
                `💬 ${data.fromUserName}`,
                {
                    timeOut: 8000
                }
            );
        });
        
        // Eventos de reconexión
        this.connection.onreconnecting((error) => {
            console.warn('⚠️ Reconectando SignalR...', error);
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
            console.error('❌ Conexión SignalR cerrada:', error);
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
            audio.volume = 0.5;
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
window.vendedorSignalR = new VendedorSignalRClient();

// Auto-inicializar cuando tengamos el vendedorId
// (se llamará desde VendedorDashboard.vue después de cargar los datos del vendedor)
