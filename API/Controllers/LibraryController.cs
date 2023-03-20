﻿using API.DataAccess;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IDataAccess library;
        private readonly IConfiguration configuration;
        public LibraryController(IDataAccess library, IConfiguration configuration = null)
        {
            this.library = library;
            this.configuration = configuration;
        }

        [HttpPost("CreateAccount")]
        public IActionResult CreateAccount(User user)
        {
            if (!library.IsEmailAvailable(user.Email))
            {
                return Ok("Email n'est pas disponible!");
            }
            user.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            user.UserType = UserType.USER;
            library.CreateUser(user);
            return Ok("Compte creé avec succes!");
        }

        [HttpGet("Login")]
        public IActionResult Login(string email, string password)
        {
            if (library.AuthenticateUser(email, password, out User? user))
            {
                if (user != null)
                {
                    var jwt = new Jwt(configuration["Jwt:Key"], configuration["Jwt:Duration"]);
                    var token = jwt.GenerateToken(user);
                    return Ok(token);
                }
            }
            return Ok("Invalide");
        }

        [HttpGet("GetAllBooks")]
        public IActionResult GetALlBooks()
        {
            var books = library.GetAllBooks();
            var booksToSend = books.Select(book => new
            {
                book.Id,
                book.Title,
                book.Category.Category,
                book.Category.SubCategory,
                book.Price,
                Available = !book.Ordered,
                book.Author
            }).ToList();
            return Ok(booksToSend);
        }

        [HttpGet("OrderBook/{userId}/{bookId}")]
        public IActionResult OrderBook(int userId, int bookId)
        {
            var result = library.OrderBook(userId, bookId) ? "succes" : "echoué";
            return Ok(result);
        }

        [HttpGet("GetOrders/{id}")]
        public IActionResult GetOrders(int id)
        {
            return Ok(library.GetOrdersOfUser(id));
        }

        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            return Ok(library.GetAllOrders());
        }

        [HttpGet("ReturnBook/{bookId}/{userId}")]
        public IActionResult ReturnBook(string bookId, string userId)
        {
            var result = library.ReturnBook(int.Parse(userId), int.Parse(bookId));
            return Ok(result == true ? "succes" : "pas retourné");
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = library.GetUsers();
            var result = users.Select(user => new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Mobile,
                user.Blocked,
                user.Active,
                user.CreatedOn,
                user.UserType,
                user.Fine
            });
            return Ok(result);
        }

        [HttpGet("ChangeBlockStatus/{status}/{id}")]
        public IActionResult ChangeBlockStatus(int status, int id)
        {
            if (status == 1)
            {
                library.BlockUser(id);
            }
            else
            {
                library.UnblockUser(id);
            }
            return Ok("succes");
        }

        [HttpGet("ChangeEnableStatus/{status}/{id}")]
        public IActionResult ChangeEnableStatus(int status, int id)
        {
            if (status == 1)
            {
                library.ActivateUser(id);
            }
            else
            {
                library.DeactivateUser(id);
            }
            return Ok("succes");
        }

        [HttpGet("GetAllCategories")]
        public IActionResult GetAllCategories()
        {
            var categories = library.GetAllCategories();
            var x = categories.GroupBy(c => c.Category).Select(item =>
            {
                return new 
                { 
                    name = item.Key, 
                    children = item.Select(item => new { name = item.SubCategory }).ToList() 
                };
            }).ToList();
            return Ok(x);
        }

        [HttpPost("InsertBook")]
        public IActionResult InsertBook(Book book)
        {
            book.Title = book.Title.Trim();
            book.Author = book.Author.Trim();
            book.Category.Category = book.Category.Category.ToLower();
            book.Category.SubCategory = book.Category.SubCategory.ToLower();

            library.InsertNewBook(book);
            return Ok("Enregistré");
        }

        [HttpDelete("DeleteBook/{id}")]
        public IActionResult DeleteBook(int id)
        {
            var returnResult = library.DeleteBook(id) ? "succes" : "Echoué";
            return Ok(returnResult);
        }

        [HttpPost("InsertCategory")]
        public IActionResult InsertCategory(BookCategory bookCategory)
        {
            bookCategory.Category = bookCategory.Category.ToLower();
            bookCategory.SubCategory = bookCategory.SubCategory.ToLower();
            library.CreateCategory(bookCategory);
            return Ok("Enregistré");
        }
    }
}
