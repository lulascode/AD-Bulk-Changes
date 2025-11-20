using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using AD_BulkChanges.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AD_BulkChanges.Services
{
    public class ADService
    {
        private ADSettings _settings;
        
        public ADService()
        {
            _settings = new ADSettings();
        }
        
        public void UpdateSettings(ADSettings settings)
        {
            _settings = settings;
        }
        
        public string GetDefaultNamingContext(ADSettings? settings = null)
        {
            var useSettings = settings ?? _settings;
            
            try
            {
                string ldapPath = string.IsNullOrEmpty(useSettings.ServerName) 
                    ? "LDAP://RootDSE" 
                    : $"LDAP://{useSettings.ServerName}/RootDSE";
                
                DirectoryEntry rootDSE;
                
                if (useSettings.UseCurrentCredentials || string.IsNullOrEmpty(useSettings.Username))
                {
                    rootDSE = new DirectoryEntry(ldapPath);
                }
                else
                {
                    rootDSE = new DirectoryEntry(ldapPath, useSettings.Username, useSettings.Password);
                }
                
                using (rootDSE)
                {
                    var defaultNC = rootDSE.Properties["defaultNamingContext"][0]?.ToString() ?? string.Empty;
                    
                    // Wenn Domain DN angegeben ist, verwende diese stattdessen
                    if (!string.IsNullOrEmpty(useSettings.DomainDN))
                    {
                        return useSettings.DomainDN;
                    }
                    
                    return defaultNC;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Verbinden mit AD: {ex.Message}", ex);
            }
        }
        
        public ADTreeNode LoadOUStructure(string? distinguishedName = null)
        {
            if (string.IsNullOrEmpty(distinguishedName))
            {
                distinguishedName = GetDefaultNamingContext();
            }
            
            var node = new ADTreeNode
            {
                DistinguishedName = distinguishedName,
                Name = GetFriendlyName(distinguishedName),
                Type = "Domain"
            };
            
            // Füge Dummy hinzu damit Expand-Button erscheint
            node.Children.Add(new ADTreeNode { Name = "Loading..." });
            
            LoadChildren(node);
            return node;
        }
        
        private void LoadChildren(ADTreeNode parentNode)
        {
            try
            {
                // Entferne Dummy
                parentNode.Children.Clear();
                
                var entry = CreateDirectoryEntry($"LDAP://{parentNode.DistinguishedName}");
                using (entry)
                {
                    using var searcher = new DirectorySearcher(entry)
                    {
                        Filter = "(|(objectClass=organizationalUnit)(objectClass=container)(objectClass=domain))",
                        SearchScope = SearchScope.OneLevel
                    };
                    
                    searcher.PropertiesToLoad.AddRange(new[] { "name", "distinguishedName", "objectClass" });
                    
                    var results = searcher.FindAll();
                
                    foreach (SearchResult result in results)
                    {
                        var childNode = new ADTreeNode
                        {
                            Name = result.Properties["name"].Count > 0 
                                ? result.Properties["name"][0]?.ToString() ?? "Unknown"
                                : GetFriendlyName(result.Properties["distinguishedName"][0]?.ToString() ?? ""),
                            DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                            Type = "OU",
                            Parent = parentNode
                        };
                        
                        // Füge Dummy hinzu damit Expand-Button erscheint
                        childNode.Children.Add(new ADTreeNode { Name = "Loading..." });
                        
                        parentNode.Children.Add(childNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Kinder von {parentNode.DistinguishedName}: {ex.Message}");
            }
        }
        
        public List<ADUserInfo> GetUsersFromOU(string ouDistinguishedName, bool includeSubOUs = true)
        {
            var users = new List<ADUserInfo>();
            
            // Validiere Input
            if (string.IsNullOrEmpty(ouDistinguishedName) || ouDistinguishedName == "Loading...")
            {
                return users;
            }
            
            try
            {
                Console.WriteLine($"Lade Benutzer von: {ouDistinguishedName}");
                Console.WriteLine($"SubOUs einbeziehen: {includeSubOUs}");
                
                var entry = CreateDirectoryEntry($"LDAP://{ouDistinguishedName}");
                using (entry)
                {
                    using var searcher = new DirectorySearcher(entry)
                    {
                        Filter = "(&(objectClass=user)(objectCategory=person))",
                        SearchScope = includeSubOUs ? SearchScope.Subtree : SearchScope.OneLevel
                    };
                    
                    searcher.PropertiesToLoad.AddRange(new[] 
                    { 
                        "distinguishedName", "sAMAccountName", "displayName", 
                        "title", "department", "mail", "description" 
                    });
                    
                    Console.WriteLine($"Suche mit Filter: {searcher.Filter}");
                    var results = searcher.FindAll();
                    Console.WriteLine($"Gefunden: {results.Count} Benutzer");
                
                    foreach (SearchResult result in results)
                {
                    var user = new ADUserInfo
                    {
                        DistinguishedName = GetProperty(result, "distinguishedName"),
                        SamAccountName = GetProperty(result, "sAMAccountName"),
                        DisplayName = GetProperty(result, "displayName"),
                        Title = GetProperty(result, "title"),
                        Department = GetProperty(result, "department"),
                        Email = GetProperty(result, "mail"),
                        Description = GetProperty(result, "description"),
                        OU = ouDistinguishedName
                    };
                    
                        users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Laden der Benutzer: {ex.Message}", ex);
            }
            
            return users;
        }
        
        public void UpdateUserField(string userDN, string fieldName, string newValue)
        {
            try
            {
                var entry = CreateDirectoryEntry($"LDAP://{userDN}");
                using (entry)
                {
                    var adFieldName = fieldName.ToLower() switch
                {
                    "title" => "title",
                    "position" => "title",
                    "department" => "department",
                    "description" => "description",
                    _ => fieldName
                };
                
                    if (entry.Properties.Contains(adFieldName))
                    {
                        entry.Properties[adFieldName].Value = newValue;
                        entry.CommitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Update von {userDN}: {ex.Message}", ex);
            }
        }
        
        private DirectoryEntry CreateDirectoryEntry(string path)
        {
            if (_settings.UseCurrentCredentials || string.IsNullOrEmpty(_settings.Username))
            {
                return new DirectoryEntry(path);
            }
            else
            {
                return new DirectoryEntry(path, _settings.Username, _settings.Password);
            }
        }
        
        public void BulkUpdateField(List<ADUserInfo> users, string fieldName, Dictionary<string, string> mappings)
        {
            foreach (var user in users)
            {
                var currentValue = fieldName.ToLower() switch
                {
                    "title" => user.Title,
                    "position" => user.Title,
                    "department" => user.Department,
                    "description" => user.Description,
                    _ => string.Empty
                };
                
                if (mappings.TryGetValue(currentValue, out var newValue))
                {
                    UpdateUserField(user.DistinguishedName, fieldName, newValue);
                }
            }
        }
        
        public void ExportToCSV(List<ADUserInfo> users, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
                csv.WriteRecords(users);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Export: {ex.Message}", ex);
            }
        }
        
        public List<ADUserInfo> ImportFromCSV(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                return csv.GetRecords<ADUserInfo>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Import: {ex.Message}", ex);
            }
        }
        
        private string GetProperty(SearchResult result, string propertyName)
        {
            if (result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0]?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
        
        private string GetFriendlyName(string distinguishedName)
        {
            if (string.IsNullOrEmpty(distinguishedName))
                return "Unknown";
                
            var parts = distinguishedName.Split(',');
            if (parts.Length > 0)
            {
                var firstPart = parts[0];
                if (firstPart.Contains('='))
                {
                    return firstPart.Split('=')[1];
                }
            }
            return distinguishedName;
        }
    }
}
