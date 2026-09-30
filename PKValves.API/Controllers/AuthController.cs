using Microsoft.AspNetCore.Mvc;
using PKValves.API.Models;
using PKValves.API.Services;

namespace PKValves.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseAuthService _firebaseAuth;
        private readonly FirestoreService _firestore;

        public AuthController(
            FirebaseAuthService firebaseAuth,
            FirestoreService firestore)
        {
            _firebaseAuth = firebaseAuth;
            _firestore = firestore;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Full name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password is required."
                });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Password must contain at least 6 characters."
                });
            }

            AuthResponse authResult =
                await _firebaseAuth.RegisterAsync(
                    request.Email,
                    request.Password);

            if (!authResult.Success)
            {
                return BadRequest(authResult);
            }

            UserProfile user = new UserProfile
            {
                Uid = authResult.Uid,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow
            };

            await _firestore.CreateUserAsync(user);

            return Ok(new
            {
                success = true,
                message = "Account created successfully.",
                uid = authResult.Uid,
                email = authResult.Email,
                idToken = authResult.IdToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Email and password are required."
                });
            }

            AuthResponse result =
                await _firebaseAuth.LoginAsync(
                    request.Email,
                    request.Password);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                uid = result.Uid,
                email = result.Email,
                idToken = result.IdToken
            });
        }

        [HttpGet("user/{uid}")]
        public async Task<IActionResult> GetUser(
            string uid)
        {
            UserProfile? user =
                await _firestore.GetUserAsync(uid);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found."
                });
            }

            return Ok(user);
        }
    }
}