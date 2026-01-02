# Developer Documentation

## 🏗️ Architecture Overview

A clean, maintainable Sudoku game with strict separation between game logic and UI presentation.

```
Solution Structure:
📦 Sudoku.Core (Class Library)
│   ├── Models/
│   │   ├── SudokuCell.cs         - Individual cell representation
│   │   └── SudokuBoard.cs        - 9x9 board with serialization
│   └── Services/
│       ├── SudokuGenerator.cs    - Puzzle generation with difficulty levels
│       ├── SudokuValidator.cs    - Move validation and error detection
│       └── SudokuSolver.cs       - Backtracking solver with hints

📦 Sudoku.Maui (MAUI App)
│   ├── Pages/
│   │   ├── SudokuPage.xaml(.cs)  - Main game UI with responsive grid
│   │   └── SettingsPage.xaml(.cs) - Settings configuration UI
│   └── Services/
│   │   ├── ISettingsService.cs   - Settings interface
│   │   └── SettingsService.cs    - JSON-based settings persistence
│   ├── Models/
│   │   └── GameSettings.cs       - Settings data model
│   └── Resources/
│       ├── Fonts/                - FontAwesome icons
│       └── Styles/               - Theme colors and styles

📄 AI_INSTRUCTIONS.md             - Comprehensive guidelines for AI agents
📄 CONSTANTS_REFERENCE.md         - All sizing and spacing constants
```

---

## 🚀 Getting Started

### Prerequisites

- **Visual Studio 2022** (17.8 or later) or **Visual Studio Code**
- **.NET 10 SDK**
- **Workload**: .NET Multi-platform App UI

### Building and Running

1. **Clone the repository**
   ```bash
   git clone https://github.com/jtmunn/SudokuGame.git
   cd SudokuGame
   ```

2. **Open Solution**
   ```bash
   # Visual Studio
   start SudokuGame.sln
   
   # VS Code
   code .
   ```

3. **Select Target Platform**
   - Windows
   - Android
   - iOS
   - macCatalyst

4. **Run**
   - Press **F5** or click **Run**

### Building for Release

```bash
# Windows
dotnet publish Sudoku.Maui/Sudoku.Maui.csproj -f net10.0-windows10.0.19041.0 -c Release

# Android
dotnet publish Sudoku.Maui/Sudoku.Maui.csproj -f net10.0-android -c Release

# iOS
dotnet publish Sudoku.Maui/Sudoku.Maui.csproj -f net10.0-ios -c Release

# macCatalyst
dotnet publish Sudoku.Maui/Sudoku.Maui.csproj -f net10.0-maccatalyst -c Release
```

---

## 🎨 Theme System

### How Themes Work

Themes are defined in separate XAML ResourceDictionary files:
- `Resources/Styles/Themes/LightTheme.xaml` + `.cs`
- `Resources/Styles/Themes/DarkTheme.xaml` + `.cs`

**Theme Loading Pattern:**
1. Theme classes are instantiated: `new LightTheme()` or `new DarkTheme()`
2. Added to `Application.Current.Resources.MergedDictionaries`
3. Controls access theme colors by searching through merged dictionaries

**⚠️ Important**: Theme colors are NOT directly in `Application.Current.Resources`. They live in merged dictionaries.

### Adding New Theme Colors

1. **Add to BOTH theme XAML files:**
   ```xml
   <!-- LightTheme.xaml -->
   <Color x:Key="NewColorName">#FFFFFF</Color>
   
   <!-- DarkTheme.xaml -->
   <Color x:Key="NewColorName">#000000</Color>
   ```

2. **Access in XAML** using `{DynamicResource}`:
   ```xml
   <Label BackgroundColor="{DynamicResource NewColorName}" />
   ```

3. **Access in C#** by searching merged dictionaries:
   ```csharp
   foreach (var dict in Application.Current.Resources.MergedDictionaries)
   {
       if (dict.ContainsKey("NewColorName"))
           color = (Color)dict["NewColorName"];
   }
   ```

---

## 📐 Layout & Sizing System

### Constants

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

Everything scales proportionally:

```csharp
scale = _currentGridSize / BaseGridSize;
scaledButtonSize = Math.Round(BaseButtonSize * scale);
scaledFontSize = Math.Round(BaseFontSize * scale);
```

### Layout Philosophy

- **Sudoku Grid**: Centered independently using `AbsoluteLayout.LayoutBounds="0.5,0.5"`
- **Action Buttons**: Positioned to the RIGHT of centered grid
- **Number Pad**: Centered independently at bottom

---

## ✅ Code Quality Standards

This project enforces **strict code quality**:

### ✅ Enabled Rules

- **⚠️ TreatWarningsAsErrors**: Zero tolerance for warnings
- **❗ Nullable Reference Types**: All nullability must be explicit
- **✅ Modern Async APIs**: Always use `DisplayAlertAsync`, `FadeToAsync`, etc.
- **🚫 No Obsolete APIs**: No `Frame`, `DisplayAlert`, `FadeTo`, etc.

### ❌ Forbidden Patterns

```csharp
// ❌ NEVER - Obsolete APIs
await DisplayAlert("Title", "Message", "OK");  // Use DisplayAlertAsync
await element.FadeTo(0);                        // Use FadeToAsync
<Frame>...</Frame>                              // Use Border

// ❌ NEVER - Blocking async
var result = SomeAsyncMethod().Result;          // Use await
SomeAsyncMethod().Wait();                       // Use await

// ❌ NEVER - Hardcoded colors
button.TextColor = Colors.Black;                // Use theme resources

// ❌ NEVER - Magic numbers
Width = 450;                                    // Use constants
```

### ✅ Correct Patterns

```csharp
// ✅ CORRECT - Modern async
await DisplayAlertAsync("Title", "Message", "OK");
await element.FadeToAsync(0);

// ✅ CORRECT - Theme resources
foreach (var dict in Application.Current.Resources.MergedDictionaries)
{
    if (dict.ContainsKey("ButtonTextColor"))
        button.TextColor = (Color)dict["ButtonTextColor"];
}

// ✅ CORRECT - Constants
private const double BaseGridSize = 450.0;
Width = BaseGridSize;
```

---

## 🧪 Testing

### Unit Tests (Planned)

```bash
# Add xUnit project
dotnet new xunit -o Sudoku.Core.Tests
dotnet add Sudoku.Core.Tests reference Sudoku.Core

# Run tests
dotnet test
```

### UI Tests (Planned)

Using Appium or similar for cross-platform UI testing.

---

## ⚠️ Technical Debt & Known Issues

### Current Limitations

1. **⚠️ Difficulty Rating is Inaccurate (HIGH PRIORITY)**
   - **Current State**: Difficulty based only on number of clues (cells filled)
   - **Problem**: True difficulty depends on **which solving techniques are required**, not clue count
   - **Impact**: Even "Hard" puzzles may feel easy if clue placement allows simple techniques
   - **Solution**: Implement technique-based difficulty rating
   
   **Research Summary:**
   
   Sudoku difficulty should be rated by the most advanced solving technique required:
   
   **Solving Techniques (Easiest → Hardest):**
   1. Naked Singles - Cell has only one possible value
   2. Hidden Singles - Number can only go in one cell within row/column/box
   3. Naked Pairs/Triples - Cells share same candidates, eliminating from others
   4. Hidden Pairs/Triples - Numbers restricted to specific cells
   5. Pointing Pairs/Box-Line Reduction - Box candidates force row/column eliminations
   6. X-Wing - Rectangle pattern eliminations
   7. Swordfish - 3x3 pattern variant
   8. XY-Wing - Three-cell chains
   9. Advanced Chains - Complex logical chains (expert level)
   
   **Implementation Approach:**
   - After generating puzzle, attempt to solve using techniques in order
   - Track which techniques are needed to make progress
   - Rate puzzle by most advanced technique required
   - Regenerate if puzzle doesn't match target difficulty
   - **References:**
     - SudokuWiki.org - Comprehensive technique explanations
     - "Sudoku Explainer" - Open-source Java project with rating algorithm
     - Donald Knuth's "Dancing Links" (Algorithm X)

2. **Simple Solver**: Uses only basic backtracking
   - *Reason*: Advanced techniques not yet needed for hint generation
   - *Solution*: Will be required for technique-based difficulty rating

3. **Limited Testing**: No unit tests yet
   - *Solution*: Add xUnit project for Core services

---

## 🔄 CI/CD

### GitHub Actions Workflow

The project uses GitHub Actions for automated builds and releases.

#### Windows MSIX Build

**File:** `.github/workflows/build-windows.yml`

**Triggers:**
- Push to `main` branch
- Pull requests to `main` branch
- Git tags matching `v*.*.*` pattern
- Manual workflow dispatch

**What it does:**
1. ✅ Sets up .NET 10 SDK
2. ✅ Installs .NET MAUI workload
3. ✅ Restores dependencies
4. ✅ Builds Windows MSIX package (x64)
5. ✅ Uploads MSIX as artifact (30-day retention)
6. ✅ Creates GitHub Release on version tags

**Build Configuration:**
```yaml
Platform: x64
Configuration: Release
Target: net10.0-windows10.0.19041.0
Package Type: MSIX (unsigned for development)
```

**Artifacts:**
- MSIX package available for download after successful build
- Automatic releases created for version tags (`v1.0.0`, etc.)

#### Future Planned Workflows

- **Android APK/AAB Build**: Automated Android package generation
- **iOS IPA Build**: macOS runner for iOS builds (requires certificates)
- **Unit Tests**: Run xUnit tests on every push/PR
- **Code Quality**: Static analysis and linting

### Manual Build Commands

See "Building for Release" section above for platform-specific build commands.

````````markdown
## 📚 Key Files for AI Agents

**⚠️ Important**: Before making any code changes, read:

1. **[AI_INSTRUCTIONS.md](AI_INSTRUCTIONS.md)** - Comprehensive guidelines
   - Critical rules (APIs, warnings, cross-platform)
   - Theme system deep-dive
   - Layout patterns
   - Common tasks

2. **[CONSTANTS_REFERENCE.md](CONSTANTS_REFERENCE.md)** - All constants
   - Sizing values
   - Spacing rules
   - Scaling formulas

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code style guidelines
- Pull request process
- Issue templates
- Development workflow

---

## 🤔 Architecture Decisions

### Why Separate Core and MAUI Projects?

- **✅ Testability**: Core logic can be unit tested without UI
- **🔄 Reusability**: Same Core can power Blazor, Console, or other UIs
- **🛠️ Maintainability**: Clear separation of concerns
- **🌐 Platform Agnostic**: Core has zero UI dependencies

### Why MAUI Over Xamarin.Forms?

- **✨ Modern**: .NET 10, C# 14, latest features
- **⚡ Performance**: Better rendering and startup time
- **📦 Single Project**: Simplified project structure
- **🚀 Future-Proof**: Microsoft's current focus

### Why JSON Over SQLite for Settings?

- **✅ Simplicity**: Settings are small and infrequent
- **💾 Portability**: Easy to backup/restore
- **🎯 No Dependencies**: No need for SQLite libraries
- *(SQLite may be added for game history later)*

---

## 📖 Additional Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [C# 14 Features](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [FontAwesome Icons](https://fontawesome.com/icons)
- [Sudoku Solving Techniques](https://www.sudokuwiki.org/sudoku.htm)

---

## 🛠️ Technology Stack Details

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10 | Runtime |
| .NET MAUI | Latest | Cross-platform UI framework |
| C# | 14 | Programming language |
| CommunityToolkit.Mvvm | Latest | MVVM utilities |
| System.Text.Json | Built-in | Settings serialization |
| FontAwesome Free | 6.5.1 | Icon font |

---

## 📚 Learning Resources

Building a similar app? Check out:
- [MAUI Tutorial](https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app)
- [Backtracking Algorithms](https://en.wikipedia.org/wiki/Backtracking)
- [Cross-Platform Design Patterns](https://learn.microsoft.com/en-us/dotnet/architecture/maui/)

---

## 💬 Support

- **Issues**: [GitHub Issues](https://github.com/jtmunn/SudokuGame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jtmunn/SudokuGame/discussions)
- **Pull Requests**: Always welcome!

---

<div align="center">

**Happy Coding!** 🚀

</div>
