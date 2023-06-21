using CimdoApi.Context;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]

public class OutputBooksController : ControllerBase
{
    private IEnumerable<Book> GetBooks() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Books.ToList();
        }
    }
    [HttpGet]
    [Route("api/[controller]")]
    public ActionResult GetDataApi()
    {
        var booksData = GetBooks(); //вывод данных в api
        return Ok(booksData);
    }
}