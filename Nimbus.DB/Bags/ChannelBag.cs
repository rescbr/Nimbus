using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.Bags
{
    public class ChannelBag:Channel    
    {
        public string CountFollowers { get; set; }
        public string ParticipationChannel { get; set; }
        public bool isMember { get; set; }
        public string MessageAlert { get; set;}
    }
}
