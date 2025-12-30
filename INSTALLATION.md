# ?? Installing Sudoku on Windows

## Quick Install

1. **Download** the latest `.msix` file from [Releases](https://github.com/jtmunn/SudokuGame/releases)
2. **Double-click** the `.msix` file
3. Click **Install** in the dialog that appears
4. Find **Sudoku** in your Start Menu

That's it! ??

---

## ?? Security Warning (First Time)

When you first run the installer, Windows will show a warning:

> **"Do you want to install this application?"**  
> Publisher: CN=jtmunn

This is normal! The app isn't signed with an expensive code-signing certificate, so Windows doesn't recognize the publisher. The code is 100% open source and auditable.

**To proceed:**
- Click **"Show more"** (if needed)
- Click **"Install anyway"**

---

## ?? Updating to a New Version

1. Download the new `.msix` file
2. Double-click to install
3. Windows will automatically replace the old version

No need to uninstall first! Your game progress and settings are preserved.

---

## ??? Uninstalling

**Via Settings:**
1. Open **Settings** ? **Apps** ? **Installed apps**
2. Find **Sudoku**
3. Click **?** ? **Uninstall**

**Via Start Menu:**
1. Right-click **Sudoku** in Start Menu
2. Click **Uninstall**

MSIX apps uninstall cleanly with no leftover files in your registry or system folders.

---

## ? Troubleshooting

### "This app package is not supported for installation by App Installer"
- Try right-clicking the `.msix` file ? **Properties** ? **Unblock** ? **OK**
- Then double-click again

### "Developer mode required"
- You shouldn't need developer mode for MSIX
- If Windows asks, go to **Settings** ? **Privacy & Security** ? **For developers** ? Enable **Developer Mode**

### App won't start after install
- Try restarting your PC
- Make sure you have the latest Windows updates

### Still having issues?
[Open an issue on GitHub](https://github.com/jtmunn/SudokuGame/issues) with:
- Your Windows version (run `winver` to check)
- Error message or screenshot
- What happened when you tried to install

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

In the future, if the project gains traction, we may explore proper code signing.

---

## ??? System Requirements

- **OS:** Windows 10 (version 1809 or later) or Windows 11
- **Architecture:** x64 (64-bit)
- **.NET:** Included in the MSIX package (no separate install needed)

---

**Need help building from source instead?** See [DEVELOPERS.md](DEVELOPERS.md)
