using System;
using System.ServiceModel;
using System.Threading.Tasks;
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

        [OperationContract]
        string[] GetTransactionsByType(
            string username, string type);

        [OperationContract]
        string GetAccountSummary(string username);
    }

    public partial class MainWindow : Window
    {
        private ICurrencyService client;
        private string loggedInUser = null;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToService();
            LoadCurrencies();
            LoadExchangeRatesAsync();
        }

        private void ConnectToService()
        {
            try
            {
                EndpointAddress address = new EndpointAddress(
                    "http://localhost:8080/CurrencyExchangeService");
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.SendTimeout = TimeSpan.FromSeconds(30);
                binding.ReceiveTimeout = TimeSpan.FromSeconds(30);
                ChannelFactory<ICurrencyService> factory =
                    new ChannelFactory<ICurrencyService>(
                        binding, address);
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
                string[] currencies =
                    client.GetAvailableCurrencies();

                FromCurrencyCombo.Items.Add("PLN");
                ToCurrencyCombo.Items.Add("PLN");

                foreach (string currency in currencies)
                {
                    FromCurrencyCombo.Items.Add(currency);
                    ToCurrencyCombo.Items.Add(currency);
                    BuySellCurrencyCombo.Items.Add(currency);
                }

                FromCurrencyCombo.SelectedIndex = 1;
                ToCurrencyCombo.SelectedIndex = 0;
                BuySellCurrencyCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading currencies: "
                    + ex.Message);
            }
        }

        private async void LoadExchangeRatesAsync()
        {
            try
            {
                RatesListBox.Items.Clear();
                RatesListBox.Items.Add("Loading rates...");
                string[] currencies =
                    client.GetAvailableCurrencies();
                RatesListBox.Items.Clear();

                foreach (string currency in currencies)
                {
                    double rate = await Task.Run(() =>
                        client.GetExchangeRate(currency));
                    RatesListBox.Items.Add(
                        "1 " + currency + " = "
                        + rate + " PLN");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rates: "
                    + ex.Message);
            }
        }

        private void RegisterButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                string username =
                    UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                if (string.IsNullOrEmpty(username) ||
                    string.IsNullOrEmpty(password))
                {
                    AccountStatusText.Text =
                        "Please enter username and password!";
                    return;
                }

                bool success = client.RegisterUser(
                    username, password);

                if (success)
                {
                    AccountStatusText.Foreground =
                        System.Windows.Media.Brushes.Green;
                    AccountStatusText.Text =
                        "Registration successful! Please login.";
                }
                else
                {
                    AccountStatusText.Foreground =
                        System.Windows.Media.Brushes.Red;
                    AccountStatusText.Text =
                        "Username already exists!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration error: "
                    + ex.Message);
            }
        }

        private void LoginButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                string username =
                    UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                bool success = client.LoginUser(
                    username, password);

                if (success)
                {
                    loggedInUser = username;
                    double balance =
                        client.GetBalance(username);
                    AccountStatusText.Foreground =
                        System.Windows.Media.Brushes.Green;
                    AccountStatusText.Text =
                        "Welcome " + username + "!";
                    BalanceText.Text = "Balance: "
                        + balance + " PLN";

                    // Show account summary
                    string summary =
                        client.GetAccountSummary(username);
                    SummaryText.Text = summary;
                }
                else
                {
                    AccountStatusText.Foreground =
                        System.Windows.Media.Brushes.Red;
                    AccountStatusText.Text =
                        "Invalid username or password!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }

        private void TopUpButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggedInUser == null)
                {
                    MessageBox.Show("Please login first!");
                    return;
                }

                double amount = double.Parse(
                    TopUpTextBox.Text);
                bool success = client.TopUpBalance(
                    loggedInUser, amount);

                if (success)
                {
                    double balance =
                        client.GetBalance(loggedInUser);
                    BalanceText.Text = "Balance: "
                        + balance + " PLN";
                    SummaryText.Text =
                        client.GetAccountSummary(
                            loggedInUser);
                    MessageBox.Show("Top up successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Top up error: "
                    + ex.Message);
            }
        }

        private async void ExchangeButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggedInUser == null)
                {
                    MessageBox.Show("Please login first!");
                    return;
                }

                string fromCurrency = FromCurrencyCombo
                    .SelectedItem.ToString();
                string toCurrency = ToCurrencyCombo
                    .SelectedItem.ToString();
                double amount = double.Parse(
                    AmountTextBox.Text);

                ResultTextBlock.Text = "Calculating...";

                double result = await Task.Run(() =>
                    client.ExchangeCurrency(
                        fromCurrency, toCurrency, amount));

                ResultTextBlock.Text = amount + " "
                    + fromCurrency + " = "
                    + result + " " + toCurrency;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exchange error: "
                    + ex.Message);
            }
        }

        private async void BuyButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggedInUser == null)
                {
                    MessageBox.Show("Please login first!");
                    return;
                }

                string currency = BuySellCurrencyCombo
                    .SelectedItem.ToString();
                double amount = double.Parse(
                    BuySellAmountTextBox.Text);

                BuySellResultText.Text = "Processing...";

                string result = await Task.Run(() =>
                    client.BuyCurrency(
                        loggedInUser, currency, amount));

                BuySellResultText.Foreground =
                    result.StartsWith("Success")
                    ? System.Windows.Media.Brushes.Green
                    : System.Windows.Media.Brushes.Red;

                BuySellResultText.Text = result;

                if (result.StartsWith("Success"))
                {
                    double balance =
                        client.GetBalance(loggedInUser);
                    BalanceText.Text = "Balance: "
                        + balance + " PLN";
                    SummaryText.Text =
                        client.GetAccountSummary(
                            loggedInUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Buy error: " + ex.Message);
            }
        }

        private async void SellButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggedInUser == null)
                {
                    MessageBox.Show("Please login first!");
                    return;
                }

                string currency = BuySellCurrencyCombo
                    .SelectedItem.ToString();
                double amount = double.Parse(
                    BuySellAmountTextBox.Text);

                BuySellResultText.Text = "Processing...";

                string result = await Task.Run(() =>
                    client.SellCurrency(
                        loggedInUser, currency, amount));

                BuySellResultText.Foreground =
                    result.StartsWith("Success")
                    ? System.Windows.Media.Brushes.Green
                    : System.Windows.Media.Brushes.Red;

                BuySellResultText.Text = result;

                if (result.StartsWith("Success"))
                {
                    double balance =
                        client.GetBalance(loggedInUser);
                    BalanceText.Text = "Balance: "
                        + balance + " PLN";
                    SummaryText.Text =
                        client.GetAccountSummary(
                            loggedInUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sell error: " + ex.Message);
            }
        }

        private void RefreshHistoryButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggedInUser == null)
                {
                    MessageBox.Show("Please login first!");
                    return;
                }

                string filter = (FilterCombo.SelectedItem as
                    System.Windows.Controls.ComboBoxItem)
                    ?.Content?.ToString() ?? "ALL";

                HistoryListBox.Items.Clear();
                string[] history = filter == "ALL"
                    ? client.GetTransactionHistory(
                        loggedInUser)
                    : client.GetTransactionsByType(
                        loggedInUser, filter);

                foreach (string record in history)
                    HistoryListBox.Items.Add(record);
            }
            catch (Exception ex)
            {
                MessageBox.Show("History error: "
                    + ex.Message);
            }
        }
    }
}