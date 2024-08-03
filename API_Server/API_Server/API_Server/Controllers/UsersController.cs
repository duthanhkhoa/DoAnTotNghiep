using API_Server.Data;
using API_Server.Models;
using API_Server.ModelView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

		private readonly API_ServerContext _context;


		public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, API_ServerContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
			_context = context;
		}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUser()
        {
            // Retrieve all users with Status == true
            var users = await _userManager.Users.Where(u => u.Status == true).ToListAsync();

            // Process each user to check their roles
            var userDtos = new List<UserModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    userDtos.Add(new UserModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Address = user.Address,
                        CreatedDate = user.CreatedDate,
                        Status = user.Status,
                    });
                }
            }

            return Ok(userDtos);
        }



        // Lấy user theo id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var userModel = new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
            };

            return userModel;
        }

        //Cập nhật
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUser(string id, UserModel UserModel)
        {
            if (id != UserModel.Id)
            {
                return BadRequest();
            }

            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin người dùng từ userDto vào user
            user.UserName = UserModel.UserName;
            user.Email = UserModel.Email;
            user.PhoneNumber = UserModel.PhoneNumber;
            user.FullName = UserModel.FullName;
            user.Address = UserModel.Address;
            

            // Cập nhật trong context và lưu vào cơ sở dữ liệu
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        

        private bool UserExists(string id)
		{
			return _context.User.Any(e => e.Id == id);
		}

		// DELETE: api/User/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await _context.User.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			_context.User.Remove(user);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login([Bind("Username,Password")] LoginModel account)
		{
			var user = await _userManager.FindByNameAsync(account.Username);
			if (user != null && await _userManager.CheckPasswordAsync(user, account.Password))
			{
				var userRoles = await _userManager.GetRolesAsync(user);
				var userId = await _userManager.GetUserIdAsync(user);

                var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};

				foreach (var userRole in userRoles)
				{
					authClaims.Add(new Claim(ClaimTypes.Role, userRole));
				}

				var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:ValidIssuer"],
					audience: _configuration["JWT:ValidAudience"],
					expires: DateTime.Now.AddHours(3),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
					);

				return Ok(new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo,
					userRole = userRoles,
                    userId = userId
                });
			}
			return Unauthorized();
		}

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return BadRequest("UserName đã tồn tại!");
            }

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                return BadRequest("Email đã tồn tại!");
            }

            User user = new User()
            {
                FullName = model.Fullname,
                Address = model.Address,
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.Now,
                Status = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "User creation failed! Please check user details and try again." });
            }

            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return Ok(new { Message = "User created successfully!" });
        }


        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return BadRequest("UserName đã tồn tại!");
            }

            User user = new User()
            {
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.Fullname,
                Address = model.Address,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                CreatedDate = DateTime.Now,
                Status = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "User creation failed! Please check user details and try again." });
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return Ok();
        }


        [HttpPost("ChangePassword")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                return Ok("Password changed successfully");
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("filterStatus")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetByStatus(bool status)
        {
            var users = await _userManager.Users.Where(u => u.Status == status).ToListAsync();

            var userDtos = new List<UserModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    userDtos.Add(new UserModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Address = user.Address,
                        CreatedDate = user.CreatedDate,
                        Status = user.Status,
                    });
                }
            }

            return Ok(userDtos);
        }

        // Cập nhật trạng thái người dùng
        [HttpPut("updateStatus/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(string id, UserModel Model)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái người dùng
            user.Status = !Model.Status;

            // Cập nhật trong context và lưu vào cơ sở dữ liệu
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        [HttpGet("Total-Users")]
        public async Task<IActionResult> GetTotalUsers()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");

            int totalUsers = users.Count;

            return Ok(new { TotalUsers = totalUsers });
        }

        [HttpGet("recently-created-users")]
        public async Task<IActionResult> GetRecentlyCreatedUsers()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            var now = DateTime.UtcNow;
            var recentlyCreatedUsers = users.Where(user =>
                user.CreatedDate >= now.AddDays(-7) &&
                user.Status
            ).ToList();

            var userDtos = new List<UserModel>();

            foreach (var user in recentlyCreatedUsers)
            {
                //var roles = await _userManager.GetRolesAsync(user);
                //var isAdmin = roles.Contains("Admin");

                userDtos.Add(new UserModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Address = user.Address,
                    //IsAdmin = isAdmin,
                    CreatedDate = user.CreatedDate,
                    Status = user.Status,
                });
            }

            return Ok(userDtos);
        }


        [HttpPost]
        [Route("ChangePasswordUser")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Password change failed", errors });
            }

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
