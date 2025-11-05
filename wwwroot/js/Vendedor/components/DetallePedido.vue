// DetallePedido.vue - Modal con detalles completos del pedido
const DetallePedido = {
    template: `
    <div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-xl">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">
                        <i class="fas fa-receipt me-2"></i>
                        Detalle del Pedido #{{ pedido.id }}
                    </h5>
                    <button type="button" class="btn-close btn-close-white" @click="$emit('cerrar')"></button>
                </div>
                
                <div class="modal-body">
                    <div class="row">
                        <!-- Información del Pedido -->
                        <div class="col-md-8">
                            <div class="card mb-3">
                                <div class="card-header">
                                    <h6 class="mb-0"><i class="fas fa-shopping-bag me-2"></i>Información del Pedido</h6>
                                </div>
                                <div class="card-body">
                                    <div class="row">
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Torta</small>
                                            <p class="fw-bold mb-0">{{ pedido.tortaNombre }}</p>
                                        </div>
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Precio Unitario</small>
                                            <p class="fw-bold mb-0 text-success">\${{ pedido.precioUnitario?.toFixed(2) }}</p>
                                        </div>
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Cantidad</small>
                                            <p class="fw-bold mb-0">{{ pedido.cantidad }}</p>
                                        </div>
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Monto Total</small>
                                            <p class="fw-bold mb-0 text-success fs-4">\${{ pedido.monto.toFixed(2) }}</p>
                                        </div>
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Fecha del Pedido</small>
                                            <p class="fw-bold mb-0">{{ formatearFecha(pedido.fechaPago) }}</p>
                                        </div>
                                        <div class="col-6 mb-3">
                                            <small class="text-muted">Fecha de Entrega</small>
                                            <p class="fw-bold mb-0">{{ pedido.fechaEntrega ? formatearFecha(pedido.fechaEntrega) : 'No especificada' }}</p>
                                        </div>
                                        <div class="col-12 mb-3">
                                            <small class="text-muted">Método de Pago</small>
                                            <p class="fw-bold mb-0">{{ pedido.metodoPago || 'No especificado' }}</p>
                                        </div>
                                        <div class="col-12">
                                            <small class="text-muted">Estado</small>
                                            <p>
                                                <span class="badge fs-6" :class="getEstadoClass(pedido.estado)">
                                                    {{ pedido.estado }}
                                                </span>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Comprobante si existe -->
                            <div v-if="pedido.archivoComprobante" class="card">
                                <div class="card-header">
                                    <h6 class="mb-0"><i class="fas fa-file-invoice me-2"></i>Comprobante de Pago</h6>
                                </div>
                                <div class="card-body">
                                    <a :href="pedido.archivoComprobante" target="_blank" class="btn btn-outline-primary">
                                        <i class="fas fa-download me-2"></i>
                                        Descargar Comprobante
                                    </a>
                                </div>
                            </div>
                        </div>

                        <!-- Información del Cliente -->
                        <div class="col-md-4">
                            <div class="card mb-3">
                                <div class="card-header">
                                    <h6 class="mb-0"><i class="fas fa-user me-2"></i>Datos del Cliente</h6>
                                </div>
                                <div class="card-body">
                                    <div class="mb-3">
                                        <small class="text-muted">Nombre</small>
                                        <p class="fw-bold mb-0">{{ pedido.compradorNombre }}</p>
                                    </div>
                                    <div class="mb-3">
                                        <small class="text-muted">Email</small>
                                        <p class="mb-0">{{ pedido.compradorEmail }}</p>
                                    </div>
                                    <div class="mb-3">
                                        <small class="text-muted">Teléfono</small>
                                        <p class="mb-0">{{ pedido.compradorTelefono || 'No disponible' }}</p>
                                    </div>
                                    <div class="mb-3">
                                        <small class="text-muted">Dirección de Entrega</small>
                                        <p class="mb-0">{{ pedido.compradorDireccion || 'No especificada' }}</p>
                                    </div>
                                    <button class="btn btn-sm btn-outline-info w-100" @click="verHistorialCliente">
                                        <i class="fas fa-history me-2"></i>
                                        Ver Historial Completo
                                    </button>
                                </div>
                            </div>

                            <!-- Acciones Rápidas -->
                            <div class="card" v-if="pedido.estado === 'Pendiente'">
                                <div class="card-header">
                                    <h6 class="mb-0"><i class="fas fa-cogs me-2"></i>Acciones</h6>
                                </div>
                                <div class="card-body">
                                    <button class="btn btn-success w-100 mb-2" @click="cambiarEstado('Completado')">
                                        <i class="fas fa-check-circle me-2"></i>
                                        Marcar como Completado
                                    </button>
                                    <button class="btn btn-danger w-100" @click="cambiarEstado('Cancelado')">
                                        <i class="fas fa-times-circle me-2"></i>
                                        Cancelar Pedido
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @click="$emit('cerrar')">
                        <i class="fas fa-times me-2"></i>Cerrar
                    </button>
                </div>
            </div>
        </div>
    </div>
    `,
    
    props: {
        pedido: {
            type: Object,
            required: true
        }
    },
    
    methods: {
        async cambiarEstado(nuevoEstado) {
            try {
                const response = await fetch(`/api/pago/${this.pedido.id}/estado`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${window.authToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ estado: nuevoEstado })
                });
                
                if (!response.ok) throw new Error('Error al cambiar estado');
                
                toastr.success(`Estado cambiado a ${nuevoEstado}`);
                this.$emit('estado-cambiado');
            } catch (error) {
                console.error('Error:', error);
                toastr.error('Error al cambiar el estado');
            }
        },
        
        verHistorialCliente() {
            // Emitir evento para abrir modal de historial
            toastr.info('Abriendo historial del cliente...');
        },
        
        getEstadoClass(estado) {
            const clases = {
                'Pendiente': 'bg-warning text-dark',
                'Completado': 'bg-success',
                'Cancelado': 'bg-danger'
            };
            return clases[estado] || 'bg-secondary';
        },
        
        formatearFecha(fecha) {
            return new Date(fecha).toLocaleDateString('es-AR', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    }
};
