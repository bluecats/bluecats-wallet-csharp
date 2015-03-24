using BCWallet.Models;
using BCWallet.Utilities.IO;
using BCWallet.Utilities.Serialization;
using BlueCats.Serial;
using BlueCats.Serial.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace BCWallet
{
    class Program
    {
        private readonly static string _version = "1.0.1";
        private static BCLib _bcLib = null;
        private static SerialPort _serialPort;
        private static bool _isSerialPortAttached = false;
        private static DemoDataSource _demoDataSource;

        static void Main(string[] args)
        {
            Console.WriteLine("BlueCats Wallet Version {0}", _version);
            Console.WriteLine("Enter 'commands' to see a list of commands.");
            Console.WriteLine("");

            _bcLib = new BCLib();
            _bcLib.BleConnectedEvent += _bcLib_BleConnectedEvent;
            _bcLib.BleDisconnectedEvent += _bcLib_BleDisconnectedEvent;
            _bcLib.BleDataRequestEvent += _bcLib_BleDataRequestEvent;
            _bcLib.BleDataBlocksSentEvent += _bcLib_BleDataBlocksSentEvent;
            _bcLib.CancelDataBlocksCommandResponse += _bcLib_CancelDataBlocksCommandResponse;

            Dictionary<string, Merchant> merchantForMerchantID = Merchant.GenerateDemoMerchants();
            List<Merchant> merchants = merchantForMerchantID.Select(kvp => kvp.Value).ToList();
            var cards = Card.GenerateDemoCards(merchants);
            _demoDataSource = new DemoDataSource(merchantForMerchantID, cards);

            Console.WriteLine(_demoDataSource.ToString());

            ConnectionManager.ListenForSerialDeviceConnection(ConnectionManager_SerialDeviceConnected, -1, false);
            ConnectionManager.ListenForSerialDeviceDisconnection(ConnectionManager_SerialDeviceDisconnected, -1, false);
            var ports = ConnectionManager.DiscoverSerialPorts();
            if (ports.Count > 0)
            {
                Console.WriteLine("Attaching to serial port {0}", ports[0].Port);
                _serialPort = ConnectionManager.AttachToSerialPort(ports[0].Port);
                _serialPort.DataReceived += SerialPort_DataReceivedHandler;
                _isSerialPortAttached = true;
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
                    if (!_isSerialPortAttached)
                    {
                        Console.WriteLine("Serial port not attached, connect USB beacon.");
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

        static void _bcLib_CancelDataBlocksCommandResponse(object sender, BlueCats.Serial.Commands.Responses.CommandResponseArgs e)
        {
            Console.WriteLine("Canceled data blocks.");
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

        static void _bcLib_BleDataBlocksSentEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Block data sent!");
        }

        static void _bcLib_BleDisconnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Device disconnected from USB beacon");
        }

        static void _bcLib_BleConnectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Device connected from USB beacon");
        }

        static void _bcLib_BleDataRequestEvent(object sender, BlueCats.Serial.Events.BleDataRequestEventArgs e)
        {
            RespondToBleDataRequest(e.Data);
        }

        static void ConnectionManager_SerialDeviceConnected(string portName)
        {
            Console.WriteLine("Attaching to serial port {0}", portName);
            _serialPort = ConnectionManager.AttachToSerialPort(portName);
            _serialPort.DataReceived += SerialPort_DataReceivedHandler;
            _isSerialPortAttached = true;
        }

        static void ConnectionManager_SerialDeviceDisconnected(string portName)
        {
            Console.WriteLine("Detaching from serial port {0}", portName);
            ConnectionManager.DetachFromSerialPort(portName);
            _serialPort.DataReceived -= SerialPort_DataReceivedHandler;
            _isSerialPortAttached = false;
        }

        static void SerialPort_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            byte[] data = new byte[sp.BytesToRead];
            sp.Read(data, 0, sp.BytesToRead);
            foreach (byte ch in data)
            {
                _bcLib.Parse(ch);
            }
        }

        static void RespondToBleDataRequest(byte[] requestData)
        {
            var requestInfo = DictionarySerializer.DeserializeFromByteArray(requestData);
            if (requestInfo != null)
            {
                var dataTypeString = (string)requestInfo[WalletConstants.WALLET_DATA_TYPE_TINY_KEY];
                if (!string.IsNullOrEmpty(dataTypeString))
                {

                    Console.WriteLine("Received data request {0}.", JsonConvert.SerializeObject(requestInfo));

                    if (string.Compare(dataTypeString, ((byte)eWalletDataTypes.CardBalanceRequest).ToString(), true) == 0)
                    {
                        RespondToCardBalanceRequest(requestInfo);
                    }
                    else if (string.Compare(dataTypeString, ((byte)eWalletDataTypes.CardRedmeptionRequest).ToString(), true) == 0)
                    {
                        RespondToCardRedemptionRequest(requestInfo);
                    }
                    else
                    {
                        Console.WriteLine("Ble data request type {0} not supported.", dataTypeString);
                        RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeNotSupported });
                    }
                }
                else
                {
                    Console.WriteLine("Ble data request JSOn invlaid.");
                    RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.JSONInvalid });
                }
            }
            else
            {
                Console.WriteLine("Ble data request type missing.");
                RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.RequestTypeMissing });
            }
        }

        static void RespondToCardBalanceRequest(Dictionary<string, object> requestInfo)
        {
            var barcode = (string)requestInfo[WalletConstants.WALLET_CARD_BARCODE_TINY_KEY];
            var merchantID = (string)requestInfo[WalletConstants.WALLET_MERCHANT_ID_TINY_KEY];

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
                RespondToBleDataRequestWithErrors(requestInfo, errors);
                return;
            }

            
            var card = _demoDataSource.GetCard(merchantID, barcode);
            if (card == null)
            {
                Console.WriteLine("Card {0} for merchant {1} not found.", barcode, merchantID);
                RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.CardNotFound });
                return;
            }

            var responseInfo = new Dictionary<string, object>(requestInfo);
            responseInfo[WalletConstants.WALLET_CARD_CURRENT_BALANCE_TINY_KEY] = card.CurrentBalance.ToString("0.00");
            var responseData = DictionarySerializer.SerializeToByteArray(responseInfo);
            if (responseData != null)
            {
                Console.WriteLine("Data request response: {0}.", JsonConvert.SerializeObject(responseInfo));

                var command = BCCommandBuilder.BuildRespondToBLEDataRequestCommand(responseData);
                _bcLib.SendCommand(_serialPort, command);
            }
            else
            {
                Console.WriteLine("Serialization failed for card balance request response.");
                RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.SerializationFailed });
            }
        }

        static void RespondToCardRedemptionRequest(Dictionary<string, object> requestInfo)
        {
            var barcode = (string)requestInfo[WalletConstants.WALLET_CARD_BARCODE_TINY_KEY];
            var merchantID = (string)requestInfo[WalletConstants.WALLET_MERCHANT_ID_TINY_KEY];
            var transactionID = (string)requestInfo[WalletConstants.WALLET_TRANSACTION_ID_TINY_KEY];
            var deviceID = (string)requestInfo[WalletConstants.WALLET_DEVICE_ID_TINY_KEY];

            decimal? amount = null;
            
            if (requestInfo.ContainsKey(WalletConstants.WALLET_AMOUNT_TINY_KEY)) 
            {
                var amountString = (string)requestInfo[WalletConstants.WALLET_AMOUNT_TINY_KEY];
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
                RespondToBleDataRequestWithErrors(requestInfo, errors);
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
                RespondToBleDataRequestWithErrors(requestInfo, errors);
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
            responseInfo[WalletConstants.WALLET_CARD_CURRENT_BALANCE_TINY_KEY] = card.CurrentBalance.ToString("0.00");
            responseInfo[WalletConstants.WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY] = transaction.RemainingAmount.ToString("0.00");

            byte[] responseData = DictionarySerializer.SerializeToByteArray(responseInfo);
            if (responseData != null)
            {
                byte[] command = BCCommandBuilder.BuildCancelDataBlocksCommand();
                _bcLib.SendCommand(_serialPort, command);

                Console.WriteLine("Data request response: {0}.", JsonConvert.SerializeObject(responseInfo));

                command = BCCommandBuilder.BuildRespondToBLEDataRequestCommand(responseData);
                _bcLib.SendCommand(_serialPort, command);

                if (Decimal.Compare(transaction.RemainingAmount, 0.0M) > 0)
                {
                    TenderCard(transaction);
                }
            }
            else
            {
                Console.WriteLine("Serialization failed for card redemption request response.");
                RespondToBleDataRequestWithErrors(requestInfo, new List<byte> { (byte)eWalletErrors.SerializationFailed });
            }
        }

        static void RespondToBleDataRequestWithErrors(Dictionary<string, object> requestInfo, List<byte> errors)
        {
            requestInfo[WalletConstants.WALLET_ERRORS_TINY_KEY] = errors;
            var responseData = DictionarySerializer.SerializeToByteArray(requestInfo);
            var command = BCCommandBuilder.BuildRespondToBLEDataRequestCommand(responseData);
            _bcLib.SendCommand(_serialPort, command);
        }

        static void TenderCard(Transaction transaction)
        {
            Dictionary<string, object> transactionInfo = new Dictionary<string, object>();
            transactionInfo[WalletConstants.WALLET_DATA_TYPE_TINY_KEY] = ((int)eWalletDataTypes.TransactionNotification).ToString();
            transactionInfo[WalletConstants.WALLET_TRANSACTION_ID_TINY_KEY] = transaction.ID.ToString();
            transactionInfo[WalletConstants.WALLET_MERCHANT_ID_TINY_KEY] = transaction.Merchant.ID.ToString();
            transactionInfo[WalletConstants.WALLET_TRANSACTION_REMAINING_AMOUNT_TINY_KEY] = transaction.RemainingAmount.ToString("0.00");

            var data = DictionarySerializer.SerializeToByteArray(transactionInfo);
            if (data != null)
            {
                Console.WriteLine("Tendering loyal card for transaction {0}.", transaction.ID);
                var command = BCCommandBuilder.BuildSendDataBlocksCommand(0, 0, 255, data);
                _bcLib.SendCommand(_serialPort, command);
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
            if (_isSerialPortAttached)
            {
                var command = BCCommandBuilder.BuildCancelDataBlocksCommand();
                _bcLib.SendCommand(_serialPort, command);
            }
        }
    }
}
