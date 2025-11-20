namespace AD_BulkChanges.Models
{
    public class FieldMapping
    {
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"{OldValue} → {NewValue}";
        }
    }
}
