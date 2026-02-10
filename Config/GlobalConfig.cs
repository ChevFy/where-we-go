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
    }
}
