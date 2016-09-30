using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFlowDemo;

namespace BackEnd
{
    class SlowFakeSpectraService : ISpectraService
    {
        Data _lastDataPosted;

        public void PostFetalData(BedId id, Data d)
        {
            System.Diagnostics.Trace.WriteLine($"Posting fetal data for {id}.");

            Task.Delay(2000).Wait();

            _lastDataPosted = d;
        }

        public object FetchAnalytics(BedId id)
        {
            System.Diagnostics.Trace.WriteLine($"Fetching analytics for {id}.");

            Task.Delay(2000).Wait();

            return _lastDataPosted?.ToString();
        }
    }

    struct BedId
    {
        public int Id { get; }

        public BedId(int id)
        {
            Id = id;
        }

        public static readonly BedId None = new BedId();
    }

    interface ISpectraService
    {
        void PostFetalData(BedId id, Data d);

        object FetchAnalytics(BedId id);
    }
}
