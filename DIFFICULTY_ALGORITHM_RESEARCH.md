# Sudoku Difficulty Algorithm Research

## Problem Statement

Current puzzle generator produces valid puzzles with unique solutions, but difficulty ratings are inaccurate.

**Root Cause:** Difficulty is based purely on number of clues, not solving techniques.

---

## SudokuWiki.org Strategy Classification

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

## Implementation Strategy

### Difficulty Scoring System

**Total Puzzle Score = Σ(Strategy Score × Usage Count)**

### Algorithm Approach

1. **Simulate Human Solving Process**
   - Apply strategies in order of difficulty
   - Track which strategies are needed to progress
   - Count how many times each strategy is used

2. **Scoring Formula**
   ```
   Difficulty = (StrategyWeight1 × Count1) + 
                (StrategyWeight2 × Count2) + ...
                + (MaxStrategyRequired × 2)
   ```

3. **Difficulty Tiers**
   - **Easy**: 0-100 (Basic strategies only)
   - **Medium**: 101-300 (Up to Tough strategies)
   - **Hard**: 301-600 (Requires Diabolical strategies)
   - **Expert**: 601-1000 (Requires Extreme strategies)
   - **Evil**: 1000+ (Multiple Extreme strategies)

### Key Insights from SudokuWiki

- **Chains are the key differentiator** between Hard and Expert puzzles
- **Bivalue cells** (2 candidates) and **bilocation** (2 positions for a digit) drive advanced techniques
- Many advanced strategies are **generalizations** of simpler ones (e.g., XY-Chains generalize Remote Pairs)
- **Uniqueness strategies** (Unique Rectangles, BUG) rely on the single-solution constraint

### Minimum Implementation for Accurate Grading

To accurately grade most published puzzles, implement:

**Must Have:**
- All Basic strategies (1-10)
- X-Wing, Y-Wing, Swordfish (core Tough strategies)
- Simple Colouring or XY-Chains (at least one chain strategy)

**Nice to Have:**
- Unique Rectangles
- Finned Fish (X-Wing/Swordfish)
- AIC (Alternating Inference Chains)

**Only for "Ultimate" Puzzles:**
- Forcing Chains
- Exocet patterns
- Pattern Overlay

---

## References

- **SudokuWiki.org** - Complete strategy documentation with scoring
- **Sudoku Explainer (Java)** - Open-source solver with difficulty rating
- **Donald Knuth's Dancing Links** - Algorithm X for puzzle solving
- **"Sudoku Creation and Grading" PDF** - Available on SudokuWiki.org
- **Top 50,000 Hard Puzzles** - Ruud's collection for testing
- **Arto Inkala's "World's Hardest Sudoku"** - Benchmark extreme puzzle

**Status:** Research complete - Ready for implementation