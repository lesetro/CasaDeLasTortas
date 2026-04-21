namespace CasaDeLasTortas.Models.Options
{
    /// <summary>
    /// Opciones de configuración para almacenamiento de archivos subidos.
    /// Se inyectan como IOptions&lt;FileStorageOptions&gt;.
    /// Los valores provienen de appsettings.json sección "FileStorage".
    /// </summary>
    public class FileStorageOptions
    {
        /// <summary>Ruta base para archivos subidos</summary>
        public string RutaBase { get; set; } = "uploads";
        
        /// <summary>Ruta para comprobantes de pago</summary>
        public string RutaComprobantes { get; set; } = "uploads/comprobantes";
        
        /// <summary>Ruta para imágenes de tortas</summary>
        public string RutaImagenesTortas { get; set; } = "uploads/tortas";
        
        /// <summary>Ruta para avatares de usuarios</summary>
        public string RutaAvatares { get; set; } = "uploads/avatares";
        
        /// <summary>Tamaño máximo de archivo en bytes (default: 5 MB)</summary>
        public long TamanoMaximoBytes { get; set; } = 5_242_880;
        
        /// <summary>Extensiones permitidas para comprobantes</summary>
        public string[] ExtensionesComprobante { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf", ".webp" };
    }
}