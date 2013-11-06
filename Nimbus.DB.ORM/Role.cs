using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Role  : Nimbus.Model.Role
    {        
        [PrimaryKey()]
        [References(typeof(User))]
        public override int UserId { get; set; }
        [PrimaryKey()]
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

    }
}
