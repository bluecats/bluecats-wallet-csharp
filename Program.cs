using BlueCats.Serial;
using BlueCats.Serial.Events.EventArgs;
using BlueCats.Wallet.Models;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using BlueCats.Serial.Exceptions;
using BlueCats.Wallet.Tools;

namespace BlueCats.Wallet
{
    class Program
    {
        private readonly static string _version = "1.0.2";

        private static DemoDataSource _demoDataSource;
        private static SerialBeacon _selectedBeacon;

        static void Main(string[] args)
        {
            if (args.Contains("--debug")) SerialBeaconManager.SpoolDebugOutputToConsole();
            Console.WriteLine("BlueCats Wallet Version {0}", _version);
            Console.WriteLine();


            // Discover serial beacons
            Console.WriteLine("Discovering connected beacons...");
            var availableBeacons = SerialBeaconManager.DiscoverSerialBeacons();
            while (!availableBeacons.Any())
            {
                Console.WriteLine("No beacons detected. Connect a beacon and press enter.");
                Console.ReadKey();
                Console.WriteLine("Rescanning for connected serial beacons...");
                availableBeacons = SerialBeaconManager.DiscoverSerialBeacons();
            }


            // Select a beacon
            if (availableBeacons.Count == 1)
            {
                _selectedBeacon = availableBeacons.First();
            }
            else
            {
                while (_selectedBeacon == null)
                {
                    for (var i = 0; i < availableBeacons.Count; i++)
                        Console.WriteLine("{0}) {1}", i + 1, availableBeacons[i]);
                    Console.WriteLine();
                    try
                    {
                        var choice = Convert.ToInt32(GetUserInput("Select a beacon")) - 1;
                        _selectedBeacon = availableBeacons.ElementAt(choice);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid choice, enter a beacon number.");
                    }
                }
                Console.WriteLine();
            }


            // Attach to beacon and start listening
            try
            {
                _selectedBeacon.Attach();

                _selectedBeacon.BleConnectedEvent += _selectedBeacon_BleConnectedEvent;
                _selectedBeacon.BleDisconnectedEvent += _selectedBeacon_BleDisconnectedEvent;
                _selectedBeacon.BleDataRequestEvent += _selectedBeacon_BleDataRequestEvent;
                _selectedBeacon.BleDataBlocksSentEvent += _selectedBeacon_BleDataBlocksSentEvent;
                _selectedBeacon.BleDataIndicatedEvent += _selectedBeacon_BleDataIndicatedEvent;

                try
                {
                    _selectedBeacon.WriteEventsEnabled(true);
                }
                catch (Exception ex)
                {
                    // enable events not supported in older beacon fw
                    Debug.Print("* Error on command WriteEventsEnabled(true): {0}", 
                        ex.GetBaseException().Message, null);
                } 

                Console.WriteLine("Attached to {0}", _selectedBeacon);
            }
            catch
            {

                if (_selectedBeacon.IsAttached)
                    _selectedBeacon.Detach();

                Console.WriteLine("Failed to attach {0}", _selectedBeacon);
            }
            Console.WriteLine();


            // Display merchant info
            var merchantForMerchantId = Merchant.GenerateDemoMerchants(5);
            var merchants = merchantForMerchantId.Values.ToList();
            var cards = Card.GenerateDemoCards(merchants);
            _demoDataSource = new DemoDataSource(merchantForMerchantId, cards);
            Console.WriteLine(_demoDataSource.ToString());


            // Display command prompt
            Console.WriteLine("Enter 'commands' to see a list of commands.");
            Console.WriteLine();

            var quit = false;
            while (!quit)
            {
                var line = GetUserInput(">");
                if (line == null) continue;
                var parts = line.Split(' ');
                if (parts.Length > 0)
                    quit = RunWalletCommand(parts);
            }

            // Cleanup
            _selectedBeacon.BleConnectedEvent -= _selectedBeacon_BleConnectedEvent;
            _selectedBeacon.BleDisconnectedEvent -= _selectedBeacon_BleDisconnectedEvent;
            _selectedBeacon.BleDataRequestEvent -= _selectedBeacon_BleDataRequestEvent;
            _selectedBeacon.BleDataBlocksSentEvent -= _selectedBeacon_BleDataBlocksSentEvent;
            _selectedBeacon.BleDataIndicatedEvent -= _selectedBeacon_BleDataIndicatedEvent;

            if (_selectedBeacon.IsAttached)
                _selectedBeacon.Detach();
        }

        private static string GetUserInput(string prompt = "")
        {
            try
            {
                Console.Write("{0}> ", prompt);
                return Console.ReadLine();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool RunWalletCommand(string[] args)
        {
            bool quit = false;
            if (string.Compare(args[0], "quit", true) == 0)
            {
                quit = true;
                Console.WriteLine("Quitting Wallet.");
            }
            else if (string.Compare(args[0], "reload", true) == 0)
            {
                foreach (Card card in _demoDataSource.Cards)
                {
                    card.CurrentBalance = card.OpeningBalance;
                }
                Console.WriteLine("Reloaded all cards.");
            }
            else if (string.Compare(args[0], "tender", true) == 0)
            {
                if (args.Length == 3)
                {
                    bool tender = true;
                    if (SerialBeaconManager.GetAttachedBeacons().Count <= 0)
                    {
                        Console.WriteLine("Serial beacon not attached.");
                        tender = false;
                    }

                    Merchant merchant = _demoDataSource.GetMerchant(args[1]);
                    if (merchant == null)
                    {
                        Console.WriteLine("Merchant {0} not found.", args[1]);
                        tender = false;
                    }

                    decimal totalAmount;
                    if (!Decimal.TryParse(args[2], out totalAmount))
                    {
                        Console.WriteLine("Invalid total amount {0}.", args[2]);
                        tender = false;
                    }

                    if (tender)
                    {
                        Transaction transaction = new Transaction
                        {
                            TotalAmount = totalAmount,
                            RemainingAmount = totalAmount,
                            Merchant = merchant
                        };
                        _demoDataSource.AddTransaction(transaction);

                        TenderCard(transaction);
                    }
                }
                else
                {
                    Console.WriteLine("Usage: tender merchantID amount");
                }
            }
            else if (string.Compare(args[0], "cancel", true) == 0)
            {
                if (args.Length <= 2)
                {
                    var transactionID = args.Length == 2 ? args[1] : null;
                    CancelTransaction(transactionID);
                }
                else
                {
                    Console.WriteLine("Usage: cancel [transactionID]");
                }
            }
            else if (string.Compare(args[0], "datasource", true) == 0)
            {
                Console.WriteLine(_demoDataSource);
            }
            else if (string.Compare(args[0], "firmware", true) == 0)
            {
                try
                {
                    if (_selectedBeacon != null)
                    {
                        var fwVer = _selectedBeacon.ReadFirmwareVersion();
                        Console.WriteLine("Serial Beacon firmware: v{0}", fwVer);
                    }
                    else
                    {
                        Console.WriteLine("Error: Beacon needs to be reset");
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine("Serial beacon not responding");
                     Debug.Print("Serial beacon not responding: {0}", ex.GetBaseException().Message);
                }
                
            }
            else if (!string.IsNullOrEmpty(args[0]))
            {
                WriteWalletCommandsToConsole();
            }
            return quit;
        }

        private static void WriteWalletCommandsToConsole()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("");
            Console.WriteLine(" + quit                         quit wallet");
            Console.WriteLine(" + reload                       reload cards to opening balance");
            Console.WriteLine(" + tender merchantID amount     tender card");
            Console.WriteLine(" + cancel [transactionID]       cancel transaction");
            Console.WriteLine(" + datasource                   print data source");
            Console.WriteLine(" + firmware                     print beacon's firmware version");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("");
        }

        static void _selectedBeacon_BleDataBlocksSentEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Block data sent!");
        }

        static void _selectedBeacon_BleDisconnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Wireless device disconnected from USB beacon");
        }

        static void _selectedBeacon_BleConnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Wireless Device connected to USB beacon");
        }

        static void _selectedBeacon_BleDataRequestEvent(object sender, BleDataRequestEventArgs e)
        {
            RespondToBleDataRequest((SerialBeacon)sender, e.Data);
        }

        static void _selectedBeacon_BleDataIndicatedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Wireless device indicated that it received the data response");
        }

        static void RespondToBleDataRequest(SerialBeacon beacon, byte[] requestData)
        {
            try
            {
                var requestInfo = DictionarySerializer.DeserializeFromByteArray(requestData);
                if (requestInfo != null)
                {
                    var dataTypeString = (string)requestInfo[Constants.WALLET_DATA_TYPE_TINY_KEY];
                    if (!string.IsNullOrEmpty(dataTypeString))
                    {

                        Console.WriteLine("Received data request {0}.", JsonConvert.SerializeObject(requestInfo));

                        if (string.Compare(dataTypeString, ((byte)eWalletDataTypes.CardBalanceRequest).ToString(), true) == 0)
                        {
                            RespondToCardBalanceRequest(beacon, requestInfo);
                        }
                        else if (string.Compare(dataTypeString, ((byte)eWalletDataTypes.CardRedmeptionRequest).ToString(), true) == 0)
                        {
                            RespondToCardRedemptionRequest(beacon, requestInfo);
                        }
                        else
                        {
                            Console.WriteLine("BLE data request type {0} not supported.", dataTypeString);
                            RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeNotSupported });
                        }
                    }
                    else
                    {
                        Console.WriteLine("BLE data request JSON invalid.");
                        RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.JSONInvalid });
                    }
                }
                else
                {
                    Console.WriteLine("LE data request type missing.");
                    RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeMissing });
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Exception in RespondToBleDataRequest: {0}", ex.GetBaseException().Message);
            }
        }

        static void RespondToCardBalanceRequest(SerialBeacon beacon, Dictionary<string, object> requestInfo)
        {
            var barcode = (string)requestInfo[Constants.WALLET_CARD_BARCODE_TINY_KEY];
            var merchantID = (string)requestInfo[Constants.WALLET_MERCHANT_ID_TINY_KEY];

            var errors = new List<byte>();
            if (string.IsNullOrEmpty(barcode))
            {
                Console.WriteLine("Barcode missing.");
                errors.Add((byte)eWalletErrors.CardBarcodeMissing);
            }

            if (string.IsNullOrEmpty(merchantID))
            {
                Console.WriteLine("Merchant ID missing.");
                errors.Add((byte)eWalletErrors.MerchantIDMissing);
            }

            if (errors.Count > 0)
            {
                RespondToBleDataRequestWithErrors(beacon, requestInfo, errors);
                return;
            }

            
            var card = _demoDataSource.GetCard(merchantID, barcode);
            if (card == null)
            {
                Console.WriteLine("Card {0} for merchant {1} not found.", barcode, merchantID);
                RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.CardNotFound });
                return;
            }

            var responseInfo = new Dictionary<string, object>(requestInfo);
            responseInfo[Constants.WALLET_CARD_CURRENT_BALANCE_TINY_KEY] = card.CurrentBalance.ToString("0.00");
            var responseData = DictionarySerializer.SerializeToByteArray(responseInfo);
            if (responseData != null)
            {
                Console.WriteLine("Data request response: {0}.", JsonConvert.SerializeObject(responseInfo));
                try
                {
                    beacon.RespondToBleDataRequest(responseData);
                }
                catch (Exception ex)
                {
                    Debug.Print("Failure when calling beacon.RespondToBleDataRequest(responseData) in Program: {0}", ex.GetBaseException().Message);
                }
            }
            else
            {
                Console.WriteLine("Serialization failed for card balance request response.");
                RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.SerializationFailed });
            }
        }

        static void RespondToCardRedemptionRequest(SerialBeacon beacon, Dictionary<string, object> requestInfo)
        {
            var barcode = (string)requestInfo[Constants.WALLET_CARD_BARCODE_TINY_KEY];
            var merchantID = (string)requestInfo[Constants.WALLET_MERCHANT_ID_TINY_KEY];
            var transactionID = (string)requestInfo[Constants.WALLET_TRANSACTION_ID_TINY_KEY];
            var deviceID = (string)requestInfo[Constants.WALLET_DEVICE_ID_TINY_KEY];

            decimal? amount = null;
            
            if (requestInfo.ContainsKey(Constants.WALLET_AMOUNT_TINY_KEY)) 
            {
                var amountString = (string)requestInfo[Constants.WALLET_AMOUNT_TINY_KEY];
                if (!String.IsNullOrEmpty(amountString))
                {
                    amount = decimal.Parse(amountString);
                }
            }

            var errors = new List<byte>();
            if (string.IsNullOrEmpty(barcode))
            {
                Console.WriteLine("Barcode missing.");
                errors.Add((byte)eWalletErrors.CardBarcodeMissing);
            }

            if (string.IsNullOrEmpty(merchantID))
            {
                Console.WriteLine("Merchant ID missing.");
                errors.Add((byte)eWalletErrors.MerchantIDMissing);
            }

            if (string.IsNullOrEmpty(transactionID))
            {
                Console.WriteLine("Transaction ID missing.");
                errors.Add((byte)eWalletErrors.TransactionIDMissing);
            }

            if (errors.Count > 0)
            {
                RespondToBleDataRequestWithErrors(beacon, requestInfo, errors);
                return;
            }

            var transaction = _demoDataSource.GetTransaction(transactionID);
            if (transaction == null)
            {
                Console.WriteLine("Transaction {0} not found.", transactionID);
                errors.Add((byte)eWalletErrors.TransactionNotFound);
            }
            else if (transaction.IsCanceled) 
            {
                Console.WriteLine("Transaction {0} was previously canceled at {1}.", transactionID, transaction.CanceledAt);
                errors.Add((byte)eWalletErrors.TransactionCanceled);
            }
            else if (transaction.IsComplete)
            {
                Console.WriteLine("Transaction {0} completed.", transactionID);
                errors.Add((byte)eWalletErrors.TransactionCompleted);
            }
            else if (amount.HasValue && Decimal.Compare(amount.Value, transaction.RemainingAmount) > 0) 
            {
                Console.WriteLine("Amount {0:0.00} exceeds remaining amount {1:0.00} on transaction {2}.", amount.Value, transaction.RemainingAmount, transactionID);
                errors.Add((byte)eWalletErrors.SpecifiedAmountExceedsRemainingAmount);
            }


            var card = _demoDataSource.GetCard(merchantID, barcode);
            if (card == null)
            {
                Console.WriteLine("Card {0} for merchant {1} not found.", barcode, merchantID);
                errors.Add((byte)eWalletErrors.CardNotFound);
            }
            else if (amount.HasValue && Decimal.Compare(amount.Value, card.CurrentBalance) > 0)
            {
                Console.WriteLine("Card {0} balance {1:0.00} less than amount {2:0.00} specified.", barcode, card.CurrentBalance, amount.Value);
                errors.Add((byte)eWalletErrors.CardBalanceInsufficient);
            }

            if (errors.Count > 0)
            {
                RespondToBleDataRequestWithErrors(beacon, requestInfo, errors);
                return;
            }

            var item = new TenderLineItem
            {
                Card = card,
                DeviceID = deviceID,
            };
            transaction.AddTenderLineItem(item);
    
            if (amount.HasValue) 
            {
                item.Amount = amount.Value;
                card.CurrentBalance = card.CurrentBalance - amount.Value;
                transaction.RemainingAmount = transaction.RemainingAmount - amount.Value;
            }
            else if (Decimal.Compare(transaction.RemainingAmount, card.CurrentBalance) >= 0) 
            {
                item.Amount = card.CurrentBalance;
                transaction.RemainingAmount = transaction.RemainingAmount - card.CurrentBalance;
                card.CurrentBalance = 0.00M;
            }
            else 
            {
                item.Amount = transaction.RemainingAmount;
                card.CurrentBalance = card.CurrentBalance - transaction.RemainingAmount;
                transaction.RemainingAmount = 0.00M;
            }

            var responseInfo = new Dictionary<string, object>(requestInfo);
            responseInfo[Constants.WALLET_CARD_CURRENT_BALANCE_TINY_KEY] = card.CurrentBalance.ToString("0.00");
            responseInfo[Constants.WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY] = transaction.RemainingAmount.ToString("0.00");

            var responseData = DictionarySerializer.SerializeToByteArray(responseInfo);
            if (responseData != null)
            {
                try
                {
                    Console.WriteLine("Canceling data blocks.");
                    beacon.CancelDataBlocks();
                    
                    Console.WriteLine("Data request response: {0}.", JsonConvert.SerializeObject(responseInfo));
                    beacon.RespondToBleDataRequest(responseData);

                    if (Decimal.Compare(transaction.RemainingAmount, 0.0M) > 0)
                    {
                        TenderCard(transaction);
                    }
                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine("Serialization failed for card redemption request response.");
                RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.SerializationFailed });
            }
        }

        static void RespondToBleDataRequestWithErrors(SerialBeacon beacon, Dictionary<string, object> requestInfo, List<byte> errors)
        {
            requestInfo[Constants.WALLET_ERRORS_TINY_KEY] = errors;
            var responseData = DictionarySerializer.SerializeToByteArray(requestInfo);
            beacon.RespondToBleDataRequest(responseData);
        }

        static void TenderCard(Transaction transaction)
        {
            try
            {
                Dictionary<string, object> transactionInfo = new Dictionary<string, object>();
                transactionInfo[Constants.WALLET_DATA_TYPE_TINY_KEY] = ((int) eWalletDataTypes.TransactionNotification).ToString();
                transactionInfo[Constants.WALLET_TRANSACTION_ID_TINY_KEY] = transaction.ID.ToString();
                transactionInfo[Constants.WALLET_MERCHANT_ID_TINY_KEY] = transaction.Merchant.ID.ToString();
                transactionInfo[Constants.WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY] = transaction.RemainingAmount.ToString("0.00");

                var data = DictionarySerializer.SerializeToByteArray(transactionInfo);
                if (data != null)
                {
                    Console.WriteLine("Tendering card for transaction {0}.", transaction.ID);
                    _selectedBeacon.SendDataBlocks(0, 0, 255, data);
                }
                else
                {
                    Console.WriteLine("Serializing transaction {0} failed.", transaction.ID);
                }
            }
            catch (CommandTimeoutException ex)
            {
                Console.WriteLine("{0} is busy. Wait a moment and try again.", _selectedBeacon);
                Debug.Print("Command Timeout: {0}", ex.GetBaseException().Message);
            }
            catch (Exception ex)
            {
                Debug.Print("Error occurred while tendering card with merchant {0}: {1}", _selectedBeacon, ex.GetBaseException().Message);
                Environment.Exit(1);
            }
            
        }

        static void CancelTransaction(string transactionID)
        {
            if (string.IsNullOrEmpty(transactionID))
            {
                CancelDataBlocks();
            }
            else
            {
                Transaction transaction = _demoDataSource.GetTransaction(transactionID);
                if (transaction == null)
                {
                    Console.WriteLine("Transaction {0} not found.", transactionID);
                }
                else if (transaction.IsCanceled)
                {
                    Console.WriteLine("Transaction {0} was previously canceled at {1}.", transaction.ID, transaction.CanceledAt);
                }
                else if (transaction.IsComplete)
                {
                    Console.WriteLine("Transaction {0} completed.", transaction.ID);
                }
                else
                {
                    transaction.CanceledAt = DateTime.Now;
                    Console.WriteLine("Canceled transaction {0}.", transaction.ID);
                    CancelDataBlocks();
                }
            }
        }

        static void CancelDataBlocks()
        {
            _selectedBeacon.CancelDataBlocks();

        }
    }
}
