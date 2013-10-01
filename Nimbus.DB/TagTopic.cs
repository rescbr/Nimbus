using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class TagTopic
    {
        public int TagId { get; set; }
        public int ChannelId { get; set; }
        public bool Visible { get; set; }
    }
}
