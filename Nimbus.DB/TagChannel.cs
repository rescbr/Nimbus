using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class TagChannel
    {
        public virtual int TagId { get; set; }
        public virtual int ChannelId { get; set; }
        public virtual bool Visible { get; set; }
    }
}
