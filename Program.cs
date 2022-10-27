using System;
using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace GetSBMessage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Keda!");
            var currentDate = DateTime.Now;
            Console.WriteLine($"Running Pod on:{currentDate:d} at {currentDate:t}!");
            string sampleMsg = GetServiceBusMessage().Result;
            Console.WriteLine("To wait for: " + sampleMsg + " minutes");
            System.Threading.Thread.Sleep(Int32.Parse(sampleMsg) * 1000);
        }

        static async Task<string> GetServiceBusMessage()
        {
            string connectionString = Environment.GetEnvironmentVariable("connectionStringServiceBus");
            string queueName = Environment.GetEnvironmentVariable("queueName");
            ServiceBusClient clientServiceBus = new ServiceBusClient(connectionString);
            ServiceBusReceiver receiver = clientServiceBus.CreateReceiver(queueName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
            if (receivedMessage == null)
            {
                await clientServiceBus.DisposeAsync();
                return "0";
            }
            else
            {
                string body = receivedMessage.Body.ToString();
                var jsonStringBody = JsonConvert.DeserializeObject<SampleObject>(body);
                await receiver.CompleteMessageAsync(receivedMessage);
                await clientServiceBus.DisposeAsync();
                if (jsonStringBody != null)
                {
                    return jsonStringBody.Delay;
                }
                else
                {
                    return "0";
                }
            }
        }
    }
    class SampleObject
    {
        public string Delay { get; set; } = "0";
    }
}
