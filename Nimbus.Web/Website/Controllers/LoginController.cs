using Nimbus.Plumbing;
using Nimbus.Web.API;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{
    public class LoginController : NimbusApiController
    {
        public View Get(string redirect = null)
        {
            if (redirect != null && Uri.IsWellFormedUriString(redirect, UriKind.Relative))
            {
                return new View("Login", new LoginModel()
                {
                    RedirectURL = redirect
                });
            }
            else
            {
                return new View("Login", new LoginModel());
            }
        }

        public HttpResponseMessage Post(LoginModel login)
        {
            const int EXPIRY_DAYS = 7;
            if (ModelState.IsValid)
            {
                DatabaseLogin dbLogin = new DatabaseLogin(DatabaseFactory);
                NimbusPrincipal loggedInUser;

                //verifica URL de redirecionamento
                if (String.IsNullOrWhiteSpace(login.RedirectURL) ||
                    !Uri.IsWellFormedUriString(login.RedirectURL, UriKind.Relative))
                {
                    login.RedirectURL = "/";
                }
                
                //faz login no banco
                if (dbLogin.Authenticate(login.Email, login.Password, out loggedInUser))
                {
                    //Usuário e senha corretos, criar token de autenticação
                    Guid token;

                    //Cria token com validade de 7 dias
                    string authToken = Token.GenerateToken(
                        new NSCInfo()
                        {
                            TokenGenerationDate = DateTime.Now.ToUniversalTime(),
                            TokenExpirationDate = DateTime.Now.AddDays(EXPIRY_DAYS).ToUniversalTime(),
                            UserId = (loggedInUser.Identity as NimbusUser).UserId
                        },
                        out token);

                    //Lembre-se de expirar o cookie também
                    var loginCookie = new CookieHeaderValue("nsc-session", authToken)
                    {
                        Expires = DateTimeOffset.Now.AddDays(EXPIRY_DAYS)
                    };

                    //Adiciona cookie de sessão ao cache
                    NimbusAppBus.Instance.Cache.SessionPrincipal.StoreAndReplicate(authToken, loggedInUser);

                    var response = Request.CreateResponse(System.Net.HttpStatusCode.Found);
                    response.Headers.Location = new Uri(login.RedirectURL, UriKind.Relative);
                    response.Headers.AddCookies(new CookieHeaderValue[] {
                        loginCookie
                    });
                    return response;

                }
                else
                {
                    //joga mensagem de erro
                    login.ErrorMessage = "LOCALIZAR: Usuário ou senha inválidos.";
                }
            }

            login.Password = ""; //limpa a senha antes de enviar
            return Request.CreateResponse<View>(new View("Login", login));
        }
    }
}