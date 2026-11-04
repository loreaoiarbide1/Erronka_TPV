using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elkartea_TPV.Models
{
    public class Stocka
    {
        public long Id { get; set; }
        public string Produktua { get; set; } = "";
        public int Kantitatea { get; set; }
        public double Prezioa { get; set; }
    }
}
