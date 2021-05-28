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
using Church.API.Models.AppModel.Request.Transactions;
using Church.API.Models.AppModel.Response.Transactions;
using Church.API.Models.AppModel.Request;
using Microsoft.AspNetCore.Http;
using Church.API.Models.AppModel.Response;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChurchWebSiteNetCore.Controllers
{
    public class Item
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    };

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

        /*public IActionResult Index(int page = 1, int pageSize = 10)
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
        }*/

        public IActionResult IndexV1(int page = 1, int pageSize = 10)
        {
            var request = new SearchTransactionsRequest()
            {
                SearchParameters = new SearchParameters()
                {
                    MaxRecords = pageSize,
                    StartAt = page == 1 ? 0 : (page - 1) * pageSize,
                    SortOrder = "desc",
                    OrderBy = "cn.date_added"
                }
            };

            var transactions = GetPagedSearchTransactionListV1(page, pageSize, request);

            //ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            //ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            //ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");
            //ViewBag.AccountList = this.GetAccountList();
            //ViewBag.MemberFullNameList = this.GetMemberFullNameList();
            ViewBag.PageSize = pageSize;
            ViewBag.PageNumber = 1;

            SelectList AccountSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetAccountList()), "Id", "Name", request.AccountId);
            ViewBag.AccountSelectList = AccountSelectList;

            SelectList MemberFullNameSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetMemberFullNameList()), "Id", "Name", request.MemberPayeeId);
            ViewBag.MemberFullNameSelectList = MemberFullNameSelectList;

            SelectList CategorySelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "category")), "Id", "Name", request.Category);
            ViewBag.CategorySelectList = CategorySelectList;

            SelectList TransactionTypeSelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_type")), "Id", "Name", request.TransactionType);
            ViewBag.TransactionTypeSelectList = TransactionTypeSelectList;

            SelectList TransactionModeSelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_mode")), "Id", "Name", request.TransactionMode);
            ViewBag.TransactionModeSelectList = TransactionModeSelectList;

            Item[] pglist = new[] {
                        new Item { Id = 10, Name = "10" },
                        new Item { Id = 25, Name = "25" },
                        new Item { Id = 50, Name = "50" },
                        new Item { Id = 100, Name = "100" }
                    };

            SelectList selectList = new SelectList(pglist, "Id", "Name", pageSize);
            ViewBag.PageSizeList = selectList;

            return View(transactions);
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var request = new SearchTransactionsRequest()
            {
                SearchParameters = new SearchParameters()
                {
                    MaxRecords = int.MaxValue,
                    StartAt = page == 1 ? 0 : (page - 1) * pageSize,
                    SortOrder = "desc",
                    OrderBy = "cn.date_added"
                }
            };

            var transactions = GetPagedSearchTransactionList(page, pageSize, request);

            ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");
            ViewBag.AccountList = this.GetAccountList();
            ViewBag.MemberFullNameList = this.GetMemberFullNameList();
            ViewBag.PageSize = pageSize;

            return View(transactions);
        }

        protected IPagedList<Contribution> GetPagedTransactionList(int? page, int pageSize)
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

        [HttpPost]
        public IActionResult SearchTransaction(IFormCollection form)
        {
            var pageNumber = int.Parse(form["pageNumber"]);
            var pageSize = int.Parse(form["pageSize"]);

            var request = new SearchTransactionsRequest()
            {
                FromDate = string.IsNullOrEmpty(form["FromDate"]) ? (DateTime?)null : Convert.ToDateTime(form["FromDate"]),
                ToDate = string.IsNullOrEmpty(form["ToDate"]) ? (DateTime?)null : Convert.ToDateTime(form["ToDate"]),
                AccountId = int.TryParse(form["AccountId"], out _) ? int.Parse(form["AccountId"]) : (int?)null,
                TransactionType = int.TryParse(form["TransType"], out _) ? int.Parse(form["TransType"]) : (int?)null,
                TransactionMode = int.TryParse(form["TransMode"], out _) ? int.Parse(form["TransMode"]) : (int?)null,
                Category = int.TryParse(form["Category"], out _) ? int.Parse(form["Category"]) : (int?)null,
                MemberPayeeId = int.TryParse(form["MemberName"], out int memPayeeId) ? int.Parse(form["MemberName"]) : (int?)null,
                CheckNumber = form["CheckNumber"],
                TransactionName = form["TransactionName"],
                SearchParameters = new SearchParameters()
                {
                    MaxRecords = int.Parse(form["PageSize"]),
                    StartAt = pageSize * (pageNumber - 1),
                    SortOrder = "desc",
                    OrderBy = "cn.date_added"
                }
            };

            var transactions = GetPagedSearchTransactionListV1(1, int.Parse(form["PageSize"]), request);

            //ViewBag.CategoryList = this.GetCVDList("contribution", "category");
            //ViewBag.TransactionTypeList = this.GetCVDList("contribution", "transaction_type");
            //ViewBag.TransactionModeList = this.GetCVDList("contribution", "transaction_mode");
            //ViewBag.AccountList = this.GetAccountList();
            //ViewBag.MemberFullNameList = this.GetMemberFullNameList();
            ViewBag.PageSize = int.Parse(form["PageSize"]);
            ViewBag.PageNumber = int.Parse(form["PageNumber"]);

            SelectList AccountSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetAccountList()), "Id", "Name", request.AccountId);
            ViewBag.AccountSelectList = AccountSelectList;

            SelectList MemberFullNameSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetMemberFullNameList()), "Id", "Name", request.MemberPayeeId);
            ViewBag.MemberFullNameSelectList = MemberFullNameSelectList;

            SelectList CategorySelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "category")), "Id", "Name", request.Category);
            ViewBag.CategorySelectList = CategorySelectList;

            SelectList TransactionTypeSelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_type")), "Id", "Name", request.TransactionType);
            ViewBag.TransactionTypeSelectList = TransactionTypeSelectList;

            SelectList TransactionModeSelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_mode")), "Id", "Name", request.TransactionMode);
            ViewBag.TransactionModeSelectList = TransactionModeSelectList;

            Item[] pglist = new[] {
                        new Item { Id = 10, Name = "10" },
                        new Item { Id = 25, Name = "25" },
                        new Item { Id = 50, Name = "50" },
                        new Item { Id = 100, Name = "100" }
                    };

            SelectList selectList = new SelectList(pglist, "Id", "Name", pageSize);
            ViewBag.PageSizeList = selectList;

            return View("IndexV1", transactions);
        }

        protected GenericSearchResponse<List<SearchTransactionsResponse>> GetPagedSearchTransactionListV1(int? page, int pageSize, SearchTransactionsRequest request)
        {
            // return a 404 if user browses to before the first page
            if (page.HasValue && page < 1)
                return null;

            // retrieve list from database/whereverand
            var transactionList = SearchTransactionsFromTheDatabaseV1(request);

            return transactionList;
        }

        protected IPagedList<SearchTransactionsResponse> GetPagedSearchTransactionList(int? page, int pageSize, SearchTransactionsRequest request)
        {
            // return a 404 if user browses to before the first page
            if (page.HasValue && page < 1)
                return null;

            // retrieve list from database/whereverand
            var listUnpaged = SearchTransactionsFromTheDatabase(request);

            // page the list
            var listPaged = listUnpaged.ToPagedList(page ?? 1, pageSize);

            // return a 404 if user browses to pages beyond last page. special case first page if no items exist
            if (listPaged.PageNumber != 1 && page.HasValue && page > listPaged.PageCount)
                return null;

            return listPaged;
        }

        protected GenericSearchResponse<List<SearchTransactionsResponse>> SearchTransactionsFromTheDatabaseV1(SearchTransactionsRequest searchTransactionsRequest)
        {
            var apiTransaction = new Church.API.Client.ApiCallerTransaction(_apiUrl.SSChurch);

            var transactionList = apiTransaction.SearchTransactions(searchTransactionsRequest);

            return transactionList;
        }

        protected List<SearchTransactionsResponse> SearchTransactionsFromTheDatabase(SearchTransactionsRequest searchTransactionsRequest)
        {
            var apiTransaction = new Church.API.Client.ApiCallerTransaction(_apiUrl.SSChurch);

            var transactionList = apiTransaction.SearchTransactions(searchTransactionsRequest);

            return new List<SearchTransactionsResponse>();
        }

        protected List<Church.API.Models.Contribution> GetTransactionsFromTheDatabase()
        {
            var apiTransaction = new Church.API.Client.ApiCallerTransaction(_apiUrl.SSChurch);

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
        protected List<string> GetAccountList1()
        {
            var apiAccount = new Church.API.Client.ApiCallerAccount("http://localhost:448/");

            var accountList = apiAccount.GetAccounts();

            return accountList.Select(x => x.AccountName).ToList();
        }

        protected Dictionary<int, string> GetAccountList()
        {
            var apiAccount = new Church.API.Client.ApiCallerAccount("http://localhost:448/");

            var accountList = apiAccount.GetAccounts();

            var dict = accountList.ToDictionary(x => x.AccountId, x => x.AccountName);

            return dict;
        }

        #endregion

        #region GetMemberFullNameList

        protected Dictionary<int, string> GetMemberFullNameList()
        {
            var apiMember = new Church.API.Client.ApiCallerMember("http://localhost:448/");

            var memberList = apiMember.GetByOrganizationId(2);

            return memberList.ToDictionary(x => x.ContributorId, x => string.Concat(x.LastName, ", ", x.FirstName));
        }

        #endregion

        #endregion

        #region Create

        public IActionResult Create()
        {
            var model = new Models.Transaction() { };

            SelectList AccountSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetAccountList(), "Select One"), "Id", "Name", null);
            ViewBag.AccountSelectList = AccountSelectList;

            SelectList MemberFullNameSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetMemberFullNameList(), "Select One"), "Id", "Name", null);
            ViewBag.MemberFullNameSelectList = MemberFullNameSelectList;

            SelectList CategorySelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "category"), "Select One"), "Id", "Name", null);
            ViewBag.CategorySelectList = CategorySelectList;

            SelectList TransactionTypeList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_type"), null, false), "Id", "Name", null);
            ViewBag.TransactionTypeList = TransactionTypeList;

            SelectList TransactionModeList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_mode"), null, false), "Id", "Name", null);
            ViewBag.TransactionModeList = TransactionModeList;

            var typeList = new List<Item>() { new Item() { Id = 1, Name = "Member" }, new Item() { Id = 2, Name = "Company/General Name" } };
            var TypeList = new SelectList(typeList, "Id", "Name", null);
            ViewBag.TypeList = TypeList;

            return PartialView("_AddEditTransactionModalPartial", model);
        }

        [HttpPost]
        public IActionResult Create(Models.Transaction model)
        {
            string errorMessage = string.Empty;

            SelectList AccountSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetAccountList(), "Select One"), "Id", "Name", null);
            ViewBag.AccountSelectList = AccountSelectList;

            SelectList MemberFullNameSelectList = new SelectList(AppUtil.GetItemList<int, string>(this.GetMemberFullNameList(), "Select One"), "Id", "Name", null);
            ViewBag.MemberFullNameSelectList = MemberFullNameSelectList;

            SelectList CategorySelectList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "category"), "Select One"), "Id", "Name", null);
            ViewBag.CategorySelectList = CategorySelectList;

            SelectList TransactionTypeList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_type"), null, false), "Id", "Name", null);
            ViewBag.TransactionTypeList = TransactionTypeList;

            SelectList TransactionModeList = new SelectList(AppUtil.GetItemList<string, string>(this.GetCVDList("contribution", "transaction_mode"), null, false), "Id", "Name", null);
            ViewBag.TransactionModeList = TransactionModeList;

            var typeList = new List<Item>() { new Item() { Id = 1, Name = "Member Name" }, new Item() { Id = 2, Name = "Company/General Name" } };
            var TypeList = new SelectList(typeList, "Id", "Name", null);
            ViewBag.TypeList = TypeList;

            if (ModelState.IsValid)
            {
                var addTransactionObj = new Church.API.Models.Contribution() {
                    AccountId = model.AccountId,
                    ContributorId = model.MemberId,
                    ContributionName = model.TransactionName,
                    Category = model.Category,
                    TransactionType = model.TransactionType,
                    TransactionMode = model.TransactionMode,
                    CheckNumber = model.CheckNumber,
                    Amount = model.Amount,
                    TransactionDate = model.TransactionDate,
                    Note = model.Note
                };

                var apiContribution = new Church.API.Client.ApiCallerTransaction(_apiUrl.SSChurch);

                try
                {
                    apiContribution.PostAddTransaction(addTransactionObj);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    ModelState.AddModelError("TransactionError", errorMessage);
                }
            }

            ViewBag.ErrorMessage = errorMessage;

            return PartialView("_AddEditTransactionModalPartial", model);
        }

        /*[HttpPost]
        public IActionResult Create(Models.Transaction model)
        {
            string errorMessage = string.Empty;

            if (ModelState.IsValid)
            {
                // TODO : Get Org Id from Claims
                var orgCatObj = new Church.API.Models.Contribution() { AccountId = model.AccountId, ContributorId = model.MemberId, ContributionName = model.TransactionName, Category = model.Category, CheckNumber = model.CheckNumber, Amount = model.Amount, TransactionMode = model.TransactionMode, Note = model.Note, TransactionDate = model.TransactionDate, TransactionType = model.TransactionType };

                var apiOrgCategory = new Church.API.Client.ApiCallerOrganizationCategory(_apiUrl.SSChurch);

                try
                {
                    apiOrgCategory.PostAddOrganizationCategory(orgCatObj);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    ModelState.AddModelError("OrgCategoryError", errorMessage);
                }
            }

            ViewBag.ErrorMessage = errorMessage;

            return PartialView("_AddEditOrgCategoryModalPartial", model);
        }*/

        #endregion
    }
}