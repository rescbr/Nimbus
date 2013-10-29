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
            
            app.PostMapRequestHandler += SwapToForceSessionHandler;
            app.PostAcquireRequestState += SwapToOriginalHandler;
            app.PreRequestHandlerExecute += PreRequestAuthenticateUser;
            app.PostRequestHandlerExecute += RedirectToLoginPageIfNotAuthorized;
        }

        void SwapToForceSessionHandler(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            
            if (app.Context.Handler is IReadOnlySessionState || app.Context.Handler is IRequiresSessionState)
            {
                // no need to replace the current handler
                return;
            }

            // swap the current handler
            app.Context.Handler = new ForceSessionHandler(app.Context.Handler);
        }

        void SwapToOriginalHandler(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            ForceSessionHandler resourceHttpHandler = HttpContext.Current.Handler as ForceSessionHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            // -> at this point session state should be available
            Debug.Assert(app.Session != null, "it did not work :(");
        }

        void RedirectToLoginPageIfNotAuthorized(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            if (context.Response.StatusCode == 401)
            {
                //apenas faz o redirecionamento caso a request seja de um browser
                if (context.Request.AcceptTypes.Contains("text/html"))
                {
                    string originalUrl = context.Request.Url.PathAndQuery;
                    context.Response.Redirect(String.Format("/login?redirect={0}", Uri.EscapeDataString(originalUrl)));
                }
                //senão continua com o 401
            }
        }

        void PreRequestAuthenticateUser(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;

            if (context.Handler == null) return; //request não possui handler, página estática, ignorar auth

            //Lê Cookie
            var cookies = context.Request.Cookies;
            string sessionToken = cookies["nsc-session"].Value;

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
                            var sessionUser = (context.Session[Const.UserSession] as NimbusPrincipal);
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

    public class ForceSessionHandler : IHttpAsyncHandler, IRequiresSessionState
    {
        internal readonly IHttpHandler OriginalHandler;
        public ForceSessionHandler(IHttpHandler originalHandler)
        {
            OriginalHandler = originalHandler;
        }
        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException("ForceSessionHandler cannot process requests.");
        }

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            throw new InvalidOperationException("ForceSessionHandler cannot process requests.");
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
            throw new InvalidOperationException("ForceSessionHandler cannot process requests.");
        }
    }



}