# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.4.0] - 2026-04-19

### Changed
- Removed MVVM ViewModel layer — game state and timer logic inlined directly into SudokuPage code-behind for a simpler architecture
- Redesigned responsive layout — buttons scale from window size, grid fills remaining space via MAUI star rows
- Code cleanup — removed dead code and duplicates, renamed classes for clarity and consistency

### Added
- Hover animation states on Hint, Check, and Settings Back buttons
- Cell error state (`HasError`) now persisted in board serialization format

### Fixed
- Cell error state correctly restores through save/load cycles
- Mistakes counter accuracy bug
- Removed duplicate `ApplyNumber` method
- Stabilized flaky clue count test bounds

### Removed
- Sudoku.Application.Tests project (consolidated with architecture simplification)

---

## [1.3.0] - 2026-01-17

### Fixed
- Cell error styling now properly clears when corrected values are entered
- Timer correctly pauses during difficulty selection modal
- Timer pauses when window loses focus and resumes on activation

### Improved
- "Abandon Puzzle?" prompt now only appears when user has actually made entries to the puzzle
- Better UX flow - completed puzzles don't trigger abandonment prompts

---

## [1.2.0] - 2026-01-12

### Added
- **Difficulty Selection Modal** - Professional modal popup at startup and for new games
  - Shows last played difficulty and best times
  - Theme-aware button styling
  - Dismissible during active games, required on first launch
- **Game Summary Popup** - Displays statistics when puzzle is completed
  - Shows completion time with personal best comparison
  - Tracks mistakes and hints used
  - "Play Again" and "Done" options
- **Number Pad Counters** - Displays remaining count for each number (1-9)
  - Automatically disables numbers when all 9 instances are placed
  - Updates dynamically as board changes

### Fixed
- Difficulty algorithm now uses clue count as primary criterion instead of strategy score
  - Easy: 38-42 clues
  - Medium: 32-36 clues
  - Hard: 28-31 clues
  - Expert: 24-27 clues
  - Evil: 22-23 clues
- Version management centralized in `Directory.Build.props`
- GitHub Actions now correctly reads version from centralized location

### Improved
- Updated all documentation to reflect implemented features
- Added comprehensive unit tests for clue count ranges across all difficulty levels
- Better distinction between puzzle generation criteria

---

## [1.1.0] - 2026-01-08

### Added
- **100% Strategy Test Coverage** - Comprehensive tests for all 11 solving strategies
  - 17 new tests for SwordfishStrategy and XYChainStrategy
  - 26 new tests for NakedTripleStrategy and SudokuGenerator
  - All basic, tough, and diabolical strategies fully covered

### Performance
- **500-1000x Speed Improvement** in solver and generator
  - Implemented bitset optimizations for candidate tracking
  - Direct array access instead of LINQ queries
  - Puzzle generation now nearly instant for all difficulty levels

### UI
- Removed button shadows from action buttons for cleaner appearance
- Adjusted padding on action button stack for better spacing

---

## [1.0.0] - 2026-01-05

### Added - Initial Release
- **Core Gameplay**
  - Full Sudoku game implementation with 9x9 grid
  - Five difficulty levels: Easy, Medium, Hard, Expert, Evil
  - Real-time error detection and highlighting
  - Valid move validation before placement
  
- **Puzzle Generation**
  - Intelligent puzzle generator using 11 solving strategies
  - Difficulty based on logical solving techniques required
  - Guaranteed unique solutions for all generated puzzles
  
- **Solving Strategies Implemented**
  - Basic: Naked Singles, Hidden Singles, Naked Pairs, Hidden Pairs, Naked Triples, Pointing Pairs, Box/Line Reduction
  - Tough: X-Wing, Y-Wing, Swordfish
  - Diabolical: XY-Chain
  
- **User Interface**
  - Responsive grid scaling from 360px to 4K displays
  - Light and dark theme support
  - Cell highlighting (selected cell, matching numbers, row/column/block)
  - Optional hint and check buttons (can be hidden in settings)
  - Keyboard support on Windows (number keys, Backspace/Delete)
  
- **Game Features**
  - Auto-save functionality - resume exactly where you left off
  - Game timer with pause when window loses focus
  - Statistics tracking - best completion times per difficulty
  - Settings persistence (theme, button visibility, last played difficulty)
  - Window size and position persistence
  
- **Quality**
  - TreatWarningsAsErrors enabled - zero warnings tolerated
  - Cross-platform architecture (Core logic separate from UI)
  - Comprehensive documentation (installation, development, troubleshooting)
  - MIT License - fully open source

---

## Version Numbering

This project uses Semantic Versioning:
- **MAJOR** version for incompatible API/data format changes
- **MINOR** version for new features in a backward-compatible manner  
- **PATCH** version for backward-compatible bug fixes

---

## Links

- [GitHub Repository](https://github.com/jtmunn/SudokuGame)
- [Latest Release](https://github.com/jtmunn/SudokuGame/releases/latest)
- [Issue Tracker](https://github.com/jtmunn/SudokuGame/issues)
