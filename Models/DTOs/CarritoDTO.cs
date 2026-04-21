namespace CasaDeLasTortas.Models.DTOs
{
    public record AgregarItemRequest(int TortaId, int Cantidad, string? Notas = null);
    public record ActualizarItemRequest(int TortaId, int Cantidad);
    public record QuitarItemRequest(int TortaId);
    public record ActualizarNotasRequest(int TortaId, string? Notas);
}
