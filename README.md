# bluecats-wallet-csharp

####CLI Commands

commands - lists commands  
quit - quits wallet   
reload - reload cards to opening balance   
tender merchantID amount - tender card   
cancel [transactionID] - cancel transaction   
datasource - print data source   

####Adding Merchants
Models/Merchant.cs
To remove or add a merchant, edit the 

####Editing the barcode or Opening Balance
Models/Card.cs  
Card barcode is defined here as well as card opening balances.  The default openening balance a random number between 25 - 250.
