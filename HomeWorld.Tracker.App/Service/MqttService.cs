using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using HomeWorld.Tracker.App.DAL.Model;
using HomeWorld.Tracker.Dto;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace HomeWorld.Tracker.App.Service
{
    public static class MqttService
    {
        private const string CloudMqttPass = "bqqhHe9qGf1A";
        private const string CloudMqttUsr = "apzvvubw";
        private static MqttClient _client;
        private const int DeviceId = 4;

        public static event EventHandler<Movement> MessageReceived;

        public static void Connect()
        {
            _client = new MqttClient("m21.cloudmqtt.com", 10891, false, MqttSslProtocols.None);

            var code = _client.Connect(Guid.NewGuid().ToString(), CloudMqttUsr, CloudMqttPass);

            var msgId = _client.Subscribe(new[] { "location/+/movement" },
                new[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

            //Wire up events
            _client.MqttMsgSubscribed += ClientOnMqttMsgSubscribed;
            _client.MqttMsgPublishReceived += ClientOnMqttMsgPublishReceived;
        }

        public static void Publish(string topic, object message, byte qos = 0, bool retain = false)
        {
            // {
            //  "CardId": "fd-a6-4a-95",
            //	"DeviceId": 6,
            //	"InLocation": 1,
            //	"SwipeTime": "2016-05-22T18:18:54Z"
            //}

            //var topic = $"location/{locationId}/movement";

            var jsonData = JsonConvert.SerializeObject(message);

            var data = Encoding.UTF8.GetBytes(jsonData);
            //var data = GetBytes(jsonData);

            if (_client == null || !_client.IsConnected)
            {
                _client = null;
                Connect();
            }

            _client?.Publish(topic, data);
        }

        private static void ClientOnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(e.Message);
                var data = JsonConvert.DeserializeObject<dynamic>(msg);

                if (data == null)
                {
                    return;
                }

                Debug.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);

                DateTime swipeTimeUtc;
                var result = DateTime.TryParse(data.SwipeTime.ToString(), out swipeTimeUtc);

                swipeTimeUtc = result ? swipeTimeUtc : DateTime.UtcNow;

                var fromDeviceId = data.DeviceId;
                if (fromDeviceId == DeviceId)
                {
                    Debug.WriteLine("Message ignored from this device.");
                    return;
                }

                var movementData = new Movement { CardId = data.CardId, SwipeTime = swipeTimeUtc.ToString("o"), InLocation = data.InLocation };

                OnMessageReceived(movementData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
            }
        }

        private static void ClientOnMqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed for id = " + e.MessageId);
        }

        public static void Disconnect()
        {
            _client.Disconnect();
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private static void OnMessageReceived(Movement movement)
        {
            MessageReceived?.Invoke(null, movement);
        }
    }
}