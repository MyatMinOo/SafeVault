// Services/UserRepository.cs
using System.Threading.Tasks;
using SafeVault.Api.Models;
using System;
using Microsoft.Data.Sqlite;

namespace SafeVault.Api.Services
{
    public class UserRepository
    {
        private readonly string _connectionString;

        // In production get from configuration
        public UserRepository()
        {
            // Example in-memory for tests or local file:
            _connectionString = "Data Source=SafeVault.db";
        }

        public async Task<long> InsertUserAsync(UserDto user)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            // Parameterized query prevents SQLi
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (Username, Email) VALUES (@username, @email); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@username", user.Username ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", user.Email ?? string.Empty);

            var result = await cmd.ExecuteScalarAsync();
            return (result == null) ? 0L : (long)result;
        }

        public async Task<int> CountUsersByUsernameAsync(string username)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Username = @username;";
            cmd.Parameters.AddWithValue("@username", username ?? string.Empty);
            var res = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(res);
        }
    }
}
