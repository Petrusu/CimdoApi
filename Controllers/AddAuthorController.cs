using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize(Policy = "UserIdPolicy")]
public class AddAuthorController : ControllerBase
{
    private readonly CimdoContext _context;

    public AddAuthorController(CimdoContext context)
    {
        _context = context;
    }
    [HttpPost]
    [Route("api/[controller]")]
    public IActionResult AddAuthor(ModelAuthor author)
    {
        Author authorModel = new Author();

        authorModel.Author1 = author.Author1;

        _context.Authors.Add(authorModel);
        _context.SaveChanges();

        return Ok("Author add.");
    }
}