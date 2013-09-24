using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class TopicReported
    {
        public virtual int Id { get; set; }

        public virtual int TopicReportedId { get; set; }

        public virtual int UserReporterId { get; set; }

        public virtual int UserReportedId { get; set; }


        public string Justification { get; set; }

    }
}
