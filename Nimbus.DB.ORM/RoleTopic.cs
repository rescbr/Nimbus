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
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

        [References(typeof(Topic))]
        public override int TopicId { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }

    }
}
