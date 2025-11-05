<template>
  <div class="historial-compras">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2><i class="fas fa-history me-2 text-primary"></i>Mi Historial de Compras</h2>
      <div class="d-flex gap-2">
        <select class="form-select" v-model="filtroEstado" @change="filtrarCompras">
          <option value="">Todos los estados</option>
          <option value="pendiente">Pendientes</option>
          <option value="completado">Completados</option>
          <option value="cancelado">Cancelados</option>
        </select>
        <input type="month" class="form-control" v-model="filtroMes" @change="filtrarCompras">
      </div>
    </div>

    <div v-if="cargando" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
    </div>

    <div v-else-if="comprasFiltradas.length === 0" class="text-center py-5">
      <i class="fas fa-receipt fa-3x text-muted mb-3"></i>
      <h4 class="text-muted">No hay compras registradas</h4>
      <p class="text-muted">Realiza tu primera compra desde el catálogo</p>
      <button class="btn btn-primary mt-3" @click="irAlCatalogo">
        <i class="fas fa-store me-2"></i>Ir al Catálogo
      </button>
    </div>

    <div v-else class="row">
      <div class="col-12">
        <div class="card">
          <div class="card-header bg-light">
            <h5 class="mb-0">Compras ({{ comprasFiltradas.length }})</h5>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-hover mb-0">
                <thead class="table-light">
                  <tr>
                    <th>ID Compra</th>
                    <th>Producto</th>
                    <th>Fecha</th>
                    <th>Cantidad</th>
                    <th>Total</th>
                    <th>Estado</th>
                    <th class="text-center">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="compra in comprasFiltradas" :key="compra.id" class="align-middle">
                    <td>
                      <strong>#{{ compra.id }}</strong>
                    </td>
                    <td>
                      <div class="d-flex align-items-center">
                        <img :src="compra.imagenTorta" :alt="compra.nombreTorta" 
                             class="rounded me-3" style="width: 50px; height: 50px; object-fit: cover;">
                        <div>
                          <strong class="d-block">{{ compra.nombreTorta }}</strong>
                          <small class="text-muted">{{ compra.nombreVendedor }}</small>
                        </div>
                      </div>
                    </td>
                    <td>
                      <div>
                        <div class="fw-bold">{{ formatFecha(compra.fechaPago) }}</div>
                        <small class="text-muted">{{ formatHora(compra.fechaPago) }}</small>
                      </div>
                    </td>
                    <td>
                      <span class="badge bg-secondary fs-6">{{ compra.cantidad }}</span>
                    </td>
                    <td>
                      <strong class="text-success">${{ compra.monto }}</strong>
                    </td>
                    <td>
                      <span class="badge" :class="getBadgeClass(compra.estado)">
                        {{ getEstadoTexto(compra.estado) }}
                      </span>
                      <div v-if="compra.fechaEntrega" class="small text-muted">
                        Entrega: {{ formatFecha(compra.fechaEntrega) }}
                      </div>
                    </td>
                    <td class="text-center">
                      <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary" 
                                @click="verDetalle(compra)"
                                title="Ver detalles">
                          <i class="fas fa-eye"></i>
                        </button>
                        <button v-if="compra.estado === 'pendiente'" 
                                class="btn btn-outline-warning"
                                @click="editarCompra(compra)"
                                title="Editar compra">
                          <i class="fas fa-edit"></i>
                        </button>
                        <button v-if="compra.estado === 'pendiente'" 
                                class="btn btn-outline-danger"
                                @click="cancelarCompra(compra)"
                                title="Cancelar compra">
                          <i class="fas fa-times"></i>
                        </button>
                        <button v-if="compra.puedeCalificar && compra.estado === 'completado'" 
                                class="btn btn-outline-info"
                                @click="calificarCompra(compra)"
                                title="Calificar producto">
                          <i class="fas fa-star"></i>
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
    </div>

    <!-- Estadísticas Rápidas -->
    <div class="row mt-4">
      <div class="col-md-3">
        <div class="card bg-primary text-white">
          <div class="card-body text-center">
            <h4>{{ estadisticas.totalCompras }}</h4>
            <p class="mb-0">Total Compras</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-success text-white">
          <div class="card-body text-center">
            <h4>${{ estadisticas.totalGastado }}</h4>
            <p class="mb-0">Total Gastado</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-warning text-white">
          <div class="card-body text-center">
            <h4>{{ estadisticas.pedidosPendientes }}</h4>
            <p class="mb-0">Pendientes</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-info text-white">
          <div class="card-body text-center">
            <h4>{{ estadisticas.comprasEsteMes }}</h4>
            <p class="mb-0">Este Mes</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'HistorialCompras',
  data() {
    return {
      compras: [],
      cargando: false,
      filtroEstado: '',
      filtroMes: '',
      estadisticas: {
        totalCompras: 0,
        totalGastado: 0,
        pedidosPendientes: 0,
        comprasEsteMes: 0
      }
    }
  },
  computed: {
    comprasFiltradas() {
      let filtered = this.compras

      if (this.filtroEstado) {
        filtered = filtered.filter(compra => compra.estado === this.filtroEstado)
      }

      if (this.filtroMes) {
        const [year, month] = this.filtroMes.split('-')
        filtered = filtered.filter(compra => {
          const compraDate = new Date(compra.fechaPago)
          return compraDate.getFullYear() === parseInt(year) && 
                 compraDate.getMonth() + 1 === parseInt(month)
        })
      }

      return filtered.sort((a, b) => new Date(b.fechaPago) - new Date(a.fechaPago))
    }
  },
  async mounted() {
    await this.cargarCompras()
    this.calcularEstadisticas()
  },
  methods: {
    async cargarCompras() {
      this.cargando = true
      try {
        // Simular carga de compras desde API
        this.compras = [
          {
            id: 1001,
            nombreTorta: 'Torta de Chocolate Premium',
            nombreVendedor: 'Dulces Tentaciones',
            imagenTorta: '/images/torta-chocolate.jpg',
            fechaPago: '2024-01-15T10:30:00',
            cantidad: 1,
            monto: 45.99,
            estado: 'completado',
            fechaEntrega: '2024-01-17T14:00:00',
            puedeCalificar: true,
            metodoPago: 'transferencia',
            direccionEntrega: 'Av. Siempre Viva 123, Springfield'
          },
          {
            id: 1002,
            nombreTorta: 'Torta de Vainilla con Frutas',
            nombreVendedor: 'Postres Artesanales',
            imagenTorta: '/images/torta-vainilla.jpg',
            fechaPago: '2024-01-10T16:45:00',
            cantidad: 2,
            monto: 77.00,
            estado: 'pendiente',
            fechaEntrega: null,
            puedeCalificar: false,
            metodoPago: 'efectivo',
            direccionEntrega: 'Calle Falsa 123, Springfield'
          },
          {
            id: 1003,
            nombreTorta: 'Cheesecake de Frutos Rojos',
            nombreVendedor: 'Dulces Tentaciones',
            imagenTorta: '/images/cheesecake.jpg',
            fechaPago: '2023-12-20T09:15:00',
            cantidad: 1,
            monto: 52.50,
            estado: 'cancelado',
            fechaEntrega: null,
            puedeCalificar: false,
            metodoPago: 'tarjeta',
            direccionEntrega: 'Av. Principal 456, Springfield'
          }
        ]
      } catch (error) {
        console.error('Error cargando compras:', error)
      } finally {
        this.cargando = false
      }
    },

    calcularEstadisticas() {
      this.estadisticas.totalCompras = this.compras.length
      this.estadisticas.totalGastado = this.compras
        .filter(c => c.estado === 'completado')
        .reduce((sum, c) => sum + c.monto, 0)
        .toFixed(2)
      this.estadisticas.pedidosPendientes = this.compras.filter(c => c.estado === 'pendiente').length
      
      const mesActual = new Date().getMonth()
      const añoActual = new Date().getFullYear()
      this.estadisticas.comprasEsteMes = this.compras.filter(c => {
        const fechaCompra = new Date(c.fechaPago)
        return fechaCompra.getMonth() === mesActual && fechaCompra.getFullYear() === añoActual
      }).length
    },

    verDetalle(compra) {
      this.$emit('ver-detalle-compra', compra)
    },

    editarCompra(compra) {
      this.$emit('editar-compra', compra)
    },

    cancelarCompra(compra) {
      this.$emit('cancelar-compra', compra)
    },

    calificarCompra(compra) {
      // Implementar calificación
      console.log('Calificar compra:', compra)
    },

    filtrarCompras() {
      // El filtrado se hace automáticamente en la computed property
    },

    formatFecha(fechaString) {
      return new Date(fechaString).toLocaleDateString('es-ES')
    },

    formatHora(fechaString) {
      return new Date(fechaString).toLocaleTimeString('es-ES', { 
        hour: '2-digit', 
        minute: '2-digit' 
      })
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
    },

    irAlCatalogo() {
      this.$emit('ir-al-catalogo')
    }
  }
}
</script>

<style scoped>
.table th {
  border-top: none;
  font-weight: 600;
}

.card {
  border: none;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
</style>