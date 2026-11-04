
using System.Windows;
using Elkartea_TPV.Models;

namespace Elkartea_TPV
{
    public partial class MainWindow : Window
    {
        private readonly Erabiltzaileak _erabiltzailea;

        public MainWindow(Erabiltzaileak erabiltzailea)
        {
            InitializeComponent();

            _erabiltzailea = erabiltzailea;

            txtOngietorria.Text =
                "Ongi etorri, " + _erabiltzailea.ErabiltzaileIzena;

            if (_erabiltzailea.Mota == "Administratzailea")
            {
                panelAdministratzailea.Visibility = Visibility.Visible;
            }
            else
            {
                panelErabiltzailea.Visibility = Visibility.Visible;
            }
        }

        private void BtnErabiltzaileak_Click(object sender, RoutedEventArgs e)
        {
            ErabiltzaileakWindow leihoa = new ErabiltzaileakWindow();
            leihoa.ShowDialog();
        }

        private void BtnStocka_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Stockaren kudeaketa");
        }

        private void BtnKontsumoak_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kontsumoen kudeaketa");
        }

        private void BtnErreserbak_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Erreserben kudeaketa");
        }
    }
}
