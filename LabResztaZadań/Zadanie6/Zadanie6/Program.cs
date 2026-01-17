using System;

class Zwierze
{
    public void Jedz() => Console.WriteLine("Zwierzę je");
}

class Pies : Zwierze
{
    public void Szczekaj() => Console.WriteLine("Hau hau!");
}

//Zadanie 6
class Kot : Zwierze
{
    public void Miaucz() => Console.WriteLine("Miau!");
}// Koniec zadania 6

class Program
{
    static void Main()
    {
        Console.WriteLine("PIES");
        Pies burek = new Pies();
        burek.Jedz();
        burek.Szczekaj();

        Console.WriteLine("---");

        Console.WriteLine("KOT");
        Kot mruczek = new Kot();
        mruczek.Jedz();
        mruczek.Miaucz();
    }
}