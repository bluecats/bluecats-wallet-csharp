# bluecats-wallet-csharp

BlueCats Wallet C# is a CLI built with the [BlueCats.Serial C# libarary](https://github.com/bluecats/bluecats-serial-csharp) for interacting with [BlueCats Wallet for iOS](https://github.com/bluecats/bluecats-wallet-ios).

**[Click here](https://github.com/bluecats/bluecats-wallet-csharp/releases) to download app binary to run a pre-complied version of the application.**

#### Commands  
Command | Action
--------|--------
commands| lists commands
quit    | quits wallet
reload  | reload cards to opening balance
tender merchantID amount | tender card   
cancel [transactionID] | cancel transaction
datasource | print data source
firmware | print beacon's firmware version

#### Merchants
File: Models/Merchant.cs   
Merchants and their IDs are held in a dictionary created in the Merchant.cs file.

#### Editing the Barcode or Opening Balance
File: Models/Card.cs  
Card barcode is defined here along with card opening balances.  The default card opening balance is a random number between 25 - 250.
