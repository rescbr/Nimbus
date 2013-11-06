using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public class ReportModel
    {
        [References(typeof(Nimbus.Model.User))]
        public int userReported_id{get;set;}

        public string justification { get; set; }
        public string typeReport { get; set; }
        public int idReport { get; set; }
    }
}