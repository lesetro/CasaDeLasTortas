<template>
  <div class="vendedor-dashboard">

    <!-- ── NAVBAR ── -->
    <nav class="navbar navbar-expand-lg navbar-dark sticky-top shadow"
         style="background:linear-gradient(90deg,#166534,#15803d);">
      <div class="container-fluid">
        <a class="navbar-brand fw-bold" href="#">
          <i class="fas fa-store me-2"></i>
          <span class="d-none d-sm-inline">Casa de las Tortas</span>
          <span class="d-sm-none">CDT</span>
          <span class="ms-2 opacity-60 small fw-normal d-none d-md-inline">Vendedor</span>
        </a>

        <div class="navbar-nav ms-auto d-flex flex-row align-items-center gap-2">

          <!-- Alerta cobros pendientes -->
          <button v-if="cobrosPendientes > 0"
                  class="btn btn-warning btn-sm fw-semibold"
                  @click="cambiarVista('cobros')"
                  title="Tenés cobros pendientes">
            <i class="fas fa-hand-holding-usd me-1"></i>
            <span class="d-none d-md-inline">Cobros: </span>
            {{ cobrosPendientes }}
          </button>

          <!-- Notificaciones -->
          <div class="position-relative">
            <button class="btn btn-outline-light btn-sm position-relative"
                    @click="toggleNotificaciones" title="Notificaciones">
              <i class="fas fa-bell"></i>
              <span v-if="notificacionesNoLeidas > 0"
                    class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                    style="font-size:.58rem;">
                {{ notificacionesNoLeidas }}
              </span>
            </button>

            <!-- Panel notificaciones -->
            <div v-if="panelNotifAbierto"
                 class="position-absolute end-0 mt-2 bg-white shadow rounded-3 border"
                 style="width:320px;z-index:1050;max-height:380px;overflow-y:auto;">
              <div class="d-flex justify-content-between align-items-center p-3 border-bottom">
                <span class="fw-bold small">Notificaciones</span>
                <button class="btn btn-link btn-sm p-0 text-muted small" @click="marcarTodasLeidas">
                  Marcar todas leídas
                </button>
              </div>
              <div v-if="notificaciones.length === 0" class="text-center py-3 text-muted small">
                Sin notificaciones
              </div>
              <div v-for="(notif, idx) in notificaciones" :key="idx"
                   class="p-2 border-bottom small"
                   :class="notif.leida ? 'bg-white' : 'bg-light'">
                <div class="fw-semibold">{{ notif.message }}</div>
                <div class="text-muted" style="font-size:.7rem;">{{ formatHora(notif.timestamp) }}</div>
              </div>
            </div>
          </div>

          <!-- Estado SignalR -->
          <span class="badge"
                :class="signalRConectado ? 'bg-success' : 'bg-secondary'"
                :title="signalRConectado ? 'Conectado' : 'Sin conexión en tiempo real'">
            <i :class="signalRConectado ? 'fas fa-wifi' : 'fas fa-wifi-slash'"></i>
          </span>

          <!-- Avatar + nombre -->
          <span class="navbar-text text-white d-flex align-items-center gap-2">
            <img :src="avatarUrl" class="rounded-circle border border-2 border-white"
                 style="width:30px;height:30px;object-fit:cover;" />
            <span class="d-none d-lg-inline small">{{ nombreComercial }}</span>
          </span>

          <button class="btn btn-outline-light btn-sm" @click="logout">
            <i class="fas fa-sign-out-alt"></i>
          </button>
        </div>
      </div>
    </nav>

    <!-- Toast tiempo real -->
    <div v-if="toastNotif" class="position-fixed bottom-0 end-0 p-3" style="z-index:1100;">
      <div class="toast show align-items-center text-white bg-success border-0 rounded-3 shadow">
        <div class="d-flex">
          <div class="toast-body small">
            <i class="fas fa-bell me-2"></i>{{ toastNotif }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto"
                  @click="toastNotif = null"></button>
        </div>
      </div>
    </div>

    <!-- ── LAYOUT ── -->
    <div class="container-fluid mt-4">
      <div class="row">

        <!-- Sidebar -->
        <div class="col-md-3 col-lg-2 mb-4">

          <!-- Mini perfil -->
          <div class="card border-0 shadow-sm rounded-3 mb-3 text-center">
            <div class="card-body py-3">
              <img :src="avatarUrl" class="rounded-circle border mb-2"
                   style="width:60px;height:60px;object-fit:cover;" />
              <h6 class="mb-0 fw-bold small">{{ nombreComercial }}</h6>
              <small class="text-muted" style="font-size:.72rem;">{{ emailUsuario }}</small>
            </div>
          </div>

          <!-- Menú -->
          <div class="card border-0 shadow-sm rounded-3 mb-3">
            <div class="card-body p-2">
              <nav class="nav flex-column gap-1">
                <button v-for="item in menuItems" :key="item.vista"
                        class="btn text-start px-3 py-2 rounded-2 d-flex align-items-center gap-2"
                        :class="vistaActiva === item.vista
                          ? 'btn-success text-white fw-semibold'
                          : 'btn-light text-secondary'"
                        @click="cambiarVista(item.vista)">
                  <i :class="item.icono" style="width:16px; text-align:center;"></i>
                  <span class="flex-grow-1 small">{{ item.label }}</span>
                  <!-- Badge pedidos pendientes -->
                  <span v-if="item.vista === 'pedidos' && pedidosPendientes > 0"
                        class="badge bg-warning text-dark rounded-pill"
                        style="font-size:.6rem;">{{ pedidosPendientes }}</span>
                  <!-- Badge cobros pendientes -->
                  <span v-if="item.vista === 'cobros' && cobrosPendientes > 0"
                        class="badge bg-danger rounded-pill"
                        style="font-size:.6rem;">{{ cobrosPendientes }}</span>
                </button>
              </nav>
            </div>
          </div>

          <!-- Resumen rápido -->
          <div class="card border-0 shadow-sm rounded-3">
            <div class="card-body p-3">
              <div class="text-muted small fw-semibold mb-2" style="font-size:.72rem; text-transform:uppercase; letter-spacing:.06em;">Resumen</div>
              <div class="d-flex justify-content-between mb-2 small">
                <span class="text-muted">Tortas activas</span>
                <strong class="text-success">{{ resumen.tortasActivas }}</strong>
              </div>
              <div class="d-flex justify-content-between mb-2 small">
                <span class="text-muted">Pedidos pend.</span>
                <strong class="text-warning">{{ pedidosPendientes }}</strong>
              </div>
              <div class="d-flex justify-content-between mb-2 small">
                <span class="text-muted">Ingresos mes</span>
                <strong class="text-success">{{ formatMoneda(resumen.ingresosMes) }}</strong>
              </div>
              <div class="d-flex justify-content-between small">
                <span class="text-muted">Por cobrar</span>
                <strong class="text-primary">{{ formatMoneda(resumen.pendienteCobro) }}</strong>
              </div>
            </div>
          </div>
        </div>

        <!-- Contenido principal -->
        <div class="col-md-9 col-lg-10">
          <div v-if="inicializando" class="card border-0 shadow-sm rounded-3">
            <div class="card-body text-center py-5">
              <div class="spinner-border text-success mb-3" style="width:3rem;height:3rem;"></div>
              <h6 class="text-muted">Cargando dashboard...</h6>
            </div>
          </div>

          <template v-else>
            <EstadisticasVendedor v-if="vistaActiva === 'estadisticas'"
                                  ref="estadisticasRef" />
            <MisTortas v-else-if="vistaActiva === 'tortas'"
                       ref="tortasRef"
                       @recargar-estadisticas="recargarEstadisticas" />
            <MisPedidos v-else-if="vistaActiva === 'pedidos'"
                        ref="pedidosRef" />
            <!-- ── NUEVA VISTA: Mis Cobros ── -->
            <MisLiberaciones v-else-if="vistaActiva === 'cobros'"
                             ref="liberacionesRef" />
            <PerfilVendedor v-else-if="vistaActiva === 'perfil'" />
          </template>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { fetchWithAuth, formatMoneda, formatHora, generarAvatarUrl } from '@shared/apiUtils.js'
import EstadisticasVendedor from './components/EstadisticasVendedor.vue'
import MisTortas            from './components/MisTortas.vue'
import MisPedidos           from './components/MisPedidos.vue'
import MisLiberaciones      from './components/MisLiberaciones.vue'   // ← NUEVO
import PerfilVendedor       from './components/PerfilVendedor.vue'

// ── Estado principal ──────────────────────────────────────────────
const vistaActiva       = ref('estadisticas')
const nombreComercial   = ref('...')
const emailUsuario      = ref('')
const avatarUsuario     = ref(null)
const nombreUsuario     = ref('')
const pedidosPendientes = ref(0)
const cobrosPendientes  = ref(0)   // ← NUEVO
const resumen           = ref({ tortasActivas: 0, ingresosMes: 0, pendienteCobro: 0 })
const inicializando     = ref(true)

const estadisticasRef = ref(null)
const tortasRef       = ref(null)
const pedidosRef      = ref(null)
const liberacionesRef = ref(null)   // ← NUEVO

const avatarUrl = computed(() => {
  if (avatarUsuario.value) return avatarUsuario.value
  return generarAvatarUrl(nombreUsuario.value || nombreComercial.value)
})

// ── Menú — incluye "Mis Cobros" ───────────────────────────────────
const menuItems = [
  { vista: 'estadisticas', label: 'Dashboard',   icono: 'fas fa-chart-line' },
  { vista: 'tortas',       label: 'Mis Tortas',  icono: 'fas fa-birthday-cake' },
  { vista: 'pedidos',      label: 'Pedidos',     icono: 'fas fa-shopping-bag' },
  { vista: 'cobros',       label: 'Mis Cobros',  icono: 'fas fa-hand-holding-usd' },  // ← NUEVO
  { vista: 'perfil',       label: 'Mi Perfil',   icono: 'fas fa-user' },
]

// ── SignalR ───────────────────────────────────────────────────────
const signalRConectado       = ref(false)
const notificaciones         = ref([])
const notificacionesNoLeidas = ref(0)
const panelNotifAbierto      = ref(false)
const toastNotif             = ref(null)
let   hubConnection          = null
let   toastTimer             = null

// ── Inicialización ───────────────────────────────────────────────
async function inicializar() {
  try {
    const token = localStorage.getItem('authToken')
    if (!token) { redirigirLogin(); return }

    const data = await fetchWithAuth('/api/AuthApi/me')
    if (!data?.user) { redirigirLogin(); return }

    const u = data.user
    nombreComercial.value = u.rolData?.nombreComercial || u.nombre
    emailUsuario.value    = u.email  || ''
    nombreUsuario.value   = u.nombre || ''
    avatarUsuario.value   = u.avatar || null

    const user = JSON.parse(localStorage.getItem('user') || '{}')
    user.vendedorId = u.rolData?.vendedorId ?? u.id
    user.avatar     = u.avatar || null
    user.nombre     = u.nombre || ''
    localStorage.setItem('user', JSON.stringify(user))

    await cargarResumen(user.vendedorId)
    await conectarSignalR(token)
  } catch (err) {
    console.error('Error inicializando vendedor:', err)
    if (err.message?.includes('401')) redirigirLogin()
  } finally {
    inicializando.value = false
  }
}

async function cargarResumen(vendedorId) {
  try {
    const [stats, libs] = await Promise.all([
      fetchWithAuth(`/api/VendedorApi/${vendedorId}/estadisticas`),
      fetchWithAuth(`/api/LiberacionApi/vendedor/${vendedorId}?pagina=1&registrosPorPagina=100`).catch(() => ({ data: [] })),
    ])
    resumen.value.tortasActivas = stats.totalTortasActivas ?? 0
    resumen.value.ingresosMes   = stats.ingresosMes        ?? 0
    pedidosPendientes.value     = stats.pedidosPendientes  ?? 0

    // Calcular cobros pendientes
    const listaLibs = libs.data ?? libs ?? []
    const pendiente = listaLibs.filter(l => l.estado === 'Pendiente').reduce((s, l) => s + (l.montoLiberado ?? 0), 0)
    resumen.value.pendienteCobro = pendiente
    cobrosPendientes.value = listaLibs.filter(l => l.estado === 'Pendiente').length
  } catch {}
}

// ── SignalR ───────────────────────────────────────────────────────
async function conectarSignalR(token) {
  try {
    const signalR = await import('@microsoft/signalr')
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    hubConnection.on('NuevoPedido', (data) => {
      pedidosPendientes.value++
      agregarNotificacion(data.message || `Nuevo pedido #${data.numeroOrden}`)
      if (vistaActiva.value === 'pedidos') pedidosRef.value?.cargarPedidos?.()
    })

    hubConnection.on('PagoConfirmado', (data) => {
      agregarNotificacion(data.mensaje || 'Pago confirmado por el admin')
    })

    // ← NUEVO: escuchar liberación de fondos
    hubConnection.on('FondosLiberados', (data) => {
      cobrosPendientes.value = Math.max(0, cobrosPendientes.value - 1)
      agregarNotificacion(data.mensaje || `💸 Fondos liberados: ${formatMoneda(data.monto)}`)
      if (vistaActiva.value === 'cobros') liberacionesRef.value?.cargarLiberaciones?.()
      if (vistaActiva.value === 'estadisticas') estadisticasRef.value?.cargarEstadisticas?.()
    })

    hubConnection.on('StockBajo', (data) => {
      agregarNotificacion(data.message || `Stock bajo: ${data.nombreTorta}`)
    })

    hubConnection.onreconnecting(() => { signalRConectado.value = false })
    hubConnection.onreconnected(() => { signalRConectado.value = true })
    hubConnection.onclose(() =>    { signalRConectado.value = false })

    await hubConnection.start()
    signalRConectado.value = true
  } catch (err) {
    console.warn('SignalR no disponible:', err.message)
    signalRConectado.value = false
  }
}

function agregarNotificacion(mensaje) {
  notificaciones.value.unshift({ message: mensaje, timestamp: new Date().toISOString(), leida: false })
  notificacionesNoLeidas.value++
  clearTimeout(toastTimer)
  toastNotif.value = mensaje
  toastTimer = setTimeout(() => { toastNotif.value = null }, 5000)
}

function toggleNotificaciones() {
  panelNotifAbierto.value = !panelNotifAbierto.value
  if (panelNotifAbierto.value) marcarTodasLeidas()
}
function marcarTodasLeidas() {
  notificaciones.value.forEach(n => { n.leida = true })
  notificacionesNoLeidas.value = 0
}

function recargarEstadisticas() { estadisticasRef.value?.cargarEstadisticas?.() }

function cambiarVista(vista) {
  vistaActiva.value       = vista
  panelNotifAbierto.value = false
}

function logout() {
  if (!confirm('¿Cerrar sesión?')) return
  hubConnection?.stop()
  localStorage.removeItem('authToken')
  localStorage.removeItem('user')
  window.location.href = '/Account/Logout'
}

function redirigirLogin() {
  hubConnection?.stop()
  localStorage.removeItem('authToken')
  localStorage.removeItem('user')
  window.location.href = '/Account/Login'
}

onMounted(inicializar)
onUnmounted(() => { hubConnection?.stop(); clearTimeout(toastTimer) })
</script>