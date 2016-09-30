using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowDemo
{
    public interface IBackEndService
    {
        Task<SessionIdentifier> InitializeAsync(int age);

        Task PostDataAsync(SessionIdentifier id, Data data);

        Task<object> FetchAnalyticsAsync(SessionIdentifier id);
    }
}
