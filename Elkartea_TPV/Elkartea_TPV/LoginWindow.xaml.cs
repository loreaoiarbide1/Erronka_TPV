
using System.Windows;
using Elkartea_TPV;

namespace Elkartea_TPV
{
    public partial class LoginWindow : Window
    {
        private readonly DatuBasea _datuBasea = new();

        public LoginWindow()
        {
            InitializeComponent();

            _datuBasea.Hasieratu();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string izena = txtErabiltzailea.Text.Trim();
            string pasahitza = txtPasahitza.Password;

            if (string.IsNullOrWhiteSpace(izena) ||
                string.IsNullOrWhiteSpace(pasahitza))
            {
                txtMezua.Text = "Bete erabiltzaile eta pasahitz eremuak.";
                return;
            }

            var erabiltzailea =
                _datuBasea.EgiaztatuLogin(izena, pasahitza);

            if (erabiltzailea == null)
            {
                txtMezua.Text = "Erabiltzailea edo pasahitza okerra da.";
                return;
            }

            // Erabiltzailearen arabera, saioa hasi ondoren leiho nagusia irekitzen dugu.
            MainWindow leihoNagusia = new MainWindow(erabiltzailea);
            leihoNagusia.Show();

            this.Close();
        }
    }
}
