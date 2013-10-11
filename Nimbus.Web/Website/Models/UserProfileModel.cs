using Nimbus.DB.Bags;
using Nimbus.DB.ORM;
using Nimbus.Plumbing;
using System.Collections.Generic;

namespace Nimbus.Web.Website.Models
{
    public class UserProfileModel
    {
        public UserBag User { get; set; }

        public NimbusUser CurrentUser {get;set;}

        public List<Channel> ChannelPaid { get; set; }

        public List<Channel> ChannelFollow { get; set; }

        public List<Channel> MyChannels { get; set; }

        public List<Channel> ReadLater { get; set; }
    }
}