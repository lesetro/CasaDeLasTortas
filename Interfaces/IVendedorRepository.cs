using CasaDeLasTortas.Models.Entities;
using System.Linq.Expressions;

namespace CasaDeLasTortas.Interfaces
{
    public interface IVendedorRepository
    {
        // CRUD básico
        Task<Vendedor?> GetByIdAsync(int id);
        Task<IEnumerable<Vendedor>> GetAllAsync(int pagina = 1, int registrosPorPagina = 10);
        Task AddAsync(Vendedor vendedor);
        void Update(Vendedor vendedor);
        void Delete(Vendedor vendedor);
        Task UpdateAsync(Vendedor vendedor);
        Task DeleteAsync(Vendedor vendedor);
        
        // Con relaciones
        Task<Vendedor?> GetByIdWithPersonaAsync(int id);
        Task<Vendedor?> GetByIdWithTortasAsync(int id);
        Task<Vendedor?> GetByIdWithDetallesCompletosAsync(int id);
        Task<IEnumerable<Vendedor>> GetAllWithPersonaAsync();
        Task<Vendedor?> GetByIdWithLiberacionesAsync(int id);
        
        // Búsqueda por Persona
        Task<Vendedor?> GetByPersonaIdAsync(int idPersona);
        Task<bool> ExistsByPersonaIdAsync(int idPersona);
        Task<IEnumerable<Persona>> GetPersonasSinVendedorAsync();
        
        // Filtros
        Task<IEnumerable<Vendedor>> GetByEspecialidadAsync(string especialidad);
        Task<IEnumerable<Vendedor>> GetActivosAsync();
        Task<IEnumerable<Vendedor>> GetVerificadosAsync();
        Task<IEnumerable<Vendedor>> SearchAsync(string termino);
        
        //  Filtros de datos de pago
        Task<IEnumerable<Vendedor>> GetConDatosPagoCompletosAsync();
        Task<IEnumerable<Vendedor>> GetSinDatosPagoAsync();
        Task<IEnumerable<Vendedor>> GetConPendientesCobroAsync();
        
        // Estadísticas
        Task<int> CountAsync();
        Task<int> CountActivosAsync();
        Task<decimal> GetCalificacionPromedioAsync(int idVendedor);
        Task<int> GetTotalVentasAsync(int idVendedor);
        Task<int> GetCountAsync();
        
        // Estadísticas financieras
        Task<decimal> GetTotalCobradoAsync(int vendedorId);
        Task<decimal> GetTotalComisionesAsync(int vendedorId);
        Task<decimal> GetPendienteCobroAsync(int vendedorId);
        Task<int> CountConDatosPagoAsync();
        
        // Ordenamiento
        Task<IEnumerable<Vendedor>> GetTopByCalificacionAsync(int cantidad = 10);
        Task<IEnumerable<Vendedor>> GetTopByVentasAsync(int cantidad = 10);
        Task<IEnumerable<Vendedor>> GetTopByIngresosAsync(int cantidad = 10);

        // Verificaciones
        Task<bool> ExistsAsync(Expression<Func<Vendedor, bool>> predicate);
        Task<bool> TieneDatosPagoCompletosAsync(int vendedorId);
        Task<bool> PuedePublicarTortasAsync(int vendedorId);
        
        // Operaciones de datos de pago
        Task ActualizarDatosPagoAsync(int vendedorId, string aliasCBU, string cbu, string banco, 
                                       string titularCuenta, string? cuit = null, string? imagenQR = null);
        Task AgregarPendienteCobroAsync(int vendedorId, decimal monto);
        Task ProcesarLiberacionAsync(int vendedorId, decimal montoBruto, decimal comision);
        Task<(decimal TotalCobrado, decimal TotalComisiones, decimal PendienteCobro)> GetResumenFinancieroAsync(int vendedorId);
    }
}