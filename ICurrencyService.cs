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
    }
}