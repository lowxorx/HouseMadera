﻿using System;
using System.Collections.Generic;

namespace HomeMadera.Entities
{
    public class CoupePrincipe
    {
        public int Id { get; set; }

        public string Nom { get; set; }
        //TODO champs de type blob

        public virtual ICollection<Plan> Plans { get; set; }
    }
}