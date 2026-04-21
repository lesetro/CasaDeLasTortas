<template>
  <div class="carrito-compras">
    <div class="card border-0 shadow-sm rounded-3">

      <!-- Header -->
      <div class="card-header bg-white border-0 d-flex justify-content-between align-items-center px-4 pt-4 pb-3">
        <h5 class="mb-0 fw-bold">
          <i class="fas fa-shopping-cart text-primary me-2"></i>Mi Carrito
          <span v-if="totalItems > 0" class="badge bg-primary ms-2 rounded-pill">{{ totalItems }}</span>
        </h5>
        <button v-if="totalItems > 0" class="btn btn-outline-danger btn-sm"
                @click="vaciarCarrito" :disabled="vaciando">
          <span v-if="vaciando" class="spinner-border spinner-border-sm me-1"></span>
          <i v-else class="fas fa-trash me-1"></i>Vaciar
        </button>
      </div>

      <div class="card-body px-4 pb-4 pt-2">

        <!-- Loading -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary"></div>
        </div>

        <!-- Carrito vacío -->
        <div v-else-if="totalItems === 0" class="text-center py-5 text-muted">
          <i class="fas fa-shopping-cart fa-3x mb-3 d-block opacity-30"></i>
          <h6 class="fw-semibold">Tu carrito está vacío</h6>
          <p class="small">Agregá productos desde el catálogo para empezar</p>
          <button class="btn btn-primary mt-2" @click="$emit('cambiar-vista', 'catalogo')">
            <i class="fas fa-store me-2"></i>Ir al Catálogo
          </button>
        </div>

        <!-- Carrito con items -->
        <div v-else class="row g-4">

          <!-- Columna izquierda: resumen y pasos -->
          <div class="col-md-7">

            <!-- Resumen visual -->
            <div class="p-4 rounded-3 text-center mb-4" style="background:#f8fafc;">
              <i class="fas fa-shopping-bag text-primary fa-2x mb-2 d-block"></i>
              <h5 class="fw-bold mb-1">
                {{ totalItems }} producto{{ totalItems !== 1 ? 's' : '' }} en el carrito
              </h5>
              <p class="text-muted small mb-3">
                Podés revisar y modificar cantidades en la página del carrito.
              </p>
              <a href="/Carrito" class="btn btn-outline-primary">
                <i class="fas fa-eye me-2"></i>Ver detalle del carrito
              </a>
            </div>

            <!-- Pasos del proceso -->
            <div class="p-4 rounded-3" style="background:#f0fdf4; border:1px solid #bbf7d0;">
              <div class="fw-bold small mb-3 text-success">
                <i class="fas fa-info-circle me-1"></i>¿Cómo funciona el pago?
              </div>
              <div v-for="(paso, i) in pasosPago" :key="i"
                   class="d-flex gap-3 mb-3"
                   :class="{ 'mb-0': i === pasosPago.length - 1 }">
                <div class="rounded-circle d-flex align-items-center justify-content-center fw-bold text-white flex-shrink-0"
                     style="width:26px;height:26px;font-size:.7rem;"
                     :style="`background:${paso.color}`">
                  {{ i + 1 }}
                </div>
                <div style="font-size:.83rem;">
                  <span class="fw-semibold">{{ paso.titulo }}</span>
                  <span class="text-muted ms-1">{{ paso.desc }}</span>
                </div>
              </div>
            </div>

          </div>

          <!-- Columna derecha: acción de checkout + datos bancarios -->
          <div class="col-md-5">

            <!-- CTA principal -->
            <div class="card border-0 rounded-3 mb-4" style="background:linear-gradient(135deg,#f0fdf4,#dcfce7); border:2px solid #bbf7d0!important;">
              <div class="card-body p-4 text-center">
                <div class="fs-5 fw-bold text-success mb-1">Listo para pagar</div>
                <div class="text-muted small mb-3">Hacé click para ver los datos de transferencia</div>
                <button @click="$emit('cambiar-vista', 'checkout')"
                        class="btn btn-success btn-lg w-100 fw-bold shadow-sm mb-3">
                  <i class="fas fa-lock me-2"></i>Proceder al Pago
                </button>
                <button class="btn btn-outline-primary w-100 mb-2"
                        @click="$emit('cambiar-vista', 'catalogo')">
                  <i class="fas fa-plus me-2"></i>Seguir comprando
                </button>
                <button class="btn btn-outline-danger w-100 btn-sm"
                        @click="vaciarCarrito" :disabled="vaciando">
                  <i class="fas fa-trash me-1"></i>Vaciar carrito
                </button>
              </div>
            </div>

            <!-- Preview datos bancarios -->
            <div v-if="datosBancarios" class="card border-0 rounded-3" style="background:#f8fafc;">
              <div class="card-body p-3">
                <div class="fw-semibold small mb-2 text-muted">
                  <i class="fas fa-university me-1"></i>Datos de transferencia (preview)
                </div>
                <div class="d-flex justify-content-between py-1 border-bottom" style="font-size:.8rem;">
                  <span class="text-muted">Alias</span>
                  <strong class="font-monospace">{{ datosBancarios.alias }}</strong>
                </div>
                <div class="d-flex justify-content-between py-1 border-bottom" style="font-size:.8rem;">
                  <span class="text-muted">Banco</span>
                  <strong>{{ datosBancarios.banco }}</strong>
                </div>
                <div class="d-flex justify-content-between py-1" style="font-size:.8rem;">
                  <span class="text-muted">Titular</span>
                  <strong>{{ datosBancarios.titular }}</strong>
                </div>
                <div class="mt-2 p-2 rounded text-center" style="background:#dcfce7; font-size:.8rem;">
                  <span class="text-muted">Recibirás el monto exacto al finalizar</span>
                </div>
              </div>
            </div>
            <div v-else class="card border-0 rounded-3 p-3" style="background:#f8fafc;">
              <div class="text-center text-muted small py-2">
                <div class="spinner-border spinner-border-sm me-1"></div>
                Cargando datos bancarios...
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
import { fetchWithAuth, mostrarToast } from '@shared/apiUtils.js'

const emit = defineEmits(['cambiar-vista', 'actualizar-carrito'])

// ── Estado ──────────────────────────────────────────────────────
const totalItems     = ref(0)
const loading        = ref(true)
const vaciando       = ref(false)
const datosBancarios = ref(null)

// ── Pasos del proceso ──────────────────────────────────────────
const pasosPago = [
  { titulo: 'Hacés click en "Proceder al Pago"', desc: '',                                    color: '#3b82f6' },
  { titulo: 'Transferís el total',               desc: 'a la cuenta de la plataforma.',       color: '#f59e0b' },
  { titulo: 'Subís el comprobante',              desc: 'foto o PDF de la transferencia.',     color: '#a855f7' },
  { titulo: 'Verificamos en 1 día hábil',        desc: 'y te notificamos por email.',         color: '#22c55e' },
  { titulo: 'Retirás en el local',               desc: 'cuando el vendedor confirme.',        color: '#f97316' },
]

// ── Carga de datos ─────────────────────────────────────────────
async function cargarCarrito() {
  try {
    loading.value = true
    const data = await fetchWithAuth('/Carrito/Contador')
    totalItems.value = data?.total ?? 0
  } catch (err) {
    console.error('Error cargando carrito:', err)
  } finally {
    loading.value = false
  }
}

async function cargarDatosBancarios() {
  try {
    const data = await fetchWithAuth('/api/ConfiguracionApi/datos-pago')
    datosBancarios.value = data
  } catch {
    // fallback amigable
    datosBancarios.value = {
      alias:   'casadelastortas.pagos',
      banco:   'Banco Nación',
      titular: 'Casa de las Tortas',
    }
  }
}

async function vaciarCarrito() {
  if (!confirm('¿Vaciar todo el carrito?')) return
  try {
    vaciando.value = true
    await fetchWithAuth('/Carrito/Vaciar', {
      method: 'POST',
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    totalItems.value = 0
    emit('actualizar-carrito', 0)
    mostrarToast('Carrito vaciado', 'success')
  } catch {
    mostrarToast('Error al vaciar el carrito', 'error')
  } finally {
    vaciando.value = false
  }
}

onMounted(() => {
  cargarCarrito()
  cargarDatosBancarios()
})
</script>