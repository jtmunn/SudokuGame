# ?? Installing Sudoku on Windows

## Quick Start

1. **Download** the latest `.zip` file from [Releases](https://github.com/jtmunn/SudokuGame/releases)
2. **Extract** the zip to any folder (e.g., `C:\Games\Sudoku\`)
3. **Run** `Sudoku.Maui.exe`
4. **(Optional)** Create a desktop shortcut for easy access

That's it! No installation required. ??

---

## ?? Security Warning (First Time)

When you first run the app, Windows may show a security warning:

> **"Windows protected your PC"**  
> Microsoft Defender SmartScreen prevented an unrecognized app from starting.

**This is normal!** The app isn't code-signed with an expensive certificate ($100-300/year), so Windows doesn't recognize it. The code is 100% open source and auditable on GitHub.

**To proceed:**
1. Click **"More info"**
2. Click **"Run anyway"**

After the first run, Windows will remember your choice.

---

## ?? Updating to a New Version

**Portable apps don't auto-update.** To upgrade:

1. Download the new `.zip` file
2. Extract to a **new folder** (or delete the old files first)
3. Copy over your game data if desired (see "Data Location" below)
4. Run the new `Sudoku.Maui.exe`

Your settings and statistics are stored separately, so they'll carry over automatically.

---

## ?? Data Location

Your game data is stored in:
```
C:\Users\<YourName>\AppData\Local\Packages\com.jtmunn.sudoku_<hash>\LocalCache\Local\
```

**Files:**
- `sudoku_settings.json` - Your preferences (difficulty, theme, button visibility)
- `sudoku_statistics.json` - Best times for each difficulty level
- `sudoku_gamestate.json` - Current game in progress (auto-saved)

You can back up these files or transfer them to another PC.

---

## ? Uninstalling

**Portable apps have no installer, so there's nothing to uninstall.**

To remove the app:
1. Delete the folder where you extracted the app
2. **(Optional)** Delete your game data folder (see "Data Location" above)

Clean and simple - no registry entries or system files left behind.

---

## ?? Troubleshooting

### "Windows protected your PC" every time I run it
- Right-click `Sudoku.Maui.exe` ? **Properties** ? **Unblock** ? **OK**
- Run the app again - Windows will remember your choice

### App won't start (crashes immediately)
- Make sure you extracted **all files** from the zip, not just the EXE
- Check Windows Event Viewer (Win + X ? Event Viewer ? Windows Logs ? Application) for error details
- See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for detailed diagnostics

### Performance issues
- Minimum requirements: Windows 10 version 1809 or later, 64-bit
- The app is self-contained and includes all required .NET runtime files

### Still having issues?
[Open an issue on GitHub](https://github.com/jtmunn/SudokuGame/issues) with:
- Your Windows version (run `winver` to check)
- Error message or screenshot
- Contents of Windows Event Viewer (Application log)

---

## ?? Why Portable Instead of Installer?

**Portable builds have advantages:**
- ? No installation required - extract and run
- ? No admin rights needed
- ? Easy to try without "committing"
- ? Clean uninstall (just delete the folder)
- ? Can run from USB drive
- ? Multiple versions can coexist

**Disadvantages:**
- ? No automatic updates
- ? No Start Menu integration (unless you manually pin it)
- ? Windows security warnings on first run

For a personal/hobby project, portable distribution keeps things simple and free (no expensive code-signing certificates required).

---

## ?? About Code Signing

This app is **not code-signed** because:
- Code-signing certificates cost $100-300/year
- This is a free, open-source project with no funding
- The code is 100% auditable on GitHub

**Want to verify the source?**
1. Clone the repository: `git clone https://github.com/jtmunn/SudokuGame.git`
2. Build it yourself (see [DEVELOPERS.md](DEVELOPERS.md))
3. Compare the behavior

In the future, if the project gains traction, we may explore proper code signing or switch to MSIX packaging.

---

## ?? System Requirements

- **OS:** Windows 10 (version 1809 or later) or Windows 11
- **Architecture:** x64 (64-bit)
- **.NET:** Included in the portable package (no separate install needed)
- **Disk Space:** ~50 MB

---

**Need help building from source instead?** See [DEVELOPERS.md](DEVELOPERS.md)
