using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Server.Data;
using API_Server.Models;
using System.Xml.Linq;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly API_ServerContext _context;
        private readonly IWebHostEnvironment _env;

        public NewsController(API_ServerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/News
        [HttpGet]
        public async Task<ActionResult<IEnumerable<News>>> GetNews()
        {
            var active = await _context.News.Where(p => p.Status != "Inactive").ToListAsync();

            return Ok(active);
        }

        // GET: api/News/5
        [HttpGet("{id}")]
        public async Task<ActionResult<News>> GetNews(int id)
        {
            var news = await _context.News.FindAsync(id);

            if (news == null)
            {
                return NotFound();
            }

            return news;
        }

        // PUT: api/News/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNews(int id, [FromForm] News news, IFormFileCollection images)
        {
            var existing = await _context.News.FirstOrDefaultAsync(p => p.Name == news.Name && p.Id != id);
            if (existing != null)
            {
                return BadRequest("Tên tin tức đã tồn tại!");
            }

            if (id != news.Id)
            {
                return BadRequest();
            }

            string imagesFolder = Path.Combine(_env.WebRootPath, "images", "news");
            string filePath = Path.Combine(imagesFolder, news.Image);

            if (!System.IO.File.Exists(filePath) && images.Count > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[0].FileName);
                news.Image = uniqueFileName;

                var firstImageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\news", uniqueFileName);
                using (var stream = System.IO.File.Create(firstImageFilePath))
                {
                    await images[0].CopyToAsync(stream);
                }
            }

            if (!System.IO.File.Exists(filePath) && images.Count == 0)
            {
                news.Image = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(news.Image);
            }

            _context.Entry(news).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost]
        public async Task<IActionResult> PostNews([FromForm] News news, IFormFileCollection images)
        {
            var existing = await _context.News.FirstOrDefaultAsync(p => p.Name == news.Name);
            if (existing != null)
            {
                return BadRequest("Tên tin tức đã tồn tại!");
            }

            try
            {
                if (images.Count > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[0].FileName);
                    news.Image = uniqueFileName;

                    var firstImageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\news", uniqueFileName);
                    using (var stream = System.IO.File.Create(firstImageFilePath))
                    {
                        await images[0].CopyToAsync(stream);
                    }
                }

                // Lưu các tệp tin ảnh còn lại với tên ngẫu nhiên
                //var uploadedFileNames = new List<string>();

                //List<Image> uploadedFiles = new List<Image>();
                //for (int i = 1; i < images.Count; i++)
                //{
                //    var fileName = Guid.NewGuid().ToString().Substring(0, 10) + Path.GetExtension(images[i].FileName);

                //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\book", fileName);
                //    using (var stream = System.IO.File.Create(filePath))
                //    {
                //        await images[i].CopyToAsync(stream);
                //    }

                //    //uploadedFileNames.Add(fileName);
                //    uploadedFiles.Add(new Image { ProductId = news.Id, Image_url = fileName });
                //}
                _context.News.Add(news);
                await _context.SaveChangesAsync();

                return Ok(news);
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        // DELETE: api/News/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<News>>> GetByStatus(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid status value. Please use 'active' or 'inactive'.");
            }

            IQueryable<News> query = _context.News;

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
        public async Task<IActionResult> UpdateStatus(int id, News Model)
        {
            var response = await _context.News.FindAsync(id);

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
