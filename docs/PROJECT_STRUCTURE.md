# AD Bulk Changes - Projektstruktur

```
AD-Bulk-Changes/
│
├── 📂 src/                              # Quellcode (SOURCE CODE)
│   ├── 📄 AD-BulkChanges.csproj        # MSBuild Projektdatei
│   ├── 📄 App.xaml                      # WPF Application Definition
│   ├── 📄 App.xaml.cs                   # Application Entry Point
│   │
│   ├── 📂 Models/                       # Datenmodelle (DATA MODELS)
│   │   ├── ADUserInfo.cs               # AD Benutzer Repräsentation
│   │   ├── ADTreeNode.cs               # TreeView Node für AD-Struktur
│   │   ├── FieldMapping.cs             # Wert-Mapping Definition (Alt → Neu)
│   │   ├── PendingChange.cs            # Ausstehende Änderungen vor AD-Sync
│   │   └── AppSettings.cs              # Persistente App-Einstellungen
│   │
│   ├── 📂 Services/                     # Business-Logik (SERVICES)
│   │   ├── ADService.cs                # Active Directory LDAP-Operationen
│   │   └── SettingsService.cs          # JSON Settings Persistenz
│   │
│   ├── 📂 Views/                        # UI-Komponenten (USER INTERFACE)
│   │   ├── MainWindow.xaml             # Haupt-UI Layout (XAML)
│   │   ├── MainWindow.xaml.cs          # Haupt-UI Logik (C# Code-Behind)
│   │   ├── SettingsWindow.xaml         # Einstellungs-Dialog Layout
│   │   └── SettingsWindow.xaml.cs      # Einstellungs-Dialog Logik
│   │
│   └── 📂 Resources/                    # Ressourcen (ASSETS)
│       └── app.ico                      # Anwendungs-Icon (256x256 Multi-Size)
│
├── 📂 build/                            # Build-Tools (BUILD SYSTEM)
│   ├── 📄 build.ps1                    # PowerShell Build-Script
│   └── 📄 BUILD.md                     # Build-Anleitung & Troubleshooting
│
├── 📂 docs/                             # Dokumentation (DOCUMENTATION)
│   └── 📄 README.md                    # Benutzer-Handbuch
│
├── 📂 Deploy/                           # Ausgabe (OUTPUT)
│   └── 📄 AD-BulkChanges.exe           # Standalone Executable (68 MB)
│
└── 📂 .git/                             # Git Repository
```

---

## 📋 Datei-Verantwortlichkeiten

### 🎯 **Core Application**
| Datei | Zweck | Schlüsselfunktionen |
|-------|-------|---------------------|
| **App.xaml.cs** | Entry Point | Anwendung starten, globale Ressourcen |
| **MainWindow.xaml.cs** | Hauptlogik | UI-Events, AD-Loading, Mapping, Publish |

### 📊 **Models (Datenstrukturen)**
| Datei | Repräsentiert | Hauptattribute |
|-------|---------------|----------------|
| **ADUserInfo.cs** | AD-Benutzer | DisplayName, SamAccountName, Title, Department, DN |
| **ADTreeNode.cs** | TreeView-Knoten | Name, DN, IsChecked, Children (Hierarchie) |
| **FieldMapping.cs** | Wert-Transformation | OldValue → NewValue |
| **PendingChange.cs** | Lokale Änderung | User, Field, OldValue, NewValue |
| **AppSettings.cs** | App-Konfiguration | Server, Credentials, Window-Position, Templates |

### ⚙️ **Services (Business-Logik)**
| Datei | Verantwortlich für | Technologie |
|-------|-------------------|-------------|
| **ADService.cs** | AD-Operationen | LDAP (DirectoryServices) |
| **SettingsService.cs** | Settings-Persistenz | JSON (Newtonsoft.Json) |

### 🖥️ **Views (UI-Komponenten)**
| Datei | UI-Element | Hauptfunktionen |
|-------|------------|----------------|
| **MainWindow.xaml** | Haupt-UI | TreeView, DataGrid, Mapping-Panel, Search |
| **SettingsWindow.xaml** | Einstellungs-Dialog | Server-Config, Credentials, Connection-Test |

---

## 🔄 Datenfluss-Übersicht

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERFACE LAYER                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ MainWindow.xaml.cs                                     │ │
│  │ • TreeView (AD Browser)                                │ │
│  │ • DataGrid (User List)                                 │ │
│  │ • Mapping Configuration                                │ │
│  │ • Pending Changes Panel                                │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────┬────────────────────────────────────┬──┘
                       │                                    │
                       ▼                                    ▼
┌──────────────────────────────────┐   ┌────────────────────────────┐
│      BUSINESS LOGIC LAYER        │   │      SETTINGS LAYER        │
│  ┌────────────────────────────┐  │   │  ┌──────────────────────┐ │
│  │ ADService.cs               │  │   │  │ SettingsService.cs   │ │
│  │ • LoadADStructure()        │  │   │  │ • LoadSettings()     │ │
│  │ • LoadUsers()              │  │   │  │ • SaveSettings()     │ │
│  │ • UpdateUser()             │  │   │  └──────────────────────┘ │
│  │ • ExportCSV()              │  │   │           │                │
│  └────────────────────────────┘  │   │           ▼                │
│              │                    │   │  %AppData%\AD-BulkChanges │
│              ▼                    │   │     settings.json          │
│  ┌────────────────────────────┐  │   └────────────────────────────┘
│  │ System.DirectoryServices   │  │
│  │ • LDAP Queries             │  │
│  │ • DirectoryEntry           │  │
│  │ • DirectorySearcher        │  │
│  └────────────────────────────┘  │
└──────────────────┬────────────────┘
                   │
                   ▼
         ┌──────────────────┐
         │  ACTIVE DIRECTORY │
         │  (Domain Server)  │
         └──────────────────┘
```

---

## 🔧 Build-Prozess

```
┌──────────────────────────────────────────────────────────────┐
│  1. ENTWICKLUNG                                              │
│  └─> src/**/*.cs + src/**/*.xaml                            │
│                                                              │
│  2. BUILD-SCRIPT AUSFÜHREN                                   │
│  └─> build/build.ps1                                        │
│                                                              │
│  3. .NET COMPILER                                            │
│  └─> dotnet publish (MSBuild)                               │
│      • NuGet Pakete laden                                   │
│      • Code kompilieren                                     │
│      • Icon einbetten                                       │
│      • Single-File EXE erstellen                            │
│                                                              │
│  4. AUSGABE                                                  │
│  └─> Deploy/AD-BulkChanges.exe                              │
│      • Self-contained (keine .NET Installation nötig)       │
│      • Größe: ~68 MB                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## 🚀 Quick Start

### Für Benutzer:
```powershell
# Programm starten
.\Deploy\AD-BulkChanges.exe
```

### Für Entwickler:
```powershell
# Kompilieren
.\build\build.ps1

# Oder mit Visual Studio
code src\AD-BulkChanges.csproj
```

---

## 📦 Dependencies (NuGet-Pakete)

| Paket | Version | Zweck |
|-------|---------|-------|
| **System.DirectoryServices** | 8.0.0 | LDAP / Active Directory |
| **System.DirectoryServices.AccountManagement** | 8.0.0 | User Account Management |
| **CsvHelper** | 30.0.1 | CSV Import/Export |
| **Newtonsoft.Json** | (implizit) | JSON Serialisierung |

---

## 🔐 Berechtigungen

### Zur Laufzeit benötigt:
- **Netzwerk:** Zugriff auf Domain Controller (LDAP Port 389/636)
- **Active Directory:** Leserechte auf User-Objekte
- **Active Directory:** Schreibrechte für Bulk-Updates (beim Publish)
- **Dateisystem:** Schreiben in `%AppData%\AD-BulkChanges\` für Settings

### Entwicklung benötigt:
- **.NET 8.0 SDK** oder höher
- **Windows 10/11** (64-bit)
- **PowerShell 5.1+**

---

## 📝 Wartung & Updates

### Code-Änderungen:
1. Dateien in `src/` bearbeiten
2. `.\build\build.ps1` ausführen
3. Neue EXE in `Deploy/` testen

### Settings-Reset:
```powershell
Remove-Item "$env:APPDATA\AD-BulkChanges\settings.json"
```

### Clean Build:
```powershell
.\build\build.ps1 -Clean
```
