using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using ChurchAccountingApiClient;
using ChurchLibrary.Model.Base.Response;
using Microsoft.AspNetCore.Mvc;
using ChurchLibrary.Model;

namespace ChurchWebSiteNetCore.Controllers
{
    public class TransactionsController : Controller
    {
        public IActionResult Index(int? startAt, int? maxRecords, string sortBy, string sortOrder)
        {
            SearchBaseResponse<List<ChurchLibrary.Model.Transaction>> transactionList = null;

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

        public SearchBaseResponse<List<ChurchLibrary.Model.Transaction>> GetData(int? startAt, int? maxRecords, string sortBy, string sortOrder)
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

        public IActionResult Details(int id)
        {
            var apiTransaction = new ApiCallerTransactions("http://localhost:8080/");

            var transaction = apiTransaction.GetTransaction(id);

            return View(transaction);
        }
    }
}