using System.ServiceModel;

namespace CurrencyExchangeService
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

        [OperationContract]
        bool RegisterUser(string username, string password);

        [OperationContract]
        bool LoginUser(string username, string password);

        [OperationContract]
        double GetBalance(string username);

        [OperationContract]
        bool TopUpBalance(string username, double amount);

        [OperationContract]
        string BuyCurrency(
            string username,
            string currencyCode,
            double amount);

        [OperationContract]
        string SellCurrency(
            string username,
            string currencyCode,
            double amount);

        [OperationContract]
        string[] GetTransactionHistory(string username);
    }
}