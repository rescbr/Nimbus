using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    //TODO: Terminar
    public class Ad
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public bool Visible { get; set; }
        public int CountDay { get; set; }
        public int CountClick { get; set; }
    }
}
