using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using API_Server.ModelView;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportInvoicesController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ImportInvoicesController(API_ServerContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImportInvoiceModel>>> GetImportInvoice()
        {
            var importInvoices = await _context.ImportInvoice
                .Include(i => i.User)
                .Include(i => i.Supplier)
                .OrderByDescending(i => i.Date)
                .Select(i => new ImportInvoiceModel
                {
                    Id = i.Id,
                    UserFullName = i.User.FullName,
                    SupplierId = i.SupplierId,
                    SupplierName = i.Supplier.Name,
                    Date = i.Date,
                    TotalAmount = i.TotalAmount,
                    PaymentMethod = i.PaymentMethod,
                    status = i.Status
                })
                .ToListAsync();

            return Ok(importInvoices);
        }

        // GET: api/ImportInvoices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImportInvoice>> GetImportInvoice(string id)
        {
            var importInvoice = await _context.ImportInvoice.FindAsync(id);

            if (importInvoice == null)
            {
                return NotFound();
            }

            return importInvoice;
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImportInvoice(string id, ImportInvoice importInvoice)
        {
            if (id != importInvoice.Id)
            {
                return BadRequest();
            }

            _context.Entry(importInvoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImportInvoiceExists(id))
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

        [HttpPost]
        public async Task<IActionResult> CreateImportInvoice([FromBody] ImportInvoiceModel importInvoiceModel)
        {
            // Tạo một ID mới cho ImportInvoice bằng GUID với 10 ký tự đầu tiên
            string importInvoiceId = Guid.NewGuid().ToString().Substring(0, 10);

            // Tính tổng số tiền
            decimal totalAmount = importInvoiceModel.Products.Sum(p => p.Price);

            // Tạo một instance mới của ImportInvoice từ ImportInvoiceModel
            ImportInvoice importInvoice = new ImportInvoice
            {
                Id = importInvoiceId,
                UserId = importInvoiceModel.UserId,
                SupplierId = importInvoiceModel.SupplierId,
                Date = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = importInvoiceModel.PaymentMethod,
                Status = importInvoiceModel.status
            };

            // Thêm ImportInvoice vào context
            _context.ImportInvoice.Add(importInvoice);

            // Tạo các instance của ImportInvoiceDetail cho từng sản phẩm
            foreach (var productModel in importInvoiceModel.Products)
            {
                ImportInvoiceDetail detail = new ImportInvoiceDetail
                {
                    ImportInvoiceId = importInvoiceId,
                    ProductId = productModel.ProductId,
                    Quantity = productModel.Quantity,
                    Price = productModel.Price,
                    Status = "Active"
                };

                // Thêm ImportInvoiceDetail vào context
                _context.ImportInvoiceDetail.Add(detail);

                // Nếu trạng thái đơn hàng là "Đã thanh toán", cập nhật sản phẩm và kho hàng
                if (importInvoiceModel.status == "Success")
                {
                    // Tìm sản phẩm trong bảng Product theo productId
                    var product = await _context.Product.FindAsync(productModel.ProductId);

                    // Nếu sản phẩm tồn tại, cập nhật số lượng có sẵn (QuantityAvailable)
                    if (product != null)
                    {
                        product.QuantityAvailable += productModel.Quantity;
                        // Lưu lại cập nhật số lượng sản phẩm vào cơ sở dữ liệu
                        _context.Product.Update(product);

                    }
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về ImportInvoice đã được tạo
            return CreatedAtAction("GetImportInvoice", new { id = importInvoice.Id }, importInvoice);
        }


        // DELETE: api/ImportInvoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImportInvoice(int id)
        {
            var importInvoice = await _context.ImportInvoice.FindAsync(id);
            if (importInvoice == null)
            {
                return NotFound();
            }

            _context.ImportInvoice.Remove(importInvoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImportInvoiceExists(string id)
        {
            return _context.ImportInvoice.Any(e => e.Id == id);
        }


        [HttpPut("updateStatus/{id}")]
        public async Task<IActionResult> UpdateInvoiceStatus(string id, ImportInvoice importInvoice)
        {
            var inv = await _context.ImportInvoice.FindAsync(id);

            if (inv == null)
            {
                return NotFound();
            }

            inv.Status = importInvoice.Status;

            _context.Entry(inv).State = EntityState.Modified;

            if (importInvoice.Status == "Success")
            {
                // Lấy danh sách chi tiết hóa đơn nhập theo id hóa đơn
                var invoiceDetails = await _context.ImportInvoiceDetail.Where(d => d.ImportInvoiceId == id).ToListAsync();

                foreach (var detail in invoiceDetails)
                {
                    // Tìm sản phẩm trong bảng Product theo productId
                    var product = await _context.Product.FindAsync(detail.ProductId);

                    // Nếu sản phẩm tồn tại, cập nhật số lượng có sẵn (QuantityAvailable)
                    if (product != null)
                    {
                        product.QuantityAvailable += detail.Quantity;
                        // Lưu lại cập nhật số lượng sản phẩm vào cơ sở dữ liệu
                        _context.Product.Update(product);

                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<ImportInvoiceModel>>> GetByStatus(string status)
        {
            try
            {
                var filteredInvoices = await _context.ImportInvoice
                    .Where(i => i.Status == status)
                    .Include(i => i.User)
                    .Include(i => i.Supplier)
                    .Select(i => new ImportInvoiceModel
                    {
                        Id = i.Id,
                        UserFullName = i.User.FullName,
                        SupplierId = i.SupplierId,
                        SupplierName = i.Supplier.Name,
                        Date = i.Date,
                        TotalAmount = i.TotalAmount,
                        PaymentMethod = i.PaymentMethod,
                        status = i.Status
                    })
                    .ToListAsync();

                if (filteredInvoices == null || filteredInvoices.Count == 0)
                {
                    return NotFound();
                }

                return Ok(filteredInvoices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không xác định: {ex.Message}");
            }
        }
    }
}
