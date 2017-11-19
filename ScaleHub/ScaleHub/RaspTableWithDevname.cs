using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleHub
{
    class RaspTableWithDevname
    {
        public string Id { get; set; }

        public DateTime createdAt { get; set; }

        public string QRCode { get; set; }

        public string IPAddress { get; set; }

        public string Devname { get; set; }

        public static RaspTableWithDevname initFromTable(RaspberryTable rt, string devname)
        {
            return new RaspTableWithDevname { Id = rt.Id, createdAt = rt.createdAt, QRCode = rt.QRCode, IPAddress = rt.IPAddress, Devname = devname };
        }
    }
}
