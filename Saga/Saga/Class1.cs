using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Saga
{
    public class Timeout : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }

    public class StartOrder : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int Quantity { get; set; }
    }

    public class ClientConfirm : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }

    public class MagazineConfirm : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }

}
