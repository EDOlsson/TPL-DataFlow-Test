using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackEnd;
using DataFlow.Tests.TestDoubles;

namespace DataFlow.Tests
{
    public class BackEndServiceTests
    {
        readonly BackEndService _sut;
        readonly DataStore _dataStore = new DataStore();
        readonly SpectraServiceSpy _spectraService = new SpectraServiceSpy();

        public BackEndServiceTests()
        {
            _sut = new BackEndService(_dataStore, _spectraService);
        }

        public void 
    }
}
