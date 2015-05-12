using System;
using System.Collections.Generic;

namespace BlueCats.Wallet.Models
{
    public class Transaction
    {
        public int ID { get; set; }
        public Merchant Merchant { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime? CanceledAt { get; set; }

        public List<TenderLineItem> TenderLineItems { get; set; }

        public bool IsCanceled
        {
            get
            {
                return CanceledAt.HasValue;
            }
        }

        public bool IsComplete
        {
            get
            {
                return CanceledAt == null && Decimal.Compare(RemainingAmount, 0.00M) <= 0;
            }
        }

        public Transaction()
        {
            TenderLineItems = new List<TenderLineItem>();
        }

        public int GetNextTenderLineItemID()
        {
            int nextID = 1;
            foreach (TenderLineItem item in this.TenderLineItems)
            {
                if (item.ID >= nextID)
                {
                    nextID = item.ID + 1;
                }
            }
            return nextID;
        }

        public void AddTenderLineItem(TenderLineItem item)
        {
            item.ID = GetNextTenderLineItemID();
            this.TenderLineItems.Add(item);
        }
    }
}
