using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.Bags
{
    public class ChannelBag:Channel    
    {
        public string countFollowers { get; set; }
        public string participationChannel { get; set; }
        public bool isMember { get; set; }
        public string messageAlert { get; set;}
        public bool isAccept { get; set; }
        public int userID { get; set; }

        public string OwnerName { get; set; }

        public int CountVotes { get; set; }

    }
}
