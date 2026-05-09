using System;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using Newtonsoft.Json.Linq;

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
            try
            {
                string url = "http://api.nbp.pl/api/exchangerates/rates/a/"
                             + currencyCode + "/?format=json";

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add(
                        "Accept", "application/json");

                    string response = httpClient
                        .GetStringAsync(url).Result;

                    JObject json = JObject.Parse(response);
                    double rate = json["rates"][0]["mid"]
                        .Value<double>();
                    return rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching rate for "
                    + currencyCode + ": " + ex.Message);
                return 0.0;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri(
                "http://localhost:8080/CurrencyExchangeService");

            ServiceHost host = new ServiceHost(
                typeof(CurrencyService), baseAddress);

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
            Console.WriteLine("Fetching REAL rates from NBP API...");
            Console.WriteLine("Press ENTER to stop the service...");
            Console.ReadLine();

            host.Close();
        }
    }
}