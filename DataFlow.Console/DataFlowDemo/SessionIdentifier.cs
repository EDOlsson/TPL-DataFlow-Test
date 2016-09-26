using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowDemo
{

    public struct SessionIdentifier
    {
        public int Id { get; }

        public DateTimeOffset Created { get; }

        public SessionIdentifier(int id)
        {
            Id = id;
            Created = DateTimeOffset.Now;
        }

        public override string ToString() => $"{Id:00000} created on {Created:g}.";

        public static readonly SessionIdentifier Empty = new SessionIdentifier(0);
    }
}
