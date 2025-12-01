using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure.Database;

namespace EduTrack.Infrastructure.Repository.Core
{
    public class AlumnoRepository : IAlumnoRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public AlumnoRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Guid> AddAsync(Alumno entity)
        {
            const string sql = @"INSERT INTO alumno (id, nombre_completo, grado_id, biometrico_id, nombre_apoderado, numero_apoderado)
                                  VALUES (@id, @nombre_completo, @grado_id, @biometrico_id, @nombre_apoderado, @numero_apoderado)
                                  RETURNING id";

            var id = entity.id == Guid.Empty ? Guid.NewGuid() : entity.id;

            var p = new DynamicParameters();
            p.Add("@id", id);
            p.Add("@nombre_completo", entity.nombre_completo);
            p.Add("@grado_id", entity.grado_id);
            p.Add("@biometrico_id", entity.biometrico_id);
            p.Add("@nombre_apoderado", entity.nombre_apoderado);
            p.Add("@numero_apoderado", entity.numero_apoderado);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                var returned = await connection.ExecuteScalarAsync<Guid>(sql, p);
                return returned;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM alumno WHERE id = @id";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, p);
            }
        }

        public async Task<AlumnoConGradoDto?> GetByBiometricoAsync(Guid gradoId, int biometricoId)
        {
            const string sql = @"SELECT al.id, al.nombre_completo, al.grado_id, al.biometrico_id, al.nombre_apoderado, al.numero_apoderado,
                                         g.nombre_grado
                                  FROM alumno al
                                  JOIN grado g ON al.grado_id = g.id
                                  WHERE al.grado_id = @grado_id AND al.biometrico_id = @biometrico_id
                                  LIMIT 1";

            var p = new DynamicParameters();
            p.Add("@grado_id", gradoId);
            p.Add("@biometrico_id", biometricoId);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return (await connection.QueryAsync<AlumnoConGradoDto>(sql, p)).FirstOrDefault();
            }
        }

        public async Task<AlumnoConGradoDto?> GetByIdAsync(Guid id)
        {
            const string sql = @"SELECT al.id, al.nombre_completo, al.grado_id, al.biometrico_id, al.nombre_apoderado, al.numero_apoderado,
                                         g.nombre_grado
                                  FROM alumno al
                                  JOIN grado g ON al.grado_id = g.id
                                  WHERE al.id = @id
                                  LIMIT 1";

            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return (await connection.QueryAsync<AlumnoConGradoDto>(sql, p)).FirstOrDefault();
            }
        }

        public async Task<PaginaDatos<AlumnoConGradoDto>> GetPagedAsync(string? search, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            var parametros = new
            {
                search = string.IsNullOrWhiteSpace(search) ? null : search,
                offset,
                pageSize
            };

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();

                var sql = @"
                    SELECT count(*)
                    FROM alumno a
                    WHERE (@search IS NULL
                    OR a.nombre_completo ILIKE '%' || @search || '%'
                    OR a.nombre_apoderado ILIKE '%' || @search || '%');

                    SELECT a.id, a.nombre_completo, a.grado_id, a.biometrico_id, a.nombre_apoderado, a.numero_apoderado,
                           g.nombre_grado
                    FROM alumno a
                    JOIN grado g ON a.grado_id = g.id
                    WHERE (@search IS NULL
                    OR a.nombre_completo ILIKE '%' || @search || '%'
                    OR a.nombre_apoderado ILIKE '%' || @search || '%')
                    ORDER BY a.nombre_completo
                    LIMIT @pageSize OFFSET @offset;";

                using var multi = await connection.QueryMultipleAsync(sql, parametros);
                var total = await multi.ReadSingleAsync<int>();
                var data = (await multi.ReadAsync<AlumnoConGradoDto>()).AsList();

                return new PaginaDatos<AlumnoConGradoDto>
                {
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = data
                };
            }
        }

        public async Task UpdateAsync(Alumno entity)
        {
            const string sql = @"UPDATE alumno SET
                                    nombre_completo = @nombre_completo,
                                    grado_id = @grado_id,
                                    biometrico_id = @biometrico_id,
                                    nombre_apoderado = @nombre_apoderado,
                                    numero_apoderado = @numero_apoderado
                                  WHERE id = @id";

            var p = new DynamicParameters();
            p.Add("@id", entity.id);
            p.Add("@nombre_completo", entity.nombre_completo);
            p.Add("@grado_id", entity.grado_id);
            p.Add("@biometrico_id", entity.biometrico_id);
            p.Add("@nombre_apoderado", entity.nombre_apoderado);
            p.Add("@numero_apoderado", entity.numero_apoderado);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, p);
            }
        }
    }
}
