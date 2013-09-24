using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ChannelTag
    {
        public int Id { get; set; }

        public int ChannelId { get; set; }

        public string TagName { get; set; }
        public bool Visible { get; set; }
    }
}
