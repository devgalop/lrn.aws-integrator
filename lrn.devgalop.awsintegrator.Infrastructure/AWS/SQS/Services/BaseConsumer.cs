using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services
{
    /// <summary>
    ///  BaseConsumer is the class responsible for creating an SQS connection and assigning tasks to different threads.
    /// </summary>
    /// <typeparam name="ClassType">Custom class that is responsible for process messages</typeparam>
    public abstract class BaseConsumer<ClassType> : BackgroundService
    {
        protected readonly ILogger<ClassType> _logger;
        protected readonly IConfiguration _configuration;
        protected readonly ConsumerConfiguration _consumerConfiguration;
        protected readonly string _queueName;
        protected readonly string _queueUrl;
        protected readonly AmazonSQSClient _sqsClient;
        private bool _firstExecution = true;

        public BaseConsumer(
            ILogger<ClassType> logger,
            IConfiguration configuration,
            ConsumerConfiguration consumerConfiguration,
            string queueName)
        {   
            _logger = logger;
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
            //Create and assign tasks to multiple threads
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
                //Assign another task in the same thread when a task is finished
                int idxEndTask = Task.WaitAny(tasks,cancellationToken);
                tasks[idxEndTask] = Task.Run(async ()=>
                {
                    await WorkerConsumerAsync(cancellationToken);
                },cancellationToken);
                counterTask +=1;

                //Clean up local memory by running the garbage collector
                if(counterTask >= 10)
                {
                    GC.Collect();
                    counterTask = 0;
                }
            }

        }

        protected async Task<IEnumerable<Message>> GetMessagesAsync(CancellationToken cancellationToken)
        {
            if(!_consumerConfiguration.EnableSqsConsumer)
            {
                await Task.Delay(5000,cancellationToken);
                return new List<Message>();
            }
            List<Message> messages = new();
            int messagesPerRequest = _consumerConfiguration.MaxNumberOfMessages < 10 ? _consumerConfiguration.MaxNumberOfMessages :10;
            var sqsRequest = new ReceiveMessageRequest()
            {
                QueueUrl = _queueUrl,
                WaitTimeSeconds = 0,
                MaxNumberOfMessages = messagesPerRequest,
                AttributeNames = new List<string>(){"All"}
            };

            for (int idxMessage = 0; idxMessage < _consumerConfiguration.MaxNumberOfMessages; idxMessage++)
            {
                var sqsResponse = await _sqsClient.ReceiveMessageAsync(sqsRequest,cancellationToken);
                if(sqsResponse.Messages.Count != 0) messages.AddRange(sqsResponse.Messages);
            }
            return messages;
        }

        protected abstract Task WorkerConsumerAsync(CancellationToken cancellationToken);
    }
}