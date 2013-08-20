using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API
{
    public class TestController : NimbusApiController
    {
        public string Get()
        {
            return "Nimbus: " + NimbusAppBus.Settings.DatabaseConnectionString;
        }
    }
}