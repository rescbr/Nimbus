﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Topic
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

        [References(typeof(User))]
        public int AuthorId { get; set; }


        public string Title { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public Enums.TopicType TopicType { get; set; }
        public bool? Visibility { get; set; }
        public string Text { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string UrlVideo { get; set; }
    }
}
