# GitHub Copilot Instructions for Sudoku .NET MAUI Project

## 🚨 CRITICAL: Verify MAUI APIs Before Suggesting

**STOP AND VERIFY before suggesting any MAUI control property, method, or event.**

The .NET MAUI API surface changes between major versions. This project targets **.NET 10** using `$(MauiVersion)` from the SDK.

### API Verification Priority

1. **First: Check the existing codebase.** Search `Sudoku.Maui/Controls/`, `Sudoku.Maui/Pages/`, and `Sudoku.Maui/Helpers/` for how the control is already used. If the project compiles, those properties are confirmed to work.
2. **Second: If the control or property is NOT already used in the codebase**, or if you're getting a build error about a property not existing, fetch the official API reference to verify:
   `https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.{classname}?view=net-maui-10.0`
3. **Never guess** a property name. If you can't find it in the codebase or the docs, say so.

### General Principles

- Suggest cross-platform MAUI solutions first, not platform-specific workarounds
- Work with the framework, not against it
- Acknowledge when you need to verify something

---

## 🚨 Known XAML Pitfalls

### Complex Properties Need Object Syntax

Some XAML properties expect object values, not simple strings or booleans. Debug builds may silently tolerate invalid syntax, but **Release builds crash** with XAML parsing exceptions (`Exception code: 0xc000027b`).

**Shadow** is the most common offender:
```xml
<!-- ❌ WRONG — crashes in Release -->
<Border Shadow="True">

<!-- ✅ CORRECT — Shadow requires an object -->
<Border>
    <Border.Shadow>
        <Shadow Brush="Black" Opacity="0.3" Radius="4" Offset="2,2"/>
    </Border.Shadow>
</Border>
```

Same pattern applies to `Stroke`, `FormattedText`, and similar complex properties. See existing usage in `NumberPadButton.xaml` and `SettingsPage.xaml` for correct patterns.

---

## ℹ️ Project Summary

**Project:** Sudoku Game - .NET MAUI  
**Target:** .NET 10, C# 14

### Architecture

- 🚨 `TreatWarningsAsErrors` enabled globally in `Directory.Build.props`
- ✅ Three-project structure: `Sudoku.Core` (solver/models) → `Sudoku.Application` (DI interfaces, settings/state models) → `Sudoku.Maui` (UI)
- ✅ DI registration in `MauiProgram.cs` (singletons for services, transient for pages)
- ✅ Theme system: `LightTheme.xaml` / `DarkTheme.xaml` with `{DynamicResource}` bindings
- ✅ Layout: 3-row `Grid` with `SquareLayoutControl` for the board, responsive sizing via `SudokuLayoutCalculator`

### Key Custom Controls & Helpers

| File | Purpose |
|------|---------|
| `Controls/SquareLayoutControl.cs` | Custom `Layout` that maintains 1:1 aspect ratio for the Sudoku grid |
| `Controls/SudokuBoardControl.cs` | Renders the 9×9 grid with borders and cells |
| `Controls/NumberPadButton.xaml` | Circular number input button with remaining-count indicator |
| `Controls/GameSummaryOverlay.xaml` | Popup shown on game completion |
| `Controls/DifficultySelectionOverlay.xaml` | Difficulty picker overlay |
| `Helpers/SudokuLayoutCalculator.cs` | Calculates button/font sizes from window dimensions |
| `Helpers/CellHighlightManager.cs` | Manages cell selection, highlighting, and error coloring |

---

## ℹ️ THEME SYSTEM

### How Themes Work

Themes are defined in separate XAML ResourceDictionary files with code-behind:
- `Resources/Styles/Themes/LightTheme.xaml` + `.cs`
- `Resources/Styles/Themes/DarkTheme.xaml` + `.cs`

**Theme Loading Pattern:**
1. Theme classes are instantiated in C#: `new LightTheme()` or `new DarkTheme()`
2. Added to `Application.Current.Resources.MergedDictionaries`
3. Controls access theme colors by searching through merged dictionaries

**IMPORTANT:** Theme colors are NOT directly in `Application.Current.Resources`. They live in the merged dictionaries and must be accessed by iterating:

```csharp
// ✅ CORRECT way to get theme colors
foreach (var dict in Application.Current.Resources.MergedDictionaries)
{
    if (dict.ContainsKey("CellDefaultColor"))
        color = (Color)dict["CellDefaultColor"];
}

// ❌ WRONG - this will return False even if theme is loaded!
Application.Current.Resources.ContainsKey("CellDefaultColor")
```

### Theme Switching

**In App.xaml.cs:**
```csharp
public void LoadTheme(AppTheme theme)
{
    // Remove old theme
    var oldTheme = Resources.MergedDictionaries.FirstOrDefault(d => 
        d.GetType().Name == "LightTheme" || d.GetType().Name == "DarkTheme");
    if (oldTheme != null)
        Resources.MergedDictionaries.Remove(oldTheme);
    
    // Add new theme
    if (theme == AppTheme.Dark)
        Resources.MergedDictionaries.Add(new Resources.Styles.Themes.DarkTheme());
    else
        Resources.MergedDictionaries.Add(new Resources.Styles.Themes.LightTheme());
}
```

**In Settings:**
```csharp
Application.Current!.UserAppTheme = AppTheme.Dark; // or AppTheme.Light
if (Application.Current is App app)
    app.LoadTheme(AppTheme.Dark);
```

### Adding New Theme Colors

1. **Add to BOTH theme XAML files:**
   ```xml
   <!-- LightTheme.xaml -->
   <Color x:Key="NewColorName">#FFFFFF</Color>
   
   <!-- DarkTheme.xaml -->
   <Color x:Key="NewColorName">#000000</Color>
   ```

2. **Access in code** by searching merged dictionaries (see pattern above)

3. **For XAML controls**, use `{DynamicResource}`:
   ```xml
   <Label BackgroundColor="{DynamicResource NewColorName}" />
   ```

### Why NOT `SetDynamicResource` in C# Constructors?

`SetDynamicResource()` fails silently if the resource doesn't exist when called. Since themes are loaded dynamically after control construction, we manually apply colors in the `Loaded` event instead.

---

## 🚨 CRITICAL RULES - NEVER VIOLATE

### 0. Theme Colors Live in XAML

**THE GOLDEN RULE: Theme colors are defined in XAML files, not C# code.**

Theme colors are defined as `<Color x:Key="...">` entries in:
- `Sudoku.Maui/Resources/Styles/Themes/LightTheme.xaml`
- `Sudoku.Maui/Resources/Styles/Themes/DarkTheme.xaml`

When adding or changing colors, update **BOTH** files. Access them in XAML with `{DynamicResource ColorName}` or in code by searching `MergedDictionaries` (see Theme System section).

For any fallback colors in C# code (e.g., `CellHighlightManager.cs`), use `Colors.White`, `Colors.LightBlue`, etc. — never `Color.FromRgb()`. Keep fallbacks consistent with the theme values.

### 1. Theme Color Management

**THE GOLDEN RULE: Theme colors live in XAML files (LightTheme.xaml / DarkTheme.xaml).**

#### ✅ CORRECT Approach
```csharp
// When user wants to change a color, update BOTH XAML files:
// - Sudoku.Maui/Resources/Styles/Themes/LightTheme.xaml
// - Sudoku.Maui/Resources/Styles/Themes/DarkTheme.xaml

// Access them by searching merged dictionaries (see Theme System section above)
```

#### ❌ WRONG Approaches
```csharp
// ❌ DON'T inline hardcoded colors in code-behind
button.TextColor = Colors.Black; // NEVER DO THIS

// ❌ DON'T add colors only to App.xaml.cs
// Always update the XAML theme files

// ❌ DON'T use Application.Current.Resources.ContainsKey for theme colors
// Must search through MergedDictionaries!
```

### 2. Layout & Positioning

**THE GOLDEN RULE: The page uses a 3-row Grid layout. The Sudoku board uses `SquareLayoutControl` to stay square.**

#### ✅ CORRECT Layout Structure
- **Row 0 (Header):** Fixed-height header with difficulty label, timer, and icon buttons
- **Row 1 (Game Area):** Star-sized row containing `SquareLayoutControl` → `SudokuBoardControl`
- **Row 2 (Bottom Bar):** Action buttons (Hint/Check) + number pad rows

The `SquareLayoutControl` automatically maintains 1:1 aspect ratio and centers the grid within its parent. No manual centering math is needed.

#### ❌ WRONG Approaches
```csharp
// ❌ DON'T use AbsoluteLayout for the main page layout
// ❌ DON'T manually calculate grid centering — SquareLayoutControl handles it
// ❌ DON'T position action buttons to the right of the grid — they go below
// ❌ DON'T use TranslationX for grid positioning
```

### 3. API Usage

#### ✅ Always Use These:
- `DisplayAlertAsync` (NOT `DisplayAlert` - it's obsolete)
- `FadeToAsync`, `ScaleToAsync` (NOT `FadeTo`, `ScaleTo` - obsolete)
- `Border` (NOT `Frame` - Frame is obsolete in .NET 9+)
- `await` for all async operations
- `Color.FromArgb("#RRGGBB")` for theme colors (NOT `Color.FromRgb()`)

#### ❌ Never Use These:
- `DisplayAlert` - obsolete
- `FadeTo`, `ScaleTo` - obsolete  
- `Frame` - obsolete in .NET 9+
- `.Result` or `.Wait()` on async methods - causes deadlocks (except for settings save on app close)
- `Color.FromRgb()` - use `Color.FromArgb()` for consistency
- `Shadow="True"` or any boolean/string value for Shadow property

### 4. Warnings = Errors

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

- **Zero warnings tolerated**
- Fix ALL warnings before completing any code change
- If you introduce a warning, you MUST fix it immediately

---

## ℹ️ Layout & Sizing Rules

### Sizing Strategy

Layout is split into two independent systems (see `SudokuLayoutCalculator.cs`):

1. **Button/font sizes** — calculated from window dimensions, not grid size
2. **Grid size** — handled entirely by MAUI's star row + `SquareLayoutControl`
3. **Cell font sizes** — derived from the actual rendered grid size via `SizeChanged`

This avoids circular dependencies between grid and button sizing.

### Key Constants

```csharp
// Grid
MinGridSize = 360              // SquareLayoutControl minimum

// Button sizing (from window dimensions)
ButtonSize = Math.Clamp(Math.Min(fromWidth, fromHeight), 44, 100)
// where fromWidth = (windowWidth - 60) / 7.5
// where fromHeight = (windowHeight - 80) / 14.0

// Font sizes (proportional to button)
FontSize = ButtonSize * 0.4
CountFontSize = ButtonSize * 0.18

// Cell font (from rendered grid)
CellFontSize = Math.Max(10, (gridSize / 9.0) * 0.55)
```

### ❌ Common Sizing Mistakes

```csharp
// ❌ DON'T calculate button sizes from grid size (circular dependency)
// ❌ DON'T hardcode pixel values for buttons or fonts
// ❌ DON'T predict grid size — read it from SizeChanged events
```

---

## ℹ️ Debugging & Troubleshooting

### When an Issue Arises

**🚨 BEFORE suggesting solutions, consult:** `docs/TROUBLESHOOTING.md`

This file contains documented solutions for:
- Release build crashes
- XAML parsing errors
- Font/resource issues
- Build path differences (local vs CI/CD)
- Windows Event Viewer diagnostics

### Debug vs Release Behavior

**Key Differences:**
- Debug builds are lenient with XAML syntax errors
- Release builds enforce strict XAML validation
- `Debug.WriteLine` doesn't work in Release (use `Console.WriteLine`)
- Resource paths may differ
- Always test Release builds before deployment

