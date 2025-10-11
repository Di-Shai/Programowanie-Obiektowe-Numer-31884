// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

Osoba osoba1 = new Osoba("Adam", 34);
Osoba osoba2 = new Osoba("Olek", 12);
Osoba osoba3 = new Osoba("Ala", 54);

osoba1.PrzedstawSie();
osoba2.PrzedstawSie();
osoba3.PrzedstawSie();
class Osoba
{
    public string Imie { get; set; }
    public int Wiek { get; set; }

public Osoba (string imie, int wiek)
    {
        Imie = imie;
        Wiek = wiek;
    }
    public void PrzedstawSie()
    {
        Console.WriteLine("Nazywam się " + Imie + " i mam " + Wiek + " lat");
    }
}
