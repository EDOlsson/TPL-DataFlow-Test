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
    class DataStore
    {
        readonly SerializedDictionary<string> _dataStore = new SerializedDictionary<string>();

        public DataStore()
        {

        } 

        public async Task<SessionIdentifier> InitializeAsync(int age)
        {
            return await _dataStore.AddNew(age);
        }

        class SerializedDictionary<T>
        {
            readonly Dictionary<SessionIdentifier, T> _dataStore = new Dictionary<SessionIdentifier, T>();
            readonly TransformBlock<DataStoreRequest, SessionIdentifier> _queue;
            readonly TransformBlock<SessionIdentifier, T> _dictionaryBlock;

            public SerializedDictionary()
            {
                _queue = new TransformBlock<DataStoreRequest, T>(r => ProcessRequest(r));
            }

            public async Task<T> AddNew(int age)
            {
                _queue.Post(new CreateSessionRequest(age));

                return await _queue.ReceiveAsync();
            }

            public async Task<T> Fetch(SessionIdentifier id)
            {
                _queue.Post(new FetchSessionRequest(id));

                return await _queue.ReceiveAsync();
            }

            public Task Delete(SessionIdentifier id)
            {
                _queue.Post(new DeleteSessionRequest(id));

                var t = _queue.ReceiveAsync();
                t.ConfigureAwait(false);

                return t;
            }

            public Task DeleteAll()
            {
                _queue.Post(new DeleteAllSessionsRequest());

                var t = _queue.ReceiveAsync();
                t.ConfigureAwait(false);

                return t;
            }

            T ProcessRequest(DataStoreRequest request)
            {
                var createRequest = request as CreateSessionRequest;
                if (createRequest != null)
                {
                    return CreateNewSession(createRequest);
                }

                var fetchRequest = request as FetchSessionRequest;
                if (fetchRequest != null)
                    return _dataStore[fetchRequest.Id];

                var deleteRequest = request as DeleteSessionRequest;
                if(deleteRequest != null)
                {
                    _dataStore.Remove(deleteRequest.Id);
                    return default(T);
                }

                var deleteAllRequest = request as DeleteAllSessionsRequest;
                if(deleteAllRequest != null)
                {
                    _dataStore.Clear();
                    return default(T);
                }

                throw new InvalidOperationException($"Unable to process {request}.");
            }

            T CreateNewSession(CreateSessionRequest r)
            {
                var id = new SessionIdentifier(_dataStore.Keys.Select(k => k.Id).Max() + 1);
                var value = default(T);

                _dataStore.Add(id, value);

                return value;
            }
        }
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

    class DeleteAllSessionsRequest : DataStoreRequest
    {

    }
}
