using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class StorageUpload : Model.StorageUpload
    {
        [PrimaryKey]
        public override Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [References(typeof(User))]
        public override int UserId
        {
            get { return base.UserId; }
            set { base.UserId = value; }
        }
    }
}
