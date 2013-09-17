using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class RoleTopic
    {
        [References(typeof(Channel))]
        public int ChannelID { get; set; }

        [References(typeof(Topic))]
        public int TopicID { get; set; }

        [References(typeof(User))]
        public int UserID { get; set; }

        public bool Paid { get; set; }
    }
}
