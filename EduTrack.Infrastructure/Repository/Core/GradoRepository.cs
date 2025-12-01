using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure.Database;

namespace EduTrack.Infrastructure.Repository.Core
{
    public class GradoRepository : IGradoRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public GradoRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<Grado>> GetAllAsync()
        {
            const string sql = "SELECT id, nombre_grado FROM grado ORDER BY nombre_grado";

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                var items = await connection.QueryAsync<Grado>(sql);
                return items.AsList();
            }
        }

        public async Task<Grado?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT id, nombre_grado FROM grado WHERE id = @id";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return (await connection.QueryAsync<Grado>(sql, p)).FirstOrDefault();
            }
        }
    }
}
