using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataFlowDemo
{
    class MainViewModel : INotifyPropertyChanged
    {
        readonly IDataStore _dataStore;

        int _age;

        public MainViewModel(IDataStore dataStore)
        {
            _dataStore = dataStore;
            Initialize = new MethodCommand(_ => InitializeSessionAsync());
            Sessions = new ObservableCollection<SessionViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        public ICommand Initialize { get; }

        public int Age
        {
            get { return _age; }
            set
            {
                if (_age == value)
                    return;

                _age = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Age)));
            }
        }

        public ObservableCollection<SessionViewModel> Sessions { get; }

        async Task InitializeSessionAsync()
        {
            var session = new SessionViewModel(_dataStore, Age);
            Sessions.Add(session);

            Age++;

            await session.InitializeAsync();
        }

        class MethodCommand : ICommand
        {
            readonly Action<object> _method;

            public MethodCommand(Action<object> method)
            {
                _method = method;
            }

            public event EventHandler CanExecuteChanged = (s, e) => { };

            public bool CanExecute(object _) => true;

            public void Execute(object parameter)
            {
                _method(parameter);
            }
        }
    }
}
