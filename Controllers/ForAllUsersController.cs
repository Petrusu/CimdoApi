using CimdoApi.Context;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ForAllUsersController : ControllerBase
{
    private IEnumerable<Book> GetBooks() //подключение к базе данных
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
    private IEnumerable<Gener> GetGeners() //подключение к базе данных
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
}