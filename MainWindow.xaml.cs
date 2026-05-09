using System;
using System.ServiceModel;
using System.Windows;

namespace CurrencyExchangeWPF
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

    public partial class MainWindow : Window
    {
        private ICurrencyService client;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToService();
            LoadCurrencies();
            LoadExchangeRates();
        }

        private void ConnectToService()
        {
            try
            {
                EndpointAddress address = new EndpointAddress(
                    "http://localhost:8080/CurrencyExchangeService");
                BasicHttpBinding binding = new BasicHttpBinding();
                ChannelFactory<ICurrencyService> factory =
                    new ChannelFactory<ICurrencyService>(binding, address);
                client = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot connect to service: "
                    + ex.Message);
            }
        }

        private void LoadCurrencies()
        {
            try
            {
                string[] currencies = client.GetAvailableCurrencies();
                FromCurrencyCombo.Items.Add("PLN");
                ToCurrencyCombo.Items.Add("PLN");
                foreach (string currency in currencies)
                {
                    FromCurrencyCombo.Items.Add(currency);
                    ToCurrencyCombo.Items.Add(currency);
                }
                FromCurrencyCombo.SelectedIndex = 1;
                ToCurrencyCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading currencies: "
                    + ex.Message);
            }
        }

        private void LoadExchangeRates()
        {
            try
            {
                string[] currencies = client.GetAvailableCurrencies();
                foreach (string currency in currencies)
                {
                    double rate = client.GetExchangeRate(currency);
                    RatesListBox.Items.Add(
                        "1 " + currency + " = " + rate + " PLN");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rates: " + ex.Message);
            }
        }

        private void ExchangeButton_Click(object sender,
            RoutedEventArgs e)
        {
            try
            {
                string fromCurrency = FromCurrencyCombo
                    .SelectedItem.ToString();
                string toCurrency = ToCurrencyCombo
                    .SelectedItem.ToString();
                double amount = double.Parse(AmountTextBox.Text);

                double result = client.ExchangeCurrency(
                    fromCurrency, toCurrency, amount);

                ResultTextBlock.Text = amount + " " + fromCurrency
                    + " = " + result + " " + toCurrency;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exchange error: " + ex.Message);
            }
        }
    }
}