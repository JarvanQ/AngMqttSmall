using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngMqttSmall.Services
{
    public class AppSettingsProvider
    {
        private IConfiguration configuration;
        public static BrokerHostSettings BrokerHostSettings;
        public static ClientSettings ClientSettings;




        public AppSettingsProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void MapConfiguration()
        {
            MapBrokerHostSettings();
            MapClientSettings();
        }

        private void MapBrokerHostSettings()
        {
            BrokerHostSettings brokerHostSettings = new BrokerHostSettings();
            configuration.GetSection(nameof(BrokerHostSettings)).Bind(brokerHostSettings);
            AppSettingsProvider.BrokerHostSettings = brokerHostSettings;
        }

        private void MapClientSettings()
        {
            ClientSettings clientSettings = new ClientSettings();
            configuration.GetSection(nameof(ClientSettings)).Bind(clientSettings);
            AppSettingsProvider.ClientSettings = clientSettings;
        }

    }

    public class BrokerHostSettings
    {
        public string Host { set; get; }
        public int Port { set; get; }
    }

    public class ClientSettings
    {
        public string Id { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
    }
}
