using System;
using System.Threading.Tasks;
using Dapper;
using NotesApp.Api.Data;
using NotesApp.Api.Models;

namespace NotesApp.Api.Repositories
{
    /// <summary>
    /// Raw, parameterized Dapper queries against the "users" table.
    /// No ORM change-tracking; every statement is explicit SQL.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT id            AS Id,
                       email         AS Email,
                       display_name  AS DisplayName,
                       password_hash AS PasswordHash,
                       created_at    AS CreatedAt,
                       updated_at    AS UpdatedAt
                FROM users
                WHERE email = @Email;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            const string sql = @"
                SELECT id            AS Id,
                       email         AS Email,
                       display_name  AS DisplayName,
                       password_hash AS PasswordHash,
                       created_at    AS CreatedAt,
                       updated_at    AS UpdatedAt
                FROM users
                WHERE id = @Id;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = "SELECT EXISTS(SELECT 1 FROM users WHERE email = @Email);";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
        }

        public async Task<User> CreateAsync(string email, string displayName, string passwordHash)
        {
            const string sql = @"
                INSERT INTO users (id, email, display_name, password_hash, created_at, updated_at)
                VALUES (gen_random_uuid(), @Email, @DisplayName, @PasswordHash, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                RETURNING id            AS Id,
                          email         AS Email,
                          display_name  AS DisplayName,
                          password_hash AS PasswordHash,
                          created_at    AS CreatedAt,
                          updated_at    AS UpdatedAt;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleAsync<User>(sql, new
            {
                Email = email,
                DisplayName = displayName,
                PasswordHash = passwordHash
            });
        }
    }
}
