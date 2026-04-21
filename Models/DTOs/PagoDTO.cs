using System;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar información de un pago
    /// </summary>
    public class PagoDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int CompradorId { get; set; }
        public string NombreComprador { get; set; } = string.Empty;
        
        // Montos
        public decimal Monto { get; set; }
        public decimal ComisionPlataforma { get; set; }
        public decimal MontoVendedores { get; set; }
        
        // Estado
        public EstadoPago Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        public MetodoPago? MetodoPago { get; set; }
        public string? MetodoPagoTexto { get; set; }
        
        // Fechas
        public DateTime FechaPago { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public DateTime? FechaComprobante { get; set; }
        public DateTime? FechaRechazo { get; set; }
        public DateTime? FechaReembolso { get; set; }
        
        // Comprobante
        public string? ArchivoComprobante { get; set; }
        public string? NumeroTransaccion { get; set; }
        
        // Verificación
        public int? VerificadoPorId { get; set; }
        public string? VerificadoPorNombre { get; set; }
        public string? ObservacionesAdmin { get; set; }
        
        // Rechazo
        public string? MotivoRechazo { get; set; }
        public int IntentosRechazados { get; set; }
        
        // Reembolso
        public string? ArchivoReembolso { get; set; }
        public string? NumeroTransaccionReembolso { get; set; }
        public string? MotivoReembolso { get; set; }
        
        // Flags
        public bool PuedeSerVerificado { get; set; }
        public bool PuedeSubirComprobante { get; set; }
        public bool RequiereReembolso { get; set; }
        
        // Observaciones
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para subir un comprobante de pago
    /// </summary>
    public class SubirComprobanteDTO
    {
        [Required(ErrorMessage = "El ID de la venta es requerido")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        public MetodoPago MetodoPago { get; set; }

        [StringLength(100)]
        [Display(Name = "Número de Transacción/Operación")]
        public string? NumeroTransaccion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // El archivo se maneja por separado (IFormFile)
    }

    /// <summary>
    /// DTO para que el admin verifique un pago
    /// </summary>
    public class VerificarPagoDTO
    {
        [Required]
        public int PagoId { get; set; }

        [Required]
        public bool Aprobado { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Motivo de rechazo (requerido si Aprobado = false)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Motivo de Rechazo")]
        public string? MotivoRechazo { get; set; }
    }

    /// <summary>
    /// DTO para procesar un reembolso
    /// </summary>
    public class ProcesarReembolsoDTO
    {
        [Required]
        public int PagoId { get; set; }

        [Required(ErrorMessage = "El número de transacción es requerido")]
        [StringLength(100)]
        public string NumeroTransaccion { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // El archivo de comprobante se maneja por separado
    }

    /// <summary>
    /// DTO para listar pagos pendientes de verificación (Admin)
    /// </summary>
    public class PagoPendienteDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        
        // Comprador
        public string NombreComprador { get; set; } = string.Empty;
        public string EmailComprador { get; set; } = string.Empty;
        
        // Monto
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        
        // Comprobante
        public string? ArchivoComprobante { get; set; }
        public string? NumeroTransaccion { get; set; }
        public DateTime? FechaComprobante { get; set; }
        
        // Tiempo
        public DateTime FechaPago { get; set; }
        public int HorasEsperando { get; set; }
        
        // Estado
        public EstadoPago Estado { get; set; }

        // Intentos previos
        public int IntentosRechazados { get; set; }

        // Vendedores involucrados
        public int CantidadVendedores { get; set; }
        public List<string> NombresVendedores { get; set; } = new();
    }

    /// <summary>
    /// DTO para estadísticas de pagos
    /// </summary>
    public class EstadisticasPagosDTO
    {
        // Contadores
        public int TotalPagos { get; set; }
        public int PagosPendientes { get; set; }
        public int PagosEnRevision { get; set; }
        public int PagosVerificados { get; set; }
        public int PagosRechazados { get; set; }
        public int ReembolsosPendientes { get; set; }
        
        // Montos
        public decimal MontoTotalRecibido { get; set; }
        public decimal MontoEnRevision { get; set; }
        public decimal MontoPendienteLiberacion { get; set; }
        public decimal ComisionesTotales { get; set; }
        
        // Hoy
        public int PagosHoy { get; set; }
        public decimal MontoHoy { get; set; }
        
        // Tiempos promedio
        public double HorasPromedioVerificacion { get; set; }
    }

    /// <summary>
    /// DTO para subir comprobante vía multipart/form-data (acepta IFormFile)
    /// </summary>
    public class SubirComprobanteApiDTO
    {
        public IFormFile? Archivo { get; set; }
        public string? ArchivoBase64 { get; set; }
        public string? NombreArchivo { get; set; }

        [Required]
        public MetodoPago MetodoPago { get; set; }

        public string? NumeroTransaccion { get; set; }
    }

    /// <summary>
    /// DTO para procesar un reembolso con comprobante adjunto (multipart/form-data)
    /// </summary>
    public class ProcesarReembolsoApiDTO
    {
        [Required]
        public IFormFile Archivo { get; set; } = null!;

        [Required]
        public string NumeroTransaccion { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambiar el estado de un detalle de venta
    /// </summary>
    public class CambiarEstadoDetalleDTO
    {
        [Required]
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para subir comprobante de pago identificando la venta por ID
    /// </summary>
    public class SubirComprobantePorVentaDTO
    {
        [Required(ErrorMessage = "El ID de venta es requerido")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El comprobante es requerido")]
        public IFormFile Comprobante { get; set; } = null!;

        public string? NumeroTransaccion { get; set; }
    }

    /// <summary>
    /// DTO con información de la plataforma para el checkout
    /// </summary>
    public class DatosPagoPlataformaDTO
    {
        public string NombrePlataforma { get; set; } = string.Empty;
        public string AliasCBU { get; set; } = string.Empty;
        public string CBU { get; set; } = string.Empty;
        public string Banco { get; set; } = string.Empty;
        public string TitularCuenta { get; set; } = string.Empty;
        public string? ImagenQR { get; set; }
        public string? InstruccionesPago { get; set; }
        public decimal ComisionPorcentaje { get; set; }
        public int DiasLimitePago { get; set; }
    }
}