using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing.Interface
{
    public class NimbusSettings
    {
        public class Crypto
        {
            public RSAParameters RSAParams { get; set; }
            public byte[] CookieHMACKey { get; set; }
        }

        public bool IsDevelopment { get; set; }
        public bool IsDebug { get; set; }
        public string DatabaseConnectionString { get; set; }
        public Crypto Cryptography { get; set; }

    }
}
