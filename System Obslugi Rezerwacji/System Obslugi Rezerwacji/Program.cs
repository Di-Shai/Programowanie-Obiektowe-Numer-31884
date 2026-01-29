#nullable disable

using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SystemHotelowyMountainPeakResort
{
    public interface IWyszukiwalny
    {
        bool CzyPasujeDoWyszukiwania(string fraza);
    }
    public class PokojDef
    {
        public int Numer { get; set; }
        public string Standard { get; set; }
        public int MaxOsob { get; set; }

        public PokojDef(int nr, string std, int max) { Numer = nr; Standard = std; MaxOsob = max; }

        public Color PobierzKolorTla()
        {
            switch (Standard)
            {
                case "Standard": return Color.FromArgb(255, 255, 255);
                case "Premium": return Color.FromArgb(250, 252, 224);
                case "Apartament": return Color.FromArgb(224, 248, 252);
                default: return Color.White;
            }
        }
    }

    public abstract class Osoba
    {
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
        public string Adres { get; set; }

        public virtual string PobierzDaneEtykieta()
        {
            return $"{Imie} {Nazwisko}";
        }
    }

    public class Gosc : Osoba
    {
        public string Pesel { get; set; }
        public string NrDowodu { get; set; }
        public string NazwaFirmy { get; set; }
        public string NIP { get; set; }
        public string AdresFirmy { get; set; }
        public bool Parking { get; set; }
        public string NrRejestracyjny { get; set; }
        public string MarkaSamochodu { get; set; }
        public bool ChceFakture { get; set; }
        public override string PobierzDaneEtykieta()
        {
            if (!string.IsNullOrEmpty(NazwaFirmy))
            {
                return $"{Imie} {Nazwisko} (Firma: {NazwaFirmy})";
            }
            return base.PobierzDaneEtykieta();
        }
    }

    public class Obciazenie
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NazwaUslugi { get; set; }
        public decimal Kwota { get; set; }
        public decimal CenaJednostkowa { get; set; }
        public int Ilosc { get; set; } = 1;
        public bool CzyOplacone { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.Now;
    }

    public class DokumentPlik
    {
        public string NazwaWyswietlana { get; set; }
        public string TrescDokumentu { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;
    }

    public class Rezerwacja : IWyszukiwalny
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string KodRezerwacji { get; set; }
        public Gosc GoscGlowny { get; set; } = new Gosc();
        public DateTime DataOd { get; set; } = DateTime.Today;
        public DateTime DataDo { get; set; } = DateTime.Today.AddDays(1);
        public string NazwaPakietu { get; set; } = "Pobyt Indywidualny";

        public int NumerPokoju { get; set; }
        public string StandardPokoju { get; set; } = "Standard";
        public int IloscOsob { get; set; } = 2;
        public bool Sniadanie { get; set; }
        public bool Obiadokolacja { get; set; }
        public string Status { get; set; } = "REZERWACJA";

        public List<Obciazenie> Rachunek { get; set; } = new List<Obciazenie>();
        public List<DokumentPlik> PlikiDokumentow { get; set; } = new List<DokumentPlik>();
        public decimal WplaconaZaliczka { get; set; } = 0;

        public Rezerwacja()
        {
            Random r = new Random();
            KodRezerwacji = $"{r.Next(1000, 9999)}/{DateTime.Now.Year}/IND";
        }

        public bool Koliduje(DateTime start, DateTime end)
        {
            return (start < DataDo && end > DataOd);
        }
        public int DlugoscPobytu => (DataDo - DataOd).Days > 0 ? (DataDo - DataOd).Days : 1;

        public bool CzyPasujeDoWyszukiwania(string fraza)
        {
            if (string.IsNullOrEmpty(fraza)) return false;
            fraza = fraza.ToLower();

            return (GoscGlowny.Nazwisko != null && GoscGlowny.Nazwisko.ToLower().Contains(fraza)) ||
                   (GoscGlowny.Imie != null && GoscGlowny.Imie.ToLower().Contains(fraza)) ||
                   (KodRezerwacji != null && KodRezerwacji.ToLower().Contains(fraza));
        }
    }
    public static class BazaDanych
    {
        private static string sciezka = "hotel_db_v7.json";
        public static List<Rezerwacja> Rezerwacje { get; set; } = new List<Rezerwacja>();
        public static List<PokojDef> PokojeHotelowe { get; set; } = new List<PokojDef>();

        public static void Inicjalizuj()
        {
            GenerujPokoje();
            ZaladujRezerwacje();
        }

        private static void GenerujPokoje()
        {
            PokojeHotelowe.Clear();

            Random rnd = new Random();
            for (int i = 1; i <= 10; i++)
            {
                int maxOsob = rnd.Next(2, 5);
                PokojeHotelowe.Add(new PokojDef(100 + i, "Standard", maxOsob));
            }
            for (int i = 1; i <= 5; i++)
            {
                PokojeHotelowe.Add(new PokojDef(200 + i, "Premium", 3));
            }
            for (int i = 1; i <= 3; i++)
            {
                PokojeHotelowe.Add(new PokojDef(300 + i, "Apartament", 4));
            }
        }

        public static void ZaladujRezerwacje()
        {
            if (File.Exists(sciezka))
            {
                try
                {
                    string json = File.ReadAllText(sciezka);
                    Rezerwacje = JsonSerializer.Deserialize<List<Rezerwacja>>(json) ?? new List<Rezerwacja>();
                }
                catch { Rezerwacje = new List<Rezerwacja>(); }
            }
        }

        public static void Zapisz()
        {
            var opt = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(sciezka, JsonSerializer.Serialize(Rezerwacje, opt));
        }
    }
    public class MainForm : Form
    {
        private Dictionary<string, List<string>> aktywneFiltry = new Dictionary<string, List<string>>();
        private Panel panelInfo;
        private Label lblInfoHeader;
        private Label lblInfoBody;
        private Button btnInfoClose;
        private Panel sideMenu;
        private Panel contentPanel;

        private Panel viewDashboard;
        private Panel viewGrafik;
        private Panel viewRezerwacje;

        private DateTimePicker dtpPulpit;
        private Label lblPrzyjazdyTitle;
        private Label lblWyjazdyTitle;
        private Label lblDashboardDate;

        private DataGridView gridKalendarz;
        private DateTime viewStart = DateTime.Today.AddDays(-3);
        private int viewDays = 40;
        private Guid selectedReservationId = Guid.Empty;

        private Panel viewRaporty;
        private DataGridView gridRaporty;
        private DateTimePicker dtpRaport;
        private ComboBox cbFiltrStatus;
        private ComboBox cbFiltrStandard;
        private ComboBox cbFiltrPakiet;

        public MainForm()
        {
            this.Text = "System Hotelowy Mountain Peak Resort v7.0";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Font = new Font("Segoe UI", 9.5f);
            this.BackColor = Color.WhiteSmoke;

            BazaDanych.Inicjalizuj();
            InicjalizujInterfejs();
            PokazWidok("DASHBOARD");
        }
        private void InicjalizujInterfejs()
        {
            sideMenu = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(20, 30, 60) };

            var panelLogo = new Panel { Dock = DockStyle.Top, Height = 100 };
            var lblLogo = new Label
            {
                Text = "MOUNTAIN PEAK RESORT",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelLogo.Controls.Add(lblLogo);
            sideMenu.Controls.Add(panelLogo);

            var panelStopka = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                Padding = new Padding(10, 0, 10, 20)
            };
            var lblDaneAdresowe = new Label
            {
                Text = "MOUNTAIN PEAK RESORT\nUl. Wypoczynkowa 1, 81-000 Sopot\nNIP: 585-000-11-22\nTel: +48 58 555 00 00 | recepcja@mountainpeakresort.pl",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter
            };
            panelStopka.Controls.Add(lblDaneAdresowe);
            sideMenu.Controls.Add(panelStopka);
            panelStopka.BringToFront();

            DodajPrzyciskMenu("PULPIT", "DASHBOARD");
            DodajPrzyciskMenu("GRAFIK", "GRAFIK");
            DodajPrzyciskMenu("LISTA REZERWACJI", "REZERWACJE");

            DodajPrzyciskMenu("RAPORTY", "RAPORTY");

            contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.FromArgb(240, 242, 245) };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sideMenu);

            InitDashboard();
            InitGrafik();
            InitRezerwacje();
            InitRaporty();
        }
        private void DodajPrzyciskMenu(string text, string tag)
        {
            Button btn = new Button();
            btn.Text = "    " + text;
            btn.Tag = tag;
            btn.Dock = DockStyle.Top;
            btn.Height = 60;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 11);
            btn.Cursor = Cursors.Hand;
            btn.Click += (s, e) => PokazWidok(tag);

            sideMenu.Controls.Add(btn);

            btn.BringToFront();
        }
        private void PokazWidok(string tag)
        {
            contentPanel.Controls.Clear();
            switch (tag)
            {
                case "DASHBOARD":
                    OdswiezDashboard();
                    contentPanel.Controls.Add(viewDashboard);
                    break;
                case "GRAFIK":
                    RysujGrafik();
                    contentPanel.Controls.Add(viewGrafik);
                    break;
                case "REZERWACJE":
                    OdswiezListeRezerwacji();
                    contentPanel.Controls.Add(viewRezerwacje);
                    break;
                case "RAPORTY":
                    contentPanel.Controls.Add(viewRaporty);
                    break;
            }
        }

        private DataGridView gridPrzyjazdy;
        private DataGridView gridWyjazdy;
        private void InitDashboard()
        {
            viewDashboard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };

            lblDashboardDate = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 30, 60),
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top
            };

            var pnlControls = new Panel { Dock = DockStyle.Top, Height = 40 };
            dtpPulpit = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(160, 5), Width = 120 };
            dtpPulpit.ValueChanged += (s, e) => OdswiezDashboard();

            var btnDzis = new Button { Text = "Wróć do Dziś", Location = new Point(300, 4), AutoSize = true, BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
            btnDzis.Click += (s, e) => { dtpPulpit.Value = DateTime.Today; };

            pnlControls.Controls.Add(new Label { Text = "Wybierz dzień podglądu:", AutoSize = true, Location = new Point(0, 8), Font = new Font("Segoe UI", 10) });
            pnlControls.Controls.Add(dtpPulpit);
            pnlControls.Controls.Add(btnDzis);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };

            var pnlTop = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10) };
            lblPrzyjazdyTitle = new Label { Text = "PRZYJAZDY", Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Green, Height = 30 };

            gridPrzyjazdy = StworzGridDash();
            gridPrzyjazdy.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && gridPrzyjazdy.Rows[e.RowIndex].Cells["Id"].Value != null)
                {
                    Guid id = (Guid)gridPrzyjazdy.Rows[e.RowIndex].Cells["Id"].Value;
                    var r = BazaDanych.Rezerwacje.First(x => x.Id == id);
                    OtwórzZarządzanieRezerwacją(r);
                }
            };
            pnlTop.Controls.Add(gridPrzyjazdy);
            pnlTop.Controls.Add(lblPrzyjazdyTitle);

            var pnlBottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            lblWyjazdyTitle = new Label { Text = "WYJAZDY", Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Red, Height = 30 };

            gridWyjazdy = StworzGridDash();
            gridWyjazdy.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && gridWyjazdy.Rows[e.RowIndex].Cells["Id"].Value != null)
                {
                    Guid id = (Guid)gridWyjazdy.Rows[e.RowIndex].Cells["Id"].Value;
                    var r = BazaDanych.Rezerwacje.First(x => x.Id == id);
                    OtwórzZarządzanieRezerwacją(r);
                }
            };
            pnlBottom.Controls.Add(gridWyjazdy);
            pnlBottom.Controls.Add(lblWyjazdyTitle);

            split.Panel1.Controls.Add(pnlTop);
            split.Panel2.Controls.Add(pnlBottom);

            viewDashboard.Controls.Add(split);
            viewDashboard.Controls.Add(pnlControls);
            viewDashboard.Controls.Add(lblDashboardDate);
        }

        private DataGridView StworzGridDash()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            };


            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 235);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            g.ColumnHeadersHeight = 35;

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pokoj", HeaderText = "Pokój", Width = 80 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Numer", HeaderText = "Nr Rezerwacji", Width = 140 });

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Gosc", HeaderText = "Gość", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 200 });

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pakiet", HeaderText = "Pakiet", Width = 180 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Standard", HeaderText = "Standard", Width = 130 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Osoby", HeaderText = "Os.", Width = 60, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Termin", HeaderText = "Termin pobytu", Width = 150 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });

            return g;
        }

        private void OdswiezDashboard()
        {
            if (gridPrzyjazdy == null || gridWyjazdy == null) return;

            gridPrzyjazdy.Rows.Clear();
            gridWyjazdy.Rows.Clear();

            DateTime wybranaData = dtpPulpit != null ? dtpPulpit.Value.Date : DateTime.Today;

            if (lblDashboardDate != null)
                lblDashboardDate.Text = $"Podgląd dnia: {wybranaData:dd MMMM yyyy (dddd)}";

            if (lblPrzyjazdyTitle != null)
                lblPrzyjazdyTitle.Text = $"PRZYJAZDY W DNIU {wybranaData:dd.MM.yyyy}";

            if (lblWyjazdyTitle != null)
                lblWyjazdyTitle.Text = $"WYJAZDY W DNIU {wybranaData:dd.MM.yyyy}";

            var przyjazdy = BazaDanych.Rezerwacje.Where(x => x.Status == "REZERWACJA" && x.DataOd.Date == wybranaData);

            foreach (var r in przyjazdy)
            {
                gridPrzyjazdy.Rows.Add(
                    r.NumerPokoju,
                    r.KodRezerwacji,
                    $"{r.GoscGlowny.PobierzDaneEtykieta()}",
                    r.NazwaPakietu,
                    r.StandardPokoju,
                    r.IloscOsob,
                    $"{r.DataOd:dd.MM}-{r.DataDo:dd.MM}",
                    r.Id
                );
            }

            var wyjazdy = BazaDanych.Rezerwacje.Where(x => x.Status == "ZAMELDOWANY" && x.DataDo.Date == wybranaData);

            foreach (var r in wyjazdy)
            {
                gridWyjazdy.Rows.Add(
                    r.NumerPokoju,
                    r.KodRezerwacji,
                    $"{r.GoscGlowny.PobierzDaneEtykieta()}",
                    r.NazwaPakietu,
                    r.StandardPokoju,
                    r.IloscOsob,
                    $"{r.DataOd:dd.MM}-{r.DataDo:dd.MM}",
                    r.Id
                );
            }
        }

        private void InitGrafik()
        {
            viewGrafik = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };

            panelInfo = new Panel
            {
                Size = new Size(320, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            lblInfoHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Text = "Szczegóły"
            };

            btnInfoClose = new Button
            {
                Text = "✕",
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
                Cursor = Cursors.Hand
            };
            btnInfoClose.FlatAppearance.BorderSize = 0;
            btnInfoClose.FlatAppearance.MouseOverBackColor = Color.Red;
            btnInfoClose.Click += (s, e) => panelInfo.Visible = false;

            lblInfoHeader.Controls.Add(btnInfoClose);

            lblInfoBody = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(15, 10, 15, 10),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(64, 64, 64),
                Text = "Treść..."
            };

            panelInfo.Controls.Add(lblInfoBody);
            panelInfo.Controls.Add(lblInfoHeader);

            var btnPrev = new Button { Text = "<< Tydzień", Width = 100, Height = 30, Location = new Point(20, 10), BackColor = Color.WhiteSmoke };
            var btnNext = new Button { Text = "Tydzień >>", Width = 100, Height = 30, Location = new Point(130, 10), BackColor = Color.WhiteSmoke };
            var btnDzis = new Button { Text = "Dzisiaj", Width = 100, Height = 30, Location = new Point(250, 10), BackColor = Color.Gold, FlatStyle = FlatStyle.Flat };

            var lblSzukaj = new Label { Text = "Podaj numer rezerwacji:  ", Location = new Point(370, 16), AutoSize = true };
            TextBox txtSzukaj = new TextBox { Location = new Point(525, 13), Width = 150 };

            txtSzukaj.TextChanged += (s, e) =>
            {
                string fraza = txtSzukaj.Text.Trim();

                if (string.IsNullOrEmpty(fraza))
                {
                    selectedReservationId = Guid.Empty;
                }
                else
                {
                    var znaleziona = BazaDanych.Rezerwacje.FirstOrDefault(r =>
                        r.Status != "ANULOWANA" && r.CzyPasujeDoWyszukiwania(fraza)
                    );

                    if (znaleziona != null)
                    {
                        selectedReservationId = znaleziona.Id;
                    }
                    else
                    {
                        selectedReservationId = Guid.Empty;
                    }
                }
                gridKalendarz.Invalidate();
            };

            Button btnIdzDo = new Button { Text = "Szukaj", Location = new Point(690, 10), Width = 100, Height = 30, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnIdzDo.Padding = new Padding(0);
            btnIdzDo.Click += (s, e) =>
            {
                if (selectedReservationId != Guid.Empty)
                {
                    var rez = BazaDanych.Rezerwacje.First(x => x.Id == selectedReservationId);

                    viewStart = rez.DataOd.AddDays(-2);
                    RysujGrafik();

                    foreach (DataGridViewRow row in gridKalendarz.Rows)
                    {
                        if (row.Tag != null && (int)row.Tag == rez.NumerPokoju)
                        {
                            gridKalendarz.FirstDisplayedScrollingRowIndex = row.Index;
                            gridKalendarz.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Najpierw wpisz nazwisko lub kod, aby znaleźć rezerwację.");
                }
            };
            panelTop.Controls.Add(btnIdzDo);

            panelTop.Controls.Add(lblSzukaj);
            panelTop.Controls.Add(txtSzukaj);

            btnPrev.Click += (s, e) => { viewStart = viewStart.AddDays(-7); RysujGrafik(); panelInfo.Visible = false; };
            btnNext.Click += (s, e) => { viewStart = viewStart.AddDays(7); RysujGrafik(); panelInfo.Visible = false; };
            btnDzis.Click += (s, e) =>
            {
                viewStart = DateTime.Today.AddDays(-3);
                RysujGrafik();
                gridKalendarz.ClearSelection();
                gridKalendarz.CurrentCell = null;
                panelInfo.Visible = false;
            };

            panelTop.Controls.AddRange(new Control[] { btnPrev, btnNext, btnDzis });

            gridKalendarz = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersWidth = 100,
                ColumnHeadersHeight = 50,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                ScrollBars = ScrollBars.Both,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.CellSelect
            };

            gridKalendarz.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 50, 70);
            gridKalendarz.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            gridKalendarz.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gridKalendarz.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 50, 70);
            gridKalendarz.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridKalendarz.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gridKalendarz.EnableHeadersVisualStyles = false;

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, gridKalendarz, new object[] { true });

            gridKalendarz.CellPainting += GridKalendarz_CellPainting;
            gridKalendarz.CellMouseClick += GridKalendarz_Click;
            gridKalendarz.RowPostPaint += GridKalendarz_RowPostPaint;
            gridKalendarz.Scroll += (s, e) => panelInfo.Visible = false;

            viewGrafik.Controls.Add(panelInfo);
            viewGrafik.Controls.Add(gridKalendarz);
            viewGrafik.Controls.Add(panelTop);

            panelInfo.BringToFront();
        }

        private void RysujGrafik()
        {
            gridKalendarz.Columns.Clear();
            gridKalendarz.Rows.Clear();

            for (int i = 0; i < viewDays; i++)
            {
                DateTime d = viewStart.AddDays(i);
                var col = gridKalendarz.Columns.Add("d" + i, $"{d.Day:00}.{d.Month:00}\n{d.ToString("ddd")}");
                gridKalendarz.Columns[col].Width = 50;
                gridKalendarz.Columns[col].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                if (d.Date == DateTime.Today)
                {
                    gridKalendarz.Columns[col].HeaderCell.Style.BackColor = Color.Gold;
                    gridKalendarz.Columns[col].HeaderCell.Style.ForeColor = Color.Black;
                    gridKalendarz.Columns[col].HeaderCell.Style.SelectionBackColor = Color.Gold;
                }
                else if (d.Date < DateTime.Today)
                {
                    gridKalendarz.Columns[col].HeaderCell.Style.BackColor = Color.DimGray;
                    gridKalendarz.Columns[col].HeaderCell.Style.ForeColor = Color.WhiteSmoke;
                }
                else if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    gridKalendarz.Columns[col].HeaderCell.Style.BackColor = Color.SlateGray;
                }
            }

            foreach (var p in BazaDanych.PokojeHotelowe)
            {
                int idx = gridKalendarz.Rows.Add();
                gridKalendarz.Rows[idx].HeaderCell.Value = $"{p.Numer}\n{p.Standard}";
                gridKalendarz.Rows[idx].Tag = p.Numer;
                gridKalendarz.Rows[idx].Height = 50;
                gridKalendarz.Rows[idx].DefaultCellStyle.BackColor = p.PobierzKolorTla();
            }
            gridKalendarz.ClearSelection();
            gridKalendarz.CurrentCell = null;
        }

        private void TworzRezerwacjeZaznaczenia()
        {
            var wybrane = gridKalendarz.SelectedCells;
            if (wybrane.Count == 0) return;

            int indexWiersza = wybrane[0].RowIndex;

            if (indexWiersza < 0 || indexWiersza >= BazaDanych.PokojeHotelowe.Count) return;

            var wybranyPokoj = BazaDanych.PokojeHotelowe[indexWiersza];
            var kolumny = wybrane.Cast<DataGridViewCell>().Select(c => c.ColumnIndex).ToList();
            DateTime dataOd = viewStart.AddDays(kolumny.Min());
            DateTime dataDo = viewStart.AddDays(kolumny.Max() + 1);

            var nowaRez = new Rezerwacja
            {
                DataOd = dataOd,
                DataDo = dataDo,
                NumerPokoju = wybranyPokoj.Numer,
                StandardPokoju = wybranyPokoj.Standard,
                GoscGlowny = new Gosc(),
                Status = "REZERWACJA",
                NazwaPakietu = "Pobyt Indywidualny"
            };
            OknoRezerwacji(nowaRez);
            RysujGrafik();
        }

        private void GridKalendarz_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            int nrPokoju = (int)gridKalendarz.Rows[e.RowIndex].Tag;
            DateTime data = viewStart.AddDays(e.ColumnIndex);
            bool toPrzeszlosc = data < DateTime.Today;
            bool toDzisiaj = data.Date == DateTime.Today;

            e.PaintBackground(e.CellBounds, true);

            if (toPrzeszlosc)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(100, 200, 200, 200)))
                    e.Graphics.FillRectangle(b, e.CellBounds);
            }
            else if (toDzisiaj)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(60, 255, 215, 0)))
                    e.Graphics.FillRectangle(b, e.CellBounds);
            }

            var rezerwacje = BazaDanych.Rezerwacje
                .Where(r => r.NumerPokoju == nrPokoju && r.Status != "ANULOWANA" &&
                            data.Date >= r.DataOd.Date && data.Date <= r.DataDo.Date)
                .GroupBy(r => r.Id).Select(g => g.First())
                .ToList();

            foreach (var rez in rezerwacje)
            {
                bool isStart = (data.Date == rez.DataOd.Date);
                bool isEnd = (data.Date == rez.DataDo.Date);
                if (isStart && isEnd) continue;

                int marginesV = 5;
                float gap = 2.0f;

                float szer = e.CellBounds.Width;
                float wys = e.CellBounds.Height - (marginesV * 2);
                RectangleF rectF = new RectangleF(e.CellBounds.X, e.CellBounds.Y + marginesV, szer, wys);

                if (isStart)
                {
                    rectF.X += (szer / 2) + gap;
                    rectF.Width = (szer / 2) - gap;
                }
                else if (isEnd)
                {
                    rectF.Width = (szer / 2) - gap;
                }

                if (!isStart) { rectF.X -= 1; rectF.Width += 1; }
                if (!isEnd) { rectF.Width += 1; }

                Color kolorTla;
                if (rez.Status == "WYMELDOWANY") kolorTla = Color.LightGray;
                else
                {
                    switch (rez.NazwaPakietu)
                    {
                        case "Pakiet Sylwestrowy": kolorTla = Color.Gold; break;
                        case "Pakiet SPA": kolorTla = Color.LightPink; break;
                        default: kolorTla = Color.SkyBlue; break;
                    }
                }
                if (rez.Id == selectedReservationId) kolorTla = Color.Orange;

                using (Brush b = new SolidBrush(kolorTla))
                    e.Graphics.FillRectangle(b, rectF);

                Color ramkaColor = (rez.Id == selectedReservationId) ? Color.Red : Color.FromArgb(120, 0, 0, 0);
                float ramkaSize = (rez.Id == selectedReservationId) ? 2.5f : 1.0f;
                using (Pen p = new Pen(ramkaColor, ramkaSize))
                {
                    e.Graphics.DrawLine(p, rectF.Left, rectF.Top, rectF.Right, rectF.Top);
                    e.Graphics.DrawLine(p, rectF.Left, rectF.Bottom, rectF.Right, rectF.Bottom);
                    if (isStart) e.Graphics.DrawLine(p, rectF.Left, rectF.Top, rectF.Left, rectF.Bottom);
                    if (isEnd) e.Graphics.DrawLine(p, rectF.Right, rectF.Top, rectF.Right, rectF.Bottom);
                }

                float szerokoscWizualna = rez.DlugoscPobytu <= 1 ? szer : (rez.DlugoscPobytu * szer);

                string imie = rez.GoscGlowny.Imie.Trim();
                string nazwisko = rez.GoscGlowny.Nazwisko.Trim();
                string i1 = imie.Length > 0 ? imie.Substring(0, 1) : "";
                string n1 = nazwisko.Length > 0 ? nazwisko.Substring(0, 1) : "";

                string wariantPelny = $"{imie} {nazwisko}";
                string wariantSredni = $"{imie} {n1}.";
                string wariantInicjaly = $"{i1}.{n1}.";

                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using (Font czcionka = new Font("Segoe UI", 8.5f, FontStyle.Regular))
                {
                    int dostepneMiejsce = (int)szerokoscWizualna - 4;

                    float lenPelny = e.Graphics.MeasureString(wariantPelny, czcionka).Width;
                    float lenSredni = e.Graphics.MeasureString(wariantSredni, czcionka).Width;

                    string tekstDoWyswietlenia;
                    if (lenPelny <= dostepneMiejsce) tekstDoWyswietlenia = wariantPelny;
                    else if (lenSredni <= dostepneMiejsce) tekstDoWyswietlenia = wariantSredni;
                    else tekstDoWyswietlenia = wariantInicjaly;

                    int dniOdStartu = (data.Date - rez.DataOd.Date).Days;
                    float startPaskaX = e.CellBounds.X - (dniOdStartu * szer);
                    startPaskaX += (szer / 2);

                    RectangleF rectCalyPasek = new RectangleF(
                        startPaskaX,
                        e.CellBounds.Y + marginesV,
                        szerokoscWizualna,
                        wys
                    );

                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                        sf.Trimming = StringTrimming.EllipsisCharacter;

                        using (Brush brushTekst = new SolidBrush(Color.FromArgb(30, 30, 30)))
                        {
                            e.Graphics.DrawString(tekstDoWyswietlenia, czcionka, brushTekst, rectCalyPasek, sf);
                        }
                    }
                }
            }

            e.Handled = true;
        }

        private void GridKalendarz_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (gridKalendarz.Rows[e.RowIndex].Tag == null) return;
            int numerPokoju = (int)gridKalendarz.Rows[e.RowIndex].Tag;

            string opisStandardu = "Standard";
            if (numerPokoju >= 200 && numerPokoju < 300) opisStandardu = "Premium";
            else if (numerPokoju >= 300) opisStandardu = "Apartament";

            Rectangle rowHeaderBounds = new Rectangle(
                e.RowBounds.Left, e.RowBounds.Top,
                gridKalendarz.RowHeadersWidth, e.RowBounds.Height
            );

            using (Brush backBrush = new SolidBrush(Color.FromArgb(40, 50, 70)))
            {
                e.Graphics.FillRectangle(backBrush, rowHeaderBounds);
            }

            using (Pen p = new Pen(Color.Gray))
            {
                e.Graphics.DrawLine(p, rowHeaderBounds.Left, rowHeaderBounds.Bottom - 1, rowHeaderBounds.Right, rowHeaderBounds.Bottom - 1);
            }

            using (Font fontNr = new Font("Segoe UI", 11, FontStyle.Bold))
            using (Brush brushNr = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(
                    numerPokoju.ToString(),
                    fontNr,
                    brushNr,
                    rowHeaderBounds.Left + 4,
                    rowHeaderBounds.Top + 4
                );
            }

            using (Font fontStd = new Font("Segoe UI", 11, FontStyle.Bold))
            using (Brush brushStd = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(
                    opisStandardu,
                    fontStd,
                    brushStd,
                    rowHeaderBounds.Left + 4,
                    rowHeaderBounds.Top + 26
                );
            }
        }

        private void GridKalendarz_Click(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                panelInfo.Visible = false;
                return;
            }

            int nr = (int)gridKalendarz.Rows[e.RowIndex].Tag;
            DateTime dt = viewStart.AddDays(e.ColumnIndex);

            var kandydaci = BazaDanych.Rezerwacje.Where(x =>
                x.NumerPokoju == nr &&
                x.Status != "ANULOWANA" &&
                dt.Date >= x.DataOd.Date && dt.Date <= x.DataDo.Date
            ).ToList();

            Rezerwacja r = null;

            if (kandydaci.Count == 0)
            {
                r = null;
            }
            else if (kandydaci.Count == 1)
            {
                r = kandydaci[0];
            }
            else
            {

                int szerokoscKomorki = gridKalendarz.Columns[e.ColumnIndex].Width;
                bool klikLewaPolowa = e.X < (szerokoscKomorki / 2);

                if (klikLewaPolowa)
                {
                    r = kandydaci.FirstOrDefault(x => x.DataDo.Date == dt.Date);

                    if (r == null) r = kandydaci[0];
                }
                else
                {
                    r = kandydaci.FirstOrDefault(x => x.DataOd.Date == dt.Date);

                    if (r == null) r = kandydaci.Last();
                }
            }

            if (r != null)
            {
                if (selectedReservationId != r.Id)
                {
                    selectedReservationId = r.Id;
                    gridKalendarz.Invalidate();
                }

                if (e.Button == MouseButtons.Left)
                {
                    Color headerColor = Color.FromArgb(40, 50, 70);
                    if (r.NazwaPakietu == "Pakiet Sylwestrowy") headerColor = Color.Goldenrod;
                    if (r.NazwaPakietu == "Pakiet SPA") headerColor = Color.HotPink;
                    if (r.Status == "ZAMELDOWANY") headerColor = Color.Teal;

                    lblInfoHeader.BackColor = headerColor;
                    lblInfoHeader.Text = $"{r.GoscGlowny.Imie} {r.GoscGlowny.Nazwisko}";

                    string czyFaktura = r.GoscGlowny.ChceFakture ? $"TAK" : "NIE";

                    decimal calosc = r.Rachunek.Sum(x => x.Kwota);

                    lblInfoBody.Text = $"📅 Termin: {r.DataOd:dd.MM.yyyy} - {r.DataDo:dd.MM.yyyy}\n" +
                                       $"⏳ Długość: {r.DlugoscPobytu} noc(y)\n\n" +
                                       $"📦 Pakiet: {r.NazwaPakietu}\n" +
                                       $"👥 Osób: {r.IloscOsob}\n" +
                                       $"📄 Faktura: {czyFaktura}\n\n";

                    Point myszka = gridKalendarz.PointToClient(Cursor.Position);
                    int x = myszka.X + 20;
                    int y = myszka.Y + 10;

                    if (x + panelInfo.Width > gridKalendarz.Width) x = myszka.X - panelInfo.Width - 10;
                    if (y + panelInfo.Height > gridKalendarz.Height) y = myszka.Y - panelInfo.Height - 10;

                    panelInfo.Location = new Point(x, y);
                    panelInfo.Visible = true;
                    panelInfo.BringToFront();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    panelInfo.Visible = false;

                    var cms = new ContextMenuStrip();

                    cms.Items.Add("Edytuj Rezerwację", null, (s, ev) => { OknoRezerwacji(r); RysujGrafik(); });
                    cms.Items.Add("-");
                    cms.Items.Add("Płatności", null, (s, ev) => OknoFinansowKlienta(r));
                    cms.Items.Add("-");
                    cms.Items.Add("Wyślij Potwierdzenie", null, (s, ev) => OknoEmail(r));
                    cms.Items.Add("-");
                    if (r.Status == "REZERWACJA") cms.Items.Add("Zamelduj", null, (s, ev) => { OknoMeldunku(r); RysujGrafik(); });
                    if (r.Status == "ZAMELDOWANY") cms.Items.Add("Wymelduj", null, (s, ev) => { ProceduraWymeldowania(r); RysujGrafik(); });

                    cms.Show(Cursor.Position);
                }
            }
            else
            {
                panelInfo.Visible = false;
                if (selectedReservationId != Guid.Empty)
                {
                    selectedReservationId = Guid.Empty;
                    gridKalendarz.Invalidate();
                }

                else if (e.Button == MouseButtons.Right)
                {
                    panelInfo.Visible = false;

                    var cms = new ContextMenuStrip();

                    int ileZaznaczono = gridKalendarz.SelectedCells.Count;

                    if (ileZaznaczono > 1)
                    {
                        var kolumny = gridKalendarz.SelectedCells.Cast<DataGridViewCell>().Select(c => c.ColumnIndex).ToList();
                        DateTime d1 = viewStart.AddDays(kolumny.Min());
                        DateTime d2 = viewStart.AddDays(kolumny.Max());

                        cms.Items.Add($"Nowa rezerwacja ({d1:dd.MM} - {d2:dd.MM})", null, (s, ev) =>
                        {
                            TworzRezerwacjeZaznaczenia();
                        });
                    }
                    else
                    {
                        cms.Items.Add("Nowa rezerwacja tutaj", null, (s, ev) =>
                        {
                            var nowa = new Rezerwacja
                            {
                                DataOd = dt,
                                DataDo = dt.AddDays(1),
                                NumerPokoju = nr,
                                GoscGlowny = new Gosc()
                            };
                            OknoRezerwacji(nowa);
                            RysujGrafik();
                        });
                    }

                    cms.Show(Cursor.Position);
                }
            }
        }

        private DataGridView gridListaRezerwacji;

        private void InitRezerwacje()
        {
            viewRezerwacje = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Color.White };

            var btnDodaj = new Button
            {
                Text = "NOWA REZERWACJA",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Height = 40,
                Width = 180,
                Location = new Point(0, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnDodaj.Click += (s, e) => { OknoRezerwacji(null); OdswiezListeRezerwacji(); };

            var lblSort = new Label { Text = "Sortuj wg:", Location = new Point(198, 18), AutoSize = true };
            var cmbSort = new ComboBox
            {
                Location = new Point(265, 15),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSort.Items.AddRange(new object[] { "Data przyjazdu", "Alfabetycznie", "Numer pokoju", "Pakiet", "Status" });
            cmbSort.SelectedIndex = 0;

            var chkRosnaco = new CheckBox
            {
                Text = "Rosnąco",
                Checked = true,
                Location = new Point(425, 17),
                AutoSize = true
            };

            var lblFiltruj = new Label
            {
                Text = "Pokaż statusy:",
                Location = new Point(0, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            string[] statusy = { "REZERWACJA", "ZAMELDOWANY", "WYMELDOWANY", "ANULOWANA" };
            int posX = 110;
            foreach (var st in statusy)
            {
                var chkStatus = new CheckBox
                {
                    Text = st,
                    Tag = st,
                    Checked = true,
                    Location = new Point(posX, 74),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9)
                };
                chkStatus.CheckedChanged += (s, e) => OdswiezListeRezerwacji(cmbSort.Text, chkRosnaco.Checked);
                pnlTop.Controls.Add(chkStatus);
                posX += 135;
            }

            cmbSort.SelectedIndexChanged += (s, e) => OdswiezListeRezerwacji(cmbSort.Text, chkRosnaco.Checked);
            chkRosnaco.CheckedChanged += (s, e) => OdswiezListeRezerwacji(cmbSort.Text, chkRosnaco.Checked);

            pnlTop.Controls.AddRange(new Control[] { btnDodaj, lblSort, cmbSort, chkRosnaco, lblFiltruj });

            gridListaRezerwacji = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 20, 0, 0)
            };

            ContextMenuStrip menuPrawy = new ContextMenuStrip();
            var itemPokazNaGrafiku = new ToolStripMenuItem { Text = "Pokaż na grafiku" };

            itemPokazNaGrafiku.Font = new Font("Segoe UI", 8.5f);

            itemPokazNaGrafiku.Click += (s, e) =>
            {
                if (gridListaRezerwacji.CurrentRow != null)
                {
                    string kod = gridListaRezerwacji.CurrentRow.Cells["Kod"].Value.ToString();
                    var rez = BazaDanych.Rezerwacje.FirstOrDefault(x => x.KodRezerwacji == kod);

                    if (rez != null && rez.Status != "ANULOWANA")
                    {
                        selectedReservationId = rez.Id;

                        PokazWidok("GRAFIK");

                        viewStart = rez.DataOd.AddDays(-2);
                        RysujGrafik();

                        foreach (DataGridViewRow row in gridKalendarz.Rows)
                        {
                            if (row.Tag != null && (int)row.Tag == rez.NumerPokoju)
                            {
                                gridKalendarz.FirstDisplayedScrollingRowIndex = row.Index;
                                break;
                            }
                        }
                    }
                    else if (rez?.Status == "ANULOWANA")
                    {
                        MessageBox.Show("Nie można pokazać anulowanej rezerwacji na grafiku.");
                    }
                }
            };
            menuPrawy.Items.Add(itemPokazNaGrafiku);
            gridListaRezerwacji.ContextMenuStrip = menuPrawy;

            gridListaRezerwacji.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = gridListaRezerwacji.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        gridListaRezerwacji.ClearSelection();
                        gridListaRezerwacji.Rows[hit.RowIndex].Selected = true;
                        gridListaRezerwacji.CurrentCell = gridListaRezerwacji.Rows[hit.RowIndex].Cells[0];
                    }
                }
            };

            gridListaRezerwacji.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var row = gridListaRezerwacji.Rows[e.RowIndex];
                    var status = row.Cells["Status"].Value?.ToString();

                    if (status == "ZAMELDOWANY") row.DefaultCellStyle.BackColor = Color.LightGreen;
                    else if (status == "REZERWACJA") row.DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                    else if (status == "ANULOWANA") row.DefaultCellStyle.ForeColor = Color.Red;
                }
            };

            gridListaRezerwacji.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    string kod = gridListaRezerwacji.Rows[e.RowIndex].Cells["Kod"].Value.ToString();
                    var rez = BazaDanych.Rezerwacje.FirstOrDefault(x => x.KodRezerwacji == kod);
                    if (rez != null)
                    {
                        OtwórzZarządzanieRezerwacją(rez);
                        OdswiezListeRezerwacji();
                    }
                }
            };

            viewRezerwacje.Controls.Add(gridListaRezerwacji);
            viewRezerwacje.Controls.Add(pnlTop);

            OdswiezListeRezerwacji();
        }

        private void OdswiezListeRezerwacji(string kryterium = "Data przyjazdu", bool rosnaco = true)
        {
            List<string> wybraneStatusy = new List<string>();
            foreach (Control c in viewRezerwacje.Controls)
            {
                if (c is Panel pTop)
                {
                    foreach (Control subC in pTop.Controls)
                    {
                        if (subC is CheckBox chk && chk.Tag != null)
                        {
                            if (chk.Checked) wybraneStatusy.Add(chk.Tag.ToString());
                        }
                    }
                }
            }

            var lista = BazaDanych.Rezerwacje.Where(r => wybraneStatusy.Contains(r.Status)).ToList();

            switch (kryterium)
            {
                case "Data przyjazdu":
                    lista = rosnaco ? lista.OrderBy(r => r.DataOd).ToList() : lista.OrderByDescending(r => r.DataOd).ToList();
                    break;
                case "Alfabetycznie":
                    lista = rosnaco ? lista.OrderBy(r => r.GoscGlowny.Imie).ThenBy(r => r.GoscGlowny.Nazwisko).ToList()
                                    : lista.OrderByDescending(r => r.GoscGlowny.Imie).ThenByDescending(r => r.GoscGlowny.Nazwisko).ToList();
                    break;
                case "Numer pokoju":
                    lista = rosnaco ? lista.OrderBy(r => r.NumerPokoju).ToList() : lista.OrderByDescending(r => r.NumerPokoju).ToList();
                    break;
                case "Pakiet":
                    lista = rosnaco ? lista.OrderBy(r => r.NazwaPakietu).ToList() : lista.OrderByDescending(r => r.NazwaPakietu).ToList();
                    break;
                case "Status":
                    lista = rosnaco ? lista.OrderBy(r => r.Status).ToList() : lista.OrderByDescending(r => r.Status).ToList();
                    break;
            }

            gridListaRezerwacji.DataSource = null;
            gridListaRezerwacji.DataSource = lista.Select(r => new
            {
                Kod = r.KodRezerwacji,
                Nr = r.NumerPokoju,
                Gość = r.GoscGlowny.Imie + " " + r.GoscGlowny.Nazwisko,
                Od = r.DataOd.ToShortDateString(),
                Do = r.DataDo.ToShortDateString(),
                Status = r.Status,
                Pakiet = r.NazwaPakietu
            }).ToList();
        }

        private void OtwórzZarządzanieRezerwacją(Rezerwacja r)
        {
            Form f = new Form
            {
                Text = $"Zarządzanie: {r.KodRezerwacji} - {r.GoscGlowny.Nazwisko}",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            FlowLayoutPanel pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown };

            var lblInfo = new Label
            {
                Text = $"Aktualny status: {r.Status}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            pnl.Controls.Add(lblInfo);

            var btnEdytuj = new Button { Text = "EDYTUJ DANE I POBYT", Width = 440, Height = 50, BackColor = Color.LightBlue, FlatStyle = FlatStyle.Flat };
            btnEdytuj.Click += (s, e) => { f.Close(); OknoRezerwacji(r); };

            var btnRachunek = new Button { Text = "DODAJ OBCIĄŻENIA / RACHUNEK", Width = 440, Height = 50, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnRachunek.Click += (s, e) => { OknoFinansowKlienta(r); };

            var grpStatus = new GroupBox { Text = "Zmień status rezerwacji", Width = 440, Height = 220, Margin = new Padding(0, 20, 0, 0) };
            var flowStatus = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var btnZamelduj = new Button
            {
                Text = "ZAMELDUJ",
                Width = 190,
                Height = 40,
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Enabled = (r.Status == "REZERWACJA")
            };

            btnZamelduj.Click += (s, e) =>
            {
                OknoMeldunku(r);
            };

            var btnWymelduj = new Button
            {
                Text = "WYMELDUJ",
                Width = 190,
                Height = 40,
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Enabled = (r.Status == "ZAMELDOWANY")
            };

            btnWymelduj.Click += (s, e) =>
            {
                ProceduraWymeldowania(r);
            };

            var btnAnuluj = new Button { Text = "ANULUJ REZERWACJĘ", Width = 390, Height = 40, BackColor = Color.Salmon, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 10, 0, 0) };
            btnAnuluj.Click += (s, e) =>
            {
                if (MessageBox.Show("Czy na pewno anulować tę rezerwację?\nZniknie ona z grafiku, ale pozostanie w bazie danych.", "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    r.Status = "ANULOWANA";
                    BazaDanych.Zapisz();
                    f.Close();
                }
            };

            flowStatus.Controls.AddRange(new Control[] { btnZamelduj, btnWymelduj, btnAnuluj });
            grpStatus.Controls.Add(flowStatus);
            pnl.Controls.AddRange(new Control[] { btnEdytuj, btnRachunek, grpStatus });

            var btnZamknij = new Button { Text = "POWRÓT", Width = 440, Height = 40, Dock = DockStyle.Bottom };
            btnZamknij.Click += (s, e) => f.Close();

            f.Controls.Add(pnl);
            f.Controls.Add(btnZamknij);
            f.ShowDialog();

            RysujGrafik();
            OdswiezDashboard();
            OdswiezListeRezerwacji();
        }

        private void OknoRezerwacji(Rezerwacja r)
        {
            bool nowa = (r == null);
            if (nowa) r = new Rezerwacja();

            Form f = new Form
            {
                Text = $"Rezerwacja {r.KodRezerwacji}",
                Size = new Size(1100, 800),
                StartPosition = FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.None
            };

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            Panel lewaKolumna = new Panel { Dock = DockStyle.Fill };

            var grpDane = new GroupBox { Text = "Dane Gościa", Dock = DockStyle.Top, Height = 345, Padding = new Padding(10) };
            var flowDane = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            DodajInput(flowDane, "Imię:", r.GoscGlowny.Imie, v => r.GoscGlowny.Imie = v);
            DodajInput(flowDane, "Nazwisko:", r.GoscGlowny.Nazwisko, v => r.GoscGlowny.Nazwisko = v);
            DodajInput(flowDane, "PESEL:", r.GoscGlowny.Pesel, v => r.GoscGlowny.Pesel = v);
            DodajInput(flowDane, "Email:", r.GoscGlowny.Email, v => r.GoscGlowny.Email = v);
            DodajInput(flowDane, "Ulica/Miasto:", r.GoscGlowny.Adres, v => r.GoscGlowny.Adres = v);
            grpDane.Controls.Add(flowDane);

            var grpFaktura = new GroupBox { Text = "Dane do faktury", Dock = DockStyle.Top, Height = 250, AutoSize = true, Padding = new Padding(10), Margin = new Padding(0, 10, 0, 0) };
            var flowFaktura = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            var chkChceFakture = new CheckBox { Text = "Wystawić fakturę VAT", Checked = r.GoscGlowny.ChceFakture, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 0, 0, 10) };
            var flowPolaFaktury = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, Width = 450, AutoSize = true, Enabled = r.GoscGlowny.ChceFakture, Padding = new Padding(5, 0, 0, 0) };
            DodajInput(flowPolaFaktury, "Nazwa Firmy:", r.GoscGlowny.NazwaFirmy, v => r.GoscGlowny.NazwaFirmy = v);
            DodajInput(flowPolaFaktury, "NIP:", r.GoscGlowny.NIP, v => r.GoscGlowny.NIP = v);
            DodajInput(flowPolaFaktury, "Adres Firmy:", r.GoscGlowny.AdresFirmy, v => r.GoscGlowny.AdresFirmy = v);
            chkChceFakture.CheckedChanged += (s, e) =>
            {
                flowPolaFaktury.Enabled = chkChceFakture.Checked;
                r.GoscGlowny.ChceFakture = chkChceFakture.Checked;
            };
            flowFaktura.Controls.Add(chkChceFakture);
            flowFaktura.Controls.Add(flowPolaFaktury);
            grpFaktura.Controls.Add(flowFaktura);

            var grpDodatki = new GroupBox { Text = "Usługi Dodatkowe", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(10, 15, 10, 10), Margin = new Padding(0, 10, 0, 0) };
            var flowDodatki = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            var chkZwierze = new CheckBox { Text = "Pobyt zwierzęcia (100 zł / doba)", AutoSize = true };
            var chkWstawka = new CheckBox { Text = "Wstawka powitalna: wino + owoce (150 zł)", AutoSize = true };

            flowDodatki.Controls.Add(chkZwierze);
            flowDodatki.Controls.Add(chkWstawka);
            grpDodatki.Controls.Add(flowDodatki);

            lewaKolumna.Controls.Add(grpDodatki);
            lewaKolumna.Controls.Add(grpFaktura);
            lewaKolumna.Controls.Add(grpDane);
            grpDodatki.BringToFront();

            var grpRez = new GroupBox { Text = "Szczegóły Pobytu", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var p = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };

            p.Controls.Add(new Label { Text = "Data od:", Margin = new Padding(0, 10, 0, 0) });
            var dtOd = new DateTimePicker { Value = (r.DataOd < DateTime.Today && nowa) ? DateTime.Today : r.DataOd, Width = 250 };
            if (nowa) dtOd.MinDate = DateTime.Today;
            p.Controls.Add(dtOd);

            p.Controls.Add(new Label { Text = "Data do:" });
            var dtDo = new DateTimePicker { Value = r.DataDo, Width = 250 };
            if (nowa) dtDo.MinDate = DateTime.Today.AddDays(-1);
            p.Controls.Add(dtDo);

            if (nowa)
            {
                dtDo.MinDate = r.DataOd;
            }

            p.Controls.Add(dtDo);

            p.Controls.Add(new Label { Text = "Pakiet:" });
            var cmbPakiet = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPakiet.Items.AddRange(new object[] { "Pobyt Indywidualny", "Pakiet SPA", "Pakiet Sylwestrowy" });
            cmbPakiet.SelectedItem = r.NazwaPakietu ?? "Pobyt Indywidualny";
            p.Controls.Add(cmbPakiet);

            var chkObiad = new CheckBox { Text = "Obiadokolacja (119 zł)", Checked = r.Obiadokolacja, AutoSize = true };
            p.Controls.Add(chkObiad);

            cmbPakiet.SelectedIndexChanged += (s, e) =>
            {
                if (cmbPakiet.Text != "Pobyt Indywidualny") { chkObiad.Checked = true; chkObiad.Enabled = false; chkObiad.Text = "Obiadokolacja (w cenie)"; }
                else { chkObiad.Enabled = true; chkObiad.Text = "Obiadokolacja (119 zł)"; }
            };

            p.Controls.Add(new Label { Text = "Liczba osób:" });
            var numOsob = new NumericUpDown { Value = r.IloscOsob > 0 ? r.IloscOsob : 1, Minimum = 1, Maximum = 6, Width = 100 };
            p.Controls.Add(numOsob);

            p.Controls.Add(new Label { Text = "Standard pokoju:" });
            var cmbStd = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStd.Items.AddRange(new object[] { "Standard", "Premium", "Apartament" });
            cmbStd.SelectedItem = r.StandardPokoju ?? "Standard";
            p.Controls.Add(cmbStd);

            var lblPrzydzielony = new Label { Text = "Pokój: " + (r.NumerPokoju > 0 ? r.NumerPokoju.ToString() : "Brak"), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Blue, AutoSize = true, Margin = new Padding(0, 15, 0, 0) };
            var btnSzukaj = new Button { Text = "SZUKAJ WOLNEGO POKOJU", Height = 40, Width = 250, BackColor = Color.Orange, FlatStyle = FlatStyle.Flat };
            int wybranyNr = r.NumerPokoju;

            btnSzukaj.Click += (s, e) =>
            {
                var d1 = dtOd.Value.Date; var d2 = dtDo.Value.Date;
                var zajete = BazaDanych.Rezerwacje.Where(x => x.Id != r.Id && x.Status != "ANULOWANA" && x.Koliduje(d1, d2)).Select(x => x.NumerPokoju).ToList();
                var wolny = BazaDanych.PokojeHotelowe.FirstOrDefault(po => po.Standard == cmbStd.Text && po.MaxOsob >= numOsob.Value && !zajete.Contains(po.Numer));
                if (wolny != null) { wybranyNr = wolny.Numer; lblPrzydzielony.Text = $"Przydzielono: {wybranyNr}"; lblPrzydzielony.ForeColor = Color.Green; }
                else { wybranyNr = 0; lblPrzydzielony.Text = "BRAK WOLNYCH POKOI!"; lblPrzydzielony.ForeColor = Color.Red; }
            };
            p.Controls.Add(btnSzukaj);
            p.Controls.Add(lblPrzydzielony);
            grpRez.Controls.Add(p);

            var btnSave = new Button { Text = "ZAPISZ REZERWACJĘ", BackColor = Color.Green, ForeColor = Color.White, Height = 60, Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(r.GoscGlowny.Nazwisko)) { MessageBox.Show("Podaj nazwisko gościa!"); return; }
                if (wybranyNr == 0) { MessageBox.Show("Musisz przydzielić pokój!"); return; }

                r.DataOd = dtOd.Value.Date;
                r.DataDo = dtDo.Value.Date;
                r.NumerPokoju = wybranyNr;
                r.IloscOsob = (int)numOsob.Value;
                r.NazwaPakietu = cmbPakiet.Text;
                r.StandardPokoju = cmbStd.Text;
                r.Obiadokolacja = chkObiad.Checked;
                r.Rachunek.Clear();

                int dni = (r.DataDo - r.DataOd).Days;
                if (dni <= 0) dni = 1;

                decimal baza = r.StandardPokoju == "Premium" ? 400 : (r.StandardPokoju == "Apartament" ? 600 : 250);

                r.Rachunek.Add(new Obciazenie { NazwaUslugi = $"Nocleg ({r.StandardPokoju}) x {dni} dni", Kwota = baza * dni, CenaJednostkowa = baza, Ilosc = dni });

                if (r.NazwaPakietu == "Pobyt Indywidualny" && r.Obiadokolacja)
                    r.Rachunek.Add(new Obciazenie { NazwaUslugi = $"Obiadokolacje x {dni} dni", Kwota = 119 * r.IloscOsob * dni, CenaJednostkowa = 119 * r.IloscOsob, Ilosc = dni });
                if (chkZwierze.Checked)
                    r.Rachunek.Add(new Obciazenie { NazwaUslugi = "Pobyt zwierzęcia", Kwota = 100 * dni, CenaJednostkowa = 100, Ilosc = dni });
                if (chkWstawka.Checked)
                    r.Rachunek.Add(new Obciazenie { NazwaUslugi = "Wstawka powitalna", Kwota = 150, CenaJednostkowa = 150, Ilosc = 1 });

                if (!BazaDanych.Rezerwacje.Contains(r))
                {
                    BazaDanych.Rezerwacje.Add(r);
                }

                BazaDanych.Zapisz();
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            mainLayout.Controls.Add(lewaKolumna, 0, 0);
            mainLayout.Controls.Add(grpRez, 1, 0);
            f.Controls.Add(mainLayout);
            f.Controls.Add(btnSave);
            f.ShowDialog();
        }

        private void OknoMeldunku(Rezerwacja r)
        {
            Form f = new Form { Text = $"Meldunek - Pokój {r.NumerPokoju}", Size = new Size(950, 850), StartPosition = FormStartPosition.CenterScreen };

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            Panel lewaKolumna = new Panel { Dock = DockStyle.Fill };

            var grpDane = new GroupBox { Text = "Dane Gościa", Dock = DockStyle.Top, Height = 380, Padding = new Padding(10) };
            var flowDane = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = false };

            DodajInput(flowDane, "Imię:", r.GoscGlowny.Imie, v => r.GoscGlowny.Imie = v);
            DodajInput(flowDane, "Nazwisko:", r.GoscGlowny.Nazwisko, v => r.GoscGlowny.Nazwisko = v);
            DodajInput(flowDane, "PESEL:", r.GoscGlowny.Pesel, v => r.GoscGlowny.Pesel = v);
            DodajInput(flowDane, "Nr Dowodu:", r.GoscGlowny.NrDowodu, v => r.GoscGlowny.NrDowodu = v);
            DodajInput(flowDane, "Email:", r.GoscGlowny.Email, v => r.GoscGlowny.Email = v);
            DodajInput(flowDane, "Ulica/Miasto:", r.GoscGlowny.Adres, v => r.GoscGlowny.Adres = v);
            grpDane.Controls.Add(flowDane);

            var grpFaktura = new GroupBox { Text = "Dane do faktury", Dock = DockStyle.Top, Height = 250, Padding = new Padding(10), Margin = new Padding(0, 15, 0, 0) };
            var flowFaktura = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = false };

            var chkChceFakture = new CheckBox
            {
                Text = "Gość prosi o fakturę VAT",
                Checked = r.GoscGlowny.ChceFakture,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10)
            };

            var flowPolaFaktury = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, Width = 400, Height = 180, Enabled = chkChceFakture.Checked };

            DodajInput(flowPolaFaktury, "Nazwa Firmy:", r.GoscGlowny.NazwaFirmy, v => r.GoscGlowny.NazwaFirmy = v);
            DodajInput(flowPolaFaktury, "NIP:", r.GoscGlowny.NIP, v => r.GoscGlowny.NIP = v);
            DodajInput(flowPolaFaktury, "Adres Firmy:", r.GoscGlowny.AdresFirmy, v => r.GoscGlowny.AdresFirmy = v);

            chkChceFakture.CheckedChanged += (s, e) =>
            {
                flowPolaFaktury.Enabled = chkChceFakture.Checked;
                if (!chkChceFakture.Checked) { }
            };

            flowFaktura.Controls.Add(chkChceFakture);
            flowFaktura.Controls.Add(flowPolaFaktury);
            grpFaktura.Controls.Add(flowFaktura);

            lewaKolumna.Controls.Add(grpFaktura);
            lewaKolumna.Controls.Add(grpDane);
            grpFaktura.BringToFront();

            var grpAuto = new GroupBox { Text = "Auto / Parking", Dock = DockStyle.Top, Height = 250, Padding = new Padding(10) };
            var flowAuto = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = false };

            var chkParking = new CheckBox { Text = "Parking (50zł/dobę)", Checked = r.GoscGlowny.Parking, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            flowAuto.Controls.Add(chkParking);
            var txtRej = DodajInput(flowAuto, "Numer rejestracyjny auta:", r.GoscGlowny.NrRejestracyjny, v => r.GoscGlowny.NrRejestracyjny = v);
            var txtMarka = DodajInput(flowAuto, "Marka:", r.GoscGlowny.MarkaSamochodu, v => r.GoscGlowny.MarkaSamochodu = v);
            grpAuto.Controls.Add(flowAuto);

            layout.Controls.Add(lewaKolumna, 0, 0);
            layout.Controls.Add(grpAuto, 1, 0);

            var btnZamelduj = new Button
            {
                Text = "ZAMELDUJ I ZAPISZ DANE",
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.Teal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            btnZamelduj.Click += (s, e) =>
            {
                r.GoscGlowny.ChceFakture = chkChceFakture.Checked;

                if (r.GoscGlowny.ChceFakture && string.IsNullOrWhiteSpace(r.GoscGlowny.NIP))
                {
                    MessageBox.Show("Proszę uzupełnić NIP firmy!");
                    return;
                }

                r.GoscGlowny.Parking = chkParking.Checked;
                r.GoscGlowny.NrRejestracyjny = txtRej.Text;
                r.GoscGlowny.MarkaSamochodu = txtMarka.Text;

                if (r.GoscGlowny.Parking)
                {
                    decimal koszt = 50 * r.DlugoscPobytu;
                    if (!r.Rachunek.Any(x => x.NazwaUslugi.Contains("Parking")))
                        r.Rachunek.Add(new Obciazenie { NazwaUslugi = $"Parking ({r.DlugoscPobytu} dni)", Kwota = koszt, CenaJednostkowa = 50, Ilosc = r.DlugoscPobytu });
                }

                r.Status = "ZAMELDOWANY";
                BazaDanych.Zapisz();
                f.Close();
                MessageBox.Show(r.GoscGlowny.ChceFakture ? "Zameldowano. Dokument: Faktura." : "Zameldowano. Dokument: Paragon.");
            };

            f.Controls.Add(layout);
            f.Controls.Add(btnZamelduj);
            f.ShowDialog();
        }

        private void OknoFinansowKlienta(Rezerwacja r)
        {
            Form f = new Form { Text = $"Finanse - {r.KodRezerwacji}", Size = new Size(1300, 850), StartPosition = FormStartPosition.CenterScreen };

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 750 };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };

            grid.Columns.Add("Usluga", "Usługa");
            grid.Columns["Usluga"].Width = 500;

            grid.Columns.Add("Kwota", "Kwota");
            grid.Columns["Kwota"].Width = 300;

            grid.Columns.Add("Status", "Status");
            grid.Columns["Status"].Width = 320;

            grid.Columns.Add("Id", "Id");
            grid.Columns["Id"].Visible = false;

            var pnlPrawa = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblSuma = new Label { Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top };
            var lblZaliczka = new Label { Font = new Font("Segoe UI", 12), ForeColor = Color.Green, AutoSize = true, Dock = DockStyle.Top };
            var lblDoZaplaty = new Label { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.Red, AutoSize = true, Dock = DockStyle.Top };

            var lblDok = new Label { Text = "Wydrukowane dokumenty (Kliknij 2x aby otworzyć podgląd):", Dock = DockStyle.Bottom, Height = 30 };
            var lstDokumenty = new ListBox { Height = 200, Dock = DockStyle.Bottom };

            lstDokumenty.DoubleClick += (s, e) =>
            {
                if (lstDokumenty.SelectedItem != null)
                {
                    var plik = r.PlikiDokumentow.FirstOrDefault(d => d.NazwaWyswietlana == lstDokumenty.SelectedItem.ToString());
                    if (plik != null) OknoPodgladuDokumentu(plik);
                }
            };

            var btnDodajUsluge = new Button { Text = "+ Dodaj usługę / produkt", Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };

            btnDodajUsluge.Click += (s, e) => DodajUslugeMenu(btnDodajUsluge, r, () =>
            {
                grid.Rows.Clear();
                OdswiezFinanseUI(r, grid, lblSuma, lblZaliczka, lblDoZaplaty, lstDokumenty);
            });

            var btnZaliczka = new Button { Text = "Wpłać Zaliczkę", Dock = DockStyle.Top, Height = 50, BackColor = Color.LightBlue, Margin = new Padding(0, 10, 0, 10) };
            var btnOplacZaznaczone = new Button { Text = "Opłać ZAZNACZONE", Dock = DockStyle.Top, Height = 50, BackColor = Color.LightGreen };

            pnlPrawa.Controls.Add(btnOplacZaznaczone);
            pnlPrawa.Controls.Add(btnZaliczka);
            pnlPrawa.Controls.Add(btnDodajUsluge);
            pnlPrawa.Controls.Add(lblDoZaplaty);
            pnlPrawa.Controls.Add(lblZaliczka);
            pnlPrawa.Controls.Add(lblSuma);
            pnlPrawa.Controls.Add(lblDok);
            pnlPrawa.Controls.Add(lstDokumenty);

            OdswiezFinanseUI(r, grid, lblSuma, lblZaliczka, lblDoZaplaty, lstDokumenty);

            btnZaliczka.Click += (s, e) =>
            {
                string v = Microsoft.VisualBasic.Interaction.InputBox("Kwota:", "Zaliczka", "0");
                if (decimal.TryParse(v, out decimal k))
                {
                    r.WplaconaZaliczka += k;
                    GenerujDokument(r, "Potwierdzenie Zaliczki", new List<Obciazenie> { new Obciazenie { NazwaUslugi = "Wpłata zaliczki", Kwota = k, Ilosc = 1, CenaJednostkowa = k } }, k, "ZALICZKA");
                    BazaDanych.Zapisz();
                    OdswiezFinanseUI(r, grid, lblSuma, lblZaliczka, lblDoZaplaty, lstDokumenty);
                }
            };

            btnOplacZaznaczone.Click += (s, e) =>
            {
                var wybraneObciazenia = new List<Obciazenie>();
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    Guid id = (Guid)row.Cells["Id"].Value;
                    var obc = r.Rachunek.First(x => x.Id == id);
                    if (!obc.CzyOplacone)
                    {
                        wybraneObciazenia.Add(obc);
                    }
                }

                if (wybraneObciazenia.Count == 0)
                {
                    MessageBox.Show("Zaznacz nieopłacone pozycje.");
                    return;
                }

                decimal dostepnaZaliczka = r.WplaconaZaliczka;

                var alokacjaZaliczki = wybraneObciazenia.ToDictionary(x => x, x => 0m);
                decimal doRozdzielenia = dostepnaZaliczka;

                bool dokonanoZmian = true;
                while (dokonanoZmian && doRozdzielenia > 0.00m)
                {
                    dokonanoZmian = false;
                    var kandydaci = wybraneObciazenia.Where(o => alokacjaZaliczki[o] < o.Kwota).ToList();

                    if (kandydaci.Count == 0) break;

                    decimal porcja = Math.Round(doRozdzielenia / kandydaci.Count, 2);
  
                    if (porcja == 0 && doRozdzielenia > 0) porcja = 0.01m;

                    foreach (var item in kandydaci)
                    {
                        if (doRozdzielenia <= 0) break;
                        decimal miejsce = item.Kwota - alokacjaZaliczki[item];

                        decimal kwotaDoDodania = Math.Min(porcja, miejsce);
                        kwotaDoDodania = Math.Min(kwotaDoDodania, doRozdzielenia);

                        if (kwotaDoDodania > 0)
                        {
                            alokacjaZaliczki[item] += kwotaDoDodania;
                            doRozdzielenia -= kwotaDoDodania;
                            dokonanoZmian = true;
                        }
                    }
                }

                decimal calkowitaUzytaZaliczka = dostepnaZaliczka - doRozdzielenia;
                decimal sumaCalkowitaPozycji = wybraneObciazenia.Sum(x => x.Kwota);
                decimal sumaDoZaplaty = sumaCalkowitaPozycji - calkowitaUzytaZaliczka;

                if (sumaCalkowitaPozycji > 0)
                {
                    string typ = "PARAGON FISKALNY";
                    bool klientFaturowy = r.GoscGlowny != null && r.GoscGlowny.ChceFakture;

                    if (klientFaturowy)
                    {
                        typ = "FAKTURA VAT";
                    }
                    else
                    {
                        typ = MessageBox.Show("Czy wygenerować FAKTURĘ VAT? (Nie = Paragon)", "Typ dokumentu", MessageBoxButtons.YesNo) == DialogResult.Yes ?
                              "FAKTURA VAT" : "PARAGON FISKALNY";
                    }

                    r.WplaconaZaliczka -= calkowitaUzytaZaliczka;
                    GenerujDokument(r, typ, wybraneObciazenia, sumaDoZaplaty, "KARTA / GOTÓWKA");

                    foreach (var o in wybraneObciazenia) o.CzyOplacone = true;

                    BazaDanych.Zapisz();
                    OdswiezFinanseUI(r, grid, lblSuma, lblZaliczka, lblDoZaplaty, lstDokumenty);

                    string infoZaliczka = calkowitaUzytaZaliczka > 0 ? $"\nRozliczono z zaliczki: {calkowitaUzytaZaliczka:C2}" : "";
                    MessageBox.Show($"Opłacono {wybraneObciazenia.Count} pozycji. Wystawiono: {typ}\nDo dopłaty: {sumaDoZaplaty:C2}{infoZaliczka}");
                }
                else
                {
                    MessageBox.Show("Suma wybranych pozycji wynosi 0.");
                }
            };

            split.Panel1.Controls.Add(grid);
            split.Panel2.Controls.Add(pnlPrawa);
            f.Controls.Add(split);
            f.ShowDialog();
        }

        private void OdswiezFinanseUI(Rezerwacja r, DataGridView grid, Label l1, Label l2, Label l3, ListBox lst)
        {
            grid.Rows.Clear();
            decimal sumaTotal = 0;

            foreach (var obc in r.Rachunek)
            {
                int idx = grid.Rows.Add(obc.NazwaUslugi, obc.Kwota + " zł", obc.CzyOplacone ? "OPŁACONE" : "DO ZAPŁATY", obc.Id);
                sumaTotal += obc.Kwota;

                if (obc.CzyOplacone)
                {
                    grid.Rows[idx].DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else
                {
                    grid.Rows[idx].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }

            decimal oplaconePrzezGrid = r.Rachunek.Where(x => x.CzyOplacone).Sum(x => x.Kwota);

            l1.Text = $"Suma rachunku: {sumaTotal:F2} zł\n";
            l2.Text = $"Wpłacona zaliczka: {r.WplaconaZaliczka:F2} zł\n";

            decimal doZaplaty = sumaTotal - r.WplaconaZaliczka - oplaconePrzezGrid;
            if (doZaplaty < 0) doZaplaty = 0;

            l3.Text = $"POZOSTAŁO: {doZaplaty:F2} zł\n";

            lst.Items.Clear();
            foreach (var d in r.PlikiDokumentow) lst.Items.Add(d.NazwaWyswietlana);
        }

        private void DodajUslugeMenu(Control ctrl, Rezerwacja r, Action onDone)
        {
            ContextMenuStrip cms = new ContextMenuStrip();

            var gastro = new ToolStripMenuItem("Restauracja & Wyżywienie");
            gastro.DropDownItems.Add("Obiadokolacja (Bufet)", null, (s, e) => DodajKoszt(r, "Obiadokolacja (Bufet)", 119, onDone));
            gastro.DropDownItems.Add("Śniadanie (Dodatkowe)", null, (s, e) => DodajKoszt(r, "Śniadanie (Dodatkowe)", 65, onDone));
            gastro.DropDownItems.Add("Lunch Box (Na wynos)", null, (s, e) => DodajKoszt(r, "Lunch Box", 45, onDone));
            gastro.DropDownItems.Add(new ToolStripSeparator());

            gastro.DropDownItems.Add("Burger Wołowy Premium", null, (s, e) => DodajKoszt(r, "Restauracja: Burger Wołowy", 59, onDone));
            gastro.DropDownItems.Add("Sałatka Cezar z Kurczakiem", null, (s, e) => DodajKoszt(r, "Restauracja: Sałatka Cezar", 42, onDone));
            gastro.DropDownItems.Add("Zupa Dnia", null, (s, e) => DodajKoszt(r, "Restauracja: Zupa Dnia", 25, onDone));

            var lobby = new ToolStripMenuItem("Lobby Bar (Drinki i Przekąski)");
            lobby.DropDownItems.Add("Drink 'Aperol Spritz'", null, (s, e) => DodajKoszt(r, "Bar: Aperol Spritz", 34, onDone));
            lobby.DropDownItems.Add("Drink 'Mojito'", null, (s, e) => DodajKoszt(r, "Bar: Mojito", 32, onDone));
            lobby.DropDownItems.Add("Whisky Single Malt (40ml)", null, (s, e) => DodajKoszt(r, "Bar: Whisky Single Malt", 45, onDone));
            lobby.DropDownItems.Add("Kieliszek Wina Domowego", null, (s, e) => DodajKoszt(r, "Bar: Wino (kieliszek)", 22, onDone));
            lobby.DropDownItems.Add("Piwo Regionalne (0.5l)", null, (s, e) => DodajKoszt(r, "Bar: Piwo Regionalne", 18, onDone));
            lobby.DropDownItems.Add(new ToolStripSeparator());

            lobby.DropDownItems.Add("Kawa Latte Macchiato", null, (s, e) => DodajKoszt(r, "Bar: Kawa Latte", 18, onDone));
            lobby.DropDownItems.Add("Szarlotka na ciepło", null, (s, e) => DodajKoszt(r, "Bar: Szarlotka z lodami", 26, onDone));
            lobby.DropDownItems.Add("Talerz Przekąsek (Sery/Wędliny)", null, (s, e) => DodajKoszt(r, "Bar: Talerz Przekąsek", 65, onDone));

            var spaZabiegi = new ToolStripMenuItem("SPA - Zabiegi i Masaże");
            spaZabiegi.DropDownItems.Add("Masaż Relaksacyjny (1h)", null, (s, e) => DodajKoszt(r, "SPA: Masaż Relaksacyjny 1h", 250, onDone));
            spaZabiegi.DropDownItems.Add("Masaż Gorącymi Kamieniami (1h)", null, (s, e) => DodajKoszt(r, "SPA: Masaż Kamieniami", 280, onDone));
            spaZabiegi.DropDownItems.Add("Masaż Klasyczny Pleców (30min)", null, (s, e) => DodajKoszt(r, "SPA: Masaż Pleców 30min", 140, onDone));
            spaZabiegi.DropDownItems.Add("Rytuał Kobido (Twarz)", null, (s, e) => DodajKoszt(r, "SPA: Rytuał Kobido", 220, onDone));
            spaZabiegi.DropDownItems.Add("Peeling Całego Ciała", null, (s, e) => DodajKoszt(r, "SPA: Peeling Ciała", 180, onDone));

            var spaButik = new ToolStripMenuItem("Butik SPA (Kosmetyki)");
            spaButik.DropDownItems.Add("Luksusowy Krem do Twarzy", null, (s, e) => DodajKoszt(r, "Butik: Krem do twarzy", 180, onDone));
            spaButik.DropDownItems.Add("Naturalne Mydło Ręcznie Robione", null, (s, e) => DodajKoszt(r, "Butik: Mydło Naturalne", 35, onDone));
            spaButik.DropDownItems.Add("Olejek do Ciała (Bursztynowy)", null, (s, e) => DodajKoszt(r, "Butik: Olejek Bursztynowy", 89, onDone));
            spaButik.DropDownItems.Add("Świeca Zapachowa SPA", null, (s, e) => DodajKoszt(r, "Butik: Świeca SPA", 60, onDone));
            spaButik.DropDownItems.Add("Szlafrok Hotelowy (Zakup)", null, (s, e) => DodajKoszt(r, "Butik: Szlafrok (zakup)", 250, onDone));

            var minibar = new ToolStripMenuItem("Minibar (Pokój)");
            minibar.DropDownItems.Add("Coca-Cola (0.5l)", null, (s, e) => DodajKoszt(r, "MB: Coca-Cola 0.5l", 12, onDone));
            minibar.DropDownItems.Add("Woda Mineralna", null, (s, e) => DodajKoszt(r, "MB: Woda Cisowianka", 8, onDone));
            minibar.DropDownItems.Add("Sok Owocowy Cappy", null, (s, e) => DodajKoszt(r, "MB: Sok Cappy", 10, onDone));
            minibar.DropDownItems.Add("Chipsy Lays", null, (s, e) => DodajKoszt(r, "MB: Chipsy Lays", 15, onDone));
            minibar.DropDownItems.Add("Batonik Czekoladowy", null, (s, e) => DodajKoszt(r, "MB: Batonik", 8, onDone));
            minibar.DropDownItems.Add("Mini Whisky Jack Daniels (50ml)", null, (s, e) => DodajKoszt(r, "MB: Jack Daniels 50ml", 35, onDone));
            minibar.DropDownItems.Add("Mini Wódka Absolut (50ml)", null, (s, e) => DodajKoszt(r, "MB: Absolut 50ml", 30, onDone));

            var zwierzeta = new ToolStripMenuItem("Opłaty za zwierzęta");
            zwierzeta.DropDownItems.Add("Pies (doba)", null, (s, e) => DodajKoszt(r, "Pobyt psa", 100, onDone));
            zwierzeta.DropDownItems.Add("Kot (doba)", null, (s, e) => DodajKoszt(r, "Pobyt kota", 80, onDone));

            var inne = new ToolStripMenuItem("Inne Usługi");
            inne.DropDownItems.Add("Parking (doba)", null, (s, e) => DodajKoszt(r, "Parking", 50, onDone));
            inne.DropDownItems.Add("Dostawka do pokoju", null, (s, e) => DodajKoszt(r, "Dostawka", 120, onDone));
            inne.DropDownItems.Add("Late Check-out (do 16:00)", null, (s, e) => DodajKoszt(r, "Przedłużona doba", 150, onDone));

            cms.Items.AddRange(new ToolStripItem[] {
        gastro,
        lobby,
        spaZabiegi,
        spaButik,
        minibar,
        zwierzeta,
        inne
    });

            cms.Show(ctrl, new Point(0, ctrl.Height));
        }

        private void DodajKoszt(Rezerwacja r, string nazwa, decimal cena, Action onDone)
        {
            r.Rachunek.Add(new Obciazenie { NazwaUslugi = nazwa, Kwota = cena, CenaJednostkowa = cena, Ilosc = 1 });
            BazaDanych.Zapisz();
            onDone();
        }

        private void GenerujDokument(Rezerwacja r, string typ, List<Obciazenie> pozycje, decimal suma, string metoda)
        {
            StringBuilder sb = new StringBuilder();
            string linia = new string('-', 68);
            string liniaGruba = new string('=', 68);

            sb.AppendLine(liniaGruba);
            sb.AppendLine("                  HOTEL BETASI PRO *****");
            sb.AppendLine("         Ul. Wypoczynkowa 1, 81-000 Sopot, Polska");
            sb.AppendLine("     NIP: 585-000-11-22 | REGON: 123456789 | BDO: 000123456");
            sb.AppendLine("        Tel: +48 58 555 00 00 | recepcja@mountainpeakresort.pl");
            sb.AppendLine("                www.hotel-betasi-pro.pl");
            sb.AppendLine(liniaGruba);

            sb.AppendLine($"DOKUMENT:   {typ.ToUpper()}");
            sb.AppendLine($"NUMER:      {r.KodRezerwacji}/DOK/{DateTime.Now.Ticks % 1000}");
            sb.AppendLine($"MIEJSCE:    Sopot");
            sb.AppendLine($"DATA WYST.: {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"DATA SPRZ.: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine(linia);

            sb.AppendLine("SPRZEDAWCA:");
            sb.AppendLine("Hotel Betasi Pro Sp. z o.o.");
            sb.AppendLine("Ul. Wypoczynkowa 1, 81-000 Sopot");
            sb.AppendLine("Konto: 12 1020 3040 0000 1111 2222 3333 (PKO BP)");
            sb.AppendLine(" ");

            sb.AppendLine("NABYWCA:");
            sb.AppendLine($"{r.GoscGlowny.Imie} {r.GoscGlowny.Nazwisko}");
            if (!string.IsNullOrEmpty(r.GoscGlowny.Adres)) sb.AppendLine($"Adres: {r.GoscGlowny.Adres}");

            if (typ.Contains("FAKTURA") && r.GoscGlowny != null && !string.IsNullOrEmpty(r.GoscGlowny.NIP))
                sb.AppendLine($"NIP: {r.GoscGlowny.NIP}");
            else if (!string.IsNullOrEmpty(r.GoscGlowny.Pesel))
                sb.AppendLine($"PESEL: {r.GoscGlowny.Pesel}");

            sb.AppendLine(linia);

            sb.AppendLine("SZCZEGÓŁY TRANSAKCJI:");
            sb.AppendLine($"Rezerwacja: {r.KodRezerwacji}  |  Pokój: Standard");
            sb.AppendLine($"Termin:     {r.DataOd:dd.MM.yyyy} - {r.DataDo:dd.MM.yyyy} ({(r.DataDo - r.DataOd).Days} dób)");
            sb.AppendLine(linia);

            sb.AppendLine(String.Format("{0,-32} | {1,3} | {2,3} | {3,9} | {4,9}", "NAZWA TOWARU / USŁUGI", "IL.", "JM", "CENA", "WARTOŚĆ"));
            sb.AppendLine(linia);

            foreach (var p in pozycje)
            {
                string nazwa = p.NazwaUslugi.Length > 32 ? p.NazwaUslugi.Substring(0, 29) + "..." : p.NazwaUslugi;
                sb.AppendLine(String.Format("{0,-32} | {1,3} | {2,3} | {3,9:F2} | {4,9:F2}", nazwa, p.Ilosc, "szt", p.CenaJednostkowa, p.Kwota));
            }

            int minWierszy = 4;
            if (pozycje.Count < minWierszy)
            {
                for (int i = 0; i < (minWierszy - pozycje.Count); i++) sb.AppendLine(String.Format("{0,-32} | {1,3} | {2,3} | {3,9} | {4,9}", ".", "", "", "", ""));
            }
            sb.AppendLine(linia);

            decimal netto = suma / 1.23m;
            decimal vat = suma - netto;

            sb.AppendLine("ROZLICZENIE PODATKU VAT (PLN):");
            sb.AppendLine(String.Format("{0,-15} | {1,-15} | {2,-15} | {3,-12}", "STAWKA", "NETTO", "KWOTA VAT", "BRUTTO"));
            sb.AppendLine(String.Format("{0,-15} | {1,-15:F2} | {2,-15:F2} | {3,-12:F2}", "Podst. 23%", netto, vat, suma));
            sb.AppendLine(linia);

            sb.AppendLine($"DO ZAPŁATY:             {suma:F2} PLN");
            sb.AppendLine($"SŁOWNIE:                (tu wstaw funkcję zamiany na słowa)");
            sb.AppendLine($"METODA PŁATNOŚCI:       {metoda.ToUpper()}");
            sb.AppendLine($"STATUS PŁATNOŚCI:       OPŁACONO");

            sb.AppendLine("\n\n");

            sb.AppendLine("UWAGI I KLAUZULE:");
            sb.AppendLine("Dokument wystawiony elektronicznie na podstawie artykułu 106n ustawy\no VAT.");
            sb.AppendLine("Niniejsza faktura stanowi tytuł wykonawczy.");
            sb.AppendLine("\n");

            sb.AppendLine("       .........................           .........................");
            sb.AppendLine("           Osoba upoważniona                   Osoba upoważniona");
            sb.AppendLine("              do odbioru                         do wystawienia");

            sb.AppendLine(liniaGruba);
            sb.AppendLine($"Wygenerowano z systemu Hotel Betasi Pro: {DateTime.Now}");

            r.PlikiDokumentow.Add(new DokumentPlik
            {
                NazwaWyswietlana = $"{typ} - {suma}zł ({DateTime.Now:HH:mm})",
                TrescDokumentu = sb.ToString()
            });
        }

        private void OknoPodgladuDokumentu(DokumentPlik plik)
        {
            Form podglad = new Form { Text = "Podgląd Dokumentu", Size = new Size(520, 700), StartPosition = FormStartPosition.CenterParent };
            var txt = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 10), Text = plik.TrescDokumentu };
            podglad.Controls.Add(txt);
            podglad.ShowDialog();
        }

        private void OknoEmail(Rezerwacja r)
        {
            Form f = new Form
            {
                Text = "Wyślij Potwierdzenie",
                Size = new Size(300, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(500, 400)
            };

            var pnlDol = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            var btn = new Button
            {
                Text = "Wyślij Wiadomość",
                BackColor = Color.Orange,
                Width = 300,
                Height = 45,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btn.FlatAppearance.BorderSize = 0;

            pnlDol.Resize += (s, e) =>
            {
                btn.Location = new Point(
                    (pnlDol.ClientSize.Width - btn.Width) / 2,
                    (pnlDol.ClientSize.Height - btn.Height) / 2
                );
            };

            btn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(r.GoscGlowny.Email))
                {
                    MessageBox.Show("Błąd: Brak adresu email nabywcy!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show($"Wysłano wiadomość na adres: {r.GoscGlowny.Email}", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                f.Close();
            };

            pnlDol.Controls.Add(btn);

            var p = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(20),
                AutoScroll = true,
                WrapContents = false
            };

            DodajInput(p, "Adres Email Gościa:", r.GoscGlowny.Email, s => { });
            DodajInput(p, "Temat:", $"Potwierdzenie rezerwacji {r.KodRezerwacji}", s => { });

            p.Controls.Add(new Label { Text = "Treść wiadomości:", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });

            var body = new TextBox
            {
                Multiline = true,
                Width = 840,
                Height = 280,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10)
            };

            p.Resize += (s, e) => { body.Width = p.ClientSize.Width - 50; };

            body.Text = $@"Szanowni Państwo {r.GoscGlowny.Nazwisko},

Bardzo dziękujemy za wybór Mountain Peak Resort.
Jest nam niezmiernie miło poinformować, że Państwa rezerwacja została potwierdzona.

SZCZEGÓŁY REZERWACJI:
------------------------------------------------------------------
NUMER REZERWACJI:   {r.KodRezerwacji}
TERMIN POBYTU:      {r.DataOd:dd.MM.yyyy} – {r.DataDo:dd.MM.yyyy}
PRZYPISANY POKÓJ:   {r.NumerPokoju}
------------------------------------------------------------------

INFORMACJE ORGANIZACYJNE:
Doba hotelowa rozpoczyna się o godzinie 15:00 w dniu przyjazdu,
a kończy o godzinie 11:00 w dniu wyjazdu.

W razie jakichkolwiek pytań lub potrzeby modyfikacji rezerwacji,
pozostajemy do Państwa dyspozycji pod numerem telefonu recepcji
lub w odpowiedzi na tę wiadomość.

Czekamy na Państwa przyjazd!

Z poważaniem,
Recepcja Mountain Peak Resort
Ul. Wypoczynkowa 1, Sopot";
            p.Controls.Add(body);

            f.Controls.Add(p);
            f.Controls.Add(pnlDol);


            pnlDol.SendToBack();
            p.BringToFront();

            f.ShowDialog();
        }

        private void ProceduraWymeldowania(Rezerwacja r)
        {
            decimal juzOplacone = r.Rachunek.Where(x => x.CzyOplacone).Sum(x => x.Kwota);
            decimal calosc = r.Rachunek.Sum(x => x.Kwota);
            decimal doZaplaty = calosc - r.WplaconaZaliczka - juzOplacone;

            if (doZaplaty > 0)
            {
                MessageBox.Show($"Nie można wymeldować! Do zapłaty: {doZaplaty} zł.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            r.Status = "WYMELDOWANY";
            BazaDanych.Zapisz();
            MessageBox.Show("Wymeldowano.");
        }

        private void InitRaporty()
        {
            viewRaporty = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(20) };

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            pnlTop.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTop.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

            var lblTyp = new Label { Text = "Rodzaj raportu:", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DimGray };
            var cbRaporty = new ComboBox { Location = new Point(20, 35), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), BackColor = Color.WhiteSmoke };
            cbRaporty.Items.AddRange(new string[] {
                    "Raport Rezerwacji (Wszystkie)",
                    "Raport Przyjazdów (Dzienny)",
                    "Raport Wyjazdów (Dzienny)",
                    "Raport Samochodów (Parking)",
                    "Raport Meldunkowy (Obecni goście)",
                    "Raport dla Kuchni",
                    "Raport dla Pokojowych"
            });
            cbRaporty.SelectedIndex = 0;

            var lblData = new Label { Text = "Data raportu:", Location = new Point(320, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DimGray };
            dtpRaport = new DateTimePicker { Location = new Point(320, 35), Width = 140, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

            var btnGeneruj = new Button
            {
                Text = "POKAŻ PODGLĄD",
                Location = new Point(500, 34),
                Width = 140,
                Height = 32,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var btnExcel = new Button
            {
                Text = "EKSPORTUJ (CSV)",
                Location = new Point(660, 34),
                Width = 140,
                Height = 32,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var btnCzyscFiltry = new Button
            {
                Text = "Wyczyść filtry",
                Location = new Point(820, 34),
                Width = 120,
                Height = 32,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = false
            };

            pnlTop.Controls.AddRange(new Control[] { lblTyp, cbRaporty, lblData, dtpRaport, btnGeneruj, btnExcel, btnCzyscFiltry });

            gridRaporty = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToOrderColumns = false
            };
            gridRaporty.RowTemplate.Height = 35;

            gridRaporty.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            gridRaporty.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridRaporty.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gridRaporty.ColumnHeadersHeight = 45;

            gridRaporty.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            gridRaporty.DefaultCellStyle.Padding = new Padding(4);
            gridRaporty.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 0) };
            pnlGridContainer.Controls.Add(gridRaporty);
            viewRaporty.Controls.Add(pnlGridContainer);
            viewRaporty.Controls.Add(pnlTop);

            cbRaporty.SelectedIndexChanged += (s, e) =>
            {
                string wybrano = cbRaporty.SelectedItem.ToString();
                dtpRaport.Enabled = wybrano.Contains("Dzienny") || wybrano.Contains("Kuchni") || wybrano.Contains("Pokojowych");
            };

            btnGeneruj.Click += (s, e) =>
            {
                aktywneFiltry.Clear();
                btnCzyscFiltry.Visible = false;
                GenerujDaneDoRaportu(cbRaporty.SelectedItem.ToString());
            };

            btnExcel.Click += (s, e) =>
            {
                if (gridRaporty.Rows.Count == 0) { MessageBox.Show("Brak danych."); return; }
                ZapiszDoPlikuCSV(cbRaporty.SelectedItem.ToString());
            };

            btnCzyscFiltry.Click += (s, e) =>
            {
                aktywneFiltry.Clear();
                ZastosujFiltry();
                btnCzyscFiltry.Visible = false;
                gridRaporty.Refresh();
            };

            gridRaporty.CellPainting += (s, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex > -1)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                    string headerText = gridRaporty.Columns[e.ColumnIndex].HeaderText;

                    bool isFiltered = aktywneFiltry.ContainsKey(gridRaporty.Columns[e.ColumnIndex].Name);
                    Color iconColor = isFiltered ? Color.Yellow : Color.LightGray;

                    TextRenderer.DrawText(e.Graphics, headerText, e.CellStyle.Font,
                        new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y, e.CellBounds.Width - 25, e.CellBounds.Height),
                        e.CellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                    var rectFilter = new Rectangle(e.CellBounds.Right - 20, e.CellBounds.Y + 12, 16, 16);
                    TextRenderer.DrawText(e.Graphics, "▼", new Font("Segoe UI", 8), rectFilter, iconColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    e.Handled = true;
                }
            };

            gridRaporty.ColumnHeaderMouseClick += (s, e) =>
            {
                Point mousePos = gridRaporty.PointToClient(Cursor.Position);
                Rectangle headerRect = gridRaporty.GetCellDisplayRectangle(e.ColumnIndex, -1, true);

                bool klikWStrzalke = mousePos.X > (headerRect.Right - 25);

                if (klikWStrzalke)
                {
                    PokazMenuFiltrowania(e.ColumnIndex, btnCzyscFiltry);
                }
                else
                {
                    var col = gridRaporty.Columns[e.ColumnIndex];
                    var dt = (System.Data.DataTable)gridRaporty.DataSource;
                    if (dt == null) return;

                    string currentSort = dt.DefaultView.Sort;
                    string newSort = col.Name + " ASC";

                    if (!string.IsNullOrEmpty(currentSort) && currentSort.StartsWith(col.Name))
                    {
                        if (currentSort.EndsWith("ASC")) newSort = col.Name + " DESC";
                    }

                    dt.DefaultView.Sort = newSort;
                }
            };
        }
        private void PokazMenuFiltrowania(int colIndex, Button btnCzysc)
        {
            string colName = gridRaporty.Columns[colIndex].Name;
            var dt = (System.Data.DataTable)gridRaporty.DataSource;

            var distinctValues = dt.AsEnumerable()
                .Select(row => row[colName].ToString())
                .Distinct()
                .OrderBy(val => val)
                .ToList();

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowCheckMargin = true;
            menu.ShowImageMargin = false;
            menu.Font = new Font("Segoe UI", 9);

            bool pozwolZamknac = false;

            var itemAll = new ToolStripMenuItem(" (Zaznacz/Odznacz wszystkie)");
            itemAll.Click += (s, args) =>
            {
                bool stanDocelowy = true;

                if (menu.Items.Count > 2 && menu.Items[2] is ToolStripMenuItem pierwszyItem)
                {
                    stanDocelowy = !pierwszyItem.Checked;
                }

                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem tsi && tsi != itemAll && tsi.Text != "ZASTOSUJ FILTR")
                    {
                        tsi.Checked = stanDocelowy;
                    }
                }
            };
            menu.Items.Add(itemAll);
            menu.Items.Add(new ToolStripSeparator());

            foreach (var val in distinctValues)
            {
                var item = new ToolStripMenuItem(string.IsNullOrEmpty(val) ? "(Puste)" : val);
                item.CheckOnClick = true;

                if (aktywneFiltry.ContainsKey(colName))
                {
                    if (aktywneFiltry[colName].Contains(val)) item.Checked = true;
                }
                else
                {
                    item.Checked = true;
                }

                menu.Items.Add(item);
            }

            menu.Items.Add(new ToolStripSeparator());
            var btnApply = new ToolStripMenuItem("ZASTOSUJ FILTR");
            btnApply.BackColor = Color.CornflowerBlue;
            btnApply.ForeColor = Color.White;
            btnApply.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            btnApply.Click += (s, args) =>
            {
                List<string> wybrane = new List<string>();
                int licznikOpcji = 0;

                foreach (ToolStripItem it in menu.Items)
                {
                    if (it is ToolStripMenuItem tsi && tsi.CheckOnClick)
                    {
                        licznikOpcji++;
                        if (tsi.Checked)
                        {
                            wybrane.Add(tsi.Text == "(Puste)" ? "" : tsi.Text);
                        }
                    }
                }

                if (wybrane.Count == licznikOpcji)
                {
                    if (aktywneFiltry.ContainsKey(colName)) aktywneFiltry.Remove(colName);
                }
                else
                {
                    aktywneFiltry[colName] = wybrane;
                }

                ZastosujFiltry();
                btnCzysc.Visible = aktywneFiltry.Count > 0;
                gridRaporty.Refresh();

                pozwolZamknac = true;
            };
            menu.Items.Add(btnApply);

            menu.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked && !pozwolZamknac)
                {
                    e.Cancel = true;
                }
            };

            Rectangle r = gridRaporty.GetCellDisplayRectangle(colIndex, -1, true);
            menu.Show(gridRaporty, new Point(r.Left, r.Bottom));
        }

        private void ZastosujFiltry()
        {
            var dt = (System.Data.DataTable)gridRaporty.DataSource;
            if (dt == null) return;

            if (aktywneFiltry.Count == 0)
            {
                dt.DefaultView.RowFilter = "";
                return;
            }

            List<string> czesciFiltra = new List<string>();

            foreach (var kvp in aktywneFiltry)
            {
                string col = kvp.Key;
                var wartosci = kvp.Value;

                if (wartosci.Count > 0)
                {
                    var clauses = wartosci.Select(v => $"[{col}] = '{v.Replace("'", "''")}'");
                    czesciFiltra.Add("(" + string.Join(" OR ", clauses) + ")");
                }
                else
                {
                    czesciFiltra.Add("(1 = 0)");
                }
            }

            dt.DefaultView.RowFilter = string.Join(" AND ", czesciFiltra);
        }

        private void GenerujDaneDoRaportu(string typRaportu)
        {
            gridRaporty.DataSource = null;
            DateTime data = dtpRaport.Value.Date;

            System.Data.DataTable dt = new System.Data.DataTable();

            IEnumerable<Rezerwacja> query = BazaDanych.Rezerwacje;

            if (typRaportu.Contains("Przyjazdów") || typRaportu.Contains("Pokojowych"))
                query = query.Where(r => r.DataOd.Date == data);
            else if (typRaportu.Contains("Wyjazdów"))
                query = query.Where(r => r.DataDo.Date == data);
            else if (typRaportu.Contains("Meldunkowy"))
                query = query.Where(r => r.Status == "ZAMELDOWANY");
            else if (typRaportu.Contains("Samochodów"))
                query = query.Where(r => r.Status == "ZAMELDOWANY" && r.GoscGlowny.Parking);
            else if (typRaportu.Contains("Kuchni"))
            {
                dt.Columns.Add("Posiłek", typeof(string));
                dt.Columns.Add("Ilość", typeof(int));

                var wTerminie = BazaDanych.Rezerwacje
                   .Where(r => (r.Status == "ZAMELDOWANY" || r.Status == "REZERWACJA")
                            && data >= r.DataOd.Date && data < r.DataDo.Date).ToList();

                int sniadania = wTerminie.Sum(r => r.IloscOsob);
                int obiady = wTerminie.Where(r => r.Obiadokolacja || r.NazwaPakietu != "Pobyt indywidualny").Sum(r => r.IloscOsob);

                dt.Rows.Add("Śniadania", sniadania);
                dt.Rows.Add("Obiadokolacje", obiady);
            }

            if (!typRaportu.Contains("Kuchni"))
            {
                var lista = query.ToList();

                dt.Columns.Add("Nr Rezerwacji", typeof(string));
                dt.Columns.Add("Pokój", typeof(int));

                if (typRaportu.Contains("Rezerwacji"))
                {
                    dt.Columns.Add("Gość", typeof(string));
                    dt.Columns.Add("Od", typeof(DateTime));
                    dt.Columns.Add("Do", typeof(DateTime));
                    dt.Columns.Add("Status", typeof(string));
                    dt.Columns.Add("Pakiet", typeof(string));
                    dt.Columns.Add("Standard", typeof(string));
                    dt.Columns.Add("Kwota", typeof(decimal));

                    foreach (var r in lista)
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.GoscGlowny.PobierzDaneEtykieta(), r.DataOd, r.DataDo, r.Status, r.NazwaPakietu, r.StandardPokoju, r.Rachunek.Sum(x => x.Kwota));
                }
                else if (typRaportu.Contains("Przyjazdów"))
                {
                    dt.Columns.Add("Gość", typeof(string));
                    dt.Columns.Add("Pakiet", typeof(string));
                    dt.Columns.Add("Osób", typeof(int));
                    dt.Columns.Add("Status", typeof(string));
                    foreach (var r in lista)
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.GoscGlowny.PobierzDaneEtykieta(), r.NazwaPakietu, r.IloscOsob, r.Status);
                }
                else if (typRaportu.Contains("Wyjazdów"))
                {
                    dt.Columns.Add("Gość", typeof(string));
                    dt.Columns.Add("Opłacono", typeof(string));
                    dt.Columns.Add("Do Zapłaty", typeof(decimal));
                    foreach (var r in lista)
                    {
                        decimal doZaplaty = r.Rachunek.Sum(x => x.Kwota) - r.WplaconaZaliczka - r.Rachunek.Where(x => x.CzyOplacone).Sum(x => x.Kwota);
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.GoscGlowny.PobierzDaneEtykieta(), r.Rachunek.All(x => x.CzyOplacone) ? "TAK" : "NIE", doZaplaty);
                    }
                }
                else if (typRaportu.Contains("Samochodów"))
                {
                    dt.Columns.Add("Gość", typeof(string));
                    dt.Columns.Add("Nr Rejestracyjny", typeof(string));
                    dt.Columns.Add("Marka", typeof(string));
                    foreach (var r in lista)
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.GoscGlowny.PobierzDaneEtykieta(), r.GoscGlowny.NrRejestracyjny ?? "", r.GoscGlowny.MarkaSamochodu ?? "");
                }
                else if (typRaportu.Contains("Meldunkowy"))
                {
                    dt.Columns.Add("Gość", typeof(string));
                    dt.Columns.Add("PESEL", typeof(string));
                    dt.Columns.Add("Adres", typeof(string));
                    foreach (var r in lista)
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.GoscGlowny.PobierzDaneEtykieta(), r.GoscGlowny.Pesel, r.GoscGlowny.Adres);
                }
                else if (typRaportu.Contains("Pokojowych"))
                {
                    dt.Columns.Add("Standard", typeof(string));
                    dt.Columns.Add("Zwierzak", typeof(string));
                    dt.Columns.Add("Wstawka", typeof(string));
                    foreach (var r in lista)
                    {
                        string zwierzak = r.Rachunek.Any(x => x.NazwaUslugi.ToLower().Contains("zwierz") || x.NazwaUslugi.ToLower().Contains("pies") || x.NazwaUslugi.ToLower().Contains("kot")) ? "TAK" : "-";
                        string wstawka = r.Rachunek.Any(x => x.NazwaUslugi.ToLower().Contains("wstawka")) ? "TAK" : "-";
                        dt.Rows.Add(r.KodRezerwacji, r.NumerPokoju, r.StandardPokoju, zwierzak, wstawka);
                    }
                }
            }

            gridRaporty.DataSource = dt;

            foreach (DataGridViewColumn col in gridRaporty.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;

                if (col.Name == "Kwota" || col.Name == "Do Zapłaty") col.DefaultCellStyle.Format = "C2";
                if (col.Name == "Od" || col.Name == "Do") col.DefaultCellStyle.Format = "d";
            }
        }

        private void ZapiszDoPlikuCSV(string nazwaRaportu)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Plik Excel (CSV)|*.csv";
                sfd.FileName = $"{nazwaRaportu.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();

                        string[] columnNames = gridRaporty.Columns.Cast<DataGridViewColumn>().Select(column => column.HeaderText).ToArray();
                        sb.AppendLine(string.Join(";", columnNames));

                        foreach (DataGridViewRow row in gridRaporty.Rows)
                        {
                            string[] cells = row.Cells.Cast<DataGridViewCell>()
                                .Select(cell => cell.Value?.ToString() ?? "")
                                .Select(val => val.Replace(";", ",").Replace("\n", " "))
                                .ToArray();
                            sb.AppendLine(string.Join(";", cells));
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Raport zapisany pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas zapisu: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private TextBox DodajInput(Control parent, string label, string val, Action<string> onBlur)
        {
            var pnl = new Panel { Height = 50, Width = 440 };
            pnl.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            var t = new TextBox { Text = val, Dock = DockStyle.Bottom, Font = new Font("Segoe UI", 10) };
            t.Leave += (s, e) => onBlur(t.Text);
            pnl.Controls.Add(t);
            parent.Controls.Add(pnl);
            return t;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}