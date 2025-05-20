using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiadomosci
{
    public class StartZamowienia
    {
        public Guid OrderId { get; set; }
        public int Ilosc { get; set; }
        public string QueueName { get; set; }
    }

    public class PytanieoPotwierdzenie
    {
        public Guid OrderId { get; set; }
        public int Ilosc { get; set; }
    }

    public class Potwierdzenie
    {
        public Guid OrderId { get; set; }
    }

    public class BrakPotwierdzenia
    {
        public Guid OrderId { get; set; }
    }

    public class PytanieoWolne
    {
        public Guid OrderId { get; set; }
        public int Ilosc { get; set; }
    }

    public class OdpowiedzWolne
    {
        public Guid OrderId { get; set; }
    }

    public class OdpowiedzWolneNegatywna
    {
        public Guid OrderId { get; set; }
    }

    public class AkceptacjaZamowienia
    {
        public Guid OrderId { get; set; }
        public int Ilosc { get; set; }
    }

    public class OdrzucenieZamowienia
    {
        public Guid OrderId { get; set; }
        public int Ilosc { get; set; }
    }
}
