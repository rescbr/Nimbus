using Nimbus.Model.Bags;
using Nimbus.Model.ORM;
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
                
        public CategoryBag Category { get; set; }

        public int NumFavorites { get; set; }
    }
}