using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypesController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ProductTypesController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/ProductTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetProductType()
        {
            var activeProductTypes = await _context.ProductType
                                               .Where(p => p.Status != "Inactive")
                                               .ToListAsync();
            return Ok(activeProductTypes);
        }

        // GET: api/ProductTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductType>> GetProductType(int id)
        {
            var productType = await _context.ProductType.FindAsync(id);

            if (productType == null)
            {
                return NotFound();
            }

            return productType;
        }

        // PUT: api/ProductTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductType(int id, ProductType productType)
        {
            var existing = await _context.ProductType.FirstOrDefaultAsync(p => p.TypeName == productType.TypeName && p.Id != id);
            if (existing != null)
            {
                return BadRequest("Tên loại sản phẩm đã tồn tại!");
            }

            if (id != productType.Id)
            {
                return BadRequest();
            }

            _context.Entry(productType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductType>> PostProductType(ProductType productType)
        {
            var existing = await _context.ProductType.FirstOrDefaultAsync(p => p.TypeName == productType.TypeName);
            if (existing != null)
            {
                return BadRequest("Loại sản phẩm đã tồn tại!");
            }

            _context.ProductType.Add(productType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductType", new { id = productType.Id }, productType);
        }

        // DELETE: api/ProductTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductType(int id)
        {
            var productType = await _context.ProductType.FindAsync(id);
            if (productType == null)
            {
                return NotFound();
            }

            _context.ProductType.Remove(productType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductTypeExists(int id)
        {
            return _context.ProductType.Any(e => e.Id == id);
        }


        
        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetByStatus(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<ProductType> query = _context.ProductType;

            if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == "Inactive");
            }
            else
            {
                query = query.Where(p => p.Status != "Inactive");
            }

            var filtered = await query.ToListAsync();

            return Ok(filtered);
        }

        [HttpPut("updateStatus/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, ProductType Model)
        {
            var response = await _context.ProductType.FindAsync(id);

            if (response == null)
            {
                return NotFound();
            }

            response.Status = Model.Status;

            _context.Entry(response).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
