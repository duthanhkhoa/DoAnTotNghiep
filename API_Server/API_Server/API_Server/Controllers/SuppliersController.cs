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
    public class SuppliersController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public SuppliersController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/Suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSupplier()
        {
            var active = await _context.Supplier.Where(p => p.Status != "Inactive").ToListAsync();

            return Ok(active);
        }

        // GET: api/Suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var supplier = await _context.Supplier.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return supplier;
        }

        // PUT: api/Suppliers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSupplier(int id, Supplier supplier)
        {
            var existing = await _context.Supplier.FirstOrDefaultAsync(p => p.Name == supplier.Name && p.Id != id);
            if (existing != null)
            {
                return BadRequest("Nhà cung cấp đã tồn tại!");
            }

            if (id != supplier.Id)
            {
                return BadRequest();
            }

            _context.Entry(supplier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(id))
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

        // POST: api/Suppliers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Supplier>> PostSupplier(Supplier supplier)
        {
            var existing = await _context.Supplier.FirstOrDefaultAsync(p => p.Name == supplier.Name);
            if (existing != null)
            {
                return BadRequest("Nhà cung cấp đã tồn tại!");
            }

            _context.Supplier.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSupplier", new { id = supplier.Id }, supplier);
        }

        // DELETE: api/Suppliers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Supplier.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            _context.Supplier.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SupplierExists(int id)
        {
            return _context.Supplier.Any(e => e.Id == id);
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetByStatus(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<Supplier> query = _context.Supplier;

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
        public async Task<IActionResult> UpdateStatus(int id, Supplier Model)
        {
            var response = await _context.Supplier.FindAsync(id);

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
