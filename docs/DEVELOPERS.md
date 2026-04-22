# Developer Documentation

## 🏗️ Architecture Overview

A clean, maintainable Sudoku game with strict separation between game logic, application services, and UI presentation. Three layered projects: `Sudoku.Core` → `Sudoku.Application` → `Sudoku.Maui`.

```
Solution Structure:
📦 Sudoku.Core (Class Library — no UI / no MAUI dependencies)
│   ├── Models/
│   │   ├── SudokuCell.cs         - Individual cell with candidate tracking
│   │   └── SudokuBoard.cs        - 9x9 board with serialization
│   ├── Services/
│   │   ├── SudokuGenerator.cs           - Puzzle generation with DifficultyLevel enum
│   │   ├── SudokuValidator.cs           - Move validation and error detection
│   │   ├── SudokuBacktrackingSolver.cs  - Backtracking solver for validation/hints
│   │   ├── SudokuLogicalSolver.cs       - Human-solvable technique solver
│   │   └── SolveResult.cs               - Difficulty analysis results
│   └── Strategies/               - 11 solving strategy implementations
│       ├── ISolvingStrategy.cs   - Strategy interface
│       ├── StrategyResult.cs     - Strategy output model
│       ├── StrategyCategory.cs   - Basic/Tough/Diabolical categorization
│       ├── Basic/                - Naked/Hidden Singles, Pairs, Triples, Pointing Pairs, Box-Line
│       ├── Tough/                - X-Wing, Y-Wing, Swordfish
│       └── Diabolical/           - XY-Chain

📦 Sudoku.Application (Class Library — no UI / no MAUI dependencies)
│   ├── Models/
│   │   ├── GamePhase.cs          - Lifecycle enum: NotStarted/Generating/Playing/Completed
│   │   ├── GameSettings.cs       - Persisted settings (theme, last difficulty, window state)
│   │   ├── GameState.cs          - Snapshot used for auto-save / restore
│   │   └── GameStatistics.cs     - Best solve times per DifficultyLevel
│   └── Services/
│       ├── IGameSession.cs       - Singleton game session contract
│       ├── GameSession.cs        - Owns board, solution, timer, phase, statistics
│       ├── GameSessionResults.cs - Discriminated outcomes (placement/hint/check)
│       ├── PuzzleSolvedEventArgs.cs
│       ├── ISettingsService.cs / IGameStateService.cs - Persistence interfaces

📦 Sudoku.Maui (MAUI App — UI + platform-specific persistence)
│   ├── MauiProgram.cs            - DI registration (singletons for services, transient for pages)
│   ├── Pages/
│   │   ├── SudokuPage.xaml(.cs)  - Thin adapter: rendering, animations, keyboard, alerts
│   │   └── SettingsPage.xaml(.cs)
│   ├── Services/
│   │   ├── SettingsService.cs    - JSON-based settings persistence
│   │   └── GameStateService.cs   - Auto-save implementation
│   ├── Controls/
│   │   ├── SquareLayoutControl.cs       - Maintains 1:1 aspect ratio for the board
│   │   ├── SudokuBoardControl.cs        - 9×9 grid renderer
│   │   ├── NumberPadButton.xaml(.cs)    - Circular number button with remaining count
│   │   ├── DifficultySelectionOverlay.xaml(.cs)
│   │   └── GameSummaryOverlay.xaml(.cs)
│   ├── Helpers/
│   │   ├── SudokuLayoutCalculator.cs    - Button/font sizing from window dimensions
│   │   └── CellHighlightManager.cs      - Cell selection / highlight / error coloring
│   └── Resources/
│       ├── Fonts/                - FontAwesome icons
│       └── Styles/Themes/        - LightTheme.xaml, DarkTheme.xaml

📦 Sudoku.Core.Tests (xUnit)
│   ├── Models/                   - Board / cell behavior
│   ├── Services/                 - Solver, generator, validator
│   └── Strategies/               - One folder per category

📦 Sudoku.Application.Tests (xUnit)
│   ├── Models/                   - GameState behavior
│   └── Services/
│       └── GameSessionTests.cs   - Authorization, restore, placement, hint, check, restart

📄 SudokuGame.slnx                 - Solution file (XML format, no GUIDs)
📄 .github/copilot-instructions.md - Comprehensive guidelines for AI agents
📄 docs/CONSTANTS_REFERENCE.md     - Sizing constants used by SudokuLayoutCalculator
📄 docs/DIFFICULTY_ALGORITHM_RESEARCH.md - Strategy research and scoring
📄 CHANGELOG.md                    - Version history and release notes
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
   start SudokuGame.slnx
   
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

**All sizing constants documented in:** [CONSTANTS_REFERENCE.md](CONSTANTS_REFERENCE.md). Implementation lives in `Sudoku.Maui/Helpers/SudokuLayoutCalculator.cs`.

### Layout Philosophy

Layout is split into two independent systems to avoid circular dependencies between grid and buttons:

1. **Button & font sizes** — derived from **window dimensions**, clamped to a sensible range.
2. **Grid size** — handled entirely by MAUI's star row plus `SquareLayoutControl`, which enforces a 1:1 aspect ratio.
3. **Cell font sizes** — derived from the **actual rendered grid size** via `SizeChanged`, not predicted.

### Page Structure (3-row Grid)

- **Row 0 — Header**: difficulty label, timer, icon buttons (fixed height).
- **Row 1 — Game Area**: star-sized; contains `SquareLayoutControl` → `SudokuBoardControl`.
- **Row 2 — Bottom Bar**: action buttons (Hint/Check) plus number pad rows.

### Key Formulas

```csharp
// Button size: clamped, derived from both width and height
double fromWidth  = (windowWidth  - 60) / 7.5;
double fromHeight = (windowHeight - 80) / 14.0;
ButtonSize = Math.Clamp(Math.Min(fromWidth, fromHeight), 44, 100);

// Font sizes proportional to button
FontSize      = ButtonSize * 0.4;
CountFontSize = ButtonSize * 0.18;

// Cell font from rendered grid
CellFontSize = Math.Max(10, (gridSize / 9.0) * 0.55);
```

---

## 🧩 Solving Strategies System

### Overview

The game uses **technique-based difficulty rating** powered by `SudokuLogicalSolver` with 11 human-solvable strategies.

### Strategy Categories

Strategies are organized by difficulty and applied in order:

#### **Basic Strategies (Score: 5-50)**
1. **Naked Single** (5) - Cell has only one candidate
2. **Hidden Single** (10) - Number can only go in one cell within unit
3. **Pointing Pair** (25) - Box candidates force row/column eliminations
4. **Box-Line Reduction** (25) - Row/column candidates force box eliminations
5. **Naked Pair** (30) - Two cells with same 2 candidates
6. **Hidden Pair** (35) - Two numbers locked to 2 cells
7. **Naked Triple** (40) - Three cells with same 3 candidates

#### **Tough Strategies (Score: 100-150)**
8. **X-Wing** (100) - 2×2 rectangle pattern across rows/columns
9. **Y-Wing** (130) - XY-Wing pattern with 3 cells
10. **Swordfish** (140) - 3×3 pattern across rows/columns

#### **Diabolical Strategies (Score: 240+)**
11. **XY-Chain** (240) - Bivalue cell chains

### How Difficulty Rating Works

1. **Puzzle Generation**:
   - `SudokuGenerator` creates a complete valid board
   - Removes cells one-by-one until target criteria met
   - **PRIMARY:** Clue count reaches industry-standard range (e.g., Easy: 36-46 givens)
   - **SECONDARY:** Difficulty score validated with `SudokuLogicalSolver`
   - Maintains unique solution throughout removal process

2. **Difficulty Scoring**:
   ```csharp
   PRIMARY - Clue Count Ranges:
   Easy:   36-46 given clues (35-45 empty cells)
   Medium: 32-35 given clues (46-49 empty cells)
   Hard:   28-31 given clues (50-53 empty cells)
   Expert: 24-27 given clues (54-57 empty cells)
   Evil:   22-25 given clues (56-59 empty cells)
   
   SECONDARY - Strategy Score Validation:
   Easy:   Target score ~50   (Basic strategies only)
   Medium: Target score ~200  (Basic + X-Wing/Y-Wing)
   Hard:   Target score ~350  (Add Swordfish)
   Expert: Target score ~500  (Add XY-Chain)
   Evil:   Target score ~700  (Multiple advanced strategies)
   ```

3. **Score Calculation**:
   - Each strategy usage adds its difficulty points
   - Total score = Σ(Strategy Score × Usage Count)
   - Solver mimics human approach: restarts from easiest strategy after each placement

### Adding New Strategies

1. **Create strategy class** in appropriate folder:
   ```csharp
   public class MyStrategy : ISolvingStrategy
   {
       public string Name => "My Strategy";
       public int DifficultyScore => 150;
       public StrategyCategory Category => StrategyCategory.Tough;
       
       public StrategyResult? Apply(SudokuBoard board)
       {
           // Implementation
       }
   }
   ```

2. **Register in SudokuLogicalSolver**:
   ```csharp
   _strategies = new List<ISolvingStrategy>
   {
       // ... existing strategies
       new MyStrategy(),  // Add in difficulty order
   };
   ```

3. **Add tests** in `Sudoku.Core.Tests/Strategies/`

### Testing Your Strategies

Run all strategy tests:
```bash
dotnet test Sudoku.Core.Tests
```

Test specific strategy:
```bash
dotnet test --filter FullyQualifiedName~XYChainStrategyTests
```

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

// ❌ NEVER - Magic numbers / hardcoded sizes
Width = 450;                                    // Use SudokuLayoutCalculator
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

// ✅ CORRECT - Sizing via the layout calculator
var metrics = SudokuLayoutCalculator.Calculate(window.Width, window.Height);
button.WidthRequest = metrics.ButtonSize;
```

---

## 🧪 Testing

### Current Test Coverage

The project includes comprehensive unit tests using **xUnit**, split across two test projects:

```
Sudoku.Core.Tests/
├── Models/                       - Board / cell behavior
├── Services/
│   ├── SudokuBacktrackingSolverTests.cs
│   ├── SudokuLogicalSolverTests.cs
│   ├── SudokuValidatorTests.cs
│   └── SudokuGeneratorTests.cs
└── Strategies/
    ├── Basic/                    - Naked/Hidden Singles, Pairs, Triples, Pointing Pairs, Box-Line
    ├── Tough/                    - X-Wing, Y-Wing, Swordfish
    └── Diabolical/               - XY-Chain

Sudoku.Application.Tests/
├── Models/
│   └── GameStateTests.cs
└── Services/
    └── GameSessionTests.cs       - Phase authorization, restore, placement,
                                     hint, check, restart, save persistence
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter FullyQualifiedName~SwordfishStrategyTests

# Run tests for a specific strategy category
dotnet test --filter FullyQualifiedName~Tough
```

### Test Patterns

All strategy tests follow consistent patterns:

```csharp
[Fact]
public void Apply_ShouldDetectPattern_WhenValidPatternExists()
{
    // Arrange - Set up board with known pattern
    var board = new SudokuBoard();
    // ... set up test case
    
    // Act - Apply strategy
    var result = _strategy.Apply(board);
    
    // Assert - Verify expected eliminations
    Assert.NotNull(result);
    Assert.True(result.HasChanges);
    Assert.Contains(result.RemovedCandidates, 
        rc => rc.Row == expectedRow && rc.Col == expectedCol);
}
```

### UI Testing (Future)

Planned: Cross-platform UI testing with Appium or similar.

---

## 🚧 Known Limitations & Roadmap

### ✅ Implemented Features

- ✅ **Technique-Based Difficulty Rating** - 11 solving strategies with accurate scoring
- ✅ **Auto-Save** - Game state persistence between sessions
- ✅ **Comprehensive Testing** - Unit tests for all strategies
- ✅ **Theme System** - Light/Dark themes with XAML ResourceDictionaries
- ✅ **Responsive Layout** - Adaptive grid scaling for all screen sizes
- ✅ **Statistics Tracking** - Best solve times per difficulty
- ✅ **Backend Candidate Tracking** - Full pencil mark support in SudokuCell

### 🚧 In Progress / Planned

1. **🎨 Pencil Marks UI (HIGH PRIORITY)**
   - **Status**: Backend fully implemented, UI missing
   - **Backend Ready**: `SudokuCell.Candidates` with Add/Remove/Clear methods
   - **Needed**: 
     - UI toggle button to switch between value/candidate entry mode
     - Visual display of multiple candidates per cell (small numbers)
     - Touch/click interface for candidate selection
   - **Design Consideration**: Keep UI clean - don't overwhelm on small screens

2. **📱 Platform Support**
   - **Status**: Windows ✅, Android/iOS/macOS 🚧
   - **Blocker**: Build/deploy configuration needed
   - **Priority**: Medium (core game works cross-platform)

3. **🔢 Additional Strategies (NICE TO HAVE)**
   - Hidden Triples, Naked Quads
   - Finned Fish (X-Wing, Swordfish)
   - Unique Rectangles
   - More chain strategies
   - **Note**: Current 11 strategies handle most published puzzles

4. **🧪 UI Testing**
   - Appium or similar framework
   - Cross-platform test automation

### ❌ Not Planned

- Ads, tracking, monetization
- Online multiplayer
- Social features
- Puzzle sharing (keeps game simple and private)

### 🔍 Performance Notes

**Current Performance**: Excellent for game purposes
- Puzzle generation: <1s for Easy/Medium, 1-3s for Expert/Evil
- Strategy application: Near-instant
- Grid rendering: Smooth 60fps

**No optimization needed** unless:
- Generation takes >5s consistently
- UI becomes sluggish on target devices
- Memory usage becomes problematic on mobile

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

1. **[.github/copilot-instructions.md](../.github/copilot-instructions.md)** - Comprehensive guidelines
   - Critical rules (APIs, warnings, cross-platform)
   - Theme system deep-dive
   - Layout patterns
   - Common tasks

2. **[CONSTANTS_REFERENCE.md](CONSTANTS_REFERENCE.md)** - All constants
   - Sizing values
   - Spacing rules
   - Scaling formulas

3. **[DIFFICULTY_ALGORITHM_RESEARCH.md](DIFFICULTY_ALGORITHM_RESEARCH.md)** - Strategy research
   - Strategy classifications and scores
   - Difficulty tier definitions
   - Implementation notes

---

## 🤝 Contributing

See [../CONTRIBUTING.md](../CONTRIBUTING.md) for:
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
- **� Future-Proof**: Microsoft's current focus

### Why a `GameSession` Singleton Instead of Full MVVM?

- **🧪 Testability**: All gameplay rules and lifecycle live in `Sudoku.Application` and are unit-tested without MAUI.
- **🔒 Single source of truth**: `GamePhase` + `CanEditBoard` / `CanUseGameActions` make input authorization impossible to bypass.
- **⚡ Performance**: The 9×9 grid hot path performed poorly with per-cell data bindings; the page subscribes to plain C# events instead.
- **🪶 Simplicity**: No view-model layer or MVVM toolkit dependency to maintain.

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
| System.Text.Json | Built-in | Settings serialization |
| xUnit | 2.9.x | Unit testing framework |
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
