using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CimdoApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RegistrationOrLoginController : ControllerBase
{
    private readonly IConfiguration _configuration;
        private readonly CimdoContext _context;
        private int TokenTimeoutMinutes = 5; // Время истечения срока действия токена в минутах
        private DateTime _tokenCreationTime;
    
        public RegistrationOrLoginController(IConfiguration configuration, CimdoContext context)
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
        
        [HttpPost]
        public async Task<IActionResult> RegisterUser(ModelUser model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверяем, существует ли пользователь с таким же именем пользователя или email'ом
            if (await _context.Users.AnyAsync(u => u.Login == model.Login || u.Email == model.Email))
            {
                return Conflict("A user with the same username or email address already exists");
            }

            //проверка пароля
            if (model.Password == model.PasswordAgain)
            {
                // Шифрование пароля
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                // Создаем нового пользователя
                var user = new User
                {
                    Login = model.Login,
                    Email = model.Email,
                    Password =
                        hashedPassword // Обычно пароль нужно хранить в зашифрованном виде, но для примера оставим его в открытом виде
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Password mismatch!");
            }

            return Ok("User successfully registered");
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
