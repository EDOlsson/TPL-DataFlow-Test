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
    [Export(typeof(IDataStore))]
    class DataStore : IDataStore
    {
        readonly TransformBlock<int, int> _delayBlock = new TransformBlock<int, int>(async incoming =>
        {
            System.Diagnostics.Trace.WriteLine($"Processing {incoming}...");

            await Task.Delay(5000);

            return incoming;
        });

        readonly TransformBlock<int, SessionIdentifier> _generationBlock = new TransformBlock<int, SessionIdentifier>(age =>
        {
            System.Diagnostics.Trace.WriteLine($"Generating session identifier for {age}...");

            return new SessionIdentifier(age);
        });

        readonly BufferBlock<int> _sourceBlock = new BufferBlock<int>();

        public DataStore()
        {
            _sourceBlock.LinkTo(_delayBlock);
            _delayBlock.LinkTo(_generationBlock);

            _sourceBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)_delayBlock).Fault(t.Exception);
                else
                    _delayBlock.Complete();
            });

            _delayBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    ((IDataflowBlock)_generationBlock).Fault(t.Exception);
                else
                    _generationBlock.Complete();
            });
        }

        public async Task<SessionIdentifier> InitializeAsync(int age)
        {
            await _sourceBlock.SendAsync(age);

            return await _generationBlock.ReceiveAsync();
        }
    }

}
