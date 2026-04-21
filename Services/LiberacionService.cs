using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Models.DTOs;

namespace CasaDeLasTortas.Services
{
    public interface ILiberacionService
    {
        Task<(bool Success, string Message)> MarcarEntregaConfirmadaAsync(int ventaId);
        Task<(bool Success, string Message)> ProcesarLiberacionAsync(int liberacionId, int adminId, 
            string numeroOperacion, string archivoComprobante);
        Task<(bool Success, string Message)> ConfirmarRecepcionVendedorAsync(int liberacionId, int vendedorId);
        Task<IEnumerable<LiberacionPendienteDTO>> GetLiberacionesPendientesAsync();
        Task<EstadisticasLiberacionesDTO> GetEstadisticasAsync();
        Task<ResumenLiberacionesVendedorDTO> GetResumenVendedorAsync(int vendedorId);
    }

    public class LiberacionService : ILiberacionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LiberacionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> MarcarEntregaConfirmadaAsync(int ventaId)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdAsync(ventaId);
                if (venta == null)
                    return (false, "Venta no encontrada");

                venta.Estado = EstadoVenta.Entregada;
                venta.FechaEntregaReal = DateTime.Now;
                _unitOfWork.Ventas.Update(venta);

                // Marcar liberaciones como listas (solo las que aún no fueron transferidas/confirmadas)
                var liberaciones = await _unitOfWork.Liberaciones.GetByVentaIdAsync(ventaId);
                foreach (var lib in liberaciones)
                {
                    if (lib.Estado != EstadoLiberacion.Transferido && lib.Estado != EstadoLiberacion.Confirmado)
                        await _unitOfWork.Liberaciones.MarcarListaParaLiberarAsync(lib.Id);
                }

                await _unitOfWork.SaveChangesAsync();
                return (true, "Entrega confirmada. Fondos listos para liberar.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ProcesarLiberacionAsync(int liberacionId, int adminId,
            string numeroOperacion, string archivoComprobante)
        {
            try
            {
                var liberacion = await _unitOfWork.Liberaciones.GetByIdWithDetallesAsync(liberacionId);
                if (liberacion == null)
                    return (false, "Liberación no encontrada");

                if (liberacion.Estado != EstadoLiberacion.ListoParaLiberar)
                    return (false, "La liberación no está lista para procesar");

                if (liberacion.Vendedor == null || !liberacion.Vendedor.DatosPagoCompletos)
                    return (false, "El vendedor no tiene datos de pago completos");

                await _unitOfWork.Liberaciones.RegistrarTransferenciaAsync(
                    liberacionId, numeroOperacion, archivoComprobante, adminId);

                await _unitOfWork.SaveChangesAsync();
                return (true, "Transferencia registrada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ConfirmarRecepcionVendedorAsync(int liberacionId, int vendedorId)
        {
            try
            {
                var liberacion = await _unitOfWork.Liberaciones.GetByIdAsync(liberacionId);
                if (liberacion == null)
                    return (false, "Liberación no encontrada");

                if (liberacion.VendedorId != vendedorId)
                    return (false, "No tiene permiso para confirmar esta liberación");

                if (liberacion.Estado != EstadoLiberacion.Transferido)
                    return (false, "La liberación no está en estado transferido");

                await _unitOfWork.Liberaciones.ConfirmarRecepcionAsync(liberacionId);
                
                // Verificar si todas las liberaciones de la venta están confirmadas
                var libsVenta = await _unitOfWork.Liberaciones.GetByVentaIdAsync(liberacion.VentaId);
                if (libsVenta.All(l => l.Estado == EstadoLiberacion.Confirmado))
                {
                    var venta = await _unitOfWork.Ventas.GetByIdAsync(liberacion.VentaId);
                    if (venta != null)
                    {
                        venta.FondosLiberados = true;
                        venta.FechaLiberacion = DateTime.Now;
                        _unitOfWork.Ventas.Update(venta);

                        // Marcar pago como completado
                        var pagos = await _unitOfWork.PagoRepository.GetByVentaIdAsync(liberacion.VentaId);
                        var pago = pagos.FirstOrDefault();
                        if (pago != null)
                        {
                            pago.Estado = EstadoPago.Completado;
                            _unitOfWork.PagoRepository.Update(pago);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return (true, "Recepción confirmada");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<IEnumerable<LiberacionPendienteDTO>> GetLiberacionesPendientesAsync()
        {
            var liberaciones = await _unitOfWork.Liberaciones.GetListasParaLiberarAsync();
            return liberaciones.Select(l => new LiberacionPendienteDTO
            {
                Id = l.Id,
                VentaId = l.VentaId,
                NumeroOrden = l.Venta?.NumeroOrden ?? "",
                VendedorId = l.VendedorId,
                NombreComercial = l.Vendedor?.NombreComercial ?? "",
                NombreVendedor = l.Vendedor?.Persona?.Nombre ?? "",
                AliasCBU = l.Vendedor?.AliasCBU,
                CBU = l.Vendedor?.CBU,
                Banco = l.Vendedor?.Banco,
                TitularCuenta = l.Vendedor?.TitularCuenta,
                DatosPagoCompletos = l.Vendedor?.DatosPagoCompletos ?? false,
                MontoNeto = l.MontoNeto,
                Comision = l.Comision,
                FechaListoParaLiberar = l.FechaListoParaLiberar ?? DateTime.Now,
                DiasEsperando = l.FechaListoParaLiberar.HasValue 
                    ? (int)(DateTime.Now - l.FechaListoParaLiberar.Value).TotalDays : 0
            }).ToList();
        }

        public async Task<EstadisticasLiberacionesDTO> GetEstadisticasAsync()
        {
            return new EstadisticasLiberacionesDTO
            {
                TotalLiberaciones = await _unitOfWork.Liberaciones.CountAsync(),
                Pendientes = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Pendiente),
                ListasParaLiberar = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.ListoParaLiberar),
                Transferidas = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Transferido),
                Confirmadas = await _unitOfWork.Liberaciones.CountByEstadoAsync(EstadoLiberacion.Confirmado),
                MontoTotalLiberado = await _unitOfWork.Liberaciones.GetMontoTotalLiberadoAsync(),
                MontoPendienteLiberacion = await _unitOfWork.Liberaciones.GetMontoPendienteLiberacionAsync()
            };
        }

        public async Task<ResumenLiberacionesVendedorDTO> GetResumenVendedorAsync(int vendedorId)
        {
            var vendedor = await _unitOfWork.VendedorRepository.GetByIdWithPersonaAsync(vendedorId);
            if (vendedor == null)
                return new ResumenLiberacionesVendedorDTO();

            var liberaciones = await _unitOfWork.Liberaciones.GetByVendedorIdAsync(vendedorId);

            return new ResumenLiberacionesVendedorDTO
            {
                VendedorId = vendedorId,
                NombreComercial = vendedor.NombreComercial,
                TotalCobrado = vendedor.TotalCobrado,
                TotalComisiones = vendedor.TotalComisiones,
                PendienteCobro = vendedor.PendienteCobro,
                CantidadLiberaciones = liberaciones.Count(),
                LiberacionesPendientes = liberaciones.Count(l => l.Estado != EstadoLiberacion.Confirmado),
                UltimaLiberacion = liberaciones
                    .Where(l => l.Estado == EstadoLiberacion.Confirmado)
                    .OrderByDescending(l => l.FechaConfirmacion)
                    .FirstOrDefault()?.FechaConfirmacion
            };
        }
    }
}