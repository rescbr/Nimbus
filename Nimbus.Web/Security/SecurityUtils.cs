using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Nimbus.Web.Security
{
    public class SecurityUtils
    {
        public static CookieHeaderValue GenerateAuthCookie()
        {
            return null;
        }

        public static string SmallHmac(INimbusAppBus nimbusAppBus, string stringToHmac)
        {
            HMACMD5 hmac = new HMACMD5(nimbusAppBus.Settings.Cryptography.CookieHMACKey.Take(8).ToArray());
            byte[] hash = hmac.ComputeHash(Encoding.Unicode.GetBytes(stringToHmac));

            return Convert.ToBase64String(hash);

        }
    }
}