using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize(Policy = "UserIdPolicy")]
public class AddGenersToBooksController : ControllerBase
{
    private readonly CimdoContext _context;

    public AddGenersToBooksController(CimdoContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("api/[controller]")]
    public IActionResult AddGenersToBooks(ModelBooksGeners booksgeners)
    {
        BooksGener modelbooksgeners = new BooksGener();

        modelbooksgeners.IdBook = booksgeners.IdBook;
        modelbooksgeners.IdGener = booksgeners.IdGener;
        
        _context.BooksGeners.Add(modelbooksgeners);
        _context.SaveChanges();

        return Ok("Genre added to book.");
    }
}