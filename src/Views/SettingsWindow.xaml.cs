using System;
using System.Windows;
using AD_BulkChanges.Models;

namespace AD_BulkChanges
{
    public partial class SettingsWindow : Window
    {
        public ADSettings Settings { get; private set; }
        
        public SettingsWindow(ADSettings currentSettings)
        {
            InitializeComponent();
            Settings = currentSettings;
            
            // Aktuelle Einstellungen laden
            if (currentSettings != null)
            {
                ChkUseCurrentCredentials.IsChecked = currentSettings.UseCurrentCredentials;
                TxtServerName.Text = currentSettings.ServerName ?? string.Empty;
                TxtDomainDN.Text = currentSettings.DomainDN ?? string.Empty;
                TxtUsername.Text = currentSettings.Username ?? string.Empty;
            }
            
            UpdateControlStates();
        }
        
        private void ChkUseCurrentCredentials_Changed(object sender, RoutedEventArgs e)
        {
            UpdateControlStates();
        }
        
        private void UpdateControlStates()
        {
            if (ChkUseCurrentCredentials == null || TxtUsername == null || TxtPassword == null)
                return;
                
            bool useCurrentCreds = ChkUseCurrentCredentials.IsChecked ?? true;
            TxtUsername.IsEnabled = !useCurrentCreds;
            TxtPassword.IsEnabled = !useCurrentCreds;
            
            if (LblUsername != null)
                LblUsername.IsEnabled = !useCurrentCreds;
            if (LblPassword != null)
                LblPassword.IsEnabled = !useCurrentCreds;
        }
        
        private void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testSettings = GetSettingsFromUI();
                var adService = new Services.ADService();
                
                // Versuche die Verbindung zu testen
                string defaultNC = adService.GetDefaultNamingContext(testSettings);
                
                MessageBox.Show(
                    $"Verbindung erfolgreich!\n\nDefault Naming Context:\n{defaultNC}",
                    "Verbindungstest erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Verbindungstest fehlgeschlagen:\n\n{ex.Message}",
                    "Verbindungsfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Settings = GetSettingsFromUI();
            DialogResult = true;
            Close();
        }
        
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private ADSettings GetSettingsFromUI()
        {
            return new ADSettings
            {
                UseCurrentCredentials = ChkUseCurrentCredentials.IsChecked ?? true,
                ServerName = TxtServerName.Text.Trim(),
                DomainDN = TxtDomainDN.Text.Trim(),
                Username = TxtUsername.Text.Trim(),
                Password = TxtPassword.Password
            };
        }
    }
}
