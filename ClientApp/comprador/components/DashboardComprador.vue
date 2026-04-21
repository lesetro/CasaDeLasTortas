<template>
  <div class="dashboard-comprador">

    <!-- ── ALERTA: pagos en revisión ── -->
    <div v-if="pagosEnRevision > 0"
         class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
         style="background:linear-gradient(135deg,#fff7ed,#ffedd5); border-left:4px solid #f97316 !important;">
      <i class="fas fa-clock text-warning fs-4"></i>
      <div class="flex-grow-1">
        <strong>Tenés {{ pagosEnRevision }} comprobante{{ pagosEnRevision > 1 ? 's' : '' }} en revisión</strong>
        <div class="text-muted small">El admin verificará tu pago en 1 día hábil.</div>
      </div>
      <button class="btn btn-sm btn-warning fw-semibold"
              @click="$emit('cambiar-vista', 'historial')">
        Ver pedidos
      </button>
    </div>

    <!-- ── ALERTA: pendientes sin pagar ── -->
    <div v-if="ordenesSinPagar > 0"
         class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
         style="background:#fef2f2; border-left:4px solid #ef4444 !important;">
      <i class="fas fa-exclamation-circle text-danger fs-4"></i>
      <div class="flex-grow-1">
        <strong>Tenés {{ ordenesSinPagar }} orden{{ ordenesSinPagar > 1 ? 'es' : '' }} sin pagar</strong>
        <div class="text-muted small">Subí el comprobante para que podamos procesar tu pedido.</div>
      </div>
      <button class="btn btn-sm btn-danger fw-semibold"
              @click="$emit('cambiar-vista', 'historial')">
        Pagar ahora
      </button>
    </div>

    <!-- ── KPIs ── -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#3b82f6,#2563eb);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 fw-semibold text-uppercase" style="font-size:.7rem;">Total compras</div>
                <div class="fs-2 fw-bold lh-1 mt-1">{{ stats.totalCompras }}</div>
              </div>
              <i class="fas fa-shopping-bag fa-2x opacity-30"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#22c55e,#16a34a);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 fw-semibold text-uppercase" style="font-size:.7rem;">Total gastado</div>
                <div class="fs-5 fw-bold lh-1 mt-1">{{ formatMoneda(stats.totalGastado) }}</div>
              </div>
              <i class="fas fa-dollar-sign fa-2x opacity-30"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-dark h-100"
             style="background:linear-gradient(135deg,#fef08a,#fde047);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 fw-semibold text-uppercase" style="font-size:.7rem;">Último mes</div>
                <div class="fs-5 fw-bold lh-1 mt-1">{{ formatMoneda(stats.gastoUltimoMes) }}</div>
              </div>
              <i class="fas fa-calendar-alt fa-2x opacity-30"></i>
            </div>
            <div class="small mt-1 opacity-60">{{ stats.comprasUltimoMes }} compras</div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#a855f7,#7c3aed);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 fw-semibold text-uppercase" style="font-size:.7rem;">Promedio</div>
                <div class="fs-5 fw-bold lh-1 mt-1">{{ formatMoneda(stats.compraPromedio) }}</div>
              </div>
              <i class="fas fa-chart-pie fa-2x opacity-30"></i>
            </div>
            <div class="small mt-1 opacity-60">por pedido</div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-4">

      <!-- ── Pedidos recientes ── -->
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-0 pt-4 pb-2 px-4">
            <div class="d-flex justify-content-between align-items-center">
              <h6 class="fw-bold mb-0">
                <i class="fas fa-history text-primary me-2"></i>Pedidos recientes
              </h6>
              <button class="btn btn-sm btn-outline-primary"
                      @click="$emit('cambiar-vista', 'historial')">
                Ver todos
              </button>
            </div>
          </div>
          <div class="card-body px-4 pb-4 pt-2">

            <div v-if="loading" class="text-center py-4">
              <div class="spinner-border text-primary"></div>
            </div>

            <div v-else-if="pedidosRecientes.length === 0" class="text-center py-4 text-muted">
              <i class="fas fa-shopping-bag fa-2x mb-2 d-block opacity-30"></i>
              <p class="mb-2">No tenés pedidos aún</p>
              <button class="btn btn-sm btn-primary" @click="$emit('cambiar-vista', 'catalogo')">
                <i class="fas fa-birthday-cake me-1"></i>Explorar tortas
              </button>
            </div>

            <div v-else class="table-responsive">
              <table class="table mb-0" style="font-size:.875rem;">
                <thead>
                  <tr>
                    <th class="border-0 text-muted fw-semibold pb-2" style="font-size:.72rem;">Orden</th>
                    <th class="border-0 text-muted fw-semibold pb-2" style="font-size:.72rem;">Fecha</th>
                    <th class="border-0 text-muted fw-semibold pb-2" style="font-size:.72rem;">Total</th>
                    <th class="border-0 text-muted fw-semibold pb-2 text-center" style="font-size:.72rem;">Estado</th>
                    <th class="border-0 text-muted fw-semibold pb-2 text-center" style="font-size:.72rem;">Pago</th>
                    <th class="border-0 pb-2"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="pedido in pedidosRecientes" :key="pedido.ventaId"
                      class="border-bottom" style="border-color:#f1f5f9!important;">
                    <td class="border-0 py-3">
                      <strong class="small">#{{ pedido.numeroOrden }}</strong>
                    </td>
                    <td class="border-0 py-3">
                      <div class="small">{{ formatFecha(pedido.fechaVenta) }}</div>
                      <div class="text-muted" style="font-size:.7rem;">{{ formatHora(pedido.fechaVenta) }}</div>
                    </td>
                    <td class="border-0 py-3 fw-bold text-success small">
                      {{ formatMoneda(pedido.total) }}
                    </td>
                    <td class="border-0 py-3 text-center">
                      <span class="badge rounded-pill" style="font-size:.68rem;"
                            :class="getBadgeClaseVenta(pedido.estado)">
                        {{ getEstadoVentaTexto(pedido.estado) }}
                      </span>
                    </td>
                    <td class="border-0 py-3 text-center">
                      <!-- Estado del pago del pedido -->
                      <span v-if="getPagoEstado(pedido)" class="badge rounded-pill" style="font-size:.68rem;"
                            :class="getBadgePago(getPagoEstado(pedido))">
                        {{ getLabelPago(getPagoEstado(pedido)) }}
                      </span>
                      <span v-else-if="pedido.estado === 'Pendiente'"
                            class="badge bg-warning text-dark rounded-pill" style="font-size:.68rem;">
                        Sin pagar
                      </span>
                      <span v-else class="text-muted small">—</span>
                    </td>
                    <td class="border-0 py-3 text-center">
                      <button class="btn btn-sm btn-outline-primary"
                              @click="$emit('ver-detalle', pedido.ventaId)"
                              title="Ver detalle">
                        <i class="fas fa-eye"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Panel derecho ── -->
      <div class="col-lg-4">

        <!-- Acciones rápidas -->
        <div class="card border-0 shadow-sm rounded-3 mb-4">
          <div class="card-header bg-white border-0 pt-3 pb-2 px-4">
            <h6 class="fw-bold mb-0 small">
              <i class="fas fa-bolt text-warning me-2"></i>Acciones rápidas
            </h6>
          </div>
          <div class="card-body px-4 pb-4 pt-2 d-grid gap-2">
            <button class="btn btn-outline-primary text-start"
                    @click="$emit('cambiar-vista', 'catalogo')">
              <i class="fas fa-birthday-cake me-2"></i>Buscar tortas
            </button>
            <button class="btn btn-outline-success text-start"
                    @click="$emit('cambiar-vista', 'carrito')">
              <i class="fas fa-shopping-cart me-2"></i>Mi carrito
            </button>
            <button class="btn btn-outline-secondary text-start"
                    @click="$emit('cambiar-vista', 'historial')">
              <i class="fas fa-history me-2"></i>Mi historial
            </button>
          </div>
        </div>

        <!-- Flujo de compra explicado -->
        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-0 pt-3 pb-2 px-4">
            <h6 class="fw-bold mb-0 small">
              <i class="fas fa-info-circle text-primary me-2"></i>¿Cómo funciona?
            </h6>
          </div>
          <div class="card-body px-4 pb-4 pt-2">
            <div v-for="(paso, i) in pasosExplicacion" :key="i"
                 class="d-flex gap-3 mb-3"
                 :class="{ 'mb-0': i === pasosExplicacion.length - 1 }">
              <div class="rounded-circle d-flex align-items-center justify-content-center fw-bold text-white flex-shrink-0"
                   style="width:28px;height:28px;font-size:.75rem;"
                   :style="`background:${paso.color}`">
                {{ i + 1 }}
              </div>
              <div>
                <div class="fw-semibold small">{{ paso.titulo }}</div>
                <div class="text-muted" style="font-size:.75rem;">{{ paso.desc }}</div>
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

const emit = defineEmits(['cambiar-vista', 'ver-detalle'])

const loading         = ref(true)
const pedidosRecientes = ref([])
const stats = ref({
  totalCompras: 0, totalGastado: 0, compraPromedio: 0,
  comprasUltimoMes: 0, gastoUltimoMes: 0, fechaUltimaCompra: null,
})

// ── Contadores de alertas ─────────────────────────────────────────
const pagosEnRevision = computed(() =>
  pedidosRecientes.value.filter(p =>
    p.pagos?.some(pg => pg.estado === 'EnRevision')
  ).length
)
const ordenesSinPagar = computed(() =>
  pedidosRecientes.value.filter(p =>
    p.estado === 'Pendiente' && !(p.pagos?.length)
  ).length
)

// ── Helpers de pago ──────────────────────────────────────────────
function getPagoEstado(pedido) {
  if (!pedido.pagos?.length) return null
  // priorizar el más reciente
  return pedido.pagos[pedido.pagos.length - 1]?.estado ?? null
}
function getBadgePago(estado) {
  return {
    Completado: 'bg-success',
    Pendiente:  'bg-warning text-dark',
    Cancelado:  'bg-danger',
  }[estado] ?? 'bg-secondary'
}
function getLabelPago(estado) {
  return { Completado: 'Aprobado ✓', Pendiente: 'En revisión ⏳', Cancelado: 'Rechazado ✗' }[estado] ?? estado
}

// ── Pasos explicación ────────────────────────────────────────────
const pasosExplicacion = [
  { titulo: 'Agregás al carrito',   desc: 'Elegís tus tortas favoritas.',                     color: '#3b82f6' },
  { titulo: 'Transferís el total',  desc: 'A la cuenta de la plataforma.',                    color: '#f59e0b' },
  { titulo: 'Subís comprobante',    desc: 'Foto o PDF de la transferencia.',                  color: '#a855f7' },
  { titulo: 'Admin verifica',       desc: 'Aprobamos el pago en 1 día hábil.',               color: '#22c55e' },
  { titulo: 'Retirás en el local',  desc: 'Cuando el vendedor confirme que está listo.',      color: '#f97316' },
]

// ── Carga de datos ───────────────────────────────────────────────
async function cargarDashboard() {
  try {
    loading.value = true
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const compradorId = user.compradorId
    if (!compradorId) { console.warn('Sin compradorId'); return }

    const [historialData, estadisticasData] = await Promise.all([
      fetchWithAuth(`/api/CompradorApi/${compradorId}/historial?pagina=1&registrosPorPagina=8`),
      fetchWithAuth(`/api/CompradorApi/${compradorId}/estadisticas`),
    ])

    pedidosRecientes.value = historialData.data || []
    stats.value = {
      totalCompras:      estadisticasData.totalCompras      ?? 0,
      totalGastado:      estadisticasData.totalGastado      ?? 0,
      compraPromedio:    estadisticasData.compraPromedio    ?? 0,
      comprasUltimoMes:  estadisticasData.comprasUltimoMes  ?? 0,
      gastoUltimoMes:    estadisticasData.gastoUltimoMes    ?? 0,
      fechaUltimaCompra: estadisticasData.fechaUltimaCompra ?? null,
    }
  } catch (err) {
    console.error('Error cargando dashboard:', err)
  } finally {
    loading.value = false
  }
}

onMounted(cargarDashboard)
</script>

<style scoped>
.kpi-card { border: none; border-radius: 14px; box-shadow: 0 2px 12px rgba(0,0,0,.08); transition: transform .18s; }
.kpi-card:hover { transform: translateY(-2px); }
</style>