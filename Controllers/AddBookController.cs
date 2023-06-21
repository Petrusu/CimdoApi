using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize(Policy = "UserIdPolicy")]
public class AddBookController : ControllerBase
{
    private readonly CimdoContext _context;

    public AddBookController(CimdoContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Route("api/[controller]")]
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
}