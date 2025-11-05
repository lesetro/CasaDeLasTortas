<template>
  <div class="modal fade" id="detalleCompraModal" tabindex="-1" aria-hidden="true" ref="modal">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title">
            <i class="fas fa-receipt me-2"></i>Detalle de Compra #{{ compra.id }}
          </h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <div v-if="cargando" class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Cargando...</span>
            </div>
          </div>

          <div v-else class="row">
            <!-- Información del Producto -->
            <div class="col-md-6">
              <div class="card">
                <div class="card-header">
                  <h6 class="mb-0">Información del Producto</h6>
                </div>
                <div class="card-body">
                  <div class="text-center mb-3">
                    <img :src="compra.imagenTorta" :alt="compra.nombreTorta" 
                         class="img-fluid rounded" style="max-height: 200px;">
                  </div>
                  <h5 class="text-center">{{ compra.nombreTorta }}</h5>
                  <div class="row text-center mt-3">
                    <div class="col-6">
                      <strong class="d-block text-success h5">${{ compra.precioUnitario }}</strong>
                      <small class="text-muted">Precio Unitario</small>
                    </div>
                    <div class="col-6">
                      <strong class="d-block h5">{{ compra.cantidad }}</strong>
                      <small class="text-muted">Cantidad</small>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Información de la Compra -->
            <div class="col-md-6">
              <div class="card">
                <div class="card-header">
                  <h6 class="mb-0">Detalles de la Compra</h6>
                </div>
                <div class="card-body">
                  <div class="mb-3">
                    <strong>Estado:</strong>
                    <span class="badge float-end" :class="getBadgeClass(compra.estado)">
                      {{ getEstadoTexto(compra.estado) }}
                    </span>
                  </div>
                  
                  <div class="mb-3">
                    <strong>Fecha de Compra:</strong>
                    <span class="float-end">{{ formatFecha(compra.fechaPago) }}</span>
                  </div>

                  <div v-if="compra.fechaEntrega" class="mb-3">
                    <strong>Fecha de Entrega:</strong>
                    <span class="float-end">{{ formatFecha(compra.fechaEntrega) }}</span>
                  </div>

                  <div class="mb-3">
                    <strong>Vendedor:</strong>
                    <span class="float-end">{{ compra.nombreVendedor }}</span>
                  </div>

                  <div class="mb-3">
                    <strong>Método de Pago:</strong>
                    <span class="float-end text-capitalize">{{ compra.metodoPago }}</span>
                  </div>

                  <hr>

                  <!-- Resumen de Pago -->
                  <div class="mb-2">
                    <strong>Subtotal:</strong>
                    <span class="float-end">${{ (compra.precioUnitario * compra.cantidad).toFixed(2) }}</span>
                  </div>
                  
                  <div class="mb-2">
                    <strong>Envío:</strong>
                    <span class="float-end">${{ compra.envio || '0.00' }}</span>
                  </div>
                  
                  <div class="mb-2">
                    <strong>Descuento:</strong>
                    <span class="float-end text-danger">-${{ compra.descuento || '0.00' }}</span>
                  </div>
                  
                  <div class="border-top pt-2">
                    <strong class="h5">Total:</strong>
                    <strong class="h5 float-end text-success">${{ compra.monto }}</strong>
                  </div>
                </div>
              </div>
            </div>

            <!-- Información de Entrega -->
            <div class="col-12 mt-3">
              <div class="card">
                <div class="card-header">
                  <h6 class="mb-0">Información de Entrega</h6>
                </div>
                <div class="card-body">
                  <div class="row">
                    <div class="col-md-8">
                      <strong>Dirección:</strong>
                      <p class="mb-2">{{ compra.direccionEntrega }}</p>
                      
                      <strong>Observaciones:</strong>
                      <p class="mb-0">{{ compra.observaciones || 'Sin observaciones' }}</p>
                    </div>
                    <div class="col-md-4">
                      <div v-if="compra.estado === 'pendiente'" class="alert alert-warning">
                        <small>
                          <i class="fas fa-clock me-1"></i>
                          Tu pedido está siendo procesado
                        </small>
                      </div>
                      <div v-else-if="compra.estado === 'completado'" class="alert alert-success">
                        <small>
                          <i class="fas fa-check me-1"></i>
                          Pedido entregado exitosamente
                        </small>
                      </div>
                      <div v-else-if="compra.estado === 'cancelado'" class="alert alert-danger">
                        <small>
                          <i class="fas fa-times me-1"></i>
                          Pedido cancelado
                        </small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Timeline del Pedido -->
            <div class="col-12 mt-3">
              <div class="card">
                <div class="card-header">
                  <h6 class="mb-0">Seguimiento del Pedido</h6>
                </div>
                <div class="card-body">
                  <div class="timeline">
                    <div class="timeline-item" :class="{ 'active': true }">
                      <div class="timeline-marker bg-success"></div>
                      <div class="timeline-content">
                        <strong>Compra Realizada</strong>
                        <small class="text-muted d-block">{{ formatFechaHora(compra.fechaPago) }}</small>
                      </div>
                    </div>
                    
                    <div v-if="compra.estado === 'completado'" class="timeline-item active">
                      <div class="timeline-marker bg-success"></div>
                      <div class="timeline-content">
                        <strong>Entregado</strong>
                        <small class="text-muted d-block">{{ formatFechaHora(compra.fechaEntrega) }}</small>
                      </div>
                    </div>
                    
                    <div v-else-if="compra.estado === 'cancelado'" class="timeline-item">
                      <div class="timeline-marker bg-danger"></div>
                      <div class="timeline-content">
                        <strong>Cancelado</strong>
                        <small class="text-muted d-block">{{ formatFechaHora(compra.fechaCancelacion) }}</small>
                      </div>
                    </div>
                    
                    <div v-else class="timeline-item">
                      <div class="timeline-marker"></div>
                      <div class="timeline-content">
                        <strong class="text-muted">En preparación</strong>
                        <small class="text-muted d-block">El vendedor está preparando tu pedido</small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
            <i class="fas fa-times me-1"></i>Cerrar
          </button>
          <button v-if="compra.estado === 'pendiente'" 
                  type="button" 
                  class="btn btn-warning"
                  @click="editarCompra">
            <i class="fas fa-edit me-1"></i>Editar Pedido
          </button>
          <button v-if="compra.puedeCalificar && compra.estado === 'completado'" 
                  type="button" 
                  class="btn btn-info"
                  @click="calificarCompra">
            <i class="fas fa-star me-1"></i>Calificar Producto
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Modal } from 'bootstrap'

export default {
  name: 'DetalleCompra',
  props: {
    compra: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      modal: null,
      cargando: false
    }
  },
  mounted() {
    this.modal = new Modal(this.$refs.modal)
    this.modal.show()
    
    this.$refs.modal.addEventListener('hidden.bs.modal', this.cerrarModal)
  },
  beforeUnmount() {
    if (this.modal) {
      this.modal.hide()
    }
  },
  methods: {
    cerrarModal() {
      this.$emit('cerrar')
    },

    editarCompra() {
      this.modal.hide()
      this.$emit('editar-compra', this.compra)
    },

    calificarCompra() {
      // Implementar calificación
      console.log('Calificar compra:', this.compra)
      alert('Funcionalidad de calificación en desarrollo')
    },

    formatFecha(fechaString) {
      return new Date(fechaString).toLocaleDateString('es-ES')
    },

    formatFechaHora(fechaString) {
      if (!fechaString) return ''
      return new Date(fechaString).toLocaleString('es-ES')
    },

    getBadgeClass(estado) {
      const classes = {
        'pendiente': 'bg-warning',
        'completado': 'bg-success',
        'cancelado': 'bg-danger'
      }
      return classes[estado] || 'bg-secondary'
    },

    getEstadoTexto(estado) {
      const textos = {
        'pendiente': 'Pendiente',
        'completado': 'Completado',
        'cancelado': 'Cancelado'
      }
      return textos[estado] || estado
    }
  }
}
</script>

<style scoped>
.timeline {
  position: relative;
  padding-left: 30px;
}

.timeline-item {
  position: relative;
  margin-bottom: 20px;
}

.timeline-marker {
  position: absolute;
  left: -30px;
  top: 0;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background-color: #dee2e6;
}

.timeline-item.active .timeline-marker {
  background-color: #28a745;
}

.timeline-content {
  padding-left: 10px;
}

.timeline-item:not(:last-child)::after {
  content: '';
  position: absolute;
  left: -21px;
  top: 20px;
  bottom: -20px;
  width: 2px;
  background-color: #dee2e6;
}

.timeline-item.active:not(:last-child)::after {
  background-color: #28a745;
}
</style>