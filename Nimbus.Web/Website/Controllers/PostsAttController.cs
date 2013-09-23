using Nimbus.Web.API;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{
    public class PostsAttController : NimbusApiController
    {
        public View Get(string redirect = null)
        {
            return new View("PostsAtt", new LoginModel());
        }

    }
}