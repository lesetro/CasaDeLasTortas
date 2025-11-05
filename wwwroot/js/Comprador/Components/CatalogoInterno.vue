<template>
  <div class="catalogo-interno">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2><i class="fas fa-store me-2 text-primary"></i>Catálogo de Tortas</h2>
      <div class="d-flex gap-2">
        <input type="text" class="form-control" placeholder="Buscar tortas..." 
               v-model="filtros.busqueda" @input="filtrarTortas">
        <select class="form-select" v-model="filtros.categoria" @change="filtrarTortas">
          <option value="">Todas las categorías</option>
          <option v-for="categoria in categorias" :key="categoria" :value="categoria">
            {{ categoria }}
          </option>
        </select>
      </div>
    </div>

    <div v-if="cargando" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
    </div>

    <div v-else class="row">
      <div v-for="torta in tortasFiltradas" :key="torta.id" class="col-md-6 col-lg-4 mb-4">
        <div class="card h-100 torta-card">
          <img :src="torta.imagenPrincipal || '/images/torta-default.jpg'" 
               class="card-img-top" :alt="torta.nombre" style="height: 200px; object-fit: cover;">
          
          <div class="card-body d-flex flex-column">
            <h5 class="card-title">{{ torta.nombre }}</h5>
            <p class="card-text text-muted flex-grow-1">{{ torta.descripcionCorta }}</p>
            
            <div class="mb-2">
              <span class="badge bg-primary">{{ torta.categoria }}</span>
              <span v-if="torta.personalizable" class="badge bg-warning ms-1">Personalizable</span>
            </div>

            <div class="d-flex justify-content-between align-items-center mb-2">
              <strong class="h5 text-success mb-0">${{ torta.precio }}</strong>
              <small class="text-muted">
                <i class="fas fa-star text-warning"></i> {{ torta.calificacion }}
              </small>
            </div>

            <div class="d-flex justify-content-between align-items-center">
              <small class="text-muted">
                Por: <strong>{{ torta.vendedor.nombreComercial }}</strong>
              </small>
              <span v-if="torta.stock > 0" class="badge bg-success">
                {{ torta.stock }} disponibles
              </span>
              <span v-else class="badge bg-danger">Agotado</span>
            </div>

            <button class="btn btn-primary mt-3" 
                    :disabled="torta.stock === 0"
                    @click="agregarAlCarrito(torta)">
              <i class="fas fa-cart-plus me-1"></i>
              {{ torta.stock > 0 ? 'Agregar al Carrito' : 'Agotado' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="tortasFiltradas.length === 0 && !cargando" class="text-center py-5">
      <i class="fas fa-search fa-3x text-muted mb-3"></i>
      <h5 class="text-muted">No se encontraron tortas</h5>
      <p class="text-muted">Intenta con otros filtros de búsqueda</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CatalogoInterno',
  data() {
    return {
      tortas: [],
      cargando: false,
      filtros: {
        busqueda: '',
        categoria: ''
      }
    }
  },
  computed: {
    tortasFiltradas() {
      return this.tortas.filter(torta => {
        const coincideBusqueda = torta.nombre.toLowerCase().includes(this.filtros.busqueda.toLowerCase()) ||
                                torta.descripcionCorta.toLowerCase().includes(this.filtros.busqueda.toLowerCase())
        const coincideCategoria = !this.filtros.categoria || torta.categoria === this.filtros.categoria
        
        return coincideBusqueda && coincideCategoria
      })
    },
    categorias() {
      return [...new Set(this.tortas.map(t => t.categoria).filter(Boolean))]
    }
  },
  async mounted() {
    await this.cargarTortas()
  },
  methods: {
    async cargarTortas() {
      this.cargando = true
      try {
        // Simular carga de tortas desde API
        this.tortas = [
          {
            id: 1,
            nombre: 'Torta de Chocolate Premium',
            descripcionCorta: 'Deliciosa torta de chocolate con relleno cremoso',
            descripcion: 'Torta elaborada con chocolate belga premium, rellena de crema de chocolate y cubierta con ganache',
            precio: 45.99,
            categoria: 'Chocolate',
            calificacion: 4.8,
            stock: 5,
            personalizable: true,
            imagenPrincipal: '/images/torta-chocolate.jpg',
            vendedor: {
              id: 1,
              nombreComercial: 'Dulces Tentaciones'
            }
          },
          {
            id: 2,
            nombre: 'Torta de Vainilla con Frutas',
            descripcionCorta: 'Torta esponjosa de vainilla decorada con frutas frescas',
            precio: 38.50,
            categoria: 'Frutas',
            calificacion: 4.6,
            stock: 3,
            personalizable: false,
            imagenPrincipal: '/images/torta-vainilla.jpg',
            vendedor: {
              id: 2,
              nombreComercial: 'Postres Artesanales'
            }
          }
          // ... más tortas de ejemplo
        ]
      } catch (error) {
        console.error('Error cargando tortas:', error)
      } finally {
        this.cargando = false
      }
    },

    agregarAlCarrito(torta) {
      this.$emit('agregar-al-carrito', torta)
    },

    filtrarTortas() {
      // El filtrado se hace automáticamente en la computed property
    }
  }
}
</script>

<style scoped>
.torta-card {
  transition: transform 0.2s, box-shadow 0.2s;
  border: none;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.torta-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.card-img-top {
  border-bottom: 1px solid #dee2e6;
}
</style>