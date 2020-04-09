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
using ChurchWebSiteNetCore.Util;
using Microsoft.AspNetCore.Mvc.Filters;
using ChurchWebSiteNetCore.Models.Config;
using Microsoft.Extensions.Options;

namespace ChurchWebSiteNetCore.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly APIUrl _apiUrl;

        public TransactionsController(IOptions<APIUrl> apiUrlCfg)
        {
            _apiUrl = apiUrlCfg.Value;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            ViewBag.Healthy = ApiCallUtil.IsApiHealthy(_apiUrl.SSChurch);
        }

        public IActionResult IndexNew(int? startAt, int? maxRecords, string sortBy, string sortOrder)
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
            ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");
            ViewBag.AccountList = this.GetAccountList();
            ViewBag.MemberFullNameList = this.GetMemberFullNameList();

            var apiTransaction = new Church.API.Client.ApiCallerTransaction("http://localhost:448/");

            var transaction = apiTransaction.GetTransaction(id);

            return View(transaction);
        }

        #region PagedData

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var transactions = GetPagedTransactionList(page, pageSize);

            ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");
            ViewBag.AccountList = this.GetAccountList();
            ViewBag.MemberFullNameList = this.GetMemberFullNameList();

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

        #region GetCVDList
        protected Dictionary<string, string> GetCVDList(string table_name, string column_name)
        {
            var apiCVD = new Church.API.Client.ApiCallerCVD("http://localhost:448/");

            var cvdList = apiCVD.GetColumnValueDescList(table_name, column_name);

            var cvdDictionaryList = cvdList.ToDictionary(x => x.Value, x => x.Description);

            return cvdDictionaryList;
        }
        #endregion

        #region GetAccountList
        protected List<string> GetAccountList()
        {
            var apiAccount = new Church.API.Client.ApiCallerAccount("http://localhost:448/");

            var accountList = apiAccount.GetAccounts();

            return accountList.Select(x => x.AccountName).ToList();
        }

        #endregion

        #region GetMemberFullNameList

        protected List<string> GetMemberFullNameList()
        {
            var apiMember = new Church.API.Client.ApiCallerMember("http://localhost:448/");

            return apiMember.GetAllFullNames();
        }

        #endregion

        #endregion
    }
}