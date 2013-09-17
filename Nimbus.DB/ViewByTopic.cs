using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ViewByTopic
    {
        [References (typeof(Topic))]
        public int TopicID { get; set; }

        public int CountView { get; set; }
    }
}
