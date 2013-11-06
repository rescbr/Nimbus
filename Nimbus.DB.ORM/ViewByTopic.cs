using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class ViewByTopic : Nimbus.Model.ViewByTopic
    {
        [References (typeof(Topic))]
        public override int TopicId { get; set; }

    }
}
