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

        public SearchBaseResponse<List<Transaction>> GetData(int? startAt, int? maxRecords, string sortBy, string sortOrder)
        {
            var apiTransactions = new ApiCallerTransactions("http://localhost:8080/");

            var transactionList = apiTransactions.SearchTransactions(new ChurchLibrary.Model.Base.Request.SearchLimiters()
            {
                start_at = startAt.HasValue ? startAt.Value : 1,
                max_records = maxRecords.HasValue ? maxRecords.Value : 0,
                sort_by = sortBy,
                sort_order = sortOrder
            });

            return transactionList;
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
