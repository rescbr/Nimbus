﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public  class UserExam : Nimbus.DB.UserExam
    {
        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Topic))]
        public override int ExamId { get; set; }

        [References(typeof(Organization))]
        public override int OrganizationId { get; set; }
    }
}
