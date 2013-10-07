using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Enums
    {
        public enum TopicType
        {
            text,
            video,
            discussion,
            exam,
            ads
        }

        public enum MessageType
        {
            received,
            send
        }

        public enum ReportType
        {
            comment,
            topic,
            channel,
            user
        }

        public enum TypeAds
        {
            sponsor,
            ads
        }
    }
}
