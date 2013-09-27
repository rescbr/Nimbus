using Nimbus.Plumbing;
using Nimbus.Web.Middleware;
using Owin;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using WebApiContrib.Formatting.Html.Configuration;
using WebApiContrib.Formatting.Html.Formatters;
using WebApiContrib.Formatting.Razor;

namespace Nimbus.Web
{
    public class NimbusOwinApp : INimbusOwinApp
    {
        private void RegisterWebApis()
        {
            _webApiConfig.Routes.MapHttpRoute(
                name: "ApiNamespace",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { 
                    @namespace = "api", //use @ antes da propriedade pq namespace é keyword.
                    id = RouteParameter.Optional
                }
            );

            _webApiConfig.Routes.MapHttpRoute(
                name: "AdminNamespace",
                routeTemplate: "admin/{controller}", 
                defaults: new { 
                    @namespace = "admin", //use @ antes da propriedade pq namespace é keyword.
                }
            );

            _webApiConfig.Routes.MapHttpRoute(
                name: "WebsiteNamespace",
                routeTemplate: "{controller}",
                defaults: new
                {
                    @namespace = "website", //use @ antes da propriedade pq namespace é keyword.
                }
            );

            _webApiConfig.Routes.MapHttpRoute(
                name: "Home",
                routeTemplate: "",
                defaults: new {
                    @namespace = "website",
                    controller = "Home"
                }
            );
        }

        

        HttpConfiguration _webApiConfig;
        public void Configuration(Owin.IAppBuilder app)
        {
            
            DatabaseStartup.CreateDatabaseIfNotThere();

            app.UseErrorPage();
            app.Use(typeof(Middleware.Authentication));
            
            app.Properties["host.AppName"] = "Nimbus";

            //WebAPI
            _webApiConfig = new HttpConfiguration();
            _webApiConfig.Formatters.Add(new HtmlMediaTypeViewFormatter()); //adiciona Razor
            GlobalViews.DefaultViewLocator = new NimbusFastViewLocator();
            GlobalViews.DefaultViewParser = new RazorViewParser();
            _webApiConfig.Services.Replace(typeof(IHttpControllerSelector), 
                new NamespaceHttpControllerSelector(_webApiConfig));
            
            RegisterWebApis();
            app.UseWebApi(_webApiConfig);

            app.UseStaticFiles(GetPhysicalSiteRootPath());
            

            //Owin.AppBuilderExtensions.Run(
            //app
                //.UseWebApi(_webApiConfig)
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