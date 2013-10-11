﻿using Nimbus.Web.API;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;


namespace Nimbus.Web.Website.Controllers
{
    public class PostsListaController : NimbusWebController
    {
        public ActionResult Get(string redirect = null)
        {
            return View("PostsLista", null);
        }

    }
}