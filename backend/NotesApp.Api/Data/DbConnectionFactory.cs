using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace NotesApp.Api.Data
{
    /// <summary>
    /// Abstraction over creation of ADO.NET connections to PostgreSQL.
    /// Keeps connection-string / provider concerns out of repositories.
    /// </summary>
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateOpenConnectionAsync();
    }

    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "Missing 'ConnectionStrings:Postgres' configuration value.");
        }

        public async Task<IDbConnection> CreateOpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
