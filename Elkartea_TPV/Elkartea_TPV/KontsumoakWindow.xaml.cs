
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Elkartea_TPV.Models;

namespace Elkartea_TPV
{
    public partial class KontsumoakWindow : Window
    {
        private readonly DatuBasea _datuBasea = new();
        private readonly Erabiltzaileak _erabiltzailea;

        private readonly ObservableCollection<KontsumoLerroa> _kontsumoak = new();

        private List<Stocka> _produktuak = new();

        public KontsumoakWindow(Erabiltzaileak erabiltzailea)
        {
            InitializeComponent();

            _erabiltzailea = erabiltzailea;

            dgvKontsumoa.ItemsSource = _kontsumoak;

            KargatuProduktuak("Denak");
            KalkulatuGuztira();
        }

        // Stockean dauden produktuak kategoriaren arabera kargatzen ditu.
        private void KargatuProduktuak(string kategoria)
        {
            panelProduktuak.Children.Clear();

            _produktuak = _datuBasea.LortuStocka();

            foreach (Stocka produktua in _produktuak)
            {
                string produktuKategoria = LortuKategoria(produktua.Produktua);

                if (kategoria != "Denak" &&
                    produktuKategoria != kategoria)
                {
                    continue;
                }

                Button botoia = new Button
                {
                    Width = 145,
                    Height = 165,
                    Margin = new Thickness(5),
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(210, 218, 225)),
                    BorderThickness = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = produktua
                };

                StackPanel edukia = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // Produktuaren argazkia bilatzen du.
                string fitxategia = LortuIrudia(produktua.Produktua);

                string irudiBidea = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Irudiak",
                    fitxategia
                );

                if (!string.IsNullOrWhiteSpace(fitxategia) &&
                    File.Exists(irudiBidea))
                {
                    Image argazkia = new Image
                    {
                        Source = new BitmapImage(
                            new Uri(irudiBidea, UriKind.Absolute)),
                        Width = 90,
                        Height = 85,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5)
                    };

                    edukia.Children.Add(argazkia);
                }
                else
                {
                    TextBlock irudirikEz = new TextBlock
                    {
                        Text = "Irudirik ez",
                        FontSize = 12,
                        Foreground = Brushes.Gray,
                        Height = 85,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };

                    edukia.Children.Add(irudirikEz);
                }

                TextBlock izena = new TextBlock
                {
                    Text = produktua.Produktua,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(5)
                };

                TextBlock prezioa = new TextBlock
                {
                    Text = produktua.Prezioa.ToString("0.00") + " €",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(22, 134, 83)),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(3)
                };

                edukia.Children.Add(izena);
                edukia.Children.Add(prezioa);

                botoia.Content = edukia;
                botoia.Click += Produktua_Click;

                panelProduktuak.Children.Add(botoia);
            }
        }

        // Produktu bakoitzari dagokion kategoria itzultzen du.

        // Produktuaren izena normalizatzen du, maiuskulak eta azentuak kenduz.
        private string NormalizatuIzena(string izena)
        {
            string testua = izena.Trim().ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            StringBuilder emaitza = new StringBuilder();

            foreach (char karakterea in testua)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(karakterea)
                    != UnicodeCategory.NonSpacingMark)
                {
                    emaitza.Append(karakterea);
                }
            }

            return emaitza.ToString()
                .Normalize(NormalizationForm.FormC);
        }

        // Produktu bakoitzari dagokion kategoria itzultzen du.
        private string LortuKategoria(string produktua)
        {
            string izena = NormalizatuIzena(produktua);

            // Edariak
            if (izena.Contains("coca") ||
                izena.Contains("kas") ||
                izena.Contains("garagard") ||
                izena.Contains("cervez") ||
                izena == "ura" ||
                izena == "agua")
            {
                return "Edariak";
            }

            // Kafeak
            if (izena.Contains("kafe") ||
                izena.Contains("cafe") ||
                izena.Contains("ebaki") ||
                izena.Contains("cortado"))
            {
                return "Kafeak";
            }

            // Janaria
            if (izena.Contains("gildak") ||
                izena.Contains("kroketak") ||
                izena.Contains("patatak") ||
                izena.Contains("rabak") ||
                izena.Contains("azeitunak")) 
                
            {
                return "Janaria";
            }

            return "Janaria";
        }


        // Produktu bakoitzari dagokion irudiaren izena itzultzen du.
        private string LortuIrudia(string produktua)
        {
            switch (produktua.Trim().ToLowerInvariant())
            {
                case "coca-cola":
                    return "coca-cola.png";

                case "ebakia":
                    return "ebakia.png";

                case "garagardoa":
                    return "garagardoa.png";

                case "kafe_deskafeinatua":
                    return "kafe_deskafeinatua.png";

                case "kafe_hutsa":
                    return "kafe_hutsa.png";

                case "kafesnia":
                    return "kafesnia.png";

                case "kas_naranja":
                case "kas":
                    return "kas_naranja.png";

                case "ura":
                    return "ura.png";

                default:
                    return "";
            }
        }

        // Aukeratutako kategoriako produktuak erakusten ditu.
        private void Kategoria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button botoia)
            {
                return;
            }

            string kategoria = botoia.Tag?.ToString() ?? "Denak";

            foreach (Button kategoriaBotoia in panelKategoriak.Children)
            {
                kategoriaBotoia.Background = new SolidColorBrush(
                    Color.FromRgb(38, 55, 70));
            }

            botoia.Background = new SolidColorBrush(
                Color.FromRgb(22, 134, 83));

            KargatuProduktuak(kategoria);
        }

        // Produktu bat aukeratutako kontsumora gehitzen du.
        // Produktu bat aukeratzean, gehitu nahi den kantitatea eskatzen du.
        private void Produktua_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button botoia || botoia.Tag is not Stocka produktua)
                return;

            // Kantitatea eskatu.
            int? kantitatea = EskatuKantitatea(produktua.Produktua);

            if (kantitatea == null)
                return;

            if (kantitatea <= 0)
            {
                MessageBox.Show("Kantitatea 0 baino handiagoa izan behar da.",
                    "Abisua", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Produktua dagoeneko saskian dagoen egiaztatu.
            KontsumoLerroa? lerroa = _kontsumoak.FirstOrDefault(
                x => x.Produktua == produktua.Produktua);

            int aurrekoKantitatea = lerroa?.Kantitatea ?? 0;

            // Stock nahikoa dagoen egiaztatu.
            if (aurrekoKantitatea + kantitatea.Value > produktua.Kantitatea)
            {
                MessageBox.Show(
                    $"Ez dago nahikoa stock. Eskuragarri: {produktua.Kantitatea - aurrekoKantitatea}",
                    "Stock nahikorik ez",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Saskian gehitu edo dagoen kantitatea handitu.
            if (lerroa != null)
            {
                lerroa.Kantitatea += kantitatea.Value;
            }
            else
            {
                _kontsumoak.Add(new KontsumoLerroa
                {
                    Produktua = produktua.Produktua,
                    Kantitatea = kantitatea.Value,
                    Prezioa = produktua.Prezioa
                });
            }

            dgvKontsumoa.Items.Refresh();
            KalkulatuGuztira();
        }

        // Erabiltzaileari gehitu nahi duen kantitatea eskatzen dio.
        private int? EskatuKantitatea(string produktua)
        {
            Window leihoa = new Window
            {
                Title = "Produktuaren kantitatea",
                Width = 320,
                Height = 190,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(245, 247, 250))
            };

            StackPanel panela = new StackPanel
            {
                Margin = new Thickness(20)
            };

            TextBlock izena = new TextBlock
            {
                Text = produktua + " - Zenbat unitate nahi dituzu?",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };

            TextBox kantitateTestua = new TextBox
            {
                Text = "1",
                FontSize = 16,
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 0, 15)
            };

            StackPanel botoiak = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button ezeztatu = new Button
            {
                Content = "Ezeztatu",
                Width = 85,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(5)
            };

            Button gehitu = new Button
            {
                Content = "Gehitu",
                Width = 85,
                Padding = new Thickness(5),
                IsDefault = true
            };

            int? emaitza = null;

            ezeztatu.Click += (s, e) =>
            {
                leihoa.DialogResult = false;
            };

            gehitu.Click += (s, e) =>
            {
                if (int.TryParse(kantitateTestua.Text, out int kantitatea)
                    && kantitatea > 0)
                {
                    emaitza = kantitatea;
                    leihoa.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Sartu kantitate zuzen bat.");
                    kantitateTestua.Focus();
                }
            };

            botoiak.Children.Add(ezeztatu);
            botoiak.Children.Add(gehitu);

            panela.Children.Add(izena);
            panela.Children.Add(kantitateTestua);
            panela.Children.Add(botoiak);

            leihoa.Content = panela;
            kantitateTestua.SelectAll();
            kantitateTestua.Focus();

            leihoa.ShowDialog();

            return emaitza;
        }

        // Kontsumoaren guztizko prezioa kalkulatzen du.
        private void KalkulatuGuztira()
        {
            double guztira = 0;

            foreach (KontsumoLerroa lerroa in _kontsumoak)
            {
                guztira += lerroa.Guztira;
            }

            txtGuztira.Text = guztira.ToString("0.00") + " €";
        }

        // Aukeratutako produktua kentzen du.
        private void BtnKendu_Click(object sender, RoutedEventArgs e)
        {
            if (dgvKontsumoa.SelectedItem is not KontsumoLerroa lerroa)
            {
                MessageBox.Show(
                    "Aukeratu kendu nahi duzun produktua.");
                return;
            }

            _kontsumoak.Remove(lerroa);

            KalkulatuGuztira();
        }

        // Kontsumoa datu-basean gordetzen du eta stocka eguneratzen du.
        private void BtnAmaitu_Click(object sender, RoutedEventArgs e)
        {
            if (_kontsumoak.Count == 0)
            {
                MessageBox.Show("Ez dago produkturik kontsumoan.");
                return;
            }

            double guztira = 0;

            foreach (KontsumoLerroa lerroa in _kontsumoak)
            {
                guztira += lerroa.Guztira;
            }

            MessageBoxResult erantzuna = MessageBox.Show(
                "Kontsumoa amaitu nahi duzu?\n\n" +
                "Guztira: " + guztira.ToString("0.00") + " €",
                "Kontsumoa amaitu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (erantzuna != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _datuBasea.GordeKontsumoaEtaEguneratuStocka(
                    _erabiltzailea.ErabiltzaileIzena,
                    _kontsumoak
                );

                MessageBox.Show(
                    "Kontsumoa behar bezala gorde da.\n\n" +
                    "Erabiltzailea: " + _erabiltzailea.ErabiltzaileIzena +
                    "\nGuztira: " + guztira.ToString("0.00") + " €",
                    "Kontsumoa gordeta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _kontsumoak.Clear();

                KalkulatuGuztira();

                KargatuProduktuak("Denak");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Stock arazoa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                KargatuProduktuak("Denak");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea kontsumoa gordetzean:\n" + ex.Message,
                    "Errorea",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Kontsumo osoa ezeztatzen du.
        private void BtnEzeztatu_Click(object sender, RoutedEventArgs e)
        {
            if (_kontsumoak.Count == 0)
            {
                return;
            }

            MessageBoxResult erantzuna = MessageBox.Show(
                "Ziur zaude kontsumoa ezeztatu nahi duzula?",
                "Kontsumoa ezeztatu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (erantzuna == MessageBoxResult.Yes)
            {
                _kontsumoak.Clear();

                KalkulatuGuztira();
            }
        }
    }
}
