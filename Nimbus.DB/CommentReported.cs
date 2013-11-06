using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class CommentReported
    {
        public int Id { get; set; }
        
        public int UserReporterId { get; set; }

        public int UserReportedId { get; set; }

        public int CommentReportedId { get; set; }

        public string Justification { get; set; }

    }
}
