using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.DTOs;
using CasaDeLasTortas.Models.Entities;
using CasaDeLasTortas.Services;

namespace CasaDeLasTortas.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagenTortaApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public ImagenTortaApiController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        /// <summary>
        /// Obtener todas las imágenes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int registrosPorPagina = 20)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetAllAsync(pagina, registrosPorPagina);
            var total = await _unitOfWork.ImagenesTorta.CountAsync();

            return Ok(new
            {
                data = imagenes,
                pagina,
                registrosPorPagina,
                totalRegistros = total,
                totalPaginas = (int)Math.Ceiling((double)total / registrosPorPagina)
            });
        }

        /// <summary>
        /// Obtener imagen por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(id);

            if (imagen == null)
                return NotFound(new { message = "Imagen no encontrada" });

            return Ok(imagen);
        }

        /// <summary>
        /// Obtener imágenes por torta
        /// </summary>
        [HttpGet("torta/{tortaId}")]
        public async Task<IActionResult> GetByTorta(int tortaId)
        {
            var imagenes = await _unitOfWork.ImagenesTorta.GetByTortaAsync(tortaId);
            return Ok(imagenes);
        }

        /// <summary>
        /// Obtener imagen principal de una torta
        /// </summary>
        [HttpGet("torta/{tortaId}/principal")]
        public async Task<IActionResult> GetPrincipal(int tortaId)
        {
            var imagen = await _unitOfWork.ImagenesTorta.GetPrincipalByTortaAsync(tortaId);

            if (imagen == null)
                return NotFound(new { message = "No hay imagen principal para esta torta" });

            return Ok(imagen);
        }

        /// <summary>
        /// Subir imagen de torta
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Upload([FromForm] ImagenTortaCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ArchivoImagen == null || dto.ArchivoImagen.Length == 0)
                return BadRequest(new { message = "Archivo de imagen requerido" });

            // Verificar que la torta existe
            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(dto.TortaId);
            if (torta == null)
                return BadRequest(new { message = "Torta no encontrada" });

            try
            {
                // Guardar archivo
                var rutaImagen = await _fileService.SaveFileAsync(dto.ArchivoImagen, "tortas");

                var imagen = new ImagenTorta
                {
                    TortaId = dto.TortaId,
                    UrlImagen = rutaImagen,
                    NombreArchivo = dto.ArchivoImagen.FileName,
                    TamanioArchivo = dto.ArchivoImagen.Length,
                    TipoArchivo = dto.ArchivoImagen.ContentType,
                    EsPrincipal = dto.EsPrincipal,
                    Orden = dto.Orden,
                    FechaSubida = DateTime.Now
                };

                // Si es principal, quitar el flag de otras imágenes
                if (dto.EsPrincipal)
                {
                    var imagenesExistentes = await _unitOfWork.ImagenesTorta.GetByTortaAsync(dto.TortaId);
                    foreach (var img in imagenesExistentes)
                    {
                        img.EsPrincipal = false;
                        _unitOfWork.ImagenesTorta.Update(img);
                    }
                }

                await _unitOfWork.ImagenesTorta.AddAsync(imagen);
                await _unitOfWork.SaveChangesAsync();

                var imagenDTO = new ImagenTortaDTO
                {
                    Id = imagen.Id,
                    TortaId = imagen.TortaId,
                    UrlImagen = imagen.UrlImagen,
                    NombreArchivo = imagen.NombreArchivo,
                    TamanioArchivo = imagen.TamanioArchivo,
                    TipoArchivo = imagen.TipoArchivo,
                    EsPrincipal = imagen.EsPrincipal,
                    Orden = imagen.Orden,
                    FechaSubida = imagen.FechaSubida
                };

                return CreatedAtAction(nameof(GetById), new { id = imagen.Id }, imagenDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al subir imagen: {ex.Message}" });
            }
        }

        /// <summary>
        /// Subir múltiples imágenes
        /// </summary>
        [HttpPost("multiple")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> UploadMultiple([FromForm] ImagenTortaUploadDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Imagenes == null || !dto.Imagenes.Any())
                return BadRequest(new { message = "Se requieren imágenes" });

            // Verificar que la torta existe
            var torta = await _unitOfWork.TortaRepository.GetByIdAsync(dto.TortaId);
            if (torta == null)
                return BadRequest(new { message = "Torta no encontrada" });

            var resultados = new List<ImagenTortaResponseDTO>();
            var imagenesCreadas = new List<ImagenTorta>();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Si hay imagen principal, quitar el flag de las existentes
                if (dto.ImagenPrincipalIndex.HasValue)
                {
                    var imagenesExistentes = await _unitOfWork.ImagenesTorta.GetByTortaAsync(dto.TortaId);
                    foreach (var img in imagenesExistentes)
                    {
                        img.EsPrincipal = false;
                        _unitOfWork.ImagenesTorta.Update(img);
                    }
                }

                for (int i = 0; i < dto.Imagenes.Count; i++)
                {
                    var archivo = dto.Imagenes[i];

                    try
                    {
                        var rutaImagen = await _fileService.SaveFileAsync(archivo, "tortas");

                        var imagen = new ImagenTorta
                        {
                            TortaId = dto.TortaId,
                            UrlImagen = rutaImagen,
                            NombreArchivo = archivo.FileName,
                            TamanioArchivo = archivo.Length,
                            TipoArchivo = archivo.ContentType,
                            EsPrincipal = dto.ImagenPrincipalIndex.HasValue && i == dto.ImagenPrincipalIndex.Value,
                            Orden = i,
                            FechaSubida = DateTime.Now
                        };

                        await _unitOfWork.ImagenesTorta.AddAsync(imagen);
                        imagenesCreadas.Add(imagen);

                        resultados.Add(new ImagenTortaResponseDTO
                        {
                            Id = imagen.Id,
                            UrlImagen = imagen.UrlImagen,
                            NombreArchivo = imagen.NombreArchivo,
                            EsPrincipal = imagen.EsPrincipal,
                            UploadExitoso = true
                        });
                    }
                    catch (Exception ex)
                    {
                        resultados.Add(new ImagenTortaResponseDTO
                        {
                            NombreArchivo = archivo.FileName,
                            UploadExitoso = false,
                            MensajeError = ex.Message
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Ok(new
                {
                    message = "Imágenes subidas correctamente",
                    resultados
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Eliminar archivos subidos en caso de error
                foreach (var imagen in imagenesCreadas)
                {
                    await _fileService.DeleteFileAsync(imagen.UrlImagen);
                }

                return BadRequest(new { message = $"Error al subir imágenes: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualizar imagen
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Update(int id, [FromBody] ImagenTortaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(id);

            if (imagen == null)
                return NotFound(new { message = "Imagen no encontrada" });

            // Si se marca como principal, quitar el flag de otras imágenes
            if (dto.EsPrincipal && !imagen.EsPrincipal)
            {
                var imagenesExistentes = await _unitOfWork.ImagenesTorta.GetByTortaAsync(imagen.TortaId);
                foreach (var img in imagenesExistentes.Where(i => i.Id != id))
                {
                    img.EsPrincipal = false;
                    _unitOfWork.ImagenesTorta.Update(img);
                }
            }

            imagen.EsPrincipal = dto.EsPrincipal;
            imagen.Orden = dto.Orden;

            _unitOfWork.ImagenesTorta.Update(imagen);
            await _unitOfWork.SaveChangesAsync();

            return Ok(imagen);
        }

        /// <summary>
        /// Establecer imagen como principal
        /// </summary>
        [HttpPatch("{id}/principal")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> SetPrincipal(int id)
        {
            var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(id);

            if (imagen == null)
                return NotFound(new { message = "Imagen no encontrada" });

            // Quitar el flag de otras imágenes de la misma torta
            var imagenesExistentes = await _unitOfWork.ImagenesTorta.GetByTortaAsync(imagen.TortaId);
            foreach (var img in imagenesExistentes)
            {
                img.EsPrincipal = (img.Id == id);
                _unitOfWork.ImagenesTorta.Update(img);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Imagen establecida como principal" });
        }

        /// <summary>
        /// Eliminar imagen
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Delete(int id)
        {
            var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(id);

            if (imagen == null)
                return NotFound(new { message = "Imagen no encontrada" });

            try
            {
                // Eliminar archivo físico
                await _fileService.DeleteFileAsync(imagen.UrlImagen);

                _unitOfWork.ImagenesTorta.Delete(imagen);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar imagen: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reordenar imágenes
        /// </summary>
        [HttpPut("reordenar")]
        [Authorize(Roles = "Admin,Vendedor")]
        public async Task<IActionResult> Reordenar([FromBody] List<ImagenTortaReordenarDTO> imagenes) 
        {
            if (!imagenes.Any())
                return BadRequest(new { message = "No se proporcionaron imágenes para reordenar" });

            try
            {
                foreach (var imagenDto in imagenes)
                {
                    var imagen = await _unitOfWork.ImagenesTorta.GetByIdAsync(imagenDto.Id);
                    if (imagen != null)
                    {
                        imagen.Orden = imagenDto.Orden;
                        _unitOfWork.ImagenesTorta.Update(imagen);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Imágenes reordenadas correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al reordenar imágenes: {ex.Message}" });
            }
        }
    }
}