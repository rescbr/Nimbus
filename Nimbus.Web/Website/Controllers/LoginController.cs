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
using System.Web.Mvc;


namespace Nimbus.Web.Website.Controllers
{
    public class LoginController : NimbusWebController
    {
        // GET: /login
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(string redirect = null)
        {
            if (redirect != null && Uri.IsWellFormedUriString(redirect, UriKind.Relative))
            {
                return View("Login", new LoginModel()
                {
                    RedirectURL = redirect
                });
            }
            else
            {
                return View(new LoginModel());
            }
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(LoginModel login)
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
                    var loginCookie = new HttpCookie("nsc-session", authToken)
                    {
                        Expires = DateTime.Now.AddDays(EXPIRY_DAYS)
                    };
                    
                    //adiciona objeto do usuário logado à sessão
                    Session[Const.UserSession] = loggedInUser;

                    Response.Cookies.Add(loginCookie);
                    return Redirect(login.RedirectURL);

                }
                else
                {
                    //joga mensagem de erro
                    login.ErrorMessage = "Usuário ou senha inválidos.";
                }
            }

            login.Password = ""; //limpa a senha antes de enviar
            return View(login);
        }

        [HttpGet]
        [ActionName("Logout")]
        public ActionResult Logout()
        {
            var cookie = Request.Cookies["nsc-session"];
            cookie.Value = "";
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            return Redirect("/");
        }
    }
}