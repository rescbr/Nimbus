using Nimbus.Model.Bags;
using Nimbus.Model.ORM;
using Nimbus.Plumbing;
using Nimbus.Web.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class ChannelModel
    {
        public List<Tag> Tags { get; set; }

        public List<User> Moderators { get; set; }

        public NimbusUser CurrentUser { get; set; }
        public ChannelBag CurrentChannel { get; set; }
        
        public List<TopicBag> AllTopics { get; set; }

        public Topic NewTopic { get; set; }

        public List<MessageBag> Messages { get; set; }

        public List<CommentBag> Comments { get; set; }

        public List<string> RolesCurrentUser { get; set; }

        public List<User> CcMessageReceiver { get; set; }

        public List<Category> Category { get; set; }

        public NotificationWrapper ChannelNotifications { get; set; }
        
    }
}