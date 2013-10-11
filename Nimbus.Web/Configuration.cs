using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Nimbus.Web
{
    public static class NimbusConfig
    {
        public static byte[] CookieHMACKey
        {
            get
            {
                //mudar para CloudConfigurationManager
                return Convert.FromBase64String(WebConfigurationManager.AppSettings["CookieHMACKey"]);
            }
        }

        public static string DatabaseConnection
        {
            get
            {
                return WebConfigurationManager.AppSettings["DatabaseConnectionString"];
            }
        }
    }
}