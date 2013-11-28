﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    [Alias("Notification")]
    public class Notification<T> : Model.Notification<T>
    {
        [PrimaryKey]
        public override Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }

        [Index(Unique=false)]
        [References(typeof(User))]
        public override int UserId { get; set; }

        [Index(Unique=false)]
        public override long Timestamp
        {
            get
            {
                return base.Timestamp;
            }
            set
            {
                base.Timestamp = value;
            }
        }

    }
}
