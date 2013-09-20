using Nimbus.Plumbing;
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

        public static string SmallHmac(string stringToHmac)
        {
            HMACMD5 hmac = new HMACMD5(NimbusAppBus.Instance.Settings.Cryptography.CookieHMACKey.Take(8).ToArray());
            byte[] hash = hmac.ComputeHash(Encoding.Unicode.GetBytes(stringToHmac));

            return Convert.ToBase64String(hash);

        }
    }
}