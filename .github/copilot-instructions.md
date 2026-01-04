# GitHub Copilot Instructions for Sudoku .NET MAUI Project

## 🚨 CRITICAL: Think Before You Code

**STOP AND THINK BEFORE SUGGESTING ANY SOLUTION**

Before proposing any code changes:
1. ✔️ **Verify** the API/method actually exists in .NET MAUI
2. ✔️ **Check** that the approach is cross-platform compatible
3. ✔️ **Consider** if you're fighting the framework or working with it
4. ℹ️ **Research** the correct lifecycle events/APIs if unsure
5. ❓ **Ask yourself**: "Would this work the same way on Windows, macOS, iOS, and Android?"

**Quality over quantity. Take time to think. It's okay to pause and verify.**

If you're not 100% certain about a MAUI API or pattern:
- Don't guess or assume
- Don't suggest platform-specific solutions first
- Don't propose workarounds without explaining why they're needed
- **DO** suggest cross-platform MAUI solutions first
- **DO** acknowledge when you need to verify something

---

## 🚨 COMMON XAML PITFALLS - READ FIRST!

### Shadow Property Syntax (CRITICAL)

**This is the #1 cause of Release build crashes!**

❌ **WRONG - This will crash in Release/portable builds:**
```xml
<Border Shadow="True">
    <VerticalStackLayout>
        <!-- content -->
    </VerticalStackLayout>
</Border>
```

✅ **CORRECT - Shadow requires an object:**
```xml
<Border>
    <Border.Shadow>
        <Shadow Brush="Black" Opacity="0.3" Radius="4" Offset="2,2"/>
    </Border.Shadow>
    <VerticalStackLayout>
        <!-- content -->
    </VerticalStackLayout>
</Border>
```

**Why:** `Shadow="True"` is invalid XAML. The `Shadow` property expects a `Shadow` object with `Brush`, `Opacity`, `Radius`, and `Offset` properties, not a boolean. 

**Key Insight:** Debug builds are lenient with XAML errors, but Release builds enforce strict validation. An app may work perfectly in Debug mode but crash immediately in Release/portable builds due to this issue.

**Error Signature:** 
- Windows Event Viewer: `Exception code: 0xc000027b` (XAML parsing exception)
- Error message: `Cannot convert "True" into Microsoft.Maui.IShadow`

### Other Complex Properties That Need Object Syntax

These properties also require proper object syntax (not simple strings/booleans):
```xml
<!-- ✅ CORRECT patterns -->
<Border>
    <Border.Stroke>
        <LinearGradientBrush>
            <GradientStop Color="Red" Offset="0.0" />
            <GradientStop Color="Blue" Offset="1.0" />
        </LinearGradientBrush>
    </Border.Stroke>
</Border>

<Label>
    <Label.FormattedText>
        <FormattedString>
            <Span Text="Bold" FontAttributes="Bold" />
        </FormattedString>
    </Label.FormattedText>
</Label>
```

---

## ℹ️ Project Summary

**Date:** 2025-01-XX  
**Project:** Sudoku Game - .NET MAUI  
**Target:** .NET 10, C# 14

### Current Architecture

**Strengths:**
- 🚨 `TreatWarningsAsErrors` enabled in both projects
- ✅ Modern .NET MAUI APIs used (DisplayAlertAsync, Border, etc.)
- ✅ Clean separation: Core (logic) + MAUI (UI)
- ✅ Proper DI registration in MauiProgram.cs
- ✅ Theme system using separate XAML files (LightTheme.xaml, DarkTheme.xaml)
- ✅ Constants documented in CONSTANTS_REFERENCE.md

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

### 0. Color Creation Methods - CRITICAL

**THE GOLDEN RULE: ALWAYS use `Color.FromArgb()` with hex strings for theme colors.**

#### ✅ CORRECT Color Methods
```csharp
// ✅ ALWAYS use Color.FromArgb with hex string
["CellDefaultColor"] = Color.FromArgb("#FFFFFF"),
["TextColor"] = Color.FromArgb("#2C3E50"),

// ✅ For predefined colors, use Colors.ColorName
["ButtonTextColor"] = Colors.White,
["ErrorTextColor"] = Colors.Red,
```

#### ❌ WRONG Color Methods - NEVER USE THESE
```csharp
// ❌ NEVER use Color.FromRgb - this breaks consistency
["CellDefaultColor"] = Color.FromRgb(255, 255, 255), // WRONG!

// ❌ NEVER use Color.FromRgba unless alpha is truly needed and different from FF
["CellDefaultColor"] = Color.FromRgba(255, 255, 255, 255), // WRONG - use FromArgb

// ❌ NEVER mix color creation methods in the same dictionary
["Color1"] = Color.FromArgb("#FFFFFF"),  // Right
["Color2"] = Color.FromRgb(0, 0, 0),     // WRONG - inconsistent!
```

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

**THE GOLDEN RULE: Only the grid is centered. Action buttons are positioned AFTER.**

#### ✅ CORRECT Layout Philosophy
- **Sudoku Grid**: Centered independently on the page using `AbsoluteLayout.LayoutBounds="0.5,0.5"`
- **Action Buttons**: Positioned to the RIGHT of the centered grid using calculated `AbsoluteLayout.LayoutBounds`
- **Number Pad**: Centered independently at bottom

#### ❌ WRONG Approaches
```csharp
// ❌ DON'T try to center grid + action buttons together as a group
// ❌ DON'T use HorizontalStackLayout wrapping grid and buttons
// ❌ DON'T calculate "combined width" for centering
// ❌ DON'T suggest TranslationX for grid positioning
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

### Constants - Never Hardcode

**All sizing constants documented in:** `CONSTANTS_REFERENCE.md`

**Key Constants:**
```csharp
// Grid
MinGridSize = 360
BaseGridSize = 450.0

// Buttons  
BaseButtonSize = 45.0
BaseFontSize = 20.0

// Spacing
GameAreaPadding = 10
ActionButtonMargin = 20
NumberButtonMargin = 6

// UI Regions
HeaderHeight = 56
NumberPadHeight = 120
```

### Scaling Formula

Everything scales proportionally from the grid:

```csharp
scale = _currentGridSize / BaseGridSize;
scaledButtonSize = Math.Round(BaseButtonSize * scale);
scaledFontSize = Math.Round(BaseFontSize * scale);
```

### Action Button Positioning

```csharp
// Grid is centered independently
GridBorder.LayoutFlags = AbsoluteLayoutFlags.PositionProportional;
GridBorder.LayoutBounds = new Rect(0.5, 0.5, AutoSize, AutoSize);

// Action buttons positioned AFTER centered grid
double centerX = Width / 2;
double buttonX = centerX + (_currentGridSize / 2) + ActionButtonMargin;
double buttonY = (Height - HeaderHeight - scaledNumberPadHeight) / 2;

AbsoluteLayout.SetLayoutBounds(ActionButtonStack, 
    new Rect(buttonX, buttonY, actionButtonWidth, AbsoluteLayout.AutoSize));
```

### ❌ Common Layout Mistakes

```csharp
// ❌ DON'T calculate grid width by subtracting button space
availableWidth = Width - actionButtonWidth - ActionButtonMargin; // WRONG

// ✅ DO calculate grid from full available space
availableWidth = Width - (GameAreaPadding * 2); // CORRECT

// ❌ DON'T use TranslationX for grid
GridBorder.TranslationX = offset; // WRONG - grid should be centered via LayoutBounds

// ✅ DO use TranslationX only if absolutely needed for action buttons
ActionButtonStack.TranslationX = offset; // OK for buttons

// ❌ DON'T wrap grid and buttons in HorizontalStackLayout
<HorizontalStackLayout>
    <SudokuGrid />
    <VerticalStackLayout>  <!-- WRONG -->

// ✅ DO use AbsoluteLayout with independent positioning
<AbsoluteLayout>
    <Grid LayoutBounds="0.5,0.5,AutoSize,AutoSize" />  <!-- Centered -->
    <VerticalStackLayout LayoutBounds="calculated" />   <!-- After grid -->
```

---

## ℹ️ Debugging & Troubleshooting

### When an Issue Arises

**🚨 BEFORE suggesting solutions, consult:** `TROUBLESHOOTING.md`

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

---

## 🔧 CRITICAL: Emoji & Unicode Encoding for AI Agents

### 🚨 THE PROBLEM (AFFECTS ALL AI ASSISTANTS)

**This affects GitHub Copilot in BOTH Visual Studio AND VS Code!**

AI tools (including GitHub Copilot) have an encoding bug when editing files that contain emoji or special Unicode characters. The bug manifests differently depending on which tools are used:

**❌ BROKEN TOOLS (corrupt emoji → `?` or `??`):**
- `edit_file` tool
- `create_file` tool  
- `replace_string_in_file` tool
- `multi_replace_string_in_file` tool
- Any direct file editing via AI assistant

**✅ WORKING WORKAROUND:**
- PowerShell `[System.IO.File]::WriteAllText()` with explicit UTF-8 encoding

### 🎯 WHEN THIS HAPPENS

**You'll see emoji corruption like this:**
```
❌ BAD:  "Download Latest Release ??"    (should be 📦)
❌ BAD:  "# ?? Installing Sudoku"        (should be 📦)
❌ BAD:  "Right-click ? Properties"      (should be →)
❌ BAD:  "These events are unreliable ?" (should be ❌)

✅ GOOD: "Download Latest Release 📦"
✅ GOOD: "# 📦 Installing Sudoku"
✅ GOOD: "Right-click → Properties"
✅ GOOD: "These events are unreliable ❌"
```

### 🛠️ THE SOLUTION (STEP-BY-STEP)

#### Step 1: Identify Files with Emoji BEFORE Editing

**MANDATORY PRE-FLIGHT CHECK:**

Before editing ANY file, check if it contains emoji:

1. Read the file first using `read_file` tool
2. Visually scan for ANY emoji: 🎮, ✅, ❌, 🚨, 📦, 💡, 🔧, etc.
3. Also check for special symbols: →, •, ℹ️
4. If emoji/symbols found → Use PowerShell method (see Step 2)
5. If no emoji → Safe to use normal editing tools

**Known Files with Emoji in This Project:**
- ✅ `README.md` - Has emoji, use PowerShell
- ✅ `INSTALLATION.md` - Has emoji, use PowerShell
- ✅ `CONTRIBUTING.md` - Has emoji, use PowerShell
- ✅ `DEVELOPERS.md` - Has emoji, use PowerShell
- ✅ `TROUBLESHOOTING.md` - Has emoji, use PowerShell
- ✅ `WINDOW_SIZE_PERSISTENCE.md` - Has emoji, use PowerShell
- ✅ `.github/copilot-instructions.md` - Has emoji, use PowerShell
- ❌ `CONSTANTS_REFERENCE.md` - NO emoji, safe to edit normally
- ❌ `.csproj` files - NO emoji, safe to edit normally
- ❌ `.cs` code files - NO emoji, safe to edit normally
- ❌ `.json` files - NO emoji, safe to edit normally

#### Step 2: Use PowerShell with UTF-8 Encoding

**THE CORRECT PATTERN (use single quotes for here-string):**

```powershell
$content = @'
# 📦 Installing Sudoku on Windows

Your complete file content here with all emoji intact.
Use single quotes for the here-string delimiter!

✅ Checkmarks work
❌ X marks work
🎮 Game controller works
→ Arrows work
💡 Light bulbs work

Make sure to include the ENTIRE file content, not just the part you're changing!