using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCats.Serial.Examples.Wallet.Models
{
    public class TenderLineItem
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public string DeviceID { get; set; }
        public Card Card { get; set; }
    }
}
