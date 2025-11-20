using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AD_BulkChanges.Models;
using AD_BulkChanges.Services;
using Microsoft.Win32;

namespace AD_BulkChanges
{
    public partial class MainWindow : Window
    {
        private readonly ADService _adService;
        private readonly ObservableCollection<ADUserInfo> _users;
        private readonly ObservableCollection<ADUserInfo> _filteredUsers;
        private readonly ObservableCollection<FieldMapping> _mappings;
        private readonly ObservableCollection<PendingChange> _pendingChanges;
        private readonly ObservableCollection<string> _availableValues;
        private readonly SettingsService _settingsService;
        private AppSettings _appSettings = new AppSettings();
        private ADSettings _adSettings = new ADSettings();
        
        public MainWindow()
        {
            InitializeComponent();
            _adService = new ADService();
            _users = new ObservableCollection<ADUserInfo>();
            _filteredUsers = new ObservableCollection<ADUserInfo>();
            _mappings = new ObservableCollection<FieldMapping>();
            _pendingChanges = new ObservableCollection<PendingChange>();
            _availableValues = new ObservableCollection<string>();
            _settingsService = new SettingsService();
            
            DgUsers.ItemsSource = _filteredUsers;
            LstMappings.ItemsSource = _mappings;
            LstPendingChanges.ItemsSource = _pendingChanges;
            
            // Lade gespeicherte Einstellungen
            LoadSettings();
            
            // Timer für Checkbox-Aktualisierung
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => UpdateSelectedCount();
            timer.Start();
            
            // Window Closing Event
            Closing += MainWindow_Closing;
            
            // Keyboard Shortcuts
            KeyDown += MainWindow_KeyDown;
        }
        
        private void LoadSettings()
        {
            _appSettings = _settingsService.LoadSettings();
            _adSettings = _appSettings.ADConnection;
            _adService.UpdateSettings(_adSettings);
            
            // Window Position & Size
            if (_appSettings.Window.Width > 0)
            {
                Width = _appSettings.Window.Width;
                Height = _appSettings.Window.Height;
                Left = _appSettings.Window.Left;
                Top = _appSettings.Window.Top;
                WindowState = _appSettings.Window.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            
            // Lade gespeicherte Mappings
            foreach (var mapping in _appSettings.SavedMappings)
            {
                _mappings.Add(mapping);
            }
            
            // SubOUs Checkbox
            ChkIncludeSubOUs.IsChecked = _appSettings.IncludeSubOUs;
            
            UpdateStatus("Einstellungen geladen");
        }
        
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        
        private void SaveSettings()
        {
            // Window Position & Size
            _appSettings.Window.Width = ActualWidth;
            _appSettings.Window.Height = ActualHeight;
            _appSettings.Window.Left = Left;
            _appSettings.Window.Top = Top;
            _appSettings.Window.IsMaximized = WindowState == WindowState.Maximized;
            
            // AD Settings
            _appSettings.ADConnection = _adSettings;
            
            // Mappings
            _appSettings.SavedMappings.Clear();
            foreach (var mapping in _mappings)
            {
                _appSettings.SavedMappings.Add(mapping);
            }
            
            // SubOUs
            _appSettings.IncludeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
            
            _settingsService.SaveSettings(_appSettings);
        }
        
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl+S = Settings
            if (e.Key == System.Windows.Input.Key.S && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                BtnSettings_Click(sender, e);
                e.Handled = true;
            }
            // Ctrl+F = Focus Search
            else if (e.Key == System.Windows.Input.Key.F && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                TxtSearch.Focus();
                e.Handled = true;
            }
            // Ctrl+E = Export
            else if (e.Key == System.Windows.Input.Key.E && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                BtnExport_Click(sender, e);
                e.Handled = true;
            }
            // F5 = Refresh
            else if (e.Key == System.Windows.Input.Key.F5)
            {
                ReloadCurrentUsers();
                e.Handled = true;
            }
        }
        
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_adSettings)
            {
                Owner = this
            };
            
            if (settingsWindow.ShowDialog() == true)
            {
                _adSettings = settingsWindow.Settings;
                _adService.UpdateSettings(_adSettings);
                UpdateStatus("AD-Einstellungen aktualisiert");
            }
        }
        
        private void BtnLoadAD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Lade AD-Struktur...");
                var rootNode = _adService.LoadOUStructure();
                TreeAD.Items.Clear();
                TreeAD.Items.Add(rootNode);
                // Setze initial expanded auf false, damit die Pfeile erscheinen
                rootNode.IsExpanded = false;
                UpdateStatus("AD-Struktur geladen");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der AD-Struktur: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Fehler beim Laden");
            }
        }
        

        
        private void LoadChildrenOnDemand(ADTreeNode node)
        {
            // Prüfe ob es ein Dummy-Element ist oder schon geladen wurde
            if (node.Children.Count == 1 && node.Children[0].Name == "Loading...")
            {
                node.Children.Clear();
                var tempNode = _adService.LoadOUStructure(node.DistinguishedName);
                foreach (var child in tempNode.Children)
                {
                    child.Parent = node;
                    node.Children.Add(child);
                }
            }
        }
        
        private void UpdateSelectedCount()
        {
            var checkedNodes = GetCheckedNodes();
            TxtSelectedCount.Text = $"{checkedNodes.Count} OU{(checkedNodes.Count != 1 ? "s" : "")} ausgewählt";
        }
        
        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text?.ToLower() ?? "";
            
            _filteredUsers.Clear();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var user in _users)
                {
                    _filteredUsers.Add(user);
                }
                LblSearchResult.Content = "";
            }
            else
            {
                var filtered = _users.Where(u =>
                    u.DisplayName?.ToLower().Contains(searchText) == true ||
                    u.SamAccountName?.ToLower().Contains(searchText) == true ||
                    u.Title?.ToLower().Contains(searchText) == true ||
                    u.Department?.ToLower().Contains(searchText) == true ||
                    u.Email?.ToLower().Contains(searchText) == true
                ).ToList();
                
                foreach (var user in filtered)
                {
                    _filteredUsers.Add(user);
                }
                
                LblSearchResult.Content = $"{filtered.Count} von {_users.Count} Benutzer gefunden";
            }
        }
        
        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
        }
        
        private void BtnSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_mappings.Count == 0)
            {
                MessageBox.Show("Keine Mappings zum Speichern vorhanden.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Template (*.json)|*.json|Alle Dateien (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"Mapping_Template_{DateTime.Now:yyyyMMdd}.json"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var template = new
                    {
                        Mappings = _mappings.ToList(),
                        CreatedDate = DateTime.Now
                    };
                    
                    var json = System.Text.Json.JsonSerializer.Serialize(template, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dialog.FileName, json);
                    
                    UpdateStatus($"Template gespeichert: {Path.GetFileName(dialog.FileName)}");
                    MessageBox.Show("Mapping-Template erfolgreich gespeichert!", 
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void BtnLoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Template (*.json)|*.json|Alle Dateien (*.*)|*.*",
                DefaultExt = "json"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var template = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    
                    // Lade Mappings
                    if (template.TryGetProperty("Mappings", out var mappingsElement))
                    {
                        var mappings = System.Text.Json.JsonSerializer.Deserialize<List<FieldMapping>>(mappingsElement.GetRawText());
                        
                        if (mappings != null)
                        {
                            _mappings.Clear();
                            foreach (var mapping in mappings)
                            {
                                _mappings.Add(mapping);
                            }
                        }
                    }
                    
                    UpdateStatus($"Template geladen: {Path.GetFileName(dialog.FileName)}");
                    MessageBox.Show($"{_mappings.Count} Mappings erfolgreich geladen!", 
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Laden: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.TreeViewItem item && item.DataContext is ADTreeNode node)
            {
                LoadChildrenOnDemand(node);
            }
        }
        
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ADTreeNode node)
            {
                // Aktualisiere Kind-Knoten rekursiv basierend auf ChkIncludeSubOUs
                var includeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
                if (includeSubOUs)
                {
                    SetChildrenCheckedState(node, node.IsChecked);
                }
            }
            
            // Aktualisiere die Anzeige nach Checkbox-Änderung
            UpdateSelectedCount();
        }

        private void SetChildrenCheckedState(ADTreeNode node, bool isChecked)
        {
            if (node.Children == null) return;
            
            foreach (var child in node.Children)
            {
                if (child.Name != "Loading...")
                {
                    child.IsChecked = isChecked;
                    SetChildrenCheckedState(child, isChecked);
                }
            }
        }

        private void ChkIncludeSubOUs_Changed(object sender, RoutedEventArgs e)
        {
            // Wenn Checkbox geändert wird, aktualisiere alle aktuell ausgewählten Nodes
            if (TreeAD == null || TreeAD.Items.Count == 0) return;
            
            if (TreeAD.Items[0] is ADTreeNode rootNode)
            {
                UpdateChildSelectionRecursive(rootNode);
            }
        }

        private void UpdateChildSelectionRecursive(ADTreeNode node)
        {
            if (node.IsChecked && node.Children != null)
            {
                var includeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
                if (includeSubOUs)
                {
                    // Aktiviere alle Kinder
                    SetChildrenCheckedState(node, true);
                }
                else
                {
                    // Deaktiviere alle Kinder
                    SetChildrenCheckedState(node, false);
                }
            }
            
            // Rekursiv für alle Kinder
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (child.Name != "Loading...")
                    {
                        UpdateChildSelectionRecursive(child);
                    }
                }
            }
        }
        
        private void TreeAD_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Wird aufgerufen wenn ein TreeView-Item ausgewählt wird
            if (e.NewValue is ADTreeNode node)
            {
                UpdateStatus($"Ausgewählt: {node.Name}");
            }
        }
        
        private void BtnLoadUsers_Click(object sender, RoutedEventArgs e)
        {
            var selectedNode = GetSelectedTreeNode();
            
            if (selectedNode == null || selectedNode.Name == "Loading...")
            {
                MessageBox.Show("Bitte wählen Sie eine OU aus dem Baum aus.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                UpdateStatus("Lade Benutzer...");
                var includeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
                var users = _adService.GetUsersFromOU(selectedNode.DistinguishedName, includeSubOUs);
                
                _users.Clear();
                _filteredUsers.Clear();
                foreach (var user in users)
                {
                    _users.Add(user);
                    _filteredUsers.Add(user);
                }
                
                LblUserCount.Content = _users.Count.ToString();
                UpdateStatus($"{_users.Count} Benutzer geladen aus {selectedNode.Name}");
                ShowQuickStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Benutzer: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Fehler beim Laden der Benutzer");
            }
        }
        
        private void ShowQuickStats()
        {
            if (_users.Count == 0)
            {
                StatsOverlay.Visibility = Visibility.Collapsed;
                return;
            }
            
            var positions = _users.GroupBy(u => u.Title).Where(g => !string.IsNullOrEmpty(g.Key)).OrderByDescending(g => g.Count()).Take(5);
            var departments = _users.GroupBy(u => u.Department).Where(g => !string.IsNullOrEmpty(g.Key)).OrderByDescending(g => g.Count()).Take(5);
            
            var stats = $"📊 Top Positionen:\n";
            foreach (var pos in positions)
            {
                stats += $"  • {pos.Key}: {pos.Count()}\n";
            }
            
            stats += $"\n📊 Top Abteilungen:\n";
            foreach (var dept in departments)
            {
                stats += $"  • {dept.Key}: {dept.Count()}\n";
            }
            
            TxtStats.Text = stats.TrimEnd();
            StatsOverlay.Visibility = Visibility.Visible;
            
            // Auto-hide nach 5 Sekunden
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                StatsOverlay.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
        

        
        private ADTreeNode? GetSelectedTreeNode()
        {
            // Finde ausgewähltes TreeViewItem
            var selectedItem = TreeAD.SelectedItem;
            if (selectedItem is ADTreeNode node && node.Name != "Loading...")
            {
                System.Diagnostics.Debug.WriteLine($"Ausgewählter Knoten: {node.Name} - DN: {node.DistinguishedName}");
                return node;
            }
            
            // Fallback: Suche in Items
            if (TreeAD.Items.Count > 0 && TreeAD.Items[0] is ADTreeNode firstNode)
            {
                var found = FindSelectedNode(firstNode);
                if (found != null && found.Name != "Loading...")
                {
                    System.Diagnostics.Debug.WriteLine($"Gefundener Knoten: {found.Name} - DN: {found.DistinguishedName}");
                    return found;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("Kein Knoten ausgewählt");
            return null;
        }
        
        private ADTreeNode? FindSelectedNode(ADTreeNode node)
        {
            if (node.IsSelected)
                return node;
                
            foreach (var child in node.Children)
            {
                var found = FindSelectedNode(child);
                if (found != null)
                    return found;
            }
            
            return null;
        }
        
        private void BtnLoadFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedNodes = GetCheckedNodes();
            
            if (selectedNodes.Count == 0)
            {
                MessageBox.Show("Bitte wählen Sie mindestens eine OU mit der Checkbox aus.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                UpdateStatus($"Lade Benutzer aus {selectedNodes.Count} OUs...");
                var includeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
                
                _users.Clear();
                int totalUsers = 0;
                
                foreach (var node in selectedNodes)
                {
                    var users = _adService.GetUsersFromOU(node.DistinguishedName, includeSubOUs);
                    foreach (var user in users)
                    {
                        // Verhindere Duplikate
                        if (!_users.Any(u => u.DistinguishedName == user.DistinguishedName))
                        {
                            _users.Add(user);
                            totalUsers++;
                        }
                    }
                }
                
                    _filteredUsers.Clear();
                foreach (var user in _users)
                {
                    _filteredUsers.Add(user);
                }
                    
                LblUserCount.Content = _users.Count.ToString();
                UpdateStatus($"{totalUsers} Benutzer aus {selectedNodes.Count} OUs geladen");
                ShowQuickStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Benutzer: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Fehler beim Laden der Benutzer");
            }
        }        private List<ADTreeNode> GetCheckedNodes()
        {
            var checkedNodes = new List<ADTreeNode>();
            
            foreach (var item in TreeAD.Items)
            {
                if (item is ADTreeNode node)
                {
                    CollectCheckedNodes(node, checkedNodes);
                }
            }
            
            return checkedNodes;
        }
        
        private void CollectCheckedNodes(ADTreeNode node, List<ADTreeNode> result)
        {
            if (node.IsChecked)
            {
                result.Add(node);
            }
            
            foreach (var child in node.Children)
            {
                CollectCheckedNodes(child, result);
            }
        }
        
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_users.Count == 0)
            {
                MessageBox.Show("Keine Benutzer zum Exportieren vorhanden.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new SaveFileDialog
            {
                Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"AD_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _adService.ExportToCSV(_users.ToList(), dialog.FileName);
                    UpdateStatus($"Export erfolgreich: {dialog.FileName}");
                    MessageBox.Show($"Daten erfolgreich exportiert nach:\n{dialog.FileName}", 
                        "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Export: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                DefaultExt = "csv"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var importedUsers = _adService.ImportFromCSV(dialog.FileName);
                    _users.Clear();
                    foreach (var user in importedUsers)
                    {
                        _users.Add(user);
                    }
                    
                    LblUserCount.Content = _users.Count.ToString();
                    UpdateStatus($"Import erfolgreich: {importedUsers.Count} Benutzer");
                    MessageBox.Show($"{importedUsers.Count} Benutzer importiert.", 
                        "Import erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Import: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void BtnAddFilterCondition_Click(object sender, RoutedEventArgs e)
        {
            var isFirstRow = PnlFilterConditions.Children.Count == 0;
            AddFilterConditionRow(isFirstRow);
        }

        private void BtnAddTargetAction_Click(object sender, RoutedEventArgs e)
        {
            AddTargetActionRow();
        }

        private void BtnCreateRule_Click(object sender, RoutedEventArgs e)
        {
            // Sammle Filter-Bedingungen
            var conditions = new List<FilterCondition>();
            foreach (var child in PnlFilterConditions.Children)
            {
                if (child is Grid grid)
                {
                    var toggle = grid.Children[0] as System.Windows.Controls.Primitives.ToggleButton;
                    var cmbField = grid.Children[1] as ComboBox;
                    var cmbValue = grid.Children[3] as ComboBox;

                    if (cmbField?.SelectedItem is ComboBoxItem fieldItem && !string.IsNullOrWhiteSpace(cmbValue?.Text))
                    {
                        var condition = new FilterCondition
                        {
                            Field = fieldItem.Tag?.ToString() ?? "title",
                            Value = cmbValue.Text.Trim(),
                            Operator = toggle?.Tag is LogicOperator op ? op : LogicOperator.AND
                        };
                        conditions.Add(condition);
                    }
                }
            }

            // Sammle Ziel-Änderungen
            var targetChanges = new List<TargetFieldChange>();
            foreach (var child in PnlTargetActions.Children)
            {
                if (child is Grid grid)
                {
                    var cmbField = grid.Children[0] as ComboBox;
                    var cmbValue = grid.Children[2] as ComboBox;

                    if (cmbField?.SelectedItem is ComboBoxItem fieldItem && !string.IsNullOrWhiteSpace(cmbValue?.Text))
                    {
                        var target = new TargetFieldChange
                        {
                            Field = fieldItem.Tag?.ToString() ?? "title",
                            NewValue = cmbValue.Text.Trim()
                        };
                        targetChanges.Add(target);
                    }
                }
            }

            // Validierung
            if (conditions.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie mindestens eine MUSS-Bedingung hinzu.", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (targetChanges.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie mindestens eine DANN-Änderung hinzu.", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Erstelle Regel-Zusammenfassung
            var conditionSummary = string.Join(" ", conditions.Select((c, i) => 
                i == 0 ? $"{c.Field}={c.Value}" : $"{(c.Operator == LogicOperator.AND ? "UND" : "ODER")} {c.Field}={c.Value}"));
            var actionSummary = string.Join(", ", targetChanges.Select(t => $"{t.Field}→{t.NewValue}"));

            var mapping = new FieldMapping
            {
                OldValue = $"[Regel: {conditionSummary}]",
                NewValue = actionSummary,
                Conditions = conditions,
                TargetChanges = targetChanges
            };

            _mappings.Add(mapping);

            // Clear Inputs
            PnlFilterConditions.Children.Clear();
            PnlTargetActions.Children.Clear();

            UpdateStatus($"Regel erstellt: {conditions.Count} Bedingung(en), {targetChanges.Count} Änderung(en)");
            MessageBox.Show($"Regel erfolgreich erstellt!\n\nBedingungen: {conditions.Count}\nÄnderungen: {targetChanges.Count}", 
                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddFilterConditionRow(bool isFirstRow)
        {
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            
            // Spalten: [UND/ODER Toggle] [Feld] [=] [Wert] [×]
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });   // 0: Toggle
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // 1: Feld
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });      // 2: =
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 3: Wert
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });      // 4: Delete

            // UND/ODER Toggle Button (nur ab zweiter Zeile sichtbar)
            var toggleButton = new System.Windows.Controls.Primitives.ToggleButton
            {
                Content = "UND",
                Margin = new Thickness(0, 0, 5, 0),
                Tag = LogicOperator.AND,
                Visibility = isFirstRow ? Visibility.Collapsed : Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 11
            };
            toggleButton.Checked += (s, e) =>
            {
                toggleButton.Content = "ODER";
                toggleButton.Tag = LogicOperator.OR;
            };
            toggleButton.Unchecked += (s, e) =>
            {
                toggleButton.Content = "UND";
                toggleButton.Tag = LogicOperator.AND;
            };

            // Feld auswählen
            var cmbField = new ComboBox
            {
                Margin = new Thickness(0, 0, 5, 0)
            };
            cmbField.Items.Add(new ComboBoxItem { Content = "Anzeigename", Tag = "displayname" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Benutzername", Tag = "samaccountname" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Position", Tag = "title" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Abteilung", Tag = "department" });
            cmbField.Items.Add(new ComboBoxItem { Content = "E-Mail", Tag = "mail" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Beschreibung", Tag = "description" });
            cmbField.Items.Add(new ComboBoxItem { Content = "OU", Tag = "ou" });
            cmbField.SelectedIndex = 0;
            cmbField.SelectionChanged += (s, e) => UpdateFilterValueDropdown(cmbField, grid);

            var lblEquals = new Label { Content = "=", FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 5, 0) };

            // Wert (ComboBox mit Auto-Complete)
            var cmbValue = new ComboBox
            {
                IsEditable = true,
                Margin = new Thickness(0, 0, 5, 0)
            };
            grid.Tag = cmbValue; // Speichere für UpdateFilterValueDropdown
            UpdateFilterValueDropdown(cmbField, grid);

            // Delete Button
            var btnDelete = new Button
            {
                Content = "×",
                Style = (Style)FindResource("SmallButton"),
                Tag = grid
            };
            btnDelete.Click += BtnDeleteFilterCondition_Click;

            Grid.SetColumn(toggleButton, 0);
            Grid.SetColumn(cmbField, 1);
            Grid.SetColumn(lblEquals, 2);
            Grid.SetColumn(cmbValue, 3);
            Grid.SetColumn(btnDelete, 4);

            grid.Children.Add(toggleButton);
            grid.Children.Add(cmbField);
            grid.Children.Add(lblEquals);
            grid.Children.Add(cmbValue);
            grid.Children.Add(btnDelete);

            PnlFilterConditions.Children.Add(grid);
        }

        private void AddTargetActionRow()
        {
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            
            // Spalten: [Feld] [→] [Neuer Wert] [×]
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Feld auswählen
            var cmbField = new ComboBox
            {
                Margin = new Thickness(0, 0, 5, 0)
            };
            cmbField.Items.Add(new ComboBoxItem { Content = "Anzeigename", Tag = "displayname" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Position", Tag = "title" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Abteilung", Tag = "department" });
            cmbField.Items.Add(new ComboBoxItem { Content = "E-Mail", Tag = "mail" });
            cmbField.Items.Add(new ComboBoxItem { Content = "Beschreibung", Tag = "description" });
            cmbField.SelectedIndex = 0;
            cmbField.SelectionChanged += (s, e) => UpdateTargetValueDropdown(cmbField, grid);

            var lblArrow = new Label { Content = "→", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 5, 0) };

            // Neuer Wert (ComboBox mit Auto-Complete)
            var cmbValue = new ComboBox
            {
                IsEditable = true,
                Margin = new Thickness(0, 0, 5, 0)
            };
            grid.Tag = cmbValue; // Speichere für UpdateTargetValueDropdown
            UpdateTargetValueDropdown(cmbField, grid);

            // Delete Button
            var btnDelete = new Button
            {
                Content = "×",
                Style = (Style)FindResource("SmallButton"),
                Tag = grid
            };
            btnDelete.Click += BtnDeleteTargetAction_Click;

            Grid.SetColumn(cmbField, 0);
            Grid.SetColumn(lblArrow, 1);
            Grid.SetColumn(cmbValue, 2);
            Grid.SetColumn(btnDelete, 3);

            grid.Children.Add(cmbField);
            grid.Children.Add(lblArrow);
            grid.Children.Add(cmbValue);
            grid.Children.Add(btnDelete);

            PnlTargetActions.Children.Add(grid);
        }

        private void UpdateTargetValueDropdown(ComboBox cmbField, Grid parentGrid)
        {
            var cmbValue = parentGrid.Tag as ComboBox;
            if (cmbValue == null) return;

            var selectedItem = cmbField.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            var fieldTag = selectedItem.Tag?.ToString();
            cmbValue.Items.Clear();

            var distinctValues = new HashSet<string>();
            foreach (var user in _users)
            {
                var value = GetUserFieldValue(user, fieldTag ?? "");
                if (!string.IsNullOrWhiteSpace(value))
                    distinctValues.Add(value);
            }

            foreach (var val in distinctValues.OrderBy(v => v))
            {
                cmbValue.Items.Add(val);
            }
        }

        private void UpdateFilterValueDropdown(ComboBox cmbField, Grid parentGrid)
        {
            var cmbValue = parentGrid.Tag as ComboBox;
            if (cmbValue == null) return;

            var selectedItem = cmbField.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            var fieldTag = selectedItem.Tag?.ToString();
            cmbValue.Items.Clear();

            var distinctValues = new HashSet<string>();
            foreach (var user in _users)
            {
                var value = GetUserFieldValue(user, fieldTag ?? "");
                if (!string.IsNullOrWhiteSpace(value))
                    distinctValues.Add(value);
            }

            foreach (var val in distinctValues.OrderBy(v => v))
            {
                cmbValue.Items.Add(val);
            }
        }

        private string GetUserFieldValue(ADUserInfo user, string fieldTag)
        {
            return fieldTag?.ToLower() switch
            {
                "displayname" => user.DisplayName,
                "samaccountname" => user.SamAccountName,
                "title" => user.Title,
                "department" => user.Department,
                "mail" => user.Email,
                "description" => user.Description,
                "ou" => user.OU,
                _ => ""
            };
        }

        private void BtnDeleteFilterCondition_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var grid = btn?.Tag as Grid;
            if (grid != null)
            {
                PnlFilterConditions.Children.Remove(grid);
                
                // Aktualisiere Sichtbarkeit der Toggle-Buttons (erste Zeile hat keinen)
                if (PnlFilterConditions.Children.Count > 0)
                {
                    var firstGrid = PnlFilterConditions.Children[0] as Grid;
                    if (firstGrid != null)
                    {
                        var firstToggle = firstGrid.Children[0] as System.Windows.Controls.Primitives.ToggleButton;
                        if (firstToggle != null)
                            firstToggle.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void BtnDeleteTargetAction_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var grid = btn?.Tag as Grid;
            if (grid != null)
            {
                PnlTargetActions.Children.Remove(grid);
            }
        }

        private void BtnDeleteMapping_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is FieldMapping mapping)
            {
                _mappings.Remove(mapping);
                UpdateStatus("Mapping gelöscht");
            }
        }
        
        private void BtnClearMappings_Click(object sender, RoutedEventArgs e)
        {
            _mappings.Clear();
            UpdateStatus("Mappings gelöscht");
        }
        
        private void BtnApplyMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_users.Count == 0)
            {
                MessageBox.Show("Keine Benutzer geladen.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            if (_mappings.Count == 0)
            {
                MessageBox.Show("Keine Mappings definiert.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                UpdateStatus("Erstelle Vorschau...");
                int changeCount = 0;
                
                foreach (var mapping in _mappings)
                {
                    if (mapping.UseAdvancedFilters)
                    {
                        // Filter-Logik mit Bedingungen
                        changeCount += ProcessAdvancedMapping(mapping);
                    }
                    else
                    {
                        // Legacy: Simple Mapping ohne Bedingungen (fallback)
                        // Diese werden jetzt auch als advanced mappings gespeichert
                        MessageBox.Show("Alte Simple Mappings werden nicht mehr unterstützt. Bitte nutzen Sie die neuen Mapping-Zeilen.", 
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                
                LblChangeCount.Content = _pendingChanges.Count.ToString();
                BtnPublish.IsEnabled = _pendingChanges.Count > 0;
                BtnClearChanges.IsEnabled = _pendingChanges.Count > 0;
                
                UpdateStatus($"{changeCount} Änderungen zur Vorschau hinzugefügt");
                MessageBox.Show($"{changeCount} Änderungen wurden zur Vorschau hinzugefügt.\n\nKlicken Sie auf 'Publish to AD' um die Änderungen ins Active Directory zu übertragen.", 
                    "Vorschau erstellt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen der Vorschau: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Fehler beim Erstellen der Vorschau");
            }
        }

        private int ProcessSimpleMapping(FieldMapping mapping, string fieldName, string displayFieldName)
        {
            int count = 0;
            
            foreach (var user in _users)
            {
                var currentValue = fieldName == "title" ? user.Title
                    : fieldName == "department" ? user.Department
                    : user.Description;
                
                if (currentValue == mapping.OldValue)
                {
                    var change = new PendingChange
                    {
                        UserDN = user.DistinguishedName,
                        UserDisplayName = user.DisplayName,
                        FieldName = displayFieldName,
                        OldValue = currentValue,
                        NewValue = mapping.NewValue
                    };
                    
                    if (!_pendingChanges.Any(c => c.UserDN == change.UserDN && c.FieldName == change.FieldName))
                    {
                        _pendingChanges.Add(change);
                        count++;
                    }
                }
            }
            
            return count;
        }

        private int ProcessAdvancedMapping(FieldMapping mapping)
        {
            int count = 0;
            
            foreach (var user in _users)
            {
                // Prüfe Bedingungen mit AND/OR Logik
                bool conditionsMet = EvaluateConditions(user, mapping.Conditions);
                
                if (conditionsMet)
                {
                    // Wende alle Ziel-Änderungen an
                    foreach (var targetChange in mapping.TargetChanges)
                    {
                        var displayFieldName = targetChange.Field == "Title" ? "Position"
                            : targetChange.Field == "Department" ? "Abteilung"
                            : targetChange.Field == "Description" ? "Beschreibung"
                            : targetChange.Field;
                        
                        var currentValue = GetUserFieldValue(user, targetChange.Field);
                        
                        var change = new PendingChange
                        {
                            UserDN = user.DistinguishedName,
                            UserDisplayName = user.DisplayName,
                            FieldName = displayFieldName,
                            OldValue = currentValue,
                            NewValue = targetChange.NewValue
                        };
                        
                        if (!_pendingChanges.Any(c => c.UserDN == change.UserDN && c.FieldName == change.FieldName))
                        {
                            _pendingChanges.Add(change);
                            count++;
                        }
                    }
                }
            }
            
            return count;
        }

        private bool EvaluateConditions(ADUserInfo user, List<FilterCondition> conditions)
        {
            if (conditions.Count == 0) return false;

            // Erste Bedingung hat immer implizit AND (wird aber ignoriert)
            bool result = GetUserFieldValue(user, conditions[0].Field) == conditions[0].Value;

            for (int i = 1; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                var conditionMet = GetUserFieldValue(user, condition.Field) == condition.Value;

                if (condition.Operator == LogicOperator.OR)
                {
                    result = result || conditionMet;
                }
                else // AND
                {
                    result = result && conditionMet;
                }
            }

            return result;
        }

        private void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingChanges.Count == 0)
            {
                MessageBox.Show("Keine ausstehenden Änderungen vorhanden.", 
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"Möchten Sie {_pendingChanges.Count} Änderungen ins Active Directory übertragen?\n\n" +
                $"ACHTUNG: Diese Änderungen werden direkt im AD durchgeführt!",
                "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    UpdateStatus($"Übertrage {_pendingChanges.Count} Änderungen...");
                    int successCount = 0;
                    var errors = new List<string>();
                    
                    foreach (var change in _pendingChanges.ToList())
                    {
                        try
                        {
                            var fieldName = change.FieldName.ToLower() == "position" ? "title"
                                : change.FieldName.ToLower() == "abteilung" ? "department"
                                : "description";
                            
                            _adService.UpdateUserField(change.UserDN, fieldName, change.NewValue);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{change.UserDisplayName}: {ex.Message}");
                        }
                    }
                    
                    _pendingChanges.Clear();
                    LblChangeCount.Content = "0";
                    BtnPublish.IsEnabled = false;
                    BtnClearChanges.IsEnabled = false;
                    
                    if (errors.Count == 0)
                    {
                        UpdateStatus($"{successCount} Änderungen erfolgreich ins AD übertragen");
                        MessageBox.Show($"Alle {successCount} Änderungen wurden erfolgreich ins Active Directory übertragen!", 
                            "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        UpdateStatus($"{successCount} erfolgreich, {errors.Count} Fehler");
                        MessageBox.Show($"{successCount} Änderungen erfolgreich.\n\n{errors.Count} Fehler:\n{string.Join("\n", errors.Take(5))}", 
                            "Teilweise erfolgreich", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
                    // Benutzer neu laden
                    ReloadCurrentUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Übertragen: {ex.Message}", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus("Fehler beim Publish");
                }
            }
        }
        
        private void BtnClearChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingChanges.Count == 0)
                return;
                
            var result = MessageBox.Show(
                $"Möchten Sie alle {_pendingChanges.Count} ausstehenden Änderungen verwerfen?",
                "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _pendingChanges.Clear();
                LblChangeCount.Content = "0";
                BtnPublish.IsEnabled = false;
                BtnClearChanges.IsEnabled = false;
                UpdateStatus("Alle Änderungen verworfen");
            }
        }
        
        private void BtnRemoveChange_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is PendingChange change)
            {
                _pendingChanges.Remove(change);
                LblChangeCount.Content = _pendingChanges.Count.ToString();
                BtnPublish.IsEnabled = _pendingChanges.Count > 0;
                BtnClearChanges.IsEnabled = _pendingChanges.Count > 0;
                UpdateStatus("Änderung entfernt");
            }
        }
        
        private void ReloadCurrentUsers()
        {
            var selectedNode = GetSelectedTreeNode();
            if (selectedNode != null && selectedNode.Name != "Loading...")
            {
                try
                {
                    var includeSubOUs = ChkIncludeSubOUs.IsChecked ?? true;
                    var users = _adService.GetUsersFromOU(selectedNode.DistinguishedName, includeSubOUs);
                    _users.Clear();
                    _filteredUsers.Clear();
                    foreach (var user in users)
                    {
                        _users.Add(user);
                        _filteredUsers.Add(user);
                    }
                    LblUserCount.Content = _users.Count.ToString();
                }
                catch
                {
                    // Fehler ignorieren beim Neuladen
                }
            }
        }
        
        private void UpdateStatus(string message)
        {
            TxtStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Link konnte nicht geöffnet werden: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
