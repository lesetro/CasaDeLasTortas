<template>
  <div class="carrito-compra">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2><i class="fas fa-shopping-cart me-2 text-primary"></i>Mi Carrito</h2>
      <div v-if="carrito.length > 0" class="d-flex align-items-center gap-3">
        <strong class="h5 mb-0 text-success">Total: ${{ totalCarrito }}</strong>
        <button class="btn btn-success btn-lg" @click="procederPago">
          <i class="fas fa-credit-card me-2"></i>Proceder al Pago
        </button>
      </div>
    </div>

    <div v-if="carrito.length === 0" class="text-center py-5">
      <i class="fas fa-shopping-cart fa-4x text-muted mb-3"></i>
      <h4 class="text-muted">Tu carrito está vacío</h4>
      <p class="text-muted">Agrega algunas tortas deliciosas desde el catálogo</p>
      <button class="btn btn-primary mt-3" @click="irAlCatalogo">
        <i class="fas fa-store me-2"></i>Ir al Catálogo
      </button>
    </div>

    <div v-else class="row">
      <!-- Lista de Productos -->
      <div class="col-lg-8">
        <div class="card">
          <div class="card-header bg-light">
            <h5 class="mb-0">Productos en el Carrito ({{ carrito.length }})</h5>
          </div>
          <div class="card-body p-0">
            <div v-for="item in carrito" :key="item.tortaId" class="carrito-item p-3 border-bottom">
              <div class="row align-items-center">
                <div class="col-md-2">
                  <img :src="item.imagen" :alt="item.nombre" 
                       class="img-fluid rounded" style="height: 80px; object-fit: cover;">
                </div>
                <div class="col-md-4">
                  <h6 class="mb-1">{{ item.nombre }}</h6>
                  <small class="text-muted">Vendedor: {{ item.vendedor.nombreComercial }}</small>
                  <div class="mt-1">
                    <strong class="text-success">${{ item.precio }} c/u</strong>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="d-flex align-items-center">
                    <button class="btn btn-outline-secondary btn-sm" 
                            @click="actualizarCantidad(item.tortaId, item.cantidad - 1)"
                            :disabled="item.cantidad <= 1">
                      <i class="fas fa-minus"></i>
                    </button>
                    <input type="number" class="form-control form-control-sm mx-2 text-center" 
                           v-model.number="item.cantidad" min="1" 
                           @change="actualizarCantidad(item.tortaId, item.cantidad)"
                           style="width: 70px;">
                    <button class="btn btn-outline-secondary btn-sm" 
                            @click="actualizarCantidad(item.tortaId, item.cantidad + 1)">
                      <i class="fas fa-plus"></i>
                    </button>
                  </div>
                </div>
                <div class="col-md-2 text-center">
                  <strong>${{ (item.precio * item.cantidad).toFixed(2) }}</strong>
                </div>
                <div class="col-md-1 text-center">
                  <button class="btn btn-outline-danger btn-sm" 
                          @click="eliminarItem(item.tortaId)"
                          title="Eliminar del carrito">
                    <i class="fas fa-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Resumen del Pedido -->
      <div class="col-lg-4">
        <div class="card sticky-top" style="top: 100px;">
          <div class="card-header bg-primary text-white">
            <h5 class="mb-0">Resumen del Pedido</h5>
          </div>
          <div class="card-body">
            <div class="d-flex justify-content-between mb-2">
              <span>Subtotal:</span>
              <strong>${{ subtotal }}</strong>
            </div>
            <div class="d-flex justify-content-between mb-2">
              <span>Envío:</span>
              <strong>${{ envio }}</strong>
            </div>
            <div class="d-flex justify-content-between mb-3 border-top pt-2">
              <span class="h5">Total:</span>
              <strong class="h5 text-success">${{ totalCarrito }}</strong>
            </div>

            <!-- Método de Pago -->
            <div class="mb-3">
              <label class="form-label">Método de Pago</label>
              <select class="form-select" v-model="metodoPago">
                <option value="transferencia">Transferencia Bancaria</option>
                <option value="efectivo">Efectivo</option>
                <option value="tarjeta">Tarjeta de Crédito/Débito</option>
                <option value="mercadopago">MercadoPago</option>
              </select>
            </div>

            <!-- Dirección de Entrega -->
            <div class="mb-3">
              <label class="form-label">Dirección de Entrega</label>
              <textarea class="form-control" v-model="direccionEntrega" 
                        rows="3" placeholder="Ingresa tu dirección completa..."></textarea>
            </div>

            <!-- Observaciones -->
            <div class="mb-3">
              <label class="form-label">Observaciones (opcional)</label>
              <textarea class="form-control" v-model="observaciones" 
                        rows="2" placeholder="Alguna instrucción especial..."></textarea>
            </div>

            <button class="btn btn-success w-100" @click="procederPago" :disabled="!direccionEntrega">
              <i class="fas fa-credit-card me-2"></i>
              Confirmar Compra - ${{ totalCarrito }}
            </button>

            <div class="mt-3 text-center">
              <small class="text-muted">
                <i class="fas fa-lock me-1"></i>
                Tu compra está protegida
              </small>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CarritoCompra',
  props: {
    carrito: {
      type: Array,
      required: true
    }
  },
  data() {
    return {
      metodoPago: 'transferencia',
      direccionEntrega: '',
      observaciones: ''
    }
  },
  computed: {
    subtotal() {
      return this.carrito.reduce((total, item) => total + (item.precio * item.cantidad), 0)
    },
    envio() {
      return this.subtotal > 100 ? 0 : 15.00
    },
    totalCarrito() {
      return (this.subtotal + this.envio).toFixed(2)
    }
  },
  methods: {
    actualizarCantidad(tortaId, nuevaCantidad) {
      if (nuevaCantidad < 1) return
      this.$emit('actualizar-cantidad', { tortaId, cantidad: nuevaCantidad })
    },

    eliminarItem(tortaId) {
      this.$emit('eliminar-del-carrito', tortaId)
    },

    procederPago() {
      if (!this.direccionEntrega.trim()) {
        alert('Por favor ingresa una dirección de entrega')
        return
      }

      const datosCompra = {
        items: this.carrito,
        metodoPago: this.metodoPago,
        direccionEntrega: this.direccionEntrega,
        observaciones: this.observaciones,
        subtotal: this.subtotal,
        envio: this.envio,
        total: this.totalCarrito,
        fecha: new Date().toISOString()
      }

      this.$emit('confirmar-compra', datosCompra)
    },

    irAlCatalogo() {
      this.$emit('ir-al-catalogo')
    }
  }
}
</script>

<style scoped>
.carrito-item {
  transition: background-color 0.2s;
}

.carrito-item:hover {
  background-color: #f8f9fa;
}

.sticky-top {
  z-index: 1020;
}

input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

input[type="number"] {
  -moz-appearance: textfield;
}
</style>