<template>
  <div class="comprador-dashboard">
    <!-- Navbar -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary sticky-top shadow">
      <div class="container-fluid">
        <a class="navbar-brand" href="#">
          <i class="fas fa-birthday-cake me-2"></i>Casa de las Tortas - Comprador
        </a>
        <div class="navbar-nav ms-auto d-flex flex-row align-items-center gap-3">
          <!-- Notificaciones -->
          <div class="dropdown">
            <button class="btn btn-outline-light btn-sm position-relative" data-bs-toggle="dropdown">
              <i class="fas fa-bell"></i>
              <span v-if="notificacionesNoLeidas > 0"
                    class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                    style="font-size:.6rem;">
                {{ notificacionesNoLeidas }}
              </span>
            </button>
            <ul class="dropdown-menu dropdown-menu-end shadow" style="min-width:300px;max-height:360px;overflow-y:auto;">
              <li class="px-3 py-2 d-flex justify-content-between align-items-center border-bottom">
                <span class="fw-semibold small">Notificaciones</span>
                <button v-if="notificaciones.length" class="btn btn-link btn-sm p-0 text-muted"
                        @click="marcarTodasLeidas">Marcar leídas</button>
              </li>
              <li v-if="!notificaciones.length" class="px-3 py-3 text-center text-muted small">
                Sin notificaciones
              </li>
              <li v-for="n in notificaciones" :key="n.id"
                  class="px-3 py-2 border-bottom"
                  :class="n.leida ? '' : 'bg-light'">
                <div class="small fw-semibold">{{ n.titulo }}</div>
                <div class="text-muted" style="font-size:.75rem;">{{ n.mensaje }}</div>
                <div class="text-muted" style="font-size:.68rem;">{{ n.hora }}</div>
              </li>
            </ul>
          </div>

          <!-- Indicador SignalR -->
          <span class="badge"
                :class="signalRConectado ? 'bg-success' : 'bg-secondary'"
                :title="signalRConectado ? 'Tiempo real activo' : 'Sin conexión en tiempo real'">
            <i :class="signalRConectado ? 'fas fa-wifi' : 'fas fa-wifi-slash'"></i>
          </span>

          <span class="navbar-text text-white d-flex align-items-center gap-2">
            <img :src="avatarUrl" class="rounded-circle border border-2 border-white"
                 style="width:32px;height:32px;object-fit:cover">
            {{ nombreUsuario }}
          </span>
          <button class="btn btn-outline-light btn-sm" @click="logout">
            <i class="fas fa-sign-out-alt me-1"></i>Salir
          </button>
        </div>
      </div>
    </nav>

    <div class="container-fluid mt-4">
      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3 col-lg-2 mb-4">
          <!-- Perfil mini -->
          <div class="card shadow-sm mb-3 text-center">
            <div class="card-body py-3">
              <img :src="avatarUrl" class="rounded-circle border mb-2"
                   style="width:64px;height:64px;object-fit:cover">
              <h6 class="mb-0">{{ nombreUsuario }}</h6>
              <small class="text-muted">{{ emailUsuario }}</small>
            </div>
          </div>

          <div class="card shadow-sm">
            <div class="card-body">
              <nav class="nav flex-column">
                <button v-for="item in menuItems" :key="item.vista"
                        class="btn mb-2 text-start"
                        :class="vistaActiva === item.vista ? 'btn-primary' : 'btn-outline-primary'"
                        @click="cambiarVista(item.vista)">
                  <i :class="item.icono + ' me-2'"></i>{{ item.label }}
                  <span v-if="item.vista === 'carrito' && contadorCarrito > 0"
                        class="badge bg-danger ms-2">{{ contadorCarrito }}</span>
                </button>
              </nav>
            </div>
          </div>
        </div>

        <!-- Contenido Principal -->
        <div class="col-md-9 col-lg-10">
          <div v-if="inicializando" class="card shadow-sm">
            <div class="card-body text-center py-5">
              <div class="spinner-border text-primary mb-3" style="width:3rem;height:3rem"></div>
              <h5 class="text-muted">Cargando dashboard...</h5>
            </div>
          </div>

          <template v-else>
            <DetalleVenta v-if="ventaSeleccionada" :venta-id="ventaSeleccionada"
                          @cerrar="ventaSeleccionada = null" />

            <template v-else>
              <DashboardComprador v-if="vistaActiva === 'dashboard'"
                                  @cambiar-vista="cambiarVista"
                                  @ver-detalle="verDetalle" />
              <CatalogoComprador v-else-if="vistaActiva === 'catalogo'"
                                 @actualizar-carrito="actualizarContadorCarrito"
                                 @ir-carrito="cambiarVista('carrito')" />
              <HistorialCompras v-else-if="vistaActiva === 'historial'"
                                @cambiar-vista="cambiarVista"
                                @ver-detalle="verDetalle" />
              <CarritoCompras v-else-if="vistaActiva === 'carrito'"
                              @cambiar-vista="cambiarVista"
                              @actualizar-carrito="actualizarContadorCarrito" />
              
              <!--  Checkout completo en Vue -->
              <CheckoutCompras v-else-if="vistaActiva === 'checkout'"
                               @cambiar-vista="cambiarVista"
                               @actualizar-carrito="actualizarContadorCarrito"
                               @ver-detalle="verDetalle" />
              
              <PerfilComprador v-else-if="vistaActiva === 'perfil'" />
            </template>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { fetchWithAuth, clearAuth } from '@shared/apiUtils.js'
import DashboardComprador from './components/DashboardComprador.vue'
import CatalogoComprador  from './components/CatalogoComprador.vue'
import HistorialCompras   from './components/HistorialCompras.vue'
import DetalleVenta       from './components/DetalleVenta.vue'
import CarritoCompras     from './components/CarritoCompras.vue'
import PerfilComprador    from './components/PerfilComprador.vue'
import CheckoutCompras    from './components/CheckoutCompras.vue'  

function generarAvatarUrl(nombre) {
  if (!nombre || !nombre.trim()) nombre = 'U'
  const encoded = encodeURIComponent(nombre.trim())
  return `https://ui-avatars.com/api/?name=${encoded}&background=random&color=fff&size=150&bold=true&format=svg`
}

const vistaActiva            = ref('dashboard')
const ventaSeleccionada      = ref(null)
const nombreUsuario          = ref('...')
const emailUsuario           = ref('')
const avatarUsuario          = ref(null)
const contadorCarrito        = ref(0)
const inicializando          = ref(true)
const signalRConectado       = ref(false)
const notificaciones         = ref([])
const notificacionesNoLeidas = ref(0)
let   hubConnection          = null
let   notifId                = 0

const avatarUrl = computed(() => {
  if (avatarUsuario.value) return avatarUsuario.value
  return generarAvatarUrl(nombreUsuario.value)
})

const menuItems = [
  { vista: 'dashboard', label: 'Dashboard',  icono: 'fas fa-home' },
  { vista: 'catalogo',  label: 'Catálogo',   icono: 'fas fa-store' },
  { vista: 'carrito',   label: 'Mi Carrito', icono: 'fas fa-shopping-cart' },
  { vista: 'historial', label: 'Historial',  icono: 'fas fa-history' },
  { vista: 'perfil',    label: 'Mi Perfil',  icono: 'fas fa-user' },
]

function agregarNotificacion(titulo, mensaje) {
  const hora = new Date().toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })
  notificaciones.value.unshift({ id: ++notifId, titulo, mensaje, hora, leida: false })
  notificacionesNoLeidas.value++
  if (notificaciones.value.length > 20)
    notificaciones.value.pop()
}

function marcarTodasLeidas() {
  notificaciones.value.forEach(n => n.leida = true)
  notificacionesNoLeidas.value = 0
}

async function conectarSignalR(token) {
  try {
    const signalR = await import('@microsoft/signalr')
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    hubConnection.on('PedidoListo', (data) => {
      const lineas = [data.mensaje || `Tu pedido #${data.numeroOrden} está listo.`]
      if (data.nombreVendedor) lineas.push(`📍 Retirá en: ${data.nombreVendedor}`)
      if (data.direccionRetiro) lineas.push(`🗺 ${data.direccionRetiro}`)
      if (data.horarioRetiro)  lineas.push(`🕐 Horario: ${data.horarioRetiro}`)
      agregarNotificacion('🎉 ¡Pedido listo para retirar!', lineas.join(' — '))
    })

    hubConnection.on('PagoVerificado', (data) => {
      agregarNotificacion(
        '✅ Pago verificado',
        data.mensaje || `Tu pago para la orden ${data.numeroOrden} fue aprobado.`
      )
    })

    hubConnection.on('PagoRechazado', (data) => {
      const motivo = data.motivo ? ` — Motivo: ${data.motivo}` : ''
      agregarNotificacion(
        '❌ Comprobante rechazado',
        (data.mensaje || `Tu comprobante de la orden ${data.numeroOrden} fue rechazado`) + motivo
      )
    })

    hubConnection.on('ReembolsoPendiente', (data) => {
      agregarNotificacion(
        '💰 Reembolso en proceso',
        data.mensaje || `Tu pago de la orden ${data.numeroOrden} será reembolsado.`
      )
    })

    hubConnection.onreconnecting(() => { signalRConectado.value = false })
    hubConnection.onreconnected(() =>  { signalRConectado.value = true  })
    hubConnection.onclose(() =>        { signalRConectado.value = false })

    await hubConnection.start()
    signalRConectado.value = true
  } catch (err) {
    console.warn('SignalR no disponible:', err.message)
    signalRConectado.value = false
  }
}

async function inicializar() {
  try {
    const token = localStorage.getItem('authToken')
    if (!token) { redirigirLogin(); return }

    const data = await fetchWithAuth('/api/AuthApi/me')
    if (data?.user) {
      nombreUsuario.value = data.user.nombre
      emailUsuario.value  = data.user.email || ''
      avatarUsuario.value = data.user.avatar || null

      const user = JSON.parse(localStorage.getItem('user') || '{}')
      user.compradorId = data.user.rolData?.compradorId ?? data.user.id
      user.avatar      = data.user.avatar || null
      user.nombre      = data.user.nombre || ''
      localStorage.setItem('user', JSON.stringify(user))
    }

    await actualizarContadorCarrito()
    await conectarSignalR(token)
  } catch (err) {
    console.error('Error inicializando comprador:', err)
    if (err.message.includes('401')) redirigirLogin()
  } finally {
    inicializando.value = false
  }
}

async function actualizarContadorCarrito(total) {
  if (total !== undefined) { contadorCarrito.value = total; return }
  try {
    const data = await fetchWithAuth('/Carrito/Contador')
    contadorCarrito.value = data?.total ?? 0
  } catch {}
}

function cambiarVista(vista) {
  vistaActiva.value = vista
  ventaSeleccionada.value = null
}

function verDetalle(ventaId) { ventaSeleccionada.value = ventaId }

function logout() {
  if (!confirm('¿Cerrar sesión?')) return
  clearAuth()
  window.location.href = '/Account/Logout'
}

function redirigirLogin() {
  clearAuth()
  window.location.href = '/Account/Login'
}

onMounted(inicializar)
</script>