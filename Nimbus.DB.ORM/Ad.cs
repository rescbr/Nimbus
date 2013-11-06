﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    //TODO: Terminar
    public class Ad : Nimbus.Model.Ad
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

    }
}
