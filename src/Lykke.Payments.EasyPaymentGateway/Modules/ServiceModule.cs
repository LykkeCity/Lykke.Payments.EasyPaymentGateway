using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Lykke.Common.Log;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Repositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Services;
using Lykke.Payments.EasyPaymentGateway.DomainServices;
using Lykke.Payments.EasyPaymentGateway.Services;
using Lykke.Payments.EasyPaymentGateway.Settings;
using Lykke.Payments.EasyPaymentGateway.Workflow;
using Lykke.Sdk;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.SettingsReader;

namespace Lykke.Payments.EasyPaymentGateway.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var appSettings = _appSettings.CurrentValue;

            var serviceSettings = appSettings.EasyPaymentGatewayService;

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.Register(ctx => new PersonalDataService(appSettings.PersonalDataServiceClient, ctx.Resolve<ILogFactory>()))
                .As<IPersonalDataService>().SingleInstance();

            builder.Register(ctx => new CreditCardsService(appSettings.PersonalDataServiceClient, ctx.Resolve<ILogFactory>()))
                .As<ICreditCardsService>().SingleInstance();

            builder.RegisterExchangeOperationsClient(appSettings.ExchangeOperationsServiceClient);

            builder.RegisterFeeCalculatorClient(appSettings.FeeCalculatorServiceClient.ServiceUrl);

            builder.RegisterLykkeServiceClient(appSettings.ClientAccountServiceClient.ServiceUrl);

            builder.RegisterInstance(appSettings.FeeSettings).AsSelf();

            builder.RegisterType<PaymentUrlProvider>()
                .WithParameter("merchantId", serviceSettings.Merchant.MerchantId)
                .WithParameter("merchantPassword", serviceSettings.Merchant.MerchantPassword)
                .WithParameter("productId", serviceSettings.Merchant.ProductId)
                .WithParameter("webHookStatusUrl", serviceSettings.WebHook.StatusUrl)
                .AsSelf()
                .SingleInstance();

            builder.Register(ctx =>
                new PaymentSystemsRawLog(AzureTableStorage<PaymentSystemRawLogEventEntity>.Create(
                    _appSettings.ConnectionString(i => i.EasyPaymentGatewayService.Db.LogsConnString), "PaymentSystemsLog", ctx.Resolve<ILogFactory>()))
            ).As<IPaymentSystemsRawLog>().SingleInstance();

            builder.Register(ctx =>
                new PaymentTransactionsRepository(
                    AzureTableStorage<PaymentTransactionEntity>.Create(
                        _appSettings.ConnectionString(i => i.EasyPaymentGatewayService.Db.ClientPersonalInfoConnString),
                        "PaymentTransactions", ctx.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureMultiIndex>.Create(
                        _appSettings.ConnectionString(i => i.EasyPaymentGatewayService.Db.ClientPersonalInfoConnString),
                        "PaymentTransactions", ctx.Resolve<ILogFactory>())
                    )
            ).As<IPaymentTransactionsRepository>().SingleInstance();

            builder.Register(ctx =>
                new PaymentTransactionEventsLog(AzureTableStorage<PaymentTransactionLogEventEntity>.Create(
                    _appSettings.ConnectionString(i => i.EasyPaymentGatewayService.Db.LogsConnString), "PaymentsLog", ctx.Resolve<ILogFactory>()))
            ).As<IPaymentTransactionEventsLog>().SingleInstance();

            builder.RegisterType<AntiFraudChecker>()
                .WithParameter(TypedParameter.From(appSettings.EasyPaymentGatewayService.AntiFraudCheckPaymentPeriod))
                .WithParameter(TypedParameter.From(appSettings.EasyPaymentGatewayService.AntiFraudCheckRegistrationDateSince));

            builder.RegisterInstance(serviceSettings.Redirect);

            builder.RegisterInstance(serviceSettings);
        }
    }
}
