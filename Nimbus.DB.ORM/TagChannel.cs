﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class TagChannel : Nimbus.Model.TagChannel
    {
       [References(typeof(TagChannel))]
        public override int TagId { get; set; }

        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

    }
}
