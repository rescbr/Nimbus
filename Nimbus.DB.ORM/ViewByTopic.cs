using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class ViewByTopic : Nimbus.DB.ViewByTopic
    {
        [References (typeof(Topic))]
        public override int TopicId { get; set; }

    }
}
