using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class ChannelUser : Nimbus.Model.ChannelUser
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }
        
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }
        
    }
}
