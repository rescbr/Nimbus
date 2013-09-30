﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class ChannelReported : Nimbus.DB.ChannelReported
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Channel))]
        public int ChannelReportedId { get; set; }

        [References(typeof(User))]
        public int UserReporterId { get; set; }

        [References(typeof(User))]
        public int UserReportedId { get; set; }
        
    }
}
