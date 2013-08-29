using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ReceiverMessage
    {
        [References(typeof(User))]
        public int UserID { get; set; }

        [References(typeof(Channel))]
        public int ChannelID { get; set; }

        [References(typeof(User))]
        public string Name { get; set; }
    }
}
