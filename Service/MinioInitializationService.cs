using where_we_go.Config;

namespace where_we_go.Service
{
    public class MinioInitializationService(
            ILogger<MinioInitializationService> logger,
            IServiceProvider serviceProvider
    ) : IHostedService
    {
        private ILogger<MinioInitializationService> _logger { get; init; } = logger;
        private IServiceProvider _serviceProvider { get; init; } = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing MinIO bucket...");

            try
            {
                // Create a scope to resolve scoped services
                using var scope = _serviceProvider.CreateScope();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

                // Check and create bucket if needed
                var bucketName = GlobalConfig.GetRequiredEnv(GlobalConfig.MinioBucketName);

                bool bucketExists = await fileService.BucketExistsAsync(bucketName);

                if (!bucketExists)
                {
                    _logger.LogWarning("Bucket '{BucketName}' does not exist. Creating...", bucketName);
                    await fileService.EnsureBucketExistsAsync(bucketName);
                    _logger.LogInformation("Bucket '{BucketName}' created successfully", bucketName);
                }
                else
                {
                    _logger.LogInformation("Bucket '{BucketName}' already exists", bucketName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MinIO bucket");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MinIO initialization service stopped");
            return Task.CompletedTask;
        }
    }
}