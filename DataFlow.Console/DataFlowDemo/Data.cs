using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowDemo
{

    public class Data
    {
        public string RawData { get; }

        public Data(string rawData)
        {
            RawData = rawData;
        }

        public override string ToString() => RawData;
    }
}
