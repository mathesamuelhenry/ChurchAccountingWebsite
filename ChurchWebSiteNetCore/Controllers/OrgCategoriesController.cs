using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Church.API.Client;
using ChurchWebSiteNetCore.Models;
using ChurchWebSiteNetCore.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace ChurchWebSiteNetCore.Controllers
{
    public class OrgCategoriesController : Controller
    {
        private readonly APIUrl _apiUrl;
        private ApiCallerOrganizationCategory apiOrgCategory;

        public OrgCategoriesController(IOptions<APIUrl> apiUrlCfg)
        {
            _apiUrl = apiUrlCfg.Value;

            apiOrgCategory = new Church.API.Client.ApiCallerOrganizationCategory(_apiUrl.SSChurch);
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            ViewBag.OrgCategoryList = GetPagedOrgCategoryList(page, pageSize);

            return View();
        }

        protected IPagedList<Church.API.Models.OrganizationCategory> GetPagedOrgCategoryList(int? page, int pageSize)
        {
            // return a 404 if user browses to before the first page
            if (page.HasValue && page < 1)
                return null;

            // retrieve list from database/whereverand
            var listUnpaged = GetOrgCategoriesFromTheDatabase();

            // page the list
            var listPaged = listUnpaged.ToPagedList(page ?? 1, pageSize);

            // return a 404 if user browses to pages beyond last page. special case first page if no items exist
            if (listPaged.PageNumber != 1 && page.HasValue && page > listPaged.PageCount)
                return null;

            return listPaged;
        }

        protected List<Church.API.Models.OrganizationCategory> GetOrgCategoriesFromTheDatabase()
        {
            var apiOrgCategory = new Church.API.Client.ApiCallerOrganizationCategory(_apiUrl.SSChurch);

            var orgCategoryList = apiOrgCategory.GetCategoryListByOrganizationId(2);

            return orgCategoryList;
        }

        public IActionResult Create()
        {
            var model = new OrgCategory { };

            return PartialView("_AddEditOrgCategoryModalPartial", model);
        }

        [HttpPost]
        public IActionResult Create(OrgCategory model)
        {
            string errorMessage = string.Empty;

            if (ModelState.IsValid)
            {
                // TODO : Get Org Id from Claims
                var orgCatObj = new Church.API.Models.OrganizationCategory() { OrganizationId = 2, CategoryName = model.CategoryName, UserAdded = "smathe" };

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
        }

        public IActionResult Edit(int id)
        {
            var orgCategory = apiOrgCategory.GetOrganizationCategoryById(id);

            var model = new OrgCategory { OrganizationCategoryId = orgCategory.OrganizationCategoryId, OrganizationId = orgCategory.OrganizationId, CategoryName = orgCategory.CategoryName };

            return PartialView("_AddEditOrgCategoryModalPartial", model);
        }

        [HttpPost]
        public IActionResult Edit(OrgCategory model)
        {
            string errorMessage = string.Empty;

            if (ModelState.IsValid)
            {
                // TODO : Get Org Id from Claims
                // TODO : Get UserAdded from Claims
                var orgCategoryObj = new Church.API.Models.OrganizationCategory() { OrganizationCategoryId = model.OrganizationCategoryId, OrganizationId = 2, CategoryName = model.CategoryName, UserChanged = "smathe" };

                var apiContributors = new Church.API.Client.ApiCallerOrganizationCategory(_apiUrl.SSChurch);

                try
                {
                    apiContributors.PutUpdateOrganizationCategory(orgCategoryObj.OrganizationCategoryId, orgCategoryObj);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    ModelState.AddModelError("OrgCategoryError", errorMessage);
                }
            }

            ViewBag.ErrorMessage = errorMessage;

            return PartialView("_AddEditOrgCategoryModalPartial", model);
        }
    }
}