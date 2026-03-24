using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Models.DTOs
{
    public class CrearPagoApiDTO
    {
        [Required]
        public int VentaId { get; set; }

        [Required]
        public MetodoPago MetodoPago { get; set; }

        public string? NumeroTransaccion { get; set; }

        public string? Observaciones { get; set; }
    }

    public class ConfirmarPagoApiDTO
    {
        public string? NumeroTransaccion { get; set; }
    }

    public class CancelarPagoApiDTO
    {
        [Required]
        public string MotivoCancelacion { get; set; } = string.Empty;
    }
}