using System.ComponentModel;
 
 // Zadanie 1
string password = "admin123";
do
{
    Console.WriteLine("Zadanie 1. Podaj haslo: ");
    password = Console.ReadLine();

    if (password != "admin123")
    {
        Console.WriteLine();
        Console.WriteLine("Zle haslo!");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("Zalogowano!");
    }

} while (password != "admin123");

Console.WriteLine("Exit...");
Console.WriteLine();

//Zadanie 2
int liczba = 0;
do
{
    Console.WriteLine();
    Console.WriteLine("Zadanie 2. Podaj liczbę: ");  //konwertuje int liczba na int z string
    liczba = Int32.Parse(Console.ReadLine());

    if (liczba <= 0)
    {
        Console.WriteLine();
        Console.WriteLine("Zła liczba. Spróbuj jeszcze raz!");
        Console.WriteLine();
    }
} while (liczba <= 0);
Console.WriteLine();
Console.WriteLine("Exit...");
Console.WriteLine();

//Zadanie 3
Console.WriteLine("Zadanie 3.");
Console.WriteLine();
string[] cities = new string[] {"Koszalin", "Poznań", "Wrocław", "Kraków", "Warszawa" };
foreach (string city in cities)
{
    Console.WriteLine(city);
}