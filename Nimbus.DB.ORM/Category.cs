﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Category : Nimbus.Model.Category
    {
        [AutoIncrement]
        public int Id { get; set; }
    }
}
