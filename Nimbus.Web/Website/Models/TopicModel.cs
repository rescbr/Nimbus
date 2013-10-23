using Nimbus.DB.Bags;
using Nimbus.DB.ORM;
using Nimbus.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class TopicModel
    {        
        public NimbusUser CurrentUser { get; set; }

        public ChannelBag CurrentChannel { get; set; }
        
        public Topic CurrentTopic { get; set; }
        
        public List<CommentBag> Comments { get; set; }

        public List<string> RolesCurrentUser { get; set; }
                
        public Category Category { get; set; }

        public int NumFavorites { get; set; }
    }
}