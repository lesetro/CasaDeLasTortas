using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Interfaces
{
    public interface IConfiguracionRepository
    {
        Task<ConfiguracionPlataforma?> GetConfiguracionAsync();
        Task<ConfiguracionPlataforma> GetOrCreateAsync();
        Task UpdateAsync(ConfiguracionPlataforma config);
        Task<decimal> GetComisionPorcentajeAsync();
        Task<int> GetDiasParaLiberarAsync();
        Task<int> GetMaxIntentosRechazadosAsync();
        Task<bool> PlataformaActivaAsync();
    }
}