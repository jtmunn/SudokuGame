# Sudoku Difficulty Algorithm Research

> **✅ STATUS: IMPLEMENTED** - This document contains the original research that guided implementation.

## Implementation Summary

**What's Implemented:**
- ✅ 11 solving strategies across 3 difficulty categories
- ✅ Technique-based difficulty scoring
- ✅ Logical solver that mimics human solving approach
- ✅ Puzzle generator with target difficulty matching
- ✅ Comprehensive unit tests for all strategies

**Implementation Details:**
- See `Sudoku.Core/Strategies/` for all strategy implementations
- See `Sudoku.Core/Services/SudokuLogicalSolver.cs` for solver logic
- See `Sudoku.Core/Services/SudokuGenerator.cs` for difficulty-driven generation

---

## Original Problem Statement

Puzzle generator needed to produce valid puzzles with accurate difficulty ratings.

**Root Cause:** Difficulty should be based on solving techniques, not clue count.

---

## Current Implementation Status

### ✅ Implemented Strategies (11 Total)

#### **BASIC (7 strategies)** - Scores: 5-40
1. ✅ **Naked Single** (5) - `NakedSingleStrategy.cs`
2. ✅ **Hidden Single** (10) - `HiddenSingleStrategy.cs`
3. ✅ **Pointing Pairs** (25) - `PointingPairStrategy.cs`
4. ✅ **Box/Line Reduction** (25) - `BoxLineReductionStrategy.cs`
5. ✅ **Naked Pairs** (30) - `NakedPairStrategy.cs`
6. ✅ **Hidden Pairs** (35) - `HiddenPairStrategy.cs`
7. ✅ **Naked Triples** (40) - `NakedTripleStrategy.cs`

#### **TOUGH (3 strategies)** - Scores: 100-140
8. ✅ **X-Wing** (100) - `XWingStrategy.cs`
9. ✅ **Y-Wing** (130) - `YWingStrategy.cs`
10. ✅ **Swordfish** (140) - `SwordfishStrategy.cs`

#### **DIABOLICAL (1 strategy)** - Score: 240
11. ✅ **XY-Chain** (240) - `XYChainStrategy.cs`

### 🚧 Potential Future Additions

These strategies would enhance difficulty grading for extreme puzzles:

- **Naked/Hidden Quads** (50) - 4-cell extensions
- **XYZ-Wing** (140) - Extended Y-Wing variant
- **Jellyfish** (200) - 4×4 pattern
- **Finned X-Wing/Swordfish** (320-350) - Fish with extra candidates
- **Unique Rectangles** (200) - Uniqueness-based logic
- **AIC (Alternating Inference Chains)** (400+) - General chain framework

**Note:** Current 11 strategies handle the vast majority of published puzzles, including most "Evil" rated puzzles.

---

## Original Research: SudokuWiki.org Strategy Classification

> **Note:** This section contains the original research. See "Current Implementation Status" above for what's actually implemented.

### **BASIC Strategies** (Difficulty Score: 1-50)
1. **Naked Single** - Only one candidate remains in a cell
2. **Hidden Single** - Only one cell in a unit can contain a digit
3. **Naked Pairs** (Score: 30) - Two cells with same 2 candidates
4. **Naked Triples** (Score: 40) - Three cells with same 3 candidates
5. **Naked Quads** (Score: 50) - Four cells with same 4 candidates
6. **Hidden Pairs** (Score: 35) - Two digits locked to 2 cells in a unit
7. **Hidden Triples** (Score: 45) - Three digits locked to 3 cells
8. **Hidden Quads** (Score: 50) - Four digits locked to 4 cells
9. **Pointing Pairs** (Score: 25) - Box/line reduction
10. **Box/Line Intersection** (Score: 25) - Line/box reduction

### **TOUGH Strategies** (Difficulty Score: 60-150)
1. **X-Wing** (Score: 100) - 2×2 pattern across rows/columns
2. **Simple Colouring** (Score: 120) - Basic chain coloring
3. **Y-Wing** (Score: 130) - XY-Wing pattern with 3 cells
4. **Swordfish** (Score: 140) - 3×3 pattern across rows/columns
5. **XYZ-Wing** (Score: 140) - Extended Y-Wing with pivot
6. **Rectangle Elimination** (Score: 110)
7. **Chute Remote Pairs** (Score: 90)
8. **BUG** (Bivalue Universal Grave) (Score: 150)
9. **Avoidable Rectangles** (Score: 150)

### **DIABOLICAL Strategies** (Difficulty Score: 160-300)
1. **X-Cycles** (Score: 180) - Extended X-Wing chains
2. **3D Medusa** (Score: 220) - Advanced multi-color chains
3. **Jellyfish** (Score: 200) - 4×4 pattern across rows/columns
4. **Unique Rectangles** (Score: 200) - Uniqueness constraint logic
5. **XY-Chains** (Score: 240) - Bivalue chains
6. **WXYZ-Wing** (Score: 250) - 4-cell wing pattern
7. **Aligned Pair Exclusion** (Score: 280)
8. **Tridagons** (Score: 290)
9. **Fireworks** (Score: 270)
10. **SK Loops** (Score: 300)
11. **Extended Rectangles** (Score: 250)
12. **Hidden Unique Rectangles** (Score: 260)

### **EXTREME Strategies** (Difficulty Score: 300+)
1. **Finned X-Wing** (Score: 320)
2. **Finned Swordfish** (Score: 350)
3. **Grouped X-Cycles** (Score: 380)
4. **Alternating Inference Chains** (Score: 400+)
5. **AIC with Groups** (Score: 420)
6. **AIC with ALSs** (Almost Locked Sets) (Score: 450)
7. **Sue-de-Coq** (Score: 440)
8. **Digit Forcing Chains** (Score: 500)
9. **Nishio Forcing Chains** (Score: 550)
10. **Cell Forcing Chains** (Score: 600)
11. **Unit Forcing Chains** (Score: 650)
12. **Almost Locked Sets** (Score: 480)
13. **Exocet** (Score: 700+)
14. **Double Exocet** (Score: 800+)
15. **Death Blossom** (Score: 750)
16. **Pattern Overlay** (Score: 900+)

---

## Implementation Details

### How the System Works

1. **Puzzle Generation** (`SudokuGenerator.cs`):
   ```csharp
   - Create complete valid board
   - Remove cells one-by-one
   - After each removal:
     * Verify unique solution (SudokuBacktrackingSolver)
     * Check clue count is in target range (PRIMARY)
     * Test difficulty score (SudokuLogicalSolver) (SECONDARY)
   - Stop when both criteria satisfied
   ```

2. **Logical Solving** (`SudokuLogicalSolver.cs`):
   ```csharp
   - Try strategies in order of difficulty
   - Apply first successful strategy
   - Restart from easiest strategy (mimics human solving)
   - Track all strategies used and count applications
   - Calculate total difficulty score
   ```

3. **Difficulty Targets** (Actual Implementation):

**PRIMARY CRITERION - Clue Count Ranges:**
```csharp
Easy:   36-46 given clues (35-45 empty cells)
Medium: 32-35 given clues (46-49 empty cells)
Hard:   28-31 given clues (50-53 empty cells)
Expert: 24-27 given clues (54-57 empty cells)
Evil:   22-25 given clues (56-59 empty cells)
```
*Based on SudokuWiki.org: "It usually leaves between twenty and thirty clues behind."*

**SECONDARY CRITERION - Difficulty Score Validation:**
```csharp
Easy:   Target score 50   (Accepts: 25-75)
Medium: Target score 200  (Accepts: 100-300)
Hard:   Target score 350  (Accepts: 175-525)
Expert: Target score 500  (Accepts: 250-750)
Evil:   Target score 700  (Accepts: 350-1050)
```

**Why Clue Count First, Then Score?**
   
From SudokuWiki.org research: *"clue density does not - in general - affect the grade or difficulty of a puzzle"*. However, with very few empty cells (like 5), puzzles become trivially easy regardless of strategy requirements.

The dual-criteria approach ensures:
- ✅ **Industry-standard puzzle appearance** - Proper empty cell distribution
- ✅ **Consistent difficulty feel** - Users see puzzles matching other Sudoku apps
- ✅ **Prevents degenerate cases** - Can't have "Easy" with 76 givens (5 empty)
- ✅ **Validates logical complexity** - Score ensures strategies match difficulty level

### Testing

All strategies have comprehensive xUnit tests:
- `Sudoku.Core.Tests/Strategies/Basic/` - 7 test files
- `Sudoku.Core.Tests/Strategies/Tough/` - 3 test files
- `Sudoku.Core.Tests/Strategies/Diabolical/` - 1 test file

Run tests: `dotnet test Sudoku.Core.Tests`

---

## References & Research Notes

**Implementation Status:** ✅ Complete and working

### Key Insights from SudokuWiki (Applied in Implementation)

- ✅ **Chains are the key differentiator** between Hard and Expert puzzles
- ✅ **Bivalue cells** (2 candidates) and **bilocation** (2 positions for a digit) drive advanced techniques
- ✅ Many advanced strategies are **generalizations** of simpler ones (e.g., XY-Chains generalize Remote Pairs)
- 🔍 **Uniqueness strategies** (Unique Rectangles, BUG) rely on the single-solution constraint (not yet implemented)

### Implementation Coverage Assessment

**Current Coverage:** Excellent for most published puzzles

**✅ Have All "Must Have" Strategies:**
- All 7 Basic strategies implemented
- X-Wing, Y-Wing, Swordfish (core Tough strategies) ✅
- XY-Chains (advanced chain strategy) ✅

**🚧 "Nice to Have" (Not Required for Most Puzzles):**
- Unique Rectangles - Future consideration
- Finned Fish - Future consideration
- AIC (Alternating Inference Chains) - Future consideration

**📝 "Ultimate Puzzles Only" (Extremely Rare):**
- Forcing Chains
- Exocet patterns
- Pattern Overlay

---

## External References

- **SudokuWiki.org** - Complete strategy documentation with scoring
  - [Introduction](https://www.sudokuwiki.org/Introduction) - "It usually leaves between twenty and thirty clues behind"
  - [Brute Force vs Logical Strategies](https://www.sudokuwiki.org/Brute_Force_vs_Logical_Strategies) - "clue density does not - in general - affect the grade or difficulty"
  - [Strategy Families](https://www.sudokuwiki.org/Strategy_Families) - Complete strategy catalog
- **Sudoku Explainer (Java)** - Open-source solver with difficulty rating
- **Donald Knuth's Dancing Links** - Algorithm X for puzzle solving
- **"Sudoku Creation and Grading" PDF** - Available on SudokuWiki.org
- **Top 50,000 Hard Puzzles** - Ruud's collection for testing
- **Arto Inkala's "World's Hardest Sudoku"** - Benchmark extreme puzzle
