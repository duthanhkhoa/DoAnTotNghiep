using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Globalization;
using API_Server.ModelView;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public InvoicesController(API_ServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoice()
        {
            try
            {
                var invoices = await _context.Invoice.OrderByDescending(i => i.Date).ToListAsync();

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không xác định: {ex.Message}");
            }
        }

        // GET: api/Invoices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(string id)
        {
            var invoice = await _context.Invoice.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return invoice;
        }

        //[HttpGet("ByUserId")]
        //public async Task<IActionResult> GetInvoicesByUserId(string userId)
        //{
        //    try
        //    {
        //        var invoices = await _context.Invoice
        //                                    .Where(i => i.UserId == userId)
        //                                    .ToListAsync();

        //        return Ok(invoices);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log lỗi
        //        Console.Error.WriteLine($"Error fetching invoices for user {userId}: {ex.Message}");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        // PUT: api/Invoices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoice(string id, Invoice invoice)
        {
            if (id != invoice.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
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

        [HttpPut("updateStatus/{id}")]
        public async Task<IActionResult> UpdateInvoiceStatus(string id, Invoice invoice)
        {
            var inv = await _context.Invoice.FindAsync(id);

            if (inv == null)
            {
                return NotFound("Id không trùng khớp");
            }

            // Cập nhật trạng thái của Invoice
            inv.Status = invoice.Status;

            // Kiểm tra và cập nhật payment_status dựa trên giá trị của status nếu payment_status không phải là "Paid"
            if (inv.payment_status != "Paid")
            {
                if (invoice.Status == "Canceled")
                {
                    inv.payment_status = "Canceled";
                   
                    var invoiceDetails = _context.InvoiceDetail.Where(d => d.InvoiceId == id).ToList();

                    foreach (var detail in invoiceDetails)
                    {
                        var product = await _context.Product.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                           
                            product.QuantityAvailable += detail.Quantity;
                        }
                    }
                }
                else if (invoice.Status == "Delivered")
                {
                    inv.payment_status = "Paid";
                }
                else if (invoice.Status == "Shipped")
                {
                    inv.payment_status = "Pending";
                }
                else if (invoice.Status == "Processed")
                {
                    inv.payment_status = "Processed";
                }
            }

            if (invoice.Status == "Returned")
            {
                inv.payment_status = "Refunded";
            }

            _context.Entry(inv).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesByStatus(string status)
        {
            try
            {
                var filtered = await _context.Invoice.Where(i => i.Status == status).ToListAsync();

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không xác định: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
        {
            try
            {
                var voucher = await _context.Voucher.FindAsync(invoice.VoucherId);

                _context.Invoice.Add(invoice);
                await _context.SaveChangesAsync();

                // Cập nhật số lượng voucher nếu có sử dụng
                if (voucher != null)
                {
                    voucher.Quantity--; // Giảm số lượng voucher đi 1
                    if (voucher.Quantity == 0)
                    {
                        voucher.Status = "Inactive"; // Thay đổi trạng thái thành không hoạt động khi hết số lượng
                    }
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction("GetInvoice", new { id = invoice.Id }, invoice);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // DELETE: api/Invoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(string id)
        {
            var invoice = await _context.Invoice.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _context.Invoice.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceExists(string id)
        {
            return _context.Invoice.Any(e => e.Id == id);
        }


        [HttpGet("Summary")]
        public async Task<IActionResult> GetSummary(string status)
        {
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1);  // Ngày đầu tiên của tháng hiện tại
            var endDate = startDate.AddMonths(1).AddDays(-1);  // Ngày cuối cùng của tháng hiện tại

            IQueryable<Invoice> query = _context.Invoice;

            switch (status)
            {
                case "daily":
                    query = query.Where(i => i.Date >= startDate && i.Date <= endDate);
                    break;
                case "weekly":
                    var invoices = await query.ToListAsync();  // Lấy toàn bộ danh sách hóa đơn
                    var weeklySummary = invoices
                        .GroupBy(i => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(i.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        .Select(g => new
                        {
                            Label = $"Tuần {g.Key}",
                            TotalAmount = g.Sum(i => i.TotalAmount),
                            TotalOrder = g.Count()
                        })
                        .ToList();  // Chuyển đổi kết quả thành danh sách bộ nhớ

                    return Ok(weeklySummary);
                case "monthly":
                    var monthlySummary = await query
                        .GroupBy(i => i.Date.Month)
                        .Select(g => new
                        {
                            Label = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                            TotalAmount = g.Sum(i => i.TotalAmount),
                            TotalOrder = g.Count()
                        })
                        .ToListAsync();

                    return Ok(monthlySummary);
                case "yearly":
                    var yearlySummary = await query
                        .GroupBy(i => i.Date.Year)
                        .Select(g => new
                        {
                            Label = g.Key,
                            TotalAmount = g.Sum(i => i.TotalAmount),
                            TotalOrder = g.Count()
                        })
                        .ToListAsync();

                    return Ok(yearlySummary);
                default:
                    return BadRequest("Invalid status parameter. Supported values: daily, weekly, monthly, yearly.");
            }

            var summary = await query
                .GroupBy(i => status == "daily" ? i.Date.Day :
                             status == "weekly" ? CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(i.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) :
                             status == "monthly" ? i.Date.Month :
                             status == "yearly" ? i.Date.Year : 0)
                .Select(g => new
                {
                    Label = status == "daily" ? g.Key.ToString() :
                            status == "weekly" ? $"Tuần {g.Key}" :
                            status == "monthly" ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key) :
                            status == "yearly" ? g.Key.ToString() : "",
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalOrder = g.Count()
                })
                .ToListAsync();

            return Ok(summary);
        }

        // Lấy doanh thu theo
        [HttpGet("current-revenue")]
        public async Task<IActionResult> GetCurrentRevenue()
        {
            var today = DateTime.Today;

            // Ngày đầu tiên của tuần hiện tại (giả sử tuần bắt đầu từ thứ Hai)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            // Ngày đầu tiên của tháng hiện tại
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Ngày đầu tiên của năm hiện tại
            var startOfYear = new DateTime(today.Year, 1, 1);

            var dailyRevenue = await _context.Invoice
                .Where(i => i.Date.Date == today)
                .GroupBy(i => i.Date.Date)
                .Select(g => new
                {
                    Label = "Ngày hôm nay",
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalOrder = g.Count()
                })
                .FirstOrDefaultAsync();

            var weeklyRevenue = await _context.Invoice
                .Where(i => i.Date >= startOfWeek && i.Date < startOfWeek.AddDays(7))
                .GroupBy(i => 1)
                .Select(g => new
                {
                    Label = "Tuần này",
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalOrder = g.Count()
                })
                .FirstOrDefaultAsync();

            var monthlyRevenue = await _context.Invoice
                .Where(i => i.Date >= startOfMonth && i.Date < startOfMonth.AddMonths(1))
                .GroupBy(i => i.Date.Month)
                .Select(g => new
                {
                    Label = $"tháng {g.Key}",
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalOrder = g.Count()
                })
                .FirstOrDefaultAsync();

            var yearlyRevenue = await _context.Invoice
                .Where(i => i.Date >= startOfYear && i.Date < startOfYear.AddYears(1))
                .GroupBy(i => i.Date.Year)
                .Select(g => new
                {
                    Label = $"Năm {g.Key}",
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalOrder = g.Count()
                })
                .FirstOrDefaultAsync();

            var result = new
            {
                Daily = dailyRevenue,
                Weekly = weeklyRevenue,
                Monthly = monthlyRevenue,
                Yearly = yearlyRevenue
            };

            return Ok(result);
        }

        [HttpGet("Recent-Orders")]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoice15day()
        {
            try
            {
                var fifteenDaysAgo = DateTime.Now.AddDays(-15);
                var invoices = await _context.Invoice
                    .Where(i => i.Date >= fifteenDaysAgo)
                    .OrderByDescending(i => i.Date)
                    .ToListAsync();

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không xác định: {ex.Message}");
            }
        }

        
        [HttpGet("ByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoiceByUser(string userId)
        {
            try
            {
                // Lấy danh sách hóa đơn từ cơ sở dữ liệu với điều kiện userId
                var invoices = await _context.Invoice
                    .Include(i => i.User) // Đảm bảo load thông tin người dùng liên quan
                    .Include(i => i.Voucher) // Đảm bảo load thông tin voucher liên quan
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.Date)
                    .ToListAsync();

                // Kiểm tra nếu không tìm thấy hóa đơn thì trả về Not Found
                if (invoices == null || invoices.Count == 0)
                {
                    return NotFound("Không tìm thấy hóa đơn cho người dùng này.");
                }

                // Lấy danh sách chi tiết hóa đơn liên quan
                var invoiceIds = invoices.Select(i => i.Id).ToList();
                var invoiceDetails = await _context.InvoiceDetail
                    .Include(d => d.Product)
                    .Where(d => invoiceIds.Contains(d.InvoiceId))
                    .ToListAsync();

                // Tạo danh sách DTO cho kết quả trả về
                var invoiceDtos = invoices.Select(invoice => new InvoiceDto
                {
                    Id = invoice.Id,
                    UserId = invoice.UserId,
                    Date = invoice.Date,
                    TotalAmount = invoice.TotalAmount,
                    DiscountAmount = invoice.discount_amountt,
                    FinalAmount = invoice.final_amount,
                    PaymentMethod = invoice.PaymentMethod,
                    ShippingAddress = invoice.ShippingAddress,
                    ShippingPhone = invoice.ShippingPhone,
                    VoucherId = invoice.VoucherId,
                    PaymentStatus = invoice.payment_status,
                    Status = invoice.Status,
                    InvoiceDetails = invoiceDetails
                        .Where(detail => detail.InvoiceId == invoice.Id)
                        .Select(detail => new InvoiceDetailDto
                        {
                            Id = detail.Id,
                            ProductId = detail.ProductId,
                            ProductName = detail.Product.ProductName,
                            ProductImage = detail.Product.Image,
                            Quantity = detail.Quantity,
                            Price = detail.Price,
                            TotalPrice = detail.total_price,
                            Status = detail.Status
                        }).ToList()
                }).ToList();
                return Ok(invoiceDtos);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về lỗi 500 nếu có lỗi xảy ra
                return StatusCode(500, "Lỗi máy chủ nội bộ: " + ex.Message);
            }
        }


        // GET: api/Invoices/generate-unique-guid
        [HttpGet("generate-unique-guid")]
        public async Task<IActionResult> GenerateUniqueGuid()
        {
            try
            {
                string newGuid;
                bool isDuplicate = true;

                // Vòng lặp để đảm bảo chuỗi GUID là duy nhất
                do
                {
                    newGuid = Guid.NewGuid().ToString("N").Substring(0, 10);
                    isDuplicate = await _context.Invoice.AnyAsync(i => i.Id == newGuid);
                } while (isDuplicate);

                return Ok(newGuid);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating unique GUID: {ex.Message}");
            }
        }

        [HttpPut("Cancel/{id}")]
        public async Task<IActionResult> CancelInvoice(string id)
        {
            // Lấy hóa đơn bằng ID
            var invoice = await _context.Invoice.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái và trạng thái thanh toán của hóa đơn
            invoice.Status = "Canceled";
            invoice.payment_status = "Canceled";

            // Lấy tất cả các chi tiết hóa đơn cho hóa đơn này
            var invoiceDetails = _context.InvoiceDetail.Where(d => d.InvoiceId == id).ToList();

            // Lặp qua từng chi tiết hóa đơn
            foreach (var detail in invoiceDetails)
            {
                // Lấy sản phẩm liên quan
                var product = await _context.Product.FindAsync(detail.ProductId);
                if (product != null)
                {
                    // Cập nhật QuantityAvailable của sản phẩm
                    product.QuantityAvailable += detail.Quantity;
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("OrderDetail/{id}")]
        public async Task<ActionResult<InvoiceDto>> GetOrder(string id)
        {
            var invoice = await _context.Invoice
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // Join InvoiceDetails với Products để lấy ProductName và ProductImage
            var invoiceDetailDtos = await _context.InvoiceDetail
                .Where(detail => detail.InvoiceId == id)
                .Join(_context.Product,
                    detail => detail.ProductId,
                    product => product.Id,
                    (detail, product) => new InvoiceDetailDto
                    {
                        Id = detail.Id,
                        ProductId = detail.ProductId,
                        ProductName = product.ProductName,
                        ProductImage = product.Image,
                        Quantity = detail.Quantity,
                        Price = detail.Price,
                        TotalPrice = detail.total_price,
                        Status = detail.Status
                    })
                .ToListAsync();

            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                UserId = invoice.UserId,
                Date = invoice.Date,
                TotalAmount = invoice.TotalAmount,
                DiscountAmount = invoice.discount_amountt,
                FinalAmount = invoice.final_amount,
                PaymentMethod = invoice.PaymentMethod,
                ConsigneeName = invoice.ConsigneeName,
                ShippingAddress = invoice.ShippingAddress,
                ShippingPhone = invoice.ShippingPhone,
                VoucherId = invoice.VoucherId,
                PaymentStatus = invoice.payment_status,
                Status = invoice.Status,
                InvoiceDetails = invoiceDetailDtos
            };

            return Ok(invoiceDto);
        }


    }
}
