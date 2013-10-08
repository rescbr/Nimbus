using Nimbus.DB.Bags;
using Nimbus.DB.ORM;
using Nimbus.Plumbing;
using System.Collections.Generic;

namespace Nimbus.Web.Website.Models
{
    public class UserProfileModel
    {
        public UserBag user { get; set; }

        public NimbusUser CurrentUser {get;set;}

        public List<Channel> Channel { get; set; }
    }
}