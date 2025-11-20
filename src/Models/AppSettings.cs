using System.Collections.Generic;
using AD_BulkChanges.Models;

namespace AD_BulkChanges.Models
{
    public class AppSettings
    {
        public ADSettings ADConnection { get; set; } = new ADSettings();
        public List<string> RecentOUs { get; set; } = new List<string>();
        public List<FieldMapping> SavedMappings { get; set; } = new List<FieldMapping>();
        public string LastSelectedField { get; set; } = "Position (Title)";
        public bool IncludeSubOUs { get; set; } = true;
        public WindowSettings Window { get; set; } = new WindowSettings();
    }
    
    public class WindowSettings
    {
        public double Width { get; set; } = 1200;
        public double Height { get; set; } = 700;
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public bool IsMaximized { get; set; } = false;
    }
}
