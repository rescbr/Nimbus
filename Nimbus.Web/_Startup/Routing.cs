using Nimbus.Web.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nimbus.Web.Startup
{
    public static class NimbusRouting
    {
        public static void RegisterSignalR(RouteCollection routes)
        {
            //SignalR é registrado em OwinApp.
        }

        public static void RegisterWebAPIRoutes(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Services.Replace(typeof(IHttpControllerSelector),
                new NamespaceHttpControllerSelector(httpConfiguration));

            httpConfiguration.Routes.MapHttpRoute(
                name: "ApiWithActionAndId",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new
                {
                    @namespace = "api",
                }
            );

            httpConfiguration.Routes.MapHttpRoute(
                name: "ApiWithAction",
                routeTemplate: "api/{controller}/{action}",
                defaults: new
                {
                    @namespace = "api",
                }
            );
            
            httpConfiguration.Routes.MapHttpRoute(
                name: "ApiNamespace",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    @namespace = "api", //use @ antes da propriedade pq namespace é keyword.
                    id = RouteParameter.Optional,
                    action =  "DefaultAction"
                }
            );
            
        }

        public static void RegisterMVCRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "WebsiteWithActionAndID",
                url: "{controller}/{action}/{id}"
            );

            routes.MapRoute(
                name: "WebsiteWithAction",
                url: "{controller}/{action}"
            );

            routes.MapRoute(
                name: "DefaultWithoutAction",
                url: "{controller}/{id}",
                defaults: new { controller = "UserProfile", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DefaultWithoutActionAndId",
                url: "{controller}",
                defaults: new { controller = "UserProfile", action = "Index" }
            );
        }

    }
}