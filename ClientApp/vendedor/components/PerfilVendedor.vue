<template>
  <div class="perfil-vendedor">
    <div class="card border-0 shadow-sm rounded-3">

      <!-- Header -->
      <div class="card-header bg-white border-0 d-flex justify-content-between align-items-center px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-user text-success me-2"></i>Mi Perfil de Vendedor
        </h5>
        <div class="d-flex gap-2">
          <button v-if="!editando" class="btn btn-success btn-sm fw-semibold" @click="activarEdicion">
            <i class="fas fa-edit me-2"></i>Editar Perfil
          </button>
          <template v-else>
            <button class="btn btn-outline-secondary btn-sm" @click="cancelarEdicion" :disabled="guardando">
              <i class="fas fa-times me-1"></i>Cancelar
            </button>
            <button class="btn btn-success btn-sm fw-semibold" @click="guardarPerfil" :disabled="guardando">
              <span v-if="guardando" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="fas fa-save me-1"></i>Guardar
            </button>
          </template>
        </div>
      </div>

      <div class="card-body px-4 pb-4">

        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-success"></div>
        </div>
        <div v-else-if="error" class="alert alert-warning border-0 rounded-3">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>
        <div v-if="errorGuardado" class="alert alert-danger border-0 rounded-3">
          <i class="fas fa-exclamation-circle me-2"></i>{{ errorGuardado }}
        </div>

        <div v-if="!loading && vendedor" class="row g-4">

          <!-- ── Columna izquierda: avatar + stats ── -->
          <div class="col-md-4">
            <div class="text-center mb-4">
              <img :src="avatarMostrar"
                   class="rounded-circle border border-3 mb-3"
                   style="width:110px;height:110px;object-fit:cover;border-color:#22c55e!important;" />
              <h5 class="fw-bold mb-0">{{ vendedor.nombreComercial }}</h5>
              <p class="text-muted small">{{ vendedor.especialidad || 'Pastelería Artesanal' }}</p>
              <span v-if="vendedor.verificado" class="badge bg-success rounded-pill px-3">
                <i class="fas fa-check-circle me-1"></i>Verificado
              </span>
              <span v-else class="badge bg-warning text-dark rounded-pill px-3">
                <i class="fas fa-clock me-1"></i>Pendiente verificación
              </span>
            </div>

            <!-- Stats card -->
            <div class="p-3 rounded-3" style="background:#f8fafc;">
              <div class="text-muted small fw-semibold text-uppercase mb-3" style="letter-spacing:.08em;">Estadísticas</div>
              <div class="mb-3">
                <div class="text-muted small mb-1">Calificación</div>
                <div class="d-flex align-items-center gap-1">
                  <span v-for="n in 5" :key="n" class="text-warning">
                    <i :class="n <= Math.round(vendedor.calificacion || 0) ? 'fas fa-star' : 'far fa-star'"></i>
                  </span>
                  <small class="text-muted ms-1">{{ (vendedor.calificacion || 0).toFixed(1) }}</small>
                </div>
              </div>
              <div class="d-flex justify-content-between py-2 border-bottom">
                <span class="text-muted small">Total ventas</span>
                <strong class="small">{{ vendedor.totalVentas || 0 }}</strong>
              </div>
              <div class="d-flex justify-content-between py-2">
                <span class="text-muted small">Horario</span>
                <span class="small">{{ vendedor.horario || '—' }}</span>
              </div>
            </div>

            <!-- Alerta datos bancarios incompletos -->
            <div v-if="!vendedor.aliasCbu && !editando"
                 class="alert border-0 rounded-3 mt-3 p-3"
                 style="background:#fef9c3; border-left:3px solid #f59e0b!important;">
              <div class="fw-semibold small">
                <i class="fas fa-exclamation-triangle text-warning me-1"></i>
                Datos de cobro incompletos
              </div>
              <div class="text-muted small mt-1">
                Completá tus datos bancarios para recibir pagos de la plataforma.
              </div>
              <button class="btn btn-sm btn-warning mt-2 fw-semibold"
                      @click="activarEdicion(); seccionActiva = 'cobros'">
                Completar ahora
              </button>
            </div>
          </div>

          <!-- ── Columna derecha: tabs ── -->
          <div class="col-md-8">

            <!-- Tabs -->
            <ul class="nav nav-tabs mb-4 border-0" role="tablist">
              <li class="nav-item">
                <button class="nav-link fw-semibold"
                        :class="{ active: seccionActiva === 'negocio' }"
                        @click="seccionActiva = 'negocio'">
                  <i class="fas fa-store me-1"></i>Negocio
                </button>
              </li>
              <li class="nav-item">
                <button class="nav-link fw-semibold"
                        :class="{ active: seccionActiva === 'personal' }"
                        @click="seccionActiva = 'personal'">
                  <i class="fas fa-user me-1"></i>Personal
                </button>
              </li>
              <li class="nav-item">
                <button class="nav-link fw-semibold position-relative"
                        :class="{ active: seccionActiva === 'cobros' }"
                        @click="seccionActiva = 'cobros'">
                  <i class="fas fa-university me-1"></i>Datos de Cobro
                  <span v-if="!vendedor.aliasCbu"
                        class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning text-dark"
                        style="font-size:.55rem;">!</span>
                </button>
              </li>
            </ul>

            <!-- ───── TAB NEGOCIO ───── -->
            <div v-show="seccionActiva === 'negocio'">
              <template v-if="!editando">
                <div class="row g-3">
                  <div class="col-12">
                    <div class="text-muted small fw-semibold mb-1">Nombre Comercial</div>
                    <div class="fw-semibold">{{ vendedor.nombreComercial }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Especialidad</div>
                    <div>{{ vendedor.especialidad || '—' }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Horario</div>
                    <div>{{ vendedor.horario || '—' }}</div>
                  </div>
                  <div class="col-12">
                    <div class="text-muted small fw-semibold mb-1">Descripción</div>
                    <div class="text-muted small">{{ vendedor.descripcion || 'Sin descripción' }}</div>
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="row g-3">
                  <div class="col-12">
                    <label class="form-label fw-semibold small">
                      Nombre Comercial <span class="text-danger">*</span>
                    </label>
                    <input type="text" class="form-control" v-model="form.nombreComercial"
                           placeholder="Mi Tortería Artesanal" maxlength="100" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">Especialidad</label>
                    <input type="text" class="form-control" v-model="form.especialidad"
                           placeholder="Ej: Tortas de cumpleaños..." maxlength="100" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">Horario de Atención</label>
                    <input type="text" class="form-control" v-model="form.horario"
                           placeholder="Lunes a Viernes 9:00 - 18:00" maxlength="100" />
                  </div>
                  <div class="col-12">
                    <label class="form-label fw-semibold small">Descripción del Negocio</label>
                    <textarea class="form-control" v-model="form.descripcion" rows="3"
                              placeholder="Contá a tus clientes sobre tu negocio..."></textarea>
                  </div>
                </div>
              </template>
            </div>

            <!-- ───── TAB PERSONAL ───── -->
            <div v-show="seccionActiva === 'personal'">
              <template v-if="!editando">
                <div class="row g-3">
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Nombre completo</div>
                    <div class="fw-semibold">{{ vendedor.nombre }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Email</div>
                    <div>{{ vendedor.email }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Teléfono</div>
                    <div>{{ vendedor.telefono || '—' }}</div>
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">Teléfono</label>
                    <input type="tel" class="form-control" v-model="form.telefono"
                           placeholder="+54 266 123-4567" />
                  </div>
                </div>
              </template>
            </div>

            <!-- ───── TAB DATOS DE COBRO ───── -->
            <div v-show="seccionActiva === 'cobros'">
              <div class="p-3 rounded-3 mb-4" style="background:#f0fdf4; border:1px solid #bbf7d0;">
                <div class="fw-semibold small text-success mb-1">
                  <i class="fas fa-info-circle me-1"></i>¿Para qué sirven estos datos?
                </div>
                <div class="text-muted small">
                  Cuando un comprador retire su pedido y lo confirme, el admin transferirá tu parte
                  (descontando la comisión del {{ comisionPlataforma }}%) a esta cuenta bancaria.
                </div>
              </div>

              <template v-if="!editando">
                <div v-if="vendedor.aliasCbu || vendedor.cbu" class="row g-3">
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Alias</div>
                    <div class="fw-semibold font-monospace">{{ vendedor.aliasCbu || '—' }}</div>
                  </div>
                  <div class="col-12">
                    <div class="text-muted small fw-semibold mb-1">CBU</div>
                    <div class="fw-semibold font-monospace small">{{ vendedor.cbu || '—' }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Banco</div>
                    <div>{{ vendedor.banco || '—' }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Titular</div>
                    <div>{{ vendedor.titularCuenta || '—' }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">CUIT / DNI</div>
                    <div>{{ vendedor.cuit || '—' }}</div>
                  </div>
                  <div class="col-md-6">
                    <div class="text-muted small fw-semibold mb-1">Tipo de cuenta</div>
                    <div>{{ vendedor.tipoCuenta || '—' }}</div>
                  </div>
                </div>
                <div v-else class="text-center py-4 text-muted">
                  <i class="fas fa-university fa-2x mb-2 d-block opacity-30"></i>
                  <p class="small mb-2">No cargaste tus datos bancarios aún.</p>
                  <button class="btn btn-sm btn-success fw-semibold" @click="activarEdicion">
                    <i class="fas fa-plus me-1"></i>Agregar datos bancarios
                  </button>
                </div>
              </template>

              <template v-else>
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">
                      Alias <span class="text-danger">*</span>
                    </label>
                    <div class="input-group">
                      <span class="input-group-text bg-white"><i class="fas fa-at text-muted small"></i></span>
                      <input type="text" class="form-control" v-model="form.aliasCbu"
                             placeholder="tu.alias.banco" />
                    </div>
                  </div>
                  <div class="col-12">
                    <label class="form-label fw-semibold small">CBU</label>
                    <input type="text" class="form-control font-monospace" v-model="form.cbu"
                           placeholder="0000000000000000000000" maxlength="22" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">Banco</label>
                    <input type="text" class="form-control" v-model="form.banco"
                           placeholder="Ej: Banco Nación" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">
                      Titular de la cuenta <span class="text-danger">*</span>
                    </label>
                    <input type="text" class="form-control" v-model="form.titularCuenta"
                           placeholder="Tu nombre completo o razón social" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">CUIT / DNI</label>
                    <input type="text" class="form-control" v-model="form.cuit"
                           placeholder="XX-XXXXXXXX-X" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-semibold small">Tipo de cuenta</label>
                    <select class="form-select" v-model="form.tipoCuenta">
                      <option value="Caja de Ahorro">Caja de Ahorro</option>
                      <option value="Cuenta Corriente">Cuenta Corriente</option>
                      <option value="Cuenta Virtual">Cuenta Virtual (CVU)</option>
                    </select>
                  </div>
                </div>
              </template>
            </div>

          </div>
        </div><!-- /row -->
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, mostrarToast, generarAvatarUrl } from '@shared/apiUtils.js'

const vendedor      = ref(null)
const loading       = ref(true)
const error         = ref(null)
const errorGuardado = ref(null)
const editando      = ref(false)
const guardando     = ref(false)
const seccionActiva = ref('negocio')
const comisionPlataforma = ref(10)

const form = ref({
  nombreComercial: '', especialidad: '', descripcion: '', horario: '', telefono: '',
  // datos bancarios
  aliasCbu: '', cbu: '', banco: '', titularCuenta: '', cuit: '', tipoCuenta: 'Caja de Ahorro',
})

const avatarMostrar = computed(() => {
  if (vendedor.value?.avatar) return vendedor.value.avatar
  return generarAvatarUrl(vendedor.value?.nombre || vendedor.value?.nombreComercial || 'V')
})

async function cargarPerfil() {
  try {
    loading.value = true
    error.value   = null
    const data = await fetchWithAuth('/api/AuthApi/me')
    if (!data?.user) { error.value = 'No se encontró el perfil.'; return }

    const u = data.user
    let vendedorData = {}
    if (u.rolData?.vendedorId) {
      try { vendedorData = await fetchWithAuth(`/api/VendedorApi/${u.rolData.vendedorId}`) } catch {}
    }

    vendedor.value = {
      nombre:          u.nombre,
      email:           u.email,
      telefono:        u.telefono,
      avatar:          u.avatar,
      nombreComercial: vendedorData.nombreComercial || u.rolData?.nombreComercial || u.nombre,
      especialidad:    vendedorData.especialidad    || '',
      descripcion:     vendedorData.descripcion     || '',
      horario:         vendedorData.horario         || '',
      calificacion:    vendedorData.calificacion    ?? 0,
      totalVentas:     vendedorData.totalVentas     ?? 0,
      verificado:      vendedorData.verificado      ?? false,
      // datos bancarios (nuevos campos de Fase 1)
      aliasCbu:        vendedorData.aliasCbu        || '',
      cbu:             vendedorData.cbu             || '',
      banco:           vendedorData.banco           || '',
      titularCuenta:   vendedorData.titularCuenta   || '',
      cuit:            vendedorData.cuit            || '',
      tipoCuenta:      vendedorData.tipoCuenta      || '',
    }

    // Cargar comisión desde config
    try {
      const cfg = await fetchWithAuth('/api/ConfiguracionApi/datos-pago')
      comisionPlataforma.value = cfg.comision ?? 10
    } catch {}

  } catch (err) {
    error.value = 'No se pudo cargar el perfil.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

function activarEdicion() {
  form.value = {
    nombreComercial: vendedor.value.nombreComercial || '',
    especialidad:    vendedor.value.especialidad    || '',
    descripcion:     vendedor.value.descripcion     || '',
    horario:         vendedor.value.horario         || '',
    telefono:        vendedor.value.telefono        || '',
    aliasCbu:        vendedor.value.aliasCbu        || '',
    cbu:             vendedor.value.cbu             || '',
    banco:           vendedor.value.banco           || '',
    titularCuenta:   vendedor.value.titularCuenta   || '',
    cuit:            vendedor.value.cuit            || '',
    tipoCuenta:      vendedor.value.tipoCuenta      || 'Caja de Ahorro',
  }
  errorGuardado.value = null
  editando.value      = true
}

function cancelarEdicion() {
  editando.value      = false
  errorGuardado.value = null
}

async function guardarPerfil() {
  errorGuardado.value = null
  if (!form.value.nombreComercial.trim()) {
    errorGuardado.value = 'El nombre comercial es obligatorio.'
    return
  }

  try {
    guardando.value = true
    const user       = JSON.parse(localStorage.getItem('user') || '{}')
    const vendedorId = user.vendedorId

    await fetchWithAuth(`/api/VendedorApi/${vendedorId}`, {
      method: 'PUT',
      body: JSON.stringify({
        personaId:       vendedorId,
        nombreComercial: form.value.nombreComercial.trim(),
        especialidad:    form.value.especialidad.trim()    || null,
        descripcion:     form.value.descripcion.trim()     || null,
        horario:         form.value.horario.trim()         || null,
        calificacion:    vendedor.value.calificacion,
        totalVentas:     vendedor.value.totalVentas,
        activo:          true,
        fechaCreacion:   new Date().toISOString(),
        // datos bancarios
        aliasCbu:        form.value.aliasCbu.trim()        || null,
        cbu:             form.value.cbu.trim()             || null,
        banco:           form.value.banco.trim()           || null,
        titularCuenta:   form.value.titularCuenta.trim()   || null,
        cuit:            form.value.cuit.trim()            || null,
        tipoCuenta:      form.value.tipoCuenta             || null,
      }),
    })

    // Actualizar local
    vendedor.value = { ...vendedor.value, ...form.value }
    editando.value = false
    mostrarToast('Perfil actualizado correctamente', 'success')
  } catch (err) {
    errorGuardado.value = 'Error al guardar. Verificá los datos e intentá nuevamente.'
    console.error(err)
  } finally {
    guardando.value = false
  }
}

onMounted(cargarPerfil)
</script>

<style scoped>
.nav-tabs .nav-link { border: none; color: #64748b; border-bottom: 2px solid transparent; border-radius: 0; }
.nav-tabs .nav-link.active { color: #22c55e; border-bottom-color: #22c55e; background: transparent; }
.nav-tabs { border-bottom: 1px solid #e2e8f0; }
</style>