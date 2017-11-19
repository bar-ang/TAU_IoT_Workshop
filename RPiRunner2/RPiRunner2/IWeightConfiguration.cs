using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner2
{
    interface IWeightConfiguration
    {
        void setData(Dictionary<string, string> query);
    }
}
