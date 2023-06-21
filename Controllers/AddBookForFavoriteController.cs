using System.IdentityModel.Tokens.Jwt;
using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
public class AddBookForFavoriteController : ControllerBase
{
    private readonly CimdoContext _context;

    public AddBookForFavoriteController(CimdoContext context)
    {
        _context = context;
    }
    [HttpPost]
    [Route("api/[controller]")]
    public IActionResult AddBookForFavorite(int id_book)
    {

        int id_user = GetUserIdFromToken();
        Favorite favoriteModel = new Favorite
        {
            IdUser = id_user,
            IdBook = id_book
        };

        _context.Favorites.Add(favoriteModel);
        _context.SaveChanges();

        return Ok("Book add to favorite.");
    }
    //получение id пользователя из токена
    private int GetUserIdFromToken()
    {
        var token = GetTokenFromAuthorizationHeader(); //получаем токен
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        //полчение срока действия токена
        var now = DateTime.UtcNow;
        if (jwtToken.ValidTo < now)
        {
            // Токен истек, выполните необходимые действия, например, вызовите исключение
            throw new Exception("Expired token.");
        }
        // Извлечение идентификатора пользователя из полезной нагрузки токена
        var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);

        return userId;
    }

    //получение токена из запроса
    private string GetTokenFromAuthorizationHeader()
    {
        var autorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

        if (autorizationHeader != null && autorizationHeader.StartsWith("Bearer "))
        {
            var token = autorizationHeader.Substring("Bearer ".Length).Trim();
            return token;
        }

        return null;
    }
}