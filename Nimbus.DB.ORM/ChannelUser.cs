﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class ChannelUser : Nimbus.DB.ChannelUser
    {
        [References(typeof(User))]
        public int UserId { get; set; }
        
        [References(typeof(Channel))]
        public int ChannelId { get; set; }
        
    }
}