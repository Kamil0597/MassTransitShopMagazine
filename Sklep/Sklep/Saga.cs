using Automatonymous;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklep
{
    public class Saga : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid OrderId { get; set; }

        public string KlientQueue { get; set; }

        public bool PotwierdzenieMagazyn {  get; set; }
        public bool PotwierdzenieKlienta {  get; set; }

        public int Ilosc { get; set; }

        public Guid? timeoutId { get; set; }
    }

    public class Timeout : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
