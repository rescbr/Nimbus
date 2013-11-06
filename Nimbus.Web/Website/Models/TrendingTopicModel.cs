using Nimbus.DB.Bags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class TrendingTopicModel
    {
        public List<TopicBag> Topics { get; set; }
    }
}