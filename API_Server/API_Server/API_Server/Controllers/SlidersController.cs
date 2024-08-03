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
    public class SlidersController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public SlidersController(API_ServerContext context)
        {
            _context = context;
        }

        // GET: api/Sliders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Slider>>> GetSlider()
        {
            return await _context.Slider.ToListAsync();
        }

        // GET: api/Sliders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Slider>> GetSlider(int id)
        {
            var slider = await _context.Slider.FindAsync(id);

            if (slider == null)
            {
                return NotFound();
            }

            return slider;
        }

        // PUT: api/Sliders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSlider(int id, Slider slider)
        {
            if (id != slider.SlidersId)
            {
                return BadRequest();
            }

            _context.Entry(slider).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SliderExists(id))
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
        public async Task<ActionResult<Slider>> PostSlider(IFormFileCollection images, IFormFileCollection imagesLeft, IFormFileCollection imagesBottom)
        {
            try
            {
                // Process images
                await AddImages(images, "Image_Auto");
                await AddImages(imagesLeft, "Image_left");
                await AddImages(imagesBottom, "Image_bottom");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        private async Task AddImages(IFormFileCollection images, string name)
        {
            if (images == null || images.Count == 0)
            {
                // Nếu không có ảnh, không thực hiện gì
                return;
            }

            for (int i = 0; i < images.Count; i++)
            {
                string uniqueId = Guid.NewGuid().ToString().Substring(0, 5);

                var fileName = uniqueId + Path.GetExtension(images[i].FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\events", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await images[i].CopyToAsync(stream);
                }

               
                var slider = new Slider
                {
                    ImageName = fileName,
                    Name = name,
                    StartDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow,
                    Status = "Active"
                };

              
                _context.Slider.Add(slider);
            }

            await _context.SaveChangesAsync();
        }


        // DELETE: api/Sliders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlider(int id)
        {
            var slider = await _context.Slider.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            _context.Slider.Remove(slider);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SliderExists(int id)
        {
            return _context.Slider.Any(e => e.SlidersId == id);
        }
    }
}
