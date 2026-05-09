using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CurrencyExchangeService
{
    public class CurrencyService : ICurrencyService
    {
        private static Dictionary<string, string> users
            = new Dictionary<string, string>();
        private static Dictionary<string, double> balances
            = new Dictionary<string, double>();

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
                Console.WriteLine("Error: " + ex.Message);
                return 0.0;
            }
        }

        public double ExchangeCurrency(
            string fromCurrency,
            string toCurrency,
            double amount)
        {
            try
            {
                double fromRate = fromCurrency == "PLN"
                    ? 1.0 : GetExchangeRate(fromCurrency);
                double toRate = toCurrency == "PLN"
                    ? 1.0 : GetExchangeRate(toCurrency);

                if (fromRate == 0 || toRate == 0) return 0;

                double amountInPln = amount * fromRate;
                double result = amountInPln / toRate;
                return Math.Round(result, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exchange error: " + ex.Message);
                return 0;
            }
        }

        public string[] GetAvailableCurrencies()
        {
            return new string[]
            {
                "USD", "EUR", "GBP", "CHF",
                "JPY", "CAD", "AUD", "SEK"
            };
        }

        public bool RegisterUser(string username, string password)
        {
            if (users.ContainsKey(username))
                return false;
            users[username] = password;
            balances[username] = 1000.0;
            Console.WriteLine("New user registered: " + username);
            return true;
        }

        public bool LoginUser(string username, string password)
        {
            if (!users.ContainsKey(username))
                return false;
            return users[username] == password;
        }

        public double GetBalance(string username)
        {
            if (balances.ContainsKey(username))
                return balances[username];
            return 0;
        }

        public bool TopUpBalance(string username, double amount)
        {
            if (!balances.ContainsKey(username))
                return false;
            balances[username] += amount;
            Console.WriteLine(username + " topped up: "
                + amount + " PLN");
            return true;
        }
    }
}