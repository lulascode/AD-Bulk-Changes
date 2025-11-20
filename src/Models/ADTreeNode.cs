using System.Collections.ObjectModel;

using System.ComponentModel;

namespace AD_BulkChanges.Models
{
    public class ADTreeNode : INotifyPropertyChanged
    {
        private bool _isChecked;
        private bool _isExpanded;
        
        public string Name { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ObservableCollection<ADTreeNode> Children { get; set; } = new();
        public ADTreeNode? Parent { get; set; }
        public bool IsSelected { get; set; }
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                    
                    // Kaskadiere zu Kindern
                    SetChildrenChecked(value);
                }
            }
        }
        
        private void SetChildrenChecked(bool isChecked)
        {
            foreach (var child in Children)
            {
                child.IsChecked = isChecked;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}
