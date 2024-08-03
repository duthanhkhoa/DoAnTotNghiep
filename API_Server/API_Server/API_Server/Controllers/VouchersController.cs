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
    public class VouchersController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public VouchersController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/Vouchers
        [HttpGet("AdminUse")]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVoucher()
        {
            var active = await _context.Voucher.Where(p => p.Status != "Inactive").ToListAsync();

            return Ok(active);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVoucherUser()
        {
            var currentDate = DateTime.UtcNow.Date; // Chỉ so sánh phần ngày
            var activeVouchers = await _context.Voucher
                .Where(p => p.Status != "Inactive" && p.ExpiryDate.Date >= currentDate)
                .ToListAsync();

            return Ok(activeVouchers);
        }

        // GET: api/Vouchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetVoucher(int id)
        {
            var voucher = await _context.Voucher.FindAsync(id);

            if (voucher == null)
            {
                return NotFound();
            }

            return voucher;
        }

        // PUT: api/Vouchers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoucher(int id, Voucher voucher)
        {
            var existingName = await _context.Voucher
                              .FirstOrDefaultAsync(p => p.VoucherName == voucher.VoucherName && p.Id != id);
            var existingCode = await _context.Voucher
                              .FirstOrDefaultAsync(p => p.VoucherCode == voucher.VoucherCode && p.Id != id);
            if (existingCode != null)
            {
                return BadRequest("Mã phiếu giảm giá đã tồn tại!");
            }
            if (existingName != null)
            {
                return BadRequest("Tên phiếu giảm giá đã tồn tại!");
            }
            

            if (id != voucher.Id)
            {
                return BadRequest();
            }

            _context.Entry(voucher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoucherExists(id))
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

        // POST: api/Vouchers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Voucher>> PostVoucher(Voucher voucher)
        {
            var existingName = await _context.Voucher
                              .FirstOrDefaultAsync(p => p.VoucherName == voucher.VoucherName);
            var existingCode = await _context.Voucher
                              .FirstOrDefaultAsync(p => p.VoucherCode == voucher.VoucherCode);
            if (existingCode != null)
            {
                return BadRequest("Mã phiếu giảm giá đã tồn tại!");
            }

            if (existingName != null)
            {
                return BadRequest("Tên phiếu giảm giá đã tồn tại!");
            }

            _context.Voucher.Add(voucher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVoucher", new { id = voucher.Id }, voucher);
        }

        // DELETE: api/Vouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.Voucher.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            _context.Voucher.Remove(voucher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoucherExists(int id)
        {
            return _context.Voucher.Any(e => e.Id == id);
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetByStatus(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<Voucher> query = _context.Voucher;

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
        public async Task<IActionResult> UpdateStatus(int id, Voucher Model)
        {
            var response = await _context.Voucher.FindAsync(id);

            if (response == null)
            {
                return NotFound();
            }

            response.Status = Model.Status;

            _context.Entry(response).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/Vouchers/code/{voucherCode}
        [HttpGet("code/{voucherCode}")]
        public async Task<ActionResult<Voucher>> GetVoucherByCode(string voucherCode)
        {
            var voucher = await _context.Voucher
                            .FirstOrDefaultAsync(p => p.VoucherCode == voucherCode && p.Status == "Active");

            if (voucher == null)
            {
                return NotFound();
            }
            return Ok(voucher);
        }

        // PUT: api/Vouchers/UpdateQuantity/{id}
        [HttpPut("UpdateQuantity/{id}")]
        public async Task<IActionResult> UpdateVoucherQuantity(int id)
        {
            var voucher = await _context.Voucher.FindAsync(id);

            if (voucher == null)
            {
                return NotFound();
            }

            if (voucher.Quantity <= 0)
            {
                return BadRequest("Số lượng voucher đã hết.");
            }

            voucher.Quantity--;

            if (voucher.Quantity == 0)
            {
                voucher.Status = "Inactive"; // Thay đổi trạng thái thành không hoạt động khi hết số lượng
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoucherExists(id))
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
    }
}
