using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

using where_we_go.Config;
using where_we_go.DTO;

namespace where_we_go.Service
{
    public interface IFileService
    {
        Task<bool> BucketExistsAsync(string bucketName);
        Task EnsureBucketExistsAsync(string bucketName);
        Task<bool> ObjectExistsAsync(string objectName);
        Task UploadFileAsync(FileUploadDto uploadDto);
        Task<string> GetPresignedUrlAsync(FileDownloadDto downloadDto);
        Task<Stream> GetFileStreamAsync(FileDownloadDto downloadDto);
        Task<string?> GeneratePresignedUrl(string? key);

    }

    public class FileService : IFileService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _defaultBucketName;

        public FileService()
        {
            var endpoint = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioEndpoint);
            var accessKey = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioAccessKey);
            var secretKey = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioSecretKey);
            _defaultBucketName = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioBucketName);
            var useSsl = GlobalConfig.GetBoolEnvOrDefault(GlobalConfig.MinioUseSsl, false);

            var clientBuilder = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey);

            if (useSsl)
            {
                clientBuilder = clientBuilder.WithSSL();
            }

            _minioClient = clientBuilder.Build();
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                return await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            }
            catch (MinioException)
            {
                return false;
            }
        }

        public async Task EnsureBucketExistsAsync(string bucketName)
        {
            bool found = await BucketExistsAsync(bucketName);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }
        }

        public async Task<bool> ObjectExistsAsync(string objectName)
        {
            try
            {
                var args = new StatObjectArgs()
                    .WithBucket(_defaultBucketName)
                    .WithObject(objectName);

                await _minioClient.StatObjectAsync(args).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UploadFileAsync(FileUploadDto uploadDto)
        {
            try
            {
                // Ensure bucket exists
                await EnsureBucketExistsAsync(_defaultBucketName);

                // Upload file
                using var stream = new MemoryStream();
                await uploadDto.File.CopyToAsync(stream);
                stream.Position = 0;

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_defaultBucketName)
                    .WithObject(uploadDto.ObjectName)
                    .WithStreamData(stream)
                    .WithObjectSize(uploadDto.File.Length)
                    .WithContentType(uploadDto.File.ContentType);
                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            }
            catch (MinioException ex)
            {
                throw new InvalidOperationException($"File upload error: {ex.Message}", ex);
            }
        }

        public async Task<string> GetPresignedUrlAsync(FileDownloadDto downloadDto)
        {
            try
            {
                bool exists = await ObjectExistsAsync(downloadDto.ObjectName);
                if (!exists)
                {
                    throw new FileNotFoundException($"File '{downloadDto.ObjectName}' not found in storage");
                }

                var args = new PresignedGetObjectArgs()
                    .WithBucket(_defaultBucketName)
                    .WithObject(downloadDto.ObjectName)
                    .WithExpiry(downloadDto.ExpiryInSeconds);

                return await _minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (MinioException ex)
            {
                throw new InvalidOperationException($"Failed to generate presigned URL: {ex.Message}", ex);
            }
        }

        public async Task<Stream> GetFileStreamAsync(FileDownloadDto downloadDto)
        {
            try
            {
                bool exists = await ObjectExistsAsync(downloadDto.ObjectName);
                if (!exists)
                {
                    throw new FileNotFoundException($"File '{downloadDto.ObjectName}' not found in storage");
                }

                var memoryStream = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(_defaultBucketName)
                    .WithObject(downloadDto.ObjectName)
                    .WithCallbackStream(stream =>
                    {
                        stream.CopyTo(memoryStream);
                    });

                await _minioClient.GetObjectAsync(args).ConfigureAwait(false);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (MinioException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve file: {ex.Message}", ex);
            }
        }

        public async Task<string?> GeneratePresignedUrl(string? key)
        {

            if (string.IsNullOrWhiteSpace(key))
            {
                key =  "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
            }
            if (Uri.IsWellFormedUriString(key, UriKind.Absolute)) 
                return key;

            return await this.GetPresignedUrlAsync(
                new FileDownloadDto
                {
                    ObjectName = key,
                    ExpiryInSeconds = 3600
                }
            );
        }
    }
}