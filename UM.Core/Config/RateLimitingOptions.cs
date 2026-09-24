namespace UM.Core.Config
{
    public class RateLimitingOptions
    {
        public const string SectionName="RateLimiting";
         public Dictionary<string, PolicyOptions> Policies { get; set; } = new();
    }
    public class PolicyOptions
    {
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 0;
    }
}