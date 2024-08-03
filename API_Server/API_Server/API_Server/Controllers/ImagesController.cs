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
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly API_ServerContext _context;

        public ImagesController(API_ServerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImage()
        {
            return await _context.Image.ToListAsync();
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(string id)
        {
            var images = await _context.Image.Where(c => c.ProductId == id).ToListAsync();

            if (images == null)
            {
                return NotFound();
            }

            return Ok(images);
        }

        // PUT: api/Images/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImage(int id, Image image)
        {
            if (id != image.Id)
            {
                return BadRequest();
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
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

        // POST: api/Images
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Image>> PostImage(Image image)
        //{
        //    _context.Image.Add(image);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetImage", new { id = image.Id }, image);
        //}

        
        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromBody] string[] imageNames)
        {
            try
            {
                // Get the path to the images folder
                string imagesFolder = Path.Combine(_env.WebRootPath, "images", "book");
                string imagesFolderNews = Path.Combine(_env.WebRootPath, "images", "news");

                // Iterate through each image name and delete the corresponding file
                foreach (string imageName in imageNames)
                {
                    string filePath = Path.Combine(imagesFolder, imageName);
                    string filePathNews = Path.Combine(imagesFolderNews, imageName);

                    // Check if the file exists before attempting to delete it
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    if (System.IO.File.Exists(filePathNews))
                    {
                        System.IO.File.Delete(filePathNews);
                    }

                    var imagesToRemove = _context.Image.Where(i => i.Image_url == imageName);
                    if (imagesToRemove != null)
                    {
                        _context.Image.RemoveRange(imagesToRemove);
                    }

                }

                await _context.SaveChangesAsync();

                return Ok("Images deleted successfully.");
            }
            catch (Exception ex)
            {
                // Log any errors and return a 500 Internal Server Error response
                Console.WriteLine($"Error deleting images: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting images.");
            }
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteImages(string productId)
        {
            try
            {
                if (productId != null)
                {
                    var imagesToDelete = await _context.Image.Where(img => img.ProductId == productId).ToListAsync();
                    _context.Image.RemoveRange(imagesToDelete);

                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }

        private bool ImageExists(int id)
        {
            return _context.Image.Any(e => e.Id == id);
        }
    }
}
