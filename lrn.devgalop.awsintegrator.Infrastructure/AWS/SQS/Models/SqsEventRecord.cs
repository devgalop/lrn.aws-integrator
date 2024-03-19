using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models
{
    public class SqsEventRecord
    {
        public string ReceiptHandle { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public int Retries { get; set; }
        public long QueueTime { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
    }
}