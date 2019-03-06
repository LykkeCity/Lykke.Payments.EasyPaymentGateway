﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.EasyPaymentGateway.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
