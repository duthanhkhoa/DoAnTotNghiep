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
    public class InvoiceDetailsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public InvoiceDetailsController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDetail>>> GetInvoiceDetail()
        {
            return await _context.InvoiceDetail.ToListAsync();
        }

        // GET: api/InvoiceDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDetail>> GetInvoiceDetail(int id)
        {
            var invoiceDetail = await _context.InvoiceDetail.FindAsync(id);

            if (invoiceDetail == null)
            {
                return NotFound();
            }

            return invoiceDetail;
        }

        [HttpGet("byInvoiceIds")]
        public async Task<IActionResult> GetInvoiceDetailsByInvoiceIds([FromQuery] List<string> invoiceIds)
        {
            var invoiceDetails = await _context.InvoiceDetail
                                               .Where(id => invoiceIds.Contains(id.InvoiceId))
                                               .ToListAsync();
            return Ok(invoiceDetails);
        }

        [HttpGet]
        [Route("GetAllById/{id}")]
        public async Task<ActionResult<InvoiceDetail>> GetAllById(string id)
        {
            var invoiceDetails = await _context.InvoiceDetail.Where(c => c.InvoiceId == id).ToListAsync();

            if (invoiceDetails == null)
            {
                return NotFound();
            }

            return Ok(invoiceDetails);
        }

        [HttpGet("Products/{id}")]
        public async Task<ActionResult<List<Product>>> GetProductsByInvoiceId(string id)
        {
            // Lấy danh sách InvoiceDetail theo InvoiceId
            var invoiceDetails = await _context.InvoiceDetail
                                            .Where(detail => detail.InvoiceId == id)
                                            .ToListAsync();

            if (invoiceDetails == null || invoiceDetails.Count == 0)
            {
                return NotFound("No invoice details found for this invoice.");
            }

            // Lấy danh sách ProductId từ InvoiceDetail
            var productIds = invoiceDetails.Select(detail => detail.ProductId).ToList();

            // Lấy danh sách sản phẩm từ Product hoặc bảng sản phẩm tương ứng
            var products = await _context.Product
                                        .Where(product => productIds.Contains(product.Id))
                                        .ToListAsync();

            return products;
        }

        // PUT: api/InvoiceDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceDetail(int id, InvoiceDetail invoiceDetail)
        {
            if (id != invoiceDetail.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceDetailExists(id))
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




        //[HttpPost]
        //public async Task<ActionResult<InvoiceDetail>> PostInvoiceDetail(InvoiceDetail invoiceDetail)
        //{
        //    _context.InvoiceDetail.Add(invoiceDetail);

        //    // Tìm sản phẩm dựa trên ProductId của InvoiceDetail
        //    var product = await _context.Product.FindAsync(invoiceDetail.ProductId);

        //    if (product == null)
        //    {
        //        return NotFound(); 
        //    }

        //    product.QuantityAvailable -= invoiceDetail.Quantity;

        //    // Cập nhật sản phẩm trong cơ sở dữ liệu
        //    _context.Product.Update(product);

        //    // Lưu các thay đổi vào cơ sở dữ liệu
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetInvoiceDetail", new { id = invoiceDetail.Id }, invoiceDetail);
        //}

        [HttpPost]
        public async Task<ActionResult<InvoiceDetail>> PostInvoiceDetail(InvoiceDetail invoiceDetail)
        {
            _context.InvoiceDetail.Add(invoiceDetail);

            // Tìm sản phẩm dựa trên ProductId của InvoiceDetail
            var product = await _context.Product.FindAsync(invoiceDetail.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            // Cập nhật QuantityAvailable của sản phẩm
            product.QuantityAvailable -= invoiceDetail.Quantity;

            // Cập nhật sản phẩm trong cơ sở dữ liệu
            _context.Product.Update(product);

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInvoiceDetail", new { id = invoiceDetail.Id }, invoiceDetail);
        }



        // DELETE: api/InvoiceDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceDetail(int id)
        {
            var invoiceDetail = await _context.InvoiceDetail.FindAsync(id);
            if (invoiceDetail == null)
            {
                return NotFound();
            }

            _context.InvoiceDetail.Remove(invoiceDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceDetailExists(int id)
        {
            return _context.InvoiceDetail.Any(e => e.Id == id);
        }
    }
}
