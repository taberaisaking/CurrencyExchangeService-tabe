using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CurrencyExchangeService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Delete old database to start fresh
            if (File.Exists("CurrencyExchange.db"))
            {
                File.Delete("CurrencyExchange.db");
                Console.WriteLine("Old database deleted!");
            }

            // Initialize fresh database
            DatabaseManager.InitializeDatabase();

            Uri baseAddress = new Uri(
                "http://localhost:8080/CurrencyExchangeService");

            ServiceHost host = new ServiceHost(
                typeof(CurrencyService), baseAddress);

            ServiceMetadataBehavior smb =
                new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            host.AddServiceEndpoint(
                typeof(ICurrencyService),
                new BasicHttpBinding(),
                "");

            host.Open();

            Console.WriteLine("======================================");
            Console.WriteLine(" CurrencyExchange WCF Service Running ");
            Console.WriteLine("======================================");
            Console.WriteLine("URL: http://localhost:8080/CurrencyExchangeService");
            Console.WriteLine("Database: Connected ✓");
            Console.WriteLine("Press ENTER to stop the service...");
            Console.ReadLine();

            host.Close();
        }
    }
}