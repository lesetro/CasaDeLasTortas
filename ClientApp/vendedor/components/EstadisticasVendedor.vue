<template>
  <div class="estadisticas-vendedor">

    <!-- ── FILA 1: KPIs ── -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#3b82f6,#2563eb);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 text-uppercase fw-semibold" style="font-size:.7rem;">Ventas del mes</div>
                <div class="fs-2 fw-bold lh-1 mt-1">{{ stats.ventasMes }}</div>
              </div>
              <i class="fas fa-shopping-cart fa-2x opacity-25"></i>
            </div>
            <div class="mt-2 small opacity-75">{{ formatMoneda(stats.ingresosMes) }} en ingresos</div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#22c55e,#16a34a);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 text-uppercase fw-semibold" style="font-size:.7rem;">Tortas activas</div>
                <div class="fs-2 fw-bold lh-1 mt-1">{{ stats.totalTortasActivas }}</div>
              </div>
              <i class="fas fa-birthday-cake fa-2x opacity-25"></i>
            </div>
            <div class="mt-2 small opacity-75">{{ stats.totalTortas }} en total</div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-dark h-100"
             style="background:linear-gradient(135deg,#fde047,#facc15);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 text-uppercase fw-semibold" style="font-size:.7rem;">Pendientes</div>
                <div class="fs-2 fw-bold lh-1 mt-1">{{ stats.pedidosPendientes }}</div>
              </div>
              <i class="fas fa-clock fa-2x opacity-25"></i>
            </div>
            <div class="mt-2 small opacity-60">Requieren atención</div>
          </div>
        </div>
      </div>

      <div class="col-6 col-md-3">
        <div class="kpi-card card text-white h-100"
             style="background:linear-gradient(135deg,#a855f7,#7c3aed);">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="small opacity-75 text-uppercase fw-semibold" style="font-size:.7rem;">Ingresos totales</div>
                <div class="fs-5 fw-bold lh-1 mt-1">{{ formatMoneda(stats.ingresosTotales) }}</div>
              </div>
              <i class="fas fa-chart-line fa-2x opacity-25"></i>
            </div>
            <div class="mt-2 small opacity-75">{{ stats.totalVentas }} ventas</div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── FILA 2: KPIs de comisiones ── -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-3">
        <div class="kpi-card card h-100 p-3" style="border-left:4px solid #22c55e;">
          <div class="text-muted small mb-1">💸 Ya cobrado</div>
          <div class="fs-5 fw-bold text-success">{{ formatMoneda(comisiones.cobrado) }}</div>
          <div class="text-muted" style="font-size:.72rem;">Transferido a tu cuenta</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="kpi-card card h-100 p-3" style="border-left:4px solid #f59e0b;">
          <div class="text-muted small mb-1">⏳ Por cobrar</div>
          <div class="fs-5 fw-bold text-warning">{{ formatMoneda(comisiones.pendiente) }}</div>
          <div class="text-muted" style="font-size:.72rem;">Pendiente de liberación</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="kpi-card card h-100 p-3" style="border-left:4px solid #ef4444;">
          <div class="text-muted small mb-1">🏷️ Comisiones pagadas</div>
          <div class="fs-5 fw-bold text-danger">{{ formatMoneda(comisiones.totalComisiones) }}</div>
          <div class="text-muted" style="font-size:.72rem;">{{ comisiones.porcentaje }}% de tus ventas</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="kpi-card card h-100 p-3" style="border-left:4px solid #3b82f6;">
          <div class="text-muted small mb-1">📈 Neto este mes</div>
          <div class="fs-5 fw-bold text-primary">{{ formatMoneda(comisiones.netoMes) }}</div>
          <div class="text-muted" style="font-size:.72rem;">Después de comisiones</div>
        </div>
      </div>
    </div>

    <!-- Loading / error -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>
    <div v-else-if="error" class="alert alert-warning border-0 rounded-3">
      <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
    </div>

    <div v-else class="row g-4">

      <!-- Top Tortas -->
      <div class="col-md-6">
        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-0 pt-4 pb-2 px-4">
            <h6 class="fw-bold mb-0">
              <i class="fas fa-trophy text-warning me-2"></i>Top Tortas Más Vendidas
            </h6>
          </div>
          <div class="card-body px-4 pb-4 pt-2">
            <div v-if="topTortas.length">
              <div v-for="(torta, i) in topTortas" :key="torta.id"
                   class="d-flex align-items-center gap-3 py-2"
                   :class="{ 'border-bottom': i < topTortas.length - 1 }">
                <div class="rounded-circle d-flex align-items-center justify-content-center fw-bold text-white flex-shrink-0"
                     style="width:28px;height:28px;font-size:.78rem;"
                     :style="`background:${i===0?'#f59e0b':i===1?'#94a3b8':i===2?'#a16207':'#e2e8f0'};color:${i<3?'#fff':'#64748b'}`">
                  {{ i + 1 }}
                </div>
                <img v-if="torta.imagen" :src="torta.imagen"
                     class="rounded flex-shrink-0" style="width:40px;height:40px;object-fit:cover;" />
                <div class="flex-grow-1">
                  <div class="fw-semibold small">{{ torta.nombre }}</div>
                  <div class="text-muted" style="font-size:.72rem;">
                    <i class="fas fa-shopping-cart me-1"></i>{{ torta.totalVentas }} ventas
                  </div>
                </div>
                <div class="text-end">
                  <div class="fw-bold text-success small">{{ formatMoneda(torta.precio) }}</div>
                </div>
              </div>
            </div>
            <div v-else class="text-center py-4 text-muted">
              <i class="fas fa-birthday-cake fa-2x mb-2 d-block opacity-30"></i>
              <p class="small mb-0">No hay ventas registradas aún</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Actividad Reciente -->
      <div class="col-md-6">
        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-0 pt-4 pb-2 px-4">
            <h6 class="fw-bold mb-0">
              <i class="fas fa-clock text-muted me-2"></i>Actividad Reciente
            </h6>
          </div>
          <div class="card-body px-4 pb-4 pt-2" style="max-height:360px;overflow-y:auto;">
            <div v-if="actividad.length">
              <div v-for="(item, i) in actividad" :key="item.id"
                   class="d-flex align-items-start gap-3 py-2"
                   :class="{ 'border-bottom': i < actividad.length - 1 }">
                <div class="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0"
                     style="width:32px;height:32px;background:#f8fafc;">
                  <i :class="item.icon || 'fas fa-circle'" style="font-size:.8rem; color:#94a3b8;"></i>
                </div>
                <div class="flex-grow-1">
                  <div class="fw-semibold small">{{ item.titulo }}</div>
                  <div class="text-muted small">{{ item.descripcion }}</div>
                  <div class="text-muted" style="font-size:.7rem;">
                    <i class="fas fa-clock me-1"></i>{{ formatFechaHora(item.fecha) }}
                  </div>
                </div>
                <span class="badge bg-light text-muted rounded-pill" style="font-size:.68rem;">
                  #{{ item.numeroOrden }}
                </span>
              </div>
            </div>
            <div v-else class="text-center py-4 text-muted">
              <i class="fas fa-inbox fa-2x mb-2 d-block opacity-30"></i>
              <p class="small mb-0">Sin actividad reciente</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Panel de comisiones detalle -->
      <div class="col-12">
        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-0 pt-4 pb-2 px-4">
            <h6 class="fw-bold mb-0">
              <i class="fas fa-percentage text-danger me-2"></i>Detalle de Comisiones
            </h6>
          </div>
          <div class="card-body px-4 pb-4 pt-3">
            <div class="row g-4 align-items-center">
              <div class="col-md-6">
                <div class="p-3 rounded-3" style="background:#fef2f2; border:1px solid #fecaca;">
                  <div class="fw-semibold small mb-2">¿Cómo se calcula?</div>
                  <div class="small text-muted mb-2">
                    Por cada venta, la plataforma retiene el
                    <strong class="text-danger">{{ comisiones.porcentaje }}%</strong>
                    de tu subtotal al momento de liberar los fondos.
                  </div>
                  <div class="p-2 rounded" style="background:#fff; border:1px dashed #f87171; font-size:.82rem;">
                    Venta de $100.000<br/>
                    − Comisión {{ comisiones.porcentaje }}% = <span class="text-danger">${{ (100000 * comisiones.porcentaje / 100).toLocaleString('es-AR') }}</span><br/>
                    <strong>= Cobrado: <span class="text-success">${{ (100000 * (1 - comisiones.porcentaje / 100)).toLocaleString('es-AR') }}</span></strong>
                  </div>
                </div>
              </div>
              <div class="col-md-6">
                <div class="d-flex flex-column gap-3">
                  <div class="d-flex justify-content-between p-2 rounded" style="background:#f0fdf4;">
                    <span class="small text-muted">Total generado por tus ventas</span>
                    <strong class="small">{{ formatMoneda(comisiones.totalVentas) }}</strong>
                  </div>
                  <div class="d-flex justify-content-between p-2 rounded" style="background:#fef2f2;">
                    <span class="small text-muted">Total en comisiones</span>
                    <strong class="small text-danger">−{{ formatMoneda(comisiones.totalComisiones) }}</strong>
                  </div>
                  <div class="d-flex justify-content-between p-2 rounded" style="background:#eff6ff; border:2px solid #bfdbfe;">
                    <span class="small fw-semibold">Neto (lo que te llega)</span>
                    <strong class="text-primary">{{ formatMoneda(comisiones.totalVentas - comisiones.totalComisiones) }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchWithAuth, formatMoneda, formatFechaHora } from '@shared/apiUtils.js'

const stats = ref({
  totalTortas: 0, totalTortasActivas: 0, totalVentas: 0,
  ventasMes: 0, ingresosTotales: 0, ingresosMes: 0, pedidosPendientes: 0,
})
const comisiones = ref({
  cobrado: 0, pendiente: 0, totalComisiones: 0, netoMes: 0, porcentaje: 10, totalVentas: 0,
})
const topTortas = ref([])
const actividad = ref([])
const loading   = ref(true)
const error     = ref(null)

async function cargarEstadisticas() {
  try {
    loading.value = true
    error.value   = null
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const vendedorId = user.vendedorId
    if (!vendedorId) { error.value = 'No se encontró el perfil de vendedor.'; return }

    const [statsData, topData, actData, libData] = await Promise.all([
      fetchWithAuth(`/api/VendedorApi/${vendedorId}/estadisticas`),
      fetchWithAuth(`/api/VendedorApi/${vendedorId}/tortas-mas-vendidas?limit=5`),
      fetchWithAuth(`/api/VendedorApi/${vendedorId}/actividad-reciente?limit=10`),
      // Datos de liberaciones para KPIs de comisiones
      fetchWithAuth(`/api/LiberacionApi/vendedor/${vendedorId}?pagina=1&registrosPorPagina=100`).catch(() => ({ data: [] })),
    ])

    stats.value = {
      totalTortas:        statsData.totalTortas        ?? 0,
      totalTortasActivas: statsData.totalTortasActivas ?? 0,
      totalVentas:        statsData.totalVentas        ?? 0,
      ventasMes:          statsData.ventasMes          ?? 0,
      ingresosTotales:    statsData.ingresosBrutos     ?? statsData.ingresosTotales    ?? 0,
      ingresosMes:        statsData.ingresosBrutosMes  ?? statsData.ingresosMes        ?? 0,
      pedidosPendientes:  statsData.pedidosPendientes  ?? 0,
    }
    topTortas.value = Array.isArray(topData) ? topData : []
    actividad.value = Array.isArray(actData) ? actData : []

    // Calcular KPIs de comisiones desde liberaciones
    const libs = libData.data ?? libData ?? []
    const porcentaje = libData.porcentajeComision ?? statsData.porcentajeComision ?? 10
    const ahora = new Date()
    const esCobrado = l => l.estado === 'Transferido' || l.estado === 'Confirmado'
    const cobrado         = libs.filter(esCobrado).reduce((s, l) => s + (l.montoNeto ?? 0), 0)
    const pendiente       = libs.filter(l => l.estado === 'Pendiente' || l.estado === 'ListoParaLiberar').reduce((s, l) => s + (l.montoNeto ?? 0), 0)
    const totalComisiones = libs.filter(esCobrado).reduce((s, l) => s + (l.comision ?? 0), 0)
    const netoMes         = libs.filter(l => esCobrado(l) && new Date(l.fechaLiberacion).getMonth() === ahora.getMonth()).reduce((s, l) => s + (l.montoNeto ?? 0), 0)
    const totalVentas     = libs.reduce((s, l) => s + (l.montoBruto ?? 0), 0)
    comisiones.value = { cobrado, pendiente, totalComisiones, netoMes, porcentaje, totalVentas }

  } catch (err) {
    error.value = 'No se pudieron cargar las estadísticas.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(cargarEstadisticas)
defineExpose({ cargarEstadisticas })
</script>

<style scoped>
.kpi-card { border: none; border-radius: 14px; box-shadow: 0 2px 12px rgba(0,0,0,.07); transition: transform .18s; }
.kpi-card:hover { transform: translateY(-2px); }
</style>