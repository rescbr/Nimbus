using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    [CompositeIndex("UserId", "TopicId", Unique = true)]
    public class UserLikeTopic: Nimbus.Model.UserLikeTopic
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Topic))]
        public override int TopicId { get; set; }

    }

}
