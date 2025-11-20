using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AD_BulkChanges.Models;

namespace AD_BulkChanges
{
    public partial class AdvancedMappingWindow : Window
    {
        private ObservableCollection<FilterCondition> _conditions = new ObservableCollection<FilterCondition>();
        private ObservableCollection<TargetFieldChange> _targetChanges = new ObservableCollection<TargetFieldChange>();
        private Dictionary<string, string> _fieldMap = new Dictionary<string, string>
        {
            { "Position (Title)", "Title" },
            { "Abteilung (Department)", "Department" },
            { "Beschreibung (Description)", "Description" },
            { "E-Mail (Email)", "Email" },
            { "Anzeigename (DisplayName)", "DisplayName" },
            { "Benutzername (SamAccountName)", "SamAccountName" }
        };

        public FieldMapping? ResultMapping { get; private set; }
        private List<ADUserInfo> _availableUsers;
        private const int MAX_CONDITIONS = 4;

        public AdvancedMappingWindow(List<ADUserInfo> users, string? selectedField = null)
        {
            InitializeComponent();
            _availableUsers = users;

            if (selectedField != null)
            {
                foreach (ComboBoxItem item in CmbTargetField.Items)
                {
                    if (item.Content.ToString()!.Contains(selectedField))
                    {
                        CmbTargetField.SelectedItem = item;
                        break;
                    }
                }
            }

            TxtTargetValue.TextChanged += TxtTargetValue_TextChanged;
            CmbConditionField.SelectionChanged += CmbConditionField_SelectionChanged;
            CmbLogicOperator.Visibility = Visibility.Collapsed; // Erste Bedingung hat keinen Operator
            UpdateConditionValueDropdown();
        }

        private void BtnAddTarget_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTargetField.SelectedItem is not ComboBoxItem item)
            {
                MessageBox.Show("Bitte wählen Sie ein Ziel-Feld aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var value = TxtTargetValue.Text?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Bitte geben Sie einen Ziel-Wert ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var field = _fieldMap[item.Content.ToString()!];

            // Prüfe ob Feld bereits existiert
            if (_targetChanges.Any(t => t.Field == field))
            {
                MessageBox.Show("Dieses Feld wurde bereits hinzugefügt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var target = new TargetFieldChange { Field = field, NewValue = value };
            _targetChanges.Add(target);
            AddTargetToUI(target);
            TxtTargetValue.Clear();
            UpdatePreview();
            ValidateForm();
        }

        private void AddTargetToUI(TargetFieldChange target)
        {
            TxtNoTargets.Visibility = Visibility.Collapsed;

            var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };

            var fieldName = _fieldMap.FirstOrDefault(kv => kv.Value == target.Field).Key ?? target.Field;
            textBlock.Inlines.Add(new System.Windows.Documents.Run(fieldName) { FontWeight = FontWeights.Bold });
            textBlock.Inlines.Add(new System.Windows.Documents.Run(" = "));
            textBlock.Inlines.Add(new System.Windows.Documents.Run($"'{target.NewValue}'") { Foreground = System.Windows.Media.Brushes.Green });

            var btnRemove = new Button
            {
                Content = "×",
                Width = 20,
                Height = 20,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Tag = target
            };
            btnRemove.Click += BtnRemoveTarget_Click;

            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(btnRemove, 1);

            grid.Children.Add(textBlock);
            grid.Children.Add(btnRemove);

            PnlTargets.Children.Add(grid);
        }

        private void BtnRemoveTarget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TargetFieldChange target)
            {
                _targetChanges.Remove(target);

                var gridToRemove = PnlTargets.Children.OfType<Grid>()
                    .FirstOrDefault(g => g.Children.OfType<Button>().Any(b => b.Tag == target));

                if (gridToRemove != null)
                {
                    PnlTargets.Children.Remove(gridToRemove);
                }

                if (_targetChanges.Count == 0)
                {
                    TxtNoTargets.Visibility = Visibility.Visible;
                }

                UpdatePreview();
                ValidateForm();
            }
        }

        private void CmbConditionField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateConditionValueDropdown();
        }

        private void UpdateConditionValueDropdown()
        {
            if (CmbConditionField.SelectedItem is ComboBoxItem item)
            {
                var field = _fieldMap[item.Content.ToString()!];
                var values = new HashSet<string>();

                foreach (var user in _availableUsers)
                {
                    var value = field switch
                    {
                        "Title" => user.Title,
                        "Department" => user.Department,
                        "Description" => user.Description,
                        "Email" => user.Email,
                        "DisplayName" => user.DisplayName,
                        "SamAccountName" => user.SamAccountName,
                        _ => ""
                    };

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        values.Add(value);
                    }
                }

                CmbConditionValue.ItemsSource = values.OrderBy(v => v).ToList();
            }
        }

        private void BtnAddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (_conditions.Count >= MAX_CONDITIONS)
            {
                MessageBox.Show($"Maximal {MAX_CONDITIONS} Bedingungen erlaubt.", "Limit erreicht", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbConditionField.SelectedItem is not ComboBoxItem fieldItem)
            {
                MessageBox.Show("Bitte wählen Sie ein Feld aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var value = CmbConditionValue.Text?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Bitte geben Sie einen Wert ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var field = _fieldMap[fieldItem.Content.ToString()!];
            var operatorItem = CmbLogicOperator.SelectedItem as ComboBoxItem;
            var logicOp = operatorItem?.Content.ToString() == "ODER" ? LogicOperator.OR : LogicOperator.AND;

            var condition = new FilterCondition 
            { 
                Field = field, 
                Value = value,
                Operator = logicOp
            };

            // Prüfe ob Bedingung bereits existiert
            if (_conditions.Any(c => c.Field == condition.Field && c.Value == condition.Value))
            {
                MessageBox.Show("Diese Bedingung existiert bereits.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _conditions.Add(condition);
            AddConditionToUI(condition);
            CmbConditionValue.Text = "";
            
            // Zeige Operator-Auswahl für nächste Bedingung
            if (_conditions.Count == 1)
            {
                CmbLogicOperator.Visibility = Visibility.Visible;
            }

            // Deaktiviere Button bei 4 Bedingungen
            if (_conditions.Count >= MAX_CONDITIONS)
            {
                BtnAddCondition.IsEnabled = false;
                BtnAddCondition.Content = "Max. 4 erreicht";
            }

            UpdatePreview();
            ValidateForm();
        }

        private void AddConditionToUI(FilterCondition condition)
        {
            TxtNoConditions.Visibility = Visibility.Collapsed;

            var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };

            var fieldName = _fieldMap.FirstOrDefault(kv => kv.Value == condition.Field).Key ?? condition.Field;
            
            // Zeige Operator außer bei erster Bedingung
            if (_conditions.IndexOf(condition) > 0)
            {
                var opText = condition.Operator == LogicOperator.OR ? "ODER" : "UND";
                textBlock.Inlines.Add(new System.Windows.Documents.Run($"{opText} ") 
                { 
                    Foreground = System.Windows.Media.Brushes.Purple, 
                    FontWeight = FontWeights.Bold 
                });
            }

            textBlock.Inlines.Add(new System.Windows.Documents.Run(fieldName) { FontWeight = FontWeights.Bold });
            textBlock.Inlines.Add(new System.Windows.Documents.Run(" = "));
            textBlock.Inlines.Add(new System.Windows.Documents.Run($"'{condition.Value}'") { Foreground = System.Windows.Media.Brushes.Blue });

            var btnRemove = new Button
            {
                Content = "×",
                Width = 20,
                Height = 20,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0, 0, 0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Tag = condition
            };
            btnRemove.Click += BtnRemoveCondition_Click;

            Grid.SetColumn(textBlock, 0);
            Grid.SetColumn(btnRemove, 1);

            grid.Children.Add(textBlock);
            grid.Children.Add(btnRemove);

            PnlConditions.Children.Add(grid);
        }

        private void BtnRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FilterCondition condition)
            {
                _conditions.Remove(condition);

                // Rebuild UI to update operators
                PnlConditions.Children.Clear();
                PnlConditions.Children.Add(TxtNoConditions);

                if (_conditions.Count == 0)
                {
                    TxtNoConditions.Visibility = Visibility.Visible;
                    CmbLogicOperator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtNoConditions.Visibility = Visibility.Collapsed;
                    foreach (var cond in _conditions)
                    {
                        AddConditionToUI(cond);
                    }
                }

                // Re-enable button wenn unter 4
                if (_conditions.Count < MAX_CONDITIONS)
                {
                    BtnAddCondition.IsEnabled = true;
                    BtnAddCondition.Content = "+ Bedingung";
                }

                UpdatePreview();
                ValidateForm();
            }
        }

        private void UpdatePreview()
        {
            if (_conditions.Count == 0)
            {
                TxtPreview.Text = "Keine Bedingungen definiert";
                return;
            }

            var conditionStrings = new List<string>();
            for (int i = 0; i < _conditions.Count; i++)
            {
                var c = _conditions[i];
                var fieldName = _fieldMap.FirstOrDefault(kv => kv.Value == c.Field).Key ?? c.Field;
                
                if (i > 0)
                {
                    conditionStrings.Add(c.Operator == LogicOperator.OR ? "ODER" : "UND");
                }
                conditionStrings.Add($"{fieldName} = '{c.Value}'");
            }

            var targetStrings = new List<string>();
            foreach (var target in _targetChanges)
            {
                var fieldName = _fieldMap.FirstOrDefault(kv => kv.Value == target.Field).Key ?? target.Field;
                targetStrings.Add($"{fieldName} = '{target.NewValue}'");
            }

            var targetText = targetStrings.Count > 0 
                ? string.Join(", ", targetStrings)
                : "Keine Ziel-Felder";

            TxtPreview.Text = $"WENN ({string.Join(" ", conditionStrings)})\n→ DANN Setze: {targetText}";
        }

        private void ValidateForm()
        {
            var hasConditions = _conditions.Count > 0;
            var hasTargets = _targetChanges.Count > 0;

            BtnOK.IsEnabled = hasConditions && hasTargets;
        }

        private void TxtTargetValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            ValidateForm();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_conditions.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie mindestens eine Bedingung hinzu.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_targetChanges.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie mindestens ein Ziel-Feld hinzu.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var summary = $"{_conditions.Count} Bedingung(en), {_targetChanges.Count} Ziel-Feld(er)";

            ResultMapping = new FieldMapping
            {
                OldValue = "[Erweiterte Regel]",
                NewValue = summary,
                Conditions = _conditions.ToList(),
                TargetChanges = _targetChanges.ToList()
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
