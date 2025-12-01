using Dapper;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure;
using EduTrack.Infrastructure.Database;

namespace App.Infrastructure.Repository.Core
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public UsuarioRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task UpdatePasswordAsync(Guid id, string password)
        {
            string _query = @"UPDATE usuario SET
                                    contrasena_hash=@contrasena_hash,
                                    sello_seguridad=@sello_seguridad
                              WHERE 
                                    id=@id";

            var _parametros = new DynamicParameters();
            _parametros.Add("@contrasena_hash", Crypto.HashPassword(password));
            _parametros.Add("@sello_seguridad", Guid.NewGuid());
            _parametros.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                await connection.ExecuteAsync(_query, _parametros);
            }
        }

        public async Task<Usuario?> GetByNameAsync(string name)
        {
            const string sql = @"
                SELECT *
                FROM usuario
                WHERE activo = true
                  AND lower(nombre_usuario) = lower(@name)
                LIMIT 1;";

            var p = new DynamicParameters();
            p.Add("@name", name);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, p);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            // La tabla no tiene deleted_at; marcar como inactivo
            const string sql = "UPDATE usuario SET activo = false WHERE id = @id";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, p);
            }
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM usuario WHERE id = @id AND activo = true";
            var p = new DynamicParameters();
            p.Add("@id", id);

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, p);
            }
        }

        public async Task UpdateAsync(Usuario entity, List<Rol> roles)
        {
            // La tabla `usuario` tiene columnas limitadas; actualizar solo las columnas presentes
            const string updateSql = @"UPDATE usuario SET
                                            nombre_usuario = @nombre_usuario,
                                            activo = @activo
                                        WHERE id = @id";

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    // actualizar usuario
                    await connection.ExecuteAsync(updateSql, new
                    {
                        id = entity.id,
                        nombre_usuario = entity.nombre_usuario,
                        activo = entity.activo
                    }, tx);

                    // reemplazar roles: eliminar existentes y agregar los nuevos
                    const string deleteRolesSql = "DELETE FROM usuario_rol WHERE usuario_id = @usuario_id";
                    await connection.ExecuteAsync(deleteRolesSql, new { usuario_id = entity.id }, tx);

                    if (roles != null && roles.Count > 0)
                    {
                        const string insertRoleSql = "INSERT INTO usuario_rol (usuario_id, rol_id) VALUES (@usuario_id, @rol_id)";
                        var roleParams = roles.Select(r => new { usuario_id = entity.id, rol_id = r.id }).ToList();
                        await connection.ExecuteAsync(insertRoleSql, roleParams, tx);
                    }

                    tx.Commit();
                }
            }
        }

        public async Task<bool> ValidatePasswordAsync(Guid id, string password)
        {
            var _usuario = await GetByIdAsync(id);
            if (_usuario != null)
            {
                return Crypto.VerifyHashedPassword(_usuario.contrasena_hash!, password);
            }

            return false;
        }

        public async Task<Guid> AddAsync(Usuario usuario, List<Rol> roles)
        {
            const string insertSql = @"INSERT INTO usuario (
                                            nombre_usuario,
                                            contrasena_hash,
                                            sello_seguridad,
                                            activo
                                        ) VALUES (
                                            @nombre_usuario,
                                            @contrasena_hash,
                                            @sello_seguridad,
                                            @activo
                                        ) RETURNING id";

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    var newId = await connection.ExecuteScalarAsync<Guid>(insertSql, new
                    {
                        nombre_usuario = usuario.nombre_usuario,
                        contrasena_hash = usuario.contrasena_hash,
                        sello_seguridad = usuario.sello_seguridad == Guid.Empty ? Guid.NewGuid() : usuario.sello_seguridad,
                        activo = usuario.activo
                    }, tx);

                    if (roles != null && roles.Count > 0)
                    {
                        const string insertRoleSql = "INSERT INTO usuario_rol (usuario_id, rol_id) VALUES (@usuario_id, @rol_id)";
                        var roleParams = roles.Select(r => new { usuario_id = newId, rol_id = r.id }).ToList();
                        await connection.ExecuteAsync(insertRoleSql, roleParams, tx);
                    }

                    tx.Commit();
                    return newId;
                }
            }
        }

        public async Task<PaginaDatos<Usuario>> GetPagedAsync(string? search, int page = 1, int pageSize = 20, bool includeRoles = false)
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

                const string countSql = @"SELECT count(*) FROM usuario u
                                          WHERE u.activo = true
                                          AND (@search IS NULL
                                               OR u.nombre_usuario ILIKE '%' || @search || '%')";

                const string dataSql = @"SELECT u.* FROM usuario u
                                         WHERE u.activo = true
                                           AND (@search IS NULL
                                                OR u.nombre_usuario ILIKE '%' || @search || '%')
                                         ORDER BY u.nombre_usuario
                                         LIMIT @pageSize OFFSET @offset";

                var total = await connection.ExecuteScalarAsync<int>(countSql, parametros);
                var items = (await connection.QueryAsync<Usuario>(dataSql, parametros)).AsList();

                // Si no se requieren roles, devolver
                if (!includeRoles || items.Count == 0)
                {
                    return new PaginaDatos<Usuario>
                    {
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        totalPages = (int)Math.Ceiling(total / (double)pageSize),
                        data = items
                    };
                }

                // Cargar roles para todos los usuarios en una sola consulta
                var ids = items.Select(i => i.id).ToArray();
                const string rolesSql = @"SELECT ur.usuario_id, r.id as rol_id, r.nombre as rol_nombre
                                          FROM usuario_rol ur
                                          JOIN rol r ON ur.rol_id = r.id
                                          WHERE ur.usuario_id = ANY(@ids)";

                var roleRows = await connection.QueryAsync(rolesSql, new { ids });

                var lookup = new Dictionary<Guid, List<Rol>>();
                foreach (var row in roleRows)
                {
                    Guid usuarioId = row.usuario_id;
                    string rolId = row.rol_id;
                    string rolNombre = row.rol_nombre;

                    if (!lookup.TryGetValue(usuarioId, out var list))
                    {
                        list = new List<Rol>();
                        lookup[usuarioId] = list;
                    }

                    list.Add(new Rol { id = rolId, nombre = rolNombre });
                }

                // Asignar roles a cada usuario
                foreach (var u in items)
                {
                    if (lookup.TryGetValue(u.id, out var rl))
                    {
                        u.roles = rl;
                    }
                }

                return new PaginaDatos<Usuario>
                {
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = items
                };
            }
        }
    }
}
