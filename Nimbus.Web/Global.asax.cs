using Mandrill;
using Nimbus.Plumbing;
using Nimbus.Web.Startup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Nimbus.Web
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            NimbusRouting.RegisterSignalR(RouteTable.Routes);
            NimbusRouting.RegisterWebAPIRoutes(GlobalConfiguration.Configuration);
            NimbusRouting.RegisterMVCRoutes(RouteTable.Routes);

            AreaRegistration.RegisterAllAreas();

            DatabaseStartup.CreateDatabaseIfNotThere();
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            MandrillApi mandril = new MandrillApi(NimbusConfig.MandrillToken);
            EmailMessage mensagem = new EmailMessage();
            List<EmailAddress> address = new List<EmailAddress>();

            var exception = Server.GetLastError();

            if (exception != null)
            {
                string exceptionMsg = exception.ToString();
                if (Context != null)
                {
                    exceptionMsg += "\n\n---- Contexto ----\n" +
                        "URL: " + Context.Request.RawUrl + "\n" +
                        "Usuário: " + (Context.User.Identity != null && Context.User.Identity.AuthenticationType == "NimbusUser" ?
                                            (Context.User.Identity as NimbusUser).Email : "não autenticado") + "\n" + 
                        "Method: " + Context.Request.HttpMethod + "\n" + 
                        "User-Agent: " + Context.Request.UserAgent + "\n" +
                        "User IP: " + Context.Request.UserHostAddress;
                }
                try
                {
                    mensagem.from_email = "bug@portalnimbus.com.br";
                    mensagem.from_name = "Nimbus Bug X9";
                    mensagem.subject = exception.Message;
                    mensagem.text = exceptionMsg;

                    address.Add(new EmailAddress("***REMOVED***"));
                    address.Add(new EmailAddress("***REMOVED***"));
                    mensagem.to = address;

                    var result = mandril.SendMessage(mensagem);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}