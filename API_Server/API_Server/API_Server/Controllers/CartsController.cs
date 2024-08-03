using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using System.Security.Claims;
using API_Server.ModelView;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public CartsController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCart()
        {
            return await _context.Cart.ToListAsync();
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            var cart = await _context.Cart.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, [FromBody] Cart updatedCart)
        {
            if (id != updatedCart.Id)
            {
                return BadRequest("Cart ID mismatch");
            }

            // Tìm sản phẩm trong giỏ hàng
            var cartItem = await _context.Cart.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }

            // Cập nhật số lượng sản phẩm
            cartItem.QuantityCart = updatedCart.QuantityCart;

            // Lưu thay đổi
            _context.Entry(cartItem).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound("Cart item not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CartDTO>> PostCart(CartDTO cartDTO)
        {
            Cart newCartItem = null; // Khai báo biến newCartItem ở đầu phương thức

            var existingCartItem = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserId == cartDTO.UserId && c.ProductId == cartDTO.ProductId && c.Status == "pending");

            if (existingCartItem != null)
            {
                // If the item already exists, update the quantity
                existingCartItem.QuantityCart += cartDTO.QuantityCart;
                _context.Entry(existingCartItem).State = EntityState.Modified;
            }
            else
            {
                // If the item does not exist, create a new cart item
                newCartItem = new Cart
                {
                    ProductId = cartDTO.ProductId,
                    QuantityCart = cartDTO.QuantityCart,
                    UserId = cartDTO.UserId,
                    Status = cartDTO.Status
                };

                _context.Cart.Add(newCartItem);
            }

            try
            {
                await _context.SaveChangesAsync();

                // Check if newCartItem is not null before accessing its Id property
                if (newCartItem != null)
                {
                    return CreatedAtAction("GetCart", new { id = newCartItem.Id }, cartDTO); // Trả về thông tin cartDTO sau khi đã lưu thành công
                }
                else
                {
                    return BadRequest(); // Nếu newCartItem là null, có thể xảy ra khi existingCartItem đã tồn tại
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }
        }



        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Cart.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Carts/clearCart/{userId}
        [HttpDelete("clearCart/{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            var cartItems = await _context.Cart
                .Where(c => c.UserId == userId && c.Status == "pending")
                .ToListAsync();

            if (cartItems == null || cartItems.Count == 0)
            {
                return NotFound("No cart items found for the user.");
            }

            _context.Cart.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Cart.Any(e => e.Id == id);
        }
    }
}
