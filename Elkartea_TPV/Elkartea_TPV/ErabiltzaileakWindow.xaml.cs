
using System;
using System.Windows;
using System.Windows.Controls;
using Elkartea_TPV.Models;

namespace Elkartea_TPV
{
    public partial class ErabiltzaileakWindow : Window
    {
        private readonly DatuBasea _datuBasea = new();

        private long? _aukeratutakoId = null;

        public ErabiltzaileakWindow()
        {
            InitializeComponent();
            KargatuErabiltzaileak();
        }

        // Erabiltzaile guztiak zerrendan erakutsi
        private void KargatuErabiltzaileak()
        {
            dgvErabiltzaileak.ItemsSource =
                _datuBasea.LortuErabiltzaileak();
        }

        // Eremuak garbitu eta erabiltzaile berria sortzeko prestatu
        private void BtnBerria_Click(object sender, RoutedEventArgs e)
        {
            _aukeratutakoId = null;

            txtIzena.Clear();
            txtPasahitza.Clear();
            cmbMota.SelectedIndex = -1;

            dgvErabiltzaileak.SelectedItem = null;
        }

        // Erabiltzaile berria gorde edo aukeratutakoa aldatu
        private void BtnGorde_Click(object sender, RoutedEventArgs e)
        {
            string izena = txtIzena.Text.Trim();
            string pasahitza = txtPasahitza.Password;

            if (string.IsNullOrWhiteSpace(izena) ||
                string.IsNullOrWhiteSpace(pasahitza) ||
                cmbMota.SelectedItem == null)
            {
                MessageBox.Show("Bete eremu guztiak.");
                return;
            }

            string mota = ((ComboBoxItem)cmbMota.SelectedItem).Content.ToString()!;

            try
            {
                if (_aukeratutakoId == null)
                {
                    _datuBasea.GordeErabiltzailea(
                        izena, pasahitza, mota);

                    MessageBox.Show("Erabiltzailea sortu da.");
                }
                else
                {
                    _datuBasea.EguneratuErabiltzailea(
                        _aukeratutakoId.Value,
                        izena,
                        pasahitza,
                        mota);

                    MessageBox.Show("Erabiltzailea eguneratu da.");
                }

                BtnBerria_Click(sender, e);
                KargatuErabiltzaileak();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Ezin izan da erabiltzailea gorde. " +
                    "Baliteke erabiltzaile izena dagoeneko erabiltzea.");
            }
        }

        // Zerrendan erabiltzaile bat aukeratzean bere datuak kargatu
        private void DgvErabiltzaileak_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (dgvErabiltzaileak.SelectedItem is Erabiltzaileak erabiltzailea)
            {
                _aukeratutakoId = erabiltzailea.Id;

                txtIzena.Text = erabiltzailea.ErabiltzaileIzena;

                // Pasahitza ez dugu zerrendan erakusten.
                // Aldatzeko, pasahitz berria idatzi behar da.
                txtPasahitza.Clear();

                foreach (ComboBoxItem item in cmbMota.Items)
                {
                    if (item.Content.ToString() == erabiltzailea.Mota)
                    {
                        cmbMota.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // Aukeratutako erabiltzailea ezabatu
        private void BtnEzabatu_Click(object sender, RoutedEventArgs e)
        {
            if (_aukeratutakoId == null)
            {
                MessageBox.Show("Aukeratu erabiltzaile bat.");
                return;
            }

            MessageBoxResult erantzuna = MessageBox.Show(
                "Ziur zaude erabiltzaile hau ezabatu nahi duzula?",
                "Erabiltzailea ezabatu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (erantzuna == MessageBoxResult.Yes)
            {
                try
                {
                    _datuBasea.EzabatuErabiltzailea(
                        _aukeratutakoId.Value);

                    MessageBox.Show("Erabiltzailea ezabatu da.");

                    BtnBerria_Click(sender, e);
                    KargatuErabiltzaileak();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ezin izan da erabiltzailea ezabatu.");
                }
            }
        }

        // Menu nagusira itzuli
        private void BtnAtzera_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
