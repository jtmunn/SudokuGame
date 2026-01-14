# Sudoku UI Constants Reference

This document describes all the constants used throughout the Sudoku application for sizing and layout calculations.

## SudokuPage.xaml.cs Constants

### Grid Sizing
- **MinGridSize** = `360` - Minimum size (width/height) for the Sudoku grid
- **BaseGridSize** = `450.0` - Reference size used for scaling calculations

### Button Sizing
- **BaseButtonSize** = `45.0` - Base size for number pad and action buttons (circular)
- **BaseFontSize** = `20.0` - Base font size for buttons

### Layout Spacing
- **GameAreaPadding** = `10` - Padding around the game area (applied to both sides)
- **ActionButtonMargin** = `20` - Left margin separating action buttons from grid
- **NumberButtonMargin** = `6` - Margin around each number button in the pad

### UI Regions
- **HeaderHeight** = `56` - Height of the header bar (difficulty, timer, settings)
- **NumberPadHeight** = `120` - Approximate height reserved for the number pad area

### Cell Font Sizing (in UpdateCellFontSizes)
- **FontSizeRatio** = `0.4` - Font size as 40% of cell size
- **MinCellFontSize** = `16` - Minimum font size for grid cells
- **MaxCellFontSize** = `60` - Maximum font size for grid cells

## App.xaml.cs Constants

### Window Sizing
- **MinGridSize** = `360` - Matches SudokuPage minimum grid size
- **BaseGridSize** = `450.0` - Reference grid size for calculations
- **BaseButtonSize** = `45.0` - Base action button size
- **ActionButtonMargin** = `20` - Action button left margin
- **GameAreaPadding** = `10` - Game area padding (per side)
- **MinSpacerWidth** = `50` - Minimum width for centering spacers
- **MinWindowHeight** = `700` - Minimum window height

## Scaling Formula

All UI elements scale proportionally based on the grid size:

```csharp
scale = currentGridSize / BaseGridSize
scaledButtonSize = BaseButtonSize * scale
scaledFontSize = BaseFontSize * scale
```

## Minimum Window Width Calculation

```csharp
MinimumWidth = MinGridSize                      // 360
             + (BaseButtonSize * minScale)      // ~36
             + ActionButtonMargin               // 20
             + (GameAreaPadding * 2)            // 20
             + MinSpacerWidth                   // 50
             = ~486 (rounded to 520 for comfort)
```

## Layout Structure

**IMPORTANT: Grid Centering vs Action Button Positioning**

The Sudoku grid is **centered independently** in the window. The action buttons are then positioned to the **right** of this centered grid using `TranslationX` offset. This ensures:
- Grid remains centered regardless of action button visibility
- Action buttons don't affect grid centering calculation
- Number pad remains centered below the grid

```
+--------------------------------------------------------+
| Header Bar (56px height)                               |
|  [Difficulty] [Timer] [New] [Settings]                 |
+--------------------------------------------------------+
| Game Area (Padding: 10px)                              |
|                                                        |
|          +----------+                                  |
|          |   Grid   |  [Hint]                          |
|          |  (360+)  |  [Check]                         |
|          | CENTERED |  | positioned via TranslationX   |
|          +----------+                                  |
|                                                        |
+--------------------------------------------------------+
| Number Pad (~120px height)                             |
|       [1] [2] [3] [4] [5] [6] [7] [8] [9]              |
|                  CENTERED                              |
+--------------------------------------------------------+
```

## Action Button Positioning

Action buttons use `TranslationX` to position themselves relative to the centered grid:

```csharp
offsetFromCenter = (gridSize / 2) + ActionButtonMargin + (buttonWidth / 2)
ActionButtonStack.TranslationX = offsetFromCenter
```

This positions the center of the action button stack to the right of the grid edge.

## Future Modifications

To adjust sizing, modify these constants:
- **Grid size range**: Change `MinGridSize` or `BaseGridSize`
- **Button sizes**: Adjust `BaseButtonSize` and `BaseFontSize`
- **Spacing**: Modify `ActionButtonMargin`, `GameAreaPadding`, `NumberButtonMargin`
- **Window constraints**: Update `MinWindowHeight` or minimum width calculation

All calculations will automatically update based on the new constant values.
