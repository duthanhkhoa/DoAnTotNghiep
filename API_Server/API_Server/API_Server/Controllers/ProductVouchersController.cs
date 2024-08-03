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
    public class ProductVouchersController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ProductVouchersController(API_ServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVoucher>>> GetProductType()
        {
            var active = await _context.ProductVoucher
                                               .Where(p => p.Status != "Inactive")
                                               .ToListAsync();
            return Ok(active);
        }

        // GET: api/ProductVouchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductVoucher>> GetProductVoucher(string id)
        {
            var productVoucher = await _context.ProductVoucher.FindAsync(id);

            if (productVoucher == null)
            {
                return NotFound();
            }

            return productVoucher;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImportInvoice(string Id, [FromBody] ProductVoucherModel productVoucherModel)
        {
            if (Id != productVoucherModel.Id)
            {
                return BadRequest("Id không trùng khớp");
            }

            // Tìm ProductVoucher hiện có trong cơ sở dữ liệu
            var productVoucher = await _context.ProductVoucher.FindAsync(Id);
            if (productVoucher == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính của ProductVoucher
            productVoucher.DiscountPercentage = productVoucherModel.DiscountPercentage;
            productVoucher.UserName = productVoucherModel.UserName;
            productVoucher.DateAdded = productVoucherModel.DateAdded;
            productVoucher.UpdateDay = DateTime.Now;
            productVoucher.Status = productVoucherModel.Status;

            // Cập nhật ProductVoucher trong context
            _context.ProductVoucher.Update(productVoucher);

            // Tìm các ProductVoucherDetail hiện có trong cơ sở dữ liệu
            var existingDetails = await _context.ProductVoucherDetail
                .Where(d => d.ProductVoucherId == productVoucherModel.Id)
                .ToListAsync();

            // Xóa tất cả các chi tiết sản phẩm giảm giá hiện có
            _context.ProductVoucherDetail.RemoveRange(existingDetails);

            // Cập nhật hoặc thêm mới các chi tiết sản phẩm giảm giá
            foreach (var productModel in productVoucherModel.Products)
            {
                // Kiểm tra xem ProductVoucherDetail đã tồn tại hay chưa
                var existingDetail = await _context.ProductVoucherDetail
                    .FirstOrDefaultAsync(d => d.ProductId == productModel.ProductId);

                // Nếu tồn tại, xóa detail cũ
                if (existingDetail != null)
                {
                    _context.ProductVoucherDetail.Remove(existingDetail);
                }

                // Tạo detail mới
                ProductVoucherDetail detail = new ProductVoucherDetail
                {
                    ProductVoucherId = productVoucherModel.Id,
                    ProductId = productModel.ProductId,
                    Status = "Active"
                };

                // Thêm ImportInvoiceDetail vào context
                _context.ProductVoucherDetail.Add(detail);

                // Tìm sản phẩm trong bảng Product theo productId
                var product = await _context.Product.FindAsync(productModel.ProductId);

                // Nếu sản phẩm tồn tại, cập nhật giá mới (NewPrice) dựa trên phần trăm giảm giá (DiscountPercentage)
                if (product != null)
                {
                    decimal discount = (productVoucherModel.DiscountPercentage / 100m) * product.Oldprice;
                    product.Price = product.Oldprice - discount;

                    // Lưu lại cập nhật giá sản phẩm vào cơ sở dữ liệu
                    _context.Product.Update(product);
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok();
        }




        [HttpPost]
        public async Task<IActionResult> CreateImportInvoice([FromBody] ProductVoucherModel productVoucherModel)
        {
            string productVoucherId = Guid.NewGuid().ToString().Substring(0, 10);

            // Tạo một instance mới của ImportInvoice từ ImportInvoiceModel
            ProductVoucher productVoucher = new ProductVoucher
            {
                Id = productVoucherId,
                DiscountPercentage = productVoucherModel.DiscountPercentage,
                UserName = productVoucherModel.UserName,
                DateAdded = DateTime.Now,
                UpdateDay = DateTime.Now,
                Status = "Active" 
            };

            // Thêm ImportInvoice vào context
            _context.ProductVoucher.Add(productVoucher);

            // Tạo các instance của ImportInvoiceDetail cho từng sản phẩm
            foreach (var productModel in productVoucherModel.Products)
            {
                // Kiểm tra xem ProductVoucherDetail đã tồn tại hay chưa
                var existingDetail = await _context.ProductVoucherDetail
                    .FirstOrDefaultAsync(d => d.ProductId == productModel.ProductId);

                // Nếu tồn tại, xóa detail cũ
                if (existingDetail != null)
                {
                    _context.ProductVoucherDetail.Remove(existingDetail);
                }

                // Tạo detail mới
                ProductVoucherDetail detail = new ProductVoucherDetail
                {
                    ProductVoucherId = productVoucherId,
                    ProductId = productModel.ProductId,
                    Status = "Active"
                };

                // Thêm ImportInvoiceDetail vào context
                _context.ProductVoucherDetail.Add(detail);

                // Tìm sản phẩm trong bảng Product theo productId
                var product = await _context.Product.FindAsync(productModel.ProductId);

                // Nếu sản phẩm tồn tại, cập nhật giá mới (NewPrice) dựa trên phần trăm giảm giá (DiscountPercentage)
                if (product != null)
                {
                    decimal discount = (productVoucherModel.DiscountPercentage / 100m) * product.Oldprice;
                    product.Price = product.Oldprice - discount;

                    // Lưu lại cập nhật giá sản phẩm vào cơ sở dữ liệu
                    _context.Product.Update(product);
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/ProductVouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductVoucher(string id)
        {
            var productVoucher = await _context.ProductVoucher.FindAsync(id);

            if (productVoucher == null)
            {
                return NotFound();
            }

            // Tìm tất cả các dòng dữ liệu trong bảng Image liên quan đến Product cần xóa.
            var ProductVoucherDetailToDelete = await _context.ProductVoucherDetail.Where(c => c.ProductVoucherId == id).ToListAsync();

            // Kiểm tra xem có dữ liệu Image cần xóa hay không.
            if (ProductVoucherDetailToDelete != null && ProductVoucherDetailToDelete.Count > 0)
            {
                // Xóa tất cả các dòng dữ liệu Image liên quan đến Product.
                _context.ProductVoucherDetail.RemoveRange(ProductVoucherDetailToDelete);
            }

            _context.ProductVoucher.Remove(productVoucher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductVoucherExists(string id)
        {
            return _context.ProductVoucher.Any(e => e.Id == id);
        }

        [HttpPut("updateStatus/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(string id, ProductVoucher model)
        {
            var productVoucher = await _context.ProductVoucher.FindAsync(id);

            if (productVoucher == null)
            {
                return NotFound();
            }

            productVoucher.Status = model.Status;
            _context.Entry(productVoucher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Tìm danh sách ProductVoucherDetail theo ProductVoucherId
            var productVoucherDetails = await _context.ProductVoucherDetail
                .Where(pvd => pvd.ProductVoucherId == productVoucher.Id)
                .ToListAsync();

            // Cập nhật giá sản phẩm theo trạng thái của ProductVoucher
            foreach (var productVoucherDetail in productVoucherDetails)
            {
                var product = await _context.Product.FindAsync(productVoucherDetail.ProductId);
                if (product != null)
                {
                    if (model.Status == "Active")
                    {
                        decimal discount = (productVoucher.DiscountPercentage / 100m) * product.Oldprice;
                        product.Price = product.Oldprice - discount;
                    }
                    else if (model.Status == "Inactive")
                    {
                        product.Price = product.Oldprice;
                    }

                    _context.Product.Update(product);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<ProductVoucher>>> GetByStatus(string status)
        {
            IQueryable<ProductVoucher> query = _context.ProductVoucher;

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
    }
}
