using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using ChurchAccountingApiClient;
using ChurchLibrary.Model.Base.Response;
using Microsoft.AspNetCore.Mvc;
using ChurchLibrary.Model;
using X.PagedList;
using Church.API.Models;

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
            var apiTransaction = new Church.API.Client.ApiCallerTransaction("http://localhost:448/");

            var transaction = apiTransaction.GetTransaction(id);

            return View(transaction);
        }

        #region PagedData

        public IActionResult IndexNew(int page = 1, int pageSize = 10)
        {
            var transactions = GetPagedTransactionList(page, pageSize);

            ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");

            return View(transactions);
        }

        protected IPagedList<Church.API.Models.Contribution> GetPagedTransactionList(int? page, int pageSize)
        {
            // return a 404 if user browses to before the first page
            if (page.HasValue && page < 1)
                return null;

            // retrieve list from database/whereverand
            var listUnpaged = GetTransactionsFromTheDatabase();

            // page the list
            var listPaged = listUnpaged.ToPagedList(page ?? 1, pageSize);

            // return a 404 if user browses to pages beyond last page. special case first page if no items exist
            if (listPaged.PageNumber != 1 && page.HasValue && page > listPaged.PageCount)
                return null;

            return listPaged;
        }

        protected List<Church.API.Models.Contribution> GetTransactionsFromTheDatabase()
        {
            var apiTransaction = new Church.API.Client.ApiCallerTransaction("http://localhost:448/");

            var transactionList = apiTransaction.GetTransactions();

            return transactionList;
        }

        protected Dictionary<string, string> GetCVDList(string table_name, string column_name)
        {
            var apiCVD = new Church.API.Client.ApiCallerCVD("http://localhost:448/");

            var cvdList = apiCVD.GetColumnValueDescList(table_name, column_name);

            var cvdDictionaryList = cvdList.ToDictionary(x => x.Value, x => x.Description);

            return cvdDictionaryList;
        }

        #endregion
    }
}