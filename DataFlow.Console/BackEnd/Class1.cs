using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DataFlowDemo;

namespace BackEnd
{
    public class BackEndService
    {
        readonly IDataStore _dataStore;

        public BackEndService(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public async Task<SessionIdentifier> InitializeAsync(int age)
        {
            return await _dataStore.InitializeAsync(age);
        }

        public async Task PostData(Data data)
        {
            return;
        }
    }
}
