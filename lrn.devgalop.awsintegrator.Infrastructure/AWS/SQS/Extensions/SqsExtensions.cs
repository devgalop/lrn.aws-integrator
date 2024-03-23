using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Interfaces;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Models;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.SQS.Extensions
{
    public static class SqsExtensions
    {
        public static void AddSQSConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            services.UseAWSSecrets();
            var serviceProvider = services.BuildServiceProvider();
            var awsAuth = (BasicSQSAuthentication)serviceProvider.GetRequiredService(typeof(BasicSQSAuthentication));
            _ = bool.TryParse(configuration["AWS:SQS:EnableConsumer"], out bool isEnabled);
            _ = int.TryParse(configuration["AWS:SQS:RequestConfiguration:MaxRetries"], out int maxRetries);
            _ = int.TryParse(configuration["AWS:SQS:RequestConfiguration:MaxNumberOfMessages"], out int maxMessages);
            _ = int.TryParse(configuration["AWS:SQS:RequestConfiguration:MaxThreads"], out int maxThreads);
            ConsumerConfiguration config = new()
            {
                Connection = awsAuth,
                EnableSqsConsumer = isEnabled,
                MaxNumberOfMessages = maxMessages,
                MaxRetries = maxRetries,
                MaxThreads = maxThreads
            };

            services.AddSingleton(_ => config);

            //Add your custom consumers as a hosted service
            //Ex: services.AddHostedService<MyOwnSQSConsumer>();
            services.AddHostedService<SampleSQSConsumer>();
        }

        public static void AddSQSPublisher(this IServiceCollection services)
        {
            services.UseAWSSecrets();
            services.AddTransient<IPublisherService, PublisherService>();
        }

        public static void UseAWSSecrets(this IServiceCollection services)
        {
            BasicSQSAuthentication awsAuth = new()
            {
                AccessKey = Environment.GetEnvironmentVariable("AWS_SQS_ACCESSKEY") ?? throw new Exception("AWS Access key is required"),
                SecretKey = Environment.GetEnvironmentVariable("AWS_SQS_SECRETKEY") ?? throw new Exception("AWS Secret key is required"),
                RegionCode = Environment.GetEnvironmentVariable("AWS_SQS_REGION") ?? throw new Exception("AWS Region is required")
            };
            services.AddSingleton(_ => awsAuth);
        }
    }
}