﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Message
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int Receiver_ID { get; set; }

        [References(typeof(User))]
        public int Sender_ID { get; set; }

        [References(typeof(Channel))]
        public int Channel_ID { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public bool ReadStatus { get; set; }
        public Enums.MessageType Status { get; set; }
        public DateTime Date { get; set; }
        public bool Visible { get; set; }
    }
}