using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DataFlowDemo;

namespace BackEnd
{
    class DataStore
    {
        public DataStore()
        {
            TheData = new Dictionary<SessionIdentifier, int>();
        }

        public SessionIdentifier Initialize(int age)
        {
            var id = new SessionIdentifier(CalculateNextId());

            TheData[id] = TheData.Values.Any() ? TheData.Values.Max() + 10 : 10;

            return id;
        }

        int CalculateNextId()
        {
            if (!TheData.Keys.Any())
                return 0;

            return TheData.Keys.Max(k => k.Id) + 1;
        }

        internal IDictionary<SessionIdentifier, int> TheData { get; }
    }
}
