using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ChannelReported
    {
        public int Id { get; set; }

        public int ChannelReportedId { get; set; }

        public int UserReporterId { get; set; }

        public int UserReportedId { get; set; }

        public string Justification { get; set; }

    }
}
