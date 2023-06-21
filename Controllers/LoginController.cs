using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CimdoApi.Context;
using CimdoApi.InnerClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace CimdoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CimdoContext _context;
        private int TokenTimeoutMinutes = 5; // Время истечения срока действия токена в минутах
        private DateTime _tokenCreationTime;
    
        public LoginController(IConfiguration configuration, CimdoContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Authenticate([FromForm] string login, [FromForm] string password)
        {
            // Проверяем, существует ли пользователь
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            if (user == null)
            {
                return Unauthorized(); // Пользователь не найден
            }

            var loginResponse = new LoginResponse();

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            // Если пароль действителен
            if (isPasswordValid)
            {
                string token = CreateToken(user.IdUser);

                loginResponse.Token = token;
                loginResponse.ResponseMsg = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                };

                // Возвращаем токен
                return Ok(new { loginResponse });
            }
            else
            {
                // Если имя пользователя или пароль недействительны, отправляем статус-код "BadRequest" в ответе
                return BadRequest("Username or Password Invalid!");
            }
        }

        private string CreateToken(int userId)
        {
            var claims = new List<Claim>()
            {
                // Список претензий (claims) - мы проверяем только id пользователя, можно добавить больше претензий.
                new Claim("userId", Convert.ToString(userId)),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(TokenTimeoutMinutes),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
