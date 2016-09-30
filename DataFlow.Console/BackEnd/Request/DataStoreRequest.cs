using DataFlowDemo;

namespace BackEnd.Request
{
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
