using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SkyQuery.AuthService.Domain.Entities.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkyQuery.AuthService.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            //return Ok(new { Authorization = Request.Headers["Authorization"].ToString() }); // FOR TESTING TO BE DELETED



            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User registered!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }

            var userRoles = await _userManager.GetRolesAsync(user); //Tilføjet til Roles/Claims

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // Tilføj rolle-claims
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role))); //Tilføjet til Roles/Claims

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),  // Time token is valid for (ASP.NET Identity Auto adds 5 minutes no matter what)
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "admin")] // Kun admin kan tildele roller
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignment model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound($"Bruger med email '{model.Email}' blev ikke fundet.");
            }
                
            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return BadRequest($"Rollen '{model.Role}' eksisterer ikke.");
            }
                
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok($"Rollen '{model.Role}' blev tildelt brugeren '{model.Email}'.");
        }

        [HttpPatch("remove-role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleAssignment model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound($"Bruger med email '{model.Email}' blev ikke fundet.");
            }

            var hasRole = await _userManager.IsInRoleAsync(user, model.Role);
            if (!hasRole)
            {
                return BadRequest($"Brugeren har ikke rollen '{model.Role}'.");
            }
                
            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok($"Rollen '{model.Role}' blev fjernet fra brugeren '{model.Email}'.");
        }

        [HttpPut("update-user")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound($"Bruger med ID '{model.UserId}' blev ikke fundet.");
            }

            if (!string.IsNullOrEmpty(model.NewEmail))
            {
                user.Email = model.NewEmail;
                user.UserName = model.NewEmail;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }

            return Ok("Bruger opdateret.");
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Bruger med ID '{userId}' blev ikke fundet.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Bruger blev slettet.");
        }

        [HttpGet("all-users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();

            var userList = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    Roles = roles.ToList()
                });
            }

            return Ok(userList);
        }
    }
}
