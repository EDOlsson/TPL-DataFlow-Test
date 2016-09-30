using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackEnd;
using DataFlowDemo;

namespace DataFlow.Tests.TestDoubles
{
    class SpectraServiceSpy : IFakeSpectraService
    {
        public void PostFetalData(SessionIdentifier id, Data d) => { }

        public object FetchAnalytics(SessionIdentifier id) => id;
    }
}
