using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API
{
    public class TestController : ApiController
    {
        public string Get()
        {
            INimbusAppBus appbus = Configuration.Properties["NimbusAppBus"] as INimbusAppBus;
            return "Nimbus: " + appbus.Settings.DatabaseConnectionString;
        }
    }
}