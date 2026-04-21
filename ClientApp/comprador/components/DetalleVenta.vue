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
          <button v-if="venta && puedeFactura" class="btn btn-outline-secondary btn-sm" @click="verFactura">
            <i class="fas fa-file-invoice me-1"></i>Ver Factura
          </button>
          <button class="btn btn-outline-secondary btn-sm" @click="$emit('cerrar')">
            <i class="fas fa-arrow-left me-1"></i>Volver
          </button>
        </div>
      </div>

      <div class="card-body px-4 pb-4">

        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary mb-3"></div>
          <p class="text-muted small">Cargando detalle...</p>
        </div>

        <div v-else-if="error" class="alert alert-warning rounded-3 border-0">
          <i class="fas fa-exclamation-triangle me-2"></i>{{ error }}
        </div>

        <template v-else-if="venta">

          <!-- BARRA DE PROGRESO -->
          <div class="flujo-pago mb-4 p-3 rounded-3" style="background:#f8fafc;">
            <div class="d-flex align-items-center justify-content-between flex-wrap gap-2">
              <div v-for="(paso, idx) in pasosFlujo" :key="idx"
                   class="d-flex flex-column align-items-center gap-1 flex-fill">
                <div class="paso-circulo d-flex align-items-center justify-content-center rounded-circle fw-bold"
                     :class="paso.activo ? 'activo' : paso.completado ? 'completado' : 'pendiente'">
                  <i v-if="paso.completado" class="fas fa-check" style="font-size:.7rem;"></i>
                  <span v-else style="font-size:.7rem;">{{ idx + 1 }}</span>
                </div>
                <span class="text-center" style="font-size:.65rem;line-height:1.2;max-width:70px;"
                      :class="paso.activo ? 'fw-bold text-primary' : paso.completado ? 'text-success' : 'text-muted'">
                  {{ paso.label }}
                </span>
              </div>
            </div>
          </div>

          <!-- ALERTAS SEGÚN ESTADO -->
          <div v-if="mostrarBotonPagar"
               class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
               style="background:linear-gradient(135deg,#fff7ed,#ffedd5);border-left:4px solid #f97316 !important;">
            <i class="fas fa-exclamation-circle text-warning fs-4"></i>
            <div class="flex-grow-1">
              <strong class="d-block">Pago pendiente</strong>
              <span class="text-muted small">Realizá la transferencia y subí el comprobante.</span>
            </div>
            <button class="btn btn-warning fw-semibold" @click="abrirModalPago">
              <i class="fas fa-upload me-1"></i>Pagar ahora
            </button>
          </div>

          <div v-else-if="pagoEnRevision"
               class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
               style="background:#fefce8;">
            <i class="fas fa-clock text-warning fs-4"></i>
            <div>
              <strong class="d-block">Comprobante en revisión</strong>
              <span class="text-muted small">El admin está verificando tu pago. Máximo 1 día hábil.</span>
            </div>
          </div>

          <!-- Estado: Pagada con detalles confirmados por el vendedor -->
          <div v-else-if="venta.estado === 'Pagada' && vendedorConfirmo"
               class="alert border-0 rounded-3 mb-4 d-flex align-items-center gap-3"
               style="background:#f0f9ff; border-left:4px solid #38bdf8 !important;">
            <i class="fas fa-check-circle text-info fs-4"></i>
            <div>
              <strong class="d-block">El vendedor tomó tu pedido</strong>
              <span class="small opacity-75">Tu pedido fue aceptado y pronto comenzará la preparación.</span>
            </div>
          </div>

          <div v-else-if="venta.estado === 'EnPreparacion'"
               class="alert alert-primary border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-cookie-bite fs-4"></i>
            <div>
              <strong class="d-block">¡Tu pedido está en preparación!</strong>
              <span class="small opacity-75">El vendedor está trabajando en tu pedido. Te avisaremos cuando esté listo.</span>
            </div>
          </div>

          <div v-else-if="venta.estado === 'ListaParaRetiro'"
               class="alert alert-success border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-store fs-4"></i>
            <div>
              <strong class="d-block">¡Tu pedido está listo para retirar!</strong>
              <span class="small opacity-75">Dirigite al local con el número de orden: #{{ venta.numeroOrden }}</span>
            </div>
          </div>

          <!-- ============================================================
                Reemplazamos "Confirmar recepción" por "Reclamo"
               ============================================================ -->
          <div v-else-if="puedeIniciarReclamo"
              class="alert alert-info border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-box-open fs-4"></i>
            <div class="flex-grow-1">
              <strong>¡Tu pedido fue entregado!</strong>
              <div class="small opacity-75">Si tenés algún problema con tu compra, podés iniciar un reclamo.</div>
            </div>
            <button class="btn btn-outline-danger btn-sm fw-semibold" @click="abrirModalReclamo">
              <i class="fas fa-exclamation-triangle me-1"></i>Tengo un problema
            </button>
          </div>

          <!-- Estado: En disputa -->
          <div v-else-if="venta.estado === 'EnDisputa'"
              class="alert alert-warning border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-gavel fs-4"></i>
            <div>
              <strong>Reclamo en proceso</strong>
              <span class="small opacity-75 d-block">Tu reclamo está siendo revisado por el administrador.</span>
            </div>
          </div>

          <div v-else-if="venta.estado === 'Entregada' && !puedeIniciarReclamo"
              class="alert alert-success border-0 rounded-3 mb-4 d-flex align-items-center gap-3">
            <i class="fas fa-check-circle fs-4"></i>
            <div>
              <strong>¡Pedido completado!</strong>
              <span class="small opacity-75 d-block">Gracias por tu compra.</span>
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
                         class="p-3 rounded-3 mb-2" :style="stiloPago(pago.estado)">
                      <div class="d-flex justify-content-between align-items-start">
                        <div>
                          <span class="badge rounded-pill me-2" :class="badgePago(pago.estado)">
                            {{ labelPago(pago.estado) }}
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
                      <div v-if="pago.archivoComprobante" class="mt-2">
                        <a :href="pago.archivoComprobante" target="_blank"
                           class="btn btn-sm btn-outline-primary" style="font-size:.75rem;">
                          <i class="fas fa-file-image me-1"></i>Ver comprobante
                        </a>
                      </div>
                    </div>
                  </div>
                  <div v-else class="text-center text-muted py-3">
                    <i class="fas fa-credit-card mb-2 d-block" style="font-size:1.5rem;opacity:.5;"></i>
                    <span class="small">Sin pagos registrados</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Detalle de productos -->
          <div class="mt-4">
            <h6 class="fw-bold mb-3 small text-uppercase text-muted" style="letter-spacing:.08em;">
              <i class="fas fa-birthday-cake me-2"></i>Productos
            </h6>
            <div class="table-responsive">
              <table class="table table-borderless mb-0">
                <thead style="background:#f8fafc;">
                  <tr>
                    <th class="small text-muted fw-normal py-2">Producto</th>
                    <th class="small text-muted fw-normal py-2 text-center">Cant.</th>
                    <th class="small text-muted fw-normal py-2 text-end">Precio</th>
                    <th class="small text-muted fw-normal py-2 text-end">Subtotal</th>
                    <th class="small text-muted fw-normal py-2 text-center">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="det in venta.detalles" :key="det.detalleId" class="border-bottom">
                    <td class="py-3">
                      <div class="d-flex align-items-center gap-2">
                        <img v-if="det.imagenTorta" :src="det.imagenTorta"
                             class="rounded" style="width:48px;height:48px;object-fit:cover;">
                        <div v-else class="rounded d-flex align-items-center justify-content-center"
                             style="width:48px;height:48px;background:#f1f5f9;">
                          <i class="fas fa-birthday-cake text-muted"></i>
                        </div>
                        <div>
                          <strong class="small d-block">{{ det.nombreTorta }}</strong>
                          <span v-if="det.notasPersonalizacion" class="text-muted" style="font-size:.7rem;">
                            {{ det.notasPersonalizacion }}
                          </span>
                        </div>
                      </div>
                    </td>
                    <td class="text-center py-3 small">{{ det.cantidad }}</td>
                    <td class="text-end py-3 small">{{ formatMoneda(det.precioUnitario) }}</td>
                    <td class="text-end py-3 small fw-bold">{{ formatMoneda(det.subtotal) }}</td>
                    <td class="text-center py-3">
                      <span class="badge rounded-pill" :class="getBadgeDetalleEstado(det.estado)"
                            style="font-size:.68rem;">
                        {{ getLabelDetalleEstado(det.estado) }}
                      </span>
                    </td>
                  </tr>
                </tbody>
                <tfoot style="background:#f8fafc;">
                  <tr>
                    <td colspan="3" class="text-end fw-bold small py-2">Total</td>
                    <td class="text-end fw-bold text-success py-2">{{ formatMoneda(venta.total) }}</td>
                    <td></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>

        </template>
      </div>
    </div>

    <!-- MODAL PAGO (versión original del usuario) -->
    <div v-if="mostrarModalPago" class="modal-overlay" @click.self="cerrarModalPago">
      <div class="modal-box rounded-4 shadow-lg p-4" style="max-width:520px;width:95%;background:#fff;">

        <div class="d-flex justify-content-between align-items-center mb-4">
          <h6 class="fw-bold mb-0">
            <i class="fas fa-upload text-warning me-2"></i>Subir comprobante de pago
          </h6>
          <button class="btn-close" @click="cerrarModalPago"></button>
        </div>

        <!-- Datos bancarios -->
        <div class="p-3 rounded-3 mb-4" style="background:#f0fdf4;border:1px solid #bbf7d0;">
          <div class="fw-semibold small mb-2 text-success">
            <i class="fas fa-university me-1"></i>Datos para la transferencia
          </div>
          <div v-if="cargandoDatosBancarios" class="text-center py-2">
            <div class="spinner-border spinner-border-sm text-success me-2"></div>
            <span class="text-muted small">Cargando...</span>
          </div>
          <div v-else-if="errorDatosBancarios" class="alert alert-warning py-2 mb-0 small">
            <i class="fas fa-exclamation-triangle me-1"></i>
            No se pudieron cargar los datos bancarios. Contacte al administrador.
          </div>
          <div v-else-if="datosBancarios" class="small">
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
          <div class="mt-3 p-2 rounded-3 text-center" style="background:#dcfce7;">
            <div class="text-muted small">Monto a transferir</div>
            <div class="fs-4 fw-bold text-success">{{ formatMoneda(venta?.total ?? 0) }}</div>
          </div>
        </div>

        <div class="mb-3">
          <label class="form-label fw-semibold small">Número de transacción / referencia</label>
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
               style="border:2px dashed #d1d5db;cursor:pointer;transition:all .2s;">
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
          El admin verificará tu pago en 1 día hábil.
        </p>
      </div>
    </div>

    <!-- ════════════════════════════════════════════════════════════════════
          MODAL DE RECLAMO
         ════════════════════════════════════════════════════════════════════ -->
    <div v-if="mostrarModalReclamo" class="modal-overlay" @click.self="cerrarModalReclamo">
      <div class="modal-box rounded-4 shadow-lg p-4" style="max-width:480px;width:95%;background:#fff;">
          <div class="p-4 border-bottom d-flex justify-content-between align-items-center">
            <h5 class="mb-0 fw-bold">
              <i class="fas fa-exclamation-triangle me-2 text-warning"></i>Reportar un problema
            </h5>
            <button class="btn btn-sm btn-light rounded-circle" @click="cerrarModalReclamo">
              <i class="fas fa-times"></i>
            </button>
          </div>
          
          <div class="p-4">
            <p class="text-muted small mb-4">
              Seleccioná el tipo de problema que tuviste con tu pedido. 
              El administrador revisará tu reclamo y se pondrá en contacto.
            </p>

            <!-- Tipos de problema -->
            <div class="mb-4">
              <label class="form-label small fw-bold">¿Qué pasó con tu pedido?</label>
              <div class="d-flex flex-column gap-2">
                <button v-for="tipo in tiposReclamo" :key="tipo.value"
                        class="btn text-start py-3 px-3 rounded-3"
                        :class="tipoReclamoSeleccionado === tipo.value 
                          ? 'btn-warning' 
                          : 'btn-outline-secondary'"
                        @click="tipoReclamoSeleccionado = tipo.value">
                  <i :class="tipo.icono" class="me-2"></i>
                  <span class="fw-semibold">{{ tipo.label }}</span>
                  <span class="d-block small text-muted mt-1" style="margin-left:1.5rem;">{{ tipo.descripcion }}</span>
                </button>
              </div>
            </div>

            <!-- Descripción opcional -->
            <div class="mb-3">
              <label class="form-label small fw-bold">Contanos más (opcional)</label>
              <textarea v-model="descripcionReclamo" class="form-control" rows="3"
                        placeholder="Describí brevemente el problema..."></textarea>
            </div>
          </div>

          <div class="p-4 border-top bg-light rounded-bottom-4 d-flex justify-content-end gap-2">
            <button class="btn btn-outline-secondary" @click="cerrarModalReclamo">Cancelar</button>
            <button class="btn btn-danger fw-semibold" @click="enviarReclamo"
                    :disabled="!tipoReclamoSeleccionado || enviandoReclamo">
              <span v-if="enviandoReclamo" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="fas fa-paper-plane me-1"></i>Enviar reclamo
            </button>
          </div>
        </div>
      </div>
    </div>

</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  fetchWithAuth, formatMoneda, formatFecha, formatFechaHora,
  getEstadoVentaTexto, getBadgeClaseVenta, mostrarToast
} from '@shared/apiUtils.js'

const props = defineProps({ ventaId: { type: Number, required: true } })
const emit  = defineEmits(['cerrar', 'pago-enviado'])

const venta                  = ref(null)
const loading                = ref(true)
const error                  = ref(null)
const datosBancarios         = ref(null)
const cargandoDatosBancarios = ref(false)
const errorDatosBancarios    = ref(false)

const mostrarModalPago    = ref(false)
const enviandoPago        = ref(false)
const archivoSeleccionado = ref(null)
const isDragOver          = ref(false)
const fileInput           = ref(null)
const formPago = ref({ numeroTransaccion: '', observaciones: '' })

// ═══════════════════════════════════════════════════════════════════════════
//  Variables para el modal de reclamo
// ═══════════════════════════════════════════════════════════════════════════
const mostrarModalReclamo     = ref(false)
const enviandoReclamo         = ref(false)
const tipoReclamoSeleccionado = ref(null)
const descripcionReclamo      = ref('')

// Tipos de reclamo (mapean a TipoDisputa en el backend)
const tiposReclamo = [
  { 
    value: 'ProductoNoRecibido', 
    label: 'No recibí el producto', 
    descripcion: 'El pedido figura como entregado pero no lo recibí.',
    icono: 'fas fa-box'
  },
  { 
    value: 'ProductoDaniado', 
    label: 'Llegó dañado', 
    descripcion: 'El producto llegó en mal estado o roto.',
    icono: 'fas fa-heart-broken'
  },
  { 
    value: 'ProductoDiferente', 
    label: 'No es lo que pedí', 
    descripcion: 'Recibí un producto diferente al que compré.',
    icono: 'fas fa-exchange-alt'
  },
  { 
    value: 'Otro', 
    label: 'Otro problema', 
    descripcion: 'Tengo otro tipo de inconveniente con mi pedido.',
    icono: 'fas fa-question-circle'
  }
]

// ─────────────────────────────────────────────────────────────────────
// COMPUTED
// ─────────────────────────────────────────────────────────────────────

const pagoEnRevision = computed(() =>
  venta.value?.pagos?.some(p => p.estado === 'EnRevision') ?? false
)

const pagoRechazado = computed(() =>
  venta.value?.pagos?.some(p => p.estado === 'Rechazado') ?? false
)

const motivoRechazo = computed(() =>
  venta.value?.pagos?.find(p => p.estado === 'Rechazado')?.observaciones ?? null
)

const mostrarBotonPagar = computed(() => {
  if (!venta.value) return false
  const est = venta.value.estado
  if (!['Pendiente', 'PagoEnRevision'].includes(est)) return false
  if (pagoEnRevision.value) return false
  const pagos = venta.value.pagos ?? []
  if (pagos.length === 0) return true
  return pagos.some(p => p.estado === 'Pendiente' || p.estado === 'Rechazado')
})

const puedeFactura = computed(() =>
  ['Entregada', 'Pagada', 'EnPreparacion'].includes(venta.value?.estado)
)

// ═══════════════════════════════════════════════════════════════════════════
//  Reemplaza puedeConfirmarRecepcion
// Ahora se llama puedeIniciarReclamo - muestra botón de reclamo si está entregada
// ═══════════════════════════════════════════════════════════════════════════
const puedeIniciarReclamo = computed(() => {
  if (!venta.value) return false
  // Solo mostrar si está entregada y no está ya en disputa
  return venta.value.estado === 'Entregada'
})

const pasosFlujo = computed(() => {
  const e   = venta.value?.estado ?? 'Pendiente'
  const ord = ['Pendiente', 'PagoEnRevision', 'Pagada', 'EnPreparacion', 'ListaParaRetiro', 'Entregada']
  const i   = ord.indexOf(e)
  return [
    { label: 'Pedido creado',      completado: i > 1,  activo: i <= 1 && i >= 0 },
    { label: 'Pago verificado',    completado: i > 2,  activo: e === 'Pagada' },
    { label: 'En preparación',     completado: i > 3,  activo: e === 'EnPreparacion' },
    { label: 'Listo para retirar', completado: i > 4,  activo: e === 'ListaParaRetiro' },
    { label: 'Entregado',          completado: e === 'Entregada', activo: e === 'Entregada' },
  ]
})

// ─────────────────────────────────────────────────────────────────────
// HELPERS VISUALES
// ─────────────────────────────────────────────────────────────────────
function labelPago(estado) {
  return { Pendiente:'Sin comprobante', EnRevision:'En revisión ⏳', Verificado:'Verificado ✓',
           Completado:'Completado ✓', Rechazado:'Rechazado ✗', Cancelado:'Cancelado ✗' }[estado] ?? estado
}
function badgePago(estado) {
  return { Verificado:'bg-success', Completado:'bg-success', EnRevision:'bg-warning text-dark',
           Pendiente:'bg-secondary', Rechazado:'bg-danger', Cancelado:'bg-danger' }[estado] ?? 'bg-secondary'
}
function stiloPago(estado) {
  if (['Verificado','Completado'].includes(estado)) return 'background:#f0fdf4;'
  if (['Rechazado','Cancelado'].includes(estado))   return 'background:#fef2f2;'
  if (estado === 'EnRevision')                      return 'background:#fefce8;'
  return 'background:#f8f9fa;'
}
function getBadgeDetalleEstado(estado) {
  return { Pendiente:'bg-warning text-dark', Confirmado:'bg-info', EnPreparacion:'bg-primary',
           Listo:'bg-success', Entregado:'bg-success', Cancelado:'bg-danger' }[estado] ?? 'bg-secondary'
}
function getLabelDetalleEstado(estado) {
  return { Pendiente:'Pendiente', Confirmado:'Aceptado ✓', EnPreparacion:'En preparación',
           Listo:'Listo para retirar', Entregado:'Entregado ✓', Cancelado:'Cancelado' }[estado] ?? estado
}

const vendedorConfirmo = computed(() =>
  venta.value?.detalles?.some(d => d.estado === 'Confirmado') ?? false
)

// ─────────────────────────────────────────────────────────────────────
// CARGA DE DATOS
// ─────────────────────────────────────────────────────────────────────
async function cargarDetalle() {
  try {
    loading.value = true
    error.value   = null
    const user        = JSON.parse(localStorage.getItem('user') || '{}')
    const compradorId = user.compradorId
    if (!compradorId) { error.value = 'No se encontró el perfil de comprador.'; return }

    const data      = await fetchWithAuth(
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
  cargandoDatosBancarios.value = true
  errorDatosBancarios.value    = false
  datosBancarios.value         = null
  try {
    const raw = await fetchWithAuth('/api/ConfiguracionApi/datos-pago')
    const alias   = raw.AliasCBU      ?? raw.aliasCBU      ?? raw.alias     ?? ''
    const cbu     = raw.CBU           ?? raw.cBU            ?? raw.cbu       ?? ''
    const banco   = raw.Banco         ?? raw.banco         ?? ''
    const titular = raw.TitularCuenta ?? raw.titularCuenta ?? ''
    if (!alias && !cbu) throw new Error('Datos incompletos')
    datosBancarios.value = { alias, cbu, banco, titular }
  } catch (err) {
    console.error('Error cargando datos bancarios:', err)
    errorDatosBancarios.value = true
  } finally {
    cargandoDatosBancarios.value = false
  }
}

// ─────────────────────────────────────────────────────────────────────
// MODAL PAGO (versión original del usuario)
// ─────────────────────────────────────────────────────────────────────
function abrirModalPago() {
  formPago.value            = { numeroTransaccion: '', observaciones: '' }
  archivoSeleccionado.value = null
  mostrarModalPago.value    = true
  cargarDatosBancarios()   // siempre recargar al abrir
}

function cerrarModalPago() {
  if (enviandoPago.value) return
  mostrarModalPago.value = false
}

function onFileChange(e)  { validarArchivo(e.target.files?.[0]) }
function onFileDrop(e)    { isDragOver.value = false; validarArchivo(e.dataTransfer.files?.[0]) }
function validarArchivo(f) {
  if (!f) return
  if (f.size > 5 * 1024 * 1024) { mostrarToast('El archivo no puede superar 5 MB', 'error'); return }
  archivoSeleccionado.value = f
}

// FIX CRÍTICO: no depender de que el pago exista en el historial.
// Si hay pagoId → usa el endpoint por pagoId (normal).
// Si no hay pago → usa endpoint por ventaId que crea el pago en el backend.
async function enviarComprobante() {
  if (!archivoSeleccionado.value || !venta.value) return

  try {
    enviandoPago.value = true
    const token = localStorage.getItem('authToken')

    const fd = new FormData()
    fd.append('Archivo',           archivoSeleccionado.value)
    fd.append('MetodoPago',        '0')  // Enum MetodoPago.Transferencia = 0
    fd.append('NumeroTransaccion', formPago.value.numeroTransaccion || '')

    // Buscar pago: rechazado primero (reintento), luego pendiente, luego el primero
    const pagoTarget = venta.value.pagos?.find(p => p.estado === 'Rechazado')
                    ?? venta.value.pagos?.find(p => p.estado === 'Pendiente')
                    ?? venta.value.pagos?.[0]

    const url = pagoTarget?.pagoId
      ? `/api/PagoApi/${pagoTarget.pagoId}/comprobante`       // pago existe → update
      : `/api/PagoApi/venta/${venta.value.ventaId}/comprobante` // sin pago → create+update

    const resp = await fetch(url, {
      method:  'POST',
      headers: { 'Authorization': `Bearer ${token}`, 'X-Requested-With': 'XMLHttpRequest' },
      body:    fd,
    })

    if (!resp.ok) {
      const e = await resp.json().catch(() => ({}))
      throw new Error(e.message || `Error ${resp.status}`)
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

// ═══════════════════════════════════════════════════════════════════════════
//  MODAL RECLAMO
// ═══════════════════════════════════════════════════════════════════════════
function abrirModalReclamo() {
  mostrarModalReclamo.value = true
  tipoReclamoSeleccionado.value = null
  descripcionReclamo.value = ''
}

function cerrarModalReclamo() {
  mostrarModalReclamo.value = false
  tipoReclamoSeleccionado.value = null
  descripcionReclamo.value = ''
}

async function enviarReclamo() {
  if (!tipoReclamoSeleccionado.value || !venta.value) return
  
  try {
    enviandoReclamo.value = true
    
    // Llamar al endpoint de crear disputa
    // Prioridad: 0=Baja, 1=Media, 2=Alta, 3=Urgente
    const tipoSeleccionado = tiposReclamo.find(t => t.value === tipoReclamoSeleccionado.value)
    await fetchWithAuth('/api/DisputaApi', {
      method: 'POST',
      body: JSON.stringify({
        ventaId: venta.value.ventaId,
        tipoStr: tipoReclamoSeleccionado.value,
        descripcion: descripcionReclamo.value || `Reclamo: ${tipoSeleccionado?.label || 'Sin especificar'}`,
        prioridad: 3,
        montoInvolucrado: venta.value.total
      })
    })
    
    mostrarToast('¡Reclamo enviado! El administrador lo revisará pronto.', 'success')
    cerrarModalReclamo()
    await cargarDetalle() // Recarga para ver el nuevo estado
    
  } catch (err) {
    mostrarToast(err.message || 'Error al enviar el reclamo', 'error')
  } finally {
    enviandoReclamo.value = false
  }
}

function verFactura() {
  const token = localStorage.getItem('authToken')
  const url = token
    ? `/Venta/Factura/${venta.value?.ventaId}?access_token=${token}`
    : `/Venta/Factura/${venta.value?.ventaId}`
  window.open(url, '_blank')
}

onMounted(() => { cargarDetalle() })
</script>

<style scoped>
.paso-circulo {
  width:28px; height:28px; font-size:.75rem; flex-shrink:0;
  border:2px solid #e2e8f0;
}
.paso-circulo.completado { background:#22c55e; border-color:#22c55e; color:white; }
.paso-circulo.activo     { background:#3b82f6; border-color:#3b82f6; color:white; }
.paso-circulo.pendiente  { background:#f1f5f9; border-color:#e2e8f0; color:#94a3b8; }
.modal-overlay {
  position:fixed; inset:0; background:rgba(0,0,0,.45);
  display:flex; align-items:center; justify-content:center;
  z-index:9999; padding:1rem; animation:fadeIn .15s ease;
}
.modal-box { animation:slideUp .2s ease; max-height:90vh; overflow-y:auto; }
.upload-area:hover    { border-color:#f97316 !important; background:#fff7ed; }
.upload-area.drag-over { border-color:#22c55e !important; background:#f0fdf4; }
@keyframes fadeIn  { from{opacity:0} to{opacity:1} }
@keyframes slideUp { from{transform:translateY(20px);opacity:0} to{transform:translateY(0);opacity:1} }
</style>