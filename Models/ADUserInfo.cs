namespace AD_BulkChanges.Models
{
    public class ADUserInfo
    {
        public string DistinguishedName { get; set; } = string.Empty;
        public string SamAccountName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OU { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"{DisplayName} ({SamAccountName})";
        }
    }
}
