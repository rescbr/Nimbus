using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Nimbus.Model
{
    public class Message
    {
        public virtual int Id { get; set; }

        public virtual int SenderId { get; set; }

        public virtual int ChannelId { get; set; }
        

        public List<Receiver> Receivers { get; set; }

        public string Title { get; set; }
        public string Text { get; set; }
        public bool ReadStatus { get; set; }
        public DateTime Date { get; set; }
        public bool Visible { get; set; }
    }

    [DataContract]
    public class Receiver
    {
        public virtual int UserId { get; set; }

        public string Name { get; set; }

        public bool IsOwner { get; set; }

        /// <summary>
        /// Atenção! O AvatarUrl não vai pro banco!
        /// </summary>
        [IgnoreDataMember]
        public string AvatarUrl { get; set; }
    }
}
