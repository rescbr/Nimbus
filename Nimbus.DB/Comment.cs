﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Comment
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Comment))]
        public int ParentId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }

        public DateTime PostedOn { get; set; }
        public bool Visible { get; set; }
        public string Text { get; set; }

    }
}
