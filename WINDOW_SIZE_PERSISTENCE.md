# Window Size Persistence

## Overview

The application automatically saves and restores window size between sessions using a **debounced save mechanism**. Window dimensions are saved 500ms after the user stops resizing, preventing excessive file writes while ensuring changes are captured.

## Implementation Details

### What Gets Saved

The following window state is persisted in the `sudoku_settings.json` file:

- **WindowWidth**: The width of the window in pixels
- **WindowHeight**: The height of the window in pixels

### How It Works

#### On Application Startup (`App.xaml.cs`)

1. The `CreateWindow` method loads saved settings
2. If specific width/height are saved, those dimensions are applied
3. If no saved size exists, defaults to 800x800
4. MAUI handles platform-specific window management automatically

#### During Window Resize (`App.xaml.cs`)

1. The `Window.SizeChanged` event fires whenever the window is resized
2. A debounce timer is reset on each resize event (500ms delay)
3. Once the user stops resizing for 500ms, the timer fires
4. Current window dimensions are saved to settings asynchronously
5. If user continues resizing, the timer resets and the process repeats

**Why Debouncing?**
- Prevents excessive file writes during drag-resize operations
- Only saves the final size after user is done resizing
- Industry-standard pattern for this type of persistence
- More efficient and reliable than trying to catch window close events

#### Debounce Logic

```csharp
// User resizes window multiple times rapidly
Resize 1 ? Timer starts (500ms)
Resize 2 ? Timer resets (500ms)  
Resize 3 ? Timer resets (500ms)
User stops resizing
...wait 500ms...
Timer fires ? Save size to disk ?
```

### Platform Behavior

The implementation is **fully cross-platform** using only MAUI's Window APIs:

- **Windows**: Window size is saved and restored
- **macOS**: Window size is saved and restored  
- **iOS/Android**: Window dimensions may not apply (mobile platforms)

MAUI handles all platform-specific details internally, including:
- Maximized/fullscreen states (handled by the platform)
- Multi-monitor scenarios
- Platform-specific window constraints

### Code Changes

#### `GameSettings.cs`
Added two properties:
```csharp
public double? WindowWidth { get; set; }
public double? WindowHeight { get; set; }
```

#### `App.xaml.cs`
- Added `_mainWindow` field to track the window instance
- Added `_saveWindowSizeTimer` for debouncing
- Added `SaveWindowSizeDelayMs` constant (500ms)
- Modified `CreateWindow()` to restore saved window size
- Added `OnWindowSizeChanged()` event handler with debounce logic
- Added `SaveWindowSize()` method to persist dimensions
- Uses MAUI's cross-platform `Window.SizeChanged` event

### Storage Location

Settings are stored in:
```
{FileSystem.AppDataDirectory}/sudoku_settings.json
```

Example content:
```json
{
  "DefaultDifficulty": "Medium",
  "ShowHintButton": true,
  "ShowCheckButton": true,
  "Theme": "Light",
  "WindowWidth": 1024,
  "WindowHeight": 768
}
```

## Technical Notes

### Cross-Platform Design

This implementation follows MAUI's philosophy of "write once, run anywhere":
- Uses only `Microsoft.Maui.Controls.Window` APIs
- No platform-specific `#if` directives
- No native platform APIs (WinUI, AppKit, etc.)
- Let MAUI handle platform differences internally

### Debounce Timer Details

- **Delay**: 500ms (configurable via `SaveWindowSizeDelayMs` constant)
- **Type**: `System.Timers.Timer` with `AutoReset = false`
- **Thread-safe**: Timer callback can access services safely
- **Memory**: Timer is disposed and recreated on each resize to prevent leaks

### Why Not Save on Close?

We initially tried using window lifecycle events (`Stopped`, `Destroying`, etc.) but:
- ? These events are unreliable across platforms
- ? Some events fire too late (after window is destroyed)
- ? Some events fire too early (when switching apps, not closing)
- ? **Debounced resize saves** work reliably on all platforms
- ? User never loses their window size preference
- ? No complex platform-specific code needed

### Null Checks

The code safely handles cases where:
- Settings service is unavailable
- Saved dimensions are null or missing
- The window reference is not available

## Philosophy

This implementation prioritizes:

1. **Reliability**: Save on resize works 100% of the time
2. **Efficiency**: Debouncing prevents excessive file writes
3. **Cross-platform compatibility**: Works on Windows, macOS, iOS, Android without platform-specific code
4. **Simplicity**: Let MAUI and the OS handle complex windowing behavior
5. **MAUI-first approach**: Use the framework as intended

Rather than fighting platform differences with conditional compilation and native APIs, we embrace MAUI's abstraction layer and trust it to do the right thing on each platform.

## Testing

To verify the feature works:

1. **Basic Size Persistence**:
   - Resize the window to a custom size
   - Wait 1 second (allow debounce timer to fire)
   - Close the app
   - Reopen - window should be the same size

2. **Debounce Behavior**:
   - Rapidly resize the window multiple times
   - Stop resizing and watch debug output
   - Should only see one "Window size saved" message ~500ms after stopping

3. **Default Behavior**:
   - Delete `sudoku_settings.json` from AppData
   - Launch app - should open at 800x800 (default size)

4. **Debug Output**:
   - Check Output window for: `Window size saved: [width]x[height]`
   - Appears ~500ms after each resize operation completes

## Compatibility

- ? Windows 10/11 (full support)
- ? macOS (full support)
- ?? iOS (size may not apply - mobile platform)
- ?? Android (size may not apply - mobile platform)

All platforms use the same code path - no special cases!
