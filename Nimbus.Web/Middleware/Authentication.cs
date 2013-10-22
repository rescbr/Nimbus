using Microsoft.Owin;
using Nimbus.Plumbing;
using Nimbus.Web.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Nimbus.Web.Middleware
{
    /// <summary>
    /// Middleware responsável pela autenticação do Nimbus
    /// </summary>
    public class Authentication : OwinMiddleware
    {
        public Authentication(OwinMiddleware next) : base(next) { }
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
                sessionToken = Uri.UnescapeDataString(sessionToken.Replace(' ', '+'));

                Guid tokenGuid;
                NSCInfo info;
                if (Token.VerifyToken(sessionToken, out tokenGuid, out info))
                {
                    //Token é válido, continuar verificando se o usuário pode ser logado
                    //if (info.TokenGenerationDate.AddDays(7.0) > DateTime.Now.ToUniversalTime())
                    if (info.TokenExpirationDate.ToUniversalTime() > DateTime.Now.ToUniversalTime())
                    {
                        //tenta pegar do cache de sessão
                        var sessionUser = (System.Web.HttpContext.Current
                            .Session[Const.UserSession] as NimbusPrincipal);
                        if (sessionUser != null)
                        {
                            request.User = sessionUser;
                        }
                    }
                    else //token velho
                    {
                    }
                }
                else //token inválido
                {
                }
            }
            else { } //token = null
            

            await Next.Invoke(context);


        }
    }
}