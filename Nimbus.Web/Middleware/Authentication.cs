using Microsoft.Owin;
using Nimbus.Plumbing.Interface;
using Nimbus.Web.Security;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Nimbus.Web.Middleware
{
    /// <summary>
    /// Middleware responsável pela autenticação do Nimbus
    /// </summary>
    public class Authentication : OwinMiddleware
    {


        private INimbusAppBus _nimbusAppBus;
        public Authentication(OwinMiddleware next, INimbusAppBus nimbusAppBus)
            : base(next)
        { _nimbusAppBus = nimbusAppBus; }

        //public override async Task Invoke(OwinRequest request, OwinResponse response)
        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.OnSendingHeaders((c) =>
            {
                var ctx = (IOwinContext)c;
                if (ctx.Response.StatusCode == 401)
                {
                    //apenas faz o redirecionamento caso a request seja de um browser
                    if (ctx.Request.Accept.Contains("text/html"))
                    {
                        string originalUrl = ctx.Request.Uri.PathAndQuery;
                        //string hmac = Security.SecurityUtils.SmallHmac(_nimbusAppBus, originalUrl); 
                        ctx.Response.Redirect(String.Format("/login?redirect={0}", Uri.EscapeDataString(originalUrl)));
                    }
                    //senão continua com o 401
                }
            }, context);

            //Lê Cookie
            var cookies = context.Request.Cookies;
            string sessionToken = cookies["nsc-session"];

            if (sessionToken != null)
            {
                sessionToken = Uri.UnescapeDataString(sessionToken);

                Guid tokenGuid;
                NSCInfo info;
                if (VerifyToken(sessionToken, out tokenGuid, out info))
                {
                    //Token é válido, continuar verificando se o usuário pode ser logado
                    if (info.TokenGenerationDate.AddDays(7.0) > DateTime.Now.ToUniversalTime())
                    {
                        //TODO!
                        //pega o NimbusUser
                        //algo como NimbusAppBus.Cache.SessionCache etc
                        var identity = new NimbusUser()
                        {
                            AvatarUrl = "avatar.png",
                            Name = "Eusébio Testoso",
                            UserId = 1,
                            IsAuthenticated = true //sempre!
                        };
                        //request.User = new ClaimsPrincipal(identity);
                        request.User = (IPrincipal)(new NimbusPrincipal(identity));
                    }
                    else { } //token velho
                }
                else { } //token inválido
            }
            else { } //token = null

            await Next.Invoke(context);


        }

        private string GenerateToken(int userId, out Guid tokenGuid)
        {
            NSCInfo info = new NSCInfo()
            {
                UserId = userId,
                TokenGenerationDate = DateTime.Now.ToUniversalTime(),
            };
            return Token.GenerateToken(_nimbusAppBus, info, out tokenGuid);
        }

        private bool VerifyToken(string token, out Guid tokenGuid, out NSCInfo info)
        {
            return Token.VerifyToken(_nimbusAppBus, token, out tokenGuid, out info);
        }
        
    }
}