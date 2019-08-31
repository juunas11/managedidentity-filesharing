using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Joonasw.ManagedIdentityFileSharingDemo.Controllers
{
    [Route("/Account")]
    public class AccountController : Controller
    {
        [AcceptVerbs("GET", "HEAD", Route = "login")]
        public IActionResult LoginView()
        {
            return View();
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        }

        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/account/loggedout"
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
