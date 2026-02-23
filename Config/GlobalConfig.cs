namespace where_we_go.Config
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

        // Mail config
        public const string MailHost = "MAIL_HOST";
        public const string MailPort = "MAIL_PORT";
        public const string MailUsername = "MAIL_USERNAME";
        public const string MailPassword = "MAIL_PASSWORD";
        public const string MailFromEmail = "MAIL_FROM_EMAIL";
        public const string MailFromName = "MAIL_FROM_NAME";
        public const string MailEnableSsl = "MAIL_ENABLE_SSL";

        public static string GetRequiredEnv(string key)
        {
            return Environment.GetEnvironmentVariable(key)
                ?? throw new InvalidOperationException($"{key} is required");
        }

        public static int GetRequiredIntEnv(string key)
        {
            var raw = GetRequiredEnv(key);
            return int.TryParse(raw, out var value)
                ? value
                : throw new InvalidOperationException($"{key} must be a valid integer");
        }

        public static string GetEnvOrDefault(string key, string defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
        }

        public static bool GetBoolEnvOrDefault(string key, bool defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

            return bool.TryParse(raw, out var value)
                ? value
                : throw new InvalidOperationException($"{key} must be 'true' or 'false'");
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
