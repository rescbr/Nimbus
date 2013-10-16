using Microsoft.WindowsAzure;
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
                string setting;
                if(Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable)
                    setting = CloudConfigurationManager.GetSetting("CookieHMACKey");
                else
                    setting = WebConfigurationManager.AppSettings["CookieHMACKey"];
                return Convert.FromBase64String(setting);
            }
        }

        public static string DatabaseConnection
        {
            get
            {
                string setting;
                if (Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.IsAvailable)
                    setting = CloudConfigurationManager.GetSetting("DatabaseConnectionString");
                else
                    setting = WebConfigurationManager.AppSettings["DatabaseConnectionString"];
                return setting;
            }
        }

        public static string StorageAccount
        {
            get
            { 
                return CloudConfigurationManager.GetSetting("StorageAccount");
            }
        }
    }
}