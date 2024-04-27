using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECommerce.Controllers
{
    [EnableCors("Policy")]
    [Route("API/Products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext? _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/<ValuesController>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            var products = _context!.Products.Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        MinimumQuantity = p.MinimumQuantity,
                        Image = p.Image,
                        Code = p.Code,
                        Category = p.Category,
                        DiscountRate = p.DiscountRate,
                        User = p.User,
                    });
            if (products.Any()) return Ok(products);
            else return Ok(new { });
        }
        // POST api/<ValuesController>
        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string authheader = HttpContext.Request.Headers["Authorization"]!;
            if (authheader == null) return Unauthorized();
            string token = authheader.Substring("Bearer ".Length);
            User user = _context!.Users.Where(x => x.token == token).FirstOrDefault()!;
            if (user == null) return Unauthorized();
            product.User = Convert.ToInt32(user.Id);
            _context!.Products.Add(product);
            _context.SaveChanges();
            return Ok(product);
        }

        // GET api/<ValuesController>/5
        [AllowAnonymous]
        [HttpGet("{id}",Name="GetProduct")]
        public IActionResult Get(int id)
        {
            var product = _context!.Products.Find(id);
            if (product == null) return NotFound();
            else return Ok(product);
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Product product)
        {
            int userId = Convert.ToInt32(Request.Cookies["id"]);
            if (id != product.Id && product.User != userId )
            {
                return BadRequest();
            }
            var p = _context!.Products.Find(id);
            p!.Category = product.Category;
            p.Code = product.Code;
            p.Name = product.Name;
            p.Image = product.Image;
            p.Price = product.Price;
            p.MinimumQuantity = product.MinimumQuantity;
            p.DiscountRate = product.DiscountRate;
            _context.SaveChanges();

            return Ok(new {Message="Successfully updated the product."});
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _context!.Products.Find(id);
            int userId = Convert.ToInt32(Request.Cookies["id"]);
            try { 
                if (product.User != userId)
                {
                    return BadRequest();
                }
            }catch(Exception ex)
            {
                return BadRequest();
            }
            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok(new {Message=$"Product {id} was deleted successfully."});
        }
    }
}
