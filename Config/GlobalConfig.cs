namespace WhereWeGo.Config
{
    /// <summary>
    /// Global configuration constants for environment variable keys
    /// </summary>
    public static class GlobalConfig
    {
        // Database
        public const string DbHost = "DB_HOST";
        public const string DbPort = "DB_PORT";
        public const string DbName = "DB_NAME";
        public const string DbUser = "DB_USER";
        public const string DbPassword = "DB_PASSWORD";

        // Authentication
        public const string GoogleClientId = "GOOGLE_CLIENT_ID";
        public const string GoogleClientSecret = "GOOGLE_CLIENT_SECRET";

        public static string GetRequiredEnv(string key)
        {
            return Environment.GetEnvironmentVariable(key) ?? throw new InvalidOperationException($"{key} is required");
        }

        public static string GetDBConnectionString()
        {
            var dbHost = GetRequiredEnv(DbHost);
            var dbPort = GetRequiredEnv(DbPort);
            var dbName = GetRequiredEnv(DbName);
            var dbUser = GetRequiredEnv(DbUser);
            var dbPassword = GetRequiredEnv(DbPassword);

            return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
        }
    }
}
