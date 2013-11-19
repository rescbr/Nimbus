using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class RoleTopic  : Nimbus.Model.RoleTopic
    {
        [PrimaryKey]
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

        [PrimaryKey]
        [References(typeof(Topic))]
        public override int TopicId { get; set; }

        [PrimaryKey]
        [References(typeof(User))]
        public override int UserId { get; set; }

    }
}
