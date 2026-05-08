using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CurrencyExchangeService
{
    [ServiceContract]
    public interface ICurrencyService
    {
        [OperationContract]
        string SayHello(string name);

        [OperationContract]
        double GetExchangeRate(string currencyCode);
    }

    public class CurrencyService : ICurrencyService
    {
        public string SayHello(string name)
        {
            return "Hello " + name + "! Welcome to Currency Exchange Service.";
        }

        public double GetExchangeRate(string currencyCode)
        {
            switch (currencyCode.ToUpper())
            {
                case "USD": return 4.05;
                case "EUR": return 4.30;
                case "GBP": return 5.10;
                default: return 0.0;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8080/CurrencyExchangeService");

            ServiceHost host = new ServiceHost(typeof(CurrencyService), baseAddress);

            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
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
            Console.WriteLine("Press ENTER to stop the service...");
            Console.ReadLine();

            host.Close();
        }
    }
}