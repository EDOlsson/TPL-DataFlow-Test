using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowDemo
{
    public interface IDataStore
    {
        Task<SessionIdentifier> InitializeAsync(int age);
    }
}
