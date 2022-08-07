using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessService3
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessService3 : StatelessService
    {
        public StatelessService3(StatelessServiceContext context)
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

            }

            catch
            {
                telemetryClient.TrackTrace("Error in reading from queue");
                telemetryClient.Flush();
            }
        }
    }
}
