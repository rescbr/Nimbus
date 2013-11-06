using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class TagTopic
    {
        public int TagId { get; set; }
        public int TopicId { get; set; }
        public bool Visible { get; set; }
    }
}
