# AD Bulk Changes Tool - Deployment

## Version 1.0

### Installationsanleitung

1. Kopieren Sie `AD-BulkChanges.exe` an einen beliebigen Ort
2. Die Anwendung ist vollständig eigenständig - keine weiteren Installationen nötig
3. Doppelklick auf `AD-BulkChanges.exe` zum Starten

### Systemanforderungen

- Windows 10/11 (64-bit)
- Active Directory Zugriff
- Berechtigungen zum Lesen/Schreiben von AD-Objekten

### Erste Schritte

1. **⚙️ Einstellungen** klicken
2. AD-Server konfigurieren (optional - verwendet standardmäßig aktuelle Domain)
3. **AD Laden** klicken
4. Ordner im Baum auswählen
5. **Benutzer laden**

### Features

✅ **AD Browser** - Navigieren durch OU-Struktur mit Pfeilen
✅ **Mehrfachauswahl** - Checkboxen für mehrere OUs gleichzeitig
✅ **Suchfunktion** - Schnelles Finden von Benutzern (Ctrl+F)
✅ **Feld-Mapping** - Position, Abteilung, Beschreibung ändern
✅ **Vorschau-Modus** - Änderungen lokal prüfen vor AD-Sync
✅ **Template-System** - Mappings speichern und wiederverwenden
✅ **Export/Import** - CSV-Dateien für Batch-Bearbeitung
✅ **Auto-Save** - Einstellungen werden automatisch gespeichert

### Tastenkombinationen

- `Ctrl+S` - Einstellungen öffnen
- `Ctrl+F` - Suche fokussieren
- `Ctrl+E` - Export CSV
- `F5` - Benutzer neu laden

### Datenspeicherung

Einstellungen werden gespeichert in:
```
%AppData%\AD-BulkChanges\settings.json
```

### Workflow-Beispiel

1. Ordner auswählen und Benutzer laden
2. Feld auswählen (z.B. "Position")
3. Mapping erstellen (z.B. "GeschFühr" → "GF")
4. **🔍 Vorschau erstellen** klicken
5. Änderungen im rechten Panel prüfen
6. **📤 Publish to AD** klicken

### Sicherheit

⚠️ **WICHTIG**:
- Änderungen werden direkt ins Active Directory geschrieben
- Erstellen Sie vorher einen Export mit **💾 Export CSV**
- Testen Sie mit wenigen Benutzern
- Prüfen Sie die Vorschau vor dem Publish

### Support

Bei Fragen oder Problemen wenden Sie sich an Ihren Administrator.

### Changelog

**Version 1.0** (2025-11-20)
- Initiale Release
- AD Browser mit TreeView
- Feld-Mapping für Position, Abteilung, Beschreibung
- Vorschau-Modus mit lokalem Change-Management
- Template-System für wiederverwendbare Mappings
- Suchfunktion
- Auto-Save Einstellungen
- CSV Export/Import
- Keyboard Shortcuts
