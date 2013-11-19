using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public  class UserExam : Nimbus.Model.UserExam
    {
        [PrimaryKey]
        [References(typeof(User))]
        public override int UserId { get; set; }

        [PrimaryKey]
        [References(typeof(Topic))]
        public override int ExamId { get; set; }

        [References(typeof(Organization))]
        public override int OrganizationId { get; set; }
    }
}
