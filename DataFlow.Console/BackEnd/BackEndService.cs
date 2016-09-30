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
    [Export(typeof(IBackEndService))]
    public class BackEndService : IBackEndService
    {
        readonly DataStore _dataStore;
        readonly IFakeSpectraService _spectraService;

        readonly TransformBlock<BackEndRequest, ServiceRequest> _dataStoreBlock;
        readonly TransformBlock<ServiceRequest, object> _spectraServiceBlock;

        public BackEndService() : this(new DataStore(), new FakeSpectraService()) { }

        internal BackEndService(DataStore dataStore, IFakeSpectraService spectraService)
        {
            _dataStore = dataStore;
            _spectraService = spectraService;
        }

        public async Task<SessionIdentifier> InitializeAsync(int age)
        {
            return await _dataStore.InitializeAsync(age);
        }

        public async Task PostDataAsync(SessionIdentifier id, Data data)
        {
            return;
        }

        public async Task<object> FetchAnalyticsAsync(SessionIdentifier id)
        {
            return new object();
        }

        enum BackEndRequest
        {
            Initialize = 0,
            PostData,
            FetchAnalytics,
            DeleteSession,
            DeleteAll
        }

        abstract class ServiceRequest { }
    }
}
