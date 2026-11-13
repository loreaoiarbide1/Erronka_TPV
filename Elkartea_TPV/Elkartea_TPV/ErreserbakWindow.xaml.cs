
using Elkartea_TPV.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Elkartea_TPV
{
    public partial class ErreserbakWindow : Window
    {
        private readonly DatuBasea _datuBasea = new DatuBasea();

        private readonly Erabiltzaileak _erabiltzailea;

        private readonly ObservableCollection<Erreserbak> _erreserbak = new();

        private int? _mahaiAukeratua;

        // Mahai bakoitzaren eserleku kopurua gordetzen du.
        private readonly Dictionary<int, int> _mahaiKopurua = new()
        {
            { 1, 2 },
            { 2, 4 },
            { 3, 6 },
            { 4, 2 },
            { 5, 4 },
            { 6, 6 }
        };

        public ErreserbakWindow(Erabiltzaileak erabiltzailea)
        {
            InitializeComponent();

            _erabiltzailea = erabiltzailea;

            // Hasierako data eta zerbitzua aukeratzen ditu.
            dpData.SelectedDate = DateTime.Today;
            cmbZerbitzua.SelectedIndex = 0;

            dgvErreserbak.ItemsSource = _erreserbak;

            // Data edo zerbitzua aldatzean mahaiak eguneratzen ditu.
            dpData.SelectedDateChanged += AukeraketaAldatu;
            cmbZerbitzua.SelectionChanged += AukeraketaAldatu;

            FreskatuErreserbak();
        }

        // Aukeratutako data eta zerbitzua lortzen ditu.
        private string LortuZerbitzua()
        {
            if (cmbZerbitzua.SelectedItem is ComboBoxItem elementua)
            {
                return elementua.Content?.ToString() ?? "";
            }

            return "";
        }

        // Erreserben zerrenda eta mahaien egoera eguneratzen ditu.
        private void FreskatuErreserbak()
        {
            if (dpData.SelectedDate == null ||
                cmbZerbitzua.SelectedItem == null)
            {
                return;
            }

            string data = dpData.SelectedDate.Value.ToString("yyyy-MM-dd");
            string zerbitzua = LortuZerbitzua();

            List<Erreserbak> erreserbaGuztiak =
                _datuBasea.LortuErreserbak();

            // Aukeratutako eguneko eta zerbitzuko erreserbak erakusten ditu.
            List<Erreserbak> egunekoErreserbak = erreserbaGuztiak
                .Where(e =>
                    e.Data == data &&
                    e.BazkariaAfaria == zerbitzua)
                .OrderBy(e => e.MahaiZenbakia)
                .ToList();

            _erreserbak.Clear();

            foreach (Erreserbak erreserba in egunekoErreserbak)
            {
                _erreserbak.Add(erreserba);
            }

            // Erreserbatutako mahaiak identifikatzen ditu.
            HashSet<int> mahaiErreserbatuak = egunekoErreserbak
                .Select(e => e.MahaiZenbakia)
                .ToHashSet();

            // Aukeratutako mahaia jada erreserbatuta badago, desautatzen du.
            if (_mahaiAukeratua.HasValue &&
                mahaiErreserbatuak.Contains(_mahaiAukeratua.Value))
            {
                _mahaiAukeratua = null;
            }

            // Mahai guztiak berriz sortzen ditu.
            panelMahaiak.Children.Clear();

            foreach (var mahaia in _mahaiKopurua)
            {
                bool erreserbatuta =
                    mahaiErreserbatuak.Contains(mahaia.Key);

                bool hautatuta =
                    _mahaiAukeratua == mahaia.Key;

                Button botoia = SortuMahaiBotoia(
                    mahaia.Key,
                    mahaia.Value,
                    erreserbatuta,
                    hautatuta);

                panelMahaiak.Children.Add(botoia);
            }

            EguneratuMahaiInformazioa();
        }

        // Data edo zerbitzua aldatzean aurreko hautaketa kentzen du.
        private void AukeraketaAldatu(object sender, EventArgs e)
        {
            _mahaiAukeratua = null;

            FreskatuErreserbak();
        }

        // Mahai bakoitzaren botoia eta irudia sortzen ditu.
        private Button SortuMahaiBotoia(
            int mahaiZenbakia,
            int eserlekuak,
            bool erreserbatuta,
            bool hautatuta)
        {
            string irudiIzena = eserlekuak switch
            {
                2 => "bi_eserleku.png",
                4 => "lau_eserleku.png",
                6 => "sei_eserleku.png",
                _ => ""
            };

            string irudiBidea = Path.Combine(
                AppContext.BaseDirectory,
                "Irudiak",
                irudiIzena);

            Border edukiontzia = new Border
            {
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(7),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(85, 184, 121))
            };

            // Mahaiaren egoeraren arabera kolorea aldatzen du.
            if (erreserbatuta)
            {
                edukiontzia.Background =
                    new SolidColorBrush(Color.FromRgb(252, 236, 236));

                edukiontzia.BorderBrush =
                    new SolidColorBrush(Color.FromRgb(230, 108, 108));
            }
            else if (hautatuta)
            {
                edukiontzia.Background =
                    new SolidColorBrush(Color.FromRgb(234, 243, 255));

                edukiontzia.BorderBrush =
                    new SolidColorBrush(Color.FromRgb(66, 133, 197));
            }

            StackPanel edukia = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Mahaiaren argazkia kargatzen du.
            if (File.Exists(irudiBidea))
            {
                BitmapImage irudia = new BitmapImage();

                irudia.BeginInit();
                irudia.UriSource = new Uri(irudiBidea, UriKind.Absolute);
                irudia.CacheOption = BitmapCacheOption.OnLoad;
                irudia.EndInit();
                irudia.Freeze();

                Image mahaiIrudia = new Image
                {
                    Source = irudia,
                    Width = 115,
                    Height = 105,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                edukia.Children.Add(mahaiIrudia);
            }
            else
            {
                TextBlock irudiMezua = new TextBlock
                {
                    Text = "Irudia ez dago",
                    Width = 115,
                    Height = 105,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Gray
                };

                edukia.Children.Add(irudiMezua);
            }

            TextBlock mahaiIzena = new TextBlock
            {
                Text = "Mahaia " + mahaiZenbakia,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(38, 55, 70)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock eserlekuIzena = new TextBlock
            {
                Text = eserlekuak + " eserleku",
                FontSize = 12,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(104, 119, 131)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock egoera = new TextBlock
            {
                Text = erreserbatuta
                    ? "Erreserbatuta"
                    : hautatuta
                        ? "Hautatuta"
                        : "Libre",

                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,

                Foreground = erreserbatuta
                    ? Brushes.Firebrick
                    : hautatuta
                        ? Brushes.RoyalBlue
                        : Brushes.ForestGreen
            };

            edukia.Children.Add(mahaiIzena);
            edukia.Children.Add(eserlekuIzena);
            edukia.Children.Add(egoera);

            edukiontzia.Child = edukia;

            Button botoia = new Button
            {
                Content = edukiontzia,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(3),
                Margin = new Thickness(5),
                Cursor = Cursors.Hand,
                Tag = mahaiZenbakia,
                IsEnabled = !erreserbatuta,
                ToolTip = erreserbatuta
                    ? "Mahai hau erreserbatuta dago"
                    : "Hautatu mahaia"
            };

            botoia.Click += MahaiaAukeratu;

            return botoia;
        }

        // Erabiltzaileak aukeratutako mahaia gordetzen du.
        private void MahaiaAukeratu(object sender, RoutedEventArgs e)
        {
            if (sender is not Button botoia ||
                botoia.Tag == null)
            {
                return;
            }

            _mahaiAukeratua = Convert.ToInt32(botoia.Tag);

            FreskatuErreserbak();
        }

        // Aukeratutako mahaiaren informazioa erakusten du.
        private void EguneratuMahaiInformazioa()
        {
            if (!_mahaiAukeratua.HasValue)
            {
                txtMahaiAukeratua.Text =
                    "Ez duzu mahairik aukeratu";

                txtMahaiEserlekuak.Text = "";

                return;
            }

            int zenbakia = _mahaiAukeratua.Value;
            int eserlekuak = _mahaiKopurua[zenbakia];

            txtMahaiAukeratua.Text =
                "Mahaia " + zenbakia;

            txtMahaiEserlekuak.Text =
                eserlekuak + " eserleku";
        }

        // Aukeratutako mahaia datu-basean gordetzen du.
        private void BtnErreserbaEgin_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dpData.SelectedDate == null)
            {
                MessageBox.Show(
                    "Aukeratu erreserbaren data.",
                    "Erreserbak",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!_mahaiAukeratua.HasValue)
            {
                MessageBox.Show(
                    "Lehenengo mahai bat aukeratu behar duzu.",
                    "Erreserbak",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string data =
                dpData.SelectedDate.Value.ToString("yyyy-MM-dd");

            string zerbitzua = LortuZerbitzua();

            try
            {
                _datuBasea.GordeErreserba(
                    _mahaiAukeratua.Value,
                    data,
                    zerbitzua,
                    _erabiltzailea.ErabiltzaileIzena);

                MessageBox.Show(
                    "Erreserba behar bezala gorde da.",
                    "Erreserba eginda",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _mahaiAukeratua = null;

                FreskatuErreserbak();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Mahai erreserbatua",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                _mahaiAukeratua = null;

                FreskatuErreserbak();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea erreserba gordetzean:\n" + ex.Message,
                    "Errorea",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Aukeratutako erreserba datu-basetik ezabatzen du.
        private void BtnErreserbaEzabatu_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgvErreserbak.SelectedItem is not Erreserbak erreserba)
            {
                MessageBox.Show(
                    "Lehenengo ezabatu nahi duzun erreserba aukeratu.",
                    "Erreserbak",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult erantzuna = MessageBox.Show(
                "Ziur zaude erreserba hau ezabatu nahi duzula?",
                "Erreserba ezabatu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (erantzuna != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _datuBasea.EzabatuErreserba(erreserba.Id);

                MessageBox.Show(
                    "Erreserba behar bezala ezabatu da.",
                    "Erreserba ezabatuta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                FreskatuErreserbak();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea erreserba ezabatzean:\n" + ex.Message,
                    "Errorea",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Menu nagusira itzultzen da.
        private void BtnAtzera_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow? leihoNagusia = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            if (leihoNagusia == null)
            {
                leihoNagusia = new MainWindow(_erabiltzailea);
                leihoNagusia.Show();
            }
            else
            {
                leihoNagusia.Show();
                leihoNagusia.Activate();
            }

            Close();
        }
    }
}
