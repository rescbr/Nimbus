using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Nimbus.Plumbing;
using Owin.Types;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using WebApiContrib.Formatting.Html.Formatters;
using WebApiContrib.Formatting.Html.Configuration;

namespace Nimbus.Web
{
    
    using AppFunc = Func<IDictionary<string, object>, System.Threading.Tasks.Task>;
    using WebApiContrib.Formatting.Razor;
    using Nimbus.Web.Middleware;
    using System.Web.Http.Filters;
    using System.Reflection;
    
    
    public class NimbusOwinApp : INimbusOwinApp
    {      
        public void Configuration(Owin.IAppBuilder app)
        {

            app.UseErrorPage();
            app.Use(typeof(Middleware.Authentication));
            
            app.Properties["host.AppName"] = "Nimbus";

            //WebAPI
            HttpConfiguration webApiConfig = new HttpConfiguration();
            webApiConfig.Formatters.Add(new HtmlMediaTypeViewFormatter()); //adiciona Razor
            GlobalViews.DefaultViewLocator = new NimbusFastViewLocator();
            GlobalViews.DefaultViewParser = new RazorViewParser();
            


            webApiConfig.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            webApiConfig.Routes.MapHttpRoute(
                name: "Admin",
                routeTemplate: "admin/{controller}"
            );

            webApiConfig.Routes.MapHttpRoute(
                name: "NewAccount",
                routeTemplate: "newaccount",
                defaults: new { controller = "NewAccount" }
            );

            webApiConfig.Routes.MapHttpRoute(
                name: "Login",
                routeTemplate: "login",
                defaults: new { controller = "Login" }
            );

            webApiConfig.Routes.MapHttpRoute(
                name: "Home",
                routeTemplate: "",
                defaults: new { controller = "Home" }
            );

            app.UseWebApi(webApiConfig);

            app.UseStaticFiles(GetPhysicalSiteRootPath());
            

            //Owin.AppBuilderExtensions.Run(
            //app
                //.UseWebApi(webApiConfig)
                //.UseFunc(AutoDebugAttach)
                //.UseShowExceptions() //Gate.Middleware
                //.UseFunc(Thrower()) //Owin.Extensions
                //.UseFunc(Meh)
                //.UseFunc(UrlRewrite("/", "/index.html"))
                //.MapHubs("signalr", new HubConfiguration()
                //{
                //    EnableCrossDomain = true,
                //    EnableJavaScriptProxies = true,
                //    Resolver = new DefaultDependencyResolver(),
                //    EnableDetailedErrors = true
                //})
                //.UseStatic();
                //.Run(new HelloWorldResponder());
            //;
        }

        // <summary>
        // Used as the T in a "conversion" of a Task into a Task{T}
        // </summary>
        private struct AsyncVoid
        {
        }

        internal static string GetPhysicalSiteRootPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
                       .Replace("file:\\", string.Empty)
                       .Replace("\\bin", string.Empty)
                       .Replace("\\Debug", string.Empty)
                       .Replace("\\Release", string.Empty);
        }
    }
}