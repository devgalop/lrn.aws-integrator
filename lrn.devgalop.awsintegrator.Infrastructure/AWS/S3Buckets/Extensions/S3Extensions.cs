using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3.Models;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Interfaces;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Services;
using Microsoft.Extensions.DependencyInjection;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Extensions
{
    public static class S3Extensions
    {
        public static void AddS3(this IServiceCollection services)
        {
            BasicS3Authentication awsAuth = new()
            {
                AccessKey = Environment.GetEnvironmentVariable("AWS_SQS_ACCESSKEY") ?? throw new Exception("AWS Access key is required"),
                SecretKey = Environment.GetEnvironmentVariable("AWS_SQS_SECRETKEY") ?? throw new Exception("AWS Secret key is required"),
                RegionCode = Environment.GetEnvironmentVariable("AWS_SQS_REGION") ?? throw new Exception("AWS Region is required")
            };
            services.AddSingleton(_ => awsAuth);
            services.AddTransient<IS3Service,S3Service>();
        }
    }
}