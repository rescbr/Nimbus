using Nimbus.Model.Bags;
using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class CategoryController: NimbusController
    {
        public ActionResult Index()
        {
            List<ChannelBag> channels = new List<ChannelBag>();

            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();
            var lstCat = new List<Category>();
            lstCat = categoryApi.showAllCategory();

            ViewBag.lstCat = lstCat.OrderBy(c => c.Name).ToList();
            return View("Category", channels);
        }
    }
}