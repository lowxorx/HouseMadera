﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeMadera.Entities
{
    public class Projet
    {
        public int Id { get; set; }

        public string Nom { get; set; }
        public string Reference { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }

        public Commercial Commercial { get; set; }
        public Client Client { get; set; }

        public virtual ICollection<Produit> Produits { get; set; }


    }
}
