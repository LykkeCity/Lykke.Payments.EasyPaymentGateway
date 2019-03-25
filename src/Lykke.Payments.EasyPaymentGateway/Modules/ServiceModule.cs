using Autofac;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Payments.EasyPaymentGateway.AzureRepositories;
using Lykke.Payments.EasyPaymentGateway.Domain.Services;
using Lykke.Payments.EasyPaymentGateway.DomainServices;
using Lykke.Payments.EasyPaymentGateway.Settings;
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
            builder.Register(ctx => new PersonalDataService(_appSettings.CurrentValue.PersonalDataServiceClient, ctx.Resolve<ILogFactory>()))
                .As<IPersonalDataService>().SingleInstance();

            var serviceSettings = _appSettings.CurrentValue.EasyPaymentGatewayService;

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

            builder.RegisterInstance(serviceSettings.Redirect);

            builder.RegisterInstance(serviceSettings);
        }
    }
}
