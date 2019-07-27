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

namespace ChurchWebSiteNetCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(int? startAt, int? maxRecords, string sortBy, string sortOrder)
        {
            //var apiContributors = new ApiCallerContributor("http://localhost:8080/");

            //var contributorList = apiContributors.GetAllFullNames();

            //apiContributors.UpdateContributor(new Contributor(21, "SAMUEL1", "MATHE", null, DateTime.Now, null)); 


            SearchBaseResponse<List<ChurchLibrary.Model.Transaction>> transactionList = null;

            if (!maxRecords.HasValue)
            {
                transactionList = this.GetData(1, 10, "cn.date_added", "desc");
            }
            else
            {
                transactionList = this.GetData(startAt, maxRecords, sortBy, sortOrder);
            }

            return View(transactionList);
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
