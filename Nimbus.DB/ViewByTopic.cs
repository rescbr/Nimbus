using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class ViewByTopic
    {
        public virtual int TopicId { get; set; }

        public int CountView { get; set; }
    }
}
