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
            TheData = new Dictionary<SessionIdentifier, BedId>();
        }

        public SessionIdentifier Initialize(int age)
        {
            var id = new SessionIdentifier(CalculateNextId());

            TheData[id] = new BedId(TheData.Values.Any() ? TheData.Values.Max(v => v.Id) + 10 : 10);

            return id;
        }

        int CalculateNextId()
        {
            if (!TheData.Keys.Any())
                return 0;

            return TheData.Keys.Max(k => k.Id) + 1;
        }

        internal IDictionary<SessionIdentifier, BedId> TheData { get; }

        public BedId Lookup(SessionIdentifier key)
        {
            if (TheData.ContainsKey(key))
                return TheData[key];

            return BedId.None;
        }

        public void DeleteSession(SessionIdentifier key)
        {
            TheData.Remove(key);
        }

        public void DeleteAllSessions()
        {
            TheData.Clear();
        }
    }
}
