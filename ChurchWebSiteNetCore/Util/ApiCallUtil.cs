using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchWebSiteNetCore.Util
{
    public static class ApiCallUtil
    {
        #region Api Health

        public static bool IsApiHealthy()
        {
            var apiHealthCheck = new Church.API.Client.ApiCallerHealthCheck("http://localhost:448/");

            return apiHealthCheck.ApiHealthy();
        }

        #endregion
    }
}
