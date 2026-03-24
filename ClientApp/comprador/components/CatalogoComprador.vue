<template>
  <div class="catalogo-comprador">
    <div class="card shadow-sm">
      <div class="card-header bg-white d-flex justify-content-between align-items-center flex-wrap gap-2">
        <h5 class="mb-0">
          <i class="fas fa-store me-2"></i>Catálogo de Tortas
          <span class="badge bg-primary ms-2">{{ tortasMostradas.length }}</span>
        </h5>
        <div class="d-flex gap-2 flex-wrap">
          <div class="input-group input-group-sm" style="width:220px">
            <span class="input-group-text">
              <span v-if="buscando" class="spinner-border spinner-border-sm"></span>
              <i v-else class="fas fa-search"></i>
            </span>
            <input type="text" class="form-control" placeholder="Buscar tortas..."
                   v-model="filtroBusqueda" @input="onBusqueda">
            <button v-if="filtroBusqueda" class="btn btn-outline-secondary btn-sm"
                    @click="limpiarBusqueda" title="Limpiar">
              <i class="fas fa-times"></i>
            </button>
          </div>
          <select class="form-select form-select-sm" style="width:160px" v-model="filtroCategoria">
            <option value="">Todas las categorías</option>
            <option v-for="cat in categorias" :key="cat" :value="cat">{{ cat }}</option>
          </select>
        </div>
      </div>

      <div class="card-body">
        <!-- Info de retiro -->
        <div class="alert alert-info small py-2 mb-3">
          <i class="fas fa-store me-2"></i>
          <strong>Retiro en local:</strong> Todos los pedidos se retiran en nuestro local una vez que el vendedor confirme que están listos.
        </div>

        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary mb-3"></div>
          <p class="text-muted">Cargando catálogo...</p>
        </div>

        <div v-else-if="error" class="alert alert-warning">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>

        <div v-else-if="tortasMostradas.length === 0" class="text-center py-5 text-muted">
          <i class="fas fa-search fa-3x mb-3"></i>
          <h5>No se encontraron tortas disponibles</h5>
          <p v-if="filtroBusqueda">No hay resultados para "<strong>{{ filtroBusqueda }}</strong>"</p>
          <p v-else>Los vendedores aún no tienen tortas con stock disponible</p>
          <button v-if="filtroBusqueda" class="btn btn-outline-primary btn-sm" @click="limpiarBusqueda">
            <i class="fas fa-times me-1"></i>Limpiar búsqueda
          </button>
        </div>

        <!-- Grilla de tortas -->
        <div v-else class="row g-3">
          <div v-for="torta in tortasMostradas" :key="torta.id" class="col-md-6 col-lg-4">
            <div class="card h-100 shadow-sm"
                 :id="`torta-${torta.id}`"
                 :class="{ 'border-primary border-3 shadow-lg': tortaDestacada === torta.id }">

              <div class="position-relative">
                <img v-if="torta.imagenPrincipal" :src="torta.imagenPrincipal"
                     class="card-img-top" style="height:200px;object-fit:cover" :alt="torta.nombre">
                <div v-else class="bg-light d-flex align-items-center justify-content-center" style="height:200px">
                  <i class="fas fa-birthday-cake fa-3x text-muted"></i>
                </div>
                <span class="position-absolute top-0 end-0 m-2 badge"
                      :class="torta.stock > 0 ? 'bg-success' : 'bg-danger'">
                  {{ torta.stock > 0 ? 'Disponible' : 'Agotado' }}
                </span>
                <span v-if="torta.personalizable" class="position-absolute top-0 start-0 m-2 badge bg-warning text-dark">
                  <i class="fas fa-paint-brush me-1"></i>Personalizable
                </span>
              </div>

              <div class="card-body d-flex flex-column">
                <h5 class="card-title">{{ torta.nombre }}</h5>
                <p class="card-text text-muted small flex-grow-1">{{ torta.descripcionCorta }}</p>

                <div class="mb-2">
                  <span v-if="torta.categoria" class="badge bg-secondary">{{ torta.categoria }}</span>
                  <span v-if="torta.tamanio" class="badge bg-outline-secondary border ms-1">{{ torta.tamanio }}</span>
                  <span v-if="torta.tiempoPreparacion" class="badge bg-light text-muted border ms-1">
                    <i class="fas fa-clock me-1"></i>{{ torta.tiempoPreparacion }} días
                  </span>
                </div>

                <div class="d-flex justify-content-between align-items-center mb-2">
                  <div class="h4 text-success mb-0">{{ formatMoneda(torta.precio) }}</div>
                  <small class="text-muted">
                    <i class="fas fa-box me-1"></i>Stock: {{ torta.stock }}
                  </small>
                </div>

                <small class="text-muted mb-3">
                  Por: <strong>{{ torta.nombreVendedor || 'Vendedor' }}</strong>
                </small>

                <!-- Botón agregar al carrito -->
                <button v-if="torta.stock > 0"
                        class="btn btn-primary w-100"
                        :disabled="agregando === torta.id"
                        @click="agregarAlCarrito(torta)">
                  <span v-if="agregando === torta.id" class="spinner-border spinner-border-sm me-2"></span>
                  <i v-else class="fas fa-cart-plus me-2"></i>
                  Agregar al Carrito
                </button>
                <button v-else class="btn btn-secondary w-100" disabled>
                  <i class="fas fa-ban me-2"></i>Sin Stock
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Indicador de búsqueda -->
        <div v-if="filtroBusqueda && !buscando && !loading" class="text-center mt-3">
          <small class="text-muted">
            Mostrando resultados para "<strong>{{ filtroBusqueda }}</strong>"
            — <a href="#" @click.prevent="limpiarBusqueda" class="text-primary">Ver todas</a>
          </small>
        </div>
      </div>
    </div>

    <!-- Toast de agregado al carrito -->
    <div v-if="toastMsg" class="position-fixed bottom-0 end-0 p-3" style="z-index:1100">
      <div class="toast show align-items-center text-white bg-success border-0">
        <div class="d-flex">
          <div class="toast-body">
            <i class="fas fa-check-circle me-2"></i>{{ toastMsg }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto"
                  @click="toastMsg = null"></button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, formatMoneda, mostrarToast } from '@shared/apiUtils.js'

const emit = defineEmits(['actualizar-carrito', 'ir-carrito'])

const todasLasTortas     = ref([])
const resultadosBusqueda = ref([])
const loading            = ref(true)
const buscando           = ref(false)
const error              = ref(null)
const filtroBusqueda     = ref('')
const filtroCategoria    = ref('')
const agregando          = ref(null)
const tortaDestacada     = ref(null)
const toastMsg           = ref(null)
let   timerBusqueda      = null
let   toastTimer         = null

const categorias = computed(() =>
  [...new Set(todasLasTortas.value.map(t => t.categoria).filter(Boolean))].sort()
)

const tortasMostradas = computed(() => {
  let lista = filtroBusqueda.value.length >= 2 ? resultadosBusqueda.value : todasLasTortas.value
  if (filtroCategoria.value) {
    lista = lista.filter(t => t.categoria === filtroCategoria.value)
  }
  return lista
})

async function cargarCatalogo() {
  try {
    loading.value = true
    error.value = null
    const data = await fetchWithAuth('/api/TortaApi/disponibles')
    
    // Manejar diferentes formatos de respuesta
    const lista = Array.isArray(data) ? data : (data.data || [])
    
    console.log('📦 Tortas recibidas:', lista.length)
    
    todasLasTortas.value = lista.map(mapTorta)
  } catch (err) {
    error.value = 'No se pudo cargar el catálogo. Intenta nuevamente.'
    console.error('Error cargando catálogo:', err)
  } finally {
    loading.value = false
  }
}

// 🔥 FUNCIÓN CORREGIDA: Maneja tanto mayúsculas como minúsculas
function mapTorta(t) {
  // Obtener imagen principal - manejar diferentes casos
  let imagenPrincipal = null
  
  // Caso 1: imagenPrincipal viene directamente
  if (t.imagenPrincipal) {
    imagenPrincipal = t.imagenPrincipal
  }
  // Caso 2: ImagenPrincipal con mayúscula
  else if (t.ImagenPrincipal) {
    imagenPrincipal = t.ImagenPrincipal
  }
  // Caso 3: array de imagenes (minúscula)
  else if (t.imagenes && Array.isArray(t.imagenes)) {
    const principal = t.imagenes.find(i => i.esPrincipal || i.EsPrincipal)
    imagenPrincipal = principal?.urlImagen || principal?.UrlImagen || t.imagenes[0]?.urlImagen || t.imagenes[0]?.UrlImagen
  }
  // Caso 4: array de Imagenes (mayúscula)
  else if (t.Imagenes && Array.isArray(t.Imagenes)) {
    const principal = t.Imagenes.find(i => i.EsPrincipal || i.esPrincipal)
    imagenPrincipal = principal?.UrlImagen || principal?.urlImagen || t.Imagenes[0]?.UrlImagen || t.Imagenes[0]?.urlImagen
  }

  // Obtener descripción
  const descripcion = t.descripcion || t.Descripcion || ''
  
  return {
    id: t.id || t.Id,
    nombre: t.nombre || t.Nombre || 'Sin nombre',
    descripcion: descripcion,
    descripcionCorta: descripcion.length > 100 
      ? descripcion.substring(0, 100) + '...' 
      : (descripcion || 'Sin descripción'),
    precio: t.precio || t.Precio || 0,
    stock: t.stock || t.Stock || 0,
    categoria: t.categoria || t.Categoria || '',
    tamanio: t.tamanio || t.Tamanio || '',
    tiempoPreparacion: t.tiempoPreparacion || t.TiempoPreparacion || 0,
    personalizable: t.personalizable || t.Personalizable || false,
    disponible: t.disponible ?? t.Disponible ?? true,
    nombreVendedor: t.nombreVendedor || t.NombreVendedor || 'Vendedor',
    vendedorId: t.vendedorId || t.VendedorId || 0,
    imagenPrincipal: imagenPrincipal,
    calificacion: t.calificacion || t.Calificacion || 0,
    vecesVendida: t.vecesVendida || t.VecesVendida || 0
  }
}

function onBusqueda() {
  clearTimeout(timerBusqueda)
  if (filtroBusqueda.value.length < 2) { resultadosBusqueda.value = []; return }
  timerBusqueda = setTimeout(ejecutarBusqueda, 400)
}

async function ejecutarBusqueda() {
  try {
    buscando.value = true
    const data = await fetchWithAuth(`/api/TortaApi/search?termino=${encodeURIComponent(filtroBusqueda.value)}`)
    const lista = Array.isArray(data) ? data : (data.data || [])
    resultadosBusqueda.value = lista.map(mapTorta)
  } catch (err) {
    console.error('Error en búsqueda:', err)
    resultadosBusqueda.value = []
  } finally {
    buscando.value = false
  }
}

function limpiarBusqueda() {
  filtroBusqueda.value = ''
  resultadosBusqueda.value = []
}

async function agregarAlCarrito(torta) {
  try {
    agregando.value = torta.id
    const data = await fetchWithAuth(`/Carrito/Agregar?tortaId=${torta.id}&cantidad=1`, {
      method: 'POST',
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    if (data?.success === false) {
      mostrarToast('No se pudo agregar al carrito', 'error')
      return
    }
    // Mostrar toast local
    clearTimeout(toastTimer)
    toastMsg.value = `"${torta.nombre}" agregada al carrito`
    toastTimer = setTimeout(() => { toastMsg.value = null }, 3000)

    emit('actualizar-carrito', data?.totalItems)
  } catch (err) {
    console.error('Error agregando al carrito:', err)
    mostrarToast('Error al agregar al carrito', 'error')
  } finally {
    agregando.value = null
  }
}

onMounted(async () => {
  await cargarCatalogo()

  const params  = new URLSearchParams(window.location.search)
  const appDiv  = document.getElementById('app')
  const tortaId = params.get('verTorta') || appDiv?.dataset?.verTorta || sessionStorage.getItem('verTorta')

  if (tortaId) {
    const id = parseInt(tortaId)
    tortaDestacada.value = id
    sessionStorage.removeItem('verTorta')
    window.history.replaceState({}, '', window.location.pathname)
    setTimeout(() => {
      document.getElementById(`torta-${id}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }, 600)
  }
})
</script>