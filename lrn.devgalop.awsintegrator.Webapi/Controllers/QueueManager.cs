using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Interfaces;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using Microsoft.AspNetCore.Mvc;

namespace lrn.devgalop.awsintegrator.Webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueManager : ControllerBase
    {
        private readonly ConsumerConfiguration _consumerConfiguration;
        private readonly IPublisherService _publisherService;

        public QueueManager(
            ConsumerConfiguration consumerConfiguration,
            IPublisherService publisherService)
        {
            _consumerConfiguration = consumerConfiguration;
            _publisherService = publisherService;
        }

        [HttpGet("enable-consumer/{status:bool}")]
        public IActionResult EnableConsumer(bool status)
        {
            string message = status ? "SQS Consumer turns on" : "SQS Consumer shuts down";
            _consumerConfiguration.UpdateConsumerStatus(status);
            return Ok(message);
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody]SampleMessage message)
        {
            try
            {
                var response = await _publisherService.PublishMessageAsync("samples",message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}