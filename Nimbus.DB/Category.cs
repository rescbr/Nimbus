﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public string ImageUrl { get; set; }
        public string ColorCode { get; set; }
    }
}
