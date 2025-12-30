# Sudoku Difficulty Algorithm Research

## Problem Statement

Current puzzle generator produces valid puzzles with unique solutions, but difficulty ratings are inaccurate.

**Root Cause:** Difficulty is based purely on number of clues, not solving techniques.

---

## Solving Techniques Hierarchy

1. Naked Single (Easiest)
2. Hidden Single
3. Naked Pairs/Triples
4. Hidden Pairs/Triples
5. Pointing Pairs
6. X-Wing
7. Swordfish
8. XY-Wing
9. Advanced Chains

---

## Resources

- SudokuWiki.org
- Sudoku Explainer (Java)
- Donald Knuth's Dancing Links

**Status:** Research complete