using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using NotesApp.Api.Data;
using NotesApp.Api.Models;

namespace NotesApp.Api.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NoteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Note>> GetAllForUserAsync(Guid userId)
        {
            const string sql = @"
                SELECT id         AS Id,
                       user_id    AS UserId,
                       title      AS Title,
                       content    AS Content,
                       is_pinned  AS IsPinned,
                       created_at AS CreatedAt,
                       updated_at AS UpdatedAt
                FROM notes
                WHERE user_id = @UserId
                ORDER BY is_pinned DESC, updated_at DESC;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QueryAsync<Note>(sql, new { UserId = userId });
        }

        public async Task<Note?> GetByIdForUserAsync(Guid noteId, Guid userId)
        {
            const string sql = @"
                SELECT id         AS Id,
                       user_id    AS UserId,
                       title      AS Title,
                       content    AS Content,
                       is_pinned  AS IsPinned,
                       created_at AS CreatedAt,
                       updated_at AS UpdatedAt
                FROM notes
                WHERE id = @NoteId AND user_id = @UserId;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<Note>(
                sql, new { NoteId = noteId, UserId = userId });
        }

        public async Task<Note> CreateAsync(Guid userId, string title, string content, bool isPinned)
        {
            const string sql = @"
                INSERT INTO notes (id, user_id, title, content, is_pinned, created_at, updated_at)
                VALUES (gen_random_uuid(), @UserId, @Title, @Content, @IsPinned, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                RETURNING id         AS Id,
                          user_id    AS UserId,
                          title      AS Title,
                          content    AS Content,
                          is_pinned  AS IsPinned,
                          created_at AS CreatedAt,
                          updated_at AS UpdatedAt;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleAsync<Note>(sql, new
            {
                UserId = userId,
                Title = title,
                Content = content,
                IsPinned = isPinned
            });
        }

        public async Task<Note?> UpdateAsync(Guid noteId, Guid userId, string title, string content, bool isPinned)
        {
            // updated
            const string sql = @"
                UPDATE notes
                SET title      = @Title,
                    content    = @Content,
                    is_pinned  = @IsPinned,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @NoteId AND user_id = @UserId
                RETURNING id         AS Id,
                          user_id    AS UserId,
                          title      AS Title,
                          content    AS Content,
                          is_pinned  AS IsPinned,
                          created_at AS CreatedAt,
                          updated_at AS UpdatedAt;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<Note>(sql, new
            {
                NoteId = noteId,
                UserId = userId,
                Title = title,
                Content = content,
                IsPinned = isPinned
            });
        }

        public async Task<bool> DeleteAsync(Guid noteId, Guid userId)
        {
            const string sql = @"
                DELETE FROM notes
                WHERE id = @NoteId AND user_id = @UserId;";

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            var rowsAffected = await connection.ExecuteAsync(sql, new { NoteId = noteId, UserId = userId });
            return rowsAffected > 0;
        }
    }
}
