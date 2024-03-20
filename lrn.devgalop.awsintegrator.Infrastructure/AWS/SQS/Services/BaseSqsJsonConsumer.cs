using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services
{
    /// <summary>
    /// This class is responsible for getting messages and process it
    /// </summary>
    /// <typeparam name="ClassType">Custom class that is responsible for process messages</typeparam>
    /// <typeparam name="MessageType">SQS message deserialization class</typeparam>
    public abstract class BaseSqsJsonConsumer<ClassType, MessageType> : BaseConsumer<ClassType>
    {
        public BaseSqsJsonConsumer(
            ILogger<ClassType> logger,
            IConfiguration configuration,
            ConsumerConfiguration consumerConfiguration,
            string queueName):base(logger,configuration,consumerConfiguration,queueName)
        {
        }

        /// <summary>
        /// It is responsible for getting messages from sqs and processing the results of each one.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override async Task WorkerConsumerAsync(CancellationToken cancellationToken)
        {
            int validateConsumerCounter = 0;
            while(!cancellationToken.IsCancellationRequested)
            {
                if(validateConsumerCounter >= 12)
                {
                    validateConsumerCounter = 0;
                    string messageConsumer = _consumerConfiguration.EnableSqsConsumer 
                        ? "Esperando mensajes ..."
                        : "Consumer apagado ..."; 
                    _logger.LogInformation($"[{DateTime.Now}] {messageConsumer}");
                }
                
                //Getting messages from sqs
                var messages = await GetMessagesAsync(cancellationToken);
                if(messages.Count()==0) continue;

                List<SqsJsonRecord<MessageType>> messagesDeserialized = new();
                foreach (var message in messages)
                {
                    var sqsMessage = JsonSerializer.Deserialize<MessageType>(message.Body) 
                                        ?? throw new Exception($"Message cannot be deserialied into {nameof(MessageType)}");
                    bool isValidQueueTime = long.TryParse(message.Attributes["ApproximateFirstReceiveTimestamp"], out long queueTime);
                    bool isValidRetries = int.TryParse(message.Attributes["ApproximateReceiveCount"], out int retries);
                    messagesDeserialized.Add(new SqsJsonRecord<MessageType>()
                    {
                        Message = sqsMessage,
                        MessageBody = message.Body,
                        QueueTime = queueTime,
                        Retries = retries,
                        ReceiptHandle = message.ReceiptHandle
                    });
                }

                var messagesSucceed = await ProcessMessages(messagesDeserialized);

                foreach (var message in messagesDeserialized)
                {
                    //Delete messages that have been processed correctly
                    if(messagesSucceed.IsSucceed && !message.HasError)
                    {
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                        continue;
                    }

                    // Validation of retries
                    if(message.Retries >= _consumerConfiguration.MaxRetries)
                    {
                        //If your process has a queue for error add the call here
                        // ex: await _sqsClient.SendMessageAsync(errorQueue, message.MessageBody,cancellationToken);
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                        continue;
                    }

                    //Change message visibility for X time
                    await _sqsClient.ChangeMessageVisibilityAsync(_queueUrl, message.ReceiptHandle, 300);
                }
                
                validateConsumerCounter++;
            }
        }

        /// <summary>
        /// Abstract method in charge of processing any message. 
        /// </summary>
        /// <param name="messages">List of deserialized messages</param>
        /// <returns></returns>
        protected abstract Task<BaseResponse> ProcessMessages(List<SqsJsonRecord<MessageType>> messages);
    }
}