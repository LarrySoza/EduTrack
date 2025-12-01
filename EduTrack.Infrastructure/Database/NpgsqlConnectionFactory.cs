using EduTrack.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace EduTrack.Infrastructure.Database
{
    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(UnitOfWork.DefaultConnection)
                                ?? throw new InvalidOperationException($"No se encontró la cadena de conexión '{UnitOfWork.DefaultConnection}'.");
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
