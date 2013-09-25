using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class VoteChannel : Nimbus.DB.VoteChannel
    {
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

    }
}
