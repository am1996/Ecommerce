using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ECommerce.Controllers
{
    [EnableCors("Policy")]
    [AllowAnonymous]
    [ApiController]
    public class UserController : ControllerBase
    {
        ApplicationDbContext _context;
        IConfiguration _configuration;
        public UserController(ApplicationDbContext context,IConfiguration configuration) {
            _configuration = configuration;
            _context = context;
        }
        [Route("API/Users/Dashboard")]
        [HttpGet]

        public IActionResult Dashboard()
        {
            string authheader = HttpContext.Request.Headers["Authorization"].ToString() ?? "";
            if(authheader.Length > 0)
            {
                string token = authheader.Substring("Bearer ".Length);
                var user = _context.Users.Where(u => u.token == token).FirstOrDefault() ?? null;
                if(user == null) return BadRequest(new { Message = "Bad token!" });
                user.Password = "";
                return Ok(user);
            }
            else
            {
                return Unauthorized();
            }

        }
        [Route("API/Users/Register")]
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (Request.Cookies["id"] != null) return BadRequest(new {Message="You are already loggedin"});
            var u = _context.Users.Where(b => b.Email == user.Email).Count();
            if (u != 0) return BadRequest(new { Message="Email already registered."});
            var password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = password;
            user.token = "";
            _context.Users.Add(user);
            _context.SaveChanges();
            return new OkObjectResult(new {Message="User Successfully Created."});
        }
        [Route("API/Users/Login")]
        [HttpPost]
        public IActionResult Login(JsonElement loginModel)
        {
            // Validate user credentials (replace with your logic)
            string jwt_secret = _configuration["Jwt:SigningKey"]!;
            string issuer = _configuration["Jwt:Issuer"]!;
            string audience = _configuration["Jwt:Audience"]!;

            try { 
                string email = loginModel.GetProperty("email").ToString();
                string password = loginModel.GetProperty("password").ToString();
                User user = _context.Users.Where(b => b.Email == email).FirstOrDefault()!;
                if (user != null &&
                    password != null &&
                    BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    string tokenString = Models.User.GenerateJWTToken(issuer,audience, jwt_secret,user.Id);
                    Response.Cookies.Append("accessToken", tokenString, new CookieOptions
                    {
                        HttpOnly = true, // Prevent access from JavaScript for XSS protection
                        Secure = true, // Only send over HTTPS connections (if applicable in production)
                        Expires = DateTime.UtcNow.AddDays(30) // Adjust expiration time as needed
                    });
                    Response.Cookies.Append("id", user.Id.ToString(), new CookieOptions
                    {
                        HttpOnly = true, // Prevent access from JavaScript for XSS protection
                        Secure = true, // Only send over HTTPS connections (if applicable in production)
                        Expires = DateTime.UtcNow.AddDays(30) // Adjust expiration time as needed
                    });
                    user.token = tokenString;
                    _context.SaveChanges();
                    return Ok(new { token = tokenString });
                }
                else
                {
                    return Ok(new {Message="Wrong Email or password" });
                }
            }
            catch(Exception)
            {
                return Unauthorized();
            }
        }
        [Route("API/Users/logout")]
        [Authorize]
        [HttpGet]
        public IActionResult logout()
        {
            string authheader = HttpContext.Request.Headers["Authorization"]!;
            if(authheader == null) return Unauthorized();
            string token = authheader.Substring("Bearer ".Length);
            User user = _context.Users.Where(x => x.token == token).FirstOrDefault()!;
            if(user == null) return Unauthorized();
            user.token = "";
            _context.SaveChanges();
            Response.Cookies.Delete("id", new CookieOptions { HttpOnly = true, Secure = true });
            Response.Cookies.Delete("accessToken", new CookieOptions { HttpOnly = true, Secure = true });
            return Ok(new { Message="Successfully logged out."});
        }
    }
}
