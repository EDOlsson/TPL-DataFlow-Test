using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BackEnd.Request;
using DataFlowDemo;

namespace BackEnd
{
    [Export(typeof(IBackEndService))]
    public class BackEndService : IBackEndService
    {
        readonly DataStore _dataStore;
        readonly ISpectraService _spectraService;

        readonly TransformBlock<DataStoreRequest, ServiceRequest> _dataStoreBlock;
        readonly TransformBlock<ServiceRequest, object> _spectraServiceBlock;

        public BackEndService() : this(new DataStore(), new SlowFakeSpectraService()) { }

        internal BackEndService(DataStore dataStore, ISpectraService spectraService)
        {
            _dataStore = dataStore;
            _spectraService = spectraService;

            _dataStoreBlock = new TransformBlock<DataStoreRequest, ServiceRequest>(r => ProcessRequest(r));
            _spectraServiceBlock = new TransformBlock<ServiceRequest, object>(r => ContactSpectraService(r));

            _dataStoreBlock.LinkTo(_spectraServiceBlock);
        }

        public async Task<SessionIdentifier> InitializeAsync(int age)
        {
            _dataStoreBlock.Post(new CreateSessionRequest(age));

            var sessionId = await _spectraServiceBlock.ReceiveAsync();

            return (SessionIdentifier)sessionId;
        }

        public async Task PostDataAsync(SessionIdentifier id, Data data)
        {
            _dataStoreBlock.Post(new PostDataToSessionRequest(id, data));

            var t = _spectraServiceBlock.ReceiveAsync();
            await t.ConfigureAwait(false);
        }

        public async Task<object> FetchAnalyticsAsync(SessionIdentifier id)
        {
            _dataStoreBlock.Post(new FetchIdForRetrievingAnalyticsDataStoreRequest(id));

            return await _spectraServiceBlock.ReceiveAsync();
        }

        public Task DeleteSessionAsync(SessionIdentifier id)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAllSessionsAsync()
        {
            return Task.CompletedTask;
        }

        ServiceRequest ProcessRequest(DataStoreRequest request)
        {
            var initRequest = request as CreateSessionRequest;
            if (initRequest != null)
            {
                var id = _dataStore.Initialize(initRequest.Age);

                return new NoOpServiceRequestWithId(id);
            }

            var postRequest = request as PostDataToSessionRequest;
            if(postRequest != null)
            {
                var bed = LookupBedIdFromSessionId(postRequest.Id);
                return new PostFetalDataServiceRequest(bed, postRequest.FetalData);
            }

            var fetchAnalytics = request as FetchIdForRetrievingAnalyticsDataStoreRequest;
            if (fetchAnalytics != null)
            {
                var bed = LookupBedIdFromSessionId(fetchAnalytics.Id);
                return new FetchAnalyticsServiceRequest(bed);
            }

            return ServiceRequest.EmptyRequest;
        }

        BedId LookupBedIdFromSessionId(SessionIdentifier id)
        {
            var bed = _dataStore.Lookup(id);
            if (bed.Equals(BedId.None))
                System.Diagnostics.Trace.WriteLine($"WARN - Session Id {id} was not found!");

            return bed;
        }

        object ContactSpectraService(ServiceRequest request)
        {
            System.Diagnostics.Trace.WriteLine($"Processing request {request}.");

            var initializeRequest = request as NoOpServiceRequestWithId;
            if (initializeRequest != null)
                return initializeRequest.Id;

            var postRequest = request as PostFetalDataServiceRequest;
            if(postRequest != null)
            {
                if (!postRequest.Id.Equals(BedId.None))
                    _spectraService.PostFetalData(postRequest.Id, postRequest.FetalData);

                return null;
            }

            var fetchRequest = request as FetchAnalyticsServiceRequest;
            if (fetchRequest != null)
            {
                if(!fetchRequest.Id.Equals(BedId.None))
                    return _spectraService.FetchAnalytics(fetchRequest.Id);

                return null;
            }


            System.Diagnostics.Trace.WriteLine($"WARN - was not able to process request {request}.");
            return null;
        }
    }
}
