using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CasaDeLasTortas.Models.Entities;

public class ActualizarConfiguracionDTO
    {
        [StringLength(100)]
        public string? NombrePlataforma { get; set; }

        [StringLength(50)]
        public string? AliasCBU { get; set; }

        [StringLength(22)]
        public string? CBU { get; set; }

        [StringLength(100)]
        public string? Banco { get; set; }

        [StringLength(200)]
        public string? TitularCuenta { get; set; }

        [StringLength(13)]
        public string? CUIT { get; set; }

        [Range(0, 50)]
        public decimal? ComisionPorcentaje { get; set; }

        [Range(1, 30)]
        public int? DiasParaLiberarFondos { get; set; }

        [Range(1, 10)]
        public int? MaxIntentosRechazados { get; set; }

        [Range(1, 30)]
        public int? DiasLimitePago { get; set; }

        [StringLength(2000)]
        public string? InstruccionesPago { get; set; }

        public bool? PlataformaActiva { get; set; }

        [EmailAddress]
        public string? EmailNotificaciones { get; set; }

        [StringLength(50)]
        public string? TelefonoContacto { get; set; }
    }
