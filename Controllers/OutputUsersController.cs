using CimdoApi.Context;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize(Policy = "UserIdPolicy")]
public class OutputUsersController : ControllerBase
{
    private IEnumerable<User> GetUsers() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Users.ToList();
        }
    }
    [HttpGet]
    [Route("api/[controller]")]
    public ActionResult GetDataApi()
    {
        var usersData = GetUsers(); //вывод данных в api
        return Ok(usersData);
    }
}