﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ViewByTopic
    {
        public virtual int TopicId { get; set; }

        public int CountView { get; set; }
    }
}