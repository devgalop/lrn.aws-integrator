using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models
{
    public class ConsumerConfiguration
    {
        public BasicSQSAuthentication Connection {get; set;} = new();
        public bool EnableSqsConsumer { get; set; } = false;
        public int MaxThreads { get; set; } = 1;
        public int MaxNumberOfMessages { get; set; } = 10;
        public int MaxRetries { get; set; } = 1;

        public void UpdateConsumerStatus(bool status)
        {
            EnableSqsConsumer = status;
        }
    }
}