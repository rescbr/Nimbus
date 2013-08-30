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
            //Caso o WebAPI informe que é necessário autenticar...
            /*request.OnSendingHeaders(state =>
            {
                var resp = (IOwinResponse)state;

                if (resp.StatusCode == 401)
                    FailLoginRequest(ref resp); //TODO: Pegar path de retorno do login
            }, response);*/

            //Lê Cookie
            var cookies = context.Request.Cookies;
            string sessionToken = null;
            try
            {
                sessionToken = Uri.UnescapeDataString(cookies["nsc-session"]);
            }
            catch (Exception)
            {
                FailLoginRequest(ref response);
            }

            if (sessionToken != null)
            {
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
                        var identity = new NimbusUser() { 
                            AvatarUrl = "avatar.png",
                            Name = "Eusébio Testoso",
                            UserId = 1,
                            IsAuthenticated = true //sempre!
                        };
                        //request.User = new ClaimsPrincipal(identity);
                        request.User = (IPrincipal)(new NimbusPrincipal(identity));
                    }
                    else FailLoginRequest(ref response); //token velho
                }
                else FailLoginRequest(ref response); //token inválido
            }
            else FailLoginRequest(ref response); //token = null

            await Next.Invoke(context);

            //Mesma coisa que o OnSendingHeaders(?)
            if (context.Response.StatusCode == 401)
            {
                var resp = context.Response;
                FailLoginRequest(ref resp);
            }
        }

        private void FailLoginRequest(ref IOwinResponse response)
        {
            //TODO: Colocar endereço de retorno
            response.Redirect("/login");
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