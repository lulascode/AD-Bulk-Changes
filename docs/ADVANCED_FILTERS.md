# Erweiterte Filter-Regeln - Benutzerhandbuch

## 🎯 Übersicht

Die erweiterten Filter-Regeln ermöglichen komplexe UND/ODER-Verknüpfungen und das gleichzeitige Ändern mehrerer Felder basierend auf Bedingungen.

---

## 🔧 Zugriff

**Hauptfenster** → **Feld-Mapping Konfiguration** → **🔧 Erweiterte Filter-Regeln (UND/ODER)** Expander öffnen

Die erweiterten Filter sind direkt im Hauptfenster integriert - **kein separates Dialog-Fenster** mehr nötig!

---

## 📋 Funktionen

### 1. **Mehrere Bedingungen (bis zu 4)**
- Erste Bedingung wird direkt hinzugefügt
- Ab zweiter Bedingung: Wahl zwischen **UND** oder **ODER**
- Beispiel: `Position = "Manager" UND Abteilung = "IT"`

### 2. **Mehrere Ziel-Felder**
- Ändere gleichzeitig mehrere Felder bei denselben Benutzern
- Beispiel: Setze `Position = "GF"` UND `Abteilung = "Management"`

---

## 💡 Anwendungsbeispiele

### Beispiel 1: Einfache UND-Verknüpfung
```
WENN (Position = "Geschäftsführer" UND Abteilung = "Verwaltung")
→ DANN Setze: Position = "GF", Beschreibung = "Geschäftsleitung"
```

**Ergebnis:** Nur Benutzer mit Position "Geschäftsführer" UND Abteilung "Verwaltung" werden geändert.

---

### Beispiel 2: ODER-Verknüpfung
```
WENN (Position = "Manager" ODER Position = "Leiter")
→ DANN Setze: Position = "Führungskraft"
```

**Ergebnis:** Alle Benutzer mit Position "Manager" ODER "Leiter" bekommen "Führungskraft".

---

### Beispiel 3: Gemischte Verknüpfung
```
WENN (Position = "Verkäufer" UND Abteilung = "Vertrieb" ODER Abteilung = "Sales")
→ DANN Setze: Position = "VK", Abteilung = "Verkauf"
```

**Auswertung:** `(Position = "Verkäufer" UND Abteilung = "Vertrieb") ODER (Abteilung = "Sales")`

---

### Beispiel 4: Vier Bedingungen (Maximum)
```
WENN (Position = "IT-Admin" UND Abteilung = "IT" ODER Description = "System" ODER Email ENTHÄLT "@admin")
→ DANN Setze: Position = "Administrator", Beschreibung = "IT-Systemadministrator"
```

---

## 🚀 Schritt-für-Schritt Anleitung

### **Schritt 1: Expander öffnen**
1. Im Hauptfenster unten: Öffne **"🔧 Erweiterte Filter-Regeln (UND/ODER)"**
2. Der Bereich klappt auf und zeigt zwei Abschnitte

### **Schritt 2: Filter-Bedingungen hinzufügen**
1. Klicke **"+ Filter-Bedingung hinzufügen"**
2. **Erste Zeile:**
   - Wähle Feld (z.B. "Position (Title)")
   - Wähle/Gib Wert ein (z.B. "Manager")
   - Kein Operator sichtbar (erste Bedingung)

3. **Zweite Zeile:**
   - Klicke erneut **"+ Filter-Bedingung hinzufügen"**
   - **Toggle-Button erscheint:** Wähle **UND** oder **ODER**
   - Wähle Feld und Wert
   - Der Toggle zeigt "UND" (Standard) - klicke zum Umschalten auf "ODER"

4. **Weitere Zeilen (bis zu 4 total):**
   - Jede neue Zeile hat einen UND/ODER Toggle vorne
   - Mit **×** Button können Zeilen einzeln gelöscht werden

### **Schritt 3: Ziel-Felder definieren**
1. Scrolle zum Abschnitt **"Änderungen anwenden (Ziel-Felder)"**
2. Klicke **"+ Ziel-Feld hinzufügen"**
3. Wähle Feld (Position, Abteilung, Beschreibung)
4. Gib neuen Wert ein
5. Füge beliebig viele Ziel-Felder hinzu
6. Mit **×** Button können Zeilen gelöscht werden

### **Schritt 4: Regel erstellen**
- Klicke **"✓ Erweiterte Regel erstellen"**
- Regel wird validiert und zur Mapping-Liste hinzugefügt
- Eingabefelder werden automatisch geleert

### **Schritt 5: Änderungen anwenden**
- Klicke **"🔍 Vorschau erstellen"**
- Prüfe die Änderungen in der rechten Liste
- Klicke **"📤 Publish to AD"** zum Übertragen

---

## ⚙️ Verfügbare Felder

### **Für Bedingungen:**
- Position (Title)
- Abteilung (Department)
- Beschreibung (Description)
- E-Mail (Email)
- Anzeigename (DisplayName)
- Benutzername (SamAccountName)

### **Für Ziel-Änderungen:**
- Position (Title)
- Abteilung (Department)
- Beschreibung (Description)

---

## 🔍 Logik-Auswertung

### **UND-Verknüpfung:**
- **Alle** Bedingungen müssen erfüllt sein
- `A UND B` = Nur wenn A **und** B wahr sind

### **ODER-Verknüpfung:**
- **Mindestens eine** Bedingung muss erfüllt sein
- `A ODER B` = Wenn A **oder** B (oder beide) wahr sind

### **Kombiniert:**
```
Bedingung1 UND Bedingung2 ODER Bedingung3 UND Bedingung4
= ((Bedingung1 UND Bedingung2) ODER Bedingung3) UND Bedingung4
```

**Auswertung von links nach rechts:**
1. Erste Bedingung wird geprüft
2. Operator bestimmt Verknüpfung mit nächster Bedingung
3. Ergebnis wird mit nächster Bedingung verknüpft
4. usw.

---

## ✅ Best Practices

### **1. Klare Bedingungen**
❌ Schlecht: `Description = "irgendwas"`
✅ Gut: `Position = "Manager" UND Abteilung = "IT"`

### **2. Teste mit Vorschau**
- Erstelle immer erst eine Vorschau
- Prüfe die Anzahl betroffener Benutzer
- Bei Unsicherheit: Exportiere als CSV

### **3. Dokumentiere komplexe Regeln**
- Speichere als Template mit beschreibendem Namen
- Beispiel: `GF-Titel-Normalisierung.json`

### **4. Start Simple**
- Beginne mit 1-2 Bedingungen
- Teste die Logik
- Erweitere schrittweise

---

## 🛠️ Troubleshooting

### **Problem: "Keine Änderungen gefunden"**
**Lösung:**
- Prüfe ob Benutzer mit diesen Werten existieren
- Nutze die Suche im Hauptfenster
- Kontrolliere Groß-/Kleinschreibung

### **Problem: "Zu viele Änderungen"**
**Lösung:**
- ODER-Bedingungen sind breiter → mehr Treffer
- Füge zusätzliche UND-Bedingungen hinzu
- Nutze spezifischere Werte

### **Problem: "Button deaktiviert"**
**Lösung:**
- Mindestens 1 Bedingung erforderlich
- Mindestens 1 Ziel-Feld erforderlich
- Maximum 4 Bedingungen

---

## 💾 Template-Verwaltung

Erweiterte Regeln können als Template gespeichert werden:

1. **Speichern:**
   - Erstelle erweiterte Regel
   - Im Hauptfenster: **💾 Mapping-Template speichern**
   - Wähle Dateinamen (z.B. `it-manager-promotion.json`)

2. **Laden:**
   - **📋 Template laden**
   - Wähle gespeicherte JSON-Datei
   - Regel wird wiederhergestellt

---

## 🔐 Sicherheit

### **Vor dem Publish:**
- ✅ Prüfe die Anzahl der Änderungen
- ✅ Kontrolliere einzelne Benutzer in der Liste
- ✅ Bei Unsicherheit: Teste an Test-OU
- ✅ Erstelle Backup (CSV-Export)

### **Nach dem Publish:**
- ✅ Prüfe im AD ob Änderungen korrekt
- ✅ Bei Fehlern: Nutze CSV-Import zur Wiederherstellung

---

## 📞 Support

Bei Fragen zur erweiterten Filter-Logik:
1. Prüfe die Vorschau-Anzeige
2. Teste mit einfachen Beispielen
3. Dokumentiere deine Regeln
4. Nutze Templates für komplexe Szenarien

---

**Version:** 2.0  
**Letzte Aktualisierung:** 20.11.2025  
**Feature:** Erweiterte Filter mit UND/ODER und Multi-Target
