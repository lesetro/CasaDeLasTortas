<template>
  <div class="historial-compras">
    <div class="card border-0 shadow-sm rounded-3">

      <!-- Header -->
      <div class="card-header bg-white border-0 d-flex flex-wrap justify-content-between align-items-center gap-2 px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-history text-primary me-2"></i>Mi Historial de Compras
          <span class="badge bg-primary ms-2 rounded-pill">{{ totalRegistros }}</span>
        </h5>
        <div class="d-flex gap-2 flex-wrap">
          <select v-model="filtroEstado" class="form-select form-select-sm" style="width:auto;"
                  @change="cargarVentas(1)">
            <option value="">Todos los estados</option>
            <option value="Pendiente">⏳ Pendiente de pago</option>
            <option value="Pagada">💳 Pago verificado</option>
            <option value="EnPreparacion">🍰 En preparación</option>
            <option value="ListaParaEnvio">✅ Lista para retirar</option>
            <option value="Entregada">📦 Entregada</option>
            <option value="Cancelada">❌ Cancelada</option>
          </select>
          <button class="btn btn-outline-secondary btn-sm" @click="cargarVentas(paginaActual)"
                  :disabled="loading">
            <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i>
          </button>
        </div>
      </div>

      <div class="card-body px-4 pb-4 pt-2">

        <!-- Loading -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary mb-3"></div>
          <p class="text-muted small">Cargando historial...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="alert alert-warning border-0 rounded-3">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
          <button class="btn btn-sm btn-outline-warning ms-2" @click="cargarVentas(1)">Reintentar</button>
        </div>

        <!-- Vacío -->
        <div v-else-if="ventas.length === 0" class="text-center py-5 text-muted">
          <i class="fas fa-receipt fa-3x mb-3 d-block opacity-30"></i>
          <h6 class="fw-semibold">No hay compras registradas</h6>
          <p class="small">{{ filtroEstado ? 'No hay compras con ese estado.' : 'Realizá tu primera compra desde el catálogo.' }}</p>
          <button class="btn btn-primary btn-sm mt-2" @click="$emit('cambiar-vista', 'catalogo')">
            <i class="fas fa-store me-2"></i>Ir al Catálogo
          </button>
        </div>

        <!-- Lista de ventas -->
        <div v-else>
          <div class="table-responsive">
            <table class="table mb-0" style="font-size:.875rem;">
              <thead>
                <tr>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Orden</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Fecha</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Productos</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-end" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Total</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Estado pedido</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Estado pago</th>
                  <th class="border-0 pb-3 text-center" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em; color:#94a3b8;">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="venta in ventas" :key="venta.ventaId"
                    class="border-bottom" style="border-color:#f1f5f9!important;">

                  <!-- Orden -->
                  <td class="border-0 py-3">
                    <strong class="small">#{{ venta.numeroOrden }}</strong>
                  </td>

                  <!-- Fecha -->
                  <td class="border-0 py-3">
                    <div class="small">{{ formatFecha(venta.fechaVenta) }}</div>
                    <div class="text-muted" style="font-size:.7rem;">{{ formatHora(venta.fechaVenta) }}</div>
                  </td>

                  <!-- Productos -->
                  <td class="border-0 py-3">
                    <div class="small">{{ venta.totalItems }} item{{ venta.totalItems !== 1 ? 's' : '' }}</div>
                    <div v-if="venta.productos?.[0]" class="text-muted" style="font-size:.72rem;">
                      {{ venta.productos[0].nombreTorta }}
                      <span v-if="venta.productos.length > 1" class="ms-1">+{{ venta.productos.length - 1 }} más</span>
                    </div>
                  </td>

                  <!-- Total -->
                  <td class="border-0 py-3 text-end fw-bold text-success small">
                    {{ formatMoneda(venta.total) }}
                  </td>

                  <!-- Estado venta -->
                  <td class="border-0 py-3 text-center">
                    <span class="badge rounded-pill" style="font-size:.68rem;"
                          :class="getBadgeClaseVenta(venta.estado)">
                      {{ getEstadoVentaTexto(venta.estado) }}
                    </span>
                  </td>

                  <!-- Estado pago — NUEVO FLUJO ── -->
                  <td class="border-0 py-3 text-center">
                    <template v-if="venta.pagos?.length">
                      <div v-for="pago in venta.pagos" :key="pago.pagoId">
                        <span class="badge rounded-pill" style="font-size:.65rem;"
                              :class="getBadgePago(pago.estado)">
                          {{ getLabelPago(pago.estado) }}
                        </span>
                      </div>
                    </template>
                    <template v-else-if="venta.estado === 'Pendiente'">
                      <span class="badge bg-warning text-dark rounded-pill" style="font-size:.65rem;">
                        Sin comprobante
                      </span>
                    </template>
                    <template v-else>
                      <span class="text-muted small">—</span>
                    </template>
                  </td>

                  <!-- Acciones -->
                  <td class="border-0 py-3 text-center">
                    <div class="d-flex gap-1 justify-content-center">
                      <!-- Ver detalle siempre -->
                      <button class="btn btn-sm btn-outline-primary" title="Ver detalle"
                              @click="$emit('ver-detalle', venta.ventaId)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <!-- Pagar si está pendiente y sin comprobante -->
                      <button v-if="venta.estado === 'Pendiente' && !venta.pagos?.length"
                              class="btn btn-sm btn-warning fw-semibold" title="Subir comprobante"
                              @click="$emit('ver-detalle', venta.ventaId)">
                        <i class="fas fa-upload me-1"></i>Pagar
                      </button>
                    </div>
                  </td>

                </tr>
              </tbody>
            </table>
          </div>

          <!-- Paginación -->
          <nav v-if="totalPaginas > 1" class="mt-4 d-flex justify-content-center">
            <ul class="pagination pagination-sm mb-0 gap-1">
              <li class="page-item" :class="{ disabled: paginaActual === 1 }">
                <a class="page-link rounded" href="#"
                   @click.prevent="cargarVentas(paginaActual - 1)">
                  <i class="fas fa-chevron-left"></i>
                </a>
              </li>
              <li v-for="p in paginasVisibles" :key="p" class="page-item"
                  :class="{ active: p === paginaActual }">
                <a class="page-link rounded" href="#"
                   @click.prevent="cargarVentas(p)">{{ p }}</a>
              </li>
              <li class="page-item" :class="{ disabled: paginaActual === totalPaginas }">
                <a class="page-link rounded" href="#"
                   @click.prevent="cargarVentas(paginaActual + 1)">
                  <i class="fas fa-chevron-right"></i>
                </a>
              </li>
            </ul>
          </nav>

          <!-- Resumen estado de pagos -->
          <div v-if="resumenPagos.conPendientes > 0 || resumenPagos.sinPagar > 0"
               class="mt-4 p-3 rounded-3" style="background:#f8fafc;">
            <div class="row text-center g-2">
              <div v-if="resumenPagos.sinPagar > 0" class="col">
                <div class="fw-bold text-warning">{{ resumenPagos.sinPagar }}</div>
                <div class="text-muted small">Sin comprobante</div>
              </div>
              <div v-if="resumenPagos.conPendientes > 0" class="col">
                <div class="fw-bold text-primary">{{ resumenPagos.conPendientes }}</div>
                <div class="text-muted small">En revisión</div>
              </div>
              <div v-if="resumenPagos.aprobados > 0" class="col">
                <div class="fw-bold text-success">{{ resumenPagos.aprobados }}</div>
                <div class="text-muted small">Aprobados</div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  fetchWithAuth, formatMoneda, formatFecha, formatHora,
  getEstadoVentaTexto, getBadgeClaseVenta
} from '@shared/apiUtils.js'

defineEmits(['cambiar-vista', 'ver-detalle'])

// ── Estado ──────────────────────────────────────────────────────────
const ventas          = ref([])
const loading         = ref(true)
const error           = ref(null)
const paginaActual    = ref(1)
const totalPaginas    = ref(0)
const totalRegistros  = ref(0)
const tamanioPagina   = 10
const filtroEstado    = ref('')

// ── Computed ─────────────────────────────────────────────────────────
const paginasVisibles = computed(() => {
  const inicio = Math.max(1, paginaActual.value - 2)
  const fin    = Math.min(totalPaginas.value, inicio + 4)
  return Array.from({ length: fin - inicio + 1 }, (_, i) => inicio + i)
})

const resumenPagos = computed(() => {
  let sinPagar = 0, conPendientes = 0, aprobados = 0
  ventas.value.forEach(v => {
    if (!v.pagos?.length && v.estado === 'Pendiente') { sinPagar++ }
    else if (v.pagos?.some(p => p.estado === 'Pendiente')) { conPendientes++ }
    else if (v.pagos?.some(p => p.estado === 'Completado')) { aprobados++ }
  })
  return { sinPagar, conPendientes, aprobados }
})

// ── Helpers pago ─────────────────────────────────────────────────────
function getBadgePago(estado) {
  return {
    Completado: 'bg-success',
    Pendiente:  'bg-warning text-dark',
    Cancelado:  'bg-danger',
  }[estado] ?? 'bg-secondary'
}
function getLabelPago(estado) {
  return {
    Completado: 'Aprobado ✓',
    Pendiente:  'En revisión ⏳',
    Cancelado:  'Rechazado ✗',
  }[estado] ?? estado
}

// ── Carga de datos ────────────────────────────────────────────────────
async function cargarVentas(pagina = 1) {
  try {
    loading.value = true
    error.value   = null
    paginaActual.value = pagina

    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const compradorId = user.compradorId
    if (!compradorId) { error.value = 'No se encontró el perfil de comprador.'; return }

    const url  = `/api/CompradorApi/${compradorId}/historial?pagina=${pagina}&registrosPorPagina=${tamanioPagina}`
    const data = await fetchWithAuth(url)
    let lista  = data.data || []

    // Filtro en cliente cuando el endpoint no soporta filtro por estado
    if (filtroEstado.value) {
      lista = lista.filter(v => v.estado === filtroEstado.value)
    }

    ventas.value         = lista
    totalRegistros.value = data.totalRegistros ?? lista.length
    totalPaginas.value   = data.totalPaginas   ?? Math.ceil(totalRegistros.value / tamanioPagina)
  } catch (err) {
    error.value = 'No se pudo cargar el historial.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => cargarVentas(1))
</script>