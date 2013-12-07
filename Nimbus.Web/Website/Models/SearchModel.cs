using Nimbus.Model.Bags;
using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class SearchModel
    {
        public List<SearchBag> ItensFound { get; set; }
    }
}