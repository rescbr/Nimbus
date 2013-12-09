using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class UserReported
    {
        public int Id { get; set; }

        public int UserReportedId { get; set; }

        public int UserReporterId { get; set; }


        public string Justification { get; set; }
        
    }

}
