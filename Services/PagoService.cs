using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.DTOs;

namespace CasaDeLasTortas.Services
{
    public interface IPagoService
    {
        Task<(bool Success, string Message)> SubirComprobanteAsync(int ventaId, int compradorId,
            string archivoComprobante, MetodoPago metodoPago, string? numeroTransaccion = null);
        Task<(bool Success, string Message)> VerificarPagoAsync(int pagoId, int adminId, bool aprobado,
            string? observaciones = null, string? motivoRechazo = null);
        Task<(bool Success, string Message)> ProcesarReembolsoAsync(int pagoId, int adminId,
            string archivoComprobante, string numeroTransaccion);
        Task<DatosPagoPlataformaDTO> GetDatosPagoPlataformaAsync();
        Task<EstadisticasPagosDTO> GetEstadisticasAsync();
    }

    public class PagoService : IPagoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PagoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> SubirComprobanteAsync(int ventaId, int compradorId,
            string archivoComprobante, MetodoPago metodoPago, string? numeroTransaccion = null)
        {
            try
            {
                var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(ventaId);
                var pago = pagos.FirstOrDefault();

                if (pago == null)
                    return (false, "No se encontró el pago");

                if (pago.CompradorId != compradorId)
                    return (false, "No tiene permiso para modificar este pago");

                var maxIntentos = await _unitOfWork.Configuracion.GetMaxIntentosRechazadosAsync();
                if (pago.IntentosRechazados >= maxIntentos)
                    return (false, $"Se superó el máximo de {maxIntentos} intentos. Contacte soporte.");

                pago.ArchivoComprobante = archivoComprobante;
                pago.MetodoPago = metodoPago;
                pago.NumeroTransaccion = numeroTransaccion;
                pago.FechaComprobante = DateTime.Now;
                pago.Estado = EstadoPago.EnRevision;

                _unitOfWork.PagoRepository.Update(pago);

                // Actualizar estado de venta
                var venta = await _unitOfWork.Ventas.GetByIdAsync(ventaId);
                if (venta != null)
                {
                    venta.Estado = EstadoVenta.PagoEnRevision;
                    _unitOfWork.Ventas.Update(venta);
                }

                await _unitOfWork.SaveChangesAsync();
                return (true, "Comprobante subido correctamente. Será verificado en las próximas horas.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> VerificarPagoAsync(int pagoId, int adminId,
     bool aprobado, string? observaciones = null, string? motivoRechazo = null)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdWithVentaAsync(pagoId);
                if (pago == null)
                    return (false, "Pago no encontrado");

                if (pago.Estado != EstadoPago.EnRevision)
                    return (false, "El pago no está pendiente de verificación");

                pago.VerificadoPorId = adminId;
                pago.FechaVerificacion = DateTime.Now;
                pago.ObservacionesAdmin = observaciones;

                if (aprobado)
                {
                    pago.Estado = EstadoPago.Verificado;

                    if (pago.Venta != null)
                    {
                        pago.Venta.Estado = EstadoVenta.Pagada;
                        pago.Venta.FechaActualizacion = DateTime.Now;

                        // Los detalles quedan en Pendiente: el vendedor es quien gestiona
                        // manualmente el progreso (Confirmado → EnPreparacion → Listo → Entregado)
                        var detalles = await _unitOfWork.DetallesVenta.GetByVentaIdAsync(pago.VentaId);
                        foreach (var detalle in detalles)
                        {
                            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(detalle.TortaId);
                            if (torta != null && torta.Stock <= 0 && torta.Disponible)
                            {
                                torta.Disponible = false;
                                torta.FechaActualizacion = DateTime.Now;
                                await _unitOfWork.TortaRepository.UpdateAsync(torta);
                            }
                        }
                    }

                    var comision = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();
                    await _unitOfWork.Liberaciones.CrearLiberacionesParaVentaAsync(pago.VentaId, comision);
                }
                else
                {
                    if (string.IsNullOrEmpty(motivoRechazo))
                        return (false, "Debe especificar el motivo del rechazo");

                    pago.Estado = EstadoPago.Rechazado;
                    pago.MotivoRechazo = motivoRechazo;
                    pago.FechaRechazo = DateTime.Now;
                    pago.IntentosRechazados++;

                    if (pago.Venta != null)
                    {
                        pago.Venta.Estado = EstadoVenta.Pendiente;

                        var detalles = await _unitOfWork.DetallesVenta.GetByVentaIdAsync(pago.VentaId);
                        foreach (var detalle in detalles)
                        {
                            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(detalle.TortaId);
                            if (torta != null)
                            {
                                torta.Stock += detalle.Cantidad;
                                torta.Disponible = true;
                                torta.FechaActualizacion = DateTime.Now;
                                await _unitOfWork.TortaRepository.UpdateAsync(torta);
                            }
                        }
                    }
                }

                _unitOfWork.PagoRepository.Update(pago);
                await _unitOfWork.SaveChangesAsync();

                return (true, aprobado ? "Pago verificado correctamente" : "Pago rechazado");
            }
            catch (Exception ex)
            {
                return (false, $"Error al verificar el pago: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ProcesarReembolsoAsync(int pagoId, int adminId,
            string archivoComprobante, string numeroTransaccion)
        {
            try
            {
                var pago = await _unitOfWork.PagoRepository.GetByIdAsync(pagoId);
                if (pago == null)
                    return (false, "Pago no encontrado");

                if (pago.Estado != EstadoPago.ReembolsoPendiente)
                    return (false, "El pago no requiere reembolso");

                pago.Estado = EstadoPago.Reembolsado;
                pago.ArchivoReembolso = archivoComprobante;
                pago.NumeroTransaccionReembolso = numeroTransaccion;
                pago.FechaReembolso = DateTime.Now;

                _unitOfWork.PagoRepository.Update(pago);

                // Actualizar venta
                var venta = await _unitOfWork.Ventas.GetByIdAsync(pago.VentaId);
                if (venta != null)
                {
                    venta.Estado = EstadoVenta.Reembolsada;
                    _unitOfWork.Ventas.Update(venta);
                }

                await _unitOfWork.SaveChangesAsync();
                return (true, "Reembolso procesado correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<DatosPagoPlataformaDTO> GetDatosPagoPlataformaAsync()
        {
            var config = await _unitOfWork.Configuracion.GetOrCreateAsync();
            return new DatosPagoPlataformaDTO
            {
                NombrePlataforma = config.NombrePlataforma,
                AliasCBU = config.AliasCBU,
                CBU = config.CBU,
                Banco = config.Banco,
                TitularCuenta = config.TitularCuenta,
                ImagenQR = config.ImagenQR,
                InstruccionesPago = config.InstruccionesPago,
                ComisionPorcentaje = config.ComisionPorcentaje,
                DiasLimitePago = config.DiasLimitePago
            };
        }

        public async Task<EstadisticasPagosDTO> GetEstadisticasAsync()
        {
            return new EstadisticasPagosDTO
            {
                TotalPagos = await _unitOfWork.PagoRepository.CountAsync(),
                PagosPendientes = await _unitOfWork.PagoRepository.CountPendientesAsync(),
                PagosEnRevision = await _unitOfWork.PagoRepository.CountByEstadoAsync(EstadoPago.EnRevision),
                PagosVerificados = await _unitOfWork.PagoRepository.CountByEstadoAsync(EstadoPago.Verificado),
                PagosRechazados = await _unitOfWork.PagoRepository.CountByEstadoAsync(EstadoPago.Rechazado),
                MontoTotalRecibido = await _unitOfWork.PagoRepository.GetTotalIngresosAsync(),
                PagosHoy = (await _unitOfWork.PagoRepository.GetPagosDelDiaAsync(DateTime.Today)).Count(),
                MontoHoy = await _unitOfWork.PagoRepository.GetIngresosDelDiaAsync(DateTime.Today)
            };
        }
    }
}