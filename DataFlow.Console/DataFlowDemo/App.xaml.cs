using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DataFlowDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var assembly = System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackEnd.dll"));

            var config = new ContainerConfiguration()
                                .WithAssembly(assembly);

            var compositionHost = config.CreateContainer();
            compositionHost.SatisfyImports(this);

            System.Diagnostics.Debug.Assert(BackEndService != null);

            var traceMessages = new TraceListenerViewModel();

            var mainViewModel = new MainViewModel(BackEndService, traceMessages);

            var mainView = new MainView { DataContext = mainViewModel };

            mainView.Show();
        }

        [Import]
        internal IBackEndService BackEndService { get; set; }
    }
}
