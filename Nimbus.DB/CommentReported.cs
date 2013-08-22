using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class CommentReported
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(User))]
        public int UserReporter_ID { get; set; }

        [References (typeof(Comment))]
        public int CommentReported_ID { get; set; }

        public string Justification { get; set; }

    }
}
