using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [AllowAnonymous]
    public class NimbusController:NimbusWebController
    {
        [HttpGet]
        public ActionResult Faq()
        {
            return View("faq"); 
        }

        [HttpGet]
        public ActionResult FaleConosco()
        {
            return View("faleconosco");
        }

        [HttpGet]
        public ActionResult QuemSomos()
        {
            return View("quemsomos");
        }

        [HttpGet]
        public ActionResult Novidades()
        {
            return View("novidades");
        }

        [HttpGet]
        public ActionResult Termosdeuso()
        {
            return View("termosdeuso");
        }


    }
}