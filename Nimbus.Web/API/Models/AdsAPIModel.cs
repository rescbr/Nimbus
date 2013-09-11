using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public class showAdsAPIModel
    {
        public int idAds { get; set; }
        public int category_id { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
    }
}