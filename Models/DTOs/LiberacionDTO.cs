using System;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar información de una liberación de fondos
    /// </summary>
    public class LiberacionDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        
        // Vendedor
        public string NombreComercial { get; set; } = string.Empty;
        public string NombreVendedor { get; set; } = string.Empty;
        public string EmailVendedor { get; set; } = string.Empty;
        
        // Montos
        public decimal MontoBruto { get; set; }
        public decimal Comision { get; set; }
        public decimal MontoNeto { get; set; }
        public decimal PorcentajeComision { get; set; }
        
        // Estado
        public EstadoLiberacion Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        
        // Fechas
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaListoParaLiberar { get; set; }
        public DateTime? FechaTransferencia { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        
        // Transferencia
        public string? NumeroOperacion { get; set; }
        public string? ArchivoComprobante { get; set; }
        
        // Datos destino (al momento de la transferencia)
        public string? AliasDestino { get; set; }
        public string? CBUDestino { get; set; }
        public string? TitularDestino { get; set; }
        
        // Admin que procesó
        public string? ProcesadoPorNombre { get; set; }
        
        // Observaciones
        public string? Observaciones { get; set; }
        
        // Flags
        public bool PuedeProcesar { get; set; }
        public bool PuedeConfirmar { get; set; }
    }

    /// <summary>
    /// DTO para listar liberaciones pendientes (Admin)
    /// </summary>
    public class LiberacionPendienteDTO
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        
        // Vendedor
        public int VendedorId { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string NombreVendedor { get; set; } = string.Empty;
        
        // Datos de pago del vendedor
        public string? AliasCBU { get; set; }
        public string? CBU { get; set; }
        public string? Banco { get; set; }
        public string? TitularCuenta { get; set; }
        public bool DatosPagoCompletos { get; set; }
        
        // Montos
        public decimal MontoNeto { get; set; }
        public decimal Comision { get; set; }
        
        // Fechas
        public DateTime FechaListoParaLiberar { get; set; }
        public int DiasEsperando { get; set; }
        
        // Comprador
        public string NombreComprador { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
    }

    /// <summary>
    /// DTO para procesar una liberación (Admin)
    /// </summary>
    public class ProcesarLiberacionDTO
    {
        [Required]
        public int LiberacionId { get; set; }

        [Required(ErrorMessage = "El número de operación es requerido")]
        [StringLength(100)]
        [Display(Name = "Número de Operación/Transacción")]
        public string NumeroOperacion { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // El archivo de comprobante se maneja por separado (IFormFile)
    }

    /// <summary>
    /// DTO para que el vendedor confirme recepción
    /// </summary>
    public class ConfirmarRecepcionDTO
    {
        [Required]
        public int LiberacionId { get; set; }

        public bool Confirmado { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de liberaciones (Admin)
    /// </summary>
    public class EstadisticasLiberacionesDTO
    {
        // Contadores
        public int TotalLiberaciones { get; set; }
        public int Pendientes { get; set; }
        public int ListasParaLiberar { get; set; }
        public int EnProceso { get; set; }
        public int Transferidas { get; set; }
        public int Confirmadas { get; set; }
        
        // Montos
        public decimal MontoTotalLiberado { get; set; }
        public decimal MontoPendienteLiberacion { get; set; }
        public decimal ComisionesTotales { get; set; }
        
        // Hoy
        public int LiberacionesHoy { get; set; }
        public decimal MontoLiberadoHoy { get; set; }
        
        // Tiempos promedio
        public double DiasPromedioLiberacion { get; set; }
    }

    /// <summary>
    /// DTO resumen de liberaciones para un vendedor
    /// </summary>
    public class ResumenLiberacionesVendedorDTO
    {
        public int VendedorId { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        
        // Totales históricos
        public decimal TotalCobrado { get; set; }
        public decimal TotalComisiones { get; set; }
        public int CantidadLiberaciones { get; set; }
        
        // Pendiente
        public decimal PendienteCobro { get; set; }
        public int LiberacionesPendientes { get; set; }
        
        // Última liberación
        public DateTime? UltimaLiberacion { get; set; }
        public decimal? MontoUltimaLiberacion { get; set; }
        
        // Lista de liberaciones recientes
        public List<LiberacionDTO> LiberacionesRecientes { get; set; } = new();
    }
}