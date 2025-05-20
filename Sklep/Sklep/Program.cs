using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using ExtensionClass;
using MassTransit.Saga;

namespace Sklep
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[SKLEP] Uruchamianie...");


            var saga = new SklepSaga();
            var repository = new InMemorySagaRepository<Saga>();

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://ostrich.lmq.cloudamqp.com/xbgnncht"), h =>
                {
                    h.Username("xbgnncht");
                    h.Password("KCR-knMzxad3qG41vnwjKxnwnD9-Ud8e");
                });

                cfg.UseInMemoryScheduler();

                cfg.ReceiveEndpoint("sklep_saga_queue", e =>
                {
                    e.StateMachineSaga(saga, repository);
                });
            });

            bus.Start();

            Console.WriteLine("[SKLEP] Działa. ENTER aby zakończyć.");
            Console.ReadLine();

            bus.Stop();
        }
    }
}
