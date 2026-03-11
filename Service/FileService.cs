// using Minio;
// using Minio.DataModel.Args;
// using Minio.Exceptions;
//
// using where_we_go.Config;
using where_we_go.DTO;
//
namespace where_we_go.Service
{
    public interface IFileService
    {
        // MinIO-specific storage operations will be no-ops when MinIO is unavailable,
        // but we keep the interface so existing controllers can compile.
        Task<bool> BucketExistsAsync(string bucketName);
        Task EnsureBucketExistsAsync(string bucketName);
        Task<bool> ObjectExistsAsync(string objectName);
        Task UploadFileAsync(FileUploadDto uploadDto);
        Task<string> GetPresignedUrlAsync(FileDownloadDto downloadDto);
        Task<Stream> GetFileStreamAsync(FileDownloadDto downloadDto);
        Task<string?> GeneratePresignedProfileUrlAsync(string? key);
        Task<string?> GeneratePresignedPostUrlAsync(string? key);
    }

    public class FileService : IFileService
    {
        public FileService()
        {
            // MinIO client initialization is disabled.
            // var endpoint = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioEndpoint);
            // var accessKey = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioAccessKey);
            // var secretKey = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioSecretKey);
            // _defaultBucketName = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioBucketName);
            // var useSsl = GlobalConfig.GetBoolEnvOrDefault(GlobalConfig.MinioUseSsl, false);
            //
            // var clientBuilder = new MinioClient()
            //     .WithEndpoint(endpoint)
            //     .WithCredentials(accessKey, secretKey);
            //
            // if (useSsl)
            // {
            //     clientBuilder = clientBuilder.WithSSL();
            // }
            //
            // _minioClient = clientBuilder.Build();
        }

        public Task<bool> BucketExistsAsync(string bucketName)
        {
            // MinIO disabled: report bucket as not existing.
            return Task.FromResult(false);
        }

        public Task EnsureBucketExistsAsync(string bucketName)
        {
            // MinIO disabled: nothing to do.
            return Task.CompletedTask;
        }

        public Task<bool> ObjectExistsAsync(string objectName)
        {
            // MinIO disabled: always report as not existing.
            return Task.FromResult(false);
        }

        public Task UploadFileAsync(FileUploadDto uploadDto)
        {
            // MinIO disabled: pretend upload succeeded without storing the file.
            return Task.CompletedTask;
        }

        public Task<string> GetPresignedUrlAsync(FileDownloadDto downloadDto)
        {
            // MinIO disabled: just return the object name as-is.
            return Task.FromResult(downloadDto.ObjectName);
        }

        public Task<Stream> GetFileStreamAsync(FileDownloadDto downloadDto)
        {
            // MinIO disabled: no file content available.
            throw new FileNotFoundException($"File '{downloadDto.ObjectName}' cannot be loaded because MinIO is disabled.");
        }

        public Task<string?> GeneratePresignedProfileUrlAsync(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
            }
            if (Uri.IsWellFormedUriString(key, UriKind.Absolute))
                return Task.FromResult<string?>(key);

            // When MinIO is disabled, just return the key as-is.
            return Task.FromResult<string?>(key);
        }

        public Task<string?> GeneratePresignedPostUrlAsync(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = "https://nftcalendar.io/storage/uploads/2021/11/30/screenshot_-_30_11_2021___15_14_00_1130202114142561a631c12a4aa.jpg";
            }
            if (Uri.IsWellFormedUriString(key, UriKind.Absolute))
                return Task.FromResult<string?>(key);

            // When MinIO is disabled, just return the key as-is.
            return Task.FromResult<string?>(key);
        }
    }
}
