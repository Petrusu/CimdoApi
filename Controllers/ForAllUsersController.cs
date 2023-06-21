using System.IdentityModel.Tokens.Jwt;
using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ForAllUsersController : ControllerBase
{
    private readonly CimdoContext _context;
    public ForAllUsersController(CimdoContext context)
    {
        _context = context;
    }
    private static IEnumerable<Book> GetBooks() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Books.ToList();
        }
    }
    [HttpGet("getbooks")]
    public ActionResult GetDataApi()
    {
        var booksData = GetBooks(); //вывод данных в api
        return Ok(booksData);
    }
    private static IEnumerable<Gener> GetGeners() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Geners.ToList();
        }
    }
    [HttpGet("getgeners")]
    public ActionResult GetData()
    {
        var genersData = GetGeners(); //вывод данных в api
        return Ok(genersData);
    }
    [HttpGet("getinformationaboutbook")]
    public async Task<ActionResult<ModelBookForFavarite>> GetInformationAboutBook(int bookId)
    {
        int userId = GetUserIdFromToken();
        var favoriteBook = await _context.Favorites
            .Include(f => f.IdBookNavigation)
            .ThenInclude(book => book.AuthorNavigation)
            .FirstOrDefaultAsync(f => f.IdUser == userId && f.IdBook == bookId);

        if (favoriteBook == null)
        {
            return NotFound("Book not found"); // если книга не найдена в избранном пользователя, возвращаем 404 Not Found
        }

        // Создаем объект ModelBookForFavarite и заполняем его данными из favoriteBook.IdBookNavigation и favoriteBook.IdBookNavigation.Author
        var bookInformation = new ModelBookForFavarite
        {
            IdBook = favoriteBook.IdBookNavigation.IdBook,
            Title = favoriteBook.IdBookNavigation.Title,
            Author = favoriteBook.IdBookNavigation.AuthorNavigation.Author1, // Используем свойство Author1 из модели ModelAuthor
            Description = favoriteBook.IdBookNavigation.Description
        };

        return Ok(bookInformation); // возвращаем информацию о книге
    }
    
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendedBooks()
    {
        // Получаем id пользователя из токена
        var userId = GetUserIdFromToken();

        // Получаем предпочтения пользователя по жанрам
        var user = await _context.Users
            .Include(u => u.UsersGeners)
            .FirstOrDefaultAsync(u => u.IdUser == userId);

        var genreIds = user.UsersGeners.Select(ug => ug.IdGener).ToArray();

        var books = await _context.Books
            .Where(b => b.BooksGeners.Any(bg => genreIds.Contains(bg.IdGener)))
            .Include(b => b.AuthorNavigation) // Включаем связанную сущность "Author"
            .Select(b => new 
            {
                Id = b.IdBook,
                Title = b.Title,
                Author = b.AuthorNavigation.Author1
            })
            .ToListAsync();

        return Ok(books);
    }

    [HttpPost("addbookforfavorite")]
    public IActionResult AddBookForFavorite(int idBook)
    {

        int idUser = GetUserIdFromToken();
        Favorite favoriteModel = new Favorite
        {
            IdUser = idUser,
            IdBook = idBook
        };

        _context.Favorites.Add(favoriteModel);
        _context.SaveChanges();

        return Ok("Book add to favorite.");
    }

    [HttpPost("addfavoritegeners")]
    public IActionResult AddFavoriteGeners(int idGener)
    {

        int idUser = GetUserIdFromToken();
        UsersGener favoritegenerModel = new UsersGener()
        {
            IdUser = idUser,
            IdGener = idGener
        };

        _context.UsersGeners.Add(favoritegenerModel);
        _context.SaveChanges();

        return Ok("Gener add to favorite.");
    }

    [HttpDelete("removebookfromfavorites")]
    public async Task<IActionResult> DeleteBookFromFavarite(int idBook)
    {
        var book =  _context.Favorites.FirstOrDefault(b => b.IdBook == idBook);

        if (book == null)
        {
            return NotFound("Book not found"); // Если пользователь с указанным id не найден, возвращаем 404 Not Found
        }

        _context.Favorites.Remove(book);
        await _context.SaveChangesAsync();

        return Ok("Book delited"); 
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