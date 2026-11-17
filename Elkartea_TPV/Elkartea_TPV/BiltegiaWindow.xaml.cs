
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Elkartea_TPV.Models;

namespace Elkartea_TPV
{
    public partial class BiltegiaWindow : Window
    {
        private readonly DatuBasea _datuBasea = new();

        private long? _aukeratutakoId = null;

        public BiltegiaWindow()
        {
            InitializeComponent();
            KargatuStocka();
        }

        // Stock-eko produktu guztiak zerrendan erakutsi
        private void KargatuStocka()
        {
            dgStocka.ItemsSource = _datuBasea.LortuStocka();
        }

        // Eremuak garbitu eta produktu berria sortzeko prestatu
        private void BtnBerria_Click(object sender, RoutedEventArgs e)
        {
            _aukeratutakoId = null;

            txtProduktua.Clear();
            txtKantitatea.Clear();
            txtPrezioa.Clear();

            cmbKategoria.SelectedIndex = 0;

            dgStocka.SelectedItem = null;
        }

        // Produktu berria gorde edo aukeratutakoa aldatu
        private void BtnGorde_Click(object sender, RoutedEventArgs e)
        {
            string produktua = txtProduktua.Text.Trim();

            if (string.IsNullOrWhiteSpace(produktua))
            {
                MessageBox.Show("Sartu produktuaren izena.");
                return;
            }

            if (!int.TryParse(txtKantitatea.Text, out int kantitatea) ||
                kantitatea < 0)
            {
                MessageBox.Show("Sartu kantitate baliagarri bat.");
                return;
            }

            string prezioTestua = txtPrezioa.Text.Trim().Replace(',', '.');

            if (!double.TryParse(
                prezioTestua,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out double prezioa) || prezioa < 0)
            {
                MessageBox.Show("Sartu prezio baliagarri bat.");
                return;
            }

            if (cmbKategoria.SelectedItem is not ComboBoxItem kategoriaAukeratua)
            {
                MessageBox.Show("Aukeratu produktuaren kategoria.");
                return;
            }

            string kategoria = kategoriaAukeratua.Content.ToString() ?? "";

            try
            {
                if (_aukeratutakoId == null)
                {
                    _datuBasea.GordeStocka(
                        produktua, kantitatea, prezioa, kategoria);

                    MessageBox.Show("Produktua gehitu da.");
                }
                else
                {
                    _datuBasea.EguneratuStocka(
                        _aukeratutakoId.Value,
                        produktua,
                        kantitatea,
                        prezioa,
                        kategoria);

                    MessageBox.Show("Produktua eguneratu da.");
                }

                BtnBerria_Click(sender, e);
                KargatuStocka();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Ezin izan da produktua gorde. " +
                    "Baliteke produktu horren izena dagoeneko erabiltzea.");
            }
        }

        // Zerrendan produktu bat aukeratzean bere datuak kargatu
        private void DgStocka_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (dgStocka.SelectedItem is Stocka produktua)
            {
                _aukeratutakoId = produktua.Id;

                txtProduktua.Text = produktua.Produktua;
                txtKantitatea.Text = produktua.Kantitatea.ToString();

                txtPrezioa.Text = produktua.Prezioa.ToString("0.00",CultureInfo.CurrentCulture);

                // Aukeratutako produktuaren kategoria ComboBox-ean erakutsi
                cmbKategoria.SelectedItem = cmbKategoria.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(item =>
                        item.Content.ToString() == produktua.Kategoria);
            }
        }

        // Aukeratutako produktua ezabatu
        private void BtnEzabatu_Click(object sender, RoutedEventArgs e)
        {
            if (_aukeratutakoId == null)
            {
                MessageBox.Show("Aukeratu produktu bat.");
                return;
            }

            MessageBoxResult erantzuna = MessageBox.Show(
                "Ziur zaude produktu hau ezabatu nahi duzula?",
                "Produktua ezabatu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (erantzuna == MessageBoxResult.Yes)
            {
                try
                {
                    _datuBasea.EzabatuStocka(_aukeratutakoId.Value);

                    MessageBox.Show("Produktua ezabatu da.");

                    BtnBerria_Click(sender, e);
                    KargatuStocka();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ezin izan da produktua ezabatu.");
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
