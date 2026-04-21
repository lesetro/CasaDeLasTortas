<template>
  <div class="mis-liberaciones">
    <div class="card border-0 shadow-sm rounded-3">

      <div class="card-header bg-white border-0 d-flex justify-content-between align-items-center px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-hand-holding-usd text-success me-2"></i>Mis Cobros
          <span class="badge bg-success ms-2 rounded-pill">{{ totalRegistros }}</span>
        </h5>
        <button class="btn btn-outline-success btn-sm" @click="cargarLiberaciones(1)" :disabled="loading">
          <i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i>
        </button>
      </div>

      <div class="card-body px-4 pb-4 pt-2">

        <!-- KPIs -->
        <div class="row g-3 mb-4">
          <div class="col-6 col-md-3">
            <div class="p-3 rounded-3 text-center" style="background:#f0fdf4;">
              <div class="text-muted small mb-1">💸 Total cobrado</div>
              <div class="fw-bold text-success fs-5">{{ formatMoneda(kpis.totalCobrado) }}</div>
            </div>
          </div>
          <div class="col-6 col-md-3">
            <div class="p-3 rounded-3 text-center" style="background:#f8fafc;">
              <div class="text-muted small mb-1">📅 Este mes</div>
              <div class="fw-bold fs-5">{{ formatMoneda(kpis.cobradoMes) }}</div>
            </div>
          </div>
          <div class="col-6 col-md-3">
            <div class="p-3 rounded-3 text-center" style="background:#fef9c3;">
              <div class="text-muted small mb-1">⏳ Pendiente</div>
              <div class="fw-bold text-warning fs-5">{{ formatMoneda(kpis.pendiente) }}</div>
            </div>
          </div>
          <div class="col-6 col-md-3">
            <div class="p-3 rounded-3 text-center" style="background:#fef2f2;">
              <div class="text-muted small mb-1">🏷️ Comisiones</div>
              <div class="fw-bold text-danger fs-5">{{ formatMoneda(kpis.totalComisiones) }}</div>
            </div>
          </div>
        </div>

        <!-- Alerta pendientes -->
        <div v-if="kpis.pendiente > 0"
             class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
             style="background:#fef9c3; border-left:3px solid #f59e0b!important;">
          <i class="fas fa-clock text-warning fs-5"></i>
          <div>
            <strong>Tenés {{ formatMoneda(kpis.pendiente) }} pendientes de acreditación.</strong>
            <div class="text-muted small">El admin procesa las transferencias después de confirmar la entrega.</div>
          </div>
        </div>

        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-success mb-3"></div>
          <p class="text-muted small">Cargando historial de cobros...</p>
        </div>

        <div v-else-if="error" class="alert alert-warning border-0 rounded-3">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
          <button class="btn btn-sm btn-outline-warning ms-2" @click="cargarLiberaciones(1)">Reintentar</button>
        </div>

        <div v-else-if="liberaciones.length === 0" class="text-center py-5 text-muted">
          <i class="fas fa-hand-holding-usd fa-3x mb-3 d-block opacity-30"></i>
          <h6 class="fw-semibold">Aún no recibiste pagos</h6>
          <p class="small">Acá aparecerá el historial de transferencias que el admin realizó a tu cuenta.</p>
        </div>

        <div v-else>
          <div class="table-responsive">
            <table class="table mb-0" style="font-size:.875rem;">
              <thead>
                <tr>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Orden</th>
                  <th class="border-0 text-muted fw-semibold pb-3" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Fecha cobro</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-end" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Venta</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-end" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Comisión</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-end" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;color:#22c55e;">Cobrado</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Estado</th>
                  <th class="border-0 text-muted fw-semibold pb-3 text-center" style="font-size:.72rem;text-transform:uppercase;letter-spacing:.06em;">Comprobante</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="lib in liberaciones" :key="lib.id"
                    class="border-bottom" style="border-color:#f1f5f9!important;">

                  <td class="border-0 py-3">
                    <strong class="small">#{{ lib.numeroOrden }}</strong>
                    <div class="text-muted" style="font-size:.7rem;">{{ lib.compradorNombre || '—' }}</div>
                  </td>

                  <td class="border-0 py-3">
                    <div class="small">{{ lib.fechaLiberacion ? formatFecha(lib.fechaLiberacion) : '—' }}</div>
                    <div v-if="lib.fechaLiberacion" class="text-muted" style="font-size:.7rem;">
                      {{ formatHora(lib.fechaLiberacion) }}
                    </div>
                  </td>

                  <!-- FIX: usar montoVenta (mapeado desde montoBruto en el API) -->
                  <td class="border-0 py-3 text-end small">{{ formatMoneda(lib.montoVenta ?? lib.montoBruto) }}</td>

                  <td class="border-0 py-3 text-end small text-danger">
                    -{{ formatMoneda(lib.montoComision ?? lib.comision) }}
                    <div class="text-muted" style="font-size:.68rem;">{{ lib.porcentajeComision ?? 10 }}%</div>
                  </td>

                  <!-- FIX: usar montoLiberado (mapeado desde montoNeto en el API) -->
                  <td class="border-0 py-3 text-end fw-bold text-success">
                    {{ formatMoneda(lib.montoLiberado ?? lib.montoNeto) }}
                  </td>

                  <!-- FIX Bug 3: el API mapea Confirmado/Transferido → 'Liberado' -->
                  <td class="border-0 py-3 text-center">
                    <span class="badge rounded-pill" style="font-size:.67rem;"
                          :class="getEstadoBadge(lib.estado)">
                      {{ getEstadoLabel(lib.estado) }}
                    </span>
                    <div v-if="lib.notas" class="text-muted mt-1" style="font-size:.7rem;" :title="lib.notas">
                      <i class="fas fa-comment-dots me-1"></i>{{ lib.notas.length > 40 ? lib.notas.substring(0,40)+'…' : lib.notas }}
                    </div>
                  </td>

                  <td class="border-0 py-3 text-center">
                    <a v-if="lib.comprobanteUrl" :href="lib.comprobanteUrl" target="_blank"
                       class="btn btn-sm btn-outline-primary" title="Ver comprobante de transferencia">
                      <i class="fas fa-file-image"></i>
                    </a>
                    <span v-else class="text-muted small">—</span>
                  </td>

                </tr>
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="2" class="border-0 pt-3 text-muted small fw-semibold">
                    Total de {{ totalRegistros }} cobro{{ totalRegistros !== 1 ? 's' : '' }}
                  </td>
                  <td class="border-0 pt-3 text-end fw-semibold small">{{ formatMoneda(kpis.totalVentas) }}</td>
                  <td class="border-0 pt-3 text-end text-danger fw-semibold small">-{{ formatMoneda(kpis.totalComisiones) }}</td>
                  <td class="border-0 pt-3 text-end text-success fw-bold">{{ formatMoneda(kpis.totalCobrado) }}</td>
                  <td colspan="2" class="border-0"></td>
                </tr>
              </tfoot>
            </table>
          </div>

          <div class="mt-3 p-3 rounded-3" style="background:#f8fafc;font-size:.78rem;">
            <i class="fas fa-info-circle text-primary me-1"></i>
            <strong>Cálculo:</strong> La plataforma retiene el <strong>{{ kpis.porcentajeComision ?? 10 }}%</strong>
            de cada venta como comisión. El monto cobrado = subtotal de tu venta − comisión.
          </div>

          <nav v-if="totalPaginas > 1" class="mt-4 d-flex justify-content-center">
            <ul class="pagination pagination-sm mb-0 gap-1">
              <li class="page-item" :class="{ disabled: paginaActual === 1 }">
                <a class="page-link rounded" href="#" @click.prevent="cargarLiberaciones(paginaActual - 1)">
                  <i class="fas fa-chevron-left"></i>
                </a>
              </li>
              <li v-for="p in paginasVisibles" :key="p" class="page-item" :class="{ active: p === paginaActual }">
                <a class="page-link rounded" href="#" @click.prevent="cargarLiberaciones(p)">{{ p }}</a>
              </li>
              <li class="page-item" :class="{ disabled: paginaActual === totalPaginas }">
                <a class="page-link rounded" href="#" @click.prevent="cargarLiberaciones(paginaActual + 1)">
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
import { fetchWithAuth, formatMoneda, formatFecha, formatHora } from '@shared/apiUtils.js'

const liberaciones   = ref([])
const loading        = ref(true)
const error          = ref(null)
const paginaActual   = ref(1)
const totalPaginas   = ref(0)
const totalRegistros = ref(0)
const tamanioPagina  = 15

const kpis = ref({
  totalCobrado: 0, cobradoMes: 0, pendiente: 0,
  totalComisiones: 0, totalVentas: 0, porcentajeComision: 10,
})

const paginasVisibles = computed(() => {
  const inicio = Math.max(1, paginaActual.value - 2)
  const fin    = Math.min(totalPaginas.value, inicio + 4)
  return Array.from({ length: fin - inicio + 1 }, (_, i) => inicio + i)
})

async function cargarLiberaciones(pagina = 1) {
  try {
    loading.value      = true
    error.value        = null
    paginaActual.value = pagina

    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const vendedorId = user.vendedorId
    if (!vendedorId) { error.value = 'No se encontró el perfil de vendedor.'; return }

    const data = await fetchWithAuth(
      `/api/LiberacionApi/vendedor/${vendedorId}?pagina=${pagina}&registrosPorPagina=${tamanioPagina}`
    )

    liberaciones.value   = data.data ?? data ?? []
    totalRegistros.value = data.totalRegistros ?? liberaciones.value.length
    totalPaginas.value   = data.totalPaginas   ?? Math.ceil(totalRegistros.value / tamanioPagina)

    const todas = liberaciones.value
    const ahora = new Date()

    const esCobrado  = l => l.estado === 'Transferido' || l.estado === 'Confirmado'
    const esPendiente = l => l.estado === 'Pendiente' || l.estado === 'ListoParaLiberar'

    const getMonto = l => l.montoNeto ?? l.montoLiberado ?? 0

    kpis.value = {
      totalCobrado:    todas.filter(esCobrado).reduce((s, l) => s + getMonto(l), 0),
      cobradoMes:      todas.filter(l => esCobrado(l) && new Date(l.fechaLiberacion).getMonth() === ahora.getMonth()).reduce((s, l) => s + getMonto(l), 0),
      pendiente:       todas.filter(esPendiente).reduce((s, l) => s + getMonto(l), 0),
      totalComisiones: todas.filter(esCobrado).reduce((s, l) => s + (l.comision ?? l.montoComision ?? 0), 0),
      totalVentas:     todas.reduce((s, l) => s + (l.montoBruto ?? l.montoVenta ?? 0), 0),
      porcentajeComision: data.porcentajeComision ?? 10,
    }
  } catch (err) {
    error.value = 'No se pudo cargar el historial de cobros.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

// FIX Bug 3: mapeo de estados del enum a labels legibles
function getEstadoLabel(estado) {
  return {
    Liberado:         '✅ Acreditado',
    Confirmado:       '✅ Acreditado',
    Transferido:      '💸 Transferido',
    ListoParaLiberar: '🔜 En proceso',
    Pendiente:        '⏳ Pendiente',
    EnProceso:        '⏳ En proceso',
    Cancelado:        '❌ Cancelado',
  }[estado] ?? estado ?? '—'
}
function getEstadoBadge(estado) {
  return {
    Liberado:         'bg-success',
    Confirmado:       'bg-success',
    Transferido:      'bg-info',
    ListoParaLiberar: 'bg-warning text-dark',
    Pendiente:        'bg-warning text-dark',
    EnProceso:        'bg-warning text-dark',
    Cancelado:        'bg-danger',
  }[estado] ?? 'bg-secondary'
}

onMounted(() => cargarLiberaciones(1))
defineExpose({ cargarLiberaciones })
</script>
