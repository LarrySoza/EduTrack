using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure.Database;

namespace App.Infrastructure.Repository.Core
{
    public class TipoConfiguracionRepository : ITipoConfiguracionRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public TipoConfiguracionRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<TipoConfiguracion>> GetAllAsync()
        {
            const string sql = "SELECT id, nombre FROM tipo_configuracion ORDER BY nombre";

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                var items = await connection.QueryAsync<TipoConfiguracion>(sql);
                return items.AsList();
            }
        }

        public async Task<TipoConfiguracion?> GetByIdAsync(string id)
        {
            const string sql = "SELECT id, nombre FROM tipo_configuracion WHERE id = @id";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return (await connection.QueryAsync<TipoConfiguracion>(sql, p)).FirstOrDefault();
            }
        }
    }
}
