using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models
{
    public class SqsJsonRecord<MessageType>:SqsEventRecord
    {
        public MessageType? Message { get; set; }
    }
}