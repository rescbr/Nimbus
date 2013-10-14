using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Nimbus.Plumbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Nimbus.Plumbing
{
    /// <summary>
    /// A classe NimbusStartup é responsável por subir o httpd e o zmq.
    /// </summary>
    public sealed class NimbusStartup
    {
        #region Startup Options
        public sealed class StartupOptions
        { 
            private short _httpPort;
            public short HttpPort
            {
                get { return _httpPort; }
                set { _httpPort = value; }
            }


            private short _zmqPort;
            public short ZmqPort
            {
                get { return _zmqPort; }
                set { _zmqPort = value; }
            }

            private ISimpleLog _initLog;
            public ISimpleLog InitLog
            {
                get { return _initLog; }
                set { _initLog = value; }
            }

            private string _nimbusWebAssemblyFile;
            public string NimbusWebAssemblyFile
            {
                get { return _nimbusWebAssemblyFile; }
                set { _nimbusWebAssemblyFile = value; }
            }

            private NimbusSettings _nimbusSettings;
            public NimbusSettings NimbusSettings
            {
                get { return _nimbusSettings; }
                set { _nimbusSettings = value; }
            }



        }
        #endregion

        public void Init(StartupOptions initOptions)
        {
            NimbusAppBus.Init(initOptions.NimbusSettings);
            StartWebApp(initOptions);
        }

        private void StartWebApp(StartupOptions initOptions)
        {

            initOptions.InitLog.Log("StartWebApp", "Initializing WebApp on port " + initOptions.HttpPort.ToString());

            var owinStartOptions = new StartOptions();
            var httpListener = typeof(Microsoft.Owin.Host.HttpListener.OwinServerFactory);
            owinStartOptions.ServerFactory = httpListener.Namespace; //"Microsoft.Owin.Host.HttpListener";
            owinStartOptions.Urls.Add("http://+:" + initOptions.HttpPort.ToString());
                
            initOptions.InitLog.Log("StartWebApp", "Trying to load N.Web from " + initOptions.NimbusWebAssemblyFile);

            INimbusOwinApp nimbusOwinApp =
                (INimbusOwinApp)Activator.CreateInstanceFrom
                (initOptions.NimbusWebAssemblyFile, "Nimbus.Web.NimbusOwinApp")
                .Unwrap();

            initOptions.InitLog.Log("StartWebApp", "Adding Nimbus.Web dir to Assembly search path...");
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(
                new DirectoryAssemblyLoader(initOptions.NimbusWebAssemblyFile, true)
                .LoadDelegate);

            initOptions.InitLog.Log("StartWebApp", "Creating context");
            var services = Microsoft.Owin.Hosting.Services.ServicesFactory.Create();
            IHostingEngine engine = (IHostingEngine)services.GetService(typeof(IHostingEngine));
            var context = new StartContext(owinStartOptions);
            context.Startup = new Action<Owin.IAppBuilder>
                ((bld) =>
                    nimbusOwinApp.Configuration(bld));

            initOptions.InitLog.Log("StartWebApp", "Taking off...");
            try
            {
                engine.Start(context);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.HResult == -2147467259)
                {
                    initOptions.InitLog.Log("HttpListenerException", "Run 'netsh http add urlacl url=http://+:9000/ user=DOMAIN\\user' as admin");
                }
                throw ex;
            }
            initOptions.InitLog.Log("StartWebApp", "WebApp initialized.");
        }
    }
}
