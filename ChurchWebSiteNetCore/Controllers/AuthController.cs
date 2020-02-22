using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Church.API.Client;
using Church.API.Models;
using Church.API.Models.AppModel.Request;
using Church.API.Models.AppModel.Request.User;
using ChurchWebSiteNetCore.Models.Auth;
using ChurchWebSiteNetCore.Models.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SSAuth.Client.ApiCall;
using SSAuth.Models;
using SSAuth.Models.AppModels.Request.AuthUser;

namespace ChurchWebSiteNetCore.Controllers
{
    public class AuthController : Controller
    {
        private readonly APIUrl _apiUrl;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, IOptions<APIUrl> apiUrlCfg)
        {
            _configuration = configuration;
            _apiUrl = apiUrlCfg.Value;
        }

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
            AuthGroup authGroupResult = null;

            try
            {
                var apiCall = new ApiCallerAuthGroup(_apiUrl.SSAuth);
                authGroupResult = apiCall.GetAuthGroupByGroupName("NP");
            }
            catch (Exception ex)
            {

            }

            var userInfoObject = new AuthUser()
            {
                AuthGroupId = authGroupResult.AuthGroupId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                LoginId = model.Email,
                Password = model.Password,
                Status = "A",
                UserAdded = "msamuelhenry@gmail.com",
                UserRole = null,
                UserSecurityQuestion = null
            };

            try
            {
                var apiAuth = new ApiCallerAuthUser(_apiUrl.SSAuth);
                var userResult = apiAuth.RegisterUser(userInfoObject);
            }
            catch (Exception ex)
            {
                
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Email),
                new Claim(ClaimTypes.Name, string.Concat(model.FirstName, model.LastName)),
                new Claim(ClaimTypes.Surname, model.LastName),
                new Claim(ClaimTypes.GivenName, model.FirstName),
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

        [Route("logoutlink")]
        public async Task<IActionResult> LogOutLink()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(SignIn signInRequest)
        {
            string errorMessage = string.Empty;

            try
            {
                if (signInRequest != null)
                {
                    var apiAuth = new ApiCallerAuthUser(_apiUrl.SSAuth);
                    var userResult = apiAuth.AuthenticateUser(signInRequest);


                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, signInRequest.LoginId),
                    new Claim(ClaimTypes.Role, "admin"),
                    }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (string.IsNullOrEmpty(errorMessage))
                return RedirectToAction("Display", "Dashboard");
            else
            {
                ViewBag.SignInError = errorMessage;
                return View("~/Views/Home/Index.cshtml");
            }
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