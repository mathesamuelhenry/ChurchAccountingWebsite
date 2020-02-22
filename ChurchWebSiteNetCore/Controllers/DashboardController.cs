using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Church.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChurchWebSiteNetCore.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.AccountList = this.GetAccountList();
            ViewBag.TotalBalance = this.GetTotalBalance();

            return View();
        }

        public IActionResult Display()
        {
            return View();
        }

        public IActionResult Chart()
        {
            Random rnd = new Random();

            //list of countries  
            var lstModel = new List<SimpleReportViewModel>();
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "Brazil",
                Quantity = rnd.Next(10)
            });
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "USA",
                Quantity = rnd.Next(10)
            });
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "Portugal",
                Quantity = rnd.Next(10)
            });
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "Russia",
                Quantity = rnd.Next(10)
            });
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "Ireland",
                Quantity = rnd.Next(10)
            });
            lstModel.Add(new SimpleReportViewModel
            {
                DimensionOne = "Germany",
                Quantity = rnd.Next(10)
            });
            return View(lstModel);
        }

        #region Get List of accounts

        protected List<Account> GetAccountList()
        {
            var apiAccount = new Church.API.Client.ApiCallerAccount("http://localhost:448/");

            return apiAccount.GetAccounts();
        }

        protected decimal GetTotalBalance()
        {
            var apiAccount = new Church.API.Client.ApiCallerTransaction("http://localhost:448/");

            var balanceList = apiAccount.GetAccountBalance();

            return balanceList.Sum(x => x.Value);
        }

        #endregion
    }

    public class SimpleReportViewModel
    {
        public string DimensionOne { get; set; }
        public int Quantity { get; set; }
    }
}