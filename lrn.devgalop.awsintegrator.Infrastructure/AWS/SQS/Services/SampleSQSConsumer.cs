using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services
{
    public class SampleSQSConsumer : BaseSqsJsonConsumer<SampleSQSConsumer, SampleMessage>
    {
        public SampleSQSConsumer(
            ILogger<SampleSQSConsumer> logger, 
            IConfiguration configuration, 
            ConsumerConfiguration consumerConfiguration) 
            : base(logger, configuration, consumerConfiguration, "sampleQueue")
        {
        }

        protected override Task<BaseResponse> ProcessMessages(List<SqsJsonRecord<SampleMessage>> messages)
        {
            //Here add your logic to process each message
            throw new NotImplementedException();
        }
    }
}