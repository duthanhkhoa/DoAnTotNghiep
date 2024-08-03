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

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVoucherDetailsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public ProductVoucherDetailsController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/ProductVoucherDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVoucherDetail>>> GetProductVoucherDetail()
        {
            return await _context.ProductVoucherDetail.ToListAsync();
        }

        // GET: api/ProductVoucherDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ProductVoucherDetail>>> GetProductVoucherDetailsByVoucherId(string id)
        {
            // Tìm danh sách ProductVoucherDetail theo ProductVoucherId
            var productVoucherDetails = await _context.ProductVoucherDetail
                                                       .Where(pvd => pvd.ProductVoucherId == id)
                                                       .ToListAsync();

            return Ok(productVoucherDetails);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductVoucherDetail(int id, ProductVoucherDetail productVoucherDetail)
        {
            if (id != productVoucherDetail.Id)
            {
                return BadRequest();
            }

            _context.Entry(productVoucherDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductVoucherDetailExists(id))
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

        // POST: api/ProductVoucherDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //public async Task<ActionResult<ProductVoucherDetail>> PostProductVoucherDetail(ProductVoucherDetail productVoucherDetail)
        //public async Task<ActionResult<ProductVoucherDetail>> PostProductVoucherDetail(object json)
        //{
        //    return Ok();
        //    //_context.ProductVoucherDetail.Add(productVoucherDetail);
        //    //await _context.SaveChangesAsync();

        //    //return CreatedAtAction("GetProductVoucherDetail", new { id = productVoucherDetail.Id }, productVoucherDetail);
        //}

        //[HttpPost]
        //public async Task<ActionResult<ProductVoucherDetail>> PostProductVoucherDetail(object json)
        //{
        //    try
        //    {
        //        var jsonObject = JObject.Parse(json.ToString());

        //        int productVoucherId = (int)jsonObject["productVoucherId"];
        //        JArray voucherIds = JArray.Parse(jsonObject["voucherId"].ToString());

        //        foreach (var voucherId in voucherIds)
        //        {
        //            var productVoucherDetail = new ProductVoucherDetail
        //            {
        //                ProductVoucherId = productVoucherId, 
                       
        //            };

        //            _context.ProductVoucherDetail.Add(productVoucherDetail);
        //            await _context.SaveChangesAsync();
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("An error occurred while processing the request.");
        //    }
        //}



        // DELETE: api/ProductVoucherDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductVoucherDetail(int id)
        {
            var productVoucherDetail = await _context.ProductVoucherDetail.FindAsync(id);
            if (productVoucherDetail == null)
            {
                return NotFound();
            }

            _context.ProductVoucherDetail.Remove(productVoucherDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductVoucherDetailExists(int id)
        {
            return _context.ProductVoucherDetail.Any(e => e.Id == id);
        }
    }
}
