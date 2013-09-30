using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserReported
    {
        public virtual int Id { get; set; }

        public virtual int UserReportedId { get; set; }

        public virtual int UserReporterId { get; set; }


        public Enums.ReportType Type { get; set; }

        public int ReportId { get; set; }
    }

}
