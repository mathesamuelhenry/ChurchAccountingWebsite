using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchAccountingApiClient;
using ChurchLibrary.Model;
using Microsoft.AspNetCore.Mvc;

namespace ChurchWebSiteNetCore.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Contributor contributor)
        {
            var apiContributor = new ApiCallerContributor("http://localhost:8080/");

            if (apiContributor.GetAllFullNames().Contains($"{contributor.LastName}, {contributor.FirstName}"))
            {

            }
            else
            {
                apiContributor.AddContributor(contributor);
            }

            return View("Add", contributor);
        }
    }
}