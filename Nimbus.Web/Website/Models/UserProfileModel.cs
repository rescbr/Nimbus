using Nimbus.Model.Bags;
using Nimbus.Model.ORM;
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

        public List<TopicBag> ReadLater { get; set; }

        public List<MessageBag> Messages { get; set; }

        public int CountMessageSend { get; set; }

        public List<Category> Categories { get; set; }

        public List<Channel> ChannelMannager { get; set; }
    }
}