﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.Bags
{
    public class CommentBag:Comment
    {
        public string AvatarUrl { get; set; }
        public string UserName { get; set; }

        public string TopicName { get; set; }
    }
}