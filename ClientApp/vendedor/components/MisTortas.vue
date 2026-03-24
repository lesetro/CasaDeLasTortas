<template>
  <div class="mis-tortas">
    <div class="card shadow-sm">
      <div class="card-header bg-white d-flex justify-content-between align-items-center flex-wrap gap-2">
        <h5 class="mb-0">
          <i class="fas fa-birthday-cake me-2"></i>Mis Tortas
          <span class="badge bg-primary ms-2">{{ tortas.length }}</span>
        </h5>
        <div class="d-flex gap-2 align-items-center flex-wrap">
          <div class="input-group input-group-sm" style="width:220px">
            <span class="input-group-text"><i class="fas fa-search"></i></span>
            <input type="text" class="form-control" placeholder="Buscar mis tortas..."
                   v-model="terminoBusqueda" @input="onBusqueda">
            <button v-if="buscando" class="btn btn-outline-secondary" disabled>
              <span class="spinner-border spinner-border-sm"></span>
            </button>
          </div>
          <button class="btn btn-success btn-sm" @click="abrirModalCrear">
            <i class="fas fa-plus me-2"></i>Nueva Torta
          </button>
        </div>
      </div>

      <div class="card-body">
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary mb-3"></div>
          <p class="text-muted">Cargando tortas...</p>
        </div>

        <div v-else-if="error" class="alert alert-warning">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>

        <div v-else-if="tortasMostradas.length === 0" class="text-center py-5 text-muted">
          <i class="fas fa-box-open fa-4x mb-3"></i>
          <h5>{{ terminoBusqueda ? 'No se encontraron tortas' : 'No tienes tortas publicadas' }}</h5>
          <p>{{ terminoBusqueda ? 'Intenta con otro término' : 'Crea tu primera torta para empezar a vender' }}</p>
          <button v-if="!terminoBusqueda" class="btn btn-success" @click="abrirModalCrear">
            <i class="fas fa-plus me-2"></i>Crear mi primera torta
          </button>
        </div>

        <div v-else class="row g-3">
          <div v-for="torta in tortasMostradas" :key="torta.id" class="col-md-6 col-lg-4">
            <div class="card h-100 shadow-sm">
              <div class="position-relative">
                <img v-if="torta.imagenPrincipal" :src="torta.imagenPrincipal"
                     class="card-img-top" style="height:200px;object-fit:cover">
                <div v-else class="bg-light d-flex align-items-center justify-content-center"
                     style="height:200px">
                  <i class="fas fa-birthday-cake fa-4x text-muted"></i>
                </div>
                <span class="position-absolute top-0 end-0 m-2 badge"
                      :class="torta.disponible ? 'bg-success' : 'bg-danger'">
                  {{ torta.disponible ? 'Activa' : 'Inactiva' }}
                </span>
                <span v-if="torta.stock <= 2 && torta.disponible"
                      class="position-absolute top-0 start-0 m-2 badge bg-warning text-dark">
                  <i class="fas fa-exclamation-triangle me-1"></i>Stock Bajo
                </span>
                <!-- Contador de imágenes -->
                <span class="position-absolute bottom-0 end-0 m-2 badge bg-dark bg-opacity-75">
                  <i class="fas fa-camera me-1"></i>{{ torta.totalImagenes || 0 }}
                </span>
              </div>

              <div class="card-body">
                <h5 class="card-title text-truncate" :title="torta.nombre">{{ torta.nombre }}</h5>
                <p class="card-text text-muted small" style="height:50px;overflow:hidden">
                  {{ torta.descripcion }}
                </p>
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <div>
                    <div class="h5 text-success mb-0">{{ formatMoneda(torta.precio) }}</div>
                    <small class="text-muted"><i class="fas fa-box me-1"></i>Stock: {{ torta.stock }}</small>
                  </div>
                  <div class="text-end">
                    <div class="small text-muted">
                      <i class="fas fa-shopping-cart me-1"></i>{{ torta.vecesVendida || 0 }} ventas
                    </div>
                  </div>
                </div>

                <div class="btn-group w-100">
                  <button class="btn btn-sm btn-outline-primary" title="Editar"
                          @click="abrirModalEditar(torta)">
                    <i class="fas fa-edit"></i>
                  </button>
                  <!-- NUEVO: Botón Imágenes -->
                  <button class="btn btn-sm btn-outline-info" title="Gestionar Imágenes"
                          @click="abrirModalImagenes(torta)">
                    <i class="fas fa-images"></i>
                  </button>
                  <button class="btn btn-sm"
                          :class="torta.disponible ? 'btn-outline-warning' : 'btn-outline-success'"
                          @click="toggleDisponibilidad(torta)"
                          :disabled="toggling === torta.id"
                          :title="torta.disponible ? 'Ocultar' : 'Mostrar'">
                    <span v-if="toggling === torta.id" class="spinner-border spinner-border-sm"></span>
                    <i v-else :class="torta.disponible ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="eliminarTorta(torta)"
                          title="Eliminar">
                    <i class="fas fa-trash"></i>
                  </button>
                </div>
              </div>

              <div class="card-footer bg-white border-top small">
                <span v-if="torta.categoria" class="badge bg-secondary">{{ torta.categoria }}</span>
                <span v-else class="text-muted">Sin categoría</span>
                <span v-if="torta.tamanio" class="badge bg-outline-secondary border ms-1">{{ torta.tamanio }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════
         MODAL CREAR / EDITAR TORTA
    ══════════════════════════════════════════ -->
    <div v-if="modalAbierto" class="modal d-block" tabindex="-1"
         style="background:rgba(0,0,0,.5)" @click.self="cerrarModal">
      <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-birthday-cake me-2"></i>
              {{ modoEdicion ? 'Editar Torta' : 'Nueva Torta' }}
            </h5>
            <button type="button" class="btn-close" @click="cerrarModal"></button>
          </div>

          <div class="modal-body">
            <div v-if="errorModal" class="alert alert-danger mb-3">
              <i class="fas fa-exclamation-circle me-2"></i>{{ errorModal }}
            </div>

            <div class="row g-3">
              <div class="col-md-8">
                <label class="form-label fw-semibold">Nombre <span class="text-danger">*</span></label>
                <input type="text" class="form-control" v-model="form.nombre"
                       placeholder="Ej: Torta de Chocolate Suprema" maxlength="100">
              </div>
              <div class="col-md-4">
                <label class="form-label fw-semibold">Categoría</label>
                <input type="text" class="form-control" v-model="form.categoria"
                       placeholder="Ej: Chocolate, Frutas..."
                       list="categorias-sugeridas">
                <datalist id="categorias-sugeridas">
                  <option v-for="cat in categoriasSugeridas" :key="cat" :value="cat" />
                </datalist>
              </div>
              <div class="col-12">
                <label class="form-label fw-semibold">Descripción <span class="text-danger">*</span></label>
                <textarea class="form-control" v-model="form.descripcion" rows="3"
                          placeholder="Describe tu torta: ingredientes, sabores, ocasiones..."></textarea>
              </div>
              <div class="col-md-4">
                <label class="form-label fw-semibold">Precio <span class="text-danger">*</span></label>
                <div class="input-group">
                  <span class="input-group-text">$</span>
                  <input type="number" class="form-control" v-model="form.precio"
                         min="0.01" step="0.01" placeholder="0.00">
                </div>
              </div>
              <div class="col-md-4">
                <label class="form-label fw-semibold">Stock <span class="text-danger">*</span></label>
                <input type="number" class="form-control" v-model="form.stock" min="0" placeholder="0">
              </div>
              <div class="col-md-4">
                <label class="form-label fw-semibold">Tamaño</label>
                <select class="form-select" v-model="form.tamanio">
                  <option value="Pequeña">Pequeña</option>
                  <option value="Mediana">Mediana</option>
                  <option value="Grande">Grande</option>
                  <option value="Extra Grande">Extra Grande</option>
                </select>
              </div>
              <div class="col-md-4">
                <label class="form-label fw-semibold">Días de preparación</label>
                <input type="number" class="form-control" v-model="form.tiempoPreparacion"
                       min="0" max="30" placeholder="1">
              </div>
              <div class="col-md-4 d-flex align-items-end">
                <div class="form-check mb-2">
                  <input class="form-check-input" type="checkbox" id="personalizable"
                         v-model="form.personalizable">
                  <label class="form-check-label fw-semibold" for="personalizable">Personalizable</label>
                </div>
              </div>
              <div class="col-md-4 d-flex align-items-end">
                <div class="form-check mb-2">
                  <input class="form-check-input" type="checkbox" id="disponible"
                         v-model="form.disponible">
                  <label class="form-check-label fw-semibold" for="disponible">Disponible al público</label>
                </div>
              </div>
              <div class="col-12">
                <label class="form-label fw-semibold">Ingredientes</label>
                <textarea class="form-control" v-model="form.ingredientes" rows="2"
                          placeholder="Harina, azúcar, huevos, chocolate..."></textarea>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="cerrarModal"
                    :disabled="guardando">Cancelar</button>
            <button type="button" class="btn btn-success" @click="guardarTorta"
                    :disabled="guardando">
              <span v-if="guardando" class="spinner-border spinner-border-sm me-2"></span>
              <i v-else class="fas fa-save me-2"></i>
              {{ modoEdicion ? 'Guardar Cambios' : 'Crear Torta' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════
         MODAL GESTIONAR IMÁGENES
    ══════════════════════════════════════════ -->
    <div v-if="modalImagenesAbierto" class="modal d-block" tabindex="-1"
         style="background:rgba(0,0,0,.5)" @click.self="cerrarModalImagenes">
      <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-images me-2"></i>
              Imágenes de "{{ tortaImagenes?.nombre }}"
            </h5>
            <button type="button" class="btn-close" @click="cerrarModalImagenes"></button>
          </div>

          <div class="modal-body">
            <!-- Subir imágenes -->
            <div class="border rounded p-3 mb-4 bg-light">
              <h6 class="fw-semibold mb-2">
                <i class="fas fa-upload me-2"></i>Subir Imágenes
              </h6>
              <div class="d-flex gap-2 align-items-end flex-wrap">
                <div class="flex-grow-1">
                  <input type="file" class="form-control" ref="inputArchivos"
                         accept="image/*" multiple @change="onSeleccionarArchivos">
                </div>
                <div class="form-check">
                  <input class="form-check-input" type="checkbox" id="esPrincipalUpload"
                         v-model="uploadEsPrincipal">
                  <label class="form-check-label small" for="esPrincipalUpload">
                    Imagen principal
                  </label>
                </div>
                <button class="btn btn-success" @click="subirImagenes"
                        :disabled="subiendo || archivosSeleccionados.length === 0">
                  <span v-if="subiendo" class="spinner-border spinner-border-sm me-1"></span>
                  <i v-else class="fas fa-cloud-upload-alt me-1"></i>
                  Subir
                </button>
              </div>
              <!-- Preview de archivos seleccionados -->
              <div v-if="archivosSeleccionados.length" class="d-flex gap-2 mt-2 flex-wrap">
                <div v-for="(file, i) in archivosSeleccionados" :key="i"
                     class="position-relative">
                  <img :src="previewUrls[i]" class="rounded border"
                       style="width:80px;height:80px;object-fit:cover">
                  <button class="btn btn-sm btn-danger position-absolute top-0 end-0 p-0"
                          style="width:20px;height:20px;font-size:10px;line-height:1"
                          @click="quitarArchivo(i)">
                    <i class="fas fa-times"></i>
                  </button>
                </div>
              </div>
              <div v-if="errorUpload" class="text-danger small mt-2">
                <i class="fas fa-exclamation-circle me-1"></i>{{ errorUpload }}
              </div>
            </div>

            <!-- Galería de imágenes existentes -->
            <h6 class="fw-semibold mb-2">
              <i class="fas fa-photo-video me-2"></i>Imágenes actuales
              <span class="badge bg-secondary ms-1">{{ imagenesExistentes.length }}</span>
            </h6>

            <div v-if="cargandoImagenes" class="text-center py-4">
              <div class="spinner-border spinner-border-sm text-primary"></div>
              <span class="ms-2 text-muted">Cargando...</span>
            </div>

            <div v-else-if="imagenesExistentes.length === 0" class="text-center py-4 text-muted">
              <i class="fas fa-image fa-3x mb-2 opacity-50"></i>
              <p class="mb-0">Esta torta no tiene imágenes. Subí la primera.</p>
            </div>

            <div v-else class="row g-2">
              <div v-for="img in imagenesExistentes" :key="img.id" class="col-4 col-md-3">
                <div class="card h-100 position-relative"
                     :class="img.esPrincipal ? 'border-success border-2' : ''">
                  <img :src="img.urlImagen" class="card-img-top"
                       style="height:120px;object-fit:cover">
                  <!-- Badge principal -->
                  <span v-if="img.esPrincipal"
                        class="position-absolute top-0 start-0 m-1 badge bg-success"
                        style="font-size:.65rem">
                    <i class="fas fa-star me-1"></i>Principal
                  </span>
                  <div class="card-body p-1 text-center">
                    <div class="btn-group btn-group-sm w-100">
                      <button v-if="!img.esPrincipal"
                              class="btn btn-outline-success" title="Establecer como principal"
                              @click="setPrincipal(img)"
                              :disabled="accionImagen === img.id">
                        <i class="fas fa-star"></i>
                      </button>
                      <button class="btn btn-outline-danger" title="Eliminar"
                              @click="eliminarImagen(img)"
                              :disabled="accionImagen === img.id">
                        <span v-if="accionImagen === img.id"
                              class="spinner-border spinner-border-sm"></span>
                        <i v-else class="fas fa-trash"></i>
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-secondary" @click="cerrarModalImagenes">Cerrar</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, formatMoneda, mostrarToast } from '@shared/apiUtils.js'

const emit = defineEmits(['recargar-estadisticas'])

// ── Estado principal ──────────────────────────────────────────────
const tortas             = ref([])
const resultadosBusqueda = ref([])
const loading            = ref(true)
const error              = ref(null)
const toggling           = ref(null)
const terminoBusqueda    = ref('')
const buscando           = ref(false)
let   timerBusqueda      = null

// ── Modal Crear/Editar ───────────────────────────────────────────
const modalAbierto   = ref(false)
const modoEdicion    = ref(false)
const guardando      = ref(false)
const errorModal     = ref(null)
const tortaEditando  = ref(null)

const formVacio = () => ({
  nombre: '', descripcion: '', precio: '', stock: 0,
  categoria: '', tamanio: 'Mediana', tiempoPreparacion: 1,
  personalizable: false, disponible: true, ingredientes: '',
})
const form = ref(formVacio())

// ── Modal Imágenes ───────────────────────────────────────────────
const modalImagenesAbierto  = ref(false)
const tortaImagenes         = ref(null)
const imagenesExistentes    = ref([])
const cargandoImagenes      = ref(false)
const accionImagen          = ref(null)
const archivosSeleccionados = ref([])
const previewUrls           = ref([])
const uploadEsPrincipal     = ref(false)
const subiendo              = ref(false)
const errorUpload           = ref(null)
const inputArchivos         = ref(null)

const categoriasSugeridas = computed(() =>
  [...new Set(tortas.value.map(t => t.categoria).filter(Boolean))]
)

const tortasMostradas = computed(() =>
  terminoBusqueda.value.length >= 2 ? resultadosBusqueda.value : tortas.value
)

// ── Carga inicial ────────────────────────────────────────────────
async function cargarTortas() {
  try {
    loading.value = true
    error.value = null
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const vendedorId = user.vendedorId
    if (!vendedorId) { error.value = 'No se encontró el perfil de vendedor.'; return }

    const data = await fetchWithAuth(`/api/VendedorApi/${vendedorId}/tortas`)
    const lista = Array.isArray(data) ? data : (data.data || [])
    tortas.value = lista.map(mapTorta)
  } catch (err) {
    error.value = 'No se pudieron cargar las tortas.'
    console.error('Error cargando tortas:', err)
  } finally {
    loading.value = false
  }
}

function mapTorta(t) {
  return {
    ...t,
    id: t.id || t.Id,    // ← normalizar ID (C# puede devolver Id en PascalCase)
    imagenPrincipal: (t.imagenes || t.Imagenes)?.find(i => i.esPrincipal || i.EsPrincipal)?.urlImagen
                   || (t.imagenes || t.Imagenes)?.[0]?.urlImagen
                   || (t.imagenes || t.Imagenes)?.[0]?.UrlImagen
                   || t.imagenPrincipal
                   || null,
    totalImagenes: (t.imagenes || t.Imagenes)?.length || 0,
  }
}

// ── Búsqueda Ajax ────────────────────────────────────────────────
function onBusqueda() {
  clearTimeout(timerBusqueda)
  if (terminoBusqueda.value.length < 2) {
    resultadosBusqueda.value = []
    return
  }
  timerBusqueda = setTimeout(buscarTortas, 400)
}

async function buscarTortas() {
  try {
    buscando.value = true
    const data = await fetchWithAuth(
      `/api/TortaApi/search?termino=${encodeURIComponent(terminoBusqueda.value)}`
    )
    const lista = Array.isArray(data) ? data : (data.data || [])
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    resultadosBusqueda.value = lista
      .filter(t => t.vendedorId === user.vendedorId)
      .map(mapTorta)
  } catch (err) {
    console.error('Error en búsqueda:', err)
  } finally {
    buscando.value = false
  }
}

// ── Modal Crear ──────────────────────────────────────────────────
function abrirModalCrear() {
  modoEdicion.value   = false
  tortaEditando.value = null
  form.value          = formVacio()
  errorModal.value    = null
  modalAbierto.value  = true
}

// ── Modal Editar ─────────────────────────────────────────────────
function abrirModalEditar(torta) {
  modoEdicion.value   = true
  tortaEditando.value = torta
  form.value = {
    nombre:            torta.nombre           || '',
    descripcion:       torta.descripcion      || '',
    precio:            torta.precio           || '',
    stock:             torta.stock            ?? 0,
    categoria:         torta.categoria        || '',
    tamanio:           torta.tamanio          || 'Mediana',
    tiempoPreparacion: torta.tiempoPreparacion ?? 1,
    personalizable:    torta.personalizable   ?? false,
    disponible:        torta.disponible       ?? true,
    ingredientes:      torta.ingredientes     || '',
  }
  errorModal.value   = null
  modalAbierto.value = true
}

function cerrarModal() {
  if (guardando.value) return
  modalAbierto.value = false
}

// ── Guardar torta ────────────────────────────────────────────────
async function guardarTorta() {
  errorModal.value = null
  if (!form.value.nombre.trim())      { errorModal.value = 'El nombre es obligatorio.'; return }
  if (!form.value.descripcion.trim()) { errorModal.value = 'La descripción es obligatoria.'; return }
  if (!form.value.precio || form.value.precio <= 0) { errorModal.value = 'El precio debe ser mayor a 0.'; return }

  try {
    guardando.value = true
    const user = JSON.parse(localStorage.getItem('user') || '{}')

    const body = {
      nombre:            form.value.nombre.trim(),
      descripcion:       form.value.descripcion.trim(),
      precio:            parseFloat(form.value.precio),
      stock:             parseInt(form.value.stock) || 0,
      categoria:         form.value.categoria.trim() || null,
      tamanio:           form.value.tamanio,
      tiempoPreparacion: parseInt(form.value.tiempoPreparacion) || 1,
      personalizable:    form.value.personalizable,
      disponible:        form.value.disponible,
      ingredientes:      form.value.ingredientes.trim() || null,
      vendedorId:        user.vendedorId,
    }

    if (modoEdicion.value) {
      await fetchWithAuth(`/api/TortaApi/${tortaEditando.value.id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
      })
      const idx = tortas.value.findIndex(t => t.id === tortaEditando.value.id)
      if (idx !== -1) tortas.value[idx] = { ...tortas.value[idx], ...body }
      mostrarToast('Torta actualizada correctamente', 'success')
    } else {
      const nueva = await fetchWithAuth('/api/TortaApi', {
        method: 'POST',
        body: JSON.stringify(body),
      })
      tortas.value.unshift(mapTorta(nueva))
      mostrarToast('Torta creada. Ahora podés agregarle imágenes.', 'success')

      // Abrir modal de imágenes para la torta recién creada
      cerrarModal()
      setTimeout(() => abrirModalImagenes(mapTorta(nueva)), 300)
      emit('recargar-estadisticas')
      return
    }

    cerrarModal()
    emit('recargar-estadisticas')
  } catch (err) {
    errorModal.value = 'Error al guardar la torta. Verificá los datos.'
    console.error('Error guardando torta:', err)
  } finally {
    guardando.value = false
  }
}

// ── Toggle disponibilidad ────────────────────────────────────────
async function toggleDisponibilidad(torta) {
  const accion = torta.disponible ? 'ocultar' : 'mostrar'
  if (!confirm(`¿Estás seguro de ${accion} la torta "${torta.nombre}"?`)) return
  try {
    toggling.value = torta.id
    await fetchWithAuth(`/api/TortaApi/${torta.id}/disponibilidad`, {
      method: 'PATCH',
      body: JSON.stringify(!torta.disponible),
    })
    torta.disponible = !torta.disponible
    mostrarToast(`Torta ${torta.disponible ? 'activada' : 'ocultada'}`, 'success')
    emit('recargar-estadisticas')
  } catch (err) {
    mostrarToast('Error al cambiar disponibilidad', 'error')
  } finally {
    toggling.value = null
  }
}

// ── Eliminar torta ───────────────────────────────────────────────
async function eliminarTorta(torta) {
  if (!confirm(`¿Eliminar la torta "${torta.nombre}"? Esta acción no se puede deshacer.`)) return
  try {
    await fetchWithAuth(`/api/TortaApi/${torta.id}`, { method: 'DELETE' })
    tortas.value = tortas.value.filter(t => t.id !== torta.id)
    mostrarToast('Torta eliminada', 'success')
    emit('recargar-estadisticas')
  } catch (err) {
    mostrarToast('Error al eliminar la torta', 'error')
  }
}

// ══════════════════════════════════════════════════════════════════
//  IMÁGENES
// ══════════════════════════════════════════════════════════════════

async function abrirModalImagenes(torta) {
  tortaImagenes.value        = torta
  imagenesExistentes.value   = []
  archivosSeleccionados.value = []
  previewUrls.value          = []
  uploadEsPrincipal.value    = false
  errorUpload.value          = null
  modalImagenesAbierto.value = true
  await cargarImagenesTorta(torta.id)
}

function cerrarModalImagenes() {
  modalImagenesAbierto.value = false
  // Limpiar previews de memoria
  previewUrls.value.forEach(url => URL.revokeObjectURL(url))
  archivosSeleccionados.value = []
  previewUrls.value = []
}

async function cargarImagenesTorta(tortaId) {
  if (!tortaId) { console.warn('tortaId es undefined, no se cargan imágenes'); return }
  try {
    cargandoImagenes.value = true
    const data = await fetchWithAuth(`/api/ImagenTortaApi/torta/${tortaId}`)
    const lista = Array.isArray(data) ? data : []
    // Normalizar campos por si vienen en PascalCase
    imagenesExistentes.value = lista.map(img => ({
      id: img.id || img.Id,
      urlImagen: img.urlImagen || img.UrlImagen,
      esPrincipal: img.esPrincipal ?? img.EsPrincipal ?? false,
      nombreArchivo: img.nombreArchivo || img.NombreArchivo,
    }))
  } catch (err) {
    console.error('Error cargando imágenes:', err)
    imagenesExistentes.value = []
  } finally {
    cargandoImagenes.value = false
  }
}

function onSeleccionarArchivos(e) {
  const files = Array.from(e.target.files || [])
  archivosSeleccionados.value = files
  // Generar previews
  previewUrls.value.forEach(url => URL.revokeObjectURL(url))
  previewUrls.value = files.map(f => URL.createObjectURL(f))
}

function quitarArchivo(index) {
  URL.revokeObjectURL(previewUrls.value[index])
  archivosSeleccionados.value.splice(index, 1)
  previewUrls.value.splice(index, 1)
}

async function subirImagenes() {
  if (!archivosSeleccionados.value.length || !tortaImagenes.value) return
  errorUpload.value = null

  try {
    subiendo.value = true
    const token = localStorage.getItem('authToken')

    const formData = new FormData()
    const tortaId = tortaImagenes.value.id || tortaImagenes.value.Id
    if (!tortaId) { errorUpload.value = 'No se pudo identificar la torta'; return }
    formData.append('TortaId', tortaId)
    if (uploadEsPrincipal.value) {
      formData.append('ImagenPrincipalIndex', '0')
    }
    archivosSeleccionados.value.forEach(f => formData.append('Imagenes', f))

    const resp = await fetch('/api/ImagenTortaApi/multiple', {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` },
      body: formData,
    })

    if (!resp.ok) {
      const err = await resp.json().catch(() => ({}))
      throw new Error(err.message || 'Error al subir')
    }

    mostrarToast('Imágenes subidas correctamente', 'success')

    // Recargar imágenes y actualizar la tarjeta
    await cargarImagenesTorta(tortaImagenes.value.id)
    actualizarImagenTarjeta(tortaImagenes.value.id)

    // Limpiar
    archivosSeleccionados.value = []
    previewUrls.value = []
    uploadEsPrincipal.value = false
    if (inputArchivos.value) inputArchivos.value.value = ''
  } catch (err) {
    errorUpload.value = err.message || 'Error al subir las imágenes'
    console.error('Error subiendo imágenes:', err)
  } finally {
    subiendo.value = false
  }
}

async function setPrincipal(img) {
  try {
    accionImagen.value = img.id
    const token = localStorage.getItem('authToken')
    await fetch(`/api/ImagenTortaApi/${img.id}/principal`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${token}` },
    })
    // Actualizar localmente
    imagenesExistentes.value.forEach(i => { i.esPrincipal = (i.id === img.id) })
    actualizarImagenTarjeta(tortaImagenes.value.id)
    mostrarToast('Imagen principal actualizada', 'success')
  } catch (err) {
    mostrarToast('Error al cambiar imagen principal', 'error')
  } finally {
    accionImagen.value = null
  }
}

async function eliminarImagen(img) {
  if (!confirm('¿Eliminar esta imagen?')) return
  try {
    accionImagen.value = img.id
    const token = localStorage.getItem('authToken')
    await fetch(`/api/ImagenTortaApi/${img.id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` },
    })
    imagenesExistentes.value = imagenesExistentes.value.filter(i => i.id !== img.id)
    actualizarImagenTarjeta(tortaImagenes.value.id)
    mostrarToast('Imagen eliminada', 'success')
  } catch (err) {
    mostrarToast('Error al eliminar la imagen', 'error')
  } finally {
    accionImagen.value = null
  }
}

function actualizarImagenTarjeta(tortaId) {
  const idx = tortas.value.findIndex(t => t.id === tortaId)
  if (idx === -1) return
  const principal = imagenesExistentes.value.find(i => i.esPrincipal)
  tortas.value[idx].imagenPrincipal = principal?.urlImagen
    || imagenesExistentes.value[0]?.urlImagen
    || null
  tortas.value[idx].totalImagenes = imagenesExistentes.value.length
}

onMounted(cargarTortas)
defineExpose({ cargarTortas })
</script>