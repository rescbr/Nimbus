using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{    
    //TODO: Terminar
    public class Ad
    {
        public int Id { get; set; }        
        public int CategoryId { get; set; }
        public int ChannelId { get; set; }
        public Enums.TypeAds TypeAd { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public bool Visible { get; set; }
    }
}
