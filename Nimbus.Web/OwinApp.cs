using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Nimbus.Plumbing.Interface;
using Owin.Types;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace Nimbus.Web
{
    
    using AppFunc = Func<IDictionary<string, object>, System.Threading.Tasks.Task>;
    
    public class NimbusOwinApp : INimbusOwinApp
    {
        private INimbusAppBus _nimbusAppBus;
        public INimbusAppBus NimbusAppBus
        {
            get { return _nimbusAppBus; }
        }
        
        public void Configuration(INimbusAppBus nimbusAppBus, IAppBuilder app)
        {
            _nimbusAppBus = nimbusAppBus;

            app.Use(typeof(Middleware.Authentication), _nimbusAppBus);
            
            app.Properties["host.AppName"] = "Nimbus";

            //WebAPI
            HttpConfiguration webApiConfig = new HttpConfiguration();
            webApiConfig.Properties["NimbusAppBus"] = nimbusAppBus;
            webApiConfig.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            webApiConfig.Routes.MapHttpRoute(
                name: "Admin",
                routeTemplate: "admin/{controller}"
            );


            app.UseWebApi(webApiConfig);
            
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

        /// <summary>
        /// Método para tratar o auto-attach do Visual Studio
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        AppFunc AutoDebugAttach(AppFunc next)
        {
            return env =>
            {
                Console.WriteLine("AutoDebugAttach!");
                var req = new OwinRequest(env);
                //Request DEBUG /debugattach.aspx
                if (!req.Path.Equals("/debugattach.aspx", StringComparison.InvariantCultureIgnoreCase)
                    && !req.Method.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
                {
                    return next(env);
                }
                Console.WriteLine("AutoDebugAttach2");
                //Se não estiver em modo debug e modo dev, continua para o handler de 404 responder.
                if (!(NimbusAppBus.Settings.IsDebug && NimbusAppBus.Settings.IsDevelopment))
                {
                    return next(env);
                }
                Console.WriteLine("AutoDebugAttach3");
                string cmd = req.GetHeader("Command");
                if (cmd == "start-debug")
                {
                    Dictionary<string, string> debugParams;
                    using (StreamReader sr = new StreamReader(req.Body))
                    {
                        string p = sr.ReadToEnd();
                        debugParams = p.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(part => part.Split('='))
                                       .ToDictionary(split => split[0], split => split[1]);
                    }

                    NimbusAppBus.NimbusDebugAutoAttach.DebugAutoAttach(debugParams["DebugSessionID"]);
                }
                Console.WriteLine("AutoDebugAttach4");
                var res = new OwinResponse(env) { ContentType = "text/plain" };
                res.Write("OK");
                Console.WriteLine("AutoDebugAttach5");
                return new TaskCompletionSource<AsyncVoid>().Task;
            };

        }

        // <summary>
        // Used as the T in a "conversion" of a Task into a Task{T}
        // </summary>
        private struct AsyncVoid
        {
        }
    }
}