using Microsoft.IdentityModel.Tokens;
using SmartDoor.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartDoor.Services
{
    public interface IAuthService
    {
        Task<Token> Authenticate(LoginModel user);
    }
    public class AuthService : IAuthService
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<Token> Authenticate(LoginModel user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
                throw new Exception("Invalid Input received!");

            string hashedPassword;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(user.Password);
                byte[] hash = sha256.ComputeHash(bytes);
                hashedPassword = Convert.ToBase64String(hash);
            }

            var users = await _userService.GetAsync();

            var u = users.Find(x => x.UserName == user.UserName && x.Password == hashedPassword);

            if (u == null)
            {
                throw new Exception("Username or password is wrong");
            }
            // User name and password are valid. 
            // Generate JSON Web Token

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

            var identity = ObjectToClaims(u);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Token { AuthToken = tokenHandler.WriteToken(token) };
        }

        private ClaimsIdentity ObjectToClaims<T>(T obj)
        {
            List<Claim> claims = new List<Claim>();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "Devices")
                {
                    object value = property.GetValue(obj);

                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        claims.Add(new Claim(property.Name, value.ToString()));
                    }
                }
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Bearer Authentication");
            return identity;
        }
    }
}
