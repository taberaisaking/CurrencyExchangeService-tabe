using System;
using System.ServiceModel;

namespace CurrencyExchangeClient
{
    [ServiceContract]
    public interface ICurrencyService
    {
        [OperationContract]
        string SayHello(string name);

        [OperationContract]
        double GetExchangeRate(string currencyCode);
    }

    class Program
    {
        static void Main(string[] args)
        {
            EndpointAddress address = new EndpointAddress(
                "http://localhost:8080/CurrencyExchangeService");

            BasicHttpBinding binding = new BasicHttpBinding();

            ChannelFactory<ICurrencyService> factory =
                new ChannelFactory<ICurrencyService>(binding, address);

            ICurrencyService client = factory.CreateChannel();

            Console.WriteLine("======================================");
            Console.WriteLine("     Currency Exchange Client         ");
            Console.WriteLine("======================================");

            Console.WriteLine("\n--- Testing SayHello ---");
            string greeting = client.SayHello("Student");
            Console.WriteLine(greeting);

            Console.WriteLine("\n--- Testing GetExchangeRate ---");
            string[] currencies = { "USD", "EUR", "GBP", "JPY" };
            foreach (string currency in currencies)
            {
                double rate = client.GetExchangeRate(currency);
                Console.WriteLine("Exchange rate for " + currency + ": " + rate + " PLN");
            }

            factory.Close();

            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }
}