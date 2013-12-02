using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public enum NotificationTypeEnum
    {
        message,
        newtopic,
    }
    public class Notification<T>
    {
        public virtual Guid Id { get; set; }
        public virtual int? UserId { get; set; }
        public virtual int? ChannelId { get; set; }
        // Timestamp em DateTime.ToFileTimeUtc()
        public virtual long Timestamp { get; set; }
        public NotificationTypeEnum Type { get; set; }
        public bool IsRead { get; set; }
        public T NotificationObject { get; set; }


    }

}
