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

        readonly TransformBlock<DataStoreRequest, ServiceRequest> _dataStoreBlock;
        readonly TransformBlock<ServiceRequest, object> _spectraServiceBlock;

        public BackEndService() : this(new DataStore(), new FakeSpectraService()) { }

        internal BackEndService(DataStore dataStore, IFakeSpectraService spectraService)
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
                return new PostFetalDataServiceRequest(postRequest.Id, postRequest.FetalData);
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
            public SessionIdentifier Id { get; }

            public Data FetalData { get; }

            public PostFetalDataServiceRequest(SessionIdentifier id, Data fetalData)
            {
                Id = id;
                FetalData = fetalData;
            }

            public override string ToString() => string.Format($"Post fetal data for session {Id} : {FetalData}.");
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

        abstract class IdSpecificDataStoreRequest : DataStoreRequest
        {
            public SessionIdentifier Id { get; }

            protected IdSpecificDataStoreRequest(SessionIdentifier id)
            {
                Id = id;
            }
        }

        class FetchSessionRequest : IdSpecificDataStoreRequest
        {
            public FetchSessionRequest(SessionIdentifier id) : base(id)
            {
            }
        }

        class DeleteSessionRequest : IdSpecificDataStoreRequest
        {
            public DeleteSessionRequest(SessionIdentifier id) : base(id)
            {
            }
        }

        class PostDataToSessionRequest : IdSpecificDataStoreRequest
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
