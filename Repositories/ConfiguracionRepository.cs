using Microsoft.EntityFrameworkCore;
using CasaDeLasTortas.Data;
using CasaDeLasTortas.Interfaces;
using CasaDeLasTortas.Models.Entities;

namespace CasaDeLasTortas.Repositories
{
    public class ConfiguracionRepository : IConfiguracionRepository
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracionPlataforma?> GetConfiguracionAsync()
        {
            return await _context.ConfiguracionPlataforma.FirstOrDefaultAsync();
        }

        public async Task<ConfiguracionPlataforma> GetOrCreateAsync()
        {
            var config = await _context.ConfiguracionPlataforma.FirstOrDefaultAsync();
            
            if (config == null)
            {
                config = ConfiguracionPlataforma.CrearDefault();
                await _context.ConfiguracionPlataforma.AddAsync(config);
                await _context.SaveChangesAsync();
            }
            
            return config;
        }

        public async Task UpdateAsync(ConfiguracionPlataforma config)
        {
            config.FechaActualizacion = DateTime.Now;
            _context.ConfiguracionPlataforma.Update(config);
            await Task.CompletedTask;
        }

        public async Task<decimal> GetComisionPorcentajeAsync()
        {
            var config = await GetConfiguracionAsync();
            return config?.ComisionPorcentaje ?? 10.00m;
        }

        public async Task<int> GetDiasParaLiberarAsync()
        {
            var config = await GetConfiguracionAsync();
            return config?.DiasParaLiberarFondos ?? 3;
        }

        public async Task<int> GetMaxIntentosRechazadosAsync()
        {
            var config = await GetConfiguracionAsync();
            return config?.MaxIntentosRechazados ?? 3;
        }

        public async Task<bool> PlataformaActivaAsync()
        {
            var config = await GetConfiguracionAsync();
            return config?.PlataformaActiva ?? true;
        }
    }
}