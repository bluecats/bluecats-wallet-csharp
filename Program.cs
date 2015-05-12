using BlueCats.Serial;
using BlueCats.Serial.Events.EventArgs;
using BlueCats.Wallet.Models;
using BlueCats.Wallet.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueCats.Wallet
{
    class Program
    {
        private readonly static string _version = "1.0.1";
        private static IList<SerialBeacon> _attachedBeacons = new List<SerialBeacon>();
        private static DemoDataSource _demoDataSource;

        static void Main(string[] args)
        {
            Console.WriteLine("BlueCats Wallet Version {0}", _version);
            Console.WriteLine("Enter 'commands' to see a list of commands.");
            Console.WriteLine("");

            Dictionary<string, Merchant> merchantForMerchantID = Merchant.GenerateDemoMerchants();
            List<Merchant> merchants = merchantForMerchantID.Select(kvp => kvp.Value).ToList();
            var cards = Card.GenerateDemoCards(merchants);
            _demoDataSource = new DemoDataSource(merchantForMerchantID, cards);

            Console.WriteLine(_demoDataSource.ToString());

            var beacons = SerialBeaconManager.GetSerialBeacons();
            foreach (SerialBeacon beacon in beacons)
            {
                try
                {
                    beacon.Attach();

                    try
                    {
                        beacon.WriteEventsEnabled(true);
                    }
                    catch { } // enable events not supported in older fw


                    beacon.BleConnectedEvent += _beacon_BleConnectedEvent;
                    beacon.BleConnectedEvent += _beacon_BleDisconnectedEvent;
                    beacon.BleDataRequestEvent += _beacon_BleDataRequestEvent;
                    beacon.BleDataBlocksSentEvent += _beacon_BleDataBlocksSentEvent;

                    Console.WriteLine("Attached to {0}", beacon);

                    _attachedBeacons.Add(beacon);
                }
                catch
                {
                    beacon.BleConnectedEvent -= _beacon_BleConnectedEvent;
                    beacon.BleConnectedEvent -= _beacon_BleDisconnectedEvent;
                    beacon.BleDataRequestEvent -= _beacon_BleDataRequestEvent;
                    beacon.BleDataBlocksSentEvent -= _beacon_BleDataBlocksSentEvent;

                    if (beacon.IsAttached)
                        beacon.Detach();

                    Console.WriteLine("Failed to attach {0}", beacon);
                }
            }

            bool quit = false;
            while (!quit)
            {
                string line = Console.ReadLine();

                Console.WriteLine("");

                string[] parts = line.Split(' ');
                if (parts.Length > 0)
                {
                    quit = RunWalletCommand(parts);
                }
            }

            foreach (SerialBeacon beacon in _attachedBeacons)
            {
                beacon.BleConnectedEvent -= _beacon_BleConnectedEvent;
                beacon.BleConnectedEvent -= _beacon_BleDisconnectedEvent;
                beacon.BleDataRequestEvent -= _beacon_BleDataRequestEvent;
                beacon.BleDataBlocksSentEvent -= _beacon_BleDataBlocksSentEvent;

                if (beacon.IsAttached)
                    beacon.Detach();
            }
            _attachedBeacons.Clear();
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
                    if (_attachedBeacons.Count <= 0)
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
            Console.WriteLine("");
            Console.WriteLine(" + quit                         quit wallet");
            Console.WriteLine(" + reload                       reload cards to opening balance");
            Console.WriteLine(" + tender merchantID amount     tender card");
            Console.WriteLine(" + cancel [transactionID]       cancel transaction");
            Console.WriteLine(" + datasource                   print data source");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("");
        }

        static void _beacon_BleDataBlocksSentEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Block data sent!");
        }

        static void _beacon_BleDisconnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Device disconnected from USB beacon");
        }

        static void _beacon_BleConnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Device connected from USB beacon");
        }

        static void _beacon_BleDataRequestEvent(object sender, BleDataRequestEventArgs e)
        {
            RespondToBleDataRequest((SerialBeacon)sender, e.Data);
        }

        static void RespondToBleDataRequest(SerialBeacon beacon, byte[] requestData)
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
                        Console.WriteLine("Ble data request type {0} not supported.", dataTypeString);
                        RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeNotSupported });
                    }
                }
                else
                {
                    Console.WriteLine("Ble data request JSOn invlaid.");
                    RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.JSONInvalid });
                }
            }
            else
            {
                Console.WriteLine("Ble data request type missing.");
                RespondToBleDataRequestWithErrors(beacon, requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeMissing });
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
                beacon.RespondToBleDataRequest(responseData);
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

            TenderLineItem item = new TenderLineItem
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

            byte[] responseData = DictionarySerializer.SerializeToByteArray(responseInfo);
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
            Dictionary<string, object> transactionInfo = new Dictionary<string, object>();
            transactionInfo[Constants.WALLET_DATA_TYPE_TINY_KEY] = ((int)eWalletDataTypes.TransactionNotification).ToString();
            transactionInfo[Constants.WALLET_TRANSACTION_ID_TINY_KEY] = transaction.ID.ToString();
            transactionInfo[Constants.WALLET_MERCHANT_ID_TINY_KEY] = transaction.Merchant.ID.ToString();
            transactionInfo[Constants.WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY] = transaction.RemainingAmount.ToString("0.00");

            var data = DictionarySerializer.SerializeToByteArray(transactionInfo);
            if (data != null)
            {
                Console.WriteLine("Tendering loyal card for transaction {0}.", transaction.ID);
                var beacon = _attachedBeacons.FirstOrDefault();
                if (beacon != null)
                    beacon.SendDataBlocks(0, 0, 255, data);
            }
            else
            {
                Console.WriteLine("Serializing transaction {0} failed.", transaction.ID);
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
            foreach (SerialBeacon beacon in _attachedBeacons)
            {
                if (beacon.IsAttached)
                    beacon.CancelDataBlocks();
            }
        }
    }
}
