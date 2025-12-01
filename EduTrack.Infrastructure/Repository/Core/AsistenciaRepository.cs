using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure.Database;

namespace EduTrack.Infrastructure.Repository.Core
{
    public class AsistenciaRepository : IAsistenciaRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public AsistenciaRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task AddAsync(Asistencia entity)
        {
            const string sql = @"INSERT INTO asistencia (alumno_id)
                                  VALUES (@alumno_id)";

            var id = entity.id == Guid.Empty ? Guid.NewGuid() : entity.id;

            var p = new DynamicParameters();
            p.Add("@alumno_id", entity.alumno_id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, p);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM asistencia WHERE id = @id";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, p);
            }
        }

        public async Task<PaginaDatos<AsistenciaConDetallesDto>> GetPagedByAlumnoAsync(Guid alumnoId, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            var parametros = new
            {
                alumnoId,
                pageSize,
                offset
            };

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();

                var sql = @"
                    SELECT count(*)
                    FROM asistencia a
                    WHERE a.alumno_id = @alumnoId;

                    SELECT a.id, a.alumno_id, a.fecha_hora,
                           al.nombre_completo AS nombre_completo_alumno,
                           al.grado_id,
                           g.nombre_grado
                    FROM asistencia a
                    JOIN alumno al ON a.alumno_id = al.id
                    JOIN grado g ON al.grado_id = g.id
                    WHERE a.alumno_id = @alumnoId
                    ORDER BY a.fecha_hora DESC
                    LIMIT @pageSize OFFSET @offset;";

                using var multi = await connection.QueryMultipleAsync(sql, parametros);
                var total = await multi.ReadSingleAsync<int>();
                var data = (await multi.ReadAsync<AsistenciaConDetallesDto>()).AsList();

                return new PaginaDatos<AsistenciaConDetallesDto>
                {
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = data
                };
            }
        }

        public async Task<PaginaDatos<AsistenciaConDetallesDto>> GetPagedByFechaAsync(DateTime fecha, int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            var parametros = new
            {
                fecha = fecha.Date,
                pageSize,
                offset
            };

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();

                var sql = @"
                    SELECT count(*)
                    FROM asistencia a
                    WHERE a.fecha_hora::date = @fecha;

                    SELECT a.id, a.alumno_id, a.fecha_hora,
                           al.nombre_completo AS nombre_completo_alumno,
                           al.grado_id,
                           g.nombre_grado
                    FROM asistencia a
                    JOIN alumno al ON a.alumno_id = al.id
                    JOIN grado g ON al.grado_id = g.id
                    WHERE a.fecha_hora::date = @fecha
                    ORDER BY a.fecha_hora DESC
                    LIMIT @pageSize OFFSET @offset;";

                using var multi = await connection.QueryMultipleAsync(sql, parametros);
                var total = await multi.ReadSingleAsync<int>();
                var data = (await multi.ReadAsync<AsistenciaConDetallesDto>()).AsList();

                return new PaginaDatos<AsistenciaConDetallesDto>
                {
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = data
                };
            }
        }
    }
}
