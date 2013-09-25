using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class VoteChannel
    {
        public virtual int ChannelId { get; set; }


        public int Score { get; set; }
    }
}
