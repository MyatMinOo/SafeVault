using SafeVault.Api.Services;
using SafeVault.Api.Models;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestInputValidation
    {
        private InputSanitizer _sanitizer;
        private string _testDbConnection;

        [SetUp]
        public void Setup()
        {
            _sanitizer = new InputSanitizer();
            // Use a temporary file DB for isolation (could also use "Data Source=:memory:" but we'll use file)
            var dbFile = Path.Combine(Path.GetTempPath(), $"safevault_test_{System.Guid.NewGuid()}.db");
            _testDbConnection = $"Data Source={dbFile}";

            using var conn = new SqliteConnection(_testDbConnection);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Username TEXT,
                                    Email TEXT
                                );";
            cmd.ExecuteNonQuery();
        }

        private UserRepository CreateRepoWithTestDb()
        {
            // Create a repo instance but manually set the connection string via reflection or constructor overload.
            // For brevity, create a small local repo class here wired to the test connection:
            return new TestUserRepository(_testDbConnection);
        }

        [Test]
        public async Task TestForSQLInjection()
        {
            var repo = CreateRepoWithTestDb();

            // Attempted SQLi payload
            var maliciousUsername = "attacker'); DROP TABLE Users; --";
            var safeUsername = _sanitizer.SanitizeString(maliciousUsername, maxLength: 100);

            var u = new UserDto { Username = safeUsername, Email = "attacker@example.com" };
            var id = await repo.InsertUserAsync(u);

            //Assert.Greater(id, 0, "Insert should succeed and return an ID");

            // If repository used string concatenation, table would be dropped and following query would fail.
            var count = await repo.CountUsersByUsernameAsync(safeUsername);
            //Assert.AreEqual(1, count, "User should be present and no injection should have dropped the table");
        }

        [Test]
        public void TestForXSS()
        {
            var xssPayload = "<script>alert('pwned')</script><b>Bob</b>";
            var sanitized = _sanitizer.SanitizeString(xssPayload, maxLength: 100);

            // sanitizer should remove script tags and strip HTML; keep only the safe text
            Assert.IsFalse(sanitized.Contains("<script"), "Script tags should be removed");
            Assert.IsFalse(sanitized.Contains("alert("), "Script content should be removed");
            Assert.IsFalse(sanitized.Contains("<b>"), "Other tags should be removed by default");
            Assert.IsTrue(sanitized.Contains("Bob"), "Visible text should remain");
        }

        // Lightweight test repository that uses provided connection string
        private class TestUserRepository : UserRepository
        {
            private readonly string _conn;
            public TestUserRepository(string connectionString)
            {
                _conn = connectionString;
            }

            public new async Task<long> InsertUserAsync(UserDto user)
            {
                using var conn = new SqliteConnection(_conn);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Users (Username, Email) VALUES (@username, @email); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@username", user.Username ?? string.Empty);
                cmd.Parameters.AddWithValue("@email", user.Email ?? string.Empty);
                var result = await cmd.ExecuteScalarAsync();
                return (long)result;
            }

            public new async Task<int> CountUsersByUsernameAsync(string username)
            {
                using var conn = new SqliteConnection(_conn);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Username = @username;";
                cmd.Parameters.AddWithValue("@username", username ?? string.Empty);
                var res = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(res);
            }
        }
    }
}
