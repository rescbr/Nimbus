using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class StorageUpload
    {
        public virtual Guid Id { get; set; }
        public virtual int UserId { get; set; }
        public string Url { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
    }
}
