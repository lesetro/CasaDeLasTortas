using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.ComponentModel.DataAnnotations;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;
        private readonly IFileService _fileService;
        private readonly ILogger<ConfiguracionApiController> _logger;

        public ConfiguracionApiController(
            IUnitOfWork unitOfWork,
            IPagoService pagoService,
            IFileService fileService,
            ILogger<ConfiguracionApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener datos de pago de la plataforma (público para compradores)
        /// </summary>
        [HttpGet("datos-pago")]
        public async Task<IActionResult> GetDatosPago()
        {
            var datos = await _pagoService.GetDatosPagoPlataformaAsync();
            return Ok(datos);
        }

        /// <summary>
        /// Obtener configuración completa (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetConfiguracion()
        {
            var config = await _unitOfWork.Configuracion.GetOrCreateAsync();
            return Ok(new
            {
                id = config.Id,
                nombrePlataforma = config.NombrePlataforma,
                aliasCBU = config.AliasCBU,
                cbu = config.CBU,
                banco = config.Banco,
                titularCuenta = config.TitularCuenta,
                cuit = config.CUIT,
                imagenQR = config.ImagenQR,
                urlImagenQR = !string.IsNullOrEmpty(config.ImagenQR) 
                    ? _fileService.GetFileUrl(config.ImagenQR) : null,
                comisionPorcentaje = config.ComisionPorcentaje,
                diasParaLiberarFondos = config.DiasParaLiberarFondos,
                maxIntentosRechazados = config.MaxIntentosRechazados,
                diasLimitePago = config.DiasLimitePago,
                instruccionesPago = config.InstruccionesPago,
                plataformaActiva = config.PlataformaActiva,
                emailNotificaciones = config.EmailNotificaciones,
                telefonoContacto = config.TelefonoContacto,
                fechaActualizacion = config.FechaActualizacion
            });
        }

        /// <summary>
        /// Actualizar configuración (Admin)
        /// </summary>
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> ActualizarConfiguracion([FromBody] ActualizarConfiguracionDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var config = await _unitOfWork.Configuracion.GetOrCreateAsync();

                // Actualizar campos
                if (request.NombrePlataforma != null)
                    config.NombrePlataforma = request.NombrePlataforma;
                if (request.AliasCBU != null)
                    config.AliasCBU = request.AliasCBU;
                if (request.CBU != null)
                    config.CBU = request.CBU;
                if (request.Banco != null)
                    config.Banco = request.Banco;
                if (request.TitularCuenta != null)
                    config.TitularCuenta = request.TitularCuenta;
                if (request.CUIT != null)
                    config.CUIT = request.CUIT;
                if (request.ComisionPorcentaje.HasValue)
                    config.ComisionPorcentaje = request.ComisionPorcentaje.Value;
                if (request.DiasParaLiberarFondos.HasValue)
                    config.DiasParaLiberarFondos = request.DiasParaLiberarFondos.Value;
                if (request.MaxIntentosRechazados.HasValue)
                    config.MaxIntentosRechazados = request.MaxIntentosRechazados.Value;
                if (request.DiasLimitePago.HasValue)
                    config.DiasLimitePago = request.DiasLimitePago.Value;
                if (request.InstruccionesPago != null)
                    config.InstruccionesPago = request.InstruccionesPago;
                if (request.PlataformaActiva.HasValue)
                    config.PlataformaActiva = request.PlataformaActiva.Value;
                if (request.EmailNotificaciones != null)
                    config.EmailNotificaciones = request.EmailNotificaciones;
                if (request.TelefonoContacto != null)
                    config.TelefonoContacto = request.TelefonoContacto;

                await _unitOfWork.Configuracion.UpdateAsync(config);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { success = true, message = "Configuración actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración");
                return StatusCode(500, new { message = "Error al actualizar la configuración" });
            }
        }

        /// <summary>
        /// Subir imagen QR de la plataforma (Admin)
        /// </summary>
        [HttpPost("imagen-qr")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> SubirImagenQR([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { message = "Debe enviar una imagen" });

            try
            {
                var rutaArchivo = await _fileService.SaveFileAsync(archivo, "qr-plataforma");

                var config = await _unitOfWork.Configuracion.GetOrCreateAsync();
                config.ImagenQR = rutaArchivo;
                await _unitOfWork.Configuracion.UpdateAsync(config);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    urlImagenQR = _fileService.GetFileUrl(rutaArchivo)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen QR");
                return StatusCode(500, new { message = "Error al subir la imagen" });
            }
        }

        /// <summary>
        /// Obtener solo el porcentaje de comisión (público)
        /// </summary>
        [HttpGet("comision")]
        public async Task<IActionResult> GetComision()
        {
            var comision = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();
            return Ok(new { comisionPorcentaje = comision });
        }

        /// <summary>
        /// Verificar si la plataforma está activa
        /// </summary>
        [HttpGet("estado")]
        public async Task<IActionResult> GetEstadoPlataforma()
        {
            var activa = await _unitOfWork.Configuracion.PlataformaActivaAsync();
            return Ok(new { plataformaActiva = activa });
        }
    }   
}

   
    