﻿using Nimbus.Web.Startup;
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

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}