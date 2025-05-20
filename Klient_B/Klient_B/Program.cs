using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using ExtensionClass;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace Klient_B
{
    internal class Program
    {
        static bool CzyPotwierdzanie = false;
        private static Task HandlePotwierdzenie(ConsumeContext<PytanieoPotwierdzenie> ctx)
        {
            CzyPotwierdzanie = true;

            ConsoleCol.WriteLine($"\n[PYTANIE O POTWIERDZENIE] Sklep pyta o {ctx.Message.Ilosc} szt. Czy potwierdzasz? (T/N): ", ConsoleColor.Yellow);
            var odp = Console.ReadLine();

            if (odp == "T" || odp == "t")
            {
                return ctx.Publish(new Potwierdzenie
                {
                    OrderId = ctx.Message.OrderId,
                });
            }
            else if (odp == "N" || odp == "n")
            {
                return ctx.Publish(new BrakPotwierdzenia
                {
                    OrderId = ctx.Message.OrderId,
                });
            }
            else
            {
                ConsoleCol.WriteLine($"\n[BLAD]: nie ma takiej opcji", ConsoleColor.Red);
                return Task.CompletedTask;
            }
        }

        static void Main(string[] args)
        {

            ConsoleCol.WriteLine("Jestem: Klientem B", ConsoleColor.Cyan);

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://ostrich.lmq.cloudamqp.com/xbgnncht"), h =>
                {
                    h.Username("xbgnncht");
                    h.Password("KCR-knMzxad3qG41vnwjKxnwnD9-Ud8e");
                });

                cfg.ReceiveEndpoint("client_b_queue", e =>
                {
                    e.Handler<PytanieoPotwierdzenie>(HandlePotwierdzenie);

                    e.Handler<AkceptacjaZamowienia>(context =>
                    {
                        CzyPotwierdzanie = false;
                        ConsoleCol.WriteLine($"[INFO] Zamowienie zaakceptowane przez sklep ({context.Message.Ilosc})", ConsoleColor.Green);
                        return Task.CompletedTask;
                    });

                    e.Handler<OdrzucenieZamowienia>(context =>
                    {
                        CzyPotwierdzanie = false;
                        ConsoleCol.WriteLine($"[INFO] Zamowienie odrzucone przez sklep ({context.Message.Ilosc})", ConsoleColor.DarkRed);
                        return Task.CompletedTask;
                    });
                });

            });

            bus.Start();

            while (true)
            {
                if (CzyPotwierdzanie)
                {
                    Thread.Sleep(100);
                    continue;
                }

                ConsoleCol.WriteLine("\n [ZAMAWIANIE] Podaj ilosc sztuk zamowienia", ConsoleColor.Yellow);
                var input = Console.ReadLine();

                if (input == null || input == "0")
                {
                    continue;
                }
                int liczba = int.Parse(input);

                Guid orderID = Guid.NewGuid();

                bus.Publish(new StartZamowienia { OrderId = orderID, Ilosc = liczba, QueueName = "client_b_queue" });

                ConsoleCol.WriteLine("\n[ZAMAWIANIE] Zostalo wyslane zamowienie", ConsoleColor.DarkYellow);
                CzyPotwierdzanie = true;
            }

            bus.Stop();
        }
    }
}
