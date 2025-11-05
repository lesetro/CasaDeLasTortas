// VendedorDashboard.vue - Componente principal del dashboard del vendedor
const VendedorDashboard = {
    template: `
    <div class="vendedor-dashboard">
        <!-- Navbar -->
        <nav class="navbar navbar-expand-lg navbar-dark bg-success sticky-top shadow">
            <div class="container-fluid">
                <a class="navbar-brand" href="#">
                    <i class="fas fa-store me-2"></i>
                    Mi Negocio - Casa de las Tortas
                </a>
                
                <div class="navbar-nav ms-auto">
                    <!-- Notificaciones -->
                    <div class="nav-item dropdown me-3">
                        <a class="nav-link position-relative" href="#" role="button" 
                           data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="fas fa-bell fa-lg text-white"></i>
                            <span v-if="notificacionesNoLeidas > 0" 
                                  class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                                {{ notificacionesNoLeidas }}
                            </span>
                        </a>
                        <ul class="dropdown-menu dropdown-menu-end" style="min-width: 300px;">
                            <li class="dropdown-header">
                                <strong>Notificaciones</strong>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                            <li v-if="notificaciones.length === 0" class="dropdown-item text-muted">
                                No hay notificaciones nuevas
                            </li>
                            <li v-for="notif in notificaciones" :key="notif.id" 
                                class="dropdown-item" @click="abrirNotificacion(notif)">
                                <div class="d-flex">
                                    <i :class="notif.icon" class="me-2 mt-1"></i>
                                    <div>
                                        <strong>{{ notif.titulo }}</strong>
                                        <p class="mb-0 small text-muted">{{ notif.mensaje }}</p>
                                        <small class="text-muted">{{ notif.tiempo }}</small>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>
                    
                    <!-- Usuario -->
                    <div class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle text-white" href="#" role="button" 
                           data-bs-toggle="dropdown">
                            <img v-if="vendedor.avatar" 
                                 :src="vendedor.avatar" 
                                 class="rounded-circle me-2" 
                                 style="width: 32px; height: 32px; object-fit: cover;">
                            <i v-else class="fas fa-user-circle me-1"></i>
                            {{ vendedor.nombreComercial || 'Vendedor' }}
                        </a>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item" href="#" @click.prevent="cambiarVista('perfil')">
                                    <i class="fas fa-user me-2"></i>Mi Perfil
                                </a>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                            <li>
                                <a class="dropdown-item text-danger" href="#" @click.prevent="logout">
                                    <i class="fas fa-sign-out-alt me-2"></i>Cerrar Sesión
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </nav>

        <!-- Main Content -->
        <div class="container-fluid mt-4">
            <div class="row">
                <!-- Sidebar -->
                <div class="col-md-3 col-lg-2 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <nav class="nav flex-column">
                                <button class="btn btn-outline-success mb-2 text-start" 
                                        :class="{ 'active': vistaActiva === 'estadisticas' }"
                                        @click="cambiarVista('estadisticas')">
                                    <i class="fas fa-chart-line me-2"></i>Estadísticas
                                </button>
                                <button class="btn btn-outline-success mb-2 text-start"
                                        :class="{ 'active': vistaActiva === 'tortas' }"
                                        @click="cambiarVista('tortas')">
                                    <i class="fas fa-birthday-cake me-2"></i>Mis Tortas
                                </button>
                                <button class="btn btn-outline-success mb-2 text-start"
                                        :class="{ 'active': vistaActiva === 'pedidos' }"
                                        @click="cambiarVista('pedidos')">
                                    <i class="fas fa-shopping-bag me-2"></i>Pedidos
                                    <span v-if="pedidosPendientes > 0" 
                                          class="badge bg-warning text-dark ms-2">
                                        {{ pedidosPendientes }}
                                    </span>
                                </button>
                                <button class="btn btn-outline-success mb-2 text-start"
                                        :class="{ 'active': vistaActiva === 'perfil' }"
                                        @click="cambiarVista('perfil')">
                                    <i class="fas fa-user me-2"></i>Mi Perfil
                                </button>
                            </nav>
                        </div>
                    </div>

                    <!-- Stats Resumen -->
                    <div class="card mt-3 shadow-sm">
                        <div class="card-body">
                            <h6 class="card-title text-muted mb-3">
                                <i class="fas fa-chart-pie me-2"></i>Resumen
                            </h6>
                            <div class="d-flex justify-content-between mb-2">
                                <span class="text-muted">Tortas activas:</span>
                                <strong class="text-success">{{ estadisticas.totalTortasActivas }}</strong>
                            </div>
                            <div class="d-flex justify-content-between mb-2">
                                <span class="text-muted">Ventas del mes:</span>
                                <strong class="text-primary">{{ estadisticas.ventasMes }}</strong>
                            </div>
                            <div class="d-flex justify-content-between mb-2">
                                <span class="text-muted">Ingresos totales:</span>
                                <strong class="text-success">\${{ estadisticas.ingresosTotales.toFixed(2) }}</strong>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span class="text-muted">Pedidos pendientes:</span>
                                <strong class="text-warning">{{ pedidosPendientes }}</strong>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Main Content Area -->
                <div class="col-md-9 col-lg-10">
                    <!-- Vista Dinámica -->
                    <component :is="componenteActivo" 
                               :vendedor-id="vendedor.id"
                               @torta-creada="onTortaCreada"
                               @torta-editada="onTortaEditada"
                               @torta-eliminada="onTortaEliminada"
                               @pedido-actualizado="onPedidoActualizado"
                               @ver-detalle-pedido="verDetallePedido">
                    </component>
                </div>
            </div>
        </div>

        <!-- Modal Detalle Pedido -->
        <DetallePedido 
            v-if="pedidoSeleccionado"
            :pedido="pedidoSeleccionado"
            @cerrar="pedidoSeleccionado = null"
            @estado-cambiado="onEstadoPedidoCambiado">
        </DetallePedido>
    </div>
    `,
    
    data() {
        return {
            vistaActiva: 'estadisticas',
            vendedor: {
                id: null,
                nombreComercial: '',
                especialidad: '',
                avatar: null,
                personaId: null
            },
            estadisticas: {
                totalTortasActivas: 0,
                ventasMes: 0,
                ingresosTotales: 0
            },
            pedidosPendientes: 0,
            notificaciones: [],
            notificacionesNoLeidas: 0,
            pedidoSeleccionado: null,
            signalrConnection: null
        }
    },
    
    computed: {
        componenteActivo() {
            const vistas = {
                'estadisticas': 'EstadisticasVendedor',
                'tortas': 'MisTortas',
                'pedidos': 'MisPedidos',
                'perfil': 'PerfilVendedor'
            };
            return vistas[this.vistaActiva] || 'EstadisticasVendedor';
        }
    },
    
    async mounted() {
        await this.inicializarDashboard();
        await this.conectarSignalR();
    },
    
    methods: {
        async inicializarDashboard() {
            try {
                await this.cargarDatosVendedor();
                await this.cargarEstadisticas();
                await this.cargarPedidosPendientes();
            } catch (error) {
                console.error('Error inicializando dashboard:', error);
                toastr.error('Error al cargar el dashboard');
            }
        },
        
        async cargarDatosVendedor() {
            try {
                const response = await fetch('/api/vendedor/mi-perfil', {
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    }
                });
                
                if (!response.ok) throw new Error('Error al cargar datos del vendedor');
                
                const data = await response.json();
                this.vendedor = {
                    id: data.id,
                    nombreComercial: data.nombreComercial,
                    especialidad: data.especialidad,
                    avatar: data.avatar,
                    personaId: data.personaId
                };
            } catch (error) {
                console.error('Error cargando vendedor:', error);
            }
        },
        
        async cargarEstadisticas() {
            try {
                const response = await fetch(`/api/vendedor/${this.vendedor.id}/estadisticas`, {
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    }
                });
                
                if (!response.ok) throw new Error('Error al cargar estadísticas');
                
                this.estadisticas = await response.json();
            } catch (error) {
                console.error('Error cargando estadísticas:', error);
            }
        },
        
        async cargarPedidosPendientes() {
            try {
                const response = await fetch(
                    `/api/pago/vendedor/${this.vendedor.id}?estado=Pendiente&pagina=1&registrosPorPagina=100`,
                    {
                        headers: {
                            'Authorization': `Bearer ${window.authToken}`,
                            'Content-Type': 'application/json'
                        }
                    }
                );
                
                if (!response.ok) throw new Error('Error al cargar pedidos');
                
                const data = await response.json();
                this.pedidosPendientes = data.totalRegistros || 0;
            } catch (error) {
                console.error('Error cargando pedidos pendientes:', error);
            }
        },
        
        async conectarSignalR() {
            if (!window.signalR) {
                console.error('SignalR no está disponible');
                return;
            }
            
            try {
                this.signalrConnection = new signalR.HubConnectionBuilder()
                    .withUrl('/hubs/notifications?access_token=' + window.authToken)
                    .withAutomaticReconnect()
                    .configureLogging(signalR.LogLevel.Information)
                    .build();
                
                // Evento: Nuevo Pedido
                this.signalrConnection.on('NuevoPedido', (data) => {
                    this.onNuevoPedido(data);
                });
                
                // Evento: Pedido Cancelado
                this.signalrConnection.on('PedidoCancelado', (data) => {
                    this.onPedidoCancelado(data);
                });
                
                await this.signalrConnection.start();
                console.log('SignalR conectado exitosamente');
            } catch (error) {
                console.error('Error conectando SignalR:', error);
            }
        },
        
        onNuevoPedido(data) {
            // Reproducir sonido
            const audio = new Audio('/sounds/new-order.mp3');
            audio.play().catch(() => {});
            
            // Mostrar notificación toast
            toastr.success(
                `${data.compradorNombre} pidió ${data.tortaNombre} por $${data.monto}`,
                '🎂 ¡Nuevo Pedido!',
                {
                    timeOut: 10000,
                    onclick: () => {
                        this.cambiarVista('pedidos');
                    }
                }
            );
            
            // Agregar a notificaciones
            this.notificaciones.unshift({
                id: Date.now(),
                icon: 'fas fa-shopping-bag text-success',
                titulo: 'Nuevo Pedido',
                mensaje: `${data.compradorNombre} - ${data.tortaNombre}`,
                tiempo: 'Ahora',
                data: data
            });
            
            this.notificacionesNoLeidas++;
            this.pedidosPendientes++;
            
            // Actualizar estadísticas
            this.cargarEstadisticas();
        },
        
        onPedidoCancelado(data) {
            toastr.warning(
                `${data.compradorNombre} canceló su pedido`,
                'Pedido Cancelado'
            );
            
            this.pedidosPendientes = Math.max(0, this.pedidosPendientes - 1);
        },
        
        cambiarVista(vista) {
            this.vistaActiva = vista;
        },
        
        verDetallePedido(pedido) {
            this.pedidoSeleccionado = pedido;
        },
        
        abrirNotificacion(notif) {
            this.notificacionesNoLeidas = Math.max(0, this.notificacionesNoLeidas - 1);
            
            if (notif.data && notif.data.pagoId) {
                this.cambiarVista('pedidos');
            }
        },
        
        onTortaCreada() {
            this.cargarEstadisticas();
            toastr.success('Torta creada exitosamente');
        },
        
        onTortaEditada() {
            this.cargarEstadisticas();
            toastr.success('Torta actualizada exitosamente');
        },
        
        onTortaEliminada() {
            this.cargarEstadisticas();
            toastr.success('Torta eliminada exitosamente');
        },
        
        onPedidoActualizado() {
            this.cargarPedidosPendientes();
            this.cargarEstadisticas();
        },
        
        onEstadoPedidoCambiado() {
            this.pedidoSeleccionado = null;
            this.onPedidoActualizado();
        },
        
        logout() {
            // Desconectar SignalR
            if (this.signalrConnection) {
                this.signalrConnection.stop();
            }
            
            // Limpiar cookies y redirigir
            document.cookie = 'auth_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            window.location.href = '/Account/Login';
        }
    }
};
