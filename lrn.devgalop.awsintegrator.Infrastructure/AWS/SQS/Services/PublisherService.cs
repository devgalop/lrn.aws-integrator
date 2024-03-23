using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Interfaces;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly ILogger<PublisherService> _logger;
        private readonly BasicAuthentication _awsAuth;
        private readonly AmazonSQSClient _sqsClient;

        public PublisherService(
            ILogger<PublisherService> logger,
            BasicAuthentication awsAuth)
        {
            _logger = logger;
            _awsAuth = awsAuth;
            _sqsClient = new AmazonSQSClient(
                _awsAuth.AccessKey,
                _awsAuth.SecretKey,
                RegionEndpoint.GetBySystemName(_awsAuth.RegionCode));
        }

        public async Task<BaseResponse> PublishMessageAsync<MessageType>(string queueName, MessageType message, CancellationToken cancellationToken = default)
        {
            try
            {
                if(string.IsNullOrEmpty(queueName)) throw new ArgumentNullException("Queue name is a required field");
                if(message is null) throw new ArgumentNullException("Message cannot be null");
                var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(queueName, cancellationToken);
                if(queueUrlResponse.HttpStatusCode != HttpStatusCode.OK) throw new Exception("The queue does not exist or the URL cannot be retrieved");
                string queueUrl = queueUrlResponse.QueueUrl;
                string messageBody = JsonSerializer.Serialize(message);
                var response = await _sqsClient.SendMessageAsync(queueUrl,messageBody,cancellationToken);
                if(response.HttpStatusCode != HttpStatusCode.OK) throw new Exception($"Message could not be published. Status code {response.HttpStatusCode}");
                
                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }
    }
}