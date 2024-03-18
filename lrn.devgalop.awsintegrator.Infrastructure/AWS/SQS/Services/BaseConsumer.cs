using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services
{
    public abstract class BaseConsumer<T> : BackgroundService
    {
        protected readonly ILogger<T> _logger;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IConfiguration _configuration;
        protected readonly ConsumerConfiguration _consumerConfiguration;
        protected readonly string _queueName;
        protected readonly string _queueUrl;
        protected readonly AmazonSQSClient _sqsClient;
        private bool _firstExecution = true;

        public BaseConsumer(
            ILogger<T> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ConsumerConfiguration consumerConfiguration,
            string queueName)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _consumerConfiguration = consumerConfiguration;
            _queueName = queueName;
            _sqsClient = new AmazonSQSClient(
                consumerConfiguration.Connection.AccessKey,
                consumerConfiguration.Connection.SecretKey,
                RegionEndpoint.GetBySystemName(consumerConfiguration.Connection.RegionCode));
            _queueUrl = _sqsClient.GetQueueUrlAsync(queueName).Result.QueueUrl;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _sqsClient.Dispose();
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Task[] tasks = new Task[_consumerConfiguration.MaxThreads];
            for (int idxTask = 0; idxTask < _consumerConfiguration.MaxThreads; idxTask++)
            {
                tasks[idxTask] = Task.Run(async () => 
                {
                    await WorkerConsumerAsync(cancellationToken);
                },cancellationToken);
            }

            if(_firstExecution)
            {
                _firstExecution = false;
                //Wait for 3 seconds while configure the consumer
                await Task.Delay(3000,cancellationToken);
            }

            int counterTask = 0;
            while(!cancellationToken.IsCancellationRequested)
            {
                int idxEndTask = Task.WaitAny(tasks,cancellationToken);
                tasks[idxEndTask] = Task.Run(async ()=>
                {
                    await WorkerConsumerAsync(cancellationToken);
                },cancellationToken);
                counterTask +=1;

                if(counterTask >= 10)
                {
                    GC.Collect();
                    counterTask = 0;
                }
            }

        }

        protected abstract Task WorkerConsumerAsync(CancellationToken cancellationToken);
    }
}