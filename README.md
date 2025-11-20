# AD Bulk Changes Tool

Ein Windows-Tool zur Durchführung von Bulk-Updates im Active Directory mit grafischer Benutzeroberfläche.

## Features

- **AD Browser**: Navigieren durch die OU-Struktur des Active Directory
- **Benutzer-Export/Import**: Export und Import von Benutzerdaten als CSV
- **Feld-Mapping**: Definieren von Wert-Mappings für Bulk-Updates
- **Bulk-Updates**: Massenänderungen an AD-Benutzerfeldern (Position, Abteilung, Beschreibung)
- **Filter**: SubOU-Optionen für selektive Benutzerauswahl

## Voraussetzungen

- Windows 10/11
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Zugriff auf Active Directory (Domain-Mitgliedschaft)
- Berechtigungen zum Lesen/Schreiben von AD-Objekten

## Installation

1. .NET 8.0 SDK installieren von: https://dotnet.microsoft.com/download/dotnet/8.0
2. Projekt klonen oder herunterladen
3. Im Projektverzeichnis ausführen:
   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```

## Verwendung

### 1. AD-Struktur laden
- Klicken Sie auf "AD Laden" um die OU-Struktur zu laden
- Navigieren Sie durch den Baum und wählen Sie eine OU aus

### 2. Benutzer laden
- Wählen Sie eine OU im Baum aus
- Option "SubOUs einbeziehen" aktivieren/deaktivieren
- Klicken Sie auf "Benutzer laden"

### 3. Export/Import
- **Export**: Speichert geladene Benutzer als CSV-Datei
- **Import**: Lädt Benutzer aus CSV-Datei

### 4. Mapping konfigurieren
- Wählen Sie das zu ändernde Feld aus (Position, Abteilung, Beschreibung)
- Fügen Sie Mappings hinzu: Alter Wert → Neuer Wert
- Beispiel: "GeschFühr" → "GF"

### 5. Bulk-Update durchführen
- Klicken Sie auf "Mapping anwenden"
- Bestätigen Sie die Änderungen

## Beispiel-Mappings

Für das Feld "Position":
- GeschFühr → GF
- VerkaLeit → VKL
- Entwickler → Dev
- Administrator → Admin

## Sicherheitshinweise

⚠️ **WICHTIG**: 
- Änderungen werden direkt im Active Directory durchgeführt
- Erstellen Sie vor größeren Änderungen ein Backup oder exportieren Sie die Daten
- Testen Sie Mappings zunächst mit einer kleinen Gruppe von Benutzern
- Stellen Sie sicher, dass Sie die erforderlichen Berechtigungen haben

## Technische Details

- **Framework**: WPF mit .NET 8.0
- **AD-Zugriff**: System.DirectoryServices
- **CSV-Verarbeitung**: CsvHelper
- **Pattern**: MVVM-ähnliche Struktur

## Lizenz

Für interne Verwendung
