using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "UserIdPolicy")]
public class ForAdminController : ControllerBase
{
    private readonly CimdoContext _context;

    public ForAdminController(CimdoContext context)
    {
        _context = context;
    }
    [HttpPost("addauthor")]
    public IActionResult AddAuthor(ModelAuthor author)
    {
        Author authorModel = new Author();

        authorModel.Author1 = author.Author1;

        _context.Authors.Add(authorModel);
        _context.SaveChanges();

        return Ok("Author add.");
    }
    [HttpPost("addbook")]
    public IActionResult AddBook(ModelBook book)
    {
        Book booknModel = new Book();

        booknModel.Title = book.Title;
        booknModel.Author = book.Author;
        booknModel.Description = book.Description;

        _context.Books.Add(booknModel);
        _context.SaveChanges();

        return Ok("Book add.");
    }
    [HttpPost("addgenerstobooks")]
    public IActionResult AddGenersToBooks(ModelBooksGeners booksgeners)
    {
        BooksGener modelbooksgeners = new BooksGener();

        modelbooksgeners.IdBook = booksgeners.IdBook;
        modelbooksgeners.IdGener = booksgeners.IdGener;
        
        _context.BooksGeners.Add(modelbooksgeners);
        _context.SaveChanges();

        return Ok("Genre added to book.");
    }
    private IEnumerable<User> GetUsers() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Users.ToList();
        }
    }
    [HttpGet("getusers")]
    public ActionResult GetDataApi()
    {
        var usersData = GetUsers(); //вывод данных в api
        return Ok(usersData);
    }
    [HttpDelete("deliteuser")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(); // Если пользователь с указанным id не найден, возвращаем 404 Not Found
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent(); // Возвращаем 204 No Content, если удаление прошло успешно
    }
}