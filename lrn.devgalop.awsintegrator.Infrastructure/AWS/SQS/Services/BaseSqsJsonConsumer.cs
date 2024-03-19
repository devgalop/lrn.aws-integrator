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
    public abstract class BaseSqsJsonConsumer<ClassType, MessageType> : BaseConsumer<ClassType>
    {
        public BaseSqsJsonConsumer(
            ILogger<ClassType> logger,
            IConfiguration configuration,
            ConsumerConfiguration consumerConfiguration,
            string queueName):base(logger,configuration,consumerConfiguration,queueName)
        {
        }

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
                
                var messages = await GetMessagesAsync(cancellationToken);
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
                    if(messagesSucceed.IsSucceed && !message.HasError)
                    {
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                        continue;
                    }

                    if(message.Retries >= _consumerConfiguration.MaxRetries)
                    {
                        //If your process has a queue for error add the call here
                        // ex: await _sqsClient.SendMessageAsync(errorQueue, message.MessageBody,cancellationToken);
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                        continue;
                    }

                    await _sqsClient.ChangeMessageVisibilityAsync(_queueUrl, message.ReceiptHandle, 300);
                }
                
                validateConsumerCounter++;
            }
        }

        protected abstract Task<BaseResponse> ProcessMessages(List<SqsJsonRecord<MessageType>> messages);
    }
}