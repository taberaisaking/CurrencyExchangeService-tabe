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
        private static Dictionary<string, List<string>> transactions
            = new Dictionary<string, List<string>>();
        private static Dictionary<string, double> rateCache
            = new Dictionary<string, double>();
        private static DateTime lastCacheTime = DateTime.MinValue;

        public string SayHello(string name)
        {
            return "Hello " + name +
                "! Welcome to Currency Exchange Service.";
        }

        public double GetExchangeRate(string currencyCode)
        {
            try
            {
                if (rateCache.ContainsKey(currencyCode) &&
                    (DateTime.Now - lastCacheTime).TotalMinutes < 5)
                {
                    return rateCache[currencyCode];
                }

                string url =
                    "http://api.nbp.pl/api/exchangerates/rates/a/"
                    + currencyCode + "/?format=json";

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    httpClient.DefaultRequestHeaders.Add(
                        "Accept", "application/json");

                    string response = httpClient
                        .GetStringAsync(url).Result;

                    JObject json = JObject.Parse(response);
                    double rate = json["rates"][0]["mid"]
                        .Value<double>();

                    rateCache[currencyCode] = rate;
                    lastCacheTime = DateTime.Now;
                    return rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (rateCache.ContainsKey(currencyCode))
                    return rateCache[currencyCode];
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
            transactions[username] = new List<string>();
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
            string record = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                + " | TOP UP | +" + amount + " PLN"
                + " | Balance: " + balances[username] + " PLN";
            transactions[username].Add(record);
            Console.WriteLine(username + " topped up: "
                + amount + " PLN");
            return true;
        }

        public string BuyCurrency(
            string username,
            string currencyCode,
            double amount)
        {
            try
            {
                if (!balances.ContainsKey(username))
                    return "User not found!";

                double rate = GetExchangeRate(currencyCode);
                if (rate == 0)
                    return "Could not get exchange rate!";

                double cost = Math.Round(amount * rate, 2);

                if (balances[username] < cost)
                    return "Insufficient balance! Need "
                        + cost + " PLN but have "
                        + Math.Round(balances[username], 2) + " PLN";

                balances[username] -= cost;
                balances[username] = Math.Round(
                    balances[username], 2);

                string record = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    + " | BUY | " + amount + " " + currencyCode
                    + " for " + cost + " PLN"
                    + " | Balance: " + balances[username] + " PLN";
                transactions[username].Add(record);

                Console.WriteLine(username + " bought "
                    + amount + " " + currencyCode);

                return "Success! Bought " + amount + " "
                    + currencyCode + " for " + cost + " PLN."
                    + " New balance: " + balances[username] + " PLN";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string SellCurrency(
            string username,
            string currencyCode,
            double amount)
        {
            try
            {
                if (!balances.ContainsKey(username))
                    return "User not found!";

                double rate = GetExchangeRate(currencyCode);
                if (rate == 0)
                    return "Could not get exchange rate!";

                double earned = Math.Round(amount * rate, 2);

                balances[username] += earned;
                balances[username] = Math.Round(
                    balances[username], 2);

                string record = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    + " | SELL | " + amount + " " + currencyCode
                    + " for " + earned + " PLN"
                    + " | Balance: " + balances[username] + " PLN";
                transactions[username].Add(record);

                Console.WriteLine(username + " sold "
                    + amount + " " + currencyCode);

                return "Success! Sold " + amount + " "
                    + currencyCode + " for " + earned + " PLN."
                    + " New balance: " + balances[username] + " PLN";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string[] GetTransactionHistory(string username)
        {
            if (!transactions.ContainsKey(username))
                return new string[] { "No transactions found!" };

            if (transactions[username].Count == 0)
                return new string[] { "No transactions yet!" };

            return transactions[username].ToArray();
        }
    }
}