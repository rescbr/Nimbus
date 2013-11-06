using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class ImgTopChannel:Nimbus.Model.ImgTopChannel
    {
        [References(typeof(Category))]
        public int CategoryId { get; set; }
    }
}
