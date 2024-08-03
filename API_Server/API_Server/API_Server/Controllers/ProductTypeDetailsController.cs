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
    public class ProductTypeDetailsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ProductTypeDetailsController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/ProductTypeDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductTypeDetail>>> GetProductTypeDetail()
        {
            var active = await _context.ProductTypeDetail
                                              .Where(p => p.Status != "Inactive")
                                              .ToListAsync();
            return Ok(active);
        }

        // GET: api/ProductTypeDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductTypeDetail>> GetProductTypeDetail(string id)
        {
            var productTypeDetail = await _context.ProductTypeDetail.FindAsync(id);

            if (productTypeDetail == null)
            {
                return NotFound();
            }

            return productTypeDetail;
        }

        [HttpGet]
        [Route("GetAllByProductType/{id:int}")]
        public async Task<ActionResult<IEnumerable<ProductTypeDetail>>> GetAllByProductType(int id)
        {
            var productTypeDetail = await _context.ProductTypeDetail
                                                   .Where(c => c.ProductTypeId == id && c.Status != "Inactive")
                                                   .ToListAsync();

            return Ok(productTypeDetail);
        }

        // PUT: api/ProductTypeDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductTypeDetail(string id, ProductTypeDetail productTypeDetail)
        {
            if (id != productTypeDetail.Id)
            {
                return BadRequest();
            }

            // Kiểm tra xem đã có chi tiết loại sản phẩm có tên giống như productTypeDetail.DetailName chưa
            var existing = await _context.ProductTypeDetail
                .FirstOrDefaultAsync(p => p.DetailName == productTypeDetail.DetailName && p.Id != id);

            if (existing != null)
            {
                return BadRequest("Chi tiết loại sản phẩm đã tồn tại!");
            }

            _context.Entry(productTypeDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeDetailExists(id))
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


        // POST: api/ProductTypeDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductTypeDetail>> PostProductTypeDetail(ProductTypeDetail productTypeDetail)
        {
            var existing = await _context.ProductTypeDetail.FirstOrDefaultAsync(p => p.DetailName == productTypeDetail.DetailName);
            if (existing != null)
            {
                return BadRequest("Chi tiết loại sản phẩm đã tồn tại!");
            }

            // Tạo GUID mới
            Guid newGuid = Guid.NewGuid();

            // Lấy 10 kí tự đầu của GUID
            string idPrefix = newGuid.ToString("N").Substring(0, 10);

            // Gán ID mới cho productTypeDetail
            productTypeDetail.Id = idPrefix;

            // Thêm productTypeDetail vào DbSet
            _context.ProductTypeDetail.Add(productTypeDetail);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về response với HTTP status code 201 Created
            return CreatedAtAction("GetProductTypeDetail", new { id = productTypeDetail.Id }, productTypeDetail);
        }


        // DELETE: api/ProductTypeDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductTypeDetail(int id)
        {
            var productTypeDetail = await _context.ProductTypeDetail.FindAsync(id);
            if (productTypeDetail == null)
            {
                return NotFound();
            }

            _context.ProductTypeDetail.Remove(productTypeDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductTypeDetailExists(string id)
        {
            return _context.ProductTypeDetail.Any(e => e.Id == id);
        }



        [HttpGet("filterStatus/{id}")]
        public async Task<ActionResult<IEnumerable<ProductTypeDetail>>> GetByStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<ProductTypeDetail> query = _context.ProductTypeDetail;

            if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == "Inactive");
            }
            else
            {
                query = query.Where(p => p.Status != "Inactive");
            }

            query = query.Where(p => p.ProductTypeId == id);

            var filtered = await query.ToListAsync();

            return Ok(filtered);
        }


        [HttpPut("updateStatus/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(string id, ProductTypeDetail Model)
        {
            var response = await _context.ProductTypeDetail.FindAsync(id);

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
