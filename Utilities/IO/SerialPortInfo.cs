using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCWallet.Utilities.IO
{
    public struct SerialPortInfo
    {
        public string Name { get; set; }
        public string Port { get; set; }

        public override string ToString()
        {
            return String.Format("Name= {0} Port= {1}", Name, Port);
        }
    }
}
