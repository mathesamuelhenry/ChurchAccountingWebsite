using SSAuth.Client.ApiCall;
using SSAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Util
{
    public static class ApiCallUtil
    {
        #region Api Health

        public static bool IsApiHealthy(string apiUrl)
        {
            var apiHealthCheck = new Church.API.Client.ApiCallerHealthCheck(apiUrl);

            return apiHealthCheck.ApiHealthy();
        }

        #endregion

        public static List<SecurityQuestion> GetSecurityQuestions(string url)
        {
            var apiCallQuestion = new ApiCallerSecurityQuestions(url);
            return apiCallQuestion.GetAllSecurityQuestions();
        }
    }
}
