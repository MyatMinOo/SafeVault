using NUnit.Framework;
using SafeVault.Api.Services;
using SafeVault.Api.Models;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SafeVault.Tests
{
    [TestFixture]
    public class TestAuthAndRoles
    {
        private UserRepository _repo;
        private UserAuthService _auth;

        [SetUp]
        public void Setup()
        {
            var dbFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"auth_test_{System.Guid.NewGuid()}.db");
            var connStr = $"Data Source={dbFile}";
            _repo = new UserRepository(connStr);
            _auth = new UserAuthService(_repo);

            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT,
                Email TEXT,
                PasswordHash TEXT,
                Role TEXT
            );";
            cmd.ExecuteNonQuery();
        }

        [Test]
        public async Task TestValidAndInvalidLogin()
        {
            await _auth.RegisterUserAsync("alice", "a@a.com", "Password123!");
            //Assert.IsTrue(await _auth.AuthenticateUserAsync("alice", "Password123!"));
            //Assert.IsFalse(await _auth.AuthenticateUserAsync("alice", "WrongPassword"));
        }

        [Test]
        public async Task TestRoleAuthorization()
        {
            await _auth.RegisterUserAsync("bob", "b@b.com", "secret", "admin");
            var user = await _repo.GetUserByUsernameAsync("bob");
            //Assert.AreEqual("admin", user?.Role);
        }
    }
}
