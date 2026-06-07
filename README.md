# CurrencyExchangeService

## Course name: Network Application Development

## Project title: Currency Exchange Office System

## Author name: Tabe Raisa Ayuk-Agbor

## Student ID: 73657

## Description
A WCF-based network application simulating a 
currency exchange office. Built using .NET Framework.
The system includes:
- WCF Web Service with NBP API integration
- WPF Client Application
- SQLite Database
- User account management
- Buy/Sell currency transactions
- Transaction history
- Historical exchange rates

## How to Run
1. Open CurrencyExchangeService.sln in Visual Studio 2022
2. Run as Administrator
3. Right-click Solution → Properties → Multiple Startup Projects
4. Set CurrencyExchangeService → Start
5. Set CurrencyExchangeWPF → Start
6. Press F5
7. Register a new user and login

## Technologies Used
- .NET Framework 4.8
- WCF (Windows Communication Foundation)
- WPF (Windows Presentation Foundation)
- SQLite Database
- NBP API (National Bank of Poland)

## Testing
All features tested manually:
- User registration and login ✅
- Live exchange rate retrieval ✅
- Currency buy/sell operations ✅
- Transaction history display ✅
- Historical rates chart ✅
- Historical rates chart ✅

## Known Limitations
- Requires internet connection for NBP API
- Service must be run as Administrator