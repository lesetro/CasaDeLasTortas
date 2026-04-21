namespace CasaDeLasTortas.Models.Options
{
    /// <summary>
    /// Opciones de configuración de la plataforma de pagos.
    /// Se inyectan como IOptions&lt;PlataformaPagosOptions&gt;.
    /// Los valores provienen de appsettings.json sección "PlataformaPagos".
    /// Si la BD tiene una fila en ConfiguracionPlataforma, esos valores
    /// tienen prioridad (se resuelven en ConfiguracionRepository).
    /// </summary>
    public class PlataformaPagosOptions
    {
        /// <summary>Alias CBU para transferencias</summary>
        public string AliasCbu { get; set; } = "casadelastortas.pagos";
        
        /// <summary>CBU completo de la cuenta</summary>
        public string Cbu { get; set; } = "";
        
        /// <summary>Nombre del banco</summary>
        public string Banco { get; set; } = "";
        
        /// <summary>Nombre del titular de la cuenta</summary>
        public string TitularCuenta { get; set; } = "";
        
        /// <summary>CUIT del titular</summary>
        public string Cuit { get; set; } = "";
        
        /// <summary>Tipo de cuenta (Caja de Ahorro, Cuenta Corriente)</summary>
        public string TipoCuenta { get; set; } = "Caja de Ahorro";
        
        /// <summary>Porcentaje de comisión que cobra la plataforma</summary>
        public decimal PorcentajeComision { get; set; } = 10m;
        
        /// <summary>Días hábiles para verificar un pago</summary>
        public int DiasVerificacionPago { get; set; } = 1;
        
        /// <summary>Mensaje personalizado en el checkout</summary>
        public string MensajeCheckout { get; set; } = "";
        
        /// <summary>Email de soporte</summary>
        public string EmailSoporte { get; set; } = "";
        
        /// <summary>Teléfono de soporte</summary>
        public string TelefonoSoporte { get; set; } = "";
    }
}