# Sudoku - .NET MAUI

A cross-platform Sudoku game built with .NET MAUI and .NET 10, featuring a clean separation between game logic (Core) and presentation layer (MAUI), with a focus on maximizing game grid visibility and responsive design.

## Current Features

### Implemented ?

- **9x9 Sudoku Grid**: Interactive interface with responsive scaling
  - Grid scales dynamically based on window size (minimum 360px, unlimited maximum)
  - Font sizes scale proportionally (16-60px range) for readability on all displays
  - Thick borders around 3x3 blocks for clear visual separation
  - Optimized for TV viewing with large, readable numbers
- **Puzzle Generation**: Backtracking algorithm to generate valid puzzles (currently using hardcoded puzzle for reliability)
- **Input Validation**: Real-time validation of moves with conflict detection
- **Error Highlighting**: Visual feedback for invalid entries (red cells)
- **Smart Cell Selection**: 
  - All cells clickable (including given/read-only cells)
  - Highlights selected cell prominently
  - Highlights matching numbers across entire grid
  - Highlights row and column (no 3x3 box highlighting)
  - Given cells show as read-only when selected
- **Hint System**: Get hints for next valid move with visual animation
- **Check Progress**: Validate current state with modal alerts
- **Clear Function**: Clear individual cells or all user entries (with confirmation)
- **New Game**: Start fresh game with confirmation dialog
- **Win Detection**: Automatic detection when puzzle is solved
- **Visual Effects**: 
  - Fade-in animations for number placement
  - Scale animation for hints
  - Color-coded cells (given, selected, error, matching, highlighted)
  - Smooth transitions and responsive feedback
- **Sound Effects** (with Plugin.Maui.Audio):
  - Correct/incorrect move sounds
  - Puzzle completion sound
  - Hint sound
  - Cell selection sound
- **Settings System**:
  - JSON-based persistence (automatically saves to app data directory)
  - Light/Dark theme toggle with instant switching
  - Default difficulty selection (Easy/Medium/Hard)
  - Show/Hide Hint button toggle
  - Show/Hide Check button toggle
- **Compact UI**:
  - Icon-based buttons (FontAwesome) for maximum grid space
  - Minimal header with icon buttons
  - No status text (uses modal alerts instead)
  - Optimized for both small (mobile) and large (TV) screens
- **Cross-Platform Theme Support**:
  - Light and Dark modes with separate theme files (LightTheme.xaml, DarkTheme.xaml)
  - Theme preference persisted across sessions
  - Automatic theme application on app start
  - Theme-aware colors throughout UI
  - **Note**: Themes use MAUI ResourceDictionary pattern with MergedDictionaries for runtime switching

### Architecture

```
Solution Structure:
??? Sudoku.Core (Class Library)
?   ??? Models/
?   ?   ??? SudokuCell.cs         - Individual cell representation
?   ?   ??? SudokuBoard.cs        - 9x9 board with serialization
?   ??? Services/
?       ??? SudokuGenerator.cs    - Puzzle generation with difficulty levels
?       ??? SudokuValidator.cs    - Move validation and error detection
?       ??? SudokuSolver.cs       - Backtracking solver with hints
?
??? Sudoku.Maui (MAUI App)
?   ??? Pages/
?   ?   ??? SudokuPage.xaml(.cs)  - Main game UI with responsive grid
?   ?   ??? SettingsPage.xaml(.cs) - Settings configuration UI
?   ??? Services/
?   ?   ??? SoundService.cs       - Audio playback wrapper
?   ?   ??? ISettingsService.cs   - Settings interface
?   ?   ??? SettingsService.cs    - JSON-based settings persistence
?   ??? Models/
?   ?   ??? GameSettings.cs       - Settings data model
?   ??? Resources/
?   ?   ??? Raw/Sounds/           - Sound effect files (placeholder)
?   ?   ??? Fonts/                - FontAwesome icons
?   ?   ??? Styles/               - Theme colors and styles
?   ??? FontAwesomeIcons.cs       - Icon constant definitions
?
??? AI_INSTRUCTIONS.md             - Comprehensive guidelines for AI agents
```

## Getting Started

### Prerequisites

- Visual Studio 2022 (17.8 or later) or Visual Studio Code
- .NET 10 SDK
- Workload: .NET Multi-platform App UI

### Building and Running

1. Open `SudokuGame.sln` in Visual Studio
2. Select target platform (Windows, Android, iOS, macCatalyst)
3. Press F5 or click Run

### Adding FontAwesome Icons

FontAwesome is used for icon buttons:

1. Download `fa-solid-900.ttf` from:
   - https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/webfonts/fa-solid-900.ttf
2. Place in `Sudoku.Maui/Resources/Fonts/`
3. Font is already registered in `MauiProgram.cs`

### Adding Sound Files

To enable sound effects:

1. Place MP3 or WAV files in `Sudoku.Maui/Resources/Raw/Sounds/`
2. Required files:
   - `correct.mp3` - Played on correct input
   - `error.mp3` - Played on invalid input
   - `complete.mp3` - Played when puzzle is solved
   - `hint.mp3` - Played when hint is shown
   - `select.mp3` - Played on cell selection

## How to Play

1. **Start**: A puzzle is loaded automatically
2. **Select Cell**: Tap/click any cell (including given/read-only cells to see matching numbers)
3. **Enter Number**: Use the number pad at the bottom (1-9)
4. **Icon Buttons**:
   - **+** (Plus): New Game (with confirmation)
   - **?** (Gear): Open Settings
   - **??** (Lightbulb): Get Hint
   - **?** (Check): Validate puzzle
5. **Settings**: Configure theme, difficulty, and button visibility
6. **Win**: Puzzle automatically detected when solved

## Code Quality Standards

This project enforces strict code quality:

- ? **Warnings treated as errors** - Zero tolerance for warnings
- ? **Modern C# patterns** - Uses C# 14 features, async/await, nullable references
- ? **Obsolete API avoidance** - Uses latest MAUI APIs (DisplayAlertAsync, FadeToAsync, Border, etc.)
- ? **Cross-platform compatibility** - Works on Windows, iOS, Android, macOS
- ? **AI Instructions** - Comprehensive guidelines in `AI_INSTRUCTIONS.md`

## Recent Updates (2025-12-29)

### ? Implemented Features
- **Settings System**: JSON persistence with Light/Dark theme, difficulty selection, button visibility toggles
- **Responsive Grid**: Dynamic scaling for mobile to TV displays with proportional font sizes
- **Compact UI**: Icon-based interface maximizing game grid space
- **Enhanced Highlighting**: Clickable given cells, matching number highlighting, simplified row/column focus
- **Theme Support**: Full Light/Dark mode with persisted preferences
- **Code Quality**: TreatWarningsAsErrors enabled, obsolete APIs replaced
- **Documentation**: Comprehensive AI_INSTRUCTIONS.md for future development

### Phase 2 Progress: Persistence & Settings ?

- [x] **Settings Page**:
  - [x] Theme selection (light/dark)
  - [x] Default difficulty selection
  - [x] Show/Hide Hint button
  - [x] Show/Hide Check button
- [x] **Settings Persistence**: JSON-based with auto-save
- [ ] **Save/Load Game**: Serialize board state (future)
- [ ] **Auto-save**: Save progress automatically on app close (future)
- [ ] **Multiple Save Slots**: Allow saving multiple game states (future)
- [ ] **Daily Challenge**: Generate a new puzzle each day (future)

## Future Expansion Plan

### Phase 1: Enhanced Gameplay (Short-term)

- [ ] **Difficulty Selector in New Game**: UI to choose Easy/Medium/Hard/Expert when starting
- [ ] **Improved Generator**: Use random generation instead of hardcoded puzzle
- [ ] **Undo/Redo Stack**: Implement move history with undo/redo buttons
- [ ] **Timer**: Add game timer with pause/resume
- [ ] **Note/Pencil Marks**: Allow users to add candidate numbers to cells
- [ ] **Auto-fill Single Candidates**: Option to auto-fill cells with only one valid number
- [ ] **Statistics**: Track games played, win rate, average time

### Phase 3: Advanced Features (Medium-term)

- [ ] **Enhanced Visual Effects**:
  - Particle effects on win
  - Smooth transitions between screens
  - Cell animations (shake on error, pulse on hint)
  - Progress bar/circle
- [ ] **Achievements System**: Track milestones and accomplishments
- [ ] **Leaderboard**: Local leaderboard for fastest times
- [ ] **Tutorial Mode**: Interactive tutorial for new players
- [ ] **Accessibility**: Screen reader support, high contrast mode

### Phase 4: Advanced Algorithms (Long-term)

- [ ] **Sophisticated Solver**:
  - Use advanced solving techniques (naked pairs, hidden singles, etc.)
  - Difficulty rating based on techniques required
- [ ] **Uniqueness Check**: Ensure generated puzzles have exactly one solution
- [ ] **Puzzle Rating**: Real difficulty rating based on solving complexity
- [ ] **Puzzle Importer**: Load puzzles from strings or files
- [ ] **Puzzle Exporter**: Share puzzles with friends

### Phase 5: Web Expansion (Long-term)

The Core library is platform-agnostic and can be reused for web:

- [ ] **Blazor WebAssembly App**:
  - Reference Sudoku.Core project
  - Implement HTML/CSS UI with Razor components
  - Use same game logic (Generator, Validator, Solver)
  - Deploy to static hosting (GitHub Pages, Azure Static Web Apps)
- [ ] **Blazor Server Option**: For real-time multiplayer features
- [ ] **Progressive Web App (PWA)**: Offline support, installable

### Phase 6: Social & Multiplayer (Future)

- [ ] **Online Multiplayer**: Race to solve same puzzle
- [ ] **Puzzle Sharing**: Share custom puzzles via codes
- [ ] **Community Puzzles**: Upload and download user-created puzzles
- [ ] **Social Features**: Friends, chat, challenges
- [ ] **Cloud Sync**: Sync progress across devices

### Phase 7: Monetization & Polish (Future)

- [ ] **Puzzle Packs**: Themed puzzle collections (beginner, expert, historical)
- [ ] **Hints Limit**: Free hints with option to unlock more
- [ ] **Premium Features**: Advanced statistics, exclusive themes
- [ ] **Ad Integration**: Optional ads for free version
- [ ] **Analytics**: Track usage patterns for improvements

## Technical Debt & Improvements

### Current Limitations

1. **Hardcoded Puzzle**: Using a static puzzle instead of random generation for reliability
   - *Solution*: Test and stabilize random generator, add uniqueness validation
   
2. **No Uniqueness Check**: Generated puzzles may have multiple solutions
   - *Solution*: Implement solution counting in RemoveCells method
   
3. **Simple Difficulty**: Difficulty based only on cell count
   - *Solution*: Analyze solving techniques required for better rating
   
4. **No Game State Persistence**: Current game not saved on app close
   - *Solution*: Extend SettingsService to save/load board state
   
5. **Limited Testing**: No unit tests yet
   - *Solution*: Add xUnit project for Core services testing

### Code Quality Improvements

- [x] Treat warnings as errors (implemented)
- [x] Use modern async APIs (DisplayAlertAsync, FadeToAsync, etc.)
- [x] Replace obsolete Frame with Border
- [x] Add AI instructions documentation
- [ ] Add XML documentation to all public APIs
- [ ] Implement unit tests for Core services
- [ ] Add UI tests for MAUI pages
- [ ] Extract magic numbers to constants
- [ ] Add logging for debugging and telemetry
- [ ] Performance profiling for large operations

## Contributing

This is a learning/demonstration project. Key areas for contribution:

1. **FontAwesome Setup**: Download and add fa-solid-900.ttf font
2. **Sound Effects**: Add free sound files to Resources/Raw/Sounds
3. **Themes**: Design additional color themes
4. **Algorithms**: Improve solver and generator efficiency
5. **UI/UX**: Enhance visual design and user experience
6. **Testing**: Add comprehensive test coverage

## Technology Stack

- **.NET 10**: Latest .NET runtime
- **.NET MAUI**: Cross-platform UI framework
- **C# 14**: Latest language features
- **CommunityToolkit.Mvvm**: MVVM utilities
- **Plugin.Maui.Audio**: Audio playback
- **FontAwesome Free**: Icon font for UI buttons
- **System.Text.Json**: Settings serialization

## For AI Agents

**Important**: Read `AI_INSTRUCTIONS.md` before making any code changes. It contains:
- Critical rules (DisplayAlertAsync, warnings as errors, cross-platform)
- Coding standards and best practices
- Architecture guidelines
- Common tasks and quick reference
- User preferences and communication style

## License

This project is provided as-is for educational and demonstration purposes.

## Acknowledgments

- Sudoku puzzle format and rules from classic Sudoku game
- Backtracking algorithm implementation based on standard CS techniques
- FontAwesome Free icons: https://fontawesome.com/
- Free sound effects from freesound.org, zapsplat.com (when added)
