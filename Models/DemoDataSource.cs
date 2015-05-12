using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCats.Serial.Examples.Wallet.Models
{
    public class DemoDataSource
    {
        Dictionary<string, Merchant> _merchantForMerchantID = null;
        Dictionary<string, Transaction> _transactionForTransactionID = null;

        public List<Card> Cards { get; set; }

        public DemoDataSource(Dictionary<string, Merchant> merchantForMerchantID, List<Card> cards)
        {
            _merchantForMerchantID = merchantForMerchantID;
            this.Cards = cards;
            _transactionForTransactionID = new Dictionary<string, Transaction>();
        }

        public Card GetCard(string merchantID, string barcode)
        {
            foreach (Card card in this.Cards)
            {
                if (string.Compare(card.Merchant.ID, merchantID, true) == 0  &&
                    string.Compare(card.Barcode, barcode, true) == 0)
                {
                    return card;
                }
            }
            return null;
        }

        public Merchant GetMerchant(string merchantID)
        {
            Merchant merchant = null;
            if (!string.IsNullOrEmpty(merchantID) && _merchantForMerchantID.ContainsKey(merchantID))
            {
                merchant = _merchantForMerchantID[merchantID];
            }
            return merchant;
        }

        public Transaction GetTransaction(string transactionID)
        {
            Transaction transaction = null;
            if (!string.IsNullOrEmpty(transactionID) && _transactionForTransactionID.ContainsKey(transactionID))
            {
                transaction = _transactionForTransactionID[transactionID];
            }
            return transaction;
        }

        public int GetNextTransactionID()
        {
            int nextID = 1;
            foreach (Transaction transaction in _transactionForTransactionID.Select(kvp => kvp.Value).ToList())
            {
                if (transaction.ID >= nextID)
                {
                    nextID = transaction.ID + 1;
                }
            }
            return nextID;
        }

        public void AddTransaction(Transaction transaction)
        {
            transaction.ID = GetNextTransactionID();
            _transactionForTransactionID.Add(transaction.ID.ToString(), transaction);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Data Source");
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine("");
            sb.AppendLine("Merchants:");
            foreach (Merchant merchant in _merchantForMerchantID.Select(kvp => kvp.Value).ToList())
            {
                sb.AppendLine(string.Format("  ID={0}", merchant.ID));
            }

            sb.AppendLine("");
            sb.AppendLine("Cards:");
            foreach (Card card in this.Cards)
            {
                sb.AppendLine(string.Format("  barcode={0}, merchantID={1}, openingBalance={2}, currentBalance={3}", card.Barcode, card.Merchant.ID, card.OpeningBalance.ToString("0.00"), card.CurrentBalance.ToString("0.00")));
            }

            sb.AppendLine("");
            sb.AppendLine("Transactions:");
            foreach (Transaction transaction in _transactionForTransactionID.Select(kvp => kvp.Value).ToList())
            {
                sb.AppendLine(string.Format("  ID={0}, merchantID={1}, totalAmount={2}, remainingAmount={3}", transaction.ID, transaction.Merchant.ID, transaction.TotalAmount.ToString("0.00"), transaction.RemainingAmount.ToString("0.00")));
                sb.AppendLine("    Tender Line Items:");
                foreach (TenderLineItem item in transaction.TenderLineItems)
                {
                    sb.AppendLine(string.Format("      ID={0}, barcode={1}, amount={2}, deviceID={3}", item.ID, item.Card.Barcode, item.Amount.ToString("0.00"), item.DeviceID));
                }
            }
            sb.AppendLine("");

            sb.AppendLine("------------------------------------------------------");

            return sb.ToString();
        }
    }
}
