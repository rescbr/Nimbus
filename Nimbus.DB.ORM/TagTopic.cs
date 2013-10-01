using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class TagTopic: Nimbus.DB.TagTopic
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }
    }
}
