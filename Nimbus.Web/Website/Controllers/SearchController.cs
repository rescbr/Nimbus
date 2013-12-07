using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class SearchController:NimbusWebController
    {

        public ActionResult Index(string text, int filter)
        {
            var searchApi = ClonedContextInstance<API.Controllers.SearchController>();            

             var search = new SearchModel();
             search.Text = text;
             if (filter == 0) //todos
             {
                 search.ItensFound = searchApi.SearchAll(text);
                 search.FieldType = 0;
             }
             else if (filter == 1)//channel
             {
                 search.ItensFound = searchApi.SearchChannel(text);
                 search.FieldType = 1;
             }
             else if (filter == 2)//topic
             {
                 search.ItensFound = searchApi.SearchTopic(text);
                 search.FieldType = 2;
             }
             else if (filter == 3)//user
             {
                 search.ItensFound = searchApi.SearchUser(text);
                 search.FieldType = 3;
             }

            return View("SearchContent", search);
        }

    }
}