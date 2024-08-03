using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using Microsoft.CodeAnalysis;
using API_Server.ModelView;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly API_ServerContext _context;

        public ProductsController(API_ServerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lấy danh sách sản phẩm có Status là Active hoạt động
        [HttpGet("AdminUse")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            // Sử dụng LINQ để lọc sản phẩm có status khác Inactive
            var activeProducts = await _context.Product
                                               .Where(p => p.Status == "Active")
                                               .ToListAsync();
            return Ok(activeProducts);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductUse()
        {
            // Sử dụng LINQ để lọc sản phẩm có status khác Inactive và QuantityAvailable > 0
            var activeProducts = await _context.Product
                                               .Where(p => p.Status == "Active" && p.QuantityAvailable > 0)
                                               .Select(p => new
                                               {
                                                   p.Id, p.ProductName, p.ISBN, p.CoverType, p.Description, p.Oldprice, p.Price, p.QuantityAvailable, p.Author, p.Publisher, p.PublishedDate,
                                                   p.ProductTypeDetailId, p.ProductTypeDetail, p.Image, p.Status,
                                                   // Tính toán phần trăm giảm giá
                                                   DiscountPercentage = Math.Round((p.Oldprice - p.Price) / p.Oldprice * 100, 2),
                                                    // Lấy số sao đánh giá trung bình
                                                   AverageRating = _context.Rating
                                                                  .Where(r => r.ProductId == p.Id)
                                                                  .Average(r => (double?)r.Ratings) ?? 0,
                                                   NumberOfRatings = _context.Rating.Where(r => r.ProductId == p.Id).Count()
                                               })
                                               .ToListAsync();
            return Ok(activeProducts);
        }

        [HttpGet("Popular")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Lấy danh sách 10 sản phẩm bán chạy nhất từ bảng InvoiceDetail
            var topSellingProductIds = await _context.InvoiceDetail
                                                     .GroupBy(i => i.ProductId)
                                                     .OrderByDescending(g => g.Sum(i => i.Quantity))
                                                     .Take(10)
                                                     .Select(g => g.Key)
                                                     .ToListAsync();

            // Lấy thông tin chi tiết của các sản phẩm bán chạy nhất
            var topSellingProducts = await _context.Product
                                                   .Where(p => topSellingProductIds.Contains(p.Id) && p.Status == "Active")
                                                   .Select(p => new
                                                   {
                                                       p.Id, p.ProductName, p.ISBN, p.CoverType, p.Description, p.Oldprice, p.Price, p.QuantityAvailable,
                                                       p.Author, p.Publisher, p.PublishedDate, p.ProductTypeDetailId, p.ProductTypeDetail, p.Image, p.Status,
                                                       // Tính toán phần trăm giảm giá
                                                       DiscountPercentage = Math.Round((p.Oldprice - p.Price) / p.Oldprice * 100, 2),
                                                       // Lấy số sao đánh giá trung bình
                                                       AverageRating = _context.Rating
                                                                  .Where(r => r.ProductId == p.Id)
                                                                  .Average(r => (double?)r.Ratings) ?? 0.0
                                                   })
                                                   .ToListAsync();
            return Ok(topSellingProducts);
        }

        [HttpGet("RecentProduct")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductImport()
        {
            // Tìm hóa đơn nhập gần nhất
            var latestImportInvoice = await _context.ImportInvoice
                                                    .OrderByDescending(ii => ii.Date)
                                                    .FirstOrDefaultAsync();

            List<string> latestProductIds = new List<string>();

            if (latestImportInvoice != null)
            {
                // Lấy danh sách 9 sản phẩm từ chi tiết hóa đơn của hóa đơn nhập gần nhất
                latestProductIds = await _context.ImportInvoiceDetail
                                                 .Where(iid => iid.ImportInvoiceId == latestImportInvoice.Id)
                                                 .Take(9)
                                                 .Select(iid => iid.ProductId)
                                                 .ToListAsync();
            }

            // Lấy thông tin chi tiết của các sản phẩm
            var latestProducts = await _context.Product
                                               .Where(p => latestProductIds.Contains(p.Id) && p.Status == "Active")
                                               .Select(p => new
                                               {
                                                    p.Id, p.ProductName, p.ISBN, p.CoverType, p.Description, p.Oldprice, p.Price, p.QuantityAvailable,
                                                    p.Author, p.Publisher, p.PublishedDate, p.ProductTypeDetailId, p.ProductTypeDetail, p.Image, p.Status,
                                                       // Tính toán phần trăm giảm giá
                                                    DiscountPercentage = Math.Round((p.Oldprice - p.Price) / p.Oldprice * 100, 2),
                                                       // Lấy số sao đánh giá trung bình
                                                    AverageRating = _context.Rating
                                                                  .Where(r => r.ProductId == p.Id)
                                                                  .Average(r => (double?)r.Ratings) ?? 0.0
                                               })
                                               .ToListAsync();
            return Ok(latestProducts);
        }

        // Lọc danh sách sản phẩm theo trạng thái
        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByStatus(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<Product> query = _context.Product;

            if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == "Inactive");
            }
            else
            {
                query = query.Where(p => p.Status != "Inactive");
            }

            var filteredProducts = await query.ToListAsync();

            return Ok(filteredProducts);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("Detail/{id}")]
        public async Task<ActionResult<object>> GetProductUser(string id)
        {
            var product = await _context.Product
                .Where(p => p.Id == id && p.Status == "Active" && p.QuantityAvailable > 0)
                .Select(p => new
                {
                    p.Id,p.ProductName, p.ISBN, p.CoverType, p.Description,  p.Oldprice, p.Price, p.QuantityAvailable, p.Author, p.Publisher, p.PublishedDate, 
                    p.ProductTypeDetailId, p.ProductTypeDetail, p.Image, p.Status,
                    // Tính toán phần trăm giảm giá
                    DiscountPercentage = Math.Round((p.Oldprice - p.Price) / p.Oldprice * 100, 2),
                    // Lấy số sao đánh giá trung bình
                    AverageRating = _context.Rating
                                            .Where(r => r.ProductId == p.Id)
                                            .Average(r => (double?)r.Ratings) ?? 0.0,
                    // Lấy số lượng đánh giá
                    NumberOfRatings = _context.Rating
                                             .Where(r => r.ProductId == p.Id)
                                             .Count()
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("byProductIds")]
        public async Task<IActionResult> GetProductsByProductIds([FromQuery] List<string> productIds)
        {
            var products = await _context.Product
                                         .Where(p => productIds.Contains(p.Id))
                                         .ToListAsync();
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query is required.");
            }

            var products = await _context.Product
                .Where(p => p.ProductName.Contains(query))
                .ToListAsync();

            return Ok(products);
        }

        [HttpPut("updateStatus/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(string id, Product Model)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Status = Model.Status;

            _context.Entry(product).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, [FromForm] Product product, IFormFileCollection images)
        {
            var existing = await _context.Product.FirstOrDefaultAsync(p => p.ProductName == product.ProductName && p.Id != id);
            if (existing != null)
            {
                return BadRequest("Tên sản phẩm đã tồn tại!");
            }

            if (id != product.Id)
            {
                return BadRequest();
            }

            bool isFirstImage = false;
            int start = 0;

            string imagesFolder = Path.Combine(_env.WebRootPath, "images", "book");
            string filePath = Path.Combine(imagesFolder, product.Image);

            if (!System.IO.File.Exists(filePath) && images.Count > 0)
            {
                //string uniqueFileName = product.Id + Path.GetExtension(images[0].FileName);
                string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[0].FileName);
                product.Image = uniqueFileName;

                var firstImageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\book", uniqueFileName);
                using (var stream = System.IO.File.Create(firstImageFilePath))
                {
                    await images[0].CopyToAsync(stream);
                }

                isFirstImage = true;
            }

            if (!System.IO.File.Exists(filePath) && images.Count == 0)
            {
                product.Image = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(product.Image);
            }

            if (isFirstImage)
            {
                start = 1;
            }

            List<Image> uploadedFiles = new List<Image>();
            for (int i = start; i < images.Count; i++)
            {
                var fileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[i].FileName);

                var filePathDetail = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\book", fileName);
                using (var stream = System.IO.File.Create(filePathDetail))
                {
                    await images[i].CopyToAsync(stream);
                }

                //uploadedFileNames.Add(fileName);
                uploadedFiles.Add(new Image { ProductId = product.Id, Image_url = fileName });
            }

            _context.Entry(product).State = EntityState.Modified;
            _context.Image.AddRange(uploadedFiles);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> UploadProductWithImages([FromForm] Product product, IFormFileCollection images)
        {
            try
            {
                // Kiểm tra xem sản phẩm có tồn tại trong cơ sở dữ liệu dựa trên tên sản phẩm
                var existingProduct = await _context.Product.FirstOrDefaultAsync(p => p.ProductName == product.ProductName);
                if (existingProduct != null)
                {
                    return BadRequest("Sản phẩm đã tồn tại!");
                }

                //Xử lý ID của sản phẩm bằng GUID
                // Tạo GUID mới và kiểm tra nếu đã tồn tại trong dữ liệu thì tạo lại GUID mới
                string newProductId;
                bool isExist;
                do
                {
                    newProductId = Guid.NewGuid().ToString().Substring(0, 10);
                    isExist = await _context.Product.AnyAsync(p => p.Id == newProductId);
                } while (isExist);

                string newISBN;
                do
                {
                    newISBN = "978" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 9); // Bắt đầu với 978 để tạo ISBN 13
                    isExist = await _context.Product.AnyAsync(p => p.ISBN == newISBN);
                } while (isExist);

                product.ISBN = newISBN;
                product.Id = newProductId;
                product.Price = product.Oldprice;

                // Lưu tệp tin ảnh đầu tiên với GUID của sản phẩm
                if (images.Count > 0)
                {
                    string uniqueFileName = product.Id + Path.GetExtension(images[0].FileName);
                    product.Image = uniqueFileName;

                    var firstImageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\book", uniqueFileName);
                    using (var stream = System.IO.File.Create(firstImageFilePath))
                    {
                        await images[0].CopyToAsync(stream);
                    }
                }

                // Lưu các tệp tin ảnh còn lại với tên ngẫu nhiên
                //var uploadedFileNames = new List<string>();

                List<Image> uploadedFiles = new List<Image>();
                for (int i = 1; i < images.Count; i++)
                {
                    var fileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[i].FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\book", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await images[i].CopyToAsync(stream);
                    }

                    //uploadedFileNames.Add(fileName);
                    uploadedFiles.Add(new Image { ProductId = product.Id, Image_url = fileName });
                }

                // Lưu thông tin sản phẩm vào cơ sở dữ liệu
                _context.Product.Add(product);
                //await _context.SaveChangesAsync();

                _context.Image.AddRange(uploadedFiles);
                await _context.SaveChangesAsync();

                // Trả về thông tin sản phẩm đã được lưu
                return Ok(product);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            // Tìm tất cả các dòng dữ liệu trong bảng Image liên quan đến Product cần xóa.
            var imagesToDelete = await _context.Image.Where(c => c.ProductId == id).ToListAsync();

            // Kiểm tra xem có dữ liệu Image cần xóa hay không.
            if (imagesToDelete != null && imagesToDelete.Count > 0)
            {
                // Xóa tất cả các dòng dữ liệu Image liên quan đến Product.
                _context.Image.RemoveRange(imagesToDelete);
            }

            // Tìm Product cần xóa.
            var product = await _context.Product.FindAsync(id);

            // Kiểm tra xem Product có tồn tại không.
            if (product == null)
            {
                return NotFound();
            }

            // Xóa Product từ bảng Product.
            _context.Product.Remove(product);

            // Lưu các thay đổi vào cơ sở dữ liệu.
            await _context.SaveChangesAsync();

            // Trả về kết quả không có nội dung (NoContent) sau khi xóa thành công.
            return NoContent();
        }

        

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Product>>> FilterProducts(string productTypeDetailId)
        {
            if (string.IsNullOrWhiteSpace(productTypeDetailId))
            {
                return BadRequest("Phải cung cấp ProductTypeDetailId.");
            }

            try
            {
                List<Product> filteredProducts;

                // Kiểm tra xem tham số có phải là toàn số hay không
                bool isNumeric = productTypeDetailId.All(char.IsDigit);

                if (isNumeric)
                {
                    // Nếu là toàn số, gán nó cho productTypeId
                    int productTypeId = int.Parse(productTypeDetailId);

                    var productTypeDetailIds = await _context.ProductTypeDetail
                        .Where(ptd => ptd.ProductTypeId == productTypeId)
                        .Select(ptd => ptd.Id)
                        .ToListAsync();

                    filteredProducts = await _context.Product
                        .Where(p => productTypeDetailIds.Contains(p.ProductTypeDetailId) && p.QuantityAvailable > 0)
                        .ToListAsync();
                }
                else
                {
                    // Nếu chứa cả chữ và số, gán nó cho productTypeDetailId
                    filteredProducts = await _context.Product
                        .Where(p => p.ProductTypeDetailId == productTypeDetailId && p.QuantityAvailable > 0)
                        .ToListAsync();
                }

                if (filteredProducts == null || !filteredProducts.Any())
                {
                    return Ok(filteredProducts);
                }

                return Ok(filteredProducts);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi (giả định có cơ chế logging)
                return StatusCode(500, "Lỗi máy chủ nội bộ: " + ex.Message);
            }
        }


        [HttpGet("summaryProduct")]
        public async Task<IActionResult> GetProductSummary()
        {
            // Lấy tổng số sách
            var totalBooks = await _context.Product.CountAsync();

            // Lấy tổng số loại sách (dựa trên ProductTypeDetailId)
            var totalBookTypes = await _context.ProductTypeDetail
                .Select(p => p.Id)
                .Distinct()
                .CountAsync();

            // Lấy tổng số lượng sách
            var totalQuantity = await _context.Product
                .SumAsync(p => p.QuantityAvailable);

            var summary = new
            {
                TotalBooks = totalBooks,
                TotalBookTypes = totalBookTypes,
                TotalQuantity = totalQuantity
            };

            return Ok(summary);
        }
    }
}
