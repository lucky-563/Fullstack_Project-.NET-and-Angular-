
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WalletApi.Models.DTOS;

namespace WalletApi.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

   

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
          
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                // Return a specific error message for the email already existing
                return BadRequest(new[] {
            new { code = "DuplicateEmail", description = "Email already exists" }
        });
            }
            var result = await _userManager.CreateAsync(user, model.Password);
           
            if (result.Succeeded)
                return Ok(new { Message = "User registered successfully" });

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized("Invalid username or password.");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid username or password.");
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Email = user.Email,
                UserName = userName,
                Token = token
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                 new Claim(ClaimTypes.Name, user.UserName),
                  new Claim(ClaimTypes.Email, user.Email)

        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyNameisyenugulausharaniiamaprojectengineer"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "BasicIdentityApi",
                audience: "BasicIdentityApiUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("usersList")]
        public async Task<IActionResult> GetUsernames()
        {
            // Fetch all users from the Identity table
            var users = _userManager.Users.ToList();

            // Extract only the usernames
            var usernames = users.Select(user => user.UserName).ToList();

            return Ok(usernames);
        }
    }

}
