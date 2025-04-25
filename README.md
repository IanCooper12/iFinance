# Setup
## Required software
- Visual Studio 2022, with the following components:
  - ASP.Net and web development
  - .NET Framework project and item templates (found under individual components in the installer)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SQL Server Management Studio](https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms)
## Setup the database:
- Execute `iFINANCE_FinalSetup.sql` in SQL Server Management Studio
## Connect the database:
Do this if you get an error when trying to log in:
- Open `Group13iFinanceFix.sln` in Visual Studio
- Open `Web.config`
- Find the 2 lines with "data source=...;"
- Replace both values in "..." with your server name (as found on SQL Server Management Studio)

# Running
1. Open `Group13iFinanceFix.sln` in Visual Studio 2022
2. Click the "IIS Express" run button
3. If the page is blocked in your browser, go to chrome://flags, find "Insecure origins treated as secure", enable the setting, and restart your browser
