using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models
{
    public enum reportType
    {
        comment,
        topic,
        channel
    }

    public class ReportAPIModel
    {
        [References(typeof(Nimbus.DB.User))]
        public int userReported_id{get;set;}

        public string Justification { get; set; }
        public reportType typeReport { get; set; }
        public int idReport { get; set; }
    }
}