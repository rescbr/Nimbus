using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class RoleTopic
    {
        public virtual int ChannelId { get; set; }

        public virtual int TopicId { get; set; }

        public virtual int UserId { get; set; }


        public bool Paid { get; set; }
    }
}
