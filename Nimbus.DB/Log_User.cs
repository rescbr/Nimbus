using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Log_User
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int ToUserId { get; set; }
        public string Log { get; set; }
        public string Type { get; set; }
        public int TopicId { get; set; }
        public int ChannelId { get; set; }
        public int CommentId { get; set; }
        public int MessageId { get; set; }
        public int PremiumId { get; set; }
        public int OrganizationId { get; set; }
        public int ReportId { get; set; }
        public string IPAddress { get; set; }

    }
}
