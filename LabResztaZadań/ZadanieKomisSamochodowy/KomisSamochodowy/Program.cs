using System;
using System.Collections.Generic;

class Pojazd
{
    public string Marka { get; set; }
    public string Model { get; set; }
    public string Kolor { get; set; }

    public Pojazd(string marka, string model, string kolor)
    {
        Marka = marka;
        Model = model;
        Kolor = kolor;
    }

    public virtual void Start() => Console.WriteLine($"\n >> [SILNIK]: {Marka} {Model} - Silnik uruchomiony.");

    public override string ToString() => $"{Marka} {Model} [{Kolor}]";
}

class Samochod : Pojazd
{
    public Samochod(string marka, string model, string kolor) : base(marka, model, kolor) { }

    public void Jedz() => Console.WriteLine($"\n >> [RUCH]: {Marka} {Model} jedzie po drodze.");
}

class ElektrycznySamochod : Samochod
{
    public ElektrycznySamochod(string marka, string model, string kolor) : base(marka, model, kolor) { }

    public void Laduj() => Console.WriteLine($"\n >> [BATERIA]: {Marka} {Model} podłączona do ładowarki...");
}
class Komis
{
    private List<Pojazd> flota = new List<Pojazd>();

    public void DodajPojazd(Pojazd p)
    {
        flota.Add(p);
        Console.WriteLine("\n[SUKCES] Dodano nowy pojazd do bazy.");
    }
    public bool CzyMaPojazdy() => flota.Count > 0;

    public void WyswietlPojazdy()
    {
        Console.WriteLine("\n--- LISTA POJAZDÓW W KOMISIE ---");
        for (int i = 0; i < flota.Count; i++)
        {
            string typ = flota[i] is ElektrycznySamochod ? "(E)" : "(Spalinowy)";
            Console.WriteLine($"[{i + 1}] {flota[i]} {typ}");
        }
        Console.WriteLine("--------------------------------");
    }

    public Pojazd PobierzPojazd(int index)
    {
        if (index >= 0 && index < flota.Count) return flota[index];
        return null;
    }

    public void UsunPojazd(Pojazd p)
    {
        flota.Remove(p);
        Console.WriteLine("\n[KOMIS] Pojazd został sprzedany/usunięty z listy.");
    }
}
class Program
{
    static Komis komis = new Komis();

    static void Main()
    {
        komis.DodajPojazd(new Samochod("Ford", "Focus", "Niebieski"));
        komis.DodajPojazd(new ElektrycznySamochod("Tesla", "Model S", "Biały"));

        bool dziala = true;

        while (dziala)
        {
            Console.WriteLine("\n=== SYSTEM ZARZĄDZANIA KOMISEM ===");
            Console.WriteLine("1. Wyświetl listę pojazdów");
            Console.WriteLine("2. Dodaj nowy pojazd");
            Console.WriteLine("3. Zarządzaj pojazdem (Jedź, Maluj, Sprzedaj)");
            Console.WriteLine("0. Wyjdź");
            Console.Write("Wybierz opcję: ");

            string wybor = Console.ReadLine();

            switch (wybor)
            {
                case "1":
                    komis.WyswietlPojazdy();
                    break;
                case "2":
                    MenuDodawania();
                    break;
                case "3":
                    MenuAkcji();
                    break;
                case "0":
                    dziala = false;
                    break;
                default:
                    Console.WriteLine("Nieznana opcja.");
                    break;
            }
        }
    }
    static void MenuDodawania()
    {
        Console.Write("Podaj markę: ");
        string marka = Console.ReadLine();
        Console.Write("Podaj model: ");
        string model = Console.ReadLine();
        Console.Write("Podaj kolor: ");
        string kolor = Console.ReadLine();

        Console.WriteLine("Typ silnika: 1 - Spalinowy, 2 - Elektryczny");
        string typ = Console.ReadLine();

        if (typ == "2")
            komis.DodajPojazd(new ElektrycznySamochod(marka, model, kolor));
        else
            komis.DodajPojazd(new Samochod(marka, model, kolor));
    }
    static void MenuAkcji()
    {
        if (!komis.CzyMaPojazdy())
        {
            Console.WriteLine("Najpierw dodaj pojazdy!");
            return;
        }

        komis.WyswietlPojazdy();
        Console.Write("Podaj numer pojazdu do akcji: ");

        if (int.TryParse(Console.ReadLine(), out int id) && id > 0)
        {
            Pojazd wybrany = komis.PobierzPojazd(id - 1);

            if (wybrany != null)
            {
                ObslugaKonkretnegoPojazdu(wybrany);
            }
            else
            {
                Console.WriteLine("Błędny numer pojazdu.");
            }
        }
    }

    static void ObslugaKonkretnegoPojazdu(Pojazd p)
    {
        bool wMenuPojazdu = true;
        while (wMenuPojazdu)
        {
            Console.WriteLine($"\n--- Panel sterowania: {p.Marka} {p.Model} ---");
            Console.WriteLine("1. Uruchom silnik (Start)");

            if (p is Samochod) Console.WriteLine("2. Jedź");

            Console.WriteLine("3. Przemaluj");
            Console.WriteLine("4. Sprzedaj (Usuń z komisu)");

            if (p is ElektrycznySamochod) Console.WriteLine("5. Ładuj baterię");

            Console.WriteLine("0. Powrót do menu głównego");
            Console.Write("Decyzja: ");

            string akcja = Console.ReadLine();

            switch (akcja)
            {
                case "1":
                    p.Start();
                    break;
                case "2":
                    if (p is Samochod s) s.Jedz();
                    break;
                case "3":
                    Console.Write("Podaj nowy kolor: ");
                    p.Kolor = Console.ReadLine();
                    Console.WriteLine("Kolor zmieniony!");
                    break;
                case "4":
                    komis.UsunPojazd(p);
                    wMenuPojazdu = false;
                    break;
                case "5":
                    if (p is ElektrycznySamochod el) el.Laduj();
                    else Console.WriteLine("To nie jest auto elektryczne!");
                    break;
                case "0":
                    wMenuPojazdu = false;
                    break;
                default:
                    Console.WriteLine("Nieznana akcja.");
                    break;
            }
        }
    }
}