// EstadisticasVendedor.vue - Dashboard de estadísticas del vendedor
const EstadisticasVendedor = {
    template: `
    <div class="estadisticas-vendedor">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2><i class="fas fa-chart-line text-success me-2"></i>Estadísticas y Resumen</h2>
            <button class="btn btn-outline-success" @click="actualizarEstadisticas">
                <i class="fas fa-sync-alt me-2"></i>Actualizar
            </button>
        </div>

        <!-- Tarjetas de Resumen -->
        <div class="row mb-4">
            <div class="col-md-3 mb-3">
                <div class="card text-white bg-primary shadow">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="card-title mb-0">Total Tortas</h6>
                                <h2 class="mb-0">{{ estadisticas.totalTortas }}</h2>
                            </div>
                            <i class="fas fa-birthday-cake fa-3x opacity-50"></i>
                        </div>
                        <small>{{ estadisticas.tortasActivas }} activas</small>
                    </div>
                </div>
            </div>

            <div class="col-md-3 mb-3">
                <div class="card text-white bg-success shadow">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="card-title mb-0">Ventas Totales</h6>
                                <h2 class="mb-0">{{ estadisticas.totalVentas }}</h2>
                            </div>
                            <i class="fas fa-chart-line fa-3x opacity-50"></i>
                        </div>
                        <small>Este mes: {{ estadisticas.ventasMes }}</small>
                    </div>
                </div>
            </div>

            <div class="col-md-3 mb-3">
                <div class="card text-white bg-info shadow">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="card-title mb-0">Ingresos</h6>
                                <h2 class="mb-0">\${{ estadisticas.ingresosTotales.toFixed(0) }}</h2>
                            </div>
                            <i class="fas fa-dollar-sign fa-3x opacity-50"></i>
                        </div>
                        <small>Este mes: \${{ estadisticas.ingresosMes.toFixed(0) }}</small>
                    </div>
                </div>
            </div>

            <div class="col-md-3 mb-3">
                <div class="card text-white bg-warning shadow">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="card-title mb-0">Pedidos Pendientes</h6>
                                <h2 class="mb-0">{{ estadisticas.pedidosPendientes }}</h2>
                            </div>
                            <i class="fas fa-clock fa-3x opacity-50"></i>
                        </div>
                        <small>Requieren atención</small>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tortas Más Vendidas -->
        <div class="row mb-4">
            <div class="col-lg-6 mb-4">
                <div class="card shadow">
                    <div class="card-header bg-white">
                        <h5 class="mb-0">
                            <i class="fas fa-trophy text-warning me-2"></i>
                            Top 5 Tortas Más Vendidas
                        </h5>
                    </div>
                    <div class="card-body">
                        <div v-if="cargando" class="text-center py-5">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Cargando...</span>
                            </div>
                        </div>
                        
                        <div v-else-if="tortasMasVendidas.length === 0" class="text-center text-muted py-5">
                            <i class="fas fa-inbox fa-3x mb-3"></i>
                            <p>Aún no tienes ventas registradas</p>
                        </div>
                        
                        <div v-else class="list-group list-group-flush">
                            <div v-for="(torta, index) in tortasMasVendidas" :key="torta.id"
                                 class="list-group-item d-flex justify-content-between align-items-center">
                                <div class="d-flex align-items-center">
                                    <span class="badge bg-success rounded-circle me-3" 
                                          style="width: 30px; height: 30px; line-height: 20px;">
                                        {{ index + 1 }}
                                    </span>
                                    <div>
                                        <strong>{{ torta.nombre }}</strong>
                                        <br>
                                        <small class="text-muted">\${{ torta.precio }} • {{ torta.totalVentas }} ventas</small>
                                    </div>
                                </div>
                                <span class="badge bg-primary">\${{ (torta.precio * torta.totalVentas).toFixed(2) }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Actividad Reciente -->
            <div class="col-lg-6 mb-4">
                <div class="card shadow">
                    <div class="card-header bg-white">
                        <h5 class="mb-0">
                            <i class="fas fa-history text-info me-2"></i>
                            Actividad Reciente
                        </h5>
                    </div>
                    <div class="card-body" style="max-height: 400px; overflow-y: auto;">
                        <div v-if="cargando" class="text-center py-5">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Cargando...</span>
                            </div>
                        </div>
                        
                        <div v-else-if="actividadReciente.length === 0" class="text-center text-muted py-5">
                            <i class="fas fa-history fa-3x mb-3"></i>
                            <p>No hay actividad reciente</p>
                        </div>
                        
                        <div v-else class="timeline">
                            <div v-for="actividad in actividadReciente" :key="actividad.id"
                                 class="timeline-item mb-3 pb-3 border-bottom">
                                <div class="d-flex">
                                    <div class="flex-shrink-0">
                                        <i :class="actividad.icon" class="fa-lg me-3"></i>
                                    </div>
                                    <div class="flex-grow-1">
                                        <strong>{{ actividad.titulo }}</strong>
                                        <p class="mb-1 text-muted small">{{ actividad.descripcion }}</p>
                                        <small class="text-muted">
                                            <i class="fas fa-clock me-1"></i>
                                            {{ formatearFecha(actividad.fecha) }}
                                        </small>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Gráfico de Ventas (Placeholder) -->
        <div class="row">
            <div class="col-12">
                <div class="card shadow">
                    <div class="card-header bg-white">
                        <h5 class="mb-0">
                            <i class="fas fa-chart-area text-primary me-2"></i>
                            Ventas de los Últimos 7 Días
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="alert alert-info">
                            <i class="fas fa-info-circle me-2"></i>
                            Gráfico de ventas en desarrollo. Próximamente disponible.
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `,
    
    props: {
        vendedorId: {
            type: Number,
            required: true
        }
    },
    
    data() {
        return {
            cargando: false,
            estadisticas: {
                totalTortas: 0,
                tortasActivas: 0,
                totalVentas: 0,
                ventasMes: 0,
                ingresosTotales: 0,
                ingresosMes: 0,
                pedidosPendientes: 0
            },
            tortasMasVendidas: [],
            actividadReciente: []
        }
    },
    
    async mounted() {
        await this.cargarEstadisticas();
    },
    
    methods: {
        async cargarEstadisticas() {
            this.cargando = true;
            
            try {
                await Promise.all([
                    this.cargarEstadisticasGenerales(),
                    this.cargarTortasMasVendidas(),
                    this.cargarActividadReciente()
                ]);
            } catch (error) {
                console.error('Error cargando estadísticas:', error);
                toastr.error('Error al cargar las estadísticas');
            } finally {
                this.cargando = false;
            }
        },
        
        async cargarEstadisticasGenerales() {
            try {
                const response = await fetch(
                    `/api/vendedor/${this.vendedorId}/estadisticas`,
                    {
                        headers: {
                            'Authorization': `Bearer ${window.authToken}`,
                            'Content-Type': 'application/json'
                        }
                    }
                );
                
                if (!response.ok) throw new Error('Error al cargar estadísticas');
                
                this.estadisticas = await response.json();
            } catch (error) {
                console.error('Error:', error);
            }
        },
        
        async cargarTortasMasVendidas() {
            try {
                const response = await fetch(
                    `/api/vendedor/${this.vendedorId}/tortas-mas-vendidas?limit=5`,
                    {
                        headers: {
                            'Authorization': `Bearer ${window.authToken}`,
                            'Content-Type': 'application/json'
                        }
                    }
                );
                
                if (!response.ok) throw new Error('Error al cargar tortas más vendidas');
                
                this.tortasMasVendidas = await response.json();
            } catch (error) {
                console.error('Error:', error);
                this.tortasMasVendidas = [];
            }
        },
        
        async cargarActividadReciente() {
            try {
                const response = await fetch(
                    `/api/vendedor/${this.vendedorId}/actividad-reciente?limit=10`,
                    {
                        headers: {
                            'Authorization': `Bearer ${window.authToken}`,
                            'Content-Type': 'application/json'
                        }
                    }
                );
                
                if (!response.ok) throw new Error('Error al cargar actividad');
                
                this.actividadReciente = await response.json();
            } catch (error) {
                console.error('Error:', error);
                this.actividadReciente = [];
            }
        },
        
        async actualizarEstadisticas() {
            await this.cargarEstadisticas();
            toastr.success('Estadísticas actualizadas');
        },
        
        formatearFecha(fecha) {
            const date = new Date(fecha);
            const ahora = new Date();
            const diff = Math.floor((ahora - date) / 1000); // segundos
            
            if (diff < 60) return 'Hace un momento';
            if (diff < 3600) return `Hace ${Math.floor(diff / 60)} minutos`;
            if (diff < 86400) return `Hace ${Math.floor(diff / 3600)} horas`;
            if (diff < 604800) return `Hace ${Math.floor(diff / 86400)} días`;
            
            return date.toLocaleDateString('es-AR');
        }
    }
};
