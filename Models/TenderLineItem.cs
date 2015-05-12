
namespace BlueCats.Wallet.Models
{
    public class TenderLineItem
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public string DeviceID { get; set; }
        public Card Card { get; set; }
    }
}
