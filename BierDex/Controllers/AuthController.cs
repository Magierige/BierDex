using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Deze regel zorgt ervoor dat 'model.Email', 'model.Password' en 'model.RememberMe' werken
    public record EmailLoginRequest(string Email, string Password, bool RememberMe = false);
    public record ChangeUsernameRequest(string NewUsername);

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            //var isAuth = User?.Identity?.IsAuthenticated == true;

            //if (!isAuth)
            //    return Unauthorized(); // 401

            //return Ok(new
            //{
            //    isAuthenticated = true,
            //    name = User.Identity!.Name
            //});
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                isAuthenticated = true,
                name = user.UserName,
                roles = roles
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("email-login")]
        public async Task<IActionResult> EmailLogin([FromBody] EmailLoginRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
                return BadRequest("Email en wachtwoord zijn verplicht.");

            // Zoek de gebruiker op basis van email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized("Ongeldige inloggegevens.");

            // Gebruik de SignInManager om in te loggen met de gevonden Username
            // We gebruiken user.UserName omdat Identity intern matcht op de UserName kolom
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(new { message = "Ingelogd!" });
            }

            if (result.IsLockedOut)
                return StatusCode(403, "Account is tijdelijk geblokkeerd.");

            return Unauthorized("Ongeldige inloggegevens.");
        }

        [Authorize]
        [HttpPost("change-username")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.NewUsername))
                return BadRequest("Nieuwe gebruikersnaam is verplicht.");

            // Haal de huidige ingelogde gebruiker op
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("");

            // Update de gebruikersnaam
            var result = await _userManager.SetUserNameAsync(user, model.NewUsername);

            if (result.Succeeded)
            {
                // Update de beveiligingsstempel en ververs de login-cookie 
                // zodat de nieuwe naam direct in User.Identity.Name staat.
                await _signInManager.RefreshSignInAsync(user);

                return Ok(new { message = "Gebruikersnaam succesvol gewijzigd!", newUsername = user.UserName });
            }

            // Foutafhandeling (bijv. validatieregels van Identity)
            return BadRequest(result.Errors);
        }
    }
}