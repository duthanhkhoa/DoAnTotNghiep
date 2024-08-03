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
    public class CommentsController : ControllerBase
    {
        private readonly API_ServerContext _context;

        public CommentsController(API_ServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComment()
        {
            var comments = await _context.Comment.Include(c => c.User).ToListAsync();
            return Ok(comments);
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comment.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }

        // Thêm action trong CommentsController để lấy comment theo ProductId
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByProductId(string productId)
        {
            var comments = await _context.Comment.Where(c => c.ProductId == productId).ToListAsync();

            //if (comments == null || comments.Count == 0)
            //{
            //    return NoContent(); // hoặc bạn có thể trả về một thông báo rằng không có comment nào
            //}

            return comments;
        }
    }
}
