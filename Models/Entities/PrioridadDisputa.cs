namespace CasaDeLasTortas.Models.Entities
{
    /// <summary>
    /// Nivel de prioridad para disputas entre compradores y vendedores.
    /// Usado en DisputaApiController.
    /// </summary>
    public enum PrioridadDisputa
    {
        Baja = 0,
        Media = 1,
        Alta = 2,
        Urgente = 3
    }
}