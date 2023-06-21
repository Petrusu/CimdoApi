using CimdoApi.Context;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize]
public class OutputGenersController : ControllerBase
{
    private IEnumerable<Gener> GetGeners() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Geners.ToList();
        }
    }
    [HttpGet]
    [Route("api/[controller]")]
    public ActionResult GetDataApi()
    {
        var genersData = GetGeners(); //вывод данных в api
        return Ok(genersData);
    }
}