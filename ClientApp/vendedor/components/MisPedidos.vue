<template>
  <div class="mis-pedidos">
    <div class="card border-0 shadow-sm rounded-3">

      <!-- Header -->
      <div class="card-header bg-white border-0 d-flex justify-content-between align-items-center flex-wrap gap-2 px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-shopping-bag text-success me-2"></i>Mis Pedidos
          <span class="badge bg-success ms-2 rounded-pill">{{ totalRegistros }}</span>
        </h5>
        <div class="d-flex gap-2 flex-wrap">
          <select v-model="filtroEstado" class="form-select form-select-sm" style="width:auto;"
                  @change="cargarPedidos(1)">
            <option value="">Todos los estados</option>
            <option value="Pendiente">⏳ Pendiente</option>
            <option value="Confirmado">✅ Confirmado</option>
            <option value="EnPreparacion">👨‍🍳 En Preparación</option>
            <option value="Listo">📦 Listo para Retirar</option>
            <option value="Entregado">🎉 Entregado</option>
            <option value="Cancelado">❌ Cancelado</option>
          </select>
          <button class="btn btn-outline-success btn-sm" @click="cargarPedidos(paginaActual)"
                  :disabled="loading">
            <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i>
          </button>
        </div>
      </div>

      <div class="card-body px-4 pb-4 pt-2">

        <!-- Loading -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-success mb-3"></div>
          <p class="text-muted small">Cargando pedidos...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="alert alert-warning border-0 rounded-3">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>

        <!-- Vacío -->
        <div v-else-if="pedidos.length === 0" class="text-center py-5 text-muted">
          <i class="fas fa-inbox fa-3x mb-3 d-block opacity-30"></i>
          <h6 class="fw-semibold">No hay pedidos{{ filtroEstado ? ' con ese estado' : '' }}</h6>
          <p class="small">Los pedidos de tus clientes aparecerán acá</p>
        </div>

        <!-- Tabla -->
        <div v-else>
          <div class="table-responsive">
            <table class="table mb-0" style="font-size:.875rem;">
              <thead>
                <tr>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;"># Orden</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Producto</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Cliente</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Fecha</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Pago</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Estado</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Liberación</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-end" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Subtotal</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="pedido in pedidos" :key="pedido.detalleId"
                    class="border-bottom" style="border-color:#f1f5f9!important;"
                    :class="{ 'bg-warning-subtle': pedido.estadoPago === 'Pendiente' }">

                  <!-- Orden -->
                  <td class="border-0 py-3">
                    <strong class="small">#{{ pedido.numeroOrden }}</strong>
                  </td>

                  <!-- Producto -->
                  <td class="border-0 py-3">
                    <div class="fw-semibold small">{{ pedido.nombreTorta }}</div>
                    <div class="text-muted" style="font-size:.72rem;">× {{ pedido.cantidad }}</div>
                    <div v-if="pedido.notasPersonalizacion"
                         class="text-muted fst-italic" style="font-size:.7rem;">
                      "{{ pedido.notasPersonalizacion }}"
                    </div>
                  </td>

                  <!-- Cliente -->
                  <td class="border-0 py-3">
                    <div class="fw-semibold small">{{ pedido.compradorNombre || pedido.nombreComprador }}</div>
                    <div class="text-muted" style="font-size:.72rem;">
                      <i class="fas fa-map-marker-alt me-1"></i>{{ pedido.direccionEntrega }}
                    </div>
                  </td>

                  <!-- Fecha -->
                  <td class="border-0 py-3">
                    <div class="small">{{ formatFecha(pedido.fecha || pedido.fechaVenta) }}</div>
                    <div class="text-muted" style="font-size:.7rem;">{{ formatHora(pedido.fecha || pedido.fechaVenta) }}</div>
                  </td>

                  <!-- Estado pago -->
                  <td class="border-0 py-3 text-center">
                    <span class="badge rounded-pill" style="font-size:.67rem;"
                          :class="getBadgePago(pedido.estadoPago)">
                      {{ getLabelPago(pedido.estadoPago) }}
                    </span>
                  </td>

                  <!-- Estado pedido -->
                  <td class="border-0 py-3 text-center">
                    <span class="badge rounded-pill" style="font-size:.67rem;"
                          :class="getBadgeEstado(pedido.estado)">
                      {{ pedido.estadoLabel || pedido.estado }}
                    </span>
                  </td>

                  <!-- ── NUEVA COLUMNA: Estado liberación ── -->
                  <td class="border-0 py-3 text-center">
                    <template v-if="pedido.estado === 'Entregado' || pedido.estado === 'Entregada'">
                      <span v-if="pedido.liberacionEstado === 'Confirmado'"
                            class="badge bg-success rounded-pill" style="font-size:.67rem;">
                        💸 Cobrado
                      </span>
                      <span v-else-if="pedido.liberacionEstado === 'Transferido'"
                            class="badge bg-info rounded-pill" style="font-size:.67rem;">
                        🔄 Transferido
                      </span>
                      <span v-else-if="pedido.liberacionEstado === 'ListoParaLiberar'"
                            class="badge bg-warning text-dark rounded-pill" style="font-size:.67rem;">
                        🔜 En proceso
                      </span>
                      <span v-else
                            class="badge bg-light text-muted rounded-pill" style="font-size:.67rem;">
                        ⏳ Pendiente
                      </span>
                    </template>
                    <span v-else class="text-muted" style="font-size:.75rem;">—</span>
                  </td>

                  <!-- Subtotal -->
                  <td class="border-0 py-3 text-end fw-bold text-success small">
                    {{ formatMoneda(pedido.subtotal) }}
                    <div v-if="pedido.liberacionEstado === 'Confirmado'" class="text-muted fw-normal" style="font-size:.7rem;">
                      Cobrado: {{ formatMoneda(pedido.montoLiberado) }}
                    </div>
                  </td>

                  <!-- Acciones -->
                  <td class="border-0 py-3 text-center">
                    <div class="d-flex gap-1 justify-content-center flex-wrap">
                      <button v-for="accion in pedido.accionesDisponibles" :key="accion"
                              class="btn btn-sm"
                              :class="getBtnAccion(accion)"
                              :disabled="cambiando === pedido.detalleId"
                              @click="cambiarEstado(pedido, accion)"
                              :title="getLabelAccion(accion)">
                        <span v-if="cambiando === pedido.detalleId"
                              class="spinner-border spinner-border-sm"></span>
                        <i v-else :class="getIconoAccion(accion)"></i>
                        <span class="d-none d-xl-inline ms-1">{{ getLabelAccion(accion) }}</span>
                      </button>
                    </div>
                  </td>

                </tr>
              </tbody>
            </table>
          </div>

          <!-- Leyenda liberación -->
          <div class="mt-3 p-3 rounded-3 d-flex flex-wrap gap-3 align-items-center"
               style="background:#f8fafc; font-size:.78rem;">
            <span class="text-muted fw-semibold">Liberación de fondos:</span>
            <span><span class="badge bg-light text-muted rounded-pill me-1">—</span>Pedido aún no entregado</span>
            <span><span class="badge bg-warning text-dark rounded-pill me-1">⏳</span>Entregado, esperando transferencia del admin</span>
            <span><span class="badge bg-success rounded-pill me-1">💸</span>Fondos acreditados en tu cuenta</span>
          </div>

          <!-- Paginación -->
          <nav v-if="totalPaginas > 1" class="mt-4 d-flex justify-content-center">
            <ul class="pagination pagination-sm mb-0 gap-1">
              <li class="page-item" :class="{ disabled: paginaActual === 1 }">
                <a class="page-link rounded" href="#" @click.prevent="cargarPedidos(paginaActual - 1)">
                  <i class="fas fa-chevron-left"></i>
                </a>
              </li>
              <li v-for="p in paginasVisibles" :key="p" class="page-item"
                  :class="{ active: p === paginaActual }">
                <a class="page-link rounded" href="#" @click.prevent="cargarPedidos(p)">{{ p }}</a>
              </li>
              <li class="page-item" :class="{ disabled: paginaActual === totalPaginas }">
                <a class="page-link rounded" href="#" @click.prevent="cargarPedidos(paginaActual + 1)">
                  <i class="fas fa-chevron-right"></i>
                </a>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, formatMoneda, formatFecha, formatHora, mostrarToast } from '@shared/apiUtils.js'

// ── Estado ────────────────────────────────────────────────────────────
const pedidos        = ref([])
const loading        = ref(true)
const error          = ref(null)
const filtroEstado   = ref('')
const paginaActual   = ref(1)
const totalPaginas   = ref(0)
const totalRegistros = ref(0)
const tamanioPagina  = 10
const cambiando      = ref(null)

const paginasVisibles = computed(() => {
  const inicio = Math.max(1, paginaActual.value - 2)
  const fin    = Math.min(totalPaginas.value, inicio + 4)
  return Array.from({ length: fin - inicio + 1 }, (_, i) => inicio + i)
})

// ── Carga ─────────────────────────────────────────────────────────────
async function cargarPedidos(pagina = 1) {
  try {
    loading.value      = true
    error.value        = null
    paginaActual.value = pagina

    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const vendedorId = user.vendedorId
    if (!vendedorId) { error.value = 'No se encontró el perfil de vendedor.'; return }

    let url = `/api/VendedorApi/${vendedorId}/pedidos?pagina=${pagina}&registrosPorPagina=${tamanioPagina}`
    if (filtroEstado.value) url += `&estado=${filtroEstado.value}`

    const data = await fetchWithAuth(url)
    pedidos.value        = data.data           ?? []
    totalRegistros.value = data.totalRegistros ?? 0
    totalPaginas.value   = data.totalPaginas   ?? 0
  } catch (err) {
    error.value = 'No se pudieron cargar los pedidos.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

// ── Cambiar estado ────────────────────────────────────────────────────
async function cambiarEstado(pedido, nuevoEstado) {
  if (!confirm(`¿Confirmar: "${getLabelAccion(nuevoEstado)}" para #${pedido.numeroOrden}?`)) return
  try {
    cambiando.value = pedido.detalleId
    const resultado = await fetchWithAuth(
      `/api/DetalleVenta/${pedido.detalleId}/estado`,
      { method: 'PATCH', body: JSON.stringify({ estado: nuevoEstado }) }
    )
    pedido.estado              = resultado.estadoNuevo
    pedido.estadoLabel         = getEstadoLabel(resultado.estadoNuevo)
    pedido.accionesDisponibles = resultado.accionesDisponibles ?? []
    mostrarToast(`Estado: ${resultado.estadoNuevo}`, 'success')
  } catch {
    mostrarToast('Error al actualizar el estado', 'error')
  } finally {
    cambiando.value = null
  }
}

// ── Helpers ───────────────────────────────────────────────────────────
function getBadgePago(estado) {
  return {
    Pendiente:  'bg-secondary',
    EnRevision: 'bg-warning text-dark',
    Verificado: 'bg-info',
    Completado: 'bg-success',
    Rechazado:  'bg-danger',
    Cancelado:  'bg-danger',
  }[estado] ?? 'bg-light text-muted'
}
function getLabelPago(estado) {
  return {
    Pendiente:  '💳 Sin pagar',
    EnRevision: '🔍 En revisión',
    Verificado: '✅ Verificado',
    Completado: '💸 Completado',
    Rechazado:  '❌ Rechazado',
    Cancelado:  '❌ Cancelado',
    SinPago:    '—',
  }[estado] ?? (estado || '—')
}
function getBadgeEstado(estado) {
  return {
    Pendiente: 'bg-warning text-dark', Confirmado: 'bg-info',
    EnPreparacion: 'bg-primary', Listo: 'bg-success',
    Entregado: 'bg-success', Cancelado: 'bg-danger',
  }[estado] ?? 'bg-secondary'
}
function getBtnAccion(accion) {
  return {
    Confirmado: 'btn-outline-info', EnPreparacion: 'btn-outline-primary',
    Listo: 'btn-outline-success', Entregado: 'btn-outline-success', Cancelado: 'btn-outline-danger',
  }[accion] ?? 'btn-outline-secondary'
}
function getLabelAccion(accion) {
  return { Confirmado: 'Confirmar', EnPreparacion: 'Preparar', Listo: 'Marcar Listo', Entregado: 'Entregar', Cancelado: 'Cancelar' }[accion] ?? accion
}
function getIconoAccion(accion) {
  return { Confirmado: 'fas fa-check', EnPreparacion: 'fas fa-fire', Listo: 'fas fa-box', Entregado: 'fas fa-handshake', Cancelado: 'fas fa-times' }[accion] ?? 'fas fa-arrow-right'
}
function getEstadoLabel(estado) {
  return { Pendiente: '⏳ Pendiente', Confirmado: '✅ Confirmado', EnPreparacion: '👨‍🍳 En Preparación', Listo: '📦 Listo', Entregado: '🎉 Entregado', Cancelado: '❌ Cancelado' }[estado] ?? estado
}

onMounted(() => cargarPedidos(1))
defineExpose({ cargarPedidos })
</script>