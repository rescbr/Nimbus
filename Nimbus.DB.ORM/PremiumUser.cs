﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class PremiumUser : Nimbus.Model.PremiumUser
    {
        [PrimaryKey]
        [References(typeof(Premium))]
        public override int PremiumId { get; set; }

        [PrimaryKey]
        [References(typeof(User))]
        public override int UserId { get; set; }
        
    }
}
