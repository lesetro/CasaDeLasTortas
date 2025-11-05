console.log('✅ vendedor-dashboard.js cargado');

// Verificar Vue
console.log('Vue disponible:', typeof Vue !== 'undefined');

// ==================== UTILIDADES ====================
const apiUtils = {
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

// ==================== COMPONENTE: ESTADÍSTICAS VENDEDOR (CON DATOS REALES) ====================
const EstadisticasVendedor = {
    name: 'EstadisticasVendedor',
    template: `
        <div class="estadisticas-vendedor">
            <!-- Tarjetas de Resumen -->
            <div class="row g-3 mb-4">
                <div class="col-md-3 col-sm-6">
                    <div class="card bg-primary text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Ventas Hoy</h6>
                                    <h3 class="mb-0 fw-bold">{{ ventasHoy }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-shopping-cart fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-dollar-sign me-1"></i>
                                {{ formatMoneda(ingresosHoy) }}
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-success text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Tortas Activas</h6>
                                    <h3 class="mb-0 fw-bold">{{ tortasActivas }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-birthday-cake fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-box me-1"></i>
                                {{ totalStock }} unidades en stock
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-warning text-dark h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Pedidos Pendientes</h6>
                                    <h3 class="mb-0 fw-bold">{{ pedidosPendientes }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-clock fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-exclamation-circle me-1"></i>
                                Requieren atención
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3 col-sm-6">
                    <div class="card bg-info text-white h-100 shadow">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="text-uppercase mb-1 opacity-75">Ingresos del Mes</h6>
                                    <h3 class="mb-0 fw-bold">{{ formatMoneda(ingresosMes) }}</h3>
                                </div>
                                <div class="opacity-50">
                                    <i class="fas fa-dollar-sign fa-3x"></i>
                                </div>
                            </div>
                            <div class="mt-2 small">
                                <i class="fas fa-chart-line me-1"></i>
                                {{ ventasMes }} ventas este mes
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Gráficos y Detalles -->
            <div class="row">
                <!-- Ventas de la Semana -->
                <div class="col-lg-8 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-header bg-white">
                            <h5 class="mb-0">
                                <i class="fas fa-chart-bar me-2"></i>
                                Ventas de los Últimos 7 Días
                            </h5>
                        </div>
                        <div class="card-body">
                            <div v-if="ventasSemana.length > 0" class="table-responsive">
                                <table class="table table-sm">
                                    <tbody>
                                        <tr v-for="(dia, index) in ventasSemana" :key="index">
                                            <td class="fw-bold" style="width: 100px;">{{ dia.fecha }}</td>
                                            <td>
                                                <div class="progress" style="height: 25px;">
                                                    <div class="progress-bar bg-success" 
                                                         :style="{ width: calcularPorcentaje(dia.cantidad) + '%' }"
                                                         :title="dia.cantidad + ' ventas'">
                                                        {{ dia.cantidad }} {{ dia.cantidad === 1 ? 'venta' : 'ventas' }}
                                                    </div>
                                                </div>
                                            </td>
                                            <td class="text-end fw-bold text-success" style="width: 120px;">
                                                {{ formatMoneda(dia.monto) }}
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div v-else class="text-center py-4 text-muted">
                                <i class="fas fa-chart-line fa-3x mb-3 opacity-50"></i>
                                <p>No hay datos de ventas en los últimos 7 días</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Top Tortas Más Vendidas -->
                <div class="col-lg-4 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-header bg-white">
                            <h5 class="mb-0">
                                <i class="fas fa-trophy me-2 text-warning"></i>
                                Top 5 Tortas
                            </h5>
                        </div>
                        <div class="card-body">
                            <div v-if="topTortas.length > 0">
                                <div v-for="(torta, index) in topTortas" :key="torta.id" 
                                     class="d-flex align-items-center mb-3 pb-3 border-bottom">
                                    <div class="me-3">
                                        <span class="badge rounded-pill" 
                                              :class="index === 0 ? 'bg-warning text-dark' : 'bg-secondary'"
                                              style="font-size: 1.1em; width: 30px; height: 30px; line-height: 20px;">
                                            {{ index + 1 }}
                                        </span>
                                    </div>
                                    <img v-if="torta.imagenPrincipal" 
                                         :src="torta.imagenPrincipal" 
                                         class="rounded me-3" 
                                         style="width: 50px; height: 50px; object-fit: cover;">
                                    <div class="flex-grow-1">
                                        <div class="fw-bold small">{{ torta.nombre }}</div>
                                        <div class="text-muted small">
                                            <i class="fas fa-shopping-cart me-1"></i>{{ torta.vecesVendida }} ventas
                                        </div>
                                    </div>
                                    <div class="text-end">
                                        <div class="fw-bold text-success">{{ formatMoneda(torta.precio) }}</div>
                                    </div>
                                </div>
                            </div>
                            <div v-else class="text-center py-4 text-muted">
                                <i class="fas fa-birthday-cake fa-3x mb-3 opacity-50"></i>
                                <p>No hay ventas registradas aún</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Estadísticas Adicionales -->
            <div class="row">
                <div class="col-md-4 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <h6 class="text-muted mb-3">
                                <i class="fas fa-chart-pie me-2"></i>Promedio de Venta
                            </h6>
                            <h3 class="fw-bold mb-0">{{ formatMoneda(promedioVenta) }}</h3>
                            <p class="text-muted small mb-0 mt-2">Por pedido</p>
                        </div>
                    </div>
                </div>

                <div class="col-md-4 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <h6 class="text-muted mb-3">
                                <i class="fas fa-star me-2"></i>Calificación Promedio
                            </h6>
                            <h3 class="fw-bold mb-0">
                                {{ calificacionPromedio.toFixed(1) }}
                                <small class="text-muted">/5</small>
                            </h3>
                            <div class="mt-2">
                                <i v-for="n in 5" :key="n" 
                                   :class="n <= Math.round(calificacionPromedio) ? 'fas fa-star text-warning' : 'far fa-star text-muted'">
                                </i>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-4 mb-4">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <h6 class="text-muted mb-3">
                                <i class="fas fa-users me-2"></i>Clientes Únicos
                            </h6>
                            <h3 class="fw-bold mb-0">{{ clientesUnicos }}</h3>
                            <p class="text-muted small mb-0 mt-2">Han comprado tus tortas</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    
    data() {
        return {
            // Estadísticas principales
            ventasHoy: 0,
            ingresosHoy: 0,
            tortasActivas: 0,
            totalStock: 0,
            pedidosPendientes: 0,
            ingresosMes: 0,
            ventasMes: 0,
            
            // Datos detallados
            ventasSemana: [],
            topTortas: [],
            
            // Estadísticas adicionales
            promedioVenta: 0,
            calificacionPromedio: 0,
            clientesUnicos: 0,
            
            // Control
            loading: true
        }
    },
    
    async mounted() {
        await this.cargarEstadisticas();
    },
    
    methods: {
        async cargarEstadisticas() {
            try {
                this.loading = true;
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const vendedorId = user.vendedorId || user.id;
                
                console.log('🔍 Cargando estadísticas para vendedor ID:', vendedorId);
                
                // Cargar tortas del vendedor
                await this.cargarTortas(vendedorId);
                
                // Cargar pagos/ventas
                await this.cargarVentas(vendedorId);
                
                console.log('✅ Estadísticas cargadas');
            } catch (error) {
                console.error('❌ Error cargando estadísticas:', error);
            } finally {
                this.loading = false;
            }
        },
        
        async cargarTortas(vendedorId) {
            try {
                const data = await apiUtils.fetchWithAuth('/api/TortaApi');
                const todasLasTortas = data.items || data || [];
                
                // Filtrar solo las tortas del vendedor actual
                const misTortas = todasLasTortas.filter(t => t.vendedorId === vendedorId);
                
                console.log('🎂 Mis tortas encontradas:', misTortas.length);
                
                this.tortasActivas = misTortas.filter(t => t.disponible && t.stock > 0).length;
                this.totalStock = misTortas.reduce((sum, t) => sum + (t.stock || 0), 0);
                
                // Top 5 tortas más vendidas
                this.topTortas = misTortas
                    .filter(t => t.vecesVendida > 0)
                    .sort((a, b) => (b.vecesVendida || 0) - (a.vecesVendida || 0))
                    .slice(0, 5)
                    .map(t => ({
                        id: t.id,
                        nombre: t.nombre,
                        precio: t.precio,
                        vecesVendida: t.vecesVendida || 0,
                        imagenPrincipal: this.obtenerImagenPrincipal(t)
                    }));
                
                // Calificación promedio
                const tortasConCalif = misTortas.filter(t => t.calificacion > 0);
                this.calificacionPromedio = tortasConCalif.length > 0
                    ? tortasConCalif.reduce((sum, t) => sum + t.calificacion, 0) / tortasConCalif.length
                    : 0;
                    
            } catch (error) {
                console.error('Error cargando tortas:', error);
            }
        },
        
        async cargarVentas(vendedorId) {
            try {
                const data = await apiUtils.fetchWithAuth('/api/PagoApi');
                const todosPagos = data.items || data || [];
                
                // Filtrar pagos del vendedor
                const misPagos = todosPagos.filter(p => p.vendedorId === vendedorId);
                
                console.log('💰 Mis pagos encontrados:', misPagos.length);
                
                // Pedidos pendientes
                this.pedidosPendientes = misPagos.filter(p => 
                    p.estado === 'Pendiente' || p.estado === 0
                ).length;
                
                // Ventas de hoy
                const hoy = new Date().toDateString();
                const ventasHoyArray = misPagos.filter(p => {
                    const fechaPago = new Date(p.fechaPago).toDateString();
                    const esHoy = fechaPago === hoy;
                    const esCompletado = p.estado === 'Completado' || p.estado === 1;
                    return esHoy && esCompletado;
                });
                
                this.ventasHoy = ventasHoyArray.length;
                this.ingresosHoy = ventasHoyArray.reduce((sum, p) => sum + (p.monto || 0), 0);
                
                // Ingresos del mes
                const mesActual = new Date().getMonth();
                const añoActual = new Date().getFullYear();
                
                const ventasMesArray = misPagos.filter(p => {
                    const fecha = new Date(p.fechaPago);
                    const esEsteMes = fecha.getMonth() === mesActual && fecha.getFullYear() === añoActual;
                    const esCompletado = p.estado === 'Completado' || p.estado === 1;
                    return esEsteMes && esCompletado;
                });
                
                this.ventasMes = ventasMesArray.length;
                this.ingresosMes = ventasMesArray.reduce((sum, p) => sum + (p.monto || 0), 0);
                
                // Promedio de venta
                const ventasCompletadas = misPagos.filter(p => 
                    p.estado === 'Completado' || p.estado === 1
                );
                this.promedioVenta = ventasCompletadas.length > 0
                    ? ventasCompletadas.reduce((sum, p) => sum + (p.monto || 0), 0) / ventasCompletadas.length
                    : 0;
                
                // Clientes únicos
                const clientesSet = new Set(misPagos.map(p => p.compradorId));
                this.clientesUnicos = clientesSet.size;
                
                // Ventas de la semana
                this.calcularVentasSemana(misPagos);
                
            } catch (error) {
                console.error('Error cargando ventas:', error);
            }
        },
        
        calcularVentasSemana(pagos) {
            const diasSemana = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
            const ventasPorDia = [];
            
            for (let i = 6; i >= 0; i--) {
                const fecha = new Date();
                fecha.setDate(fecha.getDate() - i);
                const fechaStr = fecha.toDateString();
                
                const ventasDelDia = pagos.filter(p => {
                    const fechaPago = new Date(p.fechaPago).toDateString();
                    const esCompletado = p.estado === 'Completado' || p.estado === 1;
                    return fechaPago === fechaStr && esCompletado;
                });
                
                ventasPorDia.push({
                    fecha: `${diasSemana[fecha.getDay()]} ${fecha.getDate()}/${fecha.getMonth() + 1}`,
                    cantidad: ventasDelDia.length,
                    monto: ventasDelDia.reduce((sum, p) => sum + (p.monto || 0), 0)
                });
            }
            
            this.ventasSemana = ventasPorDia;
        },
        
        obtenerImagenPrincipal(torta) {
            if (torta.imagenes && torta.imagenes.length > 0) {
                const principal = torta.imagenes.find(img => img.esPrincipal);
                return principal ? principal.urlImagen : torta.imagenes[0].urlImagen;
            }
            return null;
        },
        
        calcularPorcentaje(cantidad) {
            const maxVentas = Math.max(...this.ventasSemana.map(v => v.cantidad), 1);
            return (cantidad / maxVentas) * 100;
        },
        
        formatMoneda(valor) {
            return apiUtils.formatMoneda(valor);
        }
    }
};

// ==================== COMPONENTE: MIS TORTAS (CON DATOS REALES) ====================
const MisTortas = {
    name: 'MisTortas',
    template: `
        <div class="mis-tortas">
            <div class="card shadow-sm">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-birthday-cake me-2"></i>Mis Tortas
                        <span class="badge bg-primary ms-2">{{ tortas.length }}</span>
                    </h5>
                    <button class="btn btn-success btn-sm" @click="crearTorta">
                        <i class="fas fa-plus me-2"></i>Nueva Torta
                    </button>
                </div>
                <div class="card-body">
                    <!-- Loading -->
                    <div v-if="loading" class="text-center py-5">
                        <div class="spinner-border text-primary mb-3"></div>
                        <p class="text-muted">Cargando tortas...</p>
                    </div>

                    <!-- Sin tortas -->
                    <div v-else-if="tortas.length === 0" class="text-center py-5">
                        <i class="fas fa-box-open fa-4x text-muted mb-3"></i>
                        <h5 class="text-muted">No tienes tortas publicadas</h5>
                        <p class="text-muted mb-4">Comienza a vender creando tu primera torta</p>
                        <button class="btn btn-success" @click="crearTorta">
                            <i class="fas fa-plus me-2"></i>Crear Mi Primera Torta
                        </button>
                    </div>

                    <!-- Lista de tortas -->
                    <div v-else class="row g-3">
                        <div v-for="torta in tortas" :key="torta.id" class="col-md-6 col-lg-4">
                            <div class="card h-100 shadow-sm">
                                <!-- Imagen -->
                                <div class="position-relative">
                                    <img v-if="torta.imagenPrincipal" 
                                         :src="torta.imagenPrincipal" 
                                         class="card-img-top" 
                                         style="height: 200px; object-fit: cover;">
                                    <div v-else class="bg-light d-flex align-items-center justify-content-center" 
                                         style="height: 200px;">
                                        <i class="fas fa-birthday-cake fa-4x text-muted"></i>
                                    </div>
                                    
                                    <!-- Badge de estado -->
                                    <span class="position-absolute top-0 end-0 m-2 badge" 
                                          :class="torta.disponible ? 'bg-success' : 'bg-danger'">
                                        {{ torta.disponible ? 'Activa' : 'Inactiva' }}
                                    </span>
                                    
                                    <!-- Badge de stock bajo -->
                                    <span v-if="torta.stock <= 2 && torta.disponible" 
                                          class="position-absolute top-0 start-0 m-2 badge bg-warning text-dark">
                                        <i class="fas fa-exclamation-triangle me-1"></i>Stock Bajo
                                    </span>
                                </div>
                                
                                <div class="card-body">
                                    <h5 class="card-title text-truncate" :title="torta.nombre">
                                        {{ torta.nombre }}
                                    </h5>
                                    
                                    <p class="card-text text-muted small" style="height: 60px; overflow: hidden;">
                                        {{ torta.descripcion }}
                                    </p>
                                    
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <div>
                                            <div class="h5 text-success mb-0">{{ formatMoneda(torta.precio) }}</div>
                                            <small class="text-muted">
                                                <i class="fas fa-box me-1"></i>Stock: {{ torta.stock }}
                                            </small>
                                        </div>
                                        <div class="text-end">
                                            <div class="small text-muted">
                                                <i class="fas fa-shopping-cart me-1"></i>
                                                {{ torta.vecesVendida || 0 }} ventas
                                            </div>
                                            <div class="small">
                                                <i class="fas fa-star text-warning me-1"></i>
                                                {{ (torta.calificacion || 0).toFixed(1) }}/5
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <!-- Botones de acción -->
                                    <div class="btn-group w-100" role="group">
                                        <button class="btn btn-sm btn-outline-primary" 
                                                @click="editarTorta(torta)"
                                                title="Editar">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm" 
                                                :class="torta.disponible ? 'btn-outline-warning' : 'btn-outline-success'"
                                                @click="toggleDisponibilidad(torta)"
                                                :title="torta.disponible ? 'Ocultar' : 'Mostrar'">
                                            <i :class="torta.disponible ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-info" 
                                                @click="gestionarStock(torta)"
                                                title="Gestionar Stock">
                                            <i class="fas fa-boxes"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger" 
                                                @click="eliminarTorta(torta)"
                                                title="Eliminar">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </div>
                                </div>
                                
                                <!-- Footer con categoría -->
                                <div class="card-footer bg-white border-top">
                                    <div class="d-flex justify-content-between align-items-center small">
                                        <span v-if="torta.categoria" class="badge bg-secondary">
                                            {{ torta.categoria }}
                                        </span>
                                        <span v-else class="text-muted">Sin categoría</span>
                                        
                                        <span class="text-muted">
                                            {{ torta.tamanio || 'Mediana' }}
                                        </span>
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
            loading: true
        }
    },
    
    async mounted() {
        await this.cargarTortas();
    },
    
    methods: {
        async cargarTortas() {
            try {
                this.loading = true;
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const vendedorId = user.vendedorId || user.id;
                
                const data = await apiUtils.fetchWithAuth('/api/TortaApi');
                const todasLasTortas = data.items || data || [];
                
                // Filtrar tortas del vendedor actual
                this.tortas = todasLasTortas
                    .filter(t => t.vendedorId === vendedorId)
                    .map(t => ({
                        ...t,
                        imagenPrincipal: this.obtenerImagenPrincipal(t)
                    }));
                
                console.log('✅ Tortas cargadas:', this.tortas.length);
            } catch (error) {
                console.error('❌ Error cargando tortas:', error);
                this.mostrarMensaje('Error al cargar las tortas', 'error');
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
        
        crearTorta() {
            this.mostrarMensaje('Función de crear torta en desarrollo', 'info');
            // Aquí se abriría un modal o se navegaría a un formulario
        },
        
        editarTorta(torta) {
            this.mostrarMensaje(`Editar torta: ${torta.nombre}`, 'info');
            // Aquí se abriría un modal con el formulario de edición
        },
        
        async toggleDisponibilidad(torta) {
            const accion = torta.disponible ? 'ocultar' : 'mostrar';
            if (!confirm(`¿Estás seguro de ${accion} la torta "${torta.nombre}"?`)) {
                return;
            }

            try {
                await apiUtils.fetchWithAuth(`/api/TortaApi/${torta.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        ...torta,
                        disponible: !torta.disponible
                    })
                });

                torta.disponible = !torta.disponible;
                this.mostrarMensaje(`Torta ${accion === 'ocultar' ? 'ocultada' : 'mostrada'} correctamente`, 'success');
            } catch (error) {
                console.error('Error actualizando disponibilidad:', error);
                this.mostrarMensaje('Error al actualizar la disponibilidad', 'error');
            }
        },
        
        gestionarStock(torta) {
            const nuevoStock = prompt(`Stock actual: ${torta.stock}\n\nIngresa el nuevo stock:`, torta.stock);
            
            if (nuevoStock !== null) {
                const stock = parseInt(nuevoStock);
                if (isNaN(stock) || stock < 0) {
                    this.mostrarMensaje('Stock inválido', 'error');
                    return;
                }
                
                // Actualizar stock
                torta.stock = stock;
                this.mostrarMensaje('Stock actualizado correctamente', 'success');
            }
        },
        
        async eliminarTorta(torta) {
            if (!confirm(`¿Estás SEGURO de eliminar la torta "${torta.nombre}"?\n\nEsta acción no se puede deshacer.`)) {
                return;
            }

            try {
                await apiUtils.fetchWithAuth(`/api/TortaApi/${torta.id}`, {
                    method: 'DELETE'
                });

                this.tortas = this.tortas.filter(t => t.id !== torta.id);
                this.mostrarMensaje('Torta eliminada correctamente', 'success');
            } catch (error) {
                console.error('Error eliminando torta:', error);
                this.mostrarMensaje('Error al eliminar la torta', 'error');
            }
        },
        
        formatMoneda(valor) {
            return apiUtils.formatMoneda(valor);
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

// ==================== COMPONENTE: MIS PEDIDOS (CON DATOS REALES) ====================
const MisPedidos = {
    name: 'MisPedidos',
    template: `
        <div class="mis-pedidos">
            <div class="card shadow-sm">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-shopping-bag me-2"></i>Mis Pedidos
                        <span class="badge bg-primary ms-2">{{ pedidosFiltrados.length }}</span>
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
                        <p class="text-muted">Cargando pedidos...</p>
                    </div>

                    <!-- Sin pedidos -->
                    <div v-else-if="pedidosFiltrados.length === 0" class="text-center py-5">
                        <i class="fas fa-inbox fa-4x text-muted mb-3"></i>
                        <h5 class="text-muted">No hay pedidos {{ filtroEstado ? 'con este estado' : '' }}</h5>
                        <p class="text-muted">Los pedidos de tus clientes aparecerán aquí</p>
                    </div>

                    <!-- Lista de pedidos -->
                    <div v-else class="table-responsive">
                        <table class="table table-hover align-middle">
                            <thead class="table-light">
                                <tr>
                                    <th style="width: 80px;">ID</th>
                                    <th>Cliente</th>
                                    <th>Torta</th>
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
                                        <div>{{ pedido.nombreComprador || 'Cliente' }}</div>
                                        <small class="text-muted" v-if="pedido.telefonoComprador">
                                            <i class="fas fa-phone me-1"></i>{{ pedido.telefonoComprador }}
                                        </small>
                                    </td>
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
                                                    class="btn btn-outline-success"
                                                    @click="completarPedido(pedido)"
                                                    title="Marcar como completado">
                                                <i class="fas fa-check"></i>
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
                const vendedorId = user.vendedorId || user.id;
                
                const data = await apiUtils.fetchWithAuth('/api/PagoApi');
                const todosPagos = data.items || data || [];
                
                // Filtrar pagos del vendedor y enriquecer con datos
                this.pedidos = todosPagos
                    .filter(p => p.vendedorId === vendedorId)
                    .map(p => ({
                        ...p,
                        nombreComprador: p.comprador?.persona?.nombreCompleto || 'Cliente',
                        telefonoComprador: p.comprador?.telefono,
                        nombreTorta: p.torta?.nombre || 'Torta',
                        imagenTorta: this.obtenerImagenTorta(p.torta)
                    }))
                    .sort((a, b) => new Date(b.fechaPago) - new Date(a.fechaPago));
                
                console.log('✅ Pedidos cargados:', this.pedidos.length);
            } catch (error) {
                console.error('❌ Error cargando pedidos:', error);
                this.mostrarMensaje('Error al cargar los pedidos', 'error');
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
        
        verDetalle(pedido) {
            const detalle = `
DETALLE DEL PEDIDO #${pedido.id}

Cliente: ${pedido.nombreComprador}
Teléfono: ${pedido.telefonoComprador || 'No especificado'}

Torta: ${pedido.nombreTorta}
Cantidad: ${pedido.cantidad}
Precio Unitario: ${this.formatMoneda(pedido.precioUnitario)}
Subtotal: ${this.formatMoneda(pedido.subtotal)}
${pedido.descuento > 0 ? `Descuento: ${this.formatMoneda(pedido.descuento)}\n` : ''}
TOTAL: ${this.formatMoneda(pedido.monto)}

Método de Pago: ${this.getMetodoPagoTexto(pedido.metodoPago)}
Estado: ${this.getEstadoTexto(pedido.estado)}
Fecha: ${this.formatFecha(pedido.fechaPago)} ${this.formatHora(pedido.fechaPago)}

${pedido.direccionEntrega ? `Dirección de Entrega:\n${pedido.direccionEntrega}\n` : ''}
${pedido.fechaEntrega ? `Fecha de Entrega: ${this.formatFecha(pedido.fechaEntrega)}\n` : ''}
${pedido.observaciones ? `Observaciones:\n${pedido.observaciones}` : ''}
            `;
            
            alert(detalle);
        },
        
        async completarPedido(pedido) {
            if (!confirm(`¿Marcar el pedido #${pedido.id} como COMPLETADO?`)) {
                return;
            }

            try {
                await apiUtils.fetchWithAuth(`/api/PagoApi/${pedido.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        ...pedido,
                        estado: 1 // Completado
                    })
                });

                pedido.estado = 1;
                this.mostrarMensaje('Pedido completado correctamente', 'success');
            } catch (error) {
                console.error('Error completando pedido:', error);
                this.mostrarMensaje('Error al completar el pedido', 'error');
            }
        },
        
        async cancelarPedido(pedido) {
            const motivo = prompt(`¿Por qué deseas cancelar el pedido #${pedido.id}?`);
            if (!motivo) return;

            try {
                await apiUtils.fetchWithAuth(`/api/PagoApi/${pedido.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        ...pedido,
                        estado: 2, // Cancelado
                        observaciones: `${pedido.observaciones || ''}\n[CANCELADO] ${motivo}`
                    })
                });

                pedido.estado = 2;
                this.mostrarMensaje('Pedido cancelado', 'success');
            } catch (error) {
                console.error('Error cancelando pedido:', error);
                this.mostrarMensaje('Error al cancelar el pedido', 'error');
            }
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
            return apiUtils.formatMoneda(valor);
        },
        
        formatFecha(fecha) {
            return apiUtils.formatFecha(fecha);
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

// ==================== COMPONENTE: PERFIL VENDEDOR ====================
const PerfilVendedor = {
    name: 'PerfilVendedor',
    template: `
        <div class="perfil-vendedor">
            <div class="card shadow-sm">
                <div class="card-header bg-white">
                    <h5 class="mb-0">
                        <i class="fas fa-user me-2"></i>Mi Perfil de Vendedor
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
                                <img v-if="vendedor.avatar" 
                                     :src="vendedor.avatar" 
                                     class="rounded-circle img-thumbnail mb-3" 
                                     style="width: 150px; height: 150px; object-fit: cover;">
                                <div v-else class="rounded-circle bg-light d-flex align-items-center justify-content-center mx-auto mb-3"
                                     style="width: 150px; height: 150px;">
                                    <i class="fas fa-user fa-4x text-muted"></i>
                                </div>
                            </div>
                            
                            <h4>{{ vendedor.nombreComercial || vendedor.nombre }}</h4>
                            <p class="text-muted">{{ vendedor.especialidad || 'Pastelería Artesanal' }}</p>
                            
                            <div class="mb-3">
                                <span v-if="vendedor.verificado" class="badge bg-success">
                                    <i class="fas fa-check-circle me-1"></i>Verificado
                                </span>
                                <span v-else class="badge bg-warning text-dark">
                                    <i class="fas fa-clock me-1"></i>Pendiente verificación
                                </span>
                            </div>
                            
                            <!-- Estadísticas del Vendedor -->
                            <div class="card bg-light">
                                <div class="card-body">
                                    <h6 class="text-muted mb-3">Estadísticas</h6>
                                    <div class="mb-2">
                                        <small class="text-muted">Calificación</small>
                                        <div>
                                            <i v-for="n in 5" :key="n" 
                                               :class="n <= Math.round(vendedor.calificacion || 0) ? 'fas fa-star text-warning' : 'far fa-star text-muted'">
                                            </i>
                                            <strong class="ms-2">{{ (vendedor.calificacion || 0).toFixed(1) }}/5</strong>
                                        </div>
                                    </div>
                                    <hr>
                                    <div class="mb-2">
                                        <small class="text-muted">Total de Ventas</small>
                                        <div class="h5 mb-0">{{ vendedor.totalVentas || 0 }}</div>
                                    </div>
                                    <hr>
                                    <div class="mb-2">
                                        <small class="text-muted">Miembro desde</small>
                                        <div class="small">{{ formatFecha(vendedor.fechaCreacion) }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Columna Derecha: Información -->
                        <div class="col-md-8">
                            <h5 class="mb-3">Información del Negocio</h5>
                            
                            <table class="table table-borderless">
                                <tr>
                                    <td class="text-muted" style="width: 150px;">Nombre Comercial:</td>
                                    <td><strong>{{ vendedor.nombreComercial || 'No especificado' }}</strong></td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Especialidad:</td>
                                    <td>{{ vendedor.especialidad || 'No especificada' }}</td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Descripción:</td>
                                    <td>{{ vendedor.descripcion || 'No especificada' }}</td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Horario:</td>
                                    <td>{{ vendedor.horario || 'No especificado' }}</td>
                                </tr>
                            </table>
                            
                            <hr>
                            
                            <h5 class="mb-3">Información Personal</h5>
                            
                            <table class="table table-borderless">
                                <tr>
                                    <td class="text-muted" style="width: 150px;">Nombre Completo:</td>
                                    <td><strong>{{ vendedor.nombre }} {{ vendedor.apellido }}</strong></td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Email:</td>
                                    <td>
                                        <i class="fas fa-envelope me-2 text-muted"></i>
                                        {{ vendedor.email }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Teléfono:</td>
                                    <td>
                                        <i class="fas fa-phone me-2 text-muted"></i>
                                        {{ vendedor.telefono || 'No especificado' }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Dirección:</td>
                                    <td>
                                        <i class="fas fa-map-marker-alt me-2 text-muted"></i>
                                        {{ vendedor.direccion || 'No especificada' }}
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-muted">Último Acceso:</td>
                                    <td>{{ formatFechaHora(vendedor.ultimoAcceso) }}</td>
                                </tr>
                            </table>
                            
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
            vendedor: {},
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
                const data = await apiUtils.fetchWithAuth('/api/AuthApi/me');
                
                if (data.success && data.user) {
                    // Combinar información de persona y vendedor
                    this.vendedor = {
                        // Datos de persona
                        id: data.user.id,
                        nombre: data.user.nombre,
                        apellido: data.user.apellido || '',
                        email: data.user.email,
                        telefono: data.user.telefono,
                        direccion: data.user.direccion,
                        avatar: data.user.avatar,
                        ultimoAcceso: data.user.ultimoAcceso,
                        
                        // Datos de vendedor
                        vendedorId: data.user.rolData?.vendedorId,
                        nombreComercial: data.user.rolData?.nombreComercial || data.user.nombre,
                        especialidad: data.user.rolData?.especialidad,
                        descripcion: data.user.rolData?.descripcion,
                        calificacion: data.user.rolData?.calificacion || 0,
                        totalVentas: data.user.rolData?.totalVentas || 0,
                        horario: data.user.rolData?.horario,
                        verificado: data.user.rolData?.verificado || false,
                        fechaCreacion: data.user.rolData?.fechaCreacion || data.user.fechaRegistro
                    };
                    
                    console.log('✅ Perfil cargado:', this.vendedor);
                }
            } catch (error) {
                console.error('❌ Error cargando perfil:', error);
                this.mostrarMensaje('Error al cargar el perfil', 'error');
            } finally {
                this.loading = false;
            }
        },
        
        editarPerfil() {
            this.mostrarMensaje('Función de edición de perfil en desarrollo', 'info');
            // Aquí se abriría un modal con un formulario
        },
        
        cambiarContrasena() {
            this.mostrarMensaje('Función de cambio de contraseña en desarrollo', 'info');
            // Aquí se abriría un modal con un formulario
        },
        
        formatFecha(fecha) {
            if (!fecha) return 'No disponible';
            return apiUtils.formatFecha(fecha);
        },
        
        formatFechaHora(fecha) {
            if (!fecha) return 'No disponible';
            return apiUtils.formatFechaHora(fecha);
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

// ==================== COMPONENTE PRINCIPAL: VENDEDOR DASHBOARD ====================
const VendedorDashboard = {
    name: 'VendedorDashboard',
    template: `
        <div class="vendedor-dashboard">
            <!-- Navbar -->
            <nav class="navbar navbar-expand-lg navbar-dark bg-success sticky-top shadow">
                <div class="container-fluid">
                    <a class="navbar-brand" href="#">
                        <i class="fas fa-store me-2"></i>
                        Casa de las Tortas - Panel de Vendedor
                    </a>
                    
                    <div class="navbar-nav ms-auto">
                        <span class="navbar-text text-white me-3">
                            <i class="fas fa-user me-1"></i>
                            {{ vendedor.nombreComercial || 'Cargando...' }}
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
                                            :class="vistaActiva === 'estadisticas' ? 'btn-success' : 'btn-outline-success'"
                                            @click="cambiarVista('estadisticas')">
                                        <i class="fas fa-chart-line me-2"></i>Dashboard
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'tortas' ? 'btn-success' : 'btn-outline-success'"
                                            @click="cambiarVista('tortas')">
                                        <i class="fas fa-birthday-cake me-2"></i>Mis Tortas
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'pedidos' ? 'btn-success' : 'btn-outline-success'"
                                            @click="cambiarVista('pedidos')">
                                        <i class="fas fa-shopping-bag me-2"></i>Pedidos
                                        <span v-if="pedidosPendientes > 0" class="badge bg-warning text-dark ms-2">
                                            {{ pedidosPendientes }}
                                        </span>
                                    </button>
                                    <button class="btn mb-2 text-start"
                                            :class="vistaActiva === 'perfil' ? 'btn-success' : 'btn-outline-success'"
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
                                    <span class="text-muted">Tortas activas:</span>
                                    <strong class="text-success">{{ tortasActivas }}</strong>
                                </div>
                                <div class="d-flex justify-content-between mb-2 small">
                                    <span class="text-muted">Pedidos hoy:</span>
                                    <strong class="text-primary">{{ pedidosHoy }}</strong>
                                </div>
                                <div class="d-flex justify-content-between small">
                                    <span class="text-muted">Ingresos mes:</span>
                                    <strong class="text-success">{{ formatMoneda(ingresosMes) }}</strong>
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
            vistaActiva: 'estadisticas',
            vendedor: {},
            loading: true,
            error: null,
            
            // Resumen rápido
            tortasActivas: 0,
            pedidosHoy: 0,
            pedidosPendientes: 0,
            ingresosMes: 0
        }
    },
    
    computed: {
        componenteActual() {
            const componentes = {
                'estadisticas': 'EstadisticasVendedor',
                'tortas': 'MisTortas',
                'pedidos': 'MisPedidos',
                'perfil': 'PerfilVendedor'
            };
            return componentes[this.vistaActiva] || 'EstadisticasVendedor';
        }
    },
    
    async mounted() {
        console.log('🎯 VendedorDashboard montado');
        await this.inicializarDashboard();
    },
    
    methods: {
        async inicializarDashboard() {
            try {
                console.log('🔄 Inicializando dashboard...');
                
                // Verificar autenticación
                const token = localStorage.getItem('authToken');
                if (!token) {
                    console.warn('❌ No hay token');
                    this.redirigirALogin();
                    return;
                }

                // Cargar datos del vendedor
                await this.cargarDatosVendedor();
                
                // Cargar resumen rápido
                await this.cargarResumenRapido();
                
                this.loading = false;
                console.log('✅ Dashboard inicializado');
            } catch (error) {
                console.error('❌ Error inicializando dashboard:', error);
                this.error = error.message;
                this.loading = false;
                
                if (error.message.includes('401') || error.message.includes('Token')) {
                    this.redirigirALogin();
                }
            }
        },
        
        async cargarDatosVendedor() {
            const data = await apiUtils.fetchWithAuth('/api/AuthApi/me');
            
            if (data.success && data.user) {
                if (data.user.rol !== 'Vendedor') {
                    throw new Error('No eres un vendedor autorizado');
                }

                this.vendedor = {
                    id: data.user.id,
                    nombre: data.user.nombre,
                    email: data.user.email,
                    vendedorId: data.user.rolData?.vendedorId,
                    nombreComercial: data.user.rolData?.nombreComercial || data.user.nombre
                };
                
                // Guardar vendedorId en user de localStorage para otros componentes
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                user.vendedorId = this.vendedor.vendedorId || this.vendedor.id;
                localStorage.setItem('user', JSON.stringify(user));
                
                console.log('✅ Vendedor configurado:', this.vendedor);
            }
        },
        
        async cargarResumenRapido() {
            try {
                const user = JSON.parse(localStorage.getItem('user') || '{}');
                const vendedorId = user.vendedorId || user.id;
                
                // Cargar tortas
                const tortasData = await apiUtils.fetchWithAuth('/api/TortaApi');
                const todasTortas = tortasData.items || tortasData || [];
                const misTortas = todasTortas.filter(t => t.vendedorId === vendedorId);
                this.tortasActivas = misTortas.filter(t => t.disponible && t.stock > 0).length;
                
                // Cargar pagos
                const pagosData = await apiUtils.fetchWithAuth('/api/PagoApi');
                const todosPagos = pagosData.items || pagosData || [];
                const misPagos = todosPagos.filter(p => p.vendedorId === vendedorId);
                
                // Pedidos pendientes
                this.pedidosPendientes = misPagos.filter(p => p.estado === 0).length;
                
                // Pedidos de hoy
                const hoy = new Date().toDateString();
                this.pedidosHoy = misPagos.filter(p => {
                    const esHoy = new Date(p.fechaPago).toDateString() === hoy;
                    const esCompletado = p.estado === 1;
                    return esHoy && esCompletado;
                }).length;
                
                // Ingresos del mes
                const mesActual = new Date().getMonth();
                const añoActual = new Date().getFullYear();
                this.ingresosMes = misPagos
                    .filter(p => {
                        const fecha = new Date(p.fechaPago);
                        return fecha.getMonth() === mesActual && 
                               fecha.getFullYear() === añoActual &&
                               p.estado === 1;
                    })
                    .reduce((sum, p) => sum + (p.monto || 0), 0);
                    
            } catch (error) {
                console.error('Error cargando resumen rápido:', error);
            }
        },
        
        cambiarVista(vista) {
            console.log('🔄 Cambiando vista a:', vista);
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
            return apiUtils.formatMoneda(valor);
        }
    }
};

// ==================== INICIALIZACIÓN VUE ====================
console.log('🚀 Iniciando aplicación Vue del Vendedor...');

function inicializarVue() {
    try {
        console.log('🔧 Configurando Vue app...');
        
        const app = Vue.createApp(VendedorDashboard);
        
        // Registrar componentes
        app.component('EstadisticasVendedor', EstadisticasVendedor);
        app.component('MisTortas', MisTortas);
        app.component('MisPedidos', MisPedidos);
        app.component('PerfilVendedor', PerfilVendedor);
        app.component('CrearTorta', CrearTorta);
        app.component('EditarTorta', EditarTorta);
        app.component('DetallePedido', DetallePedido);
        
        
        app.mount('#app');
        console.log('✅ Aplicación Vue montada correctamente');
        
    } catch (error) {
        console.error('❌ Error inicializando Vue:', error);
        document.getElementById('app').innerHTML = `
            <div class="alert alert-danger m-4">
                <h4>Error al cargar el dashboard</h4>
                <p>${error.message}</p>
                <pre class="small">${error.stack}</pre>
                <button class="btn btn-primary mt-3" onclick="window.location.reload()">Reintentar</button>
            </div>
        `;
    }
}

// Esperar a que el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', inicializarVue);
} else {
    inicializarVue();
}