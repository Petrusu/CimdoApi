using CimdoApi.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CimdoApi.Controllers;
[ApiController]
[Authorize(Policy = "UserIdPolicy")]
public class DeliteUserController : ControllerBase
{
    private readonly CimdoContext _context; 

    public DeliteUserController(CimdoContext context)
    {
        _context = context;
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
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