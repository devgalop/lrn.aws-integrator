using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models
{
    public class SampleMessage
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}