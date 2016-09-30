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
                var bed = _dataStore.TheData[postRequest.Id];
                return new PostFetalDataServiceRequest(bed, postRequest.FetalData);
            }

            var fetchAnalytics = request as FetchIdForRetrievingAnalyticsDataStoreRequest;
            if (fetchAnalytics != null)
            {
                var bed = _dataStore.TheData[fetchAnalytics.Id];
                return new FetchAnalyticsServiceRequest(bed);
            }

            return ServiceRequest.EmptyRequest;
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
                _spectraService.PostFetalData(postRequest.Id, postRequest.FetalData);
                return null;
            }

            var fetchRequest = request as FetchAnalyticsServiceRequest;
            if (fetchRequest != null)
                return _spectraService.FetchAnalytics(fetchRequest.Id);


            System.Diagnostics.Trace.WriteLine($"WARN - was not able to process request {request}.");
            return null;
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

        class ServiceRequest
        {
            public static readonly ServiceRequest EmptyRequest = new ServiceRequest();

            protected ServiceRequest() { }

            public override string ToString() => "Empty request";
        }

        class NoOpServiceRequestWithId : ServiceRequest
        {
            public SessionIdentifier Id { get; }

            public NoOpServiceRequestWithId(SessionIdentifier id)
            {
                Id = id;
            }

            public override string ToString() => string.Format($"No-op service request with Id {Id}.");
        }

        class PostFetalDataServiceRequest : ServiceRequest
        {
            public BedId Id { get; }

            public Data FetalData { get; }

            public PostFetalDataServiceRequest(BedId id, Data fetalData)
            {
                Id = id;
                FetalData = fetalData;
            }

            public override string ToString() => string.Format($"Post fetal data for session {Id} : {FetalData}.");
        }

        class FetchAnalyticsServiceRequest : ServiceRequest
        {
            public BedId Id { get; }

            public FetchAnalyticsServiceRequest(BedId id)
            {
                Id = id;
            }

            public override string ToString() => string.Format($"Fetch analytics for session {Id}.");
        }


        abstract class DataStoreRequest { }

        class CreateSessionRequest : DataStoreRequest
        {
            public int Age { get; }

            public CreateSessionRequest(int age)
            {
                Age = age;
            }
        }

        abstract class FetchIdFromDataStoreRequest : DataStoreRequest
        {
            public SessionIdentifier Id { get; }

            protected FetchIdFromDataStoreRequest(SessionIdentifier id)
            {
                Id = id;
            }
        }

        class FetchIdForRetrievingAnalyticsDataStoreRequest : FetchIdFromDataStoreRequest
        {
            public FetchIdForRetrievingAnalyticsDataStoreRequest(SessionIdentifier id) : base(id)
            {
            }
        }

        class DeleteSessionRequest : FetchIdFromDataStoreRequest
        {
            public DeleteSessionRequest(SessionIdentifier id) : base(id)
            {
            }
        }

        class PostDataToSessionRequest : FetchIdFromDataStoreRequest
        {
            public Data FetalData { get; }

            public PostDataToSessionRequest(SessionIdentifier id, Data fetalData) : base(id)
            {
                FetalData = fetalData;
            }
        }

        class DeleteAllSessionsRequest : DataStoreRequest
        {

        }

    }
}
