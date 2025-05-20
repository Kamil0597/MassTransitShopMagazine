using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using System.Threading;

namespace Sklep
{
    public class SklepSaga : MassTransitStateMachine<Saga>
    {
        public State Czeka { get; private set; }

        public Event<StartZamowienia> StartZamowieniaEvent { get; private set; }
        public Event<OdpowiedzWolne> MagazynPotwierdzilIloscProd { get; private set; }
        public Event<OdpowiedzWolneNegatywna> MagazynNiePotwierdzilIloscProd { get; private set; }
        public Event<Potwierdzenie> PotwierdzenieOdKlienta { get; private set; }
        public Event<BrakPotwierdzenia> BrakPotwierdzeniaOdKlienta { get; private set; }

        public Schedule<Saga, Timeout> TO { get; private set; }

        public SklepSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => StartZamowieniaEvent, x => x.CorrelateById(s => s.Message.OrderId));
            Event(() => MagazynPotwierdzilIloscProd, x => x.CorrelateById(s => s.Message.OrderId));
            Event(() => MagazynNiePotwierdzilIloscProd, x => x.CorrelateById(s => s.Message.OrderId));
            Event(() => PotwierdzenieOdKlienta, x => x.CorrelateById(s => s.Message.OrderId));
            Event(() => BrakPotwierdzeniaOdKlienta, x => x.CorrelateById(s => s.Message.OrderId));

            Schedule(() => TO, x => x.timeoutId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(10);
            });

            Initially(
                When(StartZamowieniaEvent)
                .Then(ctx =>
                {
                    ctx.Instance.Ilosc = ctx.Data.Ilosc;
                    ctx.Instance.KlientQueue = ctx.Data.QueueName;
                })
                .Schedule(TO, ctx => new Timeout { CorrelationId = ctx.Instance.CorrelationId })
                .ThenAsync(async ctx =>
                {
                    var endpointMagazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));
                    await endpointMagazyn.Send(new PytanieoWolne
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });

                    var endpointKlient = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                    await endpointKlient.Send(new PytanieoPotwierdzenie
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });
                })
                .TransitionTo(Czeka)
            );

            During(Czeka,
                When(MagazynPotwierdzilIloscProd)
                .Then(ctx => ctx.Instance.PotwierdzenieMagazyn = true)
                .If(ctx => ctx.Instance.PotwierdzenieKlienta, then => then.Unschedule(TO)
                .ThenAsync(async ctx =>
                {
                    var client = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                    var magazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));

                    await client.Send(new AkceptacjaZamowienia
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });

                    await magazyn.Send(new AkceptacjaZamowienia
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });
                })
                .Finalize()),

                When(PotwierdzenieOdKlienta)
                .Then(ctx => ctx.Instance.PotwierdzenieKlienta = true)
                .If(ctx => ctx.Instance.PotwierdzenieMagazyn,
                    then => then
                .Unschedule(TO)
                .ThenAsync(async ctx =>
                {
                    var client = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                    var magazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));

                    await client.Send(new AkceptacjaZamowienia
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });

                    await magazyn.Send(new AkceptacjaZamowienia
                    {
                        OrderId = ctx.Instance.CorrelationId,
                        Ilosc = ctx.Instance.Ilosc
                    });
                })
                .Finalize()),

                When(MagazynNiePotwierdzilIloscProd)
                    .Unschedule(TO)
                    .ThenAsync(async ctx =>
                    {
                        var client = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                        var magazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));

                        await client.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });

                        await magazyn.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });
                    })
                    .Finalize(),

                When(BrakPotwierdzeniaOdKlienta)
                    .Unschedule(TO)
                    .ThenAsync(async ctx =>
                    {
                        var client = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                        var magazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));

                        await client.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });

                        await magazyn.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });
                    })
                    .Finalize(),

                When(TO.Received)
                    .ThenAsync(async ctx =>
                    {
                        var client = await ctx.GetSendEndpoint(new Uri($"queue:{ctx.Instance.KlientQueue}"));
                        var magazyn = await ctx.GetSendEndpoint(new Uri("queue:magazyn_queue"));

                        await client.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });

                        await magazyn.Send(new OdrzucenieZamowienia
                        {
                            OrderId = ctx.Instance.CorrelationId,
                            Ilosc = ctx.Instance.Ilosc
                        });
                    })
                    .Finalize()
                    );
        }
    }
}
