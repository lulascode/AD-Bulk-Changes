namespace AD_BulkChanges.Models
{
    public class ADSettings
    {
        public string ServerName { get; set; } = string.Empty;
        public string DomainDN { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseCurrentCredentials { get; set; } = true;
    }
}
