using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CurrencyExchangeService
{
    public class DatabaseManager
    {
        private static string dbPath =
            "CurrencyExchange.db";

        private static string connectionString =
            "Data Source=" + dbPath + ";Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(dbPath);

                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    new SQLiteCommand(@"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY,
                            Username TEXT UNIQUE NOT NULL,
                            Password TEXT NOT NULL,
                            Balance REAL DEFAULT 1000.0
                        )", conn).ExecuteNonQuery();

                    new SQLiteCommand(@"
                        CREATE TABLE IF NOT EXISTS Transactions (
                            Id INTEGER PRIMARY KEY,
                            Username TEXT NOT NULL,
                            Type TEXT NOT NULL,
                            CurrencyCode TEXT,
                            Amount REAL,
                            PlnAmount REAL,
                            Balance REAL,
                            CreatedAt TEXT
                        )", conn).ExecuteNonQuery();

                    Console.WriteLine("Database ready!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error: " + ex.Message);
            }
        }

        public static bool RegisterUser(
            string username, string password)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO Users " +
                        "(Username, Password, Balance) " +
                        "VALUES (@u, @p, 1000.0)", conn);
                    cmd.Parameters.AddWithValue(
                        "@u", username);
                    cmd.Parameters.AddWithValue(
                        "@p", password);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool LoginUser(
            string username, string password)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT COUNT(*) FROM Users " +
                        "WHERE Username=@u AND Password=@p",
                        conn);
                    cmd.Parameters.AddWithValue(
                        "@u", username);
                    cmd.Parameters.AddWithValue(
                        "@p", password);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static double GetBalance(string username)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT Balance FROM Users " +
                        "WHERE Username=@u", conn);
                    cmd.Parameters.AddWithValue(
                        "@u", username);
                    object result = cmd.ExecuteScalar();
                    return result != null
                        ? Convert.ToDouble(result) : 0;
                }
            }
            catch { return 0; }
        }

        public static bool UpdateBalance(
            string username, double newBalance)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "UPDATE Users SET Balance=@b " +
                        "WHERE Username=@u", conn);
                    cmd.Parameters.AddWithValue(
                        "@b", newBalance);
                    cmd.Parameters.AddWithValue(
                        "@u", username);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public static void SaveTransaction(
            string username, string type,
            string currencyCode, double amount,
            double plnAmount, double balance)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "INSERT INTO Transactions " +
                        "(Username, Type, CurrencyCode, " +
                        "Amount, PlnAmount, Balance, " +
                        "CreatedAt) VALUES " +
                        "(@u, @t, @c, @a, @p, @b, @d)",
                        conn);
                    cmd.Parameters.AddWithValue(
                        "@u", username);
                    cmd.Parameters.AddWithValue(
                        "@t", type);
                    cmd.Parameters.AddWithValue(
                        "@c", currencyCode);
                    cmd.Parameters.AddWithValue(
                        "@a", amount);
                    cmd.Parameters.AddWithValue(
                        "@p", plnAmount);
                    cmd.Parameters.AddWithValue(
                        "@b", balance);
                    cmd.Parameters.AddWithValue(
                        "@d", DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm"));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Transaction error: "
                    + ex.Message);
            }
        }

        public static string[] GetTransactionHistory(
            string username)
        {
            try
            {
                using (SQLiteConnection conn =
                    new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT Type, CurrencyCode, " +
                        "Amount, PlnAmount, Balance, " +
                        "CreatedAt FROM Transactions " +
                        "WHERE Username=@u " +
                        "ORDER BY Id DESC", conn);
                    cmd.Parameters.AddWithValue(
                        "@u", username);

                    SQLiteDataReader reader =
                        cmd.ExecuteReader();
                    List<string> records =
                        new List<string>();

                    while (reader.Read())
                    {
                        records.Add(
                            reader["CreatedAt"] + " | "
                            + reader["Type"] + " | "
                            + reader["Amount"] + " "
                            + reader["CurrencyCode"]
                            + " | "
                            + reader["PlnAmount"] + " PLN"
                            + " | Bal: "
                            + reader["Balance"] + " PLN");
                    }

                    return records.Count > 0
                        ? records.ToArray()
                        : new string[]
                        { "No transactions yet!" };
                }
            }
            catch (Exception ex)
            {
                return new string[]
                { "Error: " + ex.Message };
            }
        }
    }
}