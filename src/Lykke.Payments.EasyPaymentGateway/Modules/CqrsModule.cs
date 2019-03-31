using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Payments.EasyPaymentGateway.Client.Events;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Lykke.Payments.EasyPaymentGateway.Workflow;
using Lykke.Payments.EasyPaymentGateway.Workflow.Commands;
using Lykke.Payments.EasyPaymentGateway.Workflow.Events;
using Lykke.SettingsReader;
using System;
using System.Collections.Generic;

namespace Lykke.Payments.EasyPaymentGateway.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settingsManager;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settingsManager = settingsManager;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var settings = _settingsManager.CurrentValue.EasyPaymentGatewayService;

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = settings.RabbitMq.ConnectionString };

            builder.Register(ctx => new MessagingEngine(ctx.Resolve<ILogFactory>(),
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory(ctx.Resolve<ILogFactory>()))).As<IMessagingEngine>().SingleInstance();

            builder.RegisterType<PaymentSaga>();
            builder.RegisterType<MeCommandHandler>();
            builder.RegisterType<PaymentCommandHandler>();

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            builder.Register(ctx =>
            {
                var engine = new CqrsEngine(ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<IMessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,

                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq",
                        SerializationFormat.ProtoBuf, environment: settings.Environment)),

                    Register.BoundedContext(settings.EasyPaymentGatewayContext)
                        .FailedCommandRetryDelay((long)TimeSpan.FromMinutes(1).TotalMilliseconds)
                        .ListeningCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                        .On("payment-saga-commands")
                        .PublishingEvents(typeof(ProcessingStartedEvent), typeof(TransferCompletedEvent),
                            typeof(CreditCardUsedEvent))
                        .With(settings.EasyPaymentGatewayContext + "-events")
                        .WithCommandsHandler<PaymentCommandHandler>(),

                    Register.BoundedContext(settings.MeContext)
                        .FailedCommandRetryDelay((long)TimeSpan.FromMinutes(1).TotalMilliseconds)
                        .ListeningCommands(typeof(CreateTransferCommand))
                        .On("payment-saga-commands")
                        .PublishingEvents(typeof(TransferCreatedEvent))
                        .With(settings.MeContext + "-events")
                        .WithCommandsHandler<MeCommandHandler>(),

                    Register.Saga<PaymentSaga>("payment-saga")
                        .ListeningEvents(typeof(ProcessingStartedEvent), typeof(TransferCompletedEvent))
                        .From(settings.EasyPaymentGatewayContext).On(settings.EasyPaymentGatewayContext + "-events")
                        .ListeningEvents(typeof(TransferCreatedEvent))
                        .From(settings.MeContext).On(settings.MeContext + "-events")
                        .PublishingCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                        .To(settings.EasyPaymentGatewayContext).With("payment-saga-commands")
                        .PublishingCommands(typeof(CreateTransferCommand))
                        .To(settings.MeContext).With("payment-saga-commands"),

                    Register.DefaultRouting
                        .PublishingCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                        .To(settings.EasyPaymentGatewayContext).With("payment-saga-commands")
                        .PublishingCommands(typeof(CreateTransferCommand))
                        .To(settings.MeContext).With("payment-saga-commands"));
                engine.StartPublishers();
                return engine;
            })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
