using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt;

namespace CimdoApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly CimdoContext _dbContext;

    public RegistrationController(CimdoContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser(ModelUser model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Проверяем, существует ли пользователь с таким же именем пользователя или email'ом
        if (await _dbContext.Users.AnyAsync(u => u.Login == model.Login || u.Email == model.Email))
        {
            return Conflict("Пользователь с таким именем пользователя или email'ом уже существует");
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

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            return BadRequest("Password mismatch!");
        }
        return Ok("Пользователь успешно зарегистрирован");
    }
}