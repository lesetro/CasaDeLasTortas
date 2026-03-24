using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models;
using CasaDeLasTortas.Models.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CasaDeLasTortas.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private const string CARRITO_COOKIE_KEY = "CarritoCompras";
        private const string CARRITO_SESSION_KEY = "CarritoCompras";

        public CarritoService(
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext!;

        public CarritoSession ObtenerCarrito()
        {
            CarritoSession? carrito = null;

            var sessionCarrito = HttpContext.Session.GetString(CARRITO_SESSION_KEY);
            if (!string.IsNullOrEmpty(sessionCarrito))
            {
                carrito = JsonConvert.DeserializeObject<CarritoSession>(sessionCarrito);
            }

            if (carrito == null)
            {
                var cookieCarrito = HttpContext.Request.Cookies[CARRITO_COOKIE_KEY];
                if (!string.IsNullOrEmpty(cookieCarrito))
                {
                    try
                    {
                        var decoded = System.Web.HttpUtility.UrlDecode(cookieCarrito);
                        carrito = JsonConvert.DeserializeObject<CarritoSession>(decoded);
                        
                        if (carrito != null)
                        {
                            GuardarCarrito(carrito);
                            HttpContext.Response.Cookies.Delete(CARRITO_COOKIE_KEY);
                        }
                    }
                    catch
                    {
                        carrito = new CarritoSession();
                    }
                }
            }

            return carrito ?? new CarritoSession();
        }

        public void GuardarCarrito(CarritoSession carrito)
        {
            carrito.FechaActualizacion = DateTime.Now;
            var json = JsonConvert.SerializeObject(carrito);

            HttpContext.Session.SetString(CARRITO_SESSION_KEY, json);

            if (!HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var encoded = System.Web.HttpUtility.UrlEncode(json);
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = true,
                    Path = "/"
                };
                HttpContext.Response.Cookies.Append(CARRITO_COOKIE_KEY, encoded, cookieOptions);
            }
        }

        public void VaciarCarrito()
        {
            HttpContext.Session.Remove(CARRITO_SESSION_KEY);
            HttpContext.Response.Cookies.Delete(CARRITO_COOKIE_KEY);
        }

        public async Task<bool> AgregarItem(int tortaId, int cantidad, string? notas = null)
        {
            if (cantidad <= 0) return false;

            var torta = await _unitOfWork.TortaRepository.GetByIdWithVendedorAsync(tortaId);
            if (torta == null || !torta.Disponible || torta.Stock < cantidad)
                return false;

            var carrito = ObtenerCarrito();
            var itemExistente = carrito.Items.FirstOrDefault(i => i.TortaId == tortaId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                itemExistente.NotasPersonalizacion = notas ?? itemExistente.NotasPersonalizacion;
            }
            else
            {
                carrito.Items.Add(new CarritoItemSession
                {
                    TortaId = torta.Id,
                    Nombre = torta.Nombre,
                    PrecioUnitario = torta.Precio,
                    Cantidad = cantidad,
                    NotasPersonalizacion = notas,
                    VendedorId = torta.VendedorId,
                    NombreVendedor = torta.Vendedor?.NombreComercial ?? "Vendedor",
                    ImagenPrincipal = torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                });
            }

            GuardarCarrito(carrito);
            return true;
        }

        public async Task<bool> QuitarItem(int tortaId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.Items.FirstOrDefault(i => i.TortaId == tortaId);
            
            if (item != null)
            {
                carrito.Items.Remove(item);
                GuardarCarrito(carrito);
                return true;
            }
            
            return false;
        }

        public async Task<bool> ActualizarCantidad(int tortaId, int cantidad)
        {
            if (cantidad <= 0)
                return await QuitarItem(tortaId);

            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(tortaId);
            if (torta == null || torta.Stock < cantidad)
                return false;

            var carrito = ObtenerCarrito();
            var item = carrito.Items.FirstOrDefault(i => i.TortaId == tortaId);
            
            if (item != null)
            {
                item.Cantidad = cantidad;
                GuardarCarrito(carrito);
                return true;
            }
            
            return false;
        }

        public async Task<bool> ActualizarNotas(int tortaId, string? notas)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.Items.FirstOrDefault(i => i.TortaId == tortaId);
            
            if (item != null)
            {
                item.NotasPersonalizacion = notas;
                GuardarCarrito(carrito);
                return true;
            }
            
            return false;
        }

        public int GetTotalItems()
        {
            var carrito = ObtenerCarrito();
            return carrito.TotalItems;
        }

        public async Task<CarritoSession> ObtenerCarritoConDetalles()
        {
            var carrito = ObtenerCarrito();
            
            foreach (var item in carrito.Items)
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdWithImagenesAsync(item.TortaId);
                if (torta != null)
                {
                    item.PrecioUnitario = torta.Precio;
                    item.Nombre = torta.Nombre;
                    item.VendedorId = torta.VendedorId;
                    item.ImagenPrincipal = torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen;
                    
                    if (torta.Stock < item.Cantidad)
                    {
                        item.Cantidad = torta.Stock;
                    }
                }
            }
            
            carrito.Items.RemoveAll(i => 
            {
                var torta = _unitOfWork.TortaRepository.GetByIdAsync(i.TortaId).Result;
                return torta == null || !torta.Disponible || torta.Stock <= 0;
            });
            
            GuardarCarrito(carrito);
            return carrito;
        }

        public async Task<CarritoSession> FusionarCarritos(CarritoSession carritoAnonimo, int compradorId)
        {
            var carritoActual = ObtenerCarrito();
            
            foreach (var itemAnonimo in carritoAnonimo.Items)
            {
                var itemExistente = carritoActual.Items.FirstOrDefault(i => i.TortaId == itemAnonimo.TortaId);
                
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += itemAnonimo.Cantidad;
                }
                else
                {
                    carritoActual.Items.Add(itemAnonimo);
                }
            }
            
            GuardarCarrito(carritoActual);
            HttpContext.Response.Cookies.Delete(CARRITO_COOKIE_KEY);
            
            return carritoActual;
        }

        /// <summary>
        /// Convierte el carrito en una venta con cálculo de comisiones
        /// ✅ MODIFICADO: Ahora calcula comisiones y crea pago inicial
        /// </summary>
        public async Task<Venta?> ConvertirAVenta(int compradorId, string direccionEntrega, string? notas = null)
        {
            var carrito = await ObtenerCarritoConDetalles();
            
            if (!carrito.Items.Any())
                return null;

            // Validar stock de todos los items
            foreach (var item in carrito.Items)
            {
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(item.TortaId);
                if (torta == null || !torta.Disponible || torta.Stock < item.Cantidad)
                {
                    throw new InvalidOperationException($"No hay suficiente stock de {item.Nombre}");
                }
            }

            // ✅ Obtener configuración de comisión
            var comisionPorcentaje = await _unitOfWork.Configuracion.GetComisionPorcentajeAsync();

            // Crear la venta
            var venta = new Venta
            {
                CompradorId = compradorId,
                NumeroOrden = await _unitOfWork.Ventas.GenerarNumeroOrdenAsync(),
                FechaVenta = DateTime.Now,
                Estado = EstadoVenta.Pendiente,
                Subtotal = carrito.Subtotal,
                DescuentoTotal = carrito.DescuentoTotal,
                Total = carrito.Total,
                DireccionEntrega = direccionEntrega,
                NotasCliente = notas,
                FechaActualizacion = DateTime.Now,
                // ✅ NUEVOS: Campos de comisión
                PorcentajeComision = comisionPorcentaje
            };

            // ✅ Calcular comisiones
            venta.CalcularComisiones();

            await _unitOfWork.Ventas.AddAsync(venta);
            await _unitOfWork.SaveChangesAsync();

            // Crear los detalles de venta y actualizar stock
            foreach (var item in carrito.Items)
            {
                var detalle = new DetalleVenta
                {
                    VentaId = venta.Id,
                    TortaId = item.TortaId,
                    VendedorId = item.VendedorId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Descuento = item.Descuento,
                    Subtotal = item.Subtotal,
                    NotasPersonalizacion = item.NotasPersonalizacion,
                    Estado = EstadoDetalleVenta.Pendiente
                };

                await _unitOfWork.DetallesVenta.AddAsync(detalle);

                // Actualizar stock de la torta
                var torta = await _unitOfWork.TortaRepository.GetByIdAsync(item.TortaId);
                if (torta != null)
                {
                    torta.Stock -= item.Cantidad;
                    await _unitOfWork.TortaRepository.UpdateAsync(torta);
                }
            }

            // ✅ NUEVO: Crear pago inicial (pendiente)
            var pago = new Pago
            {
                VentaId = venta.Id,
                CompradorId = compradorId,
                Monto = venta.Total,
                ComisionPlataforma = venta.ComisionPlataforma,
                MontoVendedores = venta.MontoVendedores,
                FechaPago = DateTime.Now,
                Estado = EstadoPago.Pendiente
            };

            await _unitOfWork.PagoRepository.AddAsync(pago);
            await _unitOfWork.SaveChangesAsync();

            // ✅ NUEVO: Agregar monto pendiente a cada vendedor involucrado
            var vendedoresMontos = carrito.Items
                .GroupBy(i => i.VendedorId)
                .Select(g => new { VendedorId = g.Key, Monto = g.Sum(i => i.Subtotal) });

            foreach (var vm in vendedoresMontos)
            {
                await _unitOfWork.VendedorRepository.AgregarPendienteCobroAsync(vm.VendedorId, vm.Monto);
            }

            await _unitOfWork.SaveChangesAsync();

            // Vaciar el carrito
            VaciarCarrito();

            return venta;
        }
    }
}