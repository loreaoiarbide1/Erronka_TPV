
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuestPDF.Fluent;
using PdfHelpers = QuestPDF.Helpers;
using PdfColors = QuestPDF.Helpers.Colors;
using Elkartea_TPV.Models;

namespace Elkartea_TPV
{
    public partial class KontsumoakWindow : Window
    {
        private readonly DatuBasea _datuBasea = new();
        private readonly Erabiltzaileak _erabiltzailea;

        private readonly ObservableCollection<KontsumoLerroa> _kontsumoak = new();

        private List<Stocka> _produktuak = new();

        // Azken kontsumoaren ticket-aren datuak gordetzen ditu.
        private List<KontsumoLerroa>? _azkenTicketLerroak;
        private double _azkenTicketGuztira;
        private DateTime _azkenTicketData;

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
        private string LortuKategoria(string produktua)
        {
            string izena = produktua.Trim().ToLowerInvariant();

            // Edariak
            if (izena == "coca-cola" ||
                izena == "kas_naranja" ||
                izena == "garagardoa" ||
                izena == "ura")
            {
                return "Edariak";
            }

            // Kafeak
            if (izena == "ebakia" ||
                izena == "kafe_deskafeinatua" ||
                izena == "kafe_hutsa" ||
                izena == "kafesnia")
            {
                return "Kafeak";
            }

            return "Janaria";
        }

        // Produktuaren izenaren arabera dagokion irudia itzultzen du.
        private string LortuIrudia(string produktua)
        {
            switch (produktua.Trim().ToLowerInvariant())
            {
                // Edariak
                case "coca-cola":
                    return "coca-cola.png";

                case "garagardoa":
                    return "garagardoa.png";

                case "kas_naranja":
                    return "kas_naranja.png";

                case "ura":
                    return "ura.png";

                // Kafeak
                case "ebakia":
                    return "ebakia.png";

                case "kafe_deskafeinatua":
                    return "kafe_deskafeinatua.png";

                case "kafe_hutsa":
                    return "kafe_hutsa.png";

                case "kafesnia":
                    return "kafesnia.png";

                // Janaria
                case "gildak":
                    return "gildak.png";

                case "kroketak":
                    return "kroketak.png";

                case "patatak":
                    return "patatak.png";

                case "rabak":
                    return "rabak.png";

                case "azeitunak":
                    return "azeitunak.png";

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

        // Produktu bat aukeratzean, gehitu nahi den kantitatea eskatzen du.
        private void Produktua_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button botoia ||
                botoia.Tag is not Stocka produktua)
            {
                return;
            }

            int? kantitatea = EskatuKantitatea(produktua.Produktua);

            if (kantitatea == null)
                return;

            if (kantitatea <= 0)
            {
                MessageBox.Show(
                    "Kantitatea 0 baino handiagoa izan behar da.",
                    "Abisua",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Produktua dagoeneko saskian dagoen egiaztatzen du.
            KontsumoLerroa? lerroa = _kontsumoak.FirstOrDefault(
                x => x.Produktua == produktua.Produktua);

            int aurrekoKantitatea = lerroa?.Kantitatea ?? 0;

            // Stock nahikoa dagoen egiaztatzen du.
            if (aurrekoKantitatea + kantitatea.Value > produktua.Kantitatea)
            {
                MessageBox.Show(
                    $"Ez dago nahikoa stock. Eskuragarri: {produktua.Kantitatea - aurrekoKantitatea}",
                    "Stock nahikorik ez",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Saskian gehitu edo dagoen kantitatea handitzen du.
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

            // Ticket-a sortzeko kontsumoaren kopia gordetzen du.
            List<KontsumoLerroa> ticketLerroak = _kontsumoak
                .Select(lerroa => new KontsumoLerroa
                {
                    Produktua = lerroa.Produktua,
                    Kantitatea = lerroa.Kantitatea,
                    Prezioa = lerroa.Prezioa
                })
                .ToList();

            double guztira = ticketLerroak.Sum(lerroa => lerroa.Guztira);
            DateTime data = DateTime.Now;

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

            // Lehenengo kontsumoa gordetzen du eta stocka eguneratzen du.
            try
            {
                _datuBasea.GordeKontsumoaEtaEguneratuStocka(
                    _erabiltzailea.ErabiltzaileIzena,
                    ticketLerroak
                );
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Stock arazoa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                KargatuProduktuak("Denak");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea kontsumoa gordetzean:\n" + ex.Message,
                    "Errorea",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Azken kontsumoaren ticket-aren datuak gordetzen ditu.
            _azkenTicketLerroak = ticketLerroak;
            _azkenTicketGuztira = guztira;
            _azkenTicketData = data;

            // Saskia garbitzen du eta stocka berriro kargatzen du.
            _kontsumoak.Clear();

            KalkulatuGuztira();
            KargatuProduktuak("Denak");

            MessageBox.Show(
                "Kontsumoa behar bezala gorde da.",
                "Kontsumoa gordeta",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Azken kontsumoaren ticket-a PDF formatuan deskargatzen du.
        private void BtnTicketDeskargatu_Click(object sender, RoutedEventArgs e)
        {
            if (_azkenTicketLerroak == null ||
                _azkenTicketLerroak.Count == 0)
            {
                MessageBox.Show(
                    "Ez dago deskargatzeko ticket-ik. Lehenengo kontsumo bat amaitu behar duzu.",
                    "Ticket-a",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                bool pdfGordeta = SortuTicketPdf(
                    _azkenTicketLerroak,
                    _azkenTicketGuztira,
                    _azkenTicketData);

                if (pdfGordeta)
                {
                    MessageBox.Show(
                        "Ticket-a behar bezala deskargatu da.",
                        "Ticket-a gordeta",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea ticket-a sortzean:\n" + ex.Message,
                    "PDF errorea",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Kontsumoaren ticket-a PDF formatuan sortzen du.
        private bool SortuTicketPdf(
            List<KontsumoLerroa> lerroak,
            double guztira,
            DateTime data)
        {
            SaveFileDialog elkarrizketa = new SaveFileDialog
            {
                Title = "Ticket-a PDF formatuan gorde",
                Filter = "PDF fitxategiak (*.pdf)|*.pdf",
                FileName = "Ticket_" + data.ToString("yyyyMMdd_HHmmss"),
                DefaultExt = ".pdf",
                AddExtension = true,
                OverwritePrompt = true
            };

            // Erabiltzaileak gordetzeko kokapena aukeratzen du.
            if (elkarrizketa.ShowDialog(this) != true)
            {
                return false;
            }

            // QuestPDFren Community lizentzia erabiltzen du.
            QuestPDF.Settings.License =
                QuestPDF.Infrastructure.LicenseType.Community;

            Document.Create(dokumentua =>
            {
                dokumentua.Page(orria =>
                {
                    orria.Size(PdfHelpers.PageSizes.A4);
                    orria.Margin(40);
                    orria.PageColor(PdfColors.White);

                    orria.DefaultTextStyle(estiloa =>
                        estiloa.FontSize(11).FontFamily("Lato"));

                    orria.Content().Column(kontentua =>
                    {
                        kontentua.Spacing(12);

                        // Ticket-aren goiburua.
                        kontentua.Item()
                            .AlignCenter()
                            .Text("ELKARTEA_TPV")
                            .FontSize(25)
                            .Bold()
                            .FontColor(PdfColors.Green.Darken2);

                        kontentua.Item()
                            .AlignCenter()
                            .Text("KONTSUMOAREN TICKET-A")
                            .FontSize(14)
                            .SemiBold();

                        kontentua.Item()
                            .PaddingVertical(8)
                            .LineHorizontal(1)
                            .LineColor(PdfColors.Grey.Lighten2);

                        // Kontsumoaren datuak.
                        kontentua.Item().Text(
                            "Data: " + data.ToString("dd/MM/yyyy HH:mm"));

                        kontentua.Item().Text(
                            "Erabiltzailea: " + _erabiltzailea.ErabiltzaileIzena);

                        // Produktuen taula.
                        kontentua.Item().PaddingTop(15).Table(taula =>
                        {
                            taula.ColumnsDefinition(zutabeak =>
                            {
                                zutabeak.RelativeColumn(4);
                                zutabeak.RelativeColumn(1);
                                zutabeak.RelativeColumn(2);
                                zutabeak.RelativeColumn(2);
                            });

                            taula.Header(goiburua =>
                            {
                                goiburua.Cell().Text("Produktua").Bold();
                                goiburua.Cell().AlignCenter().Text("Kop.").Bold();
                                goiburua.Cell().AlignRight().Text("Prezioa").Bold();
                                goiburua.Cell().AlignRight().Text("Guztira").Bold();
                            });

                            foreach (KontsumoLerroa lerroa in lerroak)
                            {
                                taula.Cell().PaddingVertical(6)
                                    .Text(lerroa.Produktua);

                                taula.Cell().PaddingVertical(6)
                                    .AlignCenter()
                                    .Text(lerroa.Kantitatea.ToString());

                                taula.Cell().PaddingVertical(6)
                                    .AlignRight()
                                    .Text(lerroa.Prezioa.ToString("0.00") + " €");

                                taula.Cell().PaddingVertical(6)
                                    .AlignRight()
                                    .Text(lerroa.Guztira.ToString("0.00") + " €");
                            }
                        });

                        // Azken prezioa.
                        kontentua.Item()
                            .PaddingTop(15)
                            .LineHorizontal(1)
                            .LineColor(PdfColors.Grey.Lighten2);

                        kontentua.Item()
                            .AlignRight()
                            .Text("GUZTIRA: " + guztira.ToString("0.00") + " €")
                            .FontSize(20)
                            .Bold()
                            .FontColor(PdfColors.Green.Darken2);

                        // Ticket-aren amaierako mezua.
                        kontentua.Item()
                            .PaddingTop(30)
                            .AlignCenter()
                            .Text("Eskerrik asko!")
                            .FontSize(13)
                            .Italic();
                    });
                });
            }).GeneratePdf(elkarrizketa.FileName);

            return true;
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


        // Menu nagusira itzultzen da.
        private void BtnAtzera_Click(object sender, RoutedEventArgs e)
        {
            if (_kontsumoak.Count > 0)
            {
                MessageBoxResult erantzuna = MessageBox.Show(
                    "Kontsumoa amaitu gabe dago. Ziur zaude menura itzuli nahi duzula? Aukeratutako produktuak galduko dira.",
                    "Atzera",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (erantzuna != MessageBoxResult.Yes)
                    return;
            }

            // Lehendik dagoen menu nagusia bilatzen du.
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

            this.Close();
        }

    }
}
