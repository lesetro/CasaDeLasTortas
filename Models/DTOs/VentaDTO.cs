namespace CasaDeLasTortas.Models.DTOs
{
    public record CrearVentaRequest(string DireccionEntrega, string? Notas = null);
    public record CancelarVentaRequest(string? Motivo = null);
    public record ActualizarEstadoRequest(string Estado, string? Notas = null);
    public record ActualizarPagoRequest(string Estado);
    public record ActualizarVentaRequest(string Estado, string? Notas = null);
}
