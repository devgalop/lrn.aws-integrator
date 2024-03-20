using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Interfaces
{
    public interface IPublisherService
    {
        Task<BaseResponse> PublishMessageAsync<MessageType>(string queueName, MessageType message, CancellationToken cancellationToken = default);
    }
}