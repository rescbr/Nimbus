using Nimbus.Plumbing;
using Nimbus.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{

    public class HomeController : NimbusApiController
    {
        [Authorize]
        public View Get()
        {
            var hm = new HomeModel()
            {
                CurrentUser = NimbusUser,
                CurrentOrganization = NimbusOrganization.Name

            };
            return new View("Home", hm);
        }
    }

    public class HomeModel
    {
        public NimbusUser CurrentUser { get; set; }
        public string CurrentOrganization { get; set; }
    }
}