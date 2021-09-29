using Newtonsoft.Json;
using System;

namespace AurumCLI
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing connection to AUCoin Dev network...");
            LedgerModel ledger = new();

            ledger.Login("jpw0032");
            Console.WriteLine("Successfuly connected to AUCoin Dev network!");

            ledger.Write("Hello, Blockchain!");
            Console.WriteLine(ledger.Read().ToString());
        }
    }
}
