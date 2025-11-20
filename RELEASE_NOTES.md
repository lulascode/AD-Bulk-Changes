# 🚀 AD Bulk Changes v1.0.0

## 📥 Download & Installation

**Fertige Anwendung (68 MB):**
```
AD-BulkChanges.exe
```

### Installation
1. `AD-BulkChanges.exe` herunterladen
2. Direkt ausführen – keine Installation nötig
3. Windows 10/11 (64-bit) erforderlich

---

## ✨ Was ist neu?

### 🎯 Hauptfunktionen

**🔍 Intelligente Filter-Regeln**
- MUSS-Bedingungen: Benutzer nach Kriterien filtern (z.B. `Abteilung = Verkauf`)
- UND/ODER-Logik für komplexe Bedingungen
- Mehrere Filter kombinierbar

**🎨 DANN-Aktionen**
- Multi-Field Updates auf einen Schlag
- Wert-Transformationen (z.B. `GeschFühr` → `GF`)
- Bis zu 7 AD-Felder bearbeitbar:
  - Anzeigename
  - Position
  - Abteilung
  - E-Mail
  - Beschreibung
  - Benutzername (Anzeige)
  - OU

**👁️ Sichere Vorschau**
- Alle Änderungen vor dem Speichern prüfen
- Lokale Simulation ohne AD-Zugriff
- Änderungen können korrigiert werden

**🌳 AD-Browser**
- Intuitive TreeView-Navigation
- "Unterordner mit auswählen"-Checkbox für rekursive Auswahl
- Multi-OU Unterstützung

**📦 Export & Import**
- CSV-Export für Dokumentation
- CSV-Import für Massen-Updates
- Template-System für wiederverwendbare Regeln

---

## 🎯 Beispiel-Workflow

### Szenario: "Alle Verkaufsleiter umbenennen"

```
MUSS:
  └─ Abteilung = "Verkauf" UND Position = "Leitung"

DANN:
  └─ Position → "Vertriebsleiter"
```

**Ergebnis:** Alle Benutzer mit `Abteilung=Verkauf` UND `Position=Leitung` erhalten automatisch `Position=Vertriebsleiter`.

### Schritte:
1. 🌳 **OU auswählen** – Klick auf Ordner im AD-Tree
2. ✅ **Benutzer laden** – Button oder Checkbox "Unterordner mit auswählen"
3. 📝 **Regel erstellen** – MUSS-Bedingungen + DANN-Aktionen hinzufügen
4. 👁️ **Vorschau** – Änderungen in der Tabelle kontrollieren
5. 💾 **Speichern** – Button "Änderungen ins AD schreiben"

---

## ⚙️ Technische Details

| Eigenschaft | Wert |
|------------|------|
| **Framework** | .NET 8.0 (Windows) |
| **UI** | WPF |
| **Größe** | 68.67 MB (Self-Contained) |
| **Plattform** | Windows 10/11 (64-bit) |
| **AD-Protokoll** | LDAP via System.DirectoryServices |

---

## 📋 Voraussetzungen

### Runtime
- ✅ Windows 10 oder Windows 11 (64-bit)
- ✅ Active Directory Zugriffsrechte (Lesen + Schreiben)
- ✅ Netzwerkzugriff auf Domain Controller

### Berechtigungen
- Lesezugriff auf OU-Struktur
- Schreibzugriff für Benutzer-Attribute
- LDAP-Port 389 (Standard)

---

## 🔧 Bekannte Einschränkungen

- Nur Benutzer-Objekte werden unterstützt (keine Gruppen/Computer)
- Maximale Benutzeranzahl pro Vorgang: ~10.000 (Performance)
- Keine Unterstützung für verschachtelte Gruppen-Mitgliedschaften

---

## 🐛 Bugfixes in v1.0.0

- ✅ NullReferenceException beim Checkbox "Unterordner mit auswählen" behoben
- ✅ Dropdown-Auto-Complete für alle verfügbaren AD-Felder
- ✅ Recursive SubOU-Selection funktioniert zuverlässig
- ✅ Build-Fehler mit nicht-existierenden Model-Properties behoben

---

## 📚 Dokumentation

- [📖 Vollständige Dokumentation](https://github.com/lulascode/AD-Bulk-Changes/tree/main/docs)
- [🔧 Build-Anleitung](https://github.com/lulascode/AD-Bulk-Changes/blob/main/build/BUILD.md)
- [🏗️ Architektur-Übersicht](https://github.com/lulascode/AD-Bulk-Changes/blob/main/docs/PROJECT_STRUCTURE.md)

---

## 💬 Support & Feedback

- 🐛 **Bug melden:** [GitHub Issues](https://github.com/lulascode/AD-Bulk-Changes/issues)
- 💡 **Feature-Request:** [GitHub Issues](https://github.com/lulascode/AD-Bulk-Changes/issues)
- 📧 **Kontakt:** Siehe Repository

---

## 📄 Lizenz

**Proprietary** – Alle Rechte vorbehalten

---

<div align="center">

**Made with ❤️ for efficient Active Directory management**

[⬆ Zurück zum Repo](https://github.com/lulascode/AD-Bulk-Changes)

</div>
