using Microsoft.Owin;
using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nimbus.Web.Middleware
{
    public class Authentication : OwinMiddleware
    {
        private const string NSC_VERSION = "nsc:1:";


        private INimbusAppBus _nimbusAppBus;
        public Authentication(OwinMiddleware next, INimbusAppBus nimbusAppBus) : base(next)
        { _nimbusAppBus = nimbusAppBus; }

        public override async Task Invoke(OwinRequest request, OwinResponse response)
        {
            //Caso o WebAPI informe que é necessário autenticar...
            request.OnSendingHeaders(state =>
            {
                var resp = (OwinResponse)state;

                if (resp.StatusCode == 401)
                    resp.SetHeader("WWW-Authenticate", "Basic");
            }, response);

            
            //Lê Cookie
            var header = request.GetHeader("Authorization");

            if (!String.IsNullOrWhiteSpace(header))
            {
                var authHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(
                                          Convert.FromBase64String(
                                                authHeader.Parameter));
                    var parts = parameter.Split(':');

                    string userName = parts[0];
                    string password = parts[1];

                    if (userName == password) // Just a dumb check
                    {
                        var identity = new Nimbus.Plumbing.Interface.NimbusUser();
                        request.User = new ClaimsPrincipal(identity);
                    }
                }
            }

            await Next.Invoke(request, response);
        }

        public string GenerateToken(int userId, out Guid tokenGuid)
        {
            

            Guid token = Guid.NewGuid();
            NSCTokenInfo info = new NSCTokenInfo()
            {
                UserId = userId,
                TokenGenerationDate = DateTime.Now.ToUniversalTime(),
            };

            string b64info;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, info);
                b64info = Convert.ToBase64String(ms.ToArray());
            }
            
            string b64token = Convert.ToBase64String(token.ToByteArray());


            StringBuilder sb = new StringBuilder();
            sb.Append(NSC_VERSION);

            tokenGuid = token;
            return "";
        }

        public class NSCTokenInfo
        {
            public int UserId { get; set; }
            public DateTime TokenGenerationDate { get; set; }
        }
    }
}