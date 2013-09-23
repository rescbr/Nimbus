using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Role
    {
        public virtual int UserId { get; set; }

        public virtual int ChannelId { get; set; }


        public bool ChannelMagager { get; set; } 
        public bool TopicManager { get; set; }
        public bool MessageManager { get; set; }
        public bool UserManager { get; set; }
        public bool ModeratorManager { get; set; }
        public bool IsOwner { get; set; }
        public bool Paid { get; set; }
        public bool Accepted { get; set; }
    }
}
