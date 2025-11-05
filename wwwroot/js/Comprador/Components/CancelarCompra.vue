<template>
  <div class="modal fade" id="cancelarCompraModal" tabindex="-1" aria-hidden="true" ref="modal">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header bg-danger text-white">
          <h5 class="modal-title">
            <i class="fas fa-exclamation-triangle me-2"></i>Cancelar Compra
          </h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <div class="text-center mb-4">
            <i class="fas fa-times-circle fa-3x text-danger mb-3"></i>
            <h4 class="text-danger">¿Estás seguro de cancelar esta compra?</h4>
          </div>

          <!-- Información de la Compra -->
          <div class="card mb-4">
            <div class="card-body">
              <div class="d-flex align-items-center mb-3">
                <img :src="compra.imagenTorta" :alt="compra.nombreTorta" 
                     class="rounded me-3" style="width: 60px; height: 60px; object-fit: cover;">
                <div>
                  <strong class="d-block">{{ compra.nombreTorta }}</strong>
                  <small class="text-muted">Vendedor: {{ compra.nombreVendedor }}</small>
                </div>
              </div>
              
              <div class="row text-center">
                <div class="col-4">
                  <strong class="d-block text-success">${{ compra.monto }}</strong>
                  <small class="text-muted">Total</small>
                </div>
                <div class="col-4">
                  <strong class="d-block">{{ compra.cantidad }}</strong>
                  <small class="text-muted">Cantidad</small>
                </div>
                <div class="col-4">
                  <strong class="d-block">{{ formatFecha(compra.fechaPago) }}</strong>
                  <small class="text-muted">Fecha</small>
                </div>
              </div>
            </div>
          </div>

          <!-- Razón de Cancelación -->
          <div class="mb-4">
            <label class="form-label">
              <strong>Motivo de cancelación *</strong>
            </label>
            <select class="form-select" v-model="razonCancelacion" required>
              <option value="">Selecciona un motivo...</option>
              <option value="cambio_decision">Cambié de decisión</option>
              <option value="encontrado_mejor_precio">Encontré un mejor precio</option>
              <option value="problema_producto">Problema con el producto</option>
              <option value="demora_entrega">Demora en la entrega</option>
              <option value="cambio_direccion">Cambio de dirección</option>
              <option value="otro">Otro motivo</option>
            </select>
          </div>

          <!-- Comentarios Adicionales -->
          <div class="mb-3" v-if="razonCancelacion">
            <label class="form-label">Comentarios adicionales (opcional)</label>
            <textarea class="form-control" v-model="comentariosAdicionales" 
                      rows="3" placeholder="Proporciona más detalles sobre tu cancelación..."></textarea>
          </div>

          <!-- Advertencias -->
          <div class="alert alert-warning">
            <h6 class="alert-heading">
              <i class="fas fa-info-circle me-1"></i>Importante
            </h6>
            <ul class="mb-0 small">
              <li>Esta acción no se puede deshacer</li>
              <li>El vendedor será notificado de la cancelación</li>
              <li>El reembolso puede tardar hasta 5 días hábiles</li>
              <li v-if="compra.metodoPago === 'efectivo'">Para pagos en efectivo, contacta al vendedor directamente</li>
            </ul>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="cancelando">
            <i class="fas fa-arrow-left me-1"></i>Volver
          </button>
          <button type="button" class="btn btn-danger" @click="confirmarCancelacion" :disabled="!razonCancelacion || cancelando">
            <span v-if="cancelando" class="spinner-border spinner-border-sm me-1"></span>
            <i class="fas fa-times me-1"></i>Confirmar Cancelación
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Modal } from 'bootstrap'

export default {
  name: 'CancelarCompra',
  props: {
    compra: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      modal: null,
      cancelando: false,
      razonCancelacion: '',
      comentariosAdicionales: ''
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
    async confirmarCancelacion() {
      this.cancelando = true
      try {
        // Simular cancelación en API
        await new Promise(resolve => setTimeout(resolve, 2000))

        const datosCancelacion = {
          compraId: this.compra.id,
          razon: this.razonCancelacion,
          comentarios: this.comentariosAdicionales,
          fechaCancelacion: new Date().toISOString()
        }

        this.$emit('confirmar', datosCancelacion)
        this.modal.hide()
        
      } catch (error) {
        console.error('Error cancelando compra:', error)
        alert('Error al cancelar la compra')
      } finally {
        this.cancelando = false
      }
    },

    cerrarModal() {
      this.$emit('cerrar')
    },

    formatFecha(fechaString) {
      return new Date(fechaString).toLocaleDateString('es-ES')
    }
  }
}
</script>

<style scoped>
.alert ul {
  padding-left: 1rem;
}

.alert li {
  margin-bottom: 0.25rem;
}

.modal-content {
  border: none;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
</style>