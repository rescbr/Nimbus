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
            List<Channel> channels = new List<Channel>();

            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();

            double count = categoryApi.showAllCategory().Count();
            count = Math.Ceiling(count/10);
            
            var lstCat = new List<Category>();
            lstCat = categoryApi.showCategoryToPage();

            ViewBag.lstCat = lstCat.OrderBy(c => c.Name).ToList();
            ViewBag.totalCategory = Convert.ToInt32(count);

            return View("Category", channels);
        }
    }
}