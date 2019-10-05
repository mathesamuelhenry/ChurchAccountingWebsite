using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchAccountingApiClient;
using ChurchLibrary.Model;
using Microsoft.AspNetCore.Mvc;
using Church.API.Models;
using X.PagedList;
using System.Net;

namespace ChurchWebSiteNetCore.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            ViewBag.Members = GetPagedMemberList(page, pageSize);

            return View();
        }

        protected IPagedList<Church.API.Models.Contributor> GetPagedMemberList(int? page, int pageSize)
        {
            // return a 404 if user browses to before the first page
            if (page.HasValue && page < 1)
                return null;

            // retrieve list from database/whereverand
            var listUnpaged = GetMembersFromTheDatabase();

            // page the list
            var listPaged = listUnpaged.ToPagedList(page ?? 1, pageSize);

            // return a 404 if user browses to pages beyond last page. special case first page if no items exist
            if (listPaged.PageNumber != 1 && page.HasValue && page > listPaged.PageCount)
                return null;

            return listPaged;
        }

        protected List<Church.API.Models.Contributor> GetMembersFromTheDatabase()
        {
            var apiContributors = new Church.API.Client.ApiCallerMember("http://localhost:448/");

            var transactionList = apiContributors.GetMembers();

            return transactionList;
        }

        public IActionResult Details(int id)
        {
            var member = this.GetMemberById(id);

            return View(member);
        }

        public IActionResult Delete(int id)
        {
            var result = this.DeleteMember(id, out string errorMsg);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                ViewBag.MemberErrorMsg = errorMsg;
            }

            return Json(new { Message = errorMsg });
        }


        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }


        /*
         * Model binding - passes in request to contributor
         */
        [HttpPost]
        public IActionResult Add(ChurchLibrary.Model.Contributor contributor)
        {
            var apiContributor = new ApiCallerContributor("http://localhost:8080/");

            if (apiContributor.GetAllFullNames().Contains($"{contributor.LastName}, {contributor.FirstName}"))
            {
                ModelState.AddModelError("Error", "Member already exists");
            }
            else
            {
                try
                {
                    apiContributor.AddContributor(contributor);
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("Error", ex.Message);
                }
            }

            return View("Add", contributor);
        }

        #region GetMemberFullNameList

        protected Church.API.Models.Contributor GetMemberById(int id = 3333)
        {
            var apiMember = new Church.API.Client.ApiCallerMember("http://localhost:448/");

            return apiMember.GetMemberById(id);
        }

        #endregion

        protected Church.API.Models.Contributor DeleteMember(int id, out string errorMsg)
        {
            errorMsg = string.Empty;
            Church.API.Models.Contributor member = null;

            var apiMember = new Church.API.Client.ApiCallerMember("http://localhost:448/");
            try
            {
                member = apiMember.DeleteMember(id);
            }
            catch(Exception ex)
            {
                errorMsg = ex.Message;
            }

            return member;
        }
    }
}