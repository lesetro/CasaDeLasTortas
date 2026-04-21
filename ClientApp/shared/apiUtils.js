/**
 * apiUtils.js — Utilidades compartidas para los módulos Vue de Casa de las Tortas.
 *
 * Importar en los componentes:
 *   import { fetchWithAuth, formatMoneda, ... } from '@shared/apiUtils.js'
 */

// ══════════════════════════════════════════════════════════════════
//  FETCH CON AUTENTICACIÓN JWT
// ══════════════════════════════════════════════════════════════════

function _getTokenFromCookie() {
  const match = document.cookie.match(/(?:^|;\s*)authToken=([^;]+)/)
  return match ? decodeURIComponent(match[1]) : null
}

function _clearAuthCookie() {
  document.cookie = 'authToken=; path=/; max-age=0; SameSite=Strict'
}

/**
 * Limpia completamente la sesión: localStorage + cookie.
 * Usar en logout y al detectar sesión expirada.
 */
export function clearAuth() {
  localStorage.removeItem('authToken')
  localStorage.removeItem('user')
  _clearAuthCookie()
}

export async function fetchWithAuth(url, options = {}) {
  let token = localStorage.getItem('authToken')

  // Si localStorage está vacío pero la cookie existe (ej: se limpió el storage),
  // restaurar el token desde la cookie para no interrumpir la sesión.
  if (!token) {
    token = _getTokenFromCookie()
    if (token) localStorage.setItem('authToken', token)
  }

  if (!token) throw new Error('No hay token de autenticación')

  // Si el body es FormData no agregamos Content-Type (el browser lo setea con boundary)
  const isFormData = options.body instanceof FormData
  const headers = {
    'Authorization':    `Bearer ${token}`,
    'X-Requested-With': 'XMLHttpRequest',
    ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
    ...options.headers,
  }

  const resp = await fetch(url, { ...options, headers })

  if (!resp.ok) {
    if (resp.status === 401) {
      // Limpiar AMBOS: localStorage y cookie, para que AccountController.Login
      // no redirija de vuelta al dashboard (evita el bucle infinito).
      localStorage.removeItem('authToken')
      localStorage.removeItem('user')
      _clearAuthCookie()
      window.location.href = '/Account/Login'
      throw new Error('Sesión expirada')
    }
    let msg = `Error ${resp.status}: ${resp.statusText}`
    try { msg = (await resp.json()).message || msg } catch {}
    throw new Error(msg)
  }

  return resp.status === 204 ? null : resp.json()
}

// ══════════════════════════════════════════════════════════════════
//  FORMATEO DE FECHAS Y MONEDA
// ══════════════════════════════════════════════════════════════════

/** Formatea un número como moneda ARS: $1.500 */
export function formatMoneda(valor) {
  return new Intl.NumberFormat('es-AR', {
    style:                 'currency',
    currency:              'ARS',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(valor ?? 0)
}

/** Formatea una fecha como dd/mm/yyyy */
export function formatFecha(fecha) {
  if (!fecha) return 'N/A'
  return new Date(fecha).toLocaleDateString('es-AR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
  })
}

/** Formatea fecha y hora: dd/mm/yyyy HH:mm */
export function formatFechaHora(fecha) {
  if (!fecha) return 'N/A'
  return new Date(fecha).toLocaleString('es-AR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

/** Formatea solo la hora: HH:mm */
export function formatHora(fecha) {
  if (!fecha) return ''
  return new Date(fecha).toLocaleTimeString('es-AR', {
    hour: '2-digit', minute: '2-digit',
  })
}

/** Formatea fecha relativa: "hace 5 minutos", "hace 2 días", etc. */
export function formatFechaRelativa(fecha) {
  if (!fecha) return ''
  const ahora = Date.now()
  const diff  = ahora - new Date(fecha).getTime()
  const seg   = Math.floor(diff / 1000)
  const min   = Math.floor(seg   / 60)
  const hs    = Math.floor(min   / 60)
  const dias  = Math.floor(hs    / 24)

  if (seg  < 60)  return 'hace un momento'
  if (min  < 60)  return `hace ${min} minuto${min > 1 ? 's' : ''}`
  if (hs   < 24)  return `hace ${hs} hora${hs > 1 ? 's' : ''}`
  if (dias < 7)   return `hace ${dias} día${dias > 1 ? 's' : ''}`
  return formatFecha(fecha)
}

// ══════════════════════════════════════════════════════════════════
//  ESTADOS DE VENTA
// ══════════════════════════════════════════════════════════════════

/** Texto legible de un EstadoVenta */
export function getEstadoVentaTexto(estado) {
  return {
    Pendiente:       'Pendiente de pago',
    Pagada:          'Pago verificado',
    EnPreparacion:   'En Preparación',
    ListaParaEnvio:  'Lista para Retirar',
    Enviada:         'Enviada',
    Entregada:       'Entregada',
    Cancelada:       'Cancelada',
    Reembolsada:     'Reembolsada',
  }[estado] ?? estado ?? 'Desconocido'
}

/** Alias para compatibilidad con componentes anteriores */
export const getEstadoLabel = getEstadoVentaTexto

/** Clase Bootstrap del badge según EstadoVenta */
export function getBadgeClaseVenta(estado) {
  return {
    Pendiente:       'bg-warning text-dark',
    Pagada:          'bg-info',
    EnPreparacion:   'bg-primary',
    ListaParaEnvio:  'bg-secondary',
    Enviada:         'bg-info',
    Entregada:       'bg-success',
    Cancelada:       'bg-danger',
    Reembolsada:     'bg-dark',
  }[estado] ?? 'bg-secondary'
}

/** Alias */
export const getEstadoBadgeClass = getBadgeClaseVenta

/** Emoji + texto corto para el estado de venta (para uso en listas) */
export function getEstadoVentaEmoji(estado) {
  return {
    Pendiente:       '⏳ Sin pagar',
    Pagada:          '💳 Verificado',
    EnPreparacion:   '👨‍🍳 Preparando',
    ListaParaEnvio:  '📦 Lista para retirar',
    Enviada:         '🚚 Enviada',
    Entregada:       '✅ Entregada',
    Cancelada:       '❌ Cancelada',
    Reembolsada:     '↩️ Reembolsada',
  }[estado] ?? estado ?? '—'
}

// ══════════════════════════════════════════════════════════════════
//  ESTADOS DE PAGO (comprobante del comprador)
// ══════════════════════════════════════════════════════════════════

/** Texto legible de un EstadoPago */
export function getEstadoPagoTexto(estado) {
  return {
    Pendiente:  'Sin pagar',
    EnRevision: 'En revisión',
    Verificado: 'Verificado',
    Completado: 'Completado',
    Rechazado:  'Rechazado',
    Cancelado:  'Cancelado',
  }[estado] ?? estado ?? '—'
}

/** Texto con emoji del estado de pago */
export function getEstadoPagoEmoji(estado) {
  return {
    Pendiente:  '💳 Sin pagar',
    EnRevision: '🔍 En revisión',
    Verificado: '✅ Verificado',
    Completado: '💸 Completado',
    Rechazado:  '❌ Rechazado',
    Cancelado:  '❌ Cancelado',
  }[estado] ?? estado ?? '—'
}

/** Clase Bootstrap del badge según EstadoPago */
export function getBadgePago(estado) {
  return {
    Pendiente:  'bg-secondary',
    EnRevision: 'bg-warning text-dark',
    Verificado: 'bg-info',
    Completado: 'bg-success',
    Rechazado:  'bg-danger',
    Cancelado:  'bg-danger',
  }[estado] ?? 'bg-secondary'
}

// ══════════════════════════════════════════════════════════════════
//  ESTADOS DE LIBERACIÓN DE FONDOS
// ══════════════════════════════════════════════════════════════════

/** Texto legible de un estado de liberación */
export function getEstadoLiberacionTexto(estado) {
  return {
    Pendiente:  'Pendiente de transferencia',
    Liberado:   'Fondos acreditados',
    Cancelado:  'Cancelada',
  }[estado] ?? estado ?? '—'
}

/** Emoji + texto del estado de liberación */
export function getEstadoLiberacionEmoji(estado) {
  return {
    Pendiente: '⏳ Pendiente',
    Liberado:  '💸 Cobrado',
    Cancelado: '❌ Cancelada',
  }[estado] ?? estado ?? '—'
}

/** Clase Bootstrap del badge según estado de liberación */
export function getBadgeLiberacion(estado) {
  return {
    Liberado:  'bg-success',
    Pendiente: 'bg-warning text-dark',
    Cancelado: 'bg-danger',
  }[estado] ?? 'bg-secondary'
}

// ══════════════════════════════════════════════════════════════════
//  ESTADOS DE DETALLE DE VENTA (por producto)
// ══════════════════════════════════════════════════════════════════

/** Texto legible de un EstadoDetalleVenta */
export function getEstadoDetalleTexto(estado) {
  return {
    Pendiente:     'Pendiente',
    Confirmado:    'Confirmado',
    EnPreparacion: 'En Preparación',
    Listo:         'Listo para retirar',
    Entregado:     'Entregado',
    Cancelado:     'Cancelado',
  }[estado] ?? estado ?? '—'
}

/** Clase Bootstrap del badge según EstadoDetalleVenta */
export function getBadgeDetalle(estado) {
  return {
    Pendiente:     'bg-warning text-dark',
    Confirmado:    'bg-info',
    EnPreparacion: 'bg-primary',
    Listo:         'bg-success',
    Entregado:     'bg-success',
    Cancelado:     'bg-danger',
  }[estado] ?? 'bg-secondary'
}

// ══════════════════════════════════════════════════════════════════
//  ESTADOS DE DISPUTA
// ══════════════════════════════════════════════════════════════════

export function getEstadoDisputaTexto(estado) {
  return {
    Abierta:        'Abierta',
    EnProceso:      'En proceso',
    FavorComprador: 'Resuelto (favor comprador)',
    FavorVendedor:  'Resuelto (favor vendedor)',
    Resuelta:       'Resuelta',
    Cerrada:        'Cerrada',
  }[estado] ?? estado ?? '—'
}

export function getBadgeDisputa(estado) {
  return {
    Abierta:        'bg-danger',
    EnProceso:      'bg-warning text-dark',
    FavorComprador: 'bg-success',
    FavorVendedor:  'bg-success',
    Resuelta:       'bg-success',
    Cerrada:        'bg-secondary',
  }[estado] ?? 'bg-secondary'
}

// ══════════════════════════════════════════════════════════════════
//  TOAST / NOTIFICACIONES
// ══════════════════════════════════════════════════════════════════

/**
 * Muestra un toast.
 * @param {string} mensaje
 * @param {'success'|'error'|'warning'|'info'} tipo
 */
export function mostrarToast(mensaje, tipo = 'info') {
  if (typeof toastr !== 'undefined') {
    toastr[tipo]?.(mensaje) ?? toastr.info(mensaje)
  } else {
    const estilos = {
      success: 'background:#22c55e;color:#fff',
      error:   'background:#ef4444;color:#fff',
      warning: 'background:#f59e0b;color:#000',
      info:    'background:#3b82f6;color:#fff',
    }
    const div = document.createElement('div')
    div.textContent = mensaje
    div.style.cssText = `
      position:fixed; bottom:1.5rem; right:1.5rem; z-index:9999;
      padding:.75rem 1.25rem; border-radius:10px; font-size:.875rem;
      box-shadow:0 4px 12px rgba(0,0,0,.18); max-width:320px;
      ${estilos[tipo] ?? estilos.info}
    `
    document.body.appendChild(div)
    setTimeout(() => div.remove(), 4000)
  }
}

// ══════════════════════════════════════════════════════════════════
//  AVATAR
// ══════════════════════════════════════════════════════════════════

/**
 * Genera una URL de avatar con las iniciales del nombre.
 * Usa ui-avatars.com (gratuito, sin API key).
 */
export function generarAvatarUrl(nombre) {
  if (!nombre?.trim()) nombre = 'U'
  const encoded = encodeURIComponent(nombre.trim())
  return `https://ui-avatars.com/api/?name=${encoded}&background=random&color=fff&size=150&bold=true&format=svg`
}

// ══════════════════════════════════════════════════════════════════
//  CÁLCULO DE COMISIONES
// ══════════════════════════════════════════════════════════════════

/**
 * Calcula la comisión y el monto neto para el vendedor.
 * @param {number} monto - Subtotal del vendedor
 * @param {number} porcentaje - Porcentaje de comisión (default 10)
 * @returns {{ comision: number, neto: number, porcentaje: number }}
 */
export function calcularComision(monto, porcentaje = 10) {
  const comision = monto * (porcentaje / 100)
  return {
    comision:   Math.round(comision * 100) / 100,
    neto:       Math.round((monto - comision) * 100) / 100,
    porcentaje,
  }
}

// ══════════════════════════════════════════════════════════════════
//  VALIDACIONES
// ══════════════════════════════════════════════════════════════════

/** Valida que un archivo sea de un tipo permitido y no exceda el tamaño máximo */
export function validarArchivo(file, {
  extensiones = ['.jpg', '.jpeg', '.png', '.pdf', '.webp'],
  maxBytes    = 5 * 1024 * 1024,
} = {}) {
  if (!file) return { valido: false, error: 'No se seleccionó ningún archivo.' }

  const ext = '.' + file.name.split('.').pop().toLowerCase()
  if (!extensiones.includes(ext))
    return {
      valido: false,
      error:  `Tipo de archivo no permitido. Se aceptan: ${extensiones.join(', ')}`,
    }

  if (file.size > maxBytes)
    return {
      valido: false,
      error:  `El archivo supera el tamaño máximo (${(maxBytes / 1024 / 1024).toFixed(0)} MB).`,
    }

  return { valido: true, error: null }
}

// ══════════════════════════════════════════════════════════════════
//  PAGINACIÓN
// ══════════════════════════════════════════════════════════════════

/**
 * Genera el rango de páginas visibles para un componente de paginación.
 * @param {number} paginaActual
 * @param {number} totalPaginas
 * @param {number} rango - Cuántas páginas mostrar a cada lado (default 2)
 * @returns {number[]}
 */
export function getPaginasVisibles(paginaActual, totalPaginas, rango = 2) {
  const inicio = Math.max(1, paginaActual - rango)
  const fin    = Math.min(totalPaginas, paginaActual + rango)
  return Array.from({ length: fin - inicio + 1 }, (_, i) => inicio + i)
}