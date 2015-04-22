# bluecats-wallet-csharp

BlueCats Wallet C# is a CLI, built on BCLib-CSharp, for interacting with BC Wallet for iOS .

####Commands  
Command | Action
------|------  
commands | lists commands
quit     | quits wallet
reload   | reload cards to opening balance
tender merchantID amount | tender card   
cancel [transactionID] | cancel transaction
datasource | print data source

####Merchants
File: Models/Merchant.cs   
Merchants and their IDs are held in a dictionary created in the Merchant.cs file.

####Editing the Barcode or Opening Balance
File: Models/Card.cs  
Card barcode is defined here along with card opening balances.  The default card opening balance is a random number between 25 - 250.
