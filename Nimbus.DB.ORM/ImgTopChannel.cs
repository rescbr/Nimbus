using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class ImgTopChannel:Nimbus.DB.ImgTopChannel
    {
        [References(typeof(Category))]
        public int CategoryId { get; set; }
    }
}
