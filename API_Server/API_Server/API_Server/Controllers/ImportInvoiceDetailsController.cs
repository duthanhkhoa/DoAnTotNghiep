using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using API_Server.ModelView;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportInvoiceDetailsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ImportInvoiceDetailsController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/ImportInvoiceDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImportInvoiceDetail>>> GetImportInvoiceDetail()
        {
            return await _context.ImportInvoiceDetail.ToListAsync();
        }

        // GET: api/ImportInvoiceDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImportInvoiceDetail>> GetImportInvoiceDetail(int id)
        {
            var importInvoiceDetail = await _context.ImportInvoiceDetail.FindAsync(id);

            if (importInvoiceDetail == null)
            {
                return NotFound();
            }

            return importInvoiceDetail;
        }

        //[HttpGet]
        //[Route("GetAllById/{id}")]
        //public async Task<ActionResult<ImportInvoiceDetail>> GetAllById(string id)
        //{
        //    var invoiceDetails = await _context.ImportInvoiceDetail.Where(c => c.ImportInvoiceId == id).ToListAsync();

        //    if (invoiceDetails == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(invoiceDetails);
        //}


        [HttpGet]
        [Route("GetAllById/{id}")]
        public async Task<ActionResult<IEnumerable<ImportInvoiceDetailModel>>> GetAllById(string id)
        {
            var invoiceDetails = await _context.ImportInvoiceDetail
                .Where(c => c.ImportInvoiceId == id)
                .Include(c => c.Product) // Bao gồm dữ liệu liên quan từ bảng Product
                .ToListAsync();

            if (invoiceDetails == null || invoiceDetails.Count == 0)
            {
                return NotFound();
            }

            // Chuyển đổi sang DTO
            var invoiceDetailDtos = invoiceDetails.Select(detail => new ImportInvoiceDetailModel
            {
                Id = detail.Id,
                ImportInvoiceId = detail.ImportInvoiceId,
                ProductId = detail.ProductId,
                ProductName = detail.Product.ProductName,
                ProductImage = detail.Product.Image,
                Quantity = detail.Quantity,
                Price = detail.Price,
                Status = detail.Status
            }).ToList();

            return Ok(invoiceDetailDtos);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutImportInvoiceDetail(int id, ImportInvoiceDetail importInvoiceDetail)
        {
            if (id != importInvoiceDetail.Id)
            {
                return BadRequest();
            }

            _context.Entry(importInvoiceDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImportInvoiceDetailExists(id))
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

        // POST: api/ImportInvoiceDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ImportInvoiceDetail>> PostImportInvoiceDetail(ImportInvoiceDetail importInvoiceDetail)
        {
            _context.ImportInvoiceDetail.Add(importInvoiceDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImportInvoiceDetail", new { id = importInvoiceDetail.Id }, importInvoiceDetail);
        }

        // DELETE: api/ImportInvoiceDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImportInvoiceDetail(int id)
        {
            var importInvoiceDetail = await _context.ImportInvoiceDetail.FindAsync(id);
            if (importInvoiceDetail == null)
            {
                return NotFound();
            }

            _context.ImportInvoiceDetail.Remove(importInvoiceDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImportInvoiceDetailExists(int id)
        {
            return _context.ImportInvoiceDetail.Any(e => e.Id == id);
        }
    }
}
