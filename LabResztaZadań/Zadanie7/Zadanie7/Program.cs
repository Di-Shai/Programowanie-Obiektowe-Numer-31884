using System;

//Zadanie7
class Program
{
    static void Main(string[] args)
    {
        Zwierze[] zwierzeta = new Zwierze[]
        {
            new Pies(),
            new Kot(),
            new Zwierze(),
            new Pies()
        };
        Console.WriteLine("--- Odgłosy zwierząt w tablicy ---");

        foreach (Zwierze z in zwierzeta)
        {
            z.DajGlos();
        }
    }
}//Koniec zadania 7

class Zwierze
{
    public virtual void DajGlos() => Console.WriteLine("Zwierzę wydaje dźwięk");
}

class Pies : Zwierze
{
    public override void DajGlos() => Console.WriteLine("Hau hau!");
}

class Kot : Zwierze
{
    public override void DajGlos() => Console.WriteLine("Miau!");
}