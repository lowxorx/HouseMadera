﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeMadera.Entities
{
   public class Gamme
    {
        public int Id { get; set; }
        public string Nom { get; set; }

        public Finition Finition { get; set; }
        public Isolant Isolant { get; set; }

        public virtual ICollection<Plan> Plans { get; set; }
        public virtual ICollection<Module> Modules { get; set; }
    }
}
