using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public  class UserExam
    {
        public virtual int UserId { get; set; }

        public virtual int ExamId { get; set; }

        public virtual int OrganizationId { get; set; }


        public int Grade { get; set; }
        public DateTime RealizedOn { get; set; }
    }
}
