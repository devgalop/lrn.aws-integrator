using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models
{
    public class BaseResponse
    {
        public bool IsSucceed { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDescription { get; set; }
    }
}