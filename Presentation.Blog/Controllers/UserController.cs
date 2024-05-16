using Application.Blog;

using Domain.Blog.dto;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using Presentation.Blog.utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Presentation.Blog.Controllers
{
    [ApiController]
    [Route("api/user/")]
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDBContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _hostingEnvironment;



        // Constructor to inject dependencies
        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserController> logger
            , IConfiguration configuration, IEmailService emailService, ApplicationDBContext dbContext,
            IWebHostEnvironment hostingEnvironment)
        {
            _emailService = emailService;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }
        // Method to return a server error response
        private IActionResult serverError(Exception ex)
        {
            return BadRequest(new { status = 500, message = "in User error is " + ex.Message });
        }


        [HttpGet("checkAuth")]
        [Authorize] // Allow authenticated users
        public IActionResult CheckAuth()
        {
            if (User.IsInRole("User") || User.IsInRole("Admin"))
            {
                // If the user is either a "User" or an "Admin"
                return Ok(new { status = 200, message = "User is authenticated" });
            }
            else
            {
                // If the user does not have either role
                return BadRequest(new { status = 400, message = "User is authenticated but does not have required role" });
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto registerUserDto, string roleName)
        {
            _logger.LogInformation("registring");
            try
            {
                // Check if registerUserDto or its image is null
                if (registerUserDto?.image == null) return BadRequest(new { status = 400, message = "image cannot be null" });
                
                if (!ModelState.IsValid) return BadRequest(ModelState);
                // Check if email already used
                var userByEmail = await _userManager.FindByEmailAsync(registerUserDto.email);
                if (userByEmail != null) return BadRequest(new { error = "Email already used." });

                // Check image size (maximum 3 MB)

                if (registerUserDto?.image.Length > 3 * 1024 * 1024) return BadRequest(new { status = 400, error = "Image size is more than 3 MB " });

                registerUserDto.imageName = await FileHelper.SaveImage(registerUserDto.image, _hostingEnvironment);


                // Check if the specified role exists
                var role = await _roleManager.FindByNameAsync(roleName);

                // Create the role if it doesn't exist
                if (role == null)
                {
                    role = new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),

                    };
                    var isCreated = await _roleManager.CreateAsync(role);

                    if (!isCreated.Succeeded)
                    {

                        return BadRequest(new { status = 400, error = "role create failed" });
                    }
                }

                var user = new User
                { UserName = registerUserDto.userName, Email = registerUserDto.email, imageName = registerUserDto.imageName ?? "" };

                //create new user  
                var result = await _userManager.CreateAsync(user, registerUserDto.password);
                if (result.Succeeded)
                {

                    bool sendDone = await sendConfirmationEmail(registerUserDto.email, user);

                    // Provide the role as user
                    if (sendDone)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);

                        return Ok(new { message = "please verify your mail" });

                    }

                }
                string error="";
                foreach(var r in result.Errors)
                {
                   error= r.Description;
                    break;
                }
                return BadRequest(new { error = error });
            }
            catch (Exception ex)
            {
                return serverError(ex);
            }
        }


        // send the confirmation mail for verification

        private async Task<bool> sendConfirmationEmail(string? email, User? user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string url = "https://localhost:7064/api/user";
            var confirmationLink = $"{url}/confirm-email?UserId={user.Id}&Token={token}";
            await _emailService.sendEmail(email, "Confirm Your Email", $" confirm your account  <a href='{confirmationLink}'>click me</a>;.", true);
            return true;
        }




        // Handles the confirmation of user email addresses via HTTP GET method.
        [HttpGet("confirm-email")]
        public async Task<string> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            // Check if userId or token is null
            if (userId == null || token == null)
            {
                return "Link expired";
            }
            // Check if user is not found
            else if (user == null)
            {
                return "User not found";
            }
            else
            {
                // Replace spaces in the token if any
                token = token.Replace(" ", "+");

                // Confirm the email using the user and token
                var result = await _userManager.ConfirmEmailAsync(user, token);

                // Return appropriate message based on confirmation result
                if (result.Succeeded)
                {
                    return "Thank you for confirming your email";
                }
                else
                {
                    return "Email not confirmed";
                }
            }
        }






        // Retrieves all users via HTTP GET method.
        [HttpGet("get-all")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                // Retrieve all users
                var users = await _userManager.Users.ToListAsync();

                // Return response with appropriate status and data
                return Ok(new
                {
                    status = users.Count > 0 ? 200 : 400,
                    data = users.Count > 0 ? users : new List<User>()
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }
        // Retrieves the count of non-admin users via HTTP GET method.
        [HttpGet("get-count")]
        public async Task<IActionResult> GetUsersCount()
        {
            try
            {
                // Find the admin role
                var adminRole = await _roleManager.FindByNameAsync("Admin");

                // Get count of users in the admin role
                var adminUsersCount = await _userManager.GetUsersInRoleAsync(adminRole.Name);

                // Get total users count
                var totalUsersCount = await _userManager.Users.CountAsync();

                // Calculate count of non-admin users
                var nonAdminUsersCount = totalUsersCount - adminUsersCount.Count;

                // Return response with non-admin users count
                return Ok(new { status = 200, data = nonAdminUsersCount });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }



        // Deletes a user by their ID via HTTP DELETE method.
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                // Find the user by ID
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(); // Return 404 if user not found

                // Retrieve blogs, comments, and reacts associated with the user
                var blogs = _dbContext.Blogs.Include(b => b.user).Where(b => b.user.Id == userId);
                var comments = _dbContext.Comments.Include(b => b.user).Where(c => c.user.Id == userId);
                var reacts = _dbContext.Reacts.Include(b => b.user).Where(r => r.user.Id == userId);

                // Delete associated blogs, comments, and reacts
                _dbContext.Blogs.RemoveRange(blogs);
                _dbContext.Comments.RemoveRange(comments);
                _dbContext.Reacts.RemoveRange(reacts);

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                // Delete the user
                IdentityResult identityResult = await _userManager.DeleteAsync(user);

                // Return appropriate response based on deletion result
                if (identityResult.Succeeded)
                {
                    return Ok(new { status = "200", message = "User deleted" });
                }
                else
                {
                    return BadRequest(new { status = "400", message = "Delete failed" });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }


        // Retrieves a user by their ID via HTTP GET method.
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                // Find the user by ID
                var user = await _userManager.FindByIdAsync(id);

                // Return user data if found, otherwise return an error message
                return user != null
                    ? Ok(new { data = user })
                    : BadRequest(new { error = "Cannot find user" });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }



        // Updates a user's information via HTTP PUT method.
        [HttpPut("update/{userID}")]
        public async Task<IActionResult> UpdateUser(string userID, UpdateUserDto model)
        {
            try
            {
                // Check model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Find the user by ID
                var user = await _userManager.FindByIdAsync(userID);
                if (user == null)
                {
                    return NotFound(new { error = "User not found." });
                }

                // Check if the provided password is valid
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return BadRequest(new { error = "Invalid password." });
                }

                // Update the user's properties
                user.Email = model.Email;
                user.UserName = model.Username;
                // Add more properties to update as needed

                // Perform the update operation
                var result = await _userManager.UpdateAsync(user);

                // Return appropriate response based on the update result
                return result.Succeeded
                    ? Ok(new { message = "User updated successfully." })
                    : BadRequest(new { message = result.Errors });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }




        // Initiates a request for password reset via HTTP POST method.
        [HttpPost("forgotRequest")]
        public async Task<IActionResult> ForgotPasswordRequest(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email.Trim());

            // Check if user is present and email is confirmed
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest(new { error = "User not found or email not confirmed." });
            }

            // Generate a random 5-digit token
            Random random = new Random();
            int token = random.Next(10000, 99999);

            // Construct the password reset link
            var link = Url.Action(nameof(ResetPassword), "User", new { token }, Request.Scheme);

            // Log the link for debugging
            _logger.LogInformation("---------------------------------");
            Console.WriteLine("Link in controller: " + link);
            _logger.LogInformation("---------------------------------");

            // Send password reset link via email
            await _emailService.sendPasswordResetLink(email, link);

            // Return success message
            return Ok(new { message = "We have sent you a password reset link." });
        }
        // Handles the password reset link clicked from email via HTTP GET method.
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int token)
        {
            // Create a dictionary to hold token data
            Dictionary<string, object> tokenData = new Dictionary<string, object>();

            // Add the token to the dictionary
            tokenData.Add("token", token);

            // Return the token data in the response
            return Ok(new { tokenData });
        }







        // Resets user password via HTTP POST method.
        [HttpPost("reset-passwords")]
        public async Task<IActionResult> ResetPasswords(ForgotPassword forgotPassword)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(forgotPassword.email.Trim());

            // Check if user is present and email is confirmed
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Ok(new { message = "User not found or email not confirmed." });
            }

            // Reset user password using the provided token
            var identityResult = await _userManager.ResetPasswordAsync(user, forgotPassword.token, forgotPassword.password);

            // Check if the password reset operation succeeded
            if (identityResult.Succeeded)
            {
                // Add any errors to model state
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                // Return model state
                return Ok(ModelState);
            }
            // Return success message if password has been changed
            return Ok(new { message = "Password has been changed." });
        }


        // Handles user login via HTTP POST method.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                // Check model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Find the user by email
                var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

                // Check if user exists
                if (user == null)
                {
                    return BadRequest(new { error = "User not found." });
                }

                // Check if user email is confirmed
                if (!user.EmailConfirmed)
                {
                    return BadRequest(new { error = "Please confirm your email." });
                }

                // Check if password matches
                bool isPasswordMatched = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);

                if (!isPasswordMatched)
                {
                    return BadRequest(new { error = "User password did not match." });
                }

                // Create authentication claims
                var authClaims = new List<Claim> {
            new Claim(ClaimTypes.Email, loginUserDto.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

                // Get user roles and add to claims
                IList<string> roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Generate JWT token
                var jwtSecurityToken = GenerateToken(authClaims);

                // Return token along with user information
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    role = roles.FirstOrDefault(),
                    name = user.UserName,
                    email = user.Email,
                    id = user.Id,
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a server error response
                return serverError(ex);
            }
        }


        // Generates JWT token based on provided claims.
        private JwtSecurityToken GenerateToken(List<Claim> claims)
        {
            try
            {
                // Retrieve JWT secret key from configuration
                SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                // Create and return JWT token
                return new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(24), // Token expiration time
                    claims: claims,
                    signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
                );

            }
            catch (Exception ex)
            {
                // Handle any exceptions and return null
                return null;
            }
        }



    }
}
