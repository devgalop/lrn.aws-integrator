using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lrn.devgalop.awsintegrator.Infrastructure.AWS.S3.Models;

namespace lrn.devgalop.awsintegrator.Infrastructure.AWS.S3Buckets.Interfaces
{
    public interface IS3Service
    {
        Task<BaseResponse> DownloadFile(string bucketName, string s3Key, string localPath, CancellationToken ct = default);
        Task<BaseResponse> UploadFile(string bucketName, string s3Key, string localPath, CancellationToken ct = default);
        Task<BaseResponse> DeleteFile(string bucketName, string s3Key, CancellationToken ct = default);
        Task<BaseResponse> CopyFile(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, CancellationToken ct = default);
        Task<BaseResponse> ExistsFile(string bucketName, string s3Key, CancellationToken ct = default);
    }
}