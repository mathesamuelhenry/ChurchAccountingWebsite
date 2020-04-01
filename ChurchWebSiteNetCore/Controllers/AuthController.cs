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
using ChurchWebSiteNetCore.Util;
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
            var RegisterModel = new RegisterModel();
            var SecurityQuestionsList = new List<SecurityQuestion>();
            string errorMessage = string.Empty;

            try
            {
                var apiCall = new ApiCallerSecurityQuestions(_apiUrl.SSAuth);
                SecurityQuestionsList = apiCall.GetAllSecurityQuestions();

                var SecurityQuestionModelList = from secQuestion in SecurityQuestionsList
                                        select new QuestionsModel() { Id = secQuestion.SecurityQuestionId.ToString(), Name = secQuestion.Question };

                SecurityQuestionModelList.ToList().Insert(0, new QuestionsModel()
                {
                    Id = "",
                    Name = "Please select a security question"
                });

                ViewBag.QuestionList = SecurityQuestionModelList.ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            ViewBag.ErrorMessage = errorMessage;

            return View(RegisterModel);
        }

        [Route("Register")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            string errorMessage = string.Empty;

            try
            {
                var apiSecQCall = new ApiCallerSecurityQuestions(_apiUrl.SSAuth);
                var SecurityQuestionsList = apiSecQCall.GetAllSecurityQuestions();
                var SecurityQuestionModelList = from secQuestion in SecurityQuestionsList
                                                select new QuestionsModel() { Id = secQuestion.SecurityQuestionId.ToString(), Name = secQuestion.Question };
                ViewBag.QuestionList = SecurityQuestionModelList.ToList();

                // Get Auth Group by name
                var apiCall = new ApiCallerAuthGroup(_apiUrl.SSAuth);
                AuthGroup authGroupResult = apiCall.GetAuthGroupByGroupName("NP");

                // Get Role by name
                var apiCallRole = new ApiCallerRole(_apiUrl.SSAuth);
                Role roleResult = apiCallRole.GetRoleByName("Admin");

                var userInfoObject = new AuthUser()
                {
                    AuthGroupId = authGroupResult.AuthGroupId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    LoginId = model.Email,
                    Password = model.Password,
                    Status = "A",
                    UserAdded = "Admin@SS",
                    UserRole = new List<UserRole>
                    {
                        new UserRole()
                        {
                            RoleId = roleResult.RoleId,
                            UserAdded = "Admin@SS"
                        }
                    },
                    UserSecurityQuestion = new List<UserSecurityQuestion>
                    { 
                        new UserSecurityQuestion()
                        {
                            SecurityQuestionId = model.Question1,
                            Answer = model.Answer1,
                            UserAdded = "Admin@SS"
                        },
                        new UserSecurityQuestion()
                        {
                            SecurityQuestionId = model.Question2,
                            Answer = model.Answer2,
                            UserAdded = "Admin@SS"
                        },
                        new UserSecurityQuestion()
                        {
                            SecurityQuestionId = model.Question3,
                            Answer = model.Answer3,
                            UserAdded = "Admin@SS"
                        }
                    }
                };
                
                // Register User
                var apiAuth = new ApiCallerAuthUser(_apiUrl.SSAuth);
                var userResult = apiAuth.RegisterUser(userInfoObject);

                // Add Organization
                var apiOrg = new ApiCallerOrganization(_apiUrl.SSChurch);
                var orgResult = apiOrg.PostAddOrganization(new Organization
                {
                     Name = model.OrganizationName,
                     IndustryId = model.IndustryId,
                     Phone = model.OrgPhone,
                     Email = model.OrgEmail,
                     UserAdded = "Admin@SS"
                });

                // Add User Org
                var apiUserOrg = new ApiCallerUserOrganization(_apiUrl.SSChurch);
                var userOrgResult = apiUserOrg.PostAddUserOrganization(new UserOrganization
                {
                    OrganizationId = orgResult.OrganizationId,
                    AuthUserId = userResult.AuthUserId,
                    UserAdded = "Admin@SS"
                });

                // Handle Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, model.Email),
                    new Claim(ClaimTypes.Name, string.Concat(model.FirstName, model.LastName)),
                    new Claim(ClaimTypes.Surname, model.LastName),
                    new Claim(ClaimTypes.GivenName, model.FirstName),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("OrganizationName", model.OrganizationName),
                    new Claim("OrganizationId", orgResult.OrganizationId.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            ViewBag.ErrorMessage = errorMessage;
            if (!string.IsNullOrEmpty(errorMessage))
                return View("Register");
            else
                return RedirectToAction("Display", "Dashboard");
        }

        [Route("RegisterOrganization")]
        public IActionResult RegisterOrganization()
        {
            
            var RegisterModel = new RegisterModel();
            var IndustryList = new List<Industry>();
            string errorMessage = string.Empty;

            try
            {
                var apiCall = new ApiCallerIndustry(_apiUrl.SSChurch);
                IndustryList = apiCall.GetAll();

                var IndustryModelList = from industry in IndustryList
                                    select new IndustryModel() { Id = industry.IndustryId, Name = industry.IndustryName };

                ViewBag.IndustryList = IndustryModelList.ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            ViewBag.ErrorMessage = errorMessage;

            return View(RegisterModel);
        }

        [Route("RegisterOrganization")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult RegisterOrganization(RegisterModel model)
        {
            string errorMessage = string.Empty;
            var IndustryList = new List<Industry>();

            try
            {
                var apiSecQCall = new ApiCallerSecurityQuestions(_apiUrl.SSAuth);
                var SecurityQuestionsList = apiSecQCall.GetAllSecurityQuestions();
                var SecurityQuestionModelList = from secQuestion in SecurityQuestionsList
                                                select new QuestionsModel() { Id = secQuestion.SecurityQuestionId.ToString(), Name = secQuestion.Question };

                SecurityQuestionModelList.ToList().Insert(0, new QuestionsModel()
                {
                    Id = "",
                    Name = "Please select a security question"
                });

                ViewBag.QuestionList = SecurityQuestionModelList.ToList();

                var apiCall = new ApiCallerIndustry(_apiUrl.SSChurch);
                IndustryList = apiCall.GetAll();
                var IndustryModelList = from industry in IndustryList
                                        select new IndustryModel() { Id = industry.IndustryId, Name = industry.IndustryName };
                ViewBag.IndustryList = IndustryModelList.ToList();

                if (model.IndustryId == 0)
                    throw new Exception("Industry must be selected");

                if (string.IsNullOrWhiteSpace(model.OrganizationName))
                    throw new Exception("Organization name is required");
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            ViewBag.ErrorMessage = errorMessage;
            if (!string.IsNullOrEmpty(errorMessage))
                return View("RegisterOrganization", model);
            else
                return View("Register", model);
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
                    var apiOrg = new ApiCallerUserOrganization(_apiUrl.SSChurch);

                    // TODO : Get Org ID and Name from Login Id/User ID

                    // Handle Claims
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Role, "admin"),
                        new Claim(ClaimTypes.NameIdentifier, userResult.LoginId),
                        new Claim(ClaimTypes.Name, string.Concat(userResult.FirstName, userResult.LastName)),
                        new Claim(ClaimTypes.Surname, userResult.LastName),
                        new Claim(ClaimTypes.GivenName, userResult.FirstName),
                        new Claim(ClaimTypes.Email, userResult.EmailId),
                        new Claim(ClaimTypes.Role, "Admin")
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