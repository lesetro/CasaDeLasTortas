using CasaDeLasTortas.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CasaDeLasTortas.Helpers
{
    /// <summary>
    /// Helper para facilitar la paginación de listas y consultas
    /// Proporciona métodos estáticos para crear objetos de paginación
    /// </summary>
    public static class PaginacionHelper
    {
        /// <summary>
        /// Crea un objeto PaginacionDTO a partir de una lista en memoria
        /// </summary>
        /// <typeparam name="T">Tipo de entidad a paginar</typeparam>
        /// <param name="source">Lista fuente de datos</param>
        /// <param name="pageNumber">Número de página actual (1-based)</param>
        /// <param name="pageSize">Tamaño de la página (elementos por página)</param>
        /// <returns>Objeto PaginacionDTO con los datos paginados</returns>
        public static PaginacionDTO<T> CrearPaginacion<T>(
            List<T> source,
            int pageNumber,
            int pageSize)
        {
            // Validar parámetros
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Límite máximo de seguridad

            // Calcular totales
            var totalItems = source.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Asegurar que pageNumber no exceda el total de páginas
            if (pageNumber > totalPages && totalPages > 0)
            {
                pageNumber = totalPages;
            }

            // Obtener items de la página actual
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Crear objeto de paginación
            return new PaginacionDTO<T>
            {
                Items = items,
                PaginaActual = pageNumber,
                TamanioPagina = pageSize,
                TotalItems = totalItems,
                TotalPaginas = totalPages
            };
        }

        /// <summary>
        /// Crea un objeto PaginacionDTO a partir de un IQueryable (consulta de base de datos)
        /// Versión asíncrona para uso con Entity Framework
        /// </summary>
        /// <typeparam name="T">Tipo de entidad a paginar</typeparam>
        /// <param name="query">Query de base de datos</param>
        /// <param name="pageNumber">Número de página actual (1-based)</param>
        /// <param name="pageSize">Tamaño de la página</param>
        /// <returns>Objeto PaginacionDTO con los datos paginados</returns>
        public static async Task<PaginacionDTO<T>> CrearPaginacionAsync<T>(
            IQueryable<T> query,
            int pageNumber,
            int pageSize) where T : class
        {
            // Validar parámetros
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Contar total de items (ejecuta COUNT en BD)
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Asegurar que pageNumber no exceda el total de páginas
            if (pageNumber > totalPages && totalPages > 0)
            {
                pageNumber = totalPages;
            }

            // Obtener items de la página actual (ejecuta SELECT con LIMIT en BD)
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginacionDTO<T>
            {
                Items = items,
                PaginaActual = pageNumber,
                TamanioPagina = pageSize,
                TotalItems = totalItems,
                TotalPaginas = totalPages
            };
        }

        /// <summary>
        /// Valida y normaliza parámetros de paginación
        /// </summary>
        /// <param name="pageNumber">Número de página (input)</param>
        /// <param name="pageSize">Tamaño de página (input)</param>
        /// <returns>Tupla con valores validados y normalizados</returns>
        public static (int pageNumber, int pageSize) ValidarParametros(int pageNumber, int pageSize)
        {
            // Valores por defecto
            const int DEFAULT_PAGE_NUMBER = 1;
            const int DEFAULT_PAGE_SIZE = 10;
            const int MIN_PAGE_SIZE = 1;
            const int MAX_PAGE_SIZE = 100;

            // Validar y corregir pageNumber
            if (pageNumber < 1)
            {
                pageNumber = DEFAULT_PAGE_NUMBER;
            }

            // Validar y corregir pageSize
            if (pageSize < MIN_PAGE_SIZE)
            {
                pageSize = DEFAULT_PAGE_SIZE;
            }
            else if (pageSize > MAX_PAGE_SIZE)
            {
                pageSize = MAX_PAGE_SIZE;
            }

            return (pageNumber, pageSize);
        }

        /// <summary>
        /// Calcula el número total de páginas
        /// </summary>
        /// <param name="totalItems">Total de elementos</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Número total de páginas</returns>
        public static int CalcularTotalPaginas(int totalItems, int pageSize)
        {
            if (pageSize <= 0) return 0;
            return (int)Math.Ceiling(totalItems / (double)pageSize);
        }

        /// <summary>
        /// Calcula el índice del primer elemento en la página actual
        /// </summary>
        /// <param name="pageNumber">Número de página actual</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Índice inicial (0-based para Skip)</returns>
        public static int CalcularSkip(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            return (pageNumber - 1) * pageSize;
        }

        /// <summary>
        /// Genera metadata de paginación útil para APIs
        /// </summary>
        /// <param name="totalItems">Total de elementos</param>
        /// <param name="pageNumber">Página actual</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Objeto anónimo con metadata</returns>
        public static object CrearMetadata(int totalItems, int pageNumber, int pageSize)
        {
            var totalPages = CalcularTotalPaginas(totalItems, pageSize);

            return new
            {
                totalItems,
                pageNumber,
                pageSize,
                totalPages,
                hasPreviousPage = pageNumber > 1,
                hasNextPage = pageNumber < totalPages,
                firstItemIndex = CalcularSkip(pageNumber, pageSize) + 1,
                lastItemIndex = Math.Min(CalcularSkip(pageNumber, pageSize) + pageSize, totalItems)
            };
        }

        /// <summary>
        /// Extensión para IQueryable que facilita la paginación
        /// </summary>
        public static IQueryable<T> Paginar<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var (validPageNumber, validPageSize) = ValidarParametros(pageNumber, pageSize);
            return query
                .Skip((validPageNumber - 1) * validPageSize)
                .Take(validPageSize);
        }

        /// <summary>
        /// Extensión para List que facilita la paginación
        /// </summary>
        public static List<T> Paginar<T>(
            this List<T> list,
            int pageNumber,
            int pageSize)
        {
            var (validPageNumber, validPageSize) = ValidarParametros(pageNumber, pageSize);
            return list
                .Skip((validPageNumber - 1) * validPageSize)
                .Take(validPageSize)
                .ToList();
        }

        /// <summary>
        /// Genera URLs para navegación de paginación
        /// </summary>
        /// <param name="baseUrl">URL base sin parámetros</param>
        /// <param name="pageNumber">Página actual</param>
        /// <param name="totalPages">Total de páginas</param>
        /// <returns>Objeto con URLs de navegación</returns>
        public static object GenerarUrlsNavegacion(string baseUrl, int pageNumber, int totalPages)
        {
            var separator = baseUrl.Contains("?") ? "&" : "?";

            return new
            {
                first = $"{baseUrl}{separator}pagina=1",
                previous = pageNumber > 1 ? $"{baseUrl}{separator}pagina={pageNumber - 1}" : null,
                current = $"{baseUrl}{separator}pagina={pageNumber}",
                next = pageNumber < totalPages ? $"{baseUrl}{separator}pagina={pageNumber + 1}" : null,
                last = $"{baseUrl}{separator}pagina={totalPages}"
            };
        }
    }
}