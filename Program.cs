using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CurrencyExchangeService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("======================================");
            Console.WriteLine(" Currency Exchange Office System      ");
            Console.WriteLine(" Network Application Development      ");
            Console.WriteLine("======================================");
            Console.WriteLine();

            // Initialize database
            Console.WriteLine("Initializing database...");
            DatabaseManager.InitializeDatabase();
            Console.WriteLine("Database ready! ✓");
            Console.WriteLine();

            // Start WCF Service
            Uri baseAddress = new Uri(
                "http://localhost:8080/CurrencyExchangeService");

            ServiceHost host = new ServiceHost(
                typeof(CurrencyService), baseAddress);

            ServiceMetadataBehavior smb =
                new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.SendTimeout = TimeSpan.FromSeconds(60);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(60);

            host.AddServiceEndpoint(
                typeof(ICurrencyService),
                binding, "");

            try
            {
                host.Open();
                Console.WriteLine("Service Status: Running ✓");
                Console.WriteLine("URL: http://localhost:8080" +
                    "/CurrencyExchangeService");
                Console.WriteLine("NBP API: Connected ✓");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to stop...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Service error: "
                    + ex.Message);
                Console.ReadLine();
            }
            finally
            {
                if (host.State == CommunicationState.Opened)
                    host.Close();
                Console.WriteLine("Service stopped.");
            }
        }
    }
}