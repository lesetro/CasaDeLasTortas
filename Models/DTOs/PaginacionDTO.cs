using System;
using System.Collections.Generic;

namespace CasaDeLasTortas.Models.DTOs
{
    public class PaginacionDTO<T>
    {
        public List<T> Items { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalItems { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        public PaginacionDTO()
        {
            Items = new List<T>();
        }

        public PaginacionDTO(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalItems = count;
            TamanioPagina = pageSize;
            PaginaActual = pageNumber;
            TotalPaginas = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }

    public class PaginacionParametrosDTO
    {
        const int maxPageSize = 50;
        public int NumeroPagina { get; set; } = 1;
        
        private int _tamanioPagina = 10;
        public int TamanioPagina
        {
            get => _tamanioPagina;
            set => _tamanioPagina = (value > maxPageSize) ? maxPageSize : value;
        }
        
        public string? OrdenarPor { get; set; }
        public bool OrdenDescendente { get; set; } = false;
        public string? Busqueda { get; set; }
        public string? Filtro { get; set; }
    }

    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int? StatusCode { get; set; }

        public ResponseDTO()
        {
            Success = false;
            Errors = new List<string>();
        }

        public static ResponseDTO<T> SuccessResponse(T data, string message = "Operación exitosa")
        {
            return new ResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static ResponseDTO<T> ErrorResponse(string message, int statusCode = 400)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ResponseDTO<T> ErrorResponse(List<string> errors, string message = "Se encontraron errores", int statusCode = 400)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }
}