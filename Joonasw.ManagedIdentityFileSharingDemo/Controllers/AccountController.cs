using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Joonasw.ManagedIdentityFileSharingDemo.Controllers
{
    [Route("/Account")]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [AcceptVerbs("GET", "HEAD", Route = "Login")]
        public IActionResult LoginView()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost("Logout")]
        public IActionResult LogOut()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/Account/LoggedOut"
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [AcceptVerbs("GET", "HEAD", Route = "LoggedOut")]
        public IActionResult LoggedOut()
        {
            return View();
        }

        [AllowAnonymous]
        [AcceptVerbs("GET", "HEAD", Route = "AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
