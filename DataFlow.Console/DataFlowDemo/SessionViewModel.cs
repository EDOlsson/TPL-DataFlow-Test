using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowDemo
{
    class SessionViewModel : INotifyPropertyChanged
    {
        readonly IDataStore _dataStore;
        readonly int _age;

        string _status;
        SessionIdentifier _sessionId;

        public SessionViewModel(IDataStore dataStore, int age)
        {
            _dataStore = dataStore;
            _age = age;
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public string Status
        {
            get { return _status; }
            set
            {
                if (string.Equals(_status, value, StringComparison.OrdinalIgnoreCase))
                    return;

                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public async Task InitializeAsync()
        {
            Status = "Initializing...";

            _sessionId = await _dataStore.InitializeAsync(_age);

            Status = _sessionId.ToString();
        }
    }
}
