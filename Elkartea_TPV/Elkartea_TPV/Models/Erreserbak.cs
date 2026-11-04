using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elkartea_TPV.Models
{
    public class Erreserbak
    {
        public long Id { get; set; }
        public int MahaiZenbakia { get; set; }
        public string Data { get; set; } = "";
        public string BazkariaAfaria { get; set; } = "";
        public string ErabiltzaileIzena { get; set; } = "";
    }
}
