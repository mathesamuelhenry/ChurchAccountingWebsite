using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChurchWebSiteNetCore.Models;
using ChurchLibrary.Model.Base.Response;
using ChurchAccountingApiClient;
using ChurchLibrary.Model;
using ChurchWebSiteNetCore.Util;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ChurchWebSiteNetCore.Models.Config;

namespace ChurchWebSiteNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly APIUrl _apiUrl;

        public HomeController(IOptions<APIUrl> apiUrlCfg)
        {
            _apiUrl = apiUrlCfg.Value;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            ViewBag.Healthy = ApiCallUtil.IsApiHealthy(_apiUrl.SSChurch);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("userinfo")]
        [Authorize]
        public IActionResult UserInformation()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
