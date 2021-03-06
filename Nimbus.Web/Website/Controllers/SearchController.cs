﻿using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class SearchController : NimbusWebController
    {

        public ActionResult Index(string text = null, int filter = -1)
        {
            if (string.IsNullOrEmpty(text) || filter < 0 || filter > 3)
            {
                var mdl = new SearchModel();
                mdl.ItensFound = null;
                return View("SearchContent", new SearchModel());
            }
            else
            {
                var searchApi = ClonedContextInstance<API.Controllers.SearchController>();
                var search = new SearchModel();

                search.Text = text;
                search.FieldType = filter;

                if (filter == 0) //todos
                {
                    search.ItensFound = searchApi.SearchAll(text);
                }
                else if (filter == 1)//channel
                {
                    search.ItensFound = searchApi.SearchChannel(text);
                }
                else if (filter == 2)//topic
                {
                    search.ItensFound = searchApi.SearchTopic(text);
                }
                else if (filter == 3)//user
                {
                    search.ItensFound = searchApi.SearchUser(text);
                }

                return View("SearchContent", search);
            }
        }
    }
}