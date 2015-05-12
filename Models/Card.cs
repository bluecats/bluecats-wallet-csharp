using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCats.Wallet.Models
{
    public class Card
    {
        public Merchant Merchant { get; set; }
        public string Barcode { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }

        public static List<Card> GenerateDemoCards(List<Merchant> merchants)
        {
            var cards = new List<Card>();
            var random = new Random();
            foreach (Merchant merchant in merchants)
            {
                decimal openingBalance = (decimal)random.Next(25, 250);
                Card card = new Card
                {
                    Merchant = merchant,
                    Barcode = "bluecats",
                    OpeningBalance = openingBalance,
                    CurrentBalance = openingBalance
                };
                cards.Add(card);
            }
            return cards;
        }
    }
}
