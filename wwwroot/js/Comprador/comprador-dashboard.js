console.log(' comprador-dashboard.js cargado');

// Verificar Vue
console.log('Vue disponible:', typeof Vue !== 'undefined');

// ==================== UTILIDADES ====================
const apiUtilsComprador = {
    async fetchWithAuth(url, options = {}) {
        const token = localStorage.getItem('authToken');
        if (!token) {
            throw new Error('No hay token de autenticación');
        }

        const defaultOptions = {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                ...options.headers
            }
        };

        const response = await fetch(url, { ...options, ...defaultOptions });
        
        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authToken');
                localStorage.removeItem('user');
                window.location.href = '/Account/Login';
                throw new Error('Sesión expirada');
            }
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }

        return await response.json();
    },

    formatMoneda(valor) {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(valor);
    },

    formatFecha(fecha) {
        return new Date(fecha).toLocaleDateString('es-AR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    },

    formatFechaHora(fecha) {
        return new Date(fecha).toLocaleString('es-AR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
};

// ==================== COMPONENTE: DASHBOARD COMPRADOR ====================
const DashboardComprador = {
    name: 'DashboardComprador',
    template: `
        <div class="dashboard-comprador">
            <!-- Tarjetas de Resumen -->
            <div class="row g-3 mb-4">
                <div class="col-md-3 col-sm-6">
                    <div class="card bg-primary text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Pedidos Activos</h6>
                                    <h3 class="mb-0 fw-bold">{{ pedidosActivos }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-shopping-bag fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-clock me-1"></i>
                                En proceso
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-success text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Pedidos Completados</h6>
                                    <h3 class="mb-0 fw-bold">{{ pedidosCompletados }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-check-circle fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-history me-1"></i>
                                Total histórico
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-warning text-dark h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Gasto del Mes</h6>
                                    <h3 class="mb-0 fw-bold">{{ formatMoneda(gastoMes) }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-dollar-sign fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-chart-line me-1"></i>
                                {{ pedidosMes }} pedidos este mes
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-info text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Favoritos</h6>
                                    <h3 class="mb-0 fw-bold">{{ tortasFavoritas }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-heart fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-star me-1"></i>
                                Tortas guardadas
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Contenido Principal -->
            <div class="row">
                <!-- Pedidos Recientes -->
                <div class="col-lg-8 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-header bg-white d-flex justify-content-between align-items-center">
                            <h5 class="mb-0">
                                <i class="fas fa-history me-2"></i>
                                Pedidos Recientes
                            </h5>
                            <button class="btn btn-outline-primary btn-sm" @click="verTodosLosPedidos">
                                Ver Todos
                            </button>
                        </div>
                        <div class="card-body">
                            <div v-if="pedidosRecientes.length > 0" class="table-responsive">
                                <table class="table table-hover">
                                    <thead>
                                        <tr>
                                            <th>Fecha</th>
                                            <th>Torta</th>
                                            <th>Vendedor</th>
                                            <th>Monto</th>
                                            <th>Estado</th>
                                            <th>Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="pedido in pedidosRecientes" :key="pedido.id">
                                            <td>
                                                <div class="small">{{ formatFecha(pedido.fechaPago) }}</div>
                                                <div class="text-muted" style="font-size: 0.75rem;">
                                                    {{ formatHora(pedido.fechaPago) }}
                                                </div>
                                            </td>
                                            <td>
                                                <div class="d-flex align-items-center">
                                                    <img v-if="pedido.imagenTorta" 
                                                         :src="pedido.imagenTorta" 
                                                         class="rounded me-2" 
                                                         style="width: 40px; height: 40px; object-fit: cover;">
                                                    <div>
                                                        <div class="fw-bold small">{{ pedido.nombreTorta }}</div>
                                                        <div class="text-muted small">x{{ pedido.cantidad }}</div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td>
                                                <div class="small">{{ pedido.nombreVendedor }}</div>
                                            </td>
                                            <td class="fw-bold text-success">{{ formatMoneda(pedido.monto) }}</td>
                                            <td>
                                                <span class="badge" :class="getBadgeClass(pedido.estado)">
                                                    {{ getEstadoTexto(pedido.estado) }}
                                                </span>
                                            </td>
                                            <td>
                                                <button class="btn btn-sm btn-outline-primary" 
                                                        @click="verDetallePedido(pedido)">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                                <button v-if="pedido.estado === 0" 
                                                        class="btn btn-sm btn-outline-danger ms-1"
                                                        @click="cancelarPedido(pedido)">
                                                    <i class="fas fa-times"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div v-else class="text-center py-5 text-muted">
                                <i class="fas fa-shopping-bag fa-3x mb-3 opacity-50"></i>
                                <p>No tienes pedidos recientes</p>
                                <button class="btn btn-primary" @click="irATortas">
                                    <i class="fas fa-birthday-cake me-2"></i>Explorar Tortas
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Acciones Rápidas y Estadísticas -->
                <div class="col-lg-4 mb-4">
                    <!-- Acciones Rápidas -->
                    <div class="card shadow-sm mb-4">
                        <div class="card-header bg-white">
                            <h6 class="mb-0">
                                <i class="fas fa-bolt me-2"></i>Acciones Rápidas
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="d-grid gap-2">
                                <button class="btn btn-outline-primary text-start" @click="irATortas">
                                    <i class="fas fa-birthday-cake me-2"></i>Buscar Tortas
                                </button>
                                <button class="btn btn-outline-success text-start" @click="verFavoritos">
                                    <i class="fas fa-heart me-2"></i>Mis Favoritos
                                </button>
                                <button class="btn btn-outline-info text-start" @click="verCarrito">
                                    <i class="fas fa-shopping-cart me-2"></i>Mi Carrito
                                </button>
                                <button class="btn btn-outline-warning text-start" @click="contactarSoporte">
                                    <i class="fas fa-headset me-2"></i>Soporte
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- Estadísticas Personales -->
                    <div class="card shadow-sm">
                        <div class="card-header bg-white">
                            <h6 class="mb-0">
                                <i class="fas fa-chart-pie me-2"></i>Mis Estadísticas
                            </h6>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <small class="text-muted">Total Gastado</small>
                                <div class="h5 text-success">{{ formatMoneda(totalGastado) }}</div>
                            </div>
                            <div class="mb-3">
                                <small class="text-muted">Compras Realizadas</small>
                                <div class="h5">{{ totalCompras }}</div>
                            </div>
                            <div class="mb-3">
                                <small class="text-muted">Tortas Favoritas</small>
                                <div class="h5 text-info">{{ tortasFavoritas }}</div>
                            </div>
                            <div>
                                <small class="text-muted">Miembro desde</small>
                                <div class="small text-muted">{{ formatFecha(fechaRegistro) }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            // Estadísticas principales
            pedidosActivos: 0,
            pedidosCompletados: 0,
            gastoMes: 0,
            pedidosMes: 0,
            tortasFavoritas: 0,
            
            // Datos detallados
            pedidosRecientes: [],
            
            // Estadísticas adicionales
            totalGastado: 0,
            totalCompras: 0,
            fechaRegistro: null,
            
            // Control
            loading: true
        }
    },
    
    async mounted() {
        await this.cargarDashboard();
    },
    
    methods: {
        async cargarDashboard() {
            try {
                this.loading = true;
                console.log('🔍 Cargando dashboard del comprador...');
                
                // Cargar pedidos del comprador
                await this.cargarPedidos();
                
                // Cargar estadísticas
                await this.cargarEstadisticas();
                
                console.log(' Dashboard del comprador cargado');
            } catch (error) {
                console.error(' Error cargando dashboard:', error);
            } finally {
                this.loading = false;
            }
        },
        
        async cargarPedidos() {
            try {
                const data = await apiUtilsComprador.fetchWithAuth('/api/PagoApi');
                const todosPagos = data.items || data || [];
                
                // Filtrar solo los pagos del comprador actual
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const compradorId = user.id;
                
                const misPagos = todosPagos.filter(p => p.compradorId === compradorId);
                
                console.log('🛒 Mis pedidos encontrados:', misPagos.length);
                
                // Pedidos activos (pendientes)
                this.pedidosActivos = misPagos.filter(p => 
                    p.estado === 'Pendiente' || p.estado === 0
                ).length;
                
                // Pedidos completados
                this.pedidosCompletados = misPagos.filter(p => 
                    p.estado === 'Completado' || p.estado === 1
                ).length;
                
                // Pedidos del mes
                const mesActual = new Date().getMonth();
                const añoActual = new Date().getFullYear();
                
                this.pedidosMes = misPagos.filter(p => {
                    const fecha = new Date(p.fechaPago);
                    return fecha.getMonth() === mesActual && 
                           fecha.getFullYear() === añoActual;
                }).length;
                
                // Gasto del mes
                this.gastoMes = misPagos
                    .filter(p => {
                        const fecha = new Date(p.fechaPago);
                        return fecha.getMonth() === mesActual && 
                               fecha.getFullYear() === añoActual &&
                               (p.estado === 'Completado' || p.estado === 1);
                    })
                    .reduce((sum, p) => sum + (p.monto || 0), 0);
                
                // Pedidos recientes (últimos 5)
                this.pedidosRecientes = misPagos
                    .sort((a, b) => new Date(b.fechaPago) - new Date(a.fechaPago))
                    .slice(0, 5)
                    .map(p => ({
                        ...p,
                        nombreTorta: p.torta?.nombre || 'Torta',
                        nombreVendedor: p.vendedor?.nombreComercial || 'Vendedor',
                        imagenTorta: this.obtenerImagenTorta(p.torta)
                    }));
                    
            } catch (error) {
                console.error('Error cargando pedidos:', error);
            }
        },
        
        async cargarEstadisticas() {
            try {
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const compradorId = user.id;
                
                // Cargar todos los pagos para calcular estadísticas
                const data = await apiUtilsComprador.fetchWithAuth('/api/PagoApi');
                const todosPagos = data.items || data || [];
                const misPagos = todosPagos.filter(p => p.compradorId === compradorId);
                
                // Total gastado
                this.totalGastado = misPagos
                    .filter(p => p.estado === 'Completado' || p.estado === 1)
                    .reduce((sum, p) => sum + (p.monto || 0), 0);
                
                // Total de compras
                this.totalCompras = misPagos.length;
                
                // Fecha de registro (del usuario)
                this.fechaRegistro = user.fechaRegistro || new Date().toISOString();
                
                // Tortas favoritas (simulado por ahora)
                this.tortasFavoritas = 3; // Esto vendría de una API de favoritos
                
            } catch (error) {
                console.error('Error cargando estadísticas:', error);
            }
        },
        
        obtenerImagenTorta(torta) {
            if (torta && torta.imagenes && torta.imagenes.length > 0) {
                const principal = torta.imagenes.find(img => img.esPrincipal);
                return principal ? principal.urlImagen : torta.imagenes[0].urlImagen;
            }
            return '/images/torta-default.jpg';
        },
        
        getEstadoTexto(estado) {
            const estados = {
                0: 'Pendiente',
                1: 'Completado',
                2: 'Cancelado',
                'Pendiente': 'Pendiente',
                'Completado': 'Completado',
                'Cancelado': 'Cancelado'
            };
            return estados[estado] || 'Desconocido';
        },
        
        getBadgeClass(estado) {
            const clases = {
                0: 'bg-warning text-dark',
                1: 'bg-success',
                2: 'bg-danger',
                'Pendiente': 'bg-warning text-dark',
                'Completado': 'bg-success',
                'Cancelado': 'bg-danger'
            };
            return clases[estado] || 'bg-secondary';
        },
        
        verDetallePedido(pedido) {
            const detalle = `
DETALLE DEL PEDIDO #${pedido.id}

Torta: ${pedido.nombreTorta}
Vendedor: ${pedido.nombreVendedor}
Cantidad: ${pedido.cantidad}
Precio Unitario: ${this.formatMoneda(pedido.precioUnitario)}
Total: ${this.formatMoneda(pedido.monto)}

Estado: ${this.getEstadoTexto(pedido.estado)}
Fecha: ${this.formatFecha(pedido.fechaPago)}
Método de Pago: ${pedido.metodoPago || 'No especificado'}

${pedido.direccionEntrega ? `Dirección: ${pedido.direccionEntrega}` : ''}
${pedido.observaciones ? `Observaciones: ${pedido.observaciones}` : ''}
            `;
            
            alert(detalle);
        },
        
        async cancelarPedido(pedido) {
            if (!confirm(`¿Estás seguro de cancelar el pedido #${pedido.id}?`)) {
                return;
            }

            try {
                await apiUtilsComprador.fetchWithAuth(`/api/PagoApi/${pedido.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        ...pedido,
                        estado: 2 // Cancelado
                    })
                });

                // Recargar datos
                await this.cargarDashboard();
                this.mostrarMensaje('Pedido cancelado correctamente', 'success');
            } catch (error) {
                console.error('Error cancelando pedido:', error);
                this.mostrarMensaje('Error al cancelar el pedido', 'error');
            }
        },
        
        verTodosLosPedidos() {
            this.mostrarMensaje('Navegando a historial completo...', 'info');
            // Aquí se cambiaría la vista al historial completo
        },
        
        irATortas() {
            this.mostrarMensaje('Navegando al catálogo...', 'info');
            // Aquí se cambiaría la vista al catálogo
        },
        
        verFavoritos() {
            this.mostrarMensaje('Navegando a favoritos...', 'info');
        },
        
        verCarrito() {
            this.mostrarMensaje('Navegando al carrito...', 'info');
        },
        
        contactarSoporte() {
            this.mostrarMensaje('Contactando soporte...', 'info');
        },
        
        formatMoneda(valor) {
            return apiUtilsComprador.formatMoneda(valor);
        },
        
        formatFecha(fecha) {
            return apiUtilsComprador.formatFecha(fecha);
        },
        
        formatHora(fecha) {
            return new Date(fecha).toLocaleTimeString('es-AR', {
                hour: '2-digit',
                minute: '2-digit'
            });
        },
        
        mostrarMensaje(mensaje, tipo) {
            if (typeof toastr !== 'undefined') {
                toastr[tipo](mensaje);
            } else {
                alert(mensaje);
            }
        }
    }
};

// ==================== COMPONENTE: CATÁLOGO COMPRADOR ====================
const CatalogoComprador = {
    name: 'CatalogoComprador',
    template: `
        <div class="catalogo-comprador">
            <div class="card shadow-sm">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-store me-2"></i>Catálogo de Tortas
                        <span class="badge bg-primary ms-2">{{ tortas.length }}</span>
                    </h5>
                    <div class="d-flex gap-2">
                        <input type="text" class="form-control form-control-sm" 
                               placeholder="Buscar tortas..." v-model="filtroBusqueda">
                        <select class="form-select form-select-sm" v-model="filtroCategoria">
                            <option value="">Todas las categorías</option>
                            <option v-for="cat in categorias" :key="cat" :value="cat">{{ cat }}</option>
                        </select>
                    </div>
                </div>
                <div class="card-body">
                    <!-- Loading -->
                    <div v-if="loading" class="text-center py-5">
                        <div class="spinner-border text-primary mb-3"></div>
                        <p class="text-muted">Cargando catálogo...</p>
                    </div>

                    <!-- Sin tortas -->
                    <div v-else-if="tortasFiltradas.length === 0" class="text-center py-5">
                        <i class="fas fa-search fa-3x text-muted mb-3"></i>
                        <h5 class="text-muted">No se encontraron tortas</h5>
                        <p class="text-muted">Intenta con otros términos de búsqueda</p>
                    </div>

                    <!-- Grid de tortas -->
                    <div v-else class="row g-3">
                        <div v-for="torta in tortasFiltradas" :key="torta.id" class="col-md-6 col-lg-4">
                            <div class="card h-100 shadow-sm torta-card">
                                <!-- Imagen -->
                                <div class="position-relative">
                                    <img v-if="torta.imagenPrincipal" 
                                         :src="torta.imagenPrincipal" 
                                         class="card-img-top" 
                                         style="height: 200px; object-fit: cover;">
                                    <div v-else class="bg-light d-flex align-items-center justify-content-center" 
                                         style="height: 200px;">
                                        <i class="fas fa-birthday-cake fa-3x text-muted"></i>
                                    </div>
                                    
                                    <!-- Badge de disponibilidad -->
                                    <span v-if="torta.disponible && torta.stock > 0" 
                                          class="position-absolute top-0 end-0 m-2 badge bg-success">
                                        Disponible
                                    </span>
                                    <span v-else 
                                          class="position-absolute top-0 end-0 m-2 badge bg-danger">
                                        Agotado
                                    </span>
                                </div>
                                
                                <div class="card-body d-flex flex-column">
                                    <h5 class="card-title">{{ torta.nombre }}</h5>
                                    <p class="card-text text-muted small flex-grow-1">
                                        {{ torta.descripcion }}
                                    </p>
                                    
                                    <div class="mb-2">
                                        <span class="badge bg-secondary">{{ torta.categoria }}</span>
                                        <span v-if="torta.personalizable" class="badge bg-warning ms-1">Personalizable</span>
                                    </div>
                                    
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <div>
                                            <div class="h4 text-success mb-0">{{ formatMoneda(torta.precio) }}</div>
                                            <small class="text-muted">
                                                <i class="fas fa-box me-1"></i>Stock: {{ torta.stock }}
                                            </small>
                                        </div>
                                        <div class="text-end">
                                            <div class="small">
                                                <i class="fas fa-star text-warning me-1"></i>
                                                {{ (torta.calificacion || 0).toFixed(1) }}/5
                                            </div>
                                            <div class="small text-muted">
                                                <i class="fas fa-shopping-cart me-1"></i>
                                                {{ torta.vecesVendida || 0 }} ventas
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <div class="d-flex justify-content-between align-items-center">
                                        <small class="text-muted">
                                            Por: <strong>{{ torta.nombreVendedor }}</strong>
                                        </small>
                                    </div>
                                    
                                    <!-- Botones de acción -->
                                    <div class="mt-3 d-grid gap-2">
                                        <button class="btn btn-primary" 
                                                :disabled="!torta.disponible || torta.stock === 0"
                                                @click="agregarAlCarrito(torta)">
                                            <i class="fas fa-cart-plus me-2"></i>
                                            {{ torta.disponible && torta.stock > 0 ? 'Agregar al Carrito' : 'No Disponible' }}
                                        </button>
                                        <button class="btn btn-outline-secondary btn-sm" 
                                                @click="verDetalle(torta)">
                                            <i class="fas fa-info-circle me-2"></i>Ver Detalles
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            tortas: [],
            loading: true,
            filtroBusqueda: '',
            filtroCategoria: ''
        }
    },
    
    computed: {
        tortasFiltradas() {
            return this.tortas.filter(torta => {
                const coincideBusqueda = torta.nombre.toLowerCase().includes(this.filtroBusqueda.toLowerCase()) ||
                                       torta.descripcion.toLowerCase().includes(this.filtroBusqueda.toLowerCase());
                const coincideCategoria = !this.filtroCategoria || torta.categoria === this.filtroCategoria;
                return coincideBusqueda && coincideCategoria;
            });
        },
        
        categorias() {
            return [...new Set(this.tortas.map(t => t.categoria).filter(Boolean))];
        }
    },
    
    async mounted() {
        await this.cargarCatalogo();
    },
    
    methods: {
        async cargarCatalogo() {
            try {
                this.loading = true;
                const data = await apiUtilsComprador.fetchWithAuth('/api/TortaApi');
                const todasTortas = data.items || data || [];
                
                // Filtrar solo tortas disponibles
                this.tortas = todasTortas
                    .filter(t => t.disponible)
                    .map(t => ({
                        ...t,
                        imagenPrincipal: this.obtenerImagenPrincipal(t),
                        nombreVendedor: t.vendedor?.nombreComercial || 'Vendedor'
                    }));
                
                console.log(' Catálogo cargado:', this.tortas.length);
            } catch (error) {
                console.error(' Error cargando catálogo:', error);
                this.mostrarMensaje('Error al cargar el catálogo', 'error');
            } finally {
                this.loading = false;
            }
        },
        
        obtenerImagenPrincipal(torta) {
            if (torta.imagenes && torta.imagenes.length > 0) {
                const principal = torta.imagenes.find(img => img.esPrincipal);
                return principal ? principal.urlImagen : torta.imagenes[0].urlImagen;
            }
            return null;
        },
        
        agregarAlCarrito(torta) {
            this.mostrarMensaje(`${torta.nombre} agregada al carrito`, 'success');
            // Aquí se implementaría la lógica del carrito
        },
        
        verDetalle(torta) {
            const detalle = `
DETALLE DE LA TORTA

Nombre: ${torta.nombre}
Descripción: ${torta.descripcion}
Precio: ${this.formatMoneda(torta.precio)}
Categoría: ${torta.categoria}
Stock: ${torta.stock}
Calificación: ${torta.calificacion}/5
Vendedor: ${torta.nombreVendedor}

${torta.personalizable ? ' Personalizable' : ' No personalizable'}
${torta.disponible ? ' Disponible' : ' No disponible'}
            `;
            
            alert(detalle);
        },
        
        formatMoneda(valor) {
            return apiUtilsComprador.formatMoneda(valor);
        },
        
        mostrarMensaje(mensaje, tipo) {
            if (typeof toastr !== 'undefined') {
                toastr[tipo](mensaje);
            } else {
                alert(mensaje);
            }
        }
    }
};

// ==================== COMPONENTE: HISTORIAL COMPRAS ====================
const HistorialCompras = {
    name: 'HistorialCompras',
    template: `
        <div class="historial-compras">
            <div class="card shadow-sm">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-history me-2"></i>Mi Historial de Compras
                        <span class="badge bg-primary ms-2">{{ pedidos.length }}</span>
                    </h5>
                    
                    <div class="d-flex gap-2">
                        <select v-model="filtroEstado" class="form-select form-select-sm" style="width: auto;">
                            <option value="">Todos los estados</option>
                            <option value="0">Pendiente</option>
                            <option value="1">Completado</option>
                            <option value="2">Cancelado</option>
                        </select>
                        
                        <button class="btn btn-outline-primary btn-sm" @click="cargarPedidos">
                            <i class="fas fa-sync-alt"></i>
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    <!-- Loading -->
                    <div v-if="loading" class="text-center py-5">
                        <div class="spinner-border text-primary mb-3"></div>
                        <p class="text-muted">Cargando historial...</p>
                    </div>

                    <!-- Sin pedidos -->
                    <div v-else-if="pedidosFiltrados.length === 0" class="text-center py-5">
                        <i class="fas fa-receipt fa-3x text-muted mb-3"></i>
                        <h5 class="text-muted">No hay compras registradas</h5>
                        <p class="text-muted">Realiza tu primera compra desde el catálogo</p>
                        <button class="btn btn-primary" @click="irAlCatalogo">
                            <i class="fas fa-store me-2"></i>Ir al Catálogo
                        </button>
                    </div>

                    <!-- Lista de pedidos -->
                    <div v-else class="table-responsive">
                        <table class="table table-hover align-middle">
                            <thead class="table-light">
                                <tr>
                                    <th style="width: 80px;">ID</th>
                                    <th>Torta</th>
                                    <th>Vendedor</th>
                                    <th style="width: 100px;">Cantidad</th>
                                    <th style="width: 120px;">Monto</th>
                                    <th style="width: 120px;">Fecha</th>
                                    <th style="width: 120px;">Estado</th>
                                    <th style="width: 150px;">Acciones</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="pedido in pedidosFiltrados" :key="pedido.id">
                                    <td class="fw-bold">#{{ pedido.id }}</td>
                                    <td>
                                        <div class="d-flex align-items-center">
                                            <img v-if="pedido.imagenTorta" 
                                                 :src="pedido.imagenTorta" 
                                                 class="rounded me-2" 
                                                 style="width: 40px; height: 40px; object-fit: cover;">
                                            <div>
                                                <div class="fw-bold small">{{ pedido.nombreTorta }}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td>
                                        <div class="small">{{ pedido.nombreVendedor }}</div>
                                    </td>
                                    <td class="text-center">
                                        <span class="badge bg-secondary">{{ pedido.cantidad }}</span>
                                    </td>
                                    <td class="fw-bold text-success">{{ formatMoneda(pedido.monto) }}</td>
                                    <td>
                                        <div class="small">{{ formatFecha(pedido.fechaPago) }}</div>
                                        <div class="text-muted" style="font-size: 0.75rem;">
                                            {{ formatHora(pedido.fechaPago) }}
                                        </div>
                                    </td>
                                    <td>
                                        <span class="badge" :class="getBadgeClass(pedido.estado)">
                                            {{ getEstadoTexto(pedido.estado) }}
                                        </span>
                                    </td>
                                    <td>
                                        <div class="btn-group btn-group-sm">
                                            <button class="btn btn-outline-primary" 
                                                    @click="verDetalle(pedido)"
                                                    title="Ver detalle">
                                                <i class="fas fa-eye"></i>
                                            </button>
                                            <button v-if="pedido.estado === 0" 
                                                    class="btn btn-outline-danger"
                                                    @click="cancelarPedido(pedido)"
                                                    title="Cancelar">
                                                <i class="fas fa-times"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            pedidos: [],
            filtroEstado: '',
            loading: true
        }
    },
    
    computed: {
        pedidosFiltrados() {
            if (!this.filtroEstado) return this.pedidos;
            return this.pedidos.filter(p => p.estado.toString() === this.filtroEstado);
        }
    },
    
    async mounted() {
        await this.cargarPedidos();
    },
    
    methods: {
        async cargarPedidos() {
            try {
                this.loading = true;
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const compradorId = user.id;
                
                const data = await apiUtilsComprador.fetchWithAuth('/api/PagoApi');
                const todosPagos = data.items || data || [];
                
                // Filtrar pagos del comprador actual
                this.pedidos = todosPagos
                    .filter(p => p.compradorId === compradorId)
                    .map(p => ({
                        ...p,
                        nombreTorta: p.torta?.nombre || 'Torta',
                        nombreVendedor: p.vendedor?.nombreComercial || 'Vendedor',
                        imagenTorta: this.obtenerImagenTorta(p.torta)
                    }))
                    .sort((a, b) => new Date(b.fechaPago) - new Date(a.fechaPago));
                
                console.log('Historial cargado:', this.pedidos.length);
            } catch (error) {
                console.error(' Error cargando historial:', error);
                this.mostrarMensaje('Error al cargar el historial', 'error');
            } finally {
                this.loading = false;
            }
        },
        
        obtenerImagenTorta(torta) {
            if (torta && torta.imagenes && torta.imagenes.length > 0) {
                const principal = torta.imagenes.find(img => img.esPrincipal);
                return principal ? principal.urlImagen : torta.imagenes[0].urlImagen;
            }
            return null;
        },
        
        verDetalle(pedido) {
            const detalle = `
DETALLE DEL PEDIDO #${pedido.id}

Torta: ${pedido.nombreTorta}
Vendedor: ${pedido.nombreVendedor}
Cantidad: ${pedido.cantidad}
Precio Unitario: ${this.formatMoneda(pedido.precioUnitario)}
Total: ${this.formatMoneda(pedido.monto)}

Estado: ${this.getEstadoTexto(pedido.estado)}
Fecha: ${this.formatFecha(pedido.fechaPago)} ${this.formatHora(pedido.fechaPago)}

Método de Pago: ${this.getMetodoPagoTexto(pedido.metodoPago)}

${pedido.direccionEntrega ? `Dirección de Entrega:\n${pedido.direccionEntrega}\n` : ''}
${pedido.fechaEntrega ? `Fecha de Entrega: ${this.formatFecha(pedido.fechaEntrega)}\n` : ''}
${pedido.observaciones ? `Observaciones:\n${pedido.observaciones}` : ''}
            `;
            
            alert(detalle);
        },
        
        async cancelarPedido(pedido) {
            if (!confirm(`¿Estás seguro de cancelar el pedido #${pedido.id}?`)) {
                return;
            }

            try {
                await apiUtilsComprador.fetchWithAuth(`/api/PagoApi/${pedido.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        ...pedido,
                        estado: 2 // Cancelado
                    })
                });

                // Recargar lista
                await this.cargarPedidos();
                this.mostrarMensaje('Pedido cancelado correctamente', 'success');
            } catch (error) {
                console.error('Error cancelando pedido:', error);
                this.mostrarMensaje('Error al cancelar el pedido', 'error');
            }
        },
        
        getEstadoTexto(estado) {
            const estados = {
                0: 'Pendiente',
                1: 'Completado',
                2: 'Cancelado'
            };
            return estados[estado] || 'Desconocido';
        },
        
        getBadgeClass(estado) {
            const clases = {
                0: 'bg-warning text-dark',
                1: 'bg-success',
                2: 'bg-danger'
            };
            return clases[estado] || 'bg-secondary';
        },
        
        getMetodoPagoTexto(metodo) {
            const metodos = {
                0: 'Transferencia',
                1: 'Efectivo',
                2: 'Tarjeta de Crédito',
                3: 'Tarjeta de Débito',
                4: 'MercadoPago'
            };
            return metodos[metodo] || 'No especificado';
        },
        
        formatMoneda(valor) {
            return apiUtilsComprador.formatMoneda(valor);
        },
        
        formatFecha(fecha) {
            return apiUtilsComprador.formatFecha(fecha);
        },
        
        formatHora(fecha) {
            return new Date(fecha).toLocaleTimeString('es-AR', {
                hour: '2-digit',
                minute: '2-digit'
            });
        },
        
        irAlCatalogo() {
            this.mostrarMensaje('Navegando al catálogo...', 'info');
        },
        
        mostrarMensaje(mensaje, tipo) {
            if (typeof toastr !== 'undefined') {
                toastr[tipo](mensaje);
            } else {
                alert(mensaje);
            }
        }
    }
};

// ==================== COMPONENTE: PERFIL COMPRADOR ====================
const PerfilComprador = {
    name: 'PerfilComprador',
    template: `
        <div class="perfil-comprador">
            <div class="card shadow-sm">
                <div class="card-header bg-white">
                    <h5 class="mb-0">
                        <i class="fas fa-user me-2"></i>Mi Perfil
                    </h5>
                </div>
                <div class="card-body">
                    <div v-if="loading" class="text-center py-5">
                        <div class="spinner-border text-primary"></div>
                    </div>
                    <div v-else class="row">
                        <!-- Columna Izquierda: Avatar y Estadísticas -->
                        <div class="col-md-4 text-center mb-4">
                            <div class="mb-3">
                                <img v-if="comprador.avatar" 
                                     :src="comprador.avatar" 
                                     class="rounded-circle img-thumbnail mb-3" 
                                     style="width: 150px; height: 150px; object-fit: cover;">
                                <div v-else class="rounded-circle bg-light d-flex align-items-center justify-content-center mx-auto mb-3"
                                     style="width: 150px; height: 150px;">
                                    <i class="fas fa-user fa-4x text-muted"></i>
                                </div>
                            </div>
                            
                            <h4>{{ comprador.nombre }}</h4>
                            <p class="text-muted">Comprador</p>
                            
                            <div class="mb-3">
                                <span class="badge bg-success">
                                    <i class="fas fa-check-circle me-1"></i>Activo
                                </span>
                            </div>
                            
                            <!-- Estadísticas del Comprador -->
                            <div class="card bg-light">
                                <div class="card-body">
                                    <h6 class="text-muted mb-3">Mis Estadísticas</h6>
                                    <div class="mb-2">
                                        <small class="text-muted">Compras Totales</small>
                                        <div class="h5 mb-0">{{ comprador.totalCompras || 0 }}</div>
                                    </div>
                                    <hr>
                                    <div class="mb-2">
                                        <small class="text-muted">Total Gastado</small>
                                        <div class="h5 mb-0 text-success">{{ formatMoneda(comprador.totalGastado || 0) }}</div>
                                    </div>
                                    <hr>
                                    <div class="mb-2">
                                        <small class="text-muted">Miembro desde</small>
                                        <div class="small">{{ formatFecha(comprador.fechaCreacion) }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Columna Derecha: Información -->
                        <div class="col-md-8">
                            <h5 class="mb-3">Información Personal</h5>
                            
                            <table class="table table-borderless">
                                <tr>
                                    <td class="text-muted" style="width: 150px;">Nombre Completo:</td>
                                    <td><strong>{{ comprador.nombre }} {{ comprador.apellido }}</strong></td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Email:</td>
                                    <td>
                                        <i class="fas fa-envelope me-2 text-muted"></i>
                                        {{ comprador.email }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Teléfono:</td>
                                    <td>
                                        <i class="fas fa-phone me-2 text-muted"></i>
                                        {{ comprador.telefono || 'No especificado' }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Dirección:</td>
                                    <td>
                                        <i class="fas fa-map-marker-alt me-2 text-muted"></i>
                                        {{ comprador.direccion || 'No especificada' }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Último Acceso:</td>
                                    <td>{{ formatFechaHora(comprador.ultimoAcceso) }}</td>
                                </tr>
                            </table>
                            
                            <hr>
                            
                            <h5 class="mb-3">Preferencias</h5>
                            
                            <div class="mb-3">
                                <strong>Categorías favoritas:</strong>
                                <div class="mt-2">
                                    <span v-if="comprador.preferencias && comprador.preferencias.length > 0">
                                        <span v-for="pref in comprador.preferencias" :key="pref" 
                                              class="badge bg-primary me-1 mb-1">
                                            {{ pref }}
                                        </span>
                                    </span>
                                    <span v-else class="text-muted">No hay preferencias guardadas</span>
                                </div>
                            </div>
                            
                            <hr>
                            
                            <div class="mt-4">
                                <button class="btn btn-primary me-2" @click="editarPerfil">
                                    <i class="fas fa-edit me-2"></i>Editar Perfil
                                </button>
                                <button class="btn btn-outline-secondary" @click="cambiarContrasena">
                                    <i class="fas fa-key me-2"></i>Cambiar Contraseña
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            comprador: {},
            loading: true
        }
    },
    
    async mounted() {
        await this.cargarPerfil();
    },
    
    methods: {
        async cargarPerfil() {
            try {
                this.loading = true;
                const data = await apiUtilsComprador.fetchWithAuth('/api/AuthApi/me');
                
                if (data.success && data.user) {
                    if (data.user.rol !== 'Comprador') {
                        throw new Error('No eres un comprador autorizado');
                    }

                    this.comprador = {
                        // Datos de persona
                        id: data.user.id,
                        nombre: data.user.nombre,
                        apellido: data.user.apellido || '',
                        email: data.user.email,
                        telefono: data.user.telefono,
                        direccion: data.user.direccion,
                        avatar: data.user.avatar,
                        ultimoAcceso: data.user.ultimoAcceso,
                        
                        // Datos específicos del comprador
                        totalCompras: data.user.rolData?.totalCompras || 0,
                        totalGastado: data.user.rolData?.totalGastado || 0,
                        preferencias: data.user.rolData?.preferencias || [],
                        fechaCreacion: data.user.rolData?.fechaCreacion || data.user.fechaRegistro
                    };
                    
                    console.log('Perfil del comprador cargado:', this.comprador);
                }
            } catch (error) {
                console.error(' Error cargando perfil:', error);
                this.mostrarMensaje('Error al cargar el perfil', 'error');
            } finally {
                this.loading = false;
            }
        },
        
        editarPerfil() {
            this.mostrarMensaje('Función de edición de perfil en desarrollo', 'info');
        },
        
        cambiarContrasena() {
            this.mostrarMensaje('Función de cambio de contraseña en desarrollo', 'info');
        },
        
        formatMoneda(valor) {
            return apiUtilsComprador.formatMoneda(valor);
        },
        
        formatFecha(fecha) {
            if (!fecha) return 'No disponible';
            return apiUtilsComprador.formatFecha(fecha);
        },
        
        formatFechaHora(fecha) {
            if (!fecha) return 'No disponible';
            return apiUtilsComprador.formatFechaHora(fecha);
        },
        
        mostrarMensaje(mensaje, tipo) {
            if (typeof toastr !== 'undefined') {
                toastr[tipo](mensaje);
            } else {
                alert(mensaje);
            }
        }
    }
};

// ==================== COMPONENTE PRINCIPAL: COMPRADOR DASHBOARD ====================
const CompradorDashboard = {
    name: 'CompradorDashboard',
    template: `
        <div class="comprador-dashboard">
            <!-- Navbar -->
            <nav class="navbar navbar-expand-lg navbar-dark bg-primary sticky-top shadow">
                <div class="container-fluid">
                    <a class="navbar-brand" href="#">
                        <i class="fas fa-user me-2"></i>
                        Casa de las Tortas - Panel de Comprador
                    </a>
                    
                    <div class="navbar-nav ms-auto">
                        <span class="navbar-text text-white me-3">
                            <i class="fas fa-user me-1"></i>
                            {{ comprador.nombre || 'Cargando...' }}
                        </span>
                        <button class="btn btn-outline-light btn-sm" @click="logout">
                            <i class="fas fa-sign-out-alt me-1"></i>Salir
                        </button>
                    </div>
                </div>
            </nav>

            <!-- Main Content -->
            <div class="container-fluid mt-4">
                <div class="row">
                    <!-- Sidebar -->
                    <div class="col-md-3 col-lg-2 mb-4">
                        <div class="card shadow-sm mb-3">
                            <div class="card-body">
                                <nav class="nav flex-column">
                                    <button class="btn mb-2 text-start" 
                                            :class="vistaActiva === 'dashboard' ? 'btn-primary' : 'btn-outline-primary'"
                                            @click="cambiarVista('dashboard')">
                                        <i class="fas fa-home me-2"></i>Dashboard
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'catalogo' ? 'btn-primary' : 'btn-outline-primary'"
                                            @click="cambiarVista('catalogo')">
                                        <i class="fas fa-store me-2"></i>Catálogo
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'historial' ? 'btn-primary' : 'btn-outline-primary'"
                                            @click="cambiarVista('historial')">
                                        <i class="fas fa-history me-2"></i>Historial
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'perfil' ? 'btn-primary' : 'btn-outline-primary'"
                                            @click="cambiarVista('perfil')">
                                        <i class="fas fa-user me-2"></i>Mi Perfil
                                    </button>
                                </nav>
                            </div>
                        </div>

                        <!-- Resumen Rápido -->
                        <div class="card shadow-sm">
                            <div class="card-header bg-white">
                                <h6 class="mb-0 text-muted">
                                    <i class="fas fa-info-circle me-2"></i>Resumen
                                </h6>
                            </div>
                            <div class="card-body">
                                <div class="d-flex justify-content-between mb-2 small">
                                    <span class="text-muted">Pedidos activos:</span>
                                    <strong class="text-warning">{{ pedidosActivos }}</strong>
                                </div>
                                <div class="d-flex justify-content-between mb-2 small">
                                    <span class="text-muted">Compras totales:</span>
                                    <strong class="text-primary">{{ totalCompras }}</strong>
                                </div>
                                <div class="d-flex justify-content-between small">
                                    <span class="text-muted">Total gastado:</span>
                                    <strong class="text-success">{{ formatMoneda(totalGastado) }}</strong>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Main Content Area -->
                    <div class="col-md-9 col-lg-10">
                        <!-- Loading -->
                        <div v-if="loading" class="card shadow-sm">
                            <div class="card-body text-center py-5">
                                <div class="spinner-border text-primary mb-3" style="width: 3rem; height: 3rem;"></div>
                                <h5 class="text-muted">Cargando dashboard...</h5>
                            </div>
                        </div>

                        <!-- Vista Dinámica -->
                        <div v-else>
                            <component :is="componenteActual"></component>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            vistaActiva: 'dashboard',
            comprador: {},
            loading: true,
            error: null,
            
            // Resumen rápido
            pedidosActivos: 0,
            totalCompras: 0,
            totalGastado: 0
        }
    },
    
    computed: {
        componenteActual() {
            const componentes = {
                'dashboard': 'DashboardComprador',
                'catalogo': 'CatalogoComprador',
                'historial': 'HistorialCompras',
                'perfil': 'PerfilComprador'
            };
            return componentes[this.vistaActiva] || 'DashboardComprador';
        }
    },
    
    async mounted() {
        console.log(' CompradorDashboard montado');
        await this.inicializarDashboard();
    },
    
    methods: {
        async inicializarDashboard() {
            try {
                console.log(' Inicializando dashboard del comprador...');
                
                // Verificar autenticación
                const token = localStorage.getItem('authToken');
                if (!token) {
                    console.warn(' No hay token');
                    this.redirigirALogin();
                    return;
                }

                // Cargar datos del comprador
                await this.cargarDatosComprador();
                
                // Cargar resumen rápido
                await this.cargarResumenRapido();
                
                this.loading = false;
                console.log(' Dashboard del comprador inicializado');
            } catch (error) {
                console.error(' Error inicializando dashboard:', error);
                this.error = error.message;
                this.loading = false;
                
                if (error.message.includes('401') || error.message.includes('Token')) {
                    this.redirigirALogin();
                }
            }
        },
        
        async cargarDatosComprador() {
            const data = await apiUtilsComprador.fetchWithAuth('/api/AuthApi/me');
            
            if (data.success && data.user) {
                if (data.user.rol !== 'Comprador') {
                    throw new Error('No eres un comprador autorizado');
                }

                this.comprador = {
                    id: data.user.id,
                    nombre: data.user.nombre,
                    email: data.user.email
                };
                
                console.log(' Comprador configurado:', this.comprador);
            }
        },
        
        async cargarResumenRapido() {
            try {
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const compradorId = user.id;
                
                // Cargar pagos del comprador
                const pagosData = await apiUtilsComprador.fetchWithAuth('/api/PagoApi');
                const todosPagos = pagosData.items || pagosData || [];
                const misPagos = todosPagos.filter(p => p.compradorId === compradorId);
                
                // Pedidos activos
                this.pedidosActivos = misPagos.filter(p => 
                    p.estado === 'Pendiente' || p.estado === 0
                ).length;
                
                // Total de compras
                this.totalCompras = misPagos.length;
                
                // Total gastado
                this.totalGastado = misPagos
                    .filter(p => p.estado === 'Completado' || p.estado === 1)
                    .reduce((sum, p) => sum + (p.monto || 0), 0);
                    
            } catch (error) {
                console.error('Error cargando resumen rápido:', error);
            }
        },
        
        cambiarVista(vista) {
            console.log(' Cambiando vista a:', vista);
            this.vistaActiva = vista;
        },
        
        logout() {
            if (confirm('¿Estás seguro de que quieres cerrar sesión?')) {
                console.log('🚪 Cerrando sesión...');
                localStorage.removeItem('authToken');
                localStorage.removeItem('user');
                window.location.href = '/Account/Login';
            }
        },
        
        redirigirALogin() {
            console.log('🔄 Redirigiendo a login...');
            localStorage.removeItem('authToken');
            localStorage.removeItem('user');
            alert('Sesión expirada. Por favor inicia sesión nuevamente.');
            window.location.href = '/Account/Login';
        },
        
        formatMoneda(valor) {
            return apiUtilsComprador.formatMoneda(valor);
        }
    }
};

// ==================== INICIALIZACIÓN VUE ====================
console.log(' Iniciando aplicación Vue del Comprador...');

function inicializarVueComprador() {
    try {
        console.log('🔧 Configurando Vue app del comprador...');
        
        const app = Vue.createApp(CompradorDashboard);
        
        // Registrar componentes
        app.component('DashboardComprador', DashboardComprador);
        app.component('CatalogoComprador', CatalogoComprador);
        app.component('HistorialCompras', HistorialCompras);
        app.component('PerfilComprador', PerfilComprador);
        
        app.mount('#app');
        console.log(' Aplicación Vue del comprador montada correctamente');
        
    } catch (error) {
        console.error('Error inicializando Vue del comprador:', error);
        document.getElementById('app').innerHTML = `
            <div class="alert alert-danger m-4">
                <h4>Error al cargar el dashboard del comprador</h4>
                <p>${error.message}</p>
                <pre class="small">${error.stack}</pre>
                <button class="btn btn-primary mt-3" onclick="window.location.reload()">Reintentar</button>
            </div>
        `;
    }
}

// Esperar a que el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', inicializarVueComprador);
} else {
    inicializarVueComprador();
}