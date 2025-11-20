# Design System

Das AD Bulk Changes Tool verwendet ein durchgängiges Design-System mit Blau als Hauptakzentfarbe.

## Farbpalette

### Primärfarben
- **PrimaryBlue** (`#2196F3`): Hauptfarbe für primäre Aktionen
- **PrimaryBlueDark** (`#1976D2`): Dunklere Variante für Hover-Zustände
- **PrimaryBlueLight** (`#BBDEFB`): Helle Variante für Hintergründe

### Akzentfarben
- **AccentBlue** (`#03A9F4`): Alternative Akzentfarbe
- **SuccessGreen** (`#4CAF50`): Für positive Aktionen (z.B. "Hinzufügen")
- **WarningOrange** (`#FF9800`): Für Warnungen
- **DangerRed** (`#F44336`): Für destruktive Aktionen (z.B. "Löschen")

### Neutrale Farben
- **BackgroundLight** (`#FAFAFA`): Helle Hintergründe
- **BackgroundWhite** (`#FFFFFF`): Weiße Hintergründe
- **BorderGray** (`#E0E0E0`): Rahmenfarbe
- **TextPrimary** (`#212121`): Haupttextfarbe
- **TextSecondary** (`#757575`): Sekundärtextfarbe

## Button-Styles

### Standard Button
```xaml
<Button Content="Aktion" />
```
- Blaue Hintergrundfarbe (PrimaryBlue)
- Weiße Schrift
- Abgerundete Ecken (3px)
- Hover-Effekt: Dunkler (PrimaryBlueDark)

### Success Button
```xaml
<Button Content="Hinzufügen" Style="{StaticResource SuccessButton}" />
```
- Grüne Hintergrundfarbe (SuccessGreen)
- Für positive Aktionen (z.B. "Hinzufügen", "Speichern")

### Danger Button
```xaml
<Button Content="Löschen" Style="{StaticResource DangerButton}" />
```
- Rote Hintergrundfarbe (DangerRed)
- Für destruktive Aktionen (z.B. "Löschen", "Verwerfen")

### Secondary Button
```xaml
<Button Content="Abbrechen" Style="{StaticResource SecondaryButton}" />
```
- Weißer Hintergrund mit blauem Rahmen
- Blaue Schrift
- Für sekundäre Aktionen

### Small Button
```xaml
<Button Content="×" Style="{StaticResource SmallButton}" />
```
- Kleine runde Buttons (24x24px)
- Rote Hintergrundfarbe
- Für Inline-Löschaktionen

## Komponenten-Styles

### TextBox
- Grauer Rahmen (BorderGray)
- Fokus: Blauer Rahmen (PrimaryBlue), 2px
- Abgerundete Ecken (3px)
- Padding: 8px horizontal, 6px vertikal

### ComboBox
- Wie TextBox
- Padding: 8px horizontal, 6px vertikal

### GroupBox
- Blauer Header-Bereich (PrimaryBlueLight)
- Blaue Unterlinie im Header (PrimaryBlue)
- Weißer Content-Bereich
- Grauer Rahmen (BorderGray)

### DataGrid
- Weiße Zeilen, abwechselnd hellgraue Zeilen
- Blauer Header (PrimaryBlueLight)
- Horizontale Gitternetzlinien (BorderGray)

### Expander
- Blauer Rahmen (PrimaryBlue, 2px)
- Weißer Hintergrund

## Verwendung im Code

### XAML
Die Styles sind in `src/App.xaml` als Application Resources definiert und werden automatisch auf alle Komponenten angewendet.

### C# Code-Behind
Für dynamisch erstellte Buttons:
```csharp
var button = new Button
{
    Content = "Aktion",
    Style = (Style)FindResource("SuccessButton")
};
```

## Visuelle Hierarchie

1. **Primäre Aktionen**: Standard Button (Blau)
   - "Vorschau erstellen"
   - "Benutzer laden"
   
2. **Positive Aktionen**: Success Button (Grün)
   - "Publish to AD"
   - "+ Filter hinzufügen"
   
3. **Destruktive Aktionen**: Danger Button (Rot)
   - "Änderungen verwerfen"
   - "Alle Mappings löschen"
   
4. **Sekundäre Aktionen**: Secondary Button (Weiß mit blauem Rahmen)
   - "Export CSV"
   - "Import CSV"
   - "Template speichern/laden"

5. **Inline-Löschungen**: Small Button (Kleine rote Kreise)
   - "×" in Listen und dynamischen Zeilen

## Abstände und Größen

- **Margins**: Standard 5px
- **Padding**: Buttons 12px horizontal, 6px vertikal
- **Corner Radius**: 3px für Buttons, TextBoxes, etc.
- **MinWidth**: Buttons 100px
- **Font Weights**: 
  - Medium für Labels und Buttons
  - SemiBold für GroupBox Headers und DataGrid Headers
  - Bold für hervorgehobene Werte

## Best Practices

1. **Konsistenz**: Verwende immer die definierten Styles statt expliziter Farben
2. **Semantik**: Wähle den Button-Style basierend auf der Aktion (primär/sekundär/destruktiv)
3. **Lesbarkeit**: Achte auf ausreichenden Kontrast zwischen Text und Hintergrund
4. **Feedback**: Alle interaktiven Elemente haben Hover-Effekte
5. **Accessibility**: Disabled-Zustände sind klar erkennbar (50% Opacity)
