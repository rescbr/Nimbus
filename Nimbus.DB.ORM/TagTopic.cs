using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class TagTopic: Nimbus.Model.TagTopic
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tag))]
        public int TagId { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }
    }
}
