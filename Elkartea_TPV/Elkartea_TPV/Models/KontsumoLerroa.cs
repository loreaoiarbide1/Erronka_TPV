
namespace Elkartea_TPV.Models
{
    public class KontsumoLerroa
    {
        public string Produktua { get; set; } = "";
        public int Kantitatea { get; set; }
        public double Prezioa { get; set; }

        public double Guztira
        {
            get { return Kantitatea * Prezioa; }
        }
    }
}
