using Microsoft.Owin;
using Nimbus.Plumbing;
using Nimbus.Web.Middleware;
using Owin;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;

[assembly: OwinStartupAttribute(typeof(Nimbus.Web.NimbusOwinApp))]
namespace Nimbus.Web
{
    public class NimbusOwinApp
    {
        public void Configuration(Owin.IAppBuilder app)
        {
            

            //app.Use(typeof(Middleware.Authentication));
            app.Properties["host.AppName"] = "Nimbus";

            app.MapSignalR();
        }
    }
}