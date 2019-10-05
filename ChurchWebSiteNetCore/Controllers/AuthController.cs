using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Church.API.Client;
using Church.API.Models;
using Church.API.Models.AppModel.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchWebSiteNetCore.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult SignIn()
        {
            return View();
        }

        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        [Route("Register")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var userInfoObject = new Users()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                Status = "A",
                UserAdded = "msamuelhenry@gmail.com",
                UserOrganization = new List<UserOrganization>
                {
                    new UserOrganization
                    {
                        OrganizationId = 3
                    }
                }
            };

            try
            {
                var apiAuth = new ApiCallerAuth("http://localhost:448/");
                var userResult = apiAuth.RegisterUser(userInfoObject);
            }
            catch (Exception ex)
            {
                
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Email),
                new Claim(ClaimTypes.Name, string.Concat(model.FirstName, model.LastName)),
                new Claim(ClaimTypes.Email, model.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            return RedirectToAction("UserInformation", "Home");
        }

        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction(nameof(SignIn));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "admin"),
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                principal);

            return RedirectToAction(nameof(SignIn));
        }

        [Authorize(Policy = "MustBeAdmin")]
        public IActionResult Manage() => View();

        [Route("Auth/Accessdenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}