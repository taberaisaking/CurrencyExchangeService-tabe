// Final review - presentation ready
// Version 1.1 - Improved error handling for NBP API timeout scenarios
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace CurrencyExchangeService
{
    public class CurrencyService : ICurrencyService
    {
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
                    httpClient.Timeout =
                        TimeSpan.FromSeconds(10);
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
                Console.WriteLine("Exchange error: "
                    + ex.Message);
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

        public bool RegisterUser(
            string username, string password)
        {
            bool success = DatabaseManager.RegisterUser(
                username, password);
            if (success)
                Console.WriteLine(
                    "New user registered: " + username);
            return success;
        }

        public bool LoginUser(
            string username, string password)
        {
            return DatabaseManager.LoginUser(
                username, password);
        }

        public double GetBalance(string username)
        {
            return DatabaseManager.GetBalance(username);
        }

        public bool TopUpBalance(
            string username, double amount)
        {
            double currentBalance =
                DatabaseManager.GetBalance(username);
            double newBalance = Math.Round(
                currentBalance + amount, 2);

            bool success = DatabaseManager.UpdateBalance(
                username, newBalance);

            if (success)
            {
                DatabaseManager.SaveTransaction(
                    username, "TOP UP", "PLN",
                    amount, amount, newBalance);
                Console.WriteLine(username
                    + " topped up: " + amount + " PLN");
            }
            return success;
        }

        public string BuyCurrency(
            string username,
            string currencyCode,
            double amount)
        {
            try
            {
                double rate = GetExchangeRate(currencyCode);
                if (rate == 0)
                    return "Could not get exchange rate!";

                double cost = Math.Round(amount * rate, 2);
                double currentBalance =
                    DatabaseManager.GetBalance(username);

                if (currentBalance < cost)
                    return "Insufficient balance! Need "
                        + cost + " PLN but have "
                        + Math.Round(currentBalance, 2)
                        + " PLN";

                double newBalance = Math.Round(
                    currentBalance - cost, 2);
                DatabaseManager.UpdateBalance(
                    username, newBalance);
                DatabaseManager.SaveTransaction(
                    username, "BUY", currencyCode,
                    amount, cost, newBalance);

                Console.WriteLine(username + " bought "
                    + amount + " " + currencyCode);

                return "Success! Bought " + amount + " "
                    + currencyCode + " for " + cost
                    + " PLN. New balance: "
                    + newBalance + " PLN";
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
                double rate = GetExchangeRate(currencyCode);
                if (rate == 0)
                    return "Could not get exchange rate!";

                double earned = Math.Round(
                    amount * rate, 2);
                double currentBalance =
                    DatabaseManager.GetBalance(username);
                double newBalance = Math.Round(
                    currentBalance + earned, 2);

                DatabaseManager.UpdateBalance(
                    username, newBalance);
                DatabaseManager.SaveTransaction(
                    username, "SELL", currencyCode,
                    amount, earned, newBalance);

                Console.WriteLine(username + " sold "
                    + amount + " " + currencyCode);

                return "Success! Sold " + amount + " "
                    + currencyCode + " for " + earned
                    + " PLN. New balance: "
                    + newBalance + " PLN";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string[] GetTransactionHistory(
            string username)
        {
            return DatabaseManager
                .GetTransactionHistory(username);
        }

        public string[] GetTransactionsByType(
            string username, string type)
        {
            return DatabaseManager
                .GetTransactionsByType(username, type);
        }

        public string GetAccountSummary(string username)
        {
            return DatabaseManager
                .GetAccountSummary(username);
        }

        public string[] GetHistoricalRates(
            string currencyCode,
            string startDate,
            string endDate)
        {
            try
            {
                string url =
                    "http://api.nbp.pl/api/exchangerates/rates/a/"
                    + currencyCode + "/"
                    + startDate + "/" + endDate
                    + "/?format=json";

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout =
                        TimeSpan.FromSeconds(15);
                    httpClient.DefaultRequestHeaders.Add(
                        "Accept", "application/json");

                    string response = httpClient
                        .GetStringAsync(url).Result;

                    JObject json = JObject.Parse(response);
                    JArray rates = (JArray)json["rates"];

                    List<string> result =
                        new List<string>();

                    foreach (JObject rate in rates)
                    {
                        result.Add(
                            rate["effectiveDate"].ToString()
                            + " | " + currencyCode
                            + " = "
                            + rate["mid"].ToString()
                            + " PLN");
                    }

                    return result.Count > 0
                        ? result.ToArray()
                        : new string[]
                        { "No data found!" };
                }
            }
            catch (Exception ex)
            {
                return new string[]
                { "Error: " + ex.Message };
            }
        }

        public string[] GetLastDaysRates(
            string currencyCode, int days)
        {
            string endDate = DateTime.Now
                .ToString("yyyy-MM-dd");
            string startDate = DateTime.Now
                .AddDays(-days)
                .ToString("yyyy-MM-dd");

            return GetHistoricalRates(
                currencyCode, startDate, endDate);
        }
    }
}
