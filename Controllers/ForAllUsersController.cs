using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CimdoApi.Context;
using CimdoApi.InnerClasses;
using CimdoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CimdoApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ForAllUsersController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CimdoContext _context;
    private int TokenTimeoutMinutes = 5; // Время истечения срока действия токена в минутах
    private DateTime _tokenCreationTime;
    public ForAllUsersController(CimdoContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    //создание списка книг
    private static IEnumerable<Book> GetBooks() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Books.ToList();
        }
    }
    //запрос на вывод всех вниг
    [HttpGet("getbooks")]
    [Authorize]
    public ActionResult GetDataApi()
    {
        var booksData = GetBooks(); //вывод данных в api
        return Ok(booksData);
    }
    //создние списка жанров
    private static IEnumerable<Gener> GetGeners() //подключение к базе данных
    {
        using (var context = new CimdoContext())
        {
            return context.Geners.ToList();
        }
    }
    //вывод жанров
    [HttpGet("getgeners")]
    [Authorize]
    public ActionResult GetData()
    {
        var genersData = GetGeners(); //вывод данных в api
        return Ok(genersData);
    }
    //запрос на информацию о конкретной книге
    [HttpGet("getinformationaboutbook")]
    [Authorize]
    public async Task<ActionResult<ModelBookForFavarite>> GetInformationAboutBook(int bookId)
    {
        int userId = GetUserIdFromToken(); //из токена получаем id пользователя
        
        //запрос к бд для нахождения книги соотвествующей введенному id
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
            Author = favoriteBook.IdBookNavigation.AuthorNavigation.Author1, 
            Description = favoriteBook.IdBookNavigation.Description
        };

        return Ok(bookInformation); // возвращаем информацию о книге
    }
    //рекомендация книг для пользователя
    [HttpGet("recommendations")]
    [Authorize]
    public async Task<IActionResult> GetRecommendedBooks()
    {
        // Получаем id пользователя из токена
        var userId = GetUserIdFromToken();

        // Получаем предпочтения пользователя по жанрам
        var user = await _context.Users
            .Include(u => u.UsersGeners)
            .FirstOrDefaultAsync(u => u.IdUser == userId);

        //получаем жанры
        var genreIds = user.UsersGeners.Select(ug => ug.IdGener).ToArray();

        //выбираем книги по жанрам пользователя
        var books = await _context.Books
            .Where(b => b.BooksGeners.Any(bg => genreIds.Contains(bg.IdGener)))
            .Include(b => b.AuthorNavigation) 
            .Select(b => new 
            {
                Id = b.IdBook,
                Title = b.Title,
                Author = b.AuthorNavigation.Author1
            })
            .ToListAsync();

        return Ok(books); //выводим рекомендованные книги
    }
    
    [HttpPost("login")]
        public IActionResult Authenticate(LoginUser user_)
        {
            // Проверяем, существует ли пользователь
            var user = _context.Users.FirstOrDefault(u => u.Login == user_.Login);
            if (user == null)
            {
                return Unauthorized(); // Пользователь не найден
            }

            var loginResponse = new LoginResponse();

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(user_.Password, user.Password);

            // Если пароль действителен
            if (isPasswordValid)
            {
                string token = CreateToken(user.IdUser);

                loginResponse.Token = token;
                loginResponse.ResponseMsg = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                };

                // Возвращаем токен
                return Ok(new { loginResponse });
            }
            else
            {
                // Если имя пользователя или пароль недействительны, отправляем статус-код "BadRequest" в ответе
                return BadRequest("Username or Password Invalid!");
            }
        }
        
        [HttpPost("registration")]
        public async Task<IActionResult> RegisterUser(User model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверяем, существует ли пользователь с таким же именем пользователя или email'ом
            if (await _context.Users.AnyAsync(u => u.Login == model.Login || u.Email == model.Email))
            {
                return Conflict("A user with the same username or email address already exists");
            }
            
            // Шифрование пароля
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            // Создаем нового пользователя
            var user = new User
            {
                Login = model.Login,
                Email = model.Email,
                Password =
                    hashedPassword // Обычно пароль нужно хранить в зашифрованном виде, но для примера оставим его в открытом виде
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User successfully registered");
        }

    //добавление книги в избранное
    [HttpPost("addbookforfavorite")]
    [Authorize]
    public IActionResult AddBookForFavorite(int idBook)
    {

        int idUser = GetUserIdFromToken(); //получаем id пользователя из токена
        Favorite favoriteModel = new Favorite //экземпляр класса избранного
        {
            IdUser = idUser,
            IdBook = idBook
        };

        _context.Favorites.Add(favoriteModel); //добавляем 
        _context.SaveChanges(); //сохраняем

        return Ok("Book add to favorite.");
    }

    //добавляем предпочитаемые жанры
    [HttpPost("addfavoritegeners")]
    [Authorize]
    public IActionResult AddFavoriteGeners(int idGener)
    {

        int idUser = GetUserIdFromToken(); //id пользователя из токена
        UsersGener favoritegenerModel = new UsersGener()
        {
            IdUser = idUser,
            IdGener = idGener
        };

        _context.UsersGeners.Add(favoritegenerModel); //добавляем
        _context.SaveChanges(); //сохраняем

        return Ok("Gener add to favorite.");
    }
    
    //запрос на изменения пароля
    [HttpPut("changepassword")]
    public IActionResult ChangePassword(User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }
        
        // Шифрование пароля
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(requeat.Password);
        user.Password = hashedPassword;
        _context.SaveChanges(); //сохранения нового пароля
        return Ok("Password changed");
    }
    
    //изменения email
    [HttpPut("changeemail")]
    public IActionResult ChangeEmail(User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }

        user.Email = requeat.Email;

        _context.SaveChanges(); //сохранение нового мыла

        return Ok("Email changed");
    }
    
    //изменение логина
    [HttpPut("changelogin")]
    public IActionResult ChangeLogin(User requeat)
    {
        // Проверяем, существует ли пользователь
        var user = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (user == null)
        {
            return Unauthorized(); // Пользователь не найден
        }
        
        // Проверяем, что новый логин не совпадает ни с одним из существующих логинов
        var existingUser = _context.Users.FirstOrDefault(u => u.Login == requeat.Login);
        if (existingUser != null)
        {
            return BadRequest("Login already exists"); // Логин уже существует
        }

        user.Login = requeat.Login;
        _context.SaveChanges(); //сохранение нового логина

        return Ok("Login changed");
    }

    //удаляем книгу из избранного
    [HttpDelete("removebookfromfavorites")]
    public async Task<IActionResult> DeleteBookFromFavarite(int idBook)
    {
        var book =  _context.Favorites.FirstOrDefault(b => b.IdBook == idBook); //находим книгу по id

        if (book == null)
        {
            return NotFound("Book not found"); // Если пользователь с указанным id не найден, возвращаем 404 Not Found
        }

        _context.Favorites.Remove(book); //удаляем
        await _context.SaveChangesAsync(); //сохраняем

        return Ok("Book delited"); 
    }
    
    private string CreateToken(int userId)
    {
        var claims = new List<Claim>()
        {
            // Список претензий (claims) - мы проверяем только id пользователя, можно добавить больше претензий.
            new Claim("userId", Convert.ToString(userId)),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(TokenTimeoutMinutes),
            signingCredentials: cred
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
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