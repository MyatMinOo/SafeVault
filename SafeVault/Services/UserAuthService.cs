using System.Threading.Tasks;
using BCrypt.Net;
using SafeVault.Api.Models;

namespace SafeVault.Api.Services
{
    public class UserAuthService
    {
        public readonly UserRepository _repo;

        public UserAuthService(UserRepository repo)
        {
            _repo = repo;
        }

        // Register a new user with hashed password
        public async Task<long> RegisterUserAsync(string username, string email, string password, string role = "user")
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new UserDto { Username = username, Email = email };
            return await _repo.InsertUserWithPasswordAsync(user, hash, role);
        }

        // Validate login
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _repo.GetUserByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
