using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ChannelUser
    {
        public int UserId { get; set; }
        
        public int ChannelId { get; set; }
        
        public bool? Vote { get; set; }
        public int Score { get; set; }
        public int Interaction { get; set; }
        public bool Follow { get; set; }
        public bool Accepted { get; set; }
        public bool Visible { get; set; }
    }
}
