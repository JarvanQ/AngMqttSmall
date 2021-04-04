using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using SignalRHubs;

namespace AngMqttSmall.Services.MqttService
{
    struct MqttMessages
    {
        public List<int> storedMessages;
        public List<long> receivedMessages;
        public List<long> sentMessages;
    }



    public class MqttClientService : IMqttClientService
    {
        private IMqttClient mqttClient;
        private IMqttClientOptions options;
        IHubContext<MqttHub> hubContext;

        MqttMessages mqttMessages = new MqttMessages()
        {
            storedMessages = new List<int>(),
            receivedMessages = new List<long>(),
            sentMessages = new List<long>()
        };

        public MqttClientService(IHubContext<MqttHub> hubContext)
        {
            this.hubContext = hubContext;
            CreateMqttClient();
            ConfigureMqttClient();
        }


        /// <summary>
        /// Создание подписчика
        /// </summary>
        /// <param name="options">настройки подписчика</param>
        /// <returns>обьект подписчика</returns>
        private void CreateMqttClient()
        {
            var brokerHostSettings = AppSettingsProvider.BrokerHostSettings;

            options = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerHostSettings.Host, brokerHostSettings.Port).Build();

            mqttClient = new MqttFactory().CreateMqttClient();
         
        }

        /// <summary>
        /// Определение обработчиков событий
        /// </summary>
        /// <param name="mqttClient">Обьект подписчика</param>
        private void ConfigureMqttClient()
        {
            mqttClient.ConnectedHandler = this;
            mqttClient.DisconnectedHandler = this;
            mqttClient.ApplicationMessageReceivedHandler = this;
        }


        /// <summary>
        /// Обработчик получения рассылки
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            await Task.Run(() => GetMessageValue(eventArgs.ApplicationMessage));
            await hubContext.Clients.All.SendAsync("Notify", $"Добавлено: {mqttMessages.receivedMessages[0]} ");

        }

        /// <summary>
        /// Обработчик подключения
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            await mqttClient.SubscribeAsync("$SYS/broker/messages/#");
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttClient.ConnectAsync(options);

            if (!mqttClient.IsConnected)
            {
                await mqttClient.ReconnectAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var disconnectOption = new MqttClientDisconnectOptions
                {
                    ReasonCode = MqttClientDisconnectReason.NormalDisconnection,
                    ReasonString = "NormalDiconnection"
                };
                await mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
            }
            await mqttClient.DisconnectAsync();
        }

        /// <summary>
        /// Обработка полученого сообщения
        /// </summary>
        /// <param name="message"></param>
        private void GetMessageValue(MqttApplicationMessage message)
        {
            var topic = message.Topic.Split(new char[] { '/' }).Last();
            var payload = Encoding.UTF8.GetString(message.Payload);

            switch (topic)
            {
                case "stored":
                    mqttMessages.storedMessages.Add(Int32.Parse(payload));
                    break;
                case "received":
                    mqttMessages.receivedMessages.Add(long.Parse(payload));
                    break;
                case "sent":
                    mqttMessages.sentMessages.Add(long.Parse(payload));
                    break;
            }


        }


    }
}
