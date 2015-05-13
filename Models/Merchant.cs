using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCats.Wallet.Models
{
    public class Merchant
    {
        public string ID { get; set; }

        public static Dictionary<string, Merchant> GenerateDemoMerchants(int count)
        {
            var merchantForMerchantID = new Dictionary<string, Merchant>();
            for (int index = 1; index < count; index++)
            {
                var merchantID = string.Format("m{0}", index);
                Merchant merchant = new Merchant
                {
                    ID = merchantID
                };
                merchantForMerchantID.Add(merchantID, merchant);
            }
            return merchantForMerchantID;
        }
    }
}
