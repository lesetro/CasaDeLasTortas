<template>
  <div class="comprador-dashboard">
    <!-- Navbar -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary sticky-top">
      <div class="container">
        <a class="navbar-brand" href="#">
          <i class="fas fa-birthday-cake me-2"></i>
          Mi Dashboard - Casa de las Tortas
        </a>
        <div class="navbar-nav ms-auto">
          <div class="nav-item dropdown">
            <a class="nav-link dropdown-toggle text-white" href="#" role="button" data-bs-toggle="dropdown">
              <i class="fas fa-user me-1"></i>
              {{ usuario.nombre || 'Comprador' }}
            </a>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" href="#" @click="logout"><i class="fas fa-sign-out-alt me-2"></i>Cerrar Sesión</a></li>
            </ul>
          </div>
        </div>
      </div>
    </nav>

    <!-- Main Content -->
    <div class="container-fluid mt-4">
      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3 col-lg-2">
          <div class="card">
            <div class="card-body">
              <nav class="nav flex-column">
                <button class="btn btn-outline-primary mb-2 text-start" 
                        :class="{ 'active': vistaActiva === 'catalogo' }"
                        @click="cambiarVista('catalogo')">
                  <i class="fas fa-store me-2"></i>Catálogo
                </button>
                <button class="btn btn-outline-primary mb-2 text-start"
                        :class="{ 'active': vistaActiva === 'carrito' }"
                        @click="cambiarVista('carrito')">
                  <i class="fas fa-shopping-cart me-2"></i>Mi Carrito
                  <span v-if="carrito.length > 0" class="badge bg-danger ms-2">{{ carrito.length }}</span>
                </button>
                <button class="btn btn-outline-primary mb-2 text-start"
                        :class="{ 'active': vistaActiva === 'historial' }"
                        @click="cambiarVista('historial')">
                  <i class="fas fa-history me-2"></i>Historial
                </button>
                <button class="btn btn-outline-primary mb-2 text-start"
                        :class="{ 'active': vistaActiva === 'perfil' }"
                        @click="cambiarVista('perfil')">
                  <i class="fas fa-user me-2"></i>Mi Perfil
                </button>
              </nav>
            </div>
          </div>

          <!-- Stats Resumen -->
          <div class="card mt-3">
            <div class="card-body">
              <h6 class="card-title">Mi Resumen</h6>
              <div class="d-flex justify-content-between mb-2">
                <span>Compras totales:</span>
                <strong>{{ estadisticas.totalCompras }}</strong>
              </div>
              <div class="d-flex justify-content-between mb-2">
                <span>Total gastado:</span>
                <strong>${{ estadisticas.totalGastado }}</strong>
              </div>
              <div class="d-flex justify-content-between">
                <span>Pedidos activos:</span>
                <strong class="text-warning">{{ estadisticas.pedidosActivos }}</strong>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Area -->
        <div class="col-md-9 col-lg-10">
          <!-- Vista Dinámica -->
          <div v-if="vistaActiva === 'catalogo'">
            <CatalogoInterno 
              @agregar-al-carrito="agregarAlCarrito"
              @ver-detalle-torta="verDetalleTorta" />
          </div>

          <div v-else-if="vistaActiva === 'carrito'">
            <CarritoCompra 
              :carrito="carrito"
              @actualizar-cantidad="actualizarCantidadCarrito"
              @eliminar-del-carrito="eliminarDelCarrito"
              @confirmar-compra="confirmarCompra" />
          </div>

          <div v-else-if="vistaActiva === 'historial'">
            <HistorialCompras 
              @ver-detalle-compra="verDetalleCompra"
              @editar-compra="editarCompra"
              @cancelar-compra="cancelarCompra" />
          </div>

          <div v-else-if="vistaActiva === 'perfil'">
            <PerfilComprador />
          </div>

          <!-- Modales -->
          <DetalleCompra 
            v-if="compraSeleccionada"
            :compra="compraSeleccionada"
            @cerrar="compraSeleccionada = null" />

          <EditarCompra 
            v-if="compraAEditar"
            :compra="compraAEditar"
            @guardar="guardarEdicionCompra"
            @cerrar="compraAEditar = null" />

          <CancelarCompra 
            v-if="compraACancelar"
            :compra="compraACancelar"
            @confirmar="confirmarCancelacion"
            @cerrar="compraACancelar = null" />
        </div>
      </div>
    </div>

    <!-- Toast Notifications -->
    <div class="toast-container position-fixed top-0 end-0 p-3">
      <div v-for="(toast, index) in toasts" :key="index" 
           class="toast align-items-center border-0" :class="`bg-${toast.type}`" 
           role="alert">
        <div class="d-flex">
          <div class="toast-body text-white">
            <i class="fas me-2" :class="toast.icon"></i>
            {{ toast.message }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                  @click="removerToast(index)"></button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import CatalogoInterno from '@/components/comprador/CatalogoInterno.vue'
import CarritoCompra from '@/components/comprador/CarritoCompra.vue'
import HistorialCompras from '@/components/comprador/HistorialCompras.vue'
import PerfilComprador from '@/components/comprador/PerfilComprador.vue'
import DetalleCompra from '@/components/comprador/DetalleCompra.vue'
import EditarCompra from '@/components/comprador/EditarCompra.vue'
import CancelarCompra from '@/components/comprador/CancelarCompra.vue'

export default {
  name: 'CompradorDashboard',
  components: {
    CatalogoInterno,
    CarritoCompra,
    HistorialCompras,
    PerfilComprador,
    DetalleCompra,
    EditarCompra,
    CancelarCompra
  },
  data() {
    return {
      vistaActiva: 'catalogo',
      usuario: {},
      carrito: [],
      compraSeleccionada: null,
      compraAEditar: null,
      compraACancelar: null,
      estadisticas: {
        totalCompras: 0,
        totalGastado: 0,
        pedidosActivos: 0
      },
      toasts: []
    }
  },
  async mounted() {
    await this.inicializarDashboard()
  },
  methods: {
    async inicializarDashboard() {
      // Cargar datos del usuario
      await this.cargarUsuario()
      // Cargar carrito desde localStorage
      this.cargarCarrito()
      // Cargar estadísticas
      await this.cargarEstadisticas()
    },

    async cargarUsuario() {
      try {
        const token = localStorage.getItem('jwt_token')
        // Simular carga de usuario desde API
        this.usuario = {
          id: 1,
          nombre: 'Juan Comprador',
          email: 'juan@email.com'
        }
      } catch (error) {
        this.mostrarToast('Error al cargar usuario', 'danger', 'fa-exclamation-circle')
      }
    },

    cargarCarrito() {
      const carritoGuardado = localStorage.getItem('carrito_comprador')
      if (carritoGuardado) {
        this.carrito = JSON.parse(carritoGuardado)
      }
    },

    guardarCarrito() {
      localStorage.setItem('carrito_comprador', JSON.stringify(this.carrito))
    },

    async cargarEstadisticas() {
      try {
        // Simular carga de estadísticas desde API
        this.estadisticas = {
          totalCompras: 15,
          totalGastado: 1250.75,
          pedidosActivos: 2
        }
      } catch (error) {
        console.error('Error cargando estadísticas:', error)
      }
    },

    cambiarVista(vista) {
      this.vistaActiva = vista
    },

    // Métodos del Carrito (CREATE)
    agregarAlCarrito(torta) {
      const itemExistente = this.carrito.find(item => item.tortaId === torta.id)
      
      if (itemExistente) {
        itemExistente.cantidad += 1
      } else {
        this.carrito.push({
          tortaId: torta.id,
          nombre: torta.nombre,
          precio: torta.precio,
          cantidad: 1,
          imagen: torta.imagenPrincipal,
          vendedor: torta.vendedor
        })
      }
      
      this.guardarCarrito()
      this.mostrarToast(`${torta.nombre} agregada al carrito`, 'success', 'fa-cart-plus')
    },

    actualizarCantidadCarrito({ tortaId, cantidad }) {
      const item = this.carrito.find(item => item.tortaId === tortaId)
      if (item) {
        if (cantidad <= 0) {
          this.eliminarDelCarrito(tortaId)
        } else {
          item.cantidad = cantidad
          this.guardarCarrito()
        }
      }
    },

    eliminarDelCarrito(tortaId) {
      this.carrito = this.carrito.filter(item => item.tortaId !== tortaId)
      this.guardarCarrito()
      this.mostrarToast('Producto eliminado del carrito', 'warning', 'fa-trash')
    },

    async confirmarCompra(datosCompra) {
      try {
        // Simular creación de compra
        this.mostrarToast('Compra realizada exitosamente!', 'success', 'fa-check-circle')
        
        // Limpiar carrito
        this.carrito = []
        this.guardarCarrito()
        
        // Recargar estadísticas
        await this.cargarEstadisticas()
        
        // Ir al historial
        this.vistaActiva = 'historial'
      } catch (error) {
        this.mostrarToast('Error al realizar la compra', 'danger', 'fa-exclamation-circle')
      }
    },

    // Métodos del Historial (READ/UPDATE/DELETE)
    verDetalleTorta(torta) {
      // Implementar modal de detalle de torta
      console.log('Ver detalle torta:', torta)
    },

    verDetalleCompra(compra) {
      this.compraSeleccionada = compra
    },

    editarCompra(compra) {
      this.compraAEditar = compra
    },

    async guardarEdicionCompra(compraEditada) {
      try {
        // Simular actualización de compra
        this.mostrarToast('Compra actualizada exitosamente', 'success', 'fa-edit')
        this.compraAEditar = null
      } catch (error) {
        this.mostrarToast('Error al actualizar compra', 'danger', 'fa-exclamation-circle')
      }
    },

    cancelarCompra(compra) {
      this.compraACancelar = compra
    },

    async confirmarCancelacion() {
      try {
        // Simular cancelación de compra
        this.mostrarToast('Compra cancelada exitosamente', 'warning', 'fa-times-circle')
        this.compraACancelar = null
        await this.cargarEstadisticas()
      } catch (error) {
        this.mostrarToast('Error al cancelar compra', 'danger', 'fa-exclamation-circle')
      }
    },

    // Utilidades
    mostrarToast(mensaje, tipo, icono) {
      this.toasts.push({ mensaje, tipo, icono })
      setTimeout(() => {
        this.toasts.shift()
      }, 5000)
    },

    removerToast(index) {
      this.toasts.splice(index, 1)
    },

    logout() {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('carrito_comprador')
      this.$router.push('/login')
    }
  }
}
</script>

<style scoped>
.comprador-dashboard {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.nav .btn {
  text-align: left;
  border: none;
  border-radius: 0.375rem;
}

.nav .btn.active {
  background-color: #0d6efd;
  color: white;
}

.toast {
  z-index: 9999;
}
</style>