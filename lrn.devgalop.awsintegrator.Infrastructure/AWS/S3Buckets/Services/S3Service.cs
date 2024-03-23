using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3.Models;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Interfaces;
using Microsoft.Extensions.Logging;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Services
{
    public class S3Service : IS3Service
    {
        private readonly ILogger<S3Service> _logger;
        private readonly AmazonS3Client _s3Client;

        public S3Service(
            ILogger<S3Service> logger,
            BasicS3Authentication basicAuthentication
            )
        {
            _logger = logger;
            _s3Client = new AmazonS3Client(
                basicAuthentication.AccessKey,
                basicAuthentication.SecretKey,
                RegionEndpoint.GetBySystemName(basicAuthentication.RegionCode)
            );
        }

        public async Task<BaseResponse> CopyFile(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceBucket)) throw new ArgumentNullException("parameter sourceBucket cannot be null or empty");
                if (string.IsNullOrEmpty(sourceKey)) throw new ArgumentNullException("parameter sourceKey cannot be null or empty");
                if (string.IsNullOrEmpty(destinationBucket)) throw new ArgumentNullException("parameter destinationBucket cannot be null or empty");
                if (string.IsNullOrEmpty(destinationKey)) throw new ArgumentNullException("parameter destinationKey cannot be null or empty");

                var request = new CopyObjectRequest()
                {
                    SourceBucket = sourceBucket,
                    SourceKey = sourceKey,
                    DestinationBucket = destinationBucket,
                    DestinationKey = destinationKey
                };

                var response = await _s3Client.CopyObjectAsync(request, ct);

                if (response.HttpStatusCode != HttpStatusCode.OK) throw new Exception($"Error during copy between buckets. Status {response.HttpStatusCode}");

                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }

        public async Task<BaseResponse> DeleteFile(string bucketName, string s3Key, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(bucketName)) throw new ArgumentNullException("parameter bucketName cannot be null or empty");
                if (string.IsNullOrEmpty(s3Key)) throw new ArgumentNullException("parameter s3Key cannot be null or empty");

                var request = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = s3Key
                };

                var response = await _s3Client.DeleteObjectAsync(request, ct);

                if (response.HttpStatusCode != HttpStatusCode.OK) throw new Exception($"File {bucketName}/{s3Key} cannot be deleted. Status {response.HttpStatusCode}");

                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }

        public async Task<BaseResponse> DownloadFile(string bucketName, string s3Key, string localPath, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(bucketName)) throw new ArgumentNullException("parameter bucketName cannot be null or empty");
                if (string.IsNullOrEmpty(s3Key)) throw new ArgumentNullException("parameter s3Key cannot be null or empty");
                if (string.IsNullOrEmpty(localPath)) throw new ArgumentNullException("parameter localPath cannot be null or empty");

                var existsFileResponse = await ExistsFile(bucketName, s3Key, ct);
                if (!existsFileResponse.IsSucceed) throw new Exception(existsFileResponse.ErrorMessage);

                var fileStream = File.Create(localPath);
                var response = await _s3Client.GetObjectAsync(bucketName, s3Key, ct);
                await response.ResponseStream.CopyToAsync(fileStream);
                fileStream.Close();

                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }

        public async Task<BaseResponse> ExistsFile(string bucketName, string s3Key, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(bucketName)) throw new ArgumentNullException("parameter bucketName cannot be null or empty");
                if (string.IsNullOrEmpty(s3Key)) throw new ArgumentNullException("parameter s3Key cannot be null or empty");

                var response = await _s3Client.GetObjectMetadataAsync(bucketName, s3Key, ct);
                if (response.HttpStatusCode != HttpStatusCode.OK) throw new Exception($"Error during object existing validation. Status {response.HttpStatusCode}");

                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }

        public async Task<BaseResponse> UploadFile(string bucketName, string s3Key, string localPath, CancellationToken ct = default)
        {
            try
            {
                if(string.IsNullOrEmpty(bucketName))throw new ArgumentNullException("parameter bucketName cannot be null or empty");
                if(string.IsNullOrEmpty(s3Key))throw new ArgumentNullException("parameter s3Key cannot be null or empty");
                if(string.IsNullOrEmpty(localPath))throw new ArgumentNullException("parameter localPath cannot be null or empty");
                if(!File.Exists(localPath)) throw new Exception($"File {localPath} doesn't exist or is not accessible");

                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    FilePath = localPath
                };

                var response = await _s3Client.PutObjectAsync(request, ct);

                if(response.HttpStatusCode != HttpStatusCode.OK) throw new Exception($"File {localPath} cannot be uploaded into {bucketName}/{s3Key}");

                return new()
                {
                    IsSucceed = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    IsSucceed = false,
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.ToString()
                };
            }
        }
    }
}