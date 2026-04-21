<template>
  <div class="checkout-compras">
    
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- PASO 1: RESUMEN DEL CARRITO Y CONFIRMAR COMPRA -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-if="paso === 1" class="card border-0 shadow-sm rounded-3">
      <div class="card-header bg-white border-0 px-4 pt-4 pb-2">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-shopping-bag text-primary me-2"></i>Confirmar Compra
        </h5>
        <p class="text-muted small mb-0 mt-1">Revisá tu pedido antes de continuar</p>
      </div>
      
      <div class="card-body px-4 pb-4">
        <!-- Loading -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary"></div>
          <p class="text-muted mt-2">Cargando carrito...</p>
        </div>

        <!-- Carrito vacío -->
        <div v-else-if="!carrito || carrito.items?.length === 0" class="text-center py-5">
          <i class="fas fa-shopping-cart fa-3x text-muted mb-3"></i>
          <h6>Tu carrito está vacío</h6>
          <button class="btn btn-primary mt-3" @click="$emit('cambiar-vista', 'catalogo')">
            <i class="fas fa-store me-2"></i>Ir al Catálogo
          </button>
        </div>

        <!-- Contenido del carrito -->
        <div v-else>
          <!-- Lista de items -->
          <div class="mb-4">
            <div v-for="item in carrito.items" :key="item.tortaId"
                 class="d-flex gap-3 p-3 border-bottom align-items-center">
              <img :src="item.imagenPrincipal || '/images/torta-default.jpg'"
                   :alt="item.nombre"
                   class="rounded" style="width:60px;height:60px;object-fit:cover;">
              <div class="flex-grow-1">
                <div class="fw-semibold">{{ item.nombre }}</div>
                <small class="text-muted">{{ item.nombreVendedor }}</small>
              </div>
              <div class="text-end">
                <div class="fw-semibold">${{ formatNumber(item.subtotal) }}</div>
                <small class="text-muted">x{{ item.cantidad }}</small>
              </div>
            </div>
          </div>

          <!-- Totales -->
          <div class="bg-light rounded-3 p-3 mb-4">
            <div class="d-flex justify-content-between mb-2">
              <span>Subtotal</span>
              <span>${{ formatNumber(carrito.subtotal) }}</span>
            </div>
            <div v-if="carrito.descuentoTotal > 0" class="d-flex justify-content-between mb-2 text-success">
              <span>Descuento</span>
              <span>-${{ formatNumber(carrito.descuentoTotal) }}</span>
            </div>
            <hr class="my-2">
            <div class="d-flex justify-content-between fw-bold fs-5">
              <span>Total a pagar</span>
              <span class="text-success">${{ formatNumber(carrito.total) }}</span>
            </div>
          </div>

          <!-- Dirección de entrega -->
          <div class="mb-4">
            <label class="form-label fw-semibold">
              <i class="fas fa-map-marker-alt me-1 text-danger"></i>Dirección de entrega/retiro
            </label>
            <textarea v-model="direccionEntrega" class="form-control" rows="2"
                      placeholder="Ej: Av. San Martín 1234, Villa Mercedes"></textarea>
          </div>

          <!-- Notas -->
          <div class="mb-4">
            <label class="form-label fw-semibold">
              <i class="fas fa-comment me-1 text-info"></i>Notas adicionales (opcional)
            </label>
            <textarea v-model="notas" class="form-control" rows="2"
                      placeholder="Ej: Llamar antes de entregar, sin TACC, etc."></textarea>
          </div>

          <!-- Botón confirmar -->
          <button class="btn btn-success btn-lg w-100 fw-bold" 
                  @click="crearVenta" :disabled="creandoVenta || !direccionEntrega">
            <span v-if="creandoVenta" class="spinner-border spinner-border-sm me-2"></span>
            <i v-else class="fas fa-check me-2"></i>
            Confirmar Pedido
          </button>

          <div v-if="error" class="alert alert-danger mt-3 mb-0">
            <i class="fas fa-exclamation-circle me-2"></i>{{ error }}
          </div>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- PASO 2: DATOS DE PAGO Y SUBIR COMPROBANTE -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-else-if="paso === 2" class="card border-0 shadow-sm rounded-3">
      <div class="card-header bg-success text-white px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-check-circle me-2"></i>¡Pedido Creado!
        </h5>
        <p class="mb-0 mt-1 opacity-75">Orden #{{ ventaCreada?.numeroOrden }}</p>
      </div>

      <div class="card-body px-4 pb-4">
        <!-- Instrucciones -->
        <div class="alert alert-info mb-4">
          <i class="fas fa-info-circle me-2"></i>
          <strong>Siguiente paso:</strong> Transferí el monto y subí el comprobante.
        </div>

        <!-- Datos bancarios -->
        <div class="bg-light rounded-3 p-4 mb-4">
          <h6 class="fw-bold mb-3">
            <i class="fas fa-university me-2 text-primary"></i>Datos para transferir
          </h6>
          
          <div v-if="datosBancarios" class="row g-3">
            <div class="col-12">
              <label class="small text-muted">Monto a transferir</label>
              <div class="fs-3 fw-bold text-success">${{ formatNumber(ventaCreada?.total || carrito?.total) }}</div>
            </div>
            <div class="col-md-6">
              <label class="small text-muted">Alias CBU</label>
              <div class="fw-semibold font-monospace fs-5 user-select-all" 
                   @click="copiar(datosBancarios.alias)">
                {{ datosBancarios.alias }}
                <i class="fas fa-copy ms-2 text-primary" style="cursor:pointer;"></i>
              </div>
            </div>
            <div class="col-md-6">
              <label class="small text-muted">CBU</label>
              <div class="fw-semibold font-monospace small user-select-all"
                   @click="copiar(datosBancarios.cbu)">
                {{ datosBancarios.cbu }}
                <i class="fas fa-copy ms-2 text-primary" style="cursor:pointer;"></i>
              </div>
            </div>
            <div class="col-md-6">
              <label class="small text-muted">Banco</label>
              <div class="fw-semibold">{{ datosBancarios.banco }}</div>
            </div>
            <div class="col-md-6">
              <label class="small text-muted">Titular</label>
              <div class="fw-semibold">{{ datosBancarios.titular }}</div>
            </div>
          </div>
          <div v-else class="text-center py-3">
            <div class="spinner-border spinner-border-sm"></div>
            <span class="ms-2">Cargando datos...</span>
          </div>
        </div>

        <!-- QR si existe -->
        <div v-if="datosBancarios?.imagenQR" class="text-center mb-4">
          <img :src="datosBancarios.imagenQR" alt="QR de pago" 
               class="img-fluid rounded shadow-sm" style="max-width:200px;">
          <p class="text-muted small mt-2">Escaneá el QR para pagar</p>
        </div>

        <hr class="my-4">

        <!-- Subir comprobante -->
        <h6 class="fw-bold mb-3">
          <i class="fas fa-upload me-2 text-warning"></i>Subir Comprobante
        </h6>

        <div class="mb-3">
          <label class="form-label">Número de operación (opcional)</label>
          <input type="text" v-model="numeroTransaccion" class="form-control"
                 placeholder="Ej: 123456789">
        </div>

        <div class="mb-3">
          <label class="form-label">Comprobante (imagen o PDF)</label>
          <input type="file" ref="inputComprobante" class="form-control" 
                 accept="image/*,.pdf" @change="seleccionarArchivo">
          <small v-if="archivoSeleccionado" class="text-success">
            <i class="fas fa-check me-1"></i>{{ archivoSeleccionado.name }}
          </small>
        </div>

        <!-- Preview de imagen -->
        <div v-if="previewImagen" class="mb-3 text-center">
          <img :src="previewImagen" class="img-fluid rounded shadow-sm" style="max-height:200px;">
        </div>

        <button class="btn btn-warning btn-lg w-100 fw-bold" 
                @click="subirComprobante" 
                :disabled="subiendoComprobante || !archivoSeleccionado">
          <span v-if="subiendoComprobante" class="spinner-border spinner-border-sm me-2"></span>
          <i v-else class="fas fa-paper-plane me-2"></i>
          Enviar Comprobante
        </button>

        <div v-if="errorComprobante" class="alert alert-danger mt-3 mb-0">
          <i class="fas fa-exclamation-circle me-2"></i>{{ errorComprobante }}
        </div>

        <!-- Opción de subir después -->
        <div class="text-center mt-4">
          <button class="btn btn-link text-muted" @click="irAHistorial">
            Subir comprobante más tarde →
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- PASO 3: CONFIRMACIÓN FINAL -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-else-if="paso === 3" class="card border-0 shadow-sm rounded-3">
      <div class="card-body text-center py-5 px-4">
        <div class="mb-4">
          <i class="fas fa-check-circle text-success" style="font-size:4rem;"></i>
        </div>
        <h4 class="fw-bold mb-2">¡Comprobante Enviado!</h4>
        <p class="text-muted mb-4">
          Tu pago está siendo verificado. Te notificaremos por email cuando sea aprobado.
        </p>
        
        <div class="bg-light rounded-3 p-3 mb-4 text-start">
          <div class="d-flex justify-content-between mb-2">
            <span class="text-muted">Número de orden:</span>
            <span class="fw-bold">#{{ ventaCreada?.numeroOrden }}</span>
          </div>
          <div class="d-flex justify-content-between mb-2">
            <span class="text-muted">Total:</span>
            <span class="fw-bold text-success">${{ formatNumber(ventaCreada?.total) }}</span>
          </div>
          <div class="d-flex justify-content-between">
            <span class="text-muted">Estado:</span>
            <span class="badge bg-warning">En verificación</span>
          </div>
        </div>

        <div class="d-grid gap-2">
          <button class="btn btn-primary" @click="irAHistorial">
            <i class="fas fa-history me-2"></i>Ver Mis Compras
          </button>
          <button class="btn btn-outline-secondary" @click="$emit('cambiar-vista', 'catalogo')">
            <i class="fas fa-store me-2"></i>Seguir Comprando
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchWithAuth, mostrarToast } from '@shared/apiUtils.js'

const emit = defineEmits(['cambiar-vista', 'actualizar-carrito', 'ver-detalle'])

// ══════════════════════════════════════════════════════════════════════════
// ESTADO
// ══════════════════════════════════════════════════════════════════════════
const paso = ref(1)
const loading = ref(true)
const error = ref('')

// Paso 1: Carrito
const carrito = ref(null)
const direccionEntrega = ref('')
const notas = ref('')
const creandoVenta = ref(false)

// Paso 2: Pago
const ventaCreada = ref(null)
const datosBancarios = ref(null)
const numeroTransaccion = ref('')
const archivoSeleccionado = ref(null)
const previewImagen = ref(null)
const subiendoComprobante = ref(false)
const errorComprobante = ref('')
const inputComprobante = ref(null)

// ══════════════════════════════════════════════════════════════════════════
// FUNCIONES - PASO 1
// ══════════════════════════════════════════════════════════════════════════
async function cargarCarrito() {
  try {
    loading.value = true
    error.value = ''
    
    const data = await fetchWithAuth('/api/CarritoApi')
    carrito.value = data
    
    // Cargar dirección del perfil si existe
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    if (user.compradorId) {
      try {
        const perfil = await fetchWithAuth(`/api/CompradorApi/${user.compradorId}/perfil`)
        if (perfil?.direccion) {
          direccionEntrega.value = perfil.direccion
          if (perfil.ciudad) direccionEntrega.value += `, ${perfil.ciudad}`
        }
      } catch {}
    }
  } catch (err) {
    console.error('Error cargando carrito:', err)
    error.value = 'Error al cargar el carrito'
  } finally {
    loading.value = false
  }
}

async function crearVenta() {
  if (!direccionEntrega.value.trim()) {
    error.value = 'Ingresá una dirección de entrega'
    return
  }

  try {
    creandoVenta.value = true
    error.value = ''

    const response = await fetchWithAuth('/api/VentaApi/crear-desde-carrito', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        direccionEntrega: direccionEntrega.value,
        notas: notas.value
      })
    })

    ventaCreada.value = response
    emit('actualizar-carrito', 0) // Vaciar contador del carrito
    
    // Cargar datos bancarios y pasar al paso 2
    await cargarDatosBancarios()
    paso.value = 2
    
    mostrarToast('¡Pedido creado exitosamente!', 'success')
  } catch (err) {
    console.error('Error creando venta:', err)
    error.value = err.message || 'Error al crear el pedido'
  } finally {
    creandoVenta.value = false
  }
}

// ══════════════════════════════════════════════════════════════════════════
// FUNCIONES - PASO 2
// ══════════════════════════════════════════════════════════════════════════
async function cargarDatosBancarios() {
  try {
    const data = await fetchWithAuth('/api/ConfiguracionApi/datos-pago')
    datosBancarios.value = data
  } catch {
    datosBancarios.value = {
      alias: 'casadelastortas.pagos',
      cbu: '0000000000000000000000',
      banco: 'Banco Nación',
      titular: 'Casa de las Tortas'
    }
  }
}

function seleccionarArchivo(event) {
  const file = event.target.files[0]
  if (file) {
    archivoSeleccionado.value = file
    
    // Preview si es imagen
    if (file.type.startsWith('image/')) {
      const reader = new FileReader()
      reader.onload = (e) => {
        previewImagen.value = e.target.result
      }
      reader.readAsDataURL(file)
    } else {
      previewImagen.value = null
    }
  }
}

async function subirComprobante() {
  if (!archivoSeleccionado.value) {
    errorComprobante.value = 'Seleccioná un archivo'
    return
  }

  try {
    subiendoComprobante.value = true
    errorComprobante.value = ''

    const formData = new FormData()
    formData.append('VentaId', ventaCreada.value.id || ventaCreada.value.ventaId)
    formData.append('Comprobante', archivoSeleccionado.value)
    if (numeroTransaccion.value) {
      formData.append('NumeroTransaccion', numeroTransaccion.value)
    }

    const token = localStorage.getItem('authToken')
    const response = await fetch('/api/PagoApi/subir-comprobante', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Requested-With': 'XMLHttpRequest'
      },
      body: formData
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Error al subir comprobante')
    }

    paso.value = 3
    mostrarToast('¡Comprobante enviado!', 'success')
  } catch (err) {
    console.error('Error subiendo comprobante:', err)
    errorComprobante.value = err.message || 'Error al subir el comprobante'
  } finally {
    subiendoComprobante.value = false
  }
}

function copiar(texto) {
  navigator.clipboard.writeText(texto)
  mostrarToast('Copiado al portapapeles', 'success')
}

function irAHistorial() {
  emit('cambiar-vista', 'historial')
}

// ══════════════════════════════════════════════════════════════════════════
// HELPERS
// ══════════════════════════════════════════════════════════════════════════
function formatNumber(num) {
  if (!num) return '0'
  return new Intl.NumberFormat('es-AR').format(num)
}

// ══════════════════════════════════════════════════════════════════════════
// LIFECYCLE
// ══════════════════════════════════════════════════════════════════════════
onMounted(() => {
  cargarCarrito()
})
</script>

<style scoped>
.user-select-all {
  cursor: pointer;
}
.user-select-all:hover {
  background: #e3f2fd;
  border-radius: 4px;
}
</style>