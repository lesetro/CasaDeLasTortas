<template>
  <div class="modal fade" id="editarCompraModal" tabindex="-1" aria-hidden="true" ref="modal">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header bg-warning text-dark">
          <h5 class="modal-title">
            <i class="fas fa-edit me-2"></i>Editar Compra #{{ compra.id }}
          </h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <div v-if="cargando" class="text-center py-4">
            <div class="spinner-border text-warning" role="status">
              <span class="visually-hidden">Cargando...</span>
            </div>
          </div>

          <form v-else @submit.prevent="guardarCambios">
            <!-- Información del Producto -->
            <div class="card mb-4">
              <div class="card-header">
                <h6 class="mb-0">Producto: {{ compra.nombreTorta }}</h6>
              </div>
              <div class="card-body">
                <div class="row align-items-center">
                  <div class="col-md-3 text-center">
                    <img :src="compra.imagenTorta" :alt="compra.nombreTorta" 
                         class="img-fluid rounded" style="max-height: 100px;">
                  </div>
                  <div class="col-md-9">
                    <div class="row">
                      <div class="col-md-6">
                        <label class="form-label">Cantidad</label>
                        <div class="input-group">
                          <button class="btn btn-outline-secondary" 
                                  type="button"
                                  @click="form.cantidad = Math.max(1, form.cantidad - 1)">
                            <i class="fas fa-minus"></i>
                          </button>
                          <input type="number" class="form-control text-center" 
                                 v-model.number="form.cantidad" min="1" max="10">
                          <button class="btn btn-outline-secondary" 
                                  type="button"
                                  @click="form.cantidad = Math.min(10, form.cantidad + 1)">
                            <i class="fas fa-plus"></i>
                          </button>
                        </div>
                        <small class="text-muted">Máximo 10 unidades por pedido</small>
                      </div>
                      <div class="col-md-6">
                        <label class="form-label">Precio Unitario</label>
                        <input type="text" class="form-control" :value="`$${compra.precioUnitario}`" disabled>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Información de Entrega -->
            <div class="card mb-4">
              <div class="card-header">
                <h6 class="mb-0">Información de Entrega</h6>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <label class="form-label">Dirección de Entrega *</label>
                  <textarea class="form-control" v-model="form.direccionEntrega" 
                            rows="3" placeholder="Ingresa tu dirección completa..." required></textarea>
                </div>
                
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label">Ciudad *</label>
                    <input type="text" class="form-control" v-model="form.ciudad" required>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label">Provincia *</label>
                    <input type="text" class="form-control" v-model="form.provincia" required>
                  </div>
                </div>
              </div>
            </div>

            <!-- Método de Pago -->
            <div class="card mb-4">
              <div class="card-header">
                <h6 class="mb-0">Método de Pago</h6>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label">Método de Pago *</label>
                    <select class="form-select" v-model="form.metodoPago" required>
                      <option value="transferencia">Transferencia Bancaria</option>
                      <option value="efectivo">Efectivo</option>
                      <option value="tarjeta">Tarjeta de Crédito/Débito</option>
                      <option value="mercadopago">MercadoPago</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label">Fecha Deseada de Entrega</label>
                    <input type="date" class="form-control" v-model="form.fechaEntregaDeseada"
                           :min="fechaMinima" :max="fechaMaxima">
                    <small class="text-muted">Selecciona cuándo deseas recibir tu pedido</small>
                  </div>
                </div>
              </div>
            </div>

            <!-- Observaciones -->
            <div class="card">
              <div class="card-header">
                <h6 class="mb-0">Observaciones Adicionales</h6>
              </div>
              <div class="card-body">
                <textarea class="form-control" v-model="form.observaciones" 
                          rows="3" placeholder="Alguna instrucción especial, alergias, preferencias..."></textarea>
                <small class="text-muted">Estas observaciones serán compartidas con el vendedor</small>
              </div>
            </div>

            <!-- Resumen de Cambios -->
            <div class="card mt-4 bg-light">
              <div class="card-body">
                <h6 class="card-title">Resumen de Cambios</h6>
                <div class="row">
                  <div class="col-md-6">
                    <strong>Subtotal anterior:</strong> ${{ (compra.precioUnitario * compra.cantidad).toFixed(2) }}
                  </div>
                  <div class="col-md-6">
                    <strong>Nuevo subtotal:</strong> ${{ (compra.precioUnitario * form.cantidad).toFixed(2) }}
                  </div>
                </div>
                <div class="mt-2" v-if="form.cantidad !== compra.cantidad">
                  <span class="badge" :class="form.cantidad > compra.cantidad ? 'bg-success' : 'bg-warning'">
                    {{ form.cantidad > compra.cantidad ? 'Aumentó' : 'Disminuyó' }} la cantidad
                  </span>
                </div>
              </div>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" :disabled="guardando">
            <i class="fas fa-times me-1"></i>Cancelar
          </button>
          <button type="button" class="btn btn-warning" @click="guardarCambios" :disabled="guardando">
            <span v-if="guardando" class="spinner-border spinner-border-sm me-1"></span>
            <i class="fas fa-save me-1"></i>Guardar Cambios
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Modal } from 'bootstrap'

export default {
  name: 'EditarCompra',
  props: {
    compra: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      modal: null,
      cargando: false,
      guardando: false,
      form: {
        cantidad: 1,
        direccionEntrega: '',
        ciudad: '',
        provincia: '',
        metodoPago: '',
        fechaEntregaDeseada: '',
        observaciones: ''
      }
    }
  },
  computed: {
    fechaMinima() {
      return new Date().toISOString().split('T')[0]
    },
    fechaMaxima() {
      const fecha = new Date()
      fecha.setDate(fecha.getDate() + 30) // Máximo 30 días
      return fecha.toISOString().split('T')[0]
    }
  },
  mounted() {
    this.inicializarForm()
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
    inicializarForm() {
      this.form = {
        cantidad: this.compra.cantidad,
        direccionEntrega: this.compra.direccionEntrega,
        ciudad: this.compra.ciudad || '',
        provincia: this.compra.provincia || '',
        metodoPago: this.compra.metodoPago,
        fechaEntregaDeseada: this.compra.fechaEntregaDeseada || '',
        observaciones: this.compra.observaciones || ''
      }
    },

    async guardarCambios() {
      this.guardando = true
      try {
        // Validaciones
        if (!this.form.direccionEntrega.trim()) {
          alert('Por favor ingresa una dirección de entrega')
          return
        }

        if (this.form.cantidad < 1 || this.form.cantidad > 10) {
          alert('La cantidad debe estar entre 1 y 10 unidades')
          return
        }

        // Simular guardado en API
        await new Promise(resolve => setTimeout(resolve, 1500))

        const compraEditada = {
          ...this.compra,
          ...this.form,
          monto: (this.compra.precioUnitario * this.form.cantidad).toFixed(2),
          fechaActualizacion: new Date().toISOString()
        }

        this.$emit('guardar', compraEditada)
        this.modal.hide()
        
      } catch (error) {
        console.error('Error guardando cambios:', error)
        alert('Error al guardar los cambios')
      } finally {
        this.guardando = false
      }
    },

    cerrarModal() {
      this.$emit('cerrar')
    }
  }
}
</script>

<style scoped>
.card {
  border: none;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.card-header {
  background-color: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
}
</style>