using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using ExtensionClass;
using System.Threading;


namespace Magazyn
{
    internal class Program
    {
        private static int wolne = 0;
        private static int zarezerwowane = 0;

        private static Task HandlePytanieoWolne(ConsumeContext<PytanieoWolne> ctx)
        {
            var ilosc = ctx.Message.Ilosc;
            if (ilosc >= wolne)
            {
                ConsoleCol.WriteLine("\n[BRAK] Nie ma wystarczajacel liczby elementow w magazynie", ConsoleColor.Red);
                return ctx.Publish(new OdpowiedzWolneNegatywna
                {
                    OrderId = ctx.Message.OrderId
                });
            }
            else
            {
                wolne -= ilosc;
                zarezerwowane += ilosc;

                ConsoleCol.WriteLine($"\n[REZERWACJA] Magazyn zarezerwował {ilosc} produktu", ConsoleColor.Red);
                return ctx.Publish(new OdpowiedzWolne
                {
                    OrderId = ctx.Message.OrderId
                });
            }
        }

        private static Task HandleAkceptacja(ConsumeContext<AkceptacjaZamowienia> ctx)
        {
            var ilosc = ctx.Message.Ilosc;

            if (ilosc <= zarezerwowane)
            {
                zarezerwowane -= ilosc;
                ConsoleCol.WriteLine($"\n[AKCEPTACJA] Zaakceptowano zamowienie {ilosc} produktu ", ConsoleColor.Green);
            }
            return Task.CompletedTask;
        }

        private static Task HendleOdrzucenie(ConsumeContext<OdrzucenieZamowienia> ctx)
        {
            var ilosc = ctx.Message.Ilosc;

            if (ilosc <= zarezerwowane)
            {
                zarezerwowane -= ilosc;
                wolne += ilosc;
                ConsoleCol.WriteLine($"\n[ODRZUCENIE] Odrzucono zamowienie ", ConsoleColor.Red);
            }
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            ConsoleCol.WriteLine("Jestem: Magazyn", ConsoleColor.Cyan);

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://ostrich.lmq.cloudamqp.com/xbgnncht"), h =>
                {
                    h.Username("xbgnncht");
                    h.Password("KCR-knMzxad3qG41vnwjKxnwnD9-Ud8e");
                });

                cfg.ReceiveEndpoint("magazyn_queue", e =>
                {

                    e.Handler<PytanieoWolne>(HandlePytanieoWolne);
                    e.Handler<AkceptacjaZamowienia>(HandleAkceptacja);
                    e.Handler<OdrzucenieZamowienia>(HendleOdrzucenie);
                });
            });



            bus.Start();

            var displayTask = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(3000);

                    ConsoleCol.WriteLine($"\n [STAN MAGAZYNU]", ConsoleColor.Blue);
                    ConsoleCol.WriteLine($" [WOLNE]: {wolne}", ConsoleColor.Blue);
                    ConsoleCol.WriteLine($" [ZAREZERWOWANE]: {zarezerwowane}", ConsoleColor.Blue);
                }
            });

            while (true)
            {
                ConsoleCol.WriteLine("\n [DODANIE] Podaj ilosc sztuk jaka chcesz dodac: ", ConsoleColor.Yellow);
                var input = Console.ReadLine();

                if (input == null || input == "0")
                {
                    continue;
                }
                int liczba = int.Parse(input);

                wolne += liczba;

                ConsoleCol.WriteLine($"\n[DODANO] {liczba} sztuk produktu", ConsoleColor.DarkYellow);

            }

            bus.Stop();
        }
    }
}

