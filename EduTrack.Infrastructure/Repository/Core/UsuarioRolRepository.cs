using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure.Database;

namespace EduTrack.Infrastructure.Repository.Core
{
    public class UsuarioRolRepository : IUsuarioRolRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public UsuarioRolRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<Rol>> GetAllAsync(Guid usuario_id)
        {
            const string sql = @"SELECT r.id, r.nombre
                                 FROM usuario_rol ur
                                 JOIN rol r ON ur.rol_id = r.id
                                 WHERE ur.usuario_id = @usuario_id
                                 ORDER BY r.id";

            var p = new DynamicParameters();
            p.Add("@usuario_id", usuario_id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                var items = await connection.QueryAsync<Rol>(sql, p);
                return items.AsList();
            }
        }
    }
}
