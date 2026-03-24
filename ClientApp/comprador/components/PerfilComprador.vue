<template>
  <div class="perfil-comprador">
    <div class="card shadow-sm">
      <div class="card-header bg-white d-flex justify-content-between align-items-center">
        <h5 class="mb-0"><i class="fas fa-user me-2"></i>Mi Perfil</h5>
        <button v-if="!editando" class="btn btn-primary btn-sm" @click="activarEdicion">
          <i class="fas fa-edit me-2"></i>Editar Perfil
        </button>
        <div v-else class="d-flex gap-2">
          <button class="btn btn-secondary btn-sm" @click="cancelarEdicion" :disabled="guardando">
            <i class="fas fa-times me-1"></i>Cancelar
          </button>
          <button class="btn btn-success btn-sm" @click="guardarPerfil" :disabled="guardando">
            <span v-if="guardando" class="spinner-border spinner-border-sm me-1"></span>
            <i v-else class="fas fa-save me-1"></i>Guardar
          </button>
        </div>
      </div>

      <div class="card-body">
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary"></div>
        </div>
        <div v-else-if="error" class="alert alert-warning">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>
        <div v-if="errorGuardado" class="alert alert-danger">
          <i class="fas fa-exclamation-circle me-2"></i>{{ errorGuardado }}
        </div>

        <div v-if="!loading && perfil" class="row">
          <!-- Avatar + Stats -->
          <div class="col-md-4 text-center mb-4">
            <img :src="avatarMostrar" class="rounded-circle img-thumbnail mb-3"
                 style="width:150px;height:150px;object-fit:cover">
            <h4>{{ perfil.nombrePersona }}</h4>
            <p class="text-muted">Comprador</p>
            <span class="badge bg-success mb-3">
              <i class="fas fa-check-circle me-1"></i>Activo
            </span>

            <div class="card bg-light">
              <div class="card-body">
                <h6 class="text-muted mb-3">Estadísticas</h6>
                <div class="mb-2">
                  <small class="text-muted">Compras Totales</small>
                  <div class="h5 mb-0">{{ perfil.totalCompras || 0 }}</div>
                </div>
                <hr>
                <div class="mb-2">
                  <small class="text-muted">Total Gastado</small>
                  <div class="h5 mb-0 text-success">{{ formatMoneda(perfil.totalGastado || 0) }}</div>
                </div>
                <hr>
                <div>
                  <small class="text-muted">Miembro desde</small>
                  <div class="small">{{ formatFecha(perfil.fechaRegistro) }}</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Info / Formulario -->
          <div class="col-md-8">
            <template v-if="!editando">
              <h5 class="mb-3">Información Personal</h5>
              <table class="table table-borderless">
                <tr>
                  <td class="text-muted" style="width:150px">Nombre:</td>
                  <td><strong>{{ perfil.nombrePersona }}</strong></td>
                </tr>
                <tr>
                  <td class="text-muted">Email:</td>
                  <td><i class="fas fa-envelope me-2 text-muted"></i>{{ perfil.email }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Teléfono:</td>
                  <td><i class="fas fa-phone me-2 text-muted"></i>{{ perfil.telefono || 'No especificado' }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Dirección:</td>
                  <td><i class="fas fa-map-marker-alt me-2 text-muted"></i>{{ perfil.direccion || 'No especificada' }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Ciudad:</td>
                  <td>{{ perfil.ciudad || 'No especificada' }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Provincia:</td>
                  <td>{{ perfil.provincia || 'No especificada' }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Código Postal:</td>
                  <td>{{ perfil.codigoPostal || 'No especificado' }}</td>
                </tr>
                <tr>
                  <td class="text-muted">Preferencias:</td>
                  <td>{{ perfil.preferencias || 'No especificadas' }}</td>
                </tr>
              </table>
            </template>

            <template v-else>
              <h5 class="mb-3">Editar Información</h5>
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Teléfono</label>
                  <input type="tel" class="form-control" v-model="form.telefono"
                         placeholder="+54 266 123-4567">
                </div>
                <div class="col-12">
                  <label class="form-label fw-semibold">Dirección <span class="text-danger">*</span></label>
                  <input type="text" class="form-control" v-model="form.direccion"
                         placeholder="Av. Principal 123">
                </div>
                <div class="col-md-5">
                  <label class="form-label fw-semibold">Ciudad</label>
                  <input type="text" class="form-control" v-model="form.ciudad"
                         placeholder="Villa Mercedes">
                </div>
                <div class="col-md-5">
                  <label class="form-label fw-semibold">Provincia</label>
                  <input type="text" class="form-control" v-model="form.provincia"
                         placeholder="San Luis">
                </div>
                <div class="col-md-2">
                  <label class="form-label fw-semibold">C. Postal</label>
                  <input type="text" class="form-control" v-model="form.codigoPostal"
                         placeholder="5730">
                </div>
                <div class="col-12">
                  <label class="form-label fw-semibold">Preferencias</label>
                  <textarea class="form-control" v-model="form.preferencias" rows="2"
                            placeholder="Ej: Sin gluten, tortas de frutas, personalizadas..."></textarea>
                  <div class="form-text">Tus preferencias ayudan a los vendedores a recomendarte mejor.</div>
                </div>
              </div>
            </template>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, formatMoneda, formatFecha, mostrarToast } from '@shared/apiUtils.js'

function generarAvatarUrl(nombre) {
  if (!nombre || !nombre.trim()) nombre = 'U'
  const encoded = encodeURIComponent(nombre.trim())
  return `https://ui-avatars.com/api/?name=${encoded}&background=random&color=fff&size=150&bold=true&format=svg`
}

const perfil        = ref(null)
const loading       = ref(true)
const error         = ref(null)
const errorGuardado = ref(null)
const editando      = ref(false)
const guardando     = ref(false)

const form = ref({
  telefono: '', direccion: '', ciudad: '',
  provincia: '', codigoPostal: '', preferencias: '',
})

const avatarMostrar = computed(() => {
  if (perfil.value?.avatar) return perfil.value.avatar
  return generarAvatarUrl(perfil.value?.nombrePersona || 'C')
})

async function cargarPerfil() {
  try {
    loading.value = true
    error.value = null
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const compradorId = user.compradorId
    if (!compradorId) { error.value = 'No se encontró el perfil de comprador.'; return }

    const data = await fetchWithAuth(`/api/CompradorApi/${compradorId}/perfil`)
    perfil.value = data
  } catch (err) {
    error.value = 'No se pudo cargar el perfil.'
    console.error('Error cargando perfil comprador:', err)
  } finally {
    loading.value = false
  }
}

function activarEdicion() {
  form.value = {
    telefono:     perfil.value.telefono     || '',
    direccion:    perfil.value.direccion    || '',
    ciudad:       perfil.value.ciudad       || '',
    provincia:    perfil.value.provincia    || '',
    codigoPostal: perfil.value.codigoPostal || '',
    preferencias: perfil.value.preferencias || '',
  }
  errorGuardado.value = null
  editando.value = true
}

function cancelarEdicion() {
  editando.value = false
  errorGuardado.value = null
}

async function guardarPerfil() {
  errorGuardado.value = null
  if (!form.value.direccion.trim()) {
    errorGuardado.value = 'La dirección es obligatoria.'
    return
  }

  try {
    guardando.value = true
    const user = JSON.parse(localStorage.getItem('user') || '{}')

    await fetchWithAuth(`/api/CompradorApi/${user.compradorId}`, {
      method: 'PUT',
      body: JSON.stringify({
        direccion:    form.value.direccion.trim(),
        telefono:     form.value.telefono.trim(),
        ciudad:       form.value.ciudad.trim()       || null,
        provincia:    form.value.provincia.trim()    || null,
        codigoPostal: form.value.codigoPostal.trim() || null,
        preferencias: form.value.preferencias.trim() || null,
        activo:       true,
      }),
    })

    perfil.value = {
      ...perfil.value,
      telefono:     form.value.telefono,
      direccion:    form.value.direccion,
      ciudad:       form.value.ciudad,
      provincia:    form.value.provincia,
      codigoPostal: form.value.codigoPostal,
      preferencias: form.value.preferencias,
    }

    editando.value = false
    mostrarToast('Perfil actualizado correctamente', 'success')
  } catch (err) {
    errorGuardado.value = 'Error al guardar. Verificá los datos e intentá nuevamente.'
    console.error('Error guardando perfil:', err)
  } finally {
    guardando.value = false
  }
}

onMounted(cargarPerfil)
</script>