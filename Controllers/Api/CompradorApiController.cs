using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class CompradorApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompradorApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtener todos los compradores
        /// </summary>
        [HttpGet]
        
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
        {
            var compradores = await _unitOfWork.CompradorRepository.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.CompradorRepository.CountAsync();

            return Ok(new
            {
                data = compradores,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener comprador por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            return Ok(comprador);
        }

        /// <summary>
        /// Obtener comprador por IdPersona
        /// </summary>
        [HttpGet("persona/{personaId}")]
        public async Task<IActionResult> GetByPersonaId(int personaId)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(personaId);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            return Ok(comprador);
        }

        /// <summary>
        /// Crear nuevo comprador
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompradorCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que la persona existe
            var persona = await _unitOfWork.PersonaRepository.GetByIdAsync(dto.PersonaId);
            if (persona == null)
                return BadRequest(new { message = "Persona no encontrada" });

            // Verificar que la persona no sea ya un comprador
            var compradorExistente = await _unitOfWork.CompradorRepository.GetByPersonaIdAsync(dto.PersonaId);
            if (compradorExistente != null)
                return BadRequest(new { message = "Esta persona ya está registrada como comprador" });

            var comprador = new Comprador
            {
                PersonaId = dto.PersonaId,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Ciudad = dto.Ciudad,
                Provincia = dto.Provincia,
                CodigoPostal = dto.CodigoPostal,
                FechaNacimiento = dto.FechaNacimiento,
                Preferencias = dto.Preferencias,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            await _unitOfWork.CompradorRepository.AddAsync(comprador);
            await _unitOfWork.SaveChangesAsync();

            // Obtener el comprador creado con información de persona
            var compradorCreado = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(comprador.Id);

            return CreatedAtAction(nameof(GetById), new { id = comprador.Id }, compradorCreado);
        }

        /// <summary>
        /// Actualizar comprador existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompradorUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            comprador.Direccion = dto.Direccion;
            comprador.Telefono = dto.Telefono;
            comprador.Ciudad = dto.Ciudad;
            comprador.Provincia = dto.Provincia;
            comprador.CodigoPostal = dto.CodigoPostal;
            comprador.FechaNacimiento = dto.FechaNacimiento;
            comprador.Preferencias = dto.Preferencias;
            comprador.Activo = dto.Activo;

            _unitOfWork.CompradorRepository.Update(comprador);
            await _unitOfWork.SaveChangesAsync();

            var compradorActualizado = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);
            return Ok(compradorActualizado);
        }

        /// <summary>
        /// Eliminar comprador (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
            var comprador = await _unitOfWork.CompradorRepository.GetByIdAsync(id);

            if (comprador == null)
                return NotFound(new { message = "Comprador no encontrado" });

            // Soft delete - marcar como inactivo
            comprador.Activo = false;
            _unitOfWork.CompradorRepository.Update(comprador);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtener pagos del comprador
        /// </summary>
        [HttpGet("{id}/pagos")]
public async Task<IActionResult> GetPagos(int id)
{
    var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);

    if (comprador == null)
        return NotFound(new { message = "Comprador no encontrado" });

    // Obtener pagos a través de Ventas
    var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(id);
    var pagos = ventas.SelectMany(v => v.Pagos).ToList();

    var pagosDTO = pagos.Select(p => new
    {
        id = p.Id,
        ventaId = p.VentaId,
        numeroOrden = p.Venta?.NumeroOrden,
        monto = p.Monto,
        fechaPago = p.FechaPago,
        estado = p.Estado.ToString(),
        metodoPago = p.MetodoPago?.ToString(),
        tieneComprobante = !string.IsNullOrEmpty(p.ArchivoComprobante),
        totalItems = p.Venta?.Detalles?.Sum(d => d.Cantidad) ?? 0,
        productos = p.Venta?.Detalles?.Select(d => new
        {
            tortaId = d.TortaId,
            nombreTorta = d.Torta.Nombre,
            cantidad = d.Cantidad,
            precioUnitario = d.PrecioUnitario,
            imagenTorta = d.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
        }).ToList()
    }).ToList();

    return Ok(pagosDTO);
}

[HttpGet("{id}/estadisticas")]
public async Task<IActionResult> GetEstadisticas(int id)
{
    var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);

    if (comprador == null)
        return NotFound(new { message = "Comprador no encontrado" });

    // Obtener ventas y pagos
    var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(id);
    var pagosCompletados = ventas.SelectMany(v => v.Pagos.Where(p => p.Estado == EstadoPago.Completado)).ToList();
    
    var totalCompras = pagosCompletados.Count;
    var totalGastado = pagosCompletados.Sum(p => p.Monto);
    var compraPromedio = totalCompras > 0 ? totalGastado / totalCompras : 0;

    // Compras del último mes
    var ultimoMes = DateTime.Now.AddMonths(-1);
    var comprasUltimoMes = pagosCompletados.Count(p => p.FechaPago >= ultimoMes);
    var gastoUltimoMes = pagosCompletados.Where(p => p.FechaPago >= ultimoMes).Sum(p => p.Monto);

    return Ok(new
    {
        idComprador = comprador.Id,
        nombrePersona = comprador.Persona?.Nombre ?? "N/A",
        email = comprador.Persona?.Email ?? "N/A",
        totalCompras,
        totalGastado,
        compraPromedio,
        comprasUltimoMes,
        gastoUltimoMes,
        fechaPrimeraCompra = pagosCompletados.Any() ? pagosCompletados.Min(p => p.FechaPago) : (DateTime?)null,
        fechaUltimaCompra = pagosCompletados.Any() ? pagosCompletados.Max(p => p.FechaPago) : (DateTime?)null
    });
}

/// <summary>
/// Obtener historial de compras del comprador (MÉTODO CORREGIDO)
/// </summary>
[HttpGet("{id}/historial")]
public async Task<IActionResult> GetHistorial(int id, [FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 10)
{
    var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(id);
    
    var historial = ventas
        .OrderByDescending(v => v.FechaVenta)
        .Skip((pagina - 1) * registrosPorPagina)
        .Take(registrosPorPagina)
        .Select(v => new
        {
            ventaId = v.Id,
            numeroOrden = v.NumeroOrden,
            fechaVenta = v.FechaVenta,
            estado = v.Estado.ToString(),
            total = v.Total,
            totalItems = v.Detalles.Sum(d => d.Cantidad),
            pagos = v.Pagos.Select(p => new
            {
                pagoId = p.Id,
                monto = p.Monto,
                fechaPago = p.FechaPago,
                estado = p.Estado.ToString(),
                metodoPago = p.MetodoPago?.ToString()
            }),
            productos = v.Detalles.Select(d => new
            {
                tortaId = d.TortaId,
                nombreTorta = d.Torta.Nombre,
                cantidad = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal = d.Subtotal,
                imagenTorta = d.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
            })
        })
        .ToList();

    var totalVentas = ventas.Count();

    return Ok(new
    {
        data = historial,
        pagina,
        registrosPorPagina,
        totalRegistros = totalVentas,
        totalPaginas = (int)Math.Ceiling((double)totalVentas / registrosPorPagina)
    });
}

/// <summary>
/// Obtener perfil completo del comprador (MÉTODO CORREGIDO)
/// </summary>
[HttpGet("{id}/perfil")]
public async Task<IActionResult> GetPerfil(int id)
{
    var comprador = await _unitOfWork.CompradorRepository.GetByIdWithPersonaAsync(id);

    if (comprador == null)
        return NotFound(new { message = "Comprador no encontrado" });

    var ventas = await _unitOfWork.Ventas.GetByCompradorIdWithDetailsAsync(id);
    var pagosCompletados = ventas.SelectMany(v => v.Pagos.Where(p => p.Estado == EstadoPago.Completado)).ToList();
    var totalGastado = pagosCompletados.Sum(p => p.Monto);

    var historialCompras = ventas
        .OrderByDescending(v => v.FechaVenta)
        .Take(10)
        .Select(v => new
        {
            ventaId = v.Id,
            numeroOrden = v.NumeroOrden,
            fechaVenta = v.FechaVenta,
            total = v.Total,
            estado = v.Estado.ToString(),
            productos = v.Detalles.Select(d => new
            {
                tortaId = d.TortaId,
                nombreTorta = d.Torta.Nombre,
                cantidad = d.Cantidad,
                imagenTorta = d.Torta.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
            }).ToList()
        })
        .ToList();

    var perfil = new
    {
        id = comprador.Id,
        personaId = comprador.PersonaId,
        nombrePersona = comprador.Persona?.Nombre ?? "N/A",
        email = comprador.Persona?.Email ?? "N/A",
        telefono = comprador.Telefono,
        direccion = comprador.Direccion,
        ciudad = comprador.Ciudad,
        provincia = comprador.Provincia,
        codigoPostal = comprador.CodigoPostal,
        fechaNacimiento = comprador.FechaNacimiento,
        avatar = comprador.Persona?.Avatar,
        fechaRegistro = comprador.FechaCreacion,
        totalCompras = comprador.TotalCompras,
        totalGastado = totalGastado,
        historialCompras = historialCompras
    };

    return Ok(perfil);
}
    }
}