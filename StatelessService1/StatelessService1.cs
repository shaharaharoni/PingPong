using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessService1
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessService1 : StatelessService
    {
        public StatelessService1(StatelessServiceContext context)
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
                var numOfPings = 0;

                while (true)
                {
                    //Trace.WriteLine("ping");
                    telemetryClient.TrackTrace("ping");
                    telemetryClient.TrackMetric("numOfPings", numOfPings);
                    numOfPings++;
                    telemetryClient.Flush();
                    Thread.Sleep(1000);

                }
                */

                /**
                Part 2 - queue:
                // Create queue client
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=shaharpingpongstorage;AccountKey=xrT4TDaRQd9ovJwlOleeLjEaa9wIhXrsRz9fdo0eNC5VZDbKG+KvIumu+eBCWpCtC55c1XrCyNzb+AStzgA0mA==;EndpointSuffix=core.windows.net";
                var queueName = "pingpongqueue";
                QueueClient queueClient = new QueueClient(connectionString, queueName);
                queueClient.CreateIfNotExists();

                //Send random number
                var randomNum = new Random().Next(1, 11);
                queueClient.SendMessage(randomNum.ToString());
                */

                /**
                Part 3 - Event Hub
                */
                var randomNum = new Random().Next(1, 11);
                string connectionString = "Endpoint=sb://pingpongehub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=19x9rVYrTY9/xlk4QyZlJljG6BL6IJxBQNG1Pm5t01Y=";
                string ehubName = "pingpongeventhub";
                var producer = new EventHubProducerClient(connectionString, ehubName);

                // publish to EH
                using EventDataBatch eventBatch = await producer.CreateBatchAsync();
                var eventBody = new BinaryData(randomNum.ToString());
                var eventData = new EventData(eventBody);
                eventBatch.TryAdd(eventData);
                await producer.SendAsync(eventBatch);
                await producer.CloseAsync();
            }
            catch (Exception ex)
            {
                telemetryClient.TrackTrace("Error in writing to queue");
                telemetryClient.Flush();
            }
        }
    }
}
