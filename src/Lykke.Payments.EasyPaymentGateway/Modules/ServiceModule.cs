using Autofac;
using Lykke.Common.Log;
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
        }
    }
}
