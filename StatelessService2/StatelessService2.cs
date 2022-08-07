using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessService2
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessService2 : StatelessService
    {
        public StatelessService2(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = "9c2118fd-2988-4d96-b7b0-25f30e83c3f3";
            var telemetryClient = new TelemetryClient(configuration);

            try
            {

                /**
                Part 1: 

                var numOfPongs = 0;
                while (true)
                {
                    //Trace.WriteLine("pong");
                    telemetryClient.TrackTrace("pong");
                    telemetryClient.TrackMetric("numOfPongs", numOfPongs);
                    numOfPongs++;
                    telemetryClient.Flush();
                    Thread.Sleep(1000);
                }
                */


                /**
                Part 2:
                // Create queue client
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=shaharpingpongstorage;AccountKey=y4ZXGPLiuMg3howe2kuyRNI0CjtAzUcmMxMXA09K9Jq9drf8FsYwQk9gIZ+m7/9VCC/FhLev4MZC+AStVyxQiQ==;EndpointSuffix=core.windows.net";
                var queueName = "pingpongqueue";
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                //Read message from queue, print to AI trace
                QueueMessage[] messages = queueClient.ReceiveMessages();
                while (messages.Length < 1)
                {
                    messages = queueClient.ReceiveMessages();
                    Thread.Sleep(10000);
                }

                int messageData = Int32.Parse(messages[0].Body.ToString());
                queueClient.DeleteMessage(messages[0].MessageId, messages[0].PopReceipt);
                for (int i = 0; i < messageData; i++)
                {
                    telemetryClient.TrackTrace(messageData.ToString());
                    telemetryClient.Flush();
                }
                */

                // Read event from EH
                string connectionString = "Endpoint=sb://pingpongehub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=19x9rVYrTY9/xlk4QyZlJljG6BL6IJxBQNG1Pm5t01Y=";
                string ehubName = "pingpongeventhub";
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                var consumer = new EventHubConsumerClient(consumerGroup, connectionString, ehubName);

                using var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(TimeSpan.FromSeconds(45));
                var eventData = new List<string>();

                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(cancellationSource.Token))
                {
                    eventData.Add(partitionEvent.Data.EventBody.ToString());

                    if (eventData.Count >= 1)
                    {
                        break;
                    }
                }
                await consumer.CloseAsync();

                // Push message to queue
                connectionString = "DefaultEndpointsProtocol=https;AccountName=shaharpingpongstorage;AccountKey=y4ZXGPLiuMg3howe2kuyRNI0CjtAzUcmMxMXA09K9Jq9drf8FsYwQk9gIZ+m7/9VCC/FhLev4MZC+AStVyxQiQ==;EndpointSuffix=core.windows.net";
                var queueName = "pingpongqueue";
                QueueClient queueClient = new QueueClient(connectionString, queueName);
                
                queueClient.CreateIfNotExists();
                queueClient.SendMessage(eventData[0]);

            }
            catch (Exception ex)
            {
                telemetryClient.TrackTrace("Error in reading from EH");
                telemetryClient.Flush();
            }
        }
    }
}
