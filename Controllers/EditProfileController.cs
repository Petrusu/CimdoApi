using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EditProfileController : ControllerBase
{
    private readonly CimdoContext _context; //подключение бд

    public EditProfileController(CimdoContext context) //конструктор контроллера
    {
        _context = context;
    }

    //запрос на изменения пароля
    [HttpPut("changepassword")]
    public IActionResult ChangePassword(User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }
        
        // Шифрование пароля
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(requeat.Password);
        user.Password = hashedPassword;
        _context.SaveChanges(); //сохранения нового пароля
        return Ok("Password changed");
    }
    
    //изменения email
    [HttpPut("changeemail")]
    public IActionResult ChangeEmail(Models.User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }

        user.Email = requeat.Email;

        _context.SaveChanges(); //сохранение нового мыла

        return Ok("Email changed");
    }
    
    //изменение логина
    [HttpPut("changelogin")]
    public IActionResult ChangeLogin(User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
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
        _context.SaveChanges(); //сохранение нового логина

        return Ok("Login changed");
    }
    
}