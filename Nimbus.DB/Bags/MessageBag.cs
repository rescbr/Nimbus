﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.Bags
{
    public class MessageBag : Message
    {
        public string AvatarUrl { get; set; }

        public string UserName { get; set; }

        public bool UserReadStatus { get; set; }

    }
}
