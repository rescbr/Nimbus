using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public  class UserExam
    {
        [References(typeof(User))]
        public int UserID { get; set; }

        [References(typeof(Topic))]
        public int ExamID { get; set; }

        [References(typeof(Organization))]
        public int OrganizationID { get; set; }
        public int Grade { get; set; }
        public DateTime RealizedOn { get; set; }
    }
}
