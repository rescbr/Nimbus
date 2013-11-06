using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class TopicReported  : Nimbus.Model.TopicReported
    {
        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(Topic))]
        public override int TopicReportedId { get; set; }

        [References(typeof(User))]
        public override int UserReporterId { get; set; }

        [References(typeof(User))]
        public override int UserReportedId { get; set; }
        
    }
}
