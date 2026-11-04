using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elkartea_TPV.Models
{
    public class Erabiltzaileak
    {
        public long Id { get; set; }
        public string ErabiltzaileIzena { get; set; } = "";
        public string Pasahitza { get; set; } = "";
        public string Mota { get; set; } = "";
    }
}
