using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Retorno360Tacna.SERVICES
{
    public class CloudflareR2Service
    {
        public sealed class R2FileInfo
        {
            public string Key { get; set; } = string.Empty;
            public DateTime LastModifiedUtc { get; set; }
            public long Size { get; set; }
        }

        private readonly string accessKey = "3d713a34a9e16e06de5fbb67947fd549";
        private readonly string secretKey = "aa2640adaaaaf17991ad7887175b9fa55ab4729ef58bab0e217af677e45a8c04";
        private readonly string serviceUrl = "https://01a682a2f4b87ad648c0baa2dc5fe427.r2.cloudflarestorage.com";
        private readonly string bucketName;
        private readonly AmazonS3Client client;

        public CloudflareR2Service(string bucketName)
        {
            this.bucketName = bucketName;
            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };
            client = new AmazonS3Client(accessKey, secretKey, config);
        }

        public async Task<List<string>> ListFoldersAsync(string prefix = "")
        {
            var folders = new List<string>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                Delimiter = "/"
            };
            var response = await client.ListObjectsV2Async(request);

            if (response?.CommonPrefixes == null || response.CommonPrefixes.Count == 0)
                return folders;

            foreach (var commonPrefix in response.CommonPrefixes)
            {
                if (!string.IsNullOrWhiteSpace(commonPrefix))
                    folders.Add(commonPrefix.TrimEnd('/'));
            }
            return folders;
        }

        public async Task<List<string>> ListFilesAsync(string prefix = "")
        {
            var files = new List<string>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                Delimiter = ""
            };
            var response = await client.ListObjectsV2Async(request);

            if (response?.S3Objects == null || response.S3Objects.Count == 0)
                return files;

            foreach (var obj in response.S3Objects)
            {
                if (!string.IsNullOrWhiteSpace(obj?.Key) && !obj.Key.EndsWith("/"))
                    files.Add(obj.Key);
            }
            return files;
        }

        public async Task<List<R2FileInfo>> ListFileDetailsAsync(string prefix = "")
        {
            var files = new List<R2FileInfo>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                Delimiter = ""
            };

            var response = await client.ListObjectsV2Async(request);

            if (response?.S3Objects == null || response.S3Objects.Count == 0)
                return files;

            foreach (var obj in response.S3Objects)
            {
                if (string.IsNullOrWhiteSpace(obj?.Key) || obj.Key.EndsWith("/"))
                    continue;

                files.Add(new R2FileInfo
                {
                    Key = obj.Key,
                    LastModifiedUtc = obj.LastModified.GetValueOrDefault().ToUniversalTime(),
                    Size = obj.Size.GetValueOrDefault()
                });
            }

            return files;
        }

        public async Task DownloadFileAsync(string key, string localPath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };
            using (var response = await client.GetObjectAsync(request))
            {
                await response.WriteResponseStreamToFileAsync(localPath, false, default);
            }
        }

        public async Task DeleteFileAsync(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await client.DeleteObjectAsync(request);
        }
    }
}
