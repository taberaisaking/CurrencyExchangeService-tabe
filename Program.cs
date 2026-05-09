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

        [OperationContract]
        double ExchangeCurrency(
            string fromCurrency,
            string toCurrency,
            double amount);

        [OperationContract]
        string[] GetAvailableCurrencies();
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

            // Test SayHello
            Console.WriteLine("\n--- Service Test ---");
            Console.WriteLine(client.SayHello("Student"));

            // Test available currencies
            Console.WriteLine("\n--- Available Currencies ---");
            string[] currencies = client.GetAvailableCurrencies();
            foreach (string currency in currencies)
            {
                Console.Write(currency + " ");
            }

            // Test exchange rates
            Console.WriteLine("\n\n--- Live Exchange Rates ---");
            foreach (string currency in currencies)
            {
                double rate = client.GetExchangeRate(currency);
                Console.WriteLine("1 " + currency + " = "
                    + rate + " PLN");
            }

            // Test currency exchange
            Console.WriteLine("\n--- Currency Exchange ---");
            double amount = 100;
            string from = "USD";
            string to = "EUR";
            double result = client.ExchangeCurrency(from, to, amount);
            Console.WriteLine(amount + " " + from + " = "
                + result + " " + to);

            amount = 500;
            from = "PLN";
            to = "USD";
            result = client.ExchangeCurrency(from, to, amount);
            Console.WriteLine(amount + " " + from + " = "
                + result + " " + to);

            factory.Close();

            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }
}