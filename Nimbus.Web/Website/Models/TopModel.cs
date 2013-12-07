using Nimbus.Model.Bags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class TopModel
    {
        public List<TopicBag> Topics { get; set; }

        public List<ChannelBag> Channels { get; set; }
    }
}