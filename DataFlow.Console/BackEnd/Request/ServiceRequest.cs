using DataFlowDemo;

namespace BackEnd.Request
{
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
}
