using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JW_Web_Token.Data; // Add this using statement
using JW_Web_Token.Models;
using Microsoft.EntityFrameworkCore; // Add this using statement

namespace JW_Web_Token.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            // Creating password hash and salt
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Creating a new User object
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Adding user to the context and saving changes to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Returning the registered user
            return Ok(user);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            // Finding the user by username
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            // If user not found, return BadRequest
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Verifying the password
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong Password");
            }

            // Creating a JWT token for the authenticated user
            string token = CreateToken(user);

            // Returning the JWT token
            return Ok(token);
        }

        // Creating a JWT token for the user
        private string CreateToken(User user)
        {
            // Creating claims for the user (in this case, just the username)
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Getting the token key from configuration
            var tokenKey = _configuration.GetSection("AppSettings:Token").Value;

            // Checking if the token key is valid
            if (string.IsNullOrEmpty(tokenKey) || tokenKey.Length < 64)
            {
                throw new ApplicationException("Invalid or insufficient token key in configuration.");
            }

            // Creating a symmetric security key using the token key
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenKey));

            // Creating signing credentials using the security key and algorithm
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            // Creating a JWT token with claims, expiration, and signing credentials
            var token = new JwtSecurityToken(claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
            );

            // Writing the token to a string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Returning the JWT token
            return jwt;
        }

        // Creating a password hash and salt using HMACSHA512
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        // Verifying the password hash using HMACSHA512
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
