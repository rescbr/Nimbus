using ServiceStack.DataAnnotations;
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
        public int Sender_ID { get; set; }
        
        [References(typeof(Channel))]
        public int Channel_ID { get; set; }

        public class Receiver 
        {
            [References(typeof(User))]
            public int UserID { get; set; }
            
            [References(typeof(User))]
            public string Name { get; set; }

            public bool IsOwner { get; set; }
        }

        public List<Receiver> Receivers { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public bool ReadStatus { get; set; }
        public Enums.MessageType Status { get; set; }
        public DateTime Date { get; set; }
        public bool Visible { get; set; }
    }
}
