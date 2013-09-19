using Nimbus.Web.API.Models.Channel;
using Nimbus.Web.API.Models.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public enum SearchType 
    {
        topic,
        category,
        channel,
        tag,
        all
    }

    public class SearchAPIModel
    {
        public List<AbstractChannelAPI> abstractChannel { get; set; }
        public List<AbstractTopicAPI> abstractTopic { get; set; }
    }
}