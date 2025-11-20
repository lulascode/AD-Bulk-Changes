# AD Bulk Changes - Build Anleitung

## 🔧 Voraussetzungen

### Erforderliche Software:
1. **.NET 8.0 SDK** (oder höher)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Prüfen: `dotnet --version` im Terminal

2. **Windows 10/11** (64-bit)
   - PowerShell 5.1 oder höher

### Optional:
- **Visual Studio 2022** (für Code-Bearbeitung)
- **VS Code** mit C# Extension

---

## 🚀 Build-Prozess

### Methode 1: Build-Script (Empfohlen)

```powershell
# Navigiere zum build-Ordner
cd c:\Git\AD-Bulk-Changes\build

# Standard Release Build
.\build.ps1

# Mit Clean (entfernt alte Builds)
.\build.ps1 -Clean

# Debug Build
.\build.ps1 -Configuration Debug
```

**Ausgabe:** `Deploy\AD-BulkChanges.exe` (ca. 68 MB)

---

### Methode 2: Manueller Build

```powershell
# 1. Zum Quellcode-Ordner wechseln
cd c:\Git\AD-Bulk-Changes\src

# 2. NuGet-Pakete wiederherstellen
dotnet restore

# 3. Release Build erstellen
dotnet publish `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true

# 4. EXE befindet sich dann in:
# bin\Release\net8.0-windows\win-x64\publish\AD-BulkChanges.exe
```

---

### Methode 3: Visual Studio

1. Öffne `src\AD-BulkChanges.csproj` in Visual Studio
2. Wähle **Release** Configuration
3. Rechtsklick auf Projekt → **Veröffentlichen**
4. Profil auswählen oder neu erstellen:
   - **Ziel:** Ordner
   - **Runtime:** win-x64
   - **Bereitstellungsmodus:** Eigenständig
   - **Einzeldatei:** Ja
5. Klicke auf **Veröffentlichen**

---

## 📦 Build-Optionen erklärt

| Parameter | Beschreibung |
|-----------|--------------|
| `-c Release` | Optimierte Version ohne Debug-Symbole |
| `-r win-x64` | Windows 64-bit Runtime |
| `--self-contained true` | Enthält .NET Runtime (keine Installation nötig) |
| `PublishSingleFile=true` | Alles in einer EXE |
| `IncludeNativeLibrariesForSelfExtract=true` | Native DLLs einbetten |
| `EnableCompressionInSingleFile=true` | Komprimierung aktivieren |

---

## 🐛 Troubleshooting

### Fehler: "dotnet: command not found"
**Lösung:** .NET SDK installieren und Terminal neu starten

### Fehler: "Projekt konnte nicht geladen werden"
**Lösung:** 
```powershell
cd src
dotnet restore
```

### Fehler: "NuGet-Paket nicht gefunden"
**Lösung:**
```powershell
# NuGet-Cache löschen
dotnet nuget locals all --clear
dotnet restore
```

### EXE ist zu groß (>100 MB)
**Normal:** Self-contained Apps enthalten die komplette .NET Runtime (~68 MB)
**Alternative:** Framework-abhängige Version (benötigt .NET auf Zielrechner):
```powershell
dotnet publish -c Release --self-contained false
```

---

## 📂 Projektstruktur nach Build

```
AD-Bulk-Changes/
├── src/                        # Quellcode
│   ├── AD-BulkChanges.csproj  # Projektdatei
│   ├── App.xaml               # WPF Application
│   ├── Models/                # Datenmodelle
│   ├── Services/              # Business-Logik
│   ├── Views/                 # UI-Komponenten
│   └── Resources/             # Icons, Bilder
├── build/                      # Build-Tools
│   ├── build.ps1              # Build-Script
│   └── BUILD.md               # Diese Datei
├── docs/                       # Dokumentation
│   └── README.md              # Benutzer-Anleitung
└── Deploy/                     # Fertige EXE
    └── AD-BulkChanges.exe     # Standalone Executable
```

---

## 🔄 Kontinuierliche Integration (CI/CD)

### GitHub Actions Beispiel:

```yaml
name: Build Release

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore src/AD-BulkChanges.csproj
    
    - name: Build
      run: dotnet publish src/AD-BulkChanges.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: AD-BulkChanges
        path: src/bin/Release/net8.0-windows/win-x64/publish/AD-BulkChanges.exe
```

---

## ✅ Checkliste vor Release

- [ ] Code kompiliert ohne Fehler
- [ ] Icon ist korrekt eingebettet (`app.ico`)
- [ ] Version in `.csproj` aktualisiert
- [ ] Alle Tests erfolgreich
- [ ] README.md aktualisiert
- [ ] Deploy-EXE funktioniert auf Test-System
- [ ] Git-Tag für Release erstellt

---

## 📞 Support

Bei Problemen:
1. Prüfe `.NET SDK` Installation
2. Lösche `bin/` und `obj/` Ordner
3. Führe `dotnet restore` aus
4. Versuche erneut zu kompilieren
