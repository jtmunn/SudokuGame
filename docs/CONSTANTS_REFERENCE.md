# Sudoku UI Constants Reference

This document describes the layout and sizing model used throughout the Sudoku app.

The implementation lives in [`Sudoku.Maui/Helpers/SudokuLayoutCalculator.cs`](../Sudoku.Maui/Helpers/SudokuLayoutCalculator.cs). This document is a summary — the source code is authoritative.

---

## Design Principles

The layout is split into two independent systems to avoid circular dependencies between the grid and the buttons:

1. **Button & font sizes** are derived from **window dimensions** (clamped to a sensible range).
2. **Grid size** is handled entirely by MAUI's star row plus `SquareLayoutControl`, which enforces a 1:1 aspect ratio.
3. **Cell font sizes** are derived from the **actual rendered grid size** via `SizeChanged`, so the value reflects what was actually measured rather than a prediction.

This means there are no `BaseGridSize`-style scale factors and no manual `TranslationX` math.

---

## Constants

### `SudokuLayoutCalculator`

| Constant | Value | Purpose |
|----------|-------|---------|
| `MinGridSize` | `360` | Minimum size enforced by `SquareLayoutControl`. The window minimum width / height is sized so the grid never has to shrink below this. |

### Magic numbers used in the calculator

These numbers are intentional, documented in the calculator code, and are not exposed as constants:

| Expression | Where | Reason |
|------------|-------|--------|
| `(windowWidth - 60) / 7.5` | `Calculate` | "5 buttons across with margins" — yields a comfortable per-button width. |
| `(windowHeight - 80) / 14.0` | `Calculate` | Prevents buttons from eating vertical space the grid needs in landscape. |
| `Math.Clamp(..., 44, 100)` | `Calculate` | Never smaller than the platform-recommended 44 px tap target; never larger than 100 px. |
| `ButtonSize * 0.4` | `FontSize` | Button label size proportional to the button. |
| `ButtonSize * 0.18` | `CountFontSize` | Remaining-count badge size — much smaller than the label. |
| `ButtonSize * 0.15` | `CountMargin` | Inset of the count badge from the top-right corner. |
| `Math.Max(10, (gridSize / 9.0) * 0.55)` | `CalculateCellFontSize` | Cell digit takes ~55% of cell height; 10pt floor for tiny windows. |

---

## Layout Structure

The page is a **3-row `Grid`**:

```
+--------------------------------------------------------+
| Row 0 — Header (fixed height)                          |
|   [Difficulty] [Timer]              [New] [Settings]   |
+--------------------------------------------------------+
| Row 1 — Game Area (star-sized)                         |
|                                                        |
|              +-------------------+                     |
|              |                   |                     |
|              |  SudokuBoard      |                     |
|              |  inside           |                     |
|              |  SquareLayout     |                     |
|              |  (always 1:1)     |                     |
|              |                   |                     |
|              +-------------------+                     |
|                                                        |
+--------------------------------------------------------+
| Row 2 — Bottom Bar                                     |
|   [Hint]  [Check]                                      |
|   [1] [2] [3] [4] [5] [6] [7] [8] [9]                  |
+--------------------------------------------------------+
```

### Why no manual centering?

`SquareLayoutControl` automatically maintains a 1:1 aspect ratio and centers its child within whatever space MAUI gives it. The page does **not** use `AbsoluteLayout` and does **not** offset action buttons with `TranslationX` — they simply live in Row 2 below the grid.

---

## Window Sizing

The window's minimum size is set so the grid can always render at `MinGridSize` (360) plus enough room for the header, bottom bar, and padding. Window-state persistence (size + position) is handled by `SettingsService` — see [WINDOW_SIZE_PERSISTENCE.md](WINDOW_SIZE_PERSISTENCE.md).

---

## Modifying the Layout

To change sizing behavior, edit [`SudokuLayoutCalculator.cs`](../Sudoku.Maui/Helpers/SudokuLayoutCalculator.cs) directly:

- **Tap target floor / ceiling** — adjust the `Math.Clamp(..., 44, 100)` bounds.
- **Cell digit size** — adjust the `0.55` factor in `CalculateCellFontSize`.
- **Button label / badge** — adjust the `0.4` / `0.18` factors in `Calculate`.
- **Minimum grid** — change `MinGridSize` (and update the window-minimum width/height accordingly).

All consumers re-read these values on `SizeChanged`, so changes take effect immediately on the next layout pass.
