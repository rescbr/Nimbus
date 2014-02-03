using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public class ParticipationModel
    {
        public int CountChannelOwner { get; set; }

        public int CountChannelManager { get; set; }

        public int CountTopics { get; set; }

        public int CountComments { get; set; }

        public int FiftyFollowers { get; set; }

        public int HundredFollowers { get; set; }

    }
}