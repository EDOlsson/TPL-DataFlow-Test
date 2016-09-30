using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFlowDemo;

namespace BackEnd
{
    class FakeSpectraService : IFakeSpectraService
    {
        Data _lastDataPosted;

        public void PostFetalData(SessionIdentifier id, Data d)
        {
            System.Diagnostics.Trace.WriteLine($"Posting fetal data for {id}.");

            _lastDataPosted = d;
        }

        public object FetchAnalytics(SessionIdentifier id)
        {
            System.Diagnostics.Trace.WriteLine($"Fetching analytics for {id}.");

            return _lastDataPosted?.ToString();
        }
    }

    public interface IFakeSpectraService
    {
        void PostFetalData(SessionIdentifier id, Data d);

        object FetchAnalytics(SessionIdentifier id);
    }
}
