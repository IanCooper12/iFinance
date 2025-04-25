# Setup
## Setup the database:
- Execute `iFINANCE_FinalSetup.sql` in SQL Server Management Studio
## Connect the database:
- Open `Group13iFinanceFix.sln` in Visual Studio
- Open `Web.config`
- Find the 2 lines with "data source=...;"
- Replace both values in "..." with your server name (as found on SQL Server Management Studio)

# Running
1. Open `Group13iFinanceFix.sln` in Visual Studio 2022
2. Click the "IIS Express" run button
3. If the page is blocked in your browser, go to chrome://flags, find "Insecure origins treated as secure", enable the setting, and restart your browser
