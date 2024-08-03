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
    public class UserVouchersController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public UserVouchersController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/UserVouchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserVouchers>>> GetUserVouchers()
        {
            return await _context.UserVouchers.ToListAsync();
        }

        // GET: api/UserVouchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserVouchers>> GetUserVouchers(int id)
        {
            var userVouchers = await _context.UserVouchers.FindAsync(id);

            if (userVouchers == null)
            {
                return NotFound();
            }

            return userVouchers;
        }

        // PUT: api/UserVouchers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserVouchers(int id, UserVouchers userVouchers)
        {
            if (id != userVouchers.Id)
            {
                return BadRequest();
            }

            _context.Entry(userVouchers).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserVouchersExists(id))
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

        // POST: api/UserVouchers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserVouchers>> PostUserVouchers(UserVouchers userVouchers)
        {
            _context.UserVouchers.Add(userVouchers);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserVouchers", new { id = userVouchers.Id }, userVouchers);
        }

        // DELETE: api/UserVouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserVouchers(int id)
        {
            var userVouchers = await _context.UserVouchers.FindAsync(id);
            if (userVouchers == null)
            {
                return NotFound();
            }

            _context.UserVouchers.Remove(userVouchers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserVouchersExists(int id)
        {
            return _context.UserVouchers.Any(e => e.Id == id);
        }


        //[HttpGet("check-usage")]
        //public async Task<IActionResult> CheckUsage(string userId, string voucherCode)
        //{
        //    var usage = await _context.UserVouchers.AnyAsync(uv => uv.UserId == userId && uv.VoucherCode == voucherCode);
        //    return Ok(usage);
        //}

        [HttpGet("check-usage")]
        public async Task<IActionResult> CheckUsage(string userId, string voucherCode)
        {
            // Kiểm tra nếu userId hoặc voucherCode là null hoặc trống
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(voucherCode))
            {
                return BadRequest("UserId và VoucherCode không được để trống");
            }

            var usage = await _context.UserVouchers.AnyAsync(uv => uv.UserId == userId && uv.VoucherCode == voucherCode);
            return Ok(usage);
        }


        [HttpPost("record-usage")]
        public async Task<IActionResult> RecordUsage(UserVouchers userVoucher)
        {
            _context.UserVouchers.Add(userVoucher);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
