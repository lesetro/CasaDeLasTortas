<template>
  <div class="detalle-venta">
    <div class="card shadow-sm border-0 rounded-3">

      <!-- Header -->
      <div class="card-header bg-white border-0 d-flex justify-content-between align-items-center px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-receipt me-2 text-primary"></i>
          Orden {{ venta ? `#${venta.numeroOrden}` : '' }}
        </h5>
        <div class="d-flex gap-2">
          <button v-if="venta && puedeFactura" class="btn btn-outline-secondary btn-sm"
                  @click="verFactura">
            <i class="fas fa-file-invoice me-1"></i>Ver Factura
          </button>
          <button class="btn btn-outline-secondary btn-sm" @click="$emit('cerrar')">
            <i class="fas fa-arrow-left me-1"></i>Volver
          </button>
        </div>
      </div>

      <div class="card-body px-4 pb-4">

        <!-- Loading -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary mb-3"></div>
          <p class="text-muted small">Cargando detalle...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="alert alert-warning rounded-3 border-0">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>

        <template v-else-if="venta">

          <!-- ── BARRA DE PROGRESO DEL FLUJO ── -->
          <div class="flujo-pago mb-4 p-3 rounded-3" style="background:#f8fafc;">
            <div class="d-flex align-items-center justify-content-between flex-wrap gap-2">
              <div v-for="(paso, idx) in pasosFlujo" :key="idx"
                   class="d-flex flex-column align-items-center gap-1 flex-fill">
                <div class="paso-circulo d-flex align-items-center justify-content-center rounded-circle fw-bold"
                     :class="paso.activo ? 'activo' : paso.completado ? 'completado' : 'pendiente'">
                  <i v-if="paso.completado" class="fas fa-check" style="font-size:.7rem;"></i>
                  <span v-else style="font-size:.7rem;">{{ idx + 1 }}</span>
                </div>
                <span class="text-center" style="font-size:.65rem; line-height:1.2; max-width:70px;"
                      :class="paso.activo ? 'fw-bold text-primary' : paso.completado ? 'text-success' : 'text-muted'">
                  {{ paso.label }}
                </span>
                <!-- Línea entre pasos -->
                <div v-if="idx < pasosFlujo.length - 1"
                     style="position:absolute; display:none;"></div>
              </div>
            </div>
          </div>

          <!-- ── ALERTA: ACCIÓN REQUERIDA ── -->
          <!-- Caso 1: orden sin pago -->
          <div v-if="venta.estado === 'Pendiente' && !tienePagoPendienteOCompletado"
               class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
               style="background:linear-gradient(135deg,#fff7ed,#ffedd5); border-left:4px solid #f97316 !important;">
            <i class="fas fa-exclamation-circle text-warning fs-4"></i>
            <div class="flex-grow-1">
              <strong class="d-block">Pago pendiente</strong>
              <span class="text-muted small">
                Realizá la transferencia a la cuenta de la plataforma y subí el comprobante.
              </span>
            </div>
            <button class="btn btn-warning fw-semibold" @click="abrirModalPago">
              <i class="fas fa-upload me-1"></i>Pagar ahora
            </button>
          </div>

          <!-- Caso 2: comprobante en revisión -->
          <div v-else-if="tienePagoPendiente"
               class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
               style="background:#fefce8;">
            <i class="fas fa-clock text-warning fs-4"></i>
            <div>
              <strong class="d-block">Comprobante en revisión</strong>
              <span class="text-muted small">
                El admin está verificando tu pago. Te notificaremos cuando se confirme (1 día hábil).
              </span>
            </div>
          </div>

          <!-- Caso 3: pago aprobado, preparando -->
          <div v-else-if="venta.estado === 'EnPreparacion'"
               class="alert alert-primary border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-cookie-bite fs-4"></i>
            <div>
              <strong class="d-block">¡Pago confirmado! Tu pedido está en preparación.</strong>
              <span class="small opacity-75">Te avisaremos cuando esté listo para retirar.</span>
            </div>
          </div>

          <!-- Caso 4: listo para retirar -->
          <div v-else-if="venta.estado === 'ListaParaEnvio'"
               class="alert alert-success border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-store fs-4"></i>
            <div>
              <strong class="d-block">¡Tu pedido está listo para retirar!</strong>
              <span class="small opacity-75">Dirigite al local con el número de orden: #{{ venta.numeroOrden }}</span>
            </div>
          </div>

          <div class="row g-4">

            <!-- Info General -->
            <div class="col-md-6">
              <div class="card border-0 h-100" style="background:#f8fafc;">
                <div class="card-body p-3">
                  <h6 class="fw-bold mb-3 small text-uppercase text-muted" style="letter-spacing:.08em;">
                    Información del pedido
                  </h6>
                  <div class="d-flex justify-content-between py-1 border-bottom">
                    <span class="text-muted small">Orden</span>
                    <strong class="small">#{{ venta.numeroOrden }}</strong>
                  </div>
                  <div class="d-flex justify-content-between py-1 border-bottom">
                    <span class="text-muted small">Fecha</span>
                    <span class="small">{{ formatFechaHora(venta.fechaVenta) }}</span>
                  </div>
                  <div class="d-flex justify-content-between py-1 border-bottom">
                    <span class="text-muted small">Estado</span>
                    <span class="badge rounded-pill" :class="getBadgeClaseVenta(venta.estado)">
                      {{ getEstadoVentaTexto(venta.estado) }}
                    </span>
                  </div>
                  <div class="d-flex justify-content-between py-1">
                    <span class="text-muted small">Total</span>
                    <strong class="text-success">{{ formatMoneda(venta.total) }}</strong>
                  </div>
                  <div v-if="venta.fechaEntregaEstimada" class="d-flex justify-content-between py-1 border-top">
                    <span class="text-muted small">Entrega estimada</span>
                    <span class="small">{{ formatFecha(venta.fechaEntregaEstimada) }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Estado del pago -->
            <div class="col-md-6">
              <div class="card border-0 h-100" style="background:#f8fafc;">
                <div class="card-body p-3">
                  <h6 class="fw-bold mb-3 small text-uppercase text-muted" style="letter-spacing:.08em;">
                    Estado del pago
                  </h6>

                  <div v-if="venta.pagos?.length">
                    <div v-for="pago in venta.pagos" :key="pago.pagoId"
                         class="p-3 rounded-3 mb-2"
                         :style="pago.estado === 'Completado' ? 'background:#f0fdf4;' : pago.estado === 'Cancelado' ? 'background:#fef2f2;' : 'background:#fef9c3;'">
                      <div class="d-flex justify-content-between align-items-start">
                        <div>
                          <span class="badge rounded-pill me-2"
                                :class="pago.estado === 'Completado' ? 'bg-success' : pago.estado === 'Cancelado' ? 'bg-danger' : 'bg-warning text-dark'">
                            {{ getPagoEstadoLabel(pago.estado) }}
                          </span>
                          <div class="text-muted small mt-1">{{ formatFechaHora(pago.fechaPago) }}</div>
                          <div class="text-muted small">{{ pago.metodoPago || 'Transferencia' }}</div>
                          <div v-if="pago.numeroTransaccion" class="text-muted small">
                            Nº {{ pago.numeroTransaccion }}
                          </div>
                          <div v-if="pago.observaciones" class="small text-danger mt-1">
                            <i class="fas fa-info-circle me-1"></i>{{ pago.observaciones }}
                          </div>
                        </div>
                        <strong class="text-success">{{ formatMoneda(pago.monto) }}</strong>
                      </div>
                      <!-- Comprobante adjunto -->
                      <div v-if="pago.archivoComprobante" class="mt-2">
                        <a :href="pago.archivoComprobante" target="_blank"
                           class="btn btn-sm btn-outline-primary" style="font-size:.75rem;">
                          <i class="fas fa-file-image me-1"></i>Ver comprobante
                        </a>
                      </div>
                    </div>

                    <!-- Botón reintentar si fue rechazado -->
                    <div v-if="pagoRechazado" class="mt-2">
                      <button class="btn btn-warning btn-sm w-100 fw-semibold"
                              @click="abrirModalPago">
                        <i class="fas fa-redo me-1"></i>Volver a enviar comprobante
                      </button>
                    </div>
                  </div>

                  <!-- Sin pagos: mostrar botón de pagar -->
                  <div v-else class="text-center py-3">
                    <i class="fas fa-file-invoice-dollar text-muted fa-2x mb-2 d-block"></i>
                    <p class="text-muted small mb-2">Sin pagos registrados</p>
                    <button v-if="venta.estado === 'Pendiente'"
                            class="btn btn-warning fw-semibold"
                            @click="abrirModalPago">
                      <i class="fas fa-upload me-1"></i>Subir comprobante
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Productos -->
            <div class="col-12">
              <div class="card border-0" style="background:#f8fafc;">
                <div class="card-body p-3">
                  <h6 class="fw-bold mb-3 small text-uppercase text-muted" style="letter-spacing:.08em;">
                    Productos del pedido
                  </h6>
                  <div class="table-responsive">
                    <table class="table mb-0" style="font-size:.875rem;">
                      <thead>
                        <tr class="border-bottom">
                          <th class="fw-semibold text-muted border-0 pb-2">Producto</th>
                          <th class="fw-semibold text-muted border-0 pb-2 text-center">Cant.</th>
                          <th class="fw-semibold text-muted border-0 pb-2 text-end">P. Unit.</th>
                          <th class="fw-semibold text-muted border-0 pb-2 text-end">Subtotal</th>
                          <th class="fw-semibold text-muted border-0 pb-2 text-center">Estado</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="prod in venta.productos" :key="prod.tortaId" class="border-bottom">
                          <td class="border-0 py-3">
                            <div class="d-flex align-items-center gap-2">
                              <img v-if="prod.imagenTorta" :src="prod.imagenTorta"
                                   class="rounded" style="width:38px;height:38px;object-fit:cover;">
                              <div v-else class="rounded d-flex align-items-center justify-content-center"
                                   style="width:38px;height:38px;background:#ffedd5;font-size:1.2rem;flex-shrink:0;">🎂</div>
                              <div>
                                <div class="fw-semibold">{{ prod.nombreTorta }}</div>
                                <div class="text-muted small" v-if="prod.nombreVendedor">
                                  <i class="fas fa-store me-1"></i>{{ prod.nombreVendedor }}
                                </div>
                                <div class="text-muted fst-italic small" v-if="prod.notasPersonalizacion">
                                  📝 {{ prod.notasPersonalizacion }}
                                </div>
                              </div>
                            </div>
                          </td>
                          <td class="border-0 text-center">{{ prod.cantidad }}</td>
                          <td class="border-0 text-end">{{ formatMoneda(prod.precioUnitario) }}</td>
                          <td class="border-0 text-end fw-semibold">{{ formatMoneda(prod.subtotal) }}</td>
                          <td class="border-0 text-center">
                            <span class="badge rounded-pill" style="font-size:.68rem;"
                                  :class="getBadgeDetalleEstado(prod.estadoDetalle)">
                              {{ prod.estadoDetalle || '—' }}
                            </span>
                          </td>
                        </tr>
                      </tbody>
                      <tfoot>
                        <tr>
                          <td colspan="3" class="border-0 text-end text-muted small fw-semibold pt-3">Total</td>
                          <td class="border-0 text-end fw-bold text-success fs-6 pt-3">
                            {{ formatMoneda(venta.total) }}
                          </td>
                          <td class="border-0"></td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                </div>
              </div>
            </div>

          </div><!-- /row -->

        </template>
      </div><!-- /card-body -->
    </div><!-- /card -->

    <!-- ══════════════════════════════════════════════
         MODAL: SUBIR COMPROBANTE DE PAGO
    ══════════════════════════════════════════════ -->
    <div v-if="mostrarModalPago" class="modal-overlay" @click.self="cerrarModalPago">
      <div class="modal-box rounded-4 shadow-lg p-4" style="max-width:520px;width:95%;background:#fff;">

        <!-- Header modal -->
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h6 class="fw-bold mb-0">
            <i class="fas fa-upload text-warning me-2"></i>Subir comprobante de pago
          </h6>
          <button class="btn-close" @click="cerrarModalPago"></button>
        </div>

        <!-- Datos bancarios de la plataforma -->
        <div class="p-3 rounded-3 mb-4" style="background:#f0fdf4; border:1px solid #bbf7d0;">
          <div class="fw-semibold small mb-2 text-success">
            <i class="fas fa-university me-1"></i>Datos para la transferencia
          </div>
          <div v-if="datosBancarios" class="small">
            <div class="d-flex justify-content-between py-1 border-bottom">
              <span class="text-muted">Alias</span>
              <strong class="font-monospace">{{ datosBancarios.alias }}</strong>
            </div>
            <div class="d-flex justify-content-between py-1 border-bottom">
              <span class="text-muted">CBU</span>
              <strong class="font-monospace" style="font-size:.75rem;">{{ datosBancarios.cbu }}</strong>
            </div>
            <div class="d-flex justify-content-between py-1 border-bottom">
              <span class="text-muted">Banco</span>
              <strong>{{ datosBancarios.banco }}</strong>
            </div>
            <div class="d-flex justify-content-between py-1">
              <span class="text-muted">Titular</span>
              <strong>{{ datosBancarios.titular }}</strong>
            </div>
          </div>
          <div v-else class="small text-muted">Cargando datos bancarios...</div>

          <!-- Monto a transferir -->
          <div class="mt-3 p-2 rounded-3 text-center" style="background:#dcfce7;">
            <div class="text-muted small">Monto a transferir</div>
            <div class="fs-4 fw-bold text-success">{{ formatMoneda(venta?.total ?? 0) }}</div>
          </div>
        </div>

        <!-- Formulario -->
        <div class="mb-3">
          <label class="form-label fw-semibold small">
            Número de transacción / referencia
          </label>
          <input type="text" v-model="formPago.numeroTransaccion" class="form-control"
                 placeholder="Ej: TRF-2025-001234" />
        </div>

        <div class="mb-3">
          <label class="form-label fw-semibold small">
            Comprobante (foto o PDF) <span class="text-danger">*</span>
          </label>
          <div class="upload-area rounded-3 text-center p-3 position-relative"
               :class="{ 'drag-over': isDragOver }"
               @dragover.prevent="isDragOver = true"
               @dragleave="isDragOver = false"
               @drop.prevent="onFileDrop"
               @click="$refs.fileInput.click()"
               style="border:2px dashed #d1d5db; cursor:pointer; transition:all .2s;">
            <div v-if="!archivoSeleccionado">
              <i class="fas fa-cloud-upload-alt text-muted fs-3 mb-2 d-block"></i>
              <p class="text-muted small mb-0">Arrastrá el archivo acá o hacé click</p>
              <p class="text-muted" style="font-size:.72rem;">JPG, PNG o PDF — máx. 5 MB</p>
            </div>
            <div v-else class="d-flex align-items-center justify-content-center gap-2">
              <i class="fas fa-file-image text-success fs-4"></i>
              <div class="text-start">
                <div class="fw-semibold small">{{ archivoSeleccionado.name }}</div>
                <div class="text-muted" style="font-size:.72rem;">
                  {{ (archivoSeleccionado.size / 1024).toFixed(0) }} KB
                </div>
              </div>
              <button class="btn btn-sm btn-outline-danger ms-2" @click.stop="archivoSeleccionado = null">
                <i class="fas fa-times"></i>
              </button>
            </div>
            <input ref="fileInput" type="file" accept="image/*,.pdf"
                   class="d-none" @change="onFileChange" />
          </div>
        </div>

        <div class="mb-4">
          <label class="form-label fw-semibold small">Observaciones (opcional)</label>
          <textarea v-model="formPago.observaciones" class="form-control" rows="2"
                    placeholder="Ej: Transferí desde cuenta X el día Y..."></textarea>
        </div>

        <!-- Botones -->
        <div class="d-flex gap-2">
          <button class="btn btn-outline-secondary flex-fill" @click="cerrarModalPago"
                  :disabled="enviandoPago">Cancelar</button>
          <button class="btn btn-warning fw-semibold flex-fill" @click="enviarComprobante"
                  :disabled="enviandoPago || !archivoSeleccionado">
            <span v-if="enviandoPago" class="spinner-border spinner-border-sm me-1"></span>
            <i v-else class="fas fa-paper-plane me-1"></i>
            {{ enviandoPago ? 'Enviando...' : 'Enviar comprobante' }}
          </button>
        </div>

        <p class="text-muted text-center mt-3 mb-0" style="font-size:.72rem;">
          <i class="fas fa-shield-alt me-1 text-success"></i>
          El admin verificará tu pago en 1 día hábil y recibirás una notificación.
        </p>
      </div>
    </div>

  </div><!-- /detalle-venta -->
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  fetchWithAuth, formatMoneda, formatFecha, formatFechaHora,
  getEstadoVentaTexto, getBadgeClaseVenta, mostrarToast
} from '@shared/apiUtils.js'

const props = defineProps({ ventaId: { type: Number, required: true } })
const emit  = defineEmits(['cerrar', 'pago-enviado'])

// ── Estado ──────────────────────────────────────────────────────────
const venta          = ref(null)
const loading        = ref(true)
const error          = ref(null)
const datosBancarios = ref(null)

// Modal pago
const mostrarModalPago   = ref(false)
const enviandoPago       = ref(false)
const archivoSeleccionado = ref(null)
const isDragOver         = ref(false)
const fileInput          = ref(null)
const formPago = ref({ numeroTransaccion: '', observaciones: '' })

// ── Computed ─────────────────────────────────────────────────────────
const tienePagoPendiente = computed(() =>
  venta.value?.pagos?.some(p => p.estado === 'Pendiente') ?? false
)
const tienePagoPendienteOCompletado = computed(() =>
  venta.value?.pagos?.some(p => p.estado === 'Pendiente' || p.estado === 'Completado') ?? false
)
const pagoRechazado = computed(() =>
  venta.value?.pagos?.some(p => p.estado === 'Cancelado') ?? false
)
const puedeFactura = computed(() =>
  ['Entregada', 'Pagada', 'EnPreparacion'].includes(venta.value?.estado)
)

const pasosFlujo = computed(() => {
  const e = venta.value?.estado ?? 'Pendiente'
  const orden = ['Pendiente', 'Pagada', 'EnPreparacion', 'ListaParaEnvio', 'Entregada']
  const idxActual = orden.indexOf(e)
  return [
    { label: 'Pedido creado',      completado: idxActual > 0, activo: e === 'Pendiente' },
    { label: 'Pago verificado',    completado: idxActual > 1, activo: e === 'Pagada' },
    { label: 'En preparación',     completado: idxActual > 2, activo: e === 'EnPreparacion' },
    { label: 'Listo para retirar', completado: idxActual > 3, activo: e === 'ListaParaEnvio' },
    { label: 'Entregado',          completado: idxActual >= 4, activo: e === 'Entregada' },
  ]
})

// ── Helpers ──────────────────────────────────────────────────────────
function getPagoEstadoLabel(estado) {
  return { Completado: 'Aprobado ✓', Pendiente: 'En revisión ⏳', Cancelado: 'Rechazado ✗' }[estado] ?? estado
}
function getBadgeDetalleEstado(estado) {
  return {
    Pendiente: 'bg-warning text-dark', Confirmado: 'bg-info',
    EnPreparacion: 'bg-primary', Listo: 'bg-success',
    Entregado: 'bg-success', Cancelado: 'bg-danger'
  }[estado] ?? 'bg-secondary'
}

// ── Carga de datos ───────────────────────────────────────────────────
async function cargarDetalle() {
  try {
    loading.value = true
    error.value   = null
    const user       = JSON.parse(localStorage.getItem('user') || '{}')
    const compradorId = user.compradorId
    if (!compradorId) { error.value = 'No se encontró el perfil de comprador.'; return }

    const data = await fetchWithAuth(
      `/api/CompradorApi/${compradorId}/historial?pagina=1&registrosPorPagina=100`
    )
    const encontrada = (data.data || []).find(v => v.ventaId === props.ventaId)
    if (!encontrada) { error.value = 'No se encontró el detalle de esta venta.'; return }
    venta.value = encontrada
  } catch (err) {
    error.value = 'Error al cargar el detalle de la venta.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

async function cargarDatosBancarios() {
  try {
    const data = await fetchWithAuth('/api/ConfiguracionApi/datos-pago')
    datosBancarios.value = data
  } catch {
    datosBancarios.value = {
      alias: 'casadelastortas.pagos',
      cbu: '0000000000000000000000',
      banco: 'Banco Nación',
      titular: 'Casa de las Tortas',
    }
  }
}

// ── Modal pago ───────────────────────────────────────────────────────
function abrirModalPago() {
  formPago.value = { numeroTransaccion: '', observaciones: '' }
  archivoSeleccionado.value = null
  mostrarModalPago.value = true
  if (!datosBancarios.value) cargarDatosBancarios()
}
function cerrarModalPago() {
  if (enviandoPago.value) return
  mostrarModalPago.value = false
}
function onFileChange(e) {
  const file = e.target.files?.[0]
  if (file && file.size <= 5 * 1024 * 1024) {
    archivoSeleccionado.value = file
  } else if (file) {
    mostrarToast('El archivo no puede superar 5 MB', 'error')
  }
}
function onFileDrop(e) {
  isDragOver.value = false
  const file = e.dataTransfer.files?.[0]
  if (file && file.size <= 5 * 1024 * 1024) {
    archivoSeleccionado.value = file
  } else if (file) {
    mostrarToast('El archivo no puede superar 5 MB', 'error')
  }
}

async function enviarComprobante() {
  if (!archivoSeleccionado.value || !venta.value) return
  try {
    enviandoPago.value = true
    const token = localStorage.getItem('authToken')
    const fd = new FormData()
    fd.append('ventaId',            venta.value.ventaId)
    fd.append('comprobante',        archivoSeleccionado.value)
    fd.append('numeroTransaccion',  formPago.value.numeroTransaccion)
    fd.append('observaciones',      formPago.value.observaciones)
    fd.append('metodoPago',         'Transferencia')

    const resp = await fetch('/api/PagoApi/subir-comprobante', {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}`, 'X-Requested-With': 'XMLHttpRequest' },
      body: fd,
    })
    if (!resp.ok) {
      const err = await resp.json().catch(() => ({}))
      throw new Error(err.message || 'Error al enviar el comprobante')
    }
    mostrarToast('¡Comprobante enviado! El admin lo revisará pronto.', 'success')
    cerrarModalPago()
    await cargarDetalle()
    emit('pago-enviado')
  } catch (err) {
    mostrarToast(err.message || 'Error al enviar el comprobante', 'error')
  } finally {
    enviandoPago.value = false
  }
}

function verFactura() {
  window.open(`/Venta/Factura/${venta.value?.ventaId}`, '_blank')
}

onMounted(() => {
  cargarDetalle()
  cargarDatosBancarios()
})
</script>

<style scoped>
.paso-circulo {
  width: 28px; height: 28px; font-size: .75rem; flex-shrink: 0;
  border: 2px solid #e2e8f0;
}
.paso-circulo.completado { background: #22c55e; border-color: #22c55e; color: white; }
.paso-circulo.activo     { background: #3b82f6; border-color: #3b82f6; color: white; }
.paso-circulo.pendiente  { background: #f1f5f9; border-color: #e2e8f0; color: #94a3b8; }

.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.45);
  display: flex; align-items: center; justify-content: center;
  z-index: 9999; padding: 1rem;
  animation: fadeIn .15s ease;
}
.modal-box { animation: slideUp .2s ease; max-height: 90vh; overflow-y: auto; }
.upload-area:hover { border-color: #f97316 !important; background: #fff7ed; }
.upload-area.drag-over { border-color: #22c55e !important; background: #f0fdf4; }
@keyframes fadeIn { from { opacity: 0 } to { opacity: 1 } }
@keyframes slideUp { from { transform: translateY(20px); opacity: 0 } to { transform: translateY(0); opacity: 1 } }
</style>