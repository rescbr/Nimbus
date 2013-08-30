using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ChannelUser
    {
        [References(typeof(User))]
        public int UserId { get; set; }
        
        [References(typeof(Channel))]
        public int ChannelId { get; set; }
        
        public bool? Vote { get; set; }

        public int Interaction { get; set; }

        public bool Follow { get; set; }
    }
}
