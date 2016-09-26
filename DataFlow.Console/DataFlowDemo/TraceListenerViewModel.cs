using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DataFlowDemo
{
    class TraceListenerViewModel : TraceListener, INotifyPropertyChanged
    {
        readonly Dispatcher _dispatcher;

        TraceMessageViewModel _lastMessage;

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public ObservableCollection<TraceMessageViewModel> Messages { get; }

        public TraceListenerViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            Messages = new ObservableCollection<TraceMessageViewModel>();

            Trace.Listeners.Add(this);
        }

        public override void Write(string message)
        {
            if (_lastMessage == null)
            {
                _lastMessage = new TraceMessageViewModel(message);
                AddMessageToCollectionOfMessages(_lastMessage);
            }
            else
                _lastMessage.Message += message;
        }

        public override void WriteLine(string message)
        {
            if (_lastMessage == null)
                AddMessageToCollectionOfMessages(new TraceMessageViewModel(message));
            else
            {
                _lastMessage.Message += message;
                _lastMessage = null;
            }
        }

        void AddMessageToCollectionOfMessages(TraceMessageViewModel message)
        {
            _dispatcher.Invoke(() => Messages.Insert(0, message));
        }
    }

    class TraceMessageViewModel : INotifyPropertyChanged
    {
        string _message;

        public DateTimeOffset Timestamp { get; }

        public string Message
        {
            get { return _message; }
            set
            {
                if (string.Equals(_message, value, StringComparison.OrdinalIgnoreCase))
                    return;

                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        public TraceMessageViewModel(string message)
        {
            Timestamp = DateTimeOffset.Now;
            Message = message;
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public override string ToString() => $"{Timestamp:h:MM:ss:fff} {Message}";
    }
}
