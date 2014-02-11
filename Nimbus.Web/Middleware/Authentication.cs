using Microsoft.Owin;
using Nimbus.Plumbing;
using Nimbus.Web.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace Nimbus.Web.Middleware
{
    /// <summary>
    /// Módulo IIS responsável pela autenticação do Nimbus
    /// </summary>
    public class Authentication : IHttpModule
    {
        void IHttpModule.Dispose() { }
        void IHttpModule.Init(HttpApplication app)
        {
            app.BeginRequest += app_BeginRequest;
            app.AuthorizeRequest += app_AuthenticateRequest;
            app.PreRequestHandlerExecute += app_PreRequestHandlerExecute;
            app.PostRequestHandlerExecute += RedirectToLoginPageIfNotAuthorized;

            app.EndRequest += app_EndRequest;
        }

        void app_EndRequest(object sender, EventArgs e)
        {
        }

        void app_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            //força sessão para requests webapi e owin (/api/ e /signalr/)
            //Resolve o bug criado pelo signalr que foi para resolver um bug.
            //Não fiquei bravo. Estou calmo. hehe =)
            if (app.Context.Request.Path.StartsWith("/api/") ||
                app.Context.Request.Path.StartsWith("/signalr/"))
            {   
                app.Context.Items[Const.Auth.PerformAuthLater] = false;
            }
            else
            {
                //caso seja outra rota, o mvc inicializa a sessão se necessário
                app.Context.Items[Const.Auth.PerformAuthLater] = true;
            }

            //if (app.Context.Request.QueryString["performance"] != null)
            //{
                var now = DateTime.UtcNow;
                app.Context.Items[Const.Auth.RequestPerformance] = now;
                app.Context.Response.AddHeader("X-Nimbus-Perf-StartedOn", now.ToString("o"));
            //}

        }

        /// <summary>
        /// Autenticação antes da pipeline chegar ao handler, para OWIN e WebApi.
        /// </summary>
        void app_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;

            if ((bool)app.Context.Items[Const.Auth.PerformAuthLater]) return;
            
            AuthenticateUser(sender, e);
            if (app.Context.Items[Const.Auth.RequestPerformance] != null)
            {
                var authtime = (DateTime.UtcNow -
                    (DateTime)(app.Context.Items[Const.Auth.RequestPerformance])).TotalMilliseconds;
                app.Context.Response.AddHeader("X-Nimbus-Perf-TimeAuthF", authtime.ToString());
            }
        }

        /// <summary>
        /// Autenticação quando o Handler (muito provavelmente MVC) for chamado, 
        /// permite que requests sem handler (estáticas) não sejam autenticadas.
        /// </summary>
        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            if (context.Handler == null) return; //request não possui handler, página estática, ignorar auth

            AuthenticateUser(sender, e);
            if (app.Context.Items[Const.Auth.RequestPerformance] != null)
            {
                var authtime = (DateTime.UtcNow -
                    (DateTime)(app.Context.Items[Const.Auth.RequestPerformance])).TotalMilliseconds;
                app.Context.Response.AddHeader("X-Nimbus-Perf-TimeAuthP", authtime.ToString());
            }
        }

        void RedirectToLoginPageIfNotAuthorized(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            if (context.Response.StatusCode == 401)
            {
                //apenas faz o redirecionamento caso a request seja de um browser
                if (context.Request.AcceptTypes != null && context.Request.AcceptTypes.Contains("text/html"))
                {
                    string originalUrl = context.Request.Url.PathAndQuery;
                    try
                    {
                        context.Response.Redirect(String.Format("/login?redirect={0}", Uri.EscapeDataString(originalUrl)));
                    }
                    catch { }
                }
                //senão continua com o 401
            }
        }

        void AuthenticateUser(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            //Lê Cookie
            string sessionToken = null;
            var cookies = context.Request.Cookies;
            if(cookies["nsc-session"] != null)
                sessionToken = cookies["nsc-session"].Value;

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
                        try
                        {
                            var sessionUser = new SessionManagement().GetNimbusPrincipal(info.UserId);
                            if (sessionUser != null)
                            {
                                context.User = sessionUser;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(String.Format("Nimbus: context.Session exception on request to {0}: {1}",
                                context.Request.Path, ex.Message));
                        } //sessao nao inicializada
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

        }

    }

}