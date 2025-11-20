namespace AD_BulkChanges.Models
{
    public class PendingChange
    {
        public string UserDN { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"{UserDisplayName}: {FieldName} '{OldValue}' → '{NewValue}'";
        }
    }
}
