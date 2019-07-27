using ChurchAccountingApiClient;
using ChurchLibrary.Model;
using ChurchLibrary.Model.Base.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChurchAccountingMVCDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int? startAt, int? maxRecords, string sortBy, string sortOrder)
        {
            SearchBaseResponse<List<Transaction>> transactionList = null;

            if (!maxRecords.HasValue)
            {
                transactionList = this.GetData(1, 50, "cn.date_added", "desc");
            }
            else
            {
                transactionList = this.GetData(startAt, maxRecords, sortBy, sortOrder);
            }

            return View(transactionList);
        }

        public ActionResult Page(int startAt, int maxRecords, string sortBy, string sortOrder)
        {
            var transactionList = this.GetData(startAt, maxRecords, sortBy, sortOrder);

            return RedirectToAction("Index", transactionList);
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Pagination()
        {
            ViewBag.Message = "Your contact page.";

            return RedirectToAction("Contact");
        }
    }
}