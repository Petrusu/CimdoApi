using CimdoApi.Context;
using CimdoApi.InnerClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EditProfileController : ControllerBase
{
    private readonly CimdoContext _context;

    public EditProfileController(CimdoContext context)
    {
        _context = context;
    }

    [HttpPut("changepassword")]
    public IActionResult ChangePassword(ModelChangePassword requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(requeat.OldPassword, user.Password);

        // Если пароль действителен
        if (isPasswordValid)
        {
            if (requeat.Password == requeat.PasswordAgain)
            {
                // Шифрование пароля
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(requeat.Password);

                user.Password = hashedPassword;

                _context.SaveChanges();
                return Ok("Password changed");
            }
            else
            {
                return BadRequest("Password mismatch");
            }
        }
        else
        {
            return BadRequest("Old password is not mismatch");
        }
    }
    [HttpPut("changeemail")]
    public IActionResult ChangeEmail(ModelChangeEmail requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }

        user.Email = requeat.Email;

        _context.SaveChanges();

        return Ok("Email changed");
    }
    [HttpPut("changelogin")]
    public IActionResult ChangeLogin(ModelChangeLogin requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.OldLogin);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }
        
        // Проверяем, что новый логин не совпадает ни с одним из существующих логинов
        var existingUser = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (existingUser != null)
        {
            return BadRequest("Login already exists"); // Логин уже существует
        }

        user.Login = requeat.Login;
        _context.SaveChanges();

        return Ok("Login changed");
    }
    
}