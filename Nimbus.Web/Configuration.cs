﻿using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Nimbus.Web
{
    public static class NimbusConfig
    {
        private static byte[] _cookieHMACKey = null;
        public static byte[] CookieHMACKey
        {
            get
            {
                if (_cookieHMACKey == null)
                    _cookieHMACKey = Convert.FromBase64String(GetSetting("CookieHMACKey"));
                return _cookieHMACKey;
            }
        }

        private static string _databaseConnection = null;
        public static string DatabaseConnection
        {
            get
            {
                if (_databaseConnection == null)
                    _databaseConnection = GetSetting("DatabaseConnectionString");

                return _databaseConnection;
            }
        }

        private static string _storageAccount = null;
        public static string StorageAccount
        {
            get
            {
                if (_storageAccount == null)
                    _storageAccount = GetSetting("StorageAccount");
                return _storageAccount;
            }
        }

        private static byte[] _generalHMACKey = null;
        public static byte[] GeneralHMACKey
        {
            get
            {
                if (_generalHMACKey == null)
                    _generalHMACKey = Convert.FromBase64String(GetSetting("GeneralHMACKey"));

                return _generalHMACKey;
            }
        }

        private static string GetSetting(string key)
        {
            if (RoleEnvironment.IsAvailable)
                return CloudConfigurationManager.GetSetting(key);
            else
                return WebConfigurationManager.AppSettings[key];
        }
    }
}