using System;

namespace MojNowyBank
{
    class KontoBankowe
    {
        private double saldo;
        public void Wplata(double kwota)
        {
            saldo += kwota;
        }
        public double PobierzSaldo()
        {
            return saldo;
        }

        //Zadanie 5
        public void Wyplata(double kwota)
        {
            if (saldo >= kwota)
            {
                saldo -= kwota;
                Console.WriteLine($"\n-> Sukces! Wypłacono: {kwota} zł");
            }
            else
            {
                Console.WriteLine($"\n-> Operacja odrzucona: Chcesz wypłacić {kwota} zł, a masz tylko {saldo} zł.");
            }
        }
    } //Koniec metody do zadania 5

    class Program
    {
        static void Main(string[] args)
        {
            KontoBankowe mojeKonto = new KontoBankowe();

            mojeKonto.Wplata(500);
            Console.WriteLine($"Twoje obecne saldo to: {mojeKonto.PobierzSaldo()} zł");
            Console.WriteLine("------------------------------------------------");

            Console.Write("Wpisz kwotę, jaką chcesz wypłacić: ");

            string wpisanyTekst = Console.ReadLine();
            double kwotaDoWyplaty = double.Parse(wpisanyTekst);

            mojeKonto.Wyplata(kwotaDoWyplaty);

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Saldo po operacji: {mojeKonto.PobierzSaldo()} zł");

            Console.ReadKey();
        }
    }
}