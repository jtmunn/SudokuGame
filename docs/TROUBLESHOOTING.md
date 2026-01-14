# Troubleshooting Guide

This document covers common issues encountered during development and deployment of the Sudoku MAUI application.

---

## 🚨 Critical Issues

### Settings Page Crash in Release/Portable Builds

**Symptoms:**
- App works fine in Debug mode
- App crashes when clicking Settings button in Release builds or portable deployments
- Windows Event Viewer shows: `Exception code: 0xc000027b` (XAML parsing exception)
- Error message: `Cannot convert "True" into Microsoft.Maui.IShadow`

**Root Cause:**
Invalid XAML syntax in SettingsPage.xaml. The `Shadow` property was set to `Shadow="True"` which is invalid. Debug builds are lenient with XAML errors, but Release builds enforce strict validation.

**Solution:**

❌ **WRONG:**
```xml
<Border Shadow="True">
    <VerticalStackLayout>
        <!-- content -->
    </VerticalStackLayout>
</Border>
```

✅ **CORRECT:**
```xml
<Border>
    <Border.Shadow>
        <Shadow Brush="Black" Opacity="0.3" Radius="4" Offset="2,2"/>
    </Border.Shadow>
    <VerticalStackLayout>
        <!-- content -->
    </VerticalStackLayout>
</Border>
```

**Key Takeaway:**
The `Shadow` property in .NET MAUI requires a `Shadow` object with properties like `Brush`, `Opacity`, `Radius`, and `Offset`. It cannot be set to a boolean value.

---

## ℹ️ Build & Deployment Issues

### Build Output Path Differences

**Issue:**
Build output paths differ between local builds and GitHub Actions builds, causing CI/CD failures.

**Paths:**
- **Local builds:** `Sudoku.Maui\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish`
- **GitHub Actions:** `Sudoku.Maui\bin\x64\Release\net10.0-windows10.0.19041.0\win-x64\publish`

**Why:** GitHub Actions uses explicit platform configuration that creates an `x64` subdirectory. Local builds using `dotnet publish` without explicit platform configuration don't create this subdirectory.

**Solution:**
Use the correct path in CI/CD workflows:
```yaml
$publishDir = "Sudoku.Maui\bin\x64\Release\net10.0-windows10.0.19041.0\win-x64\publish"
```

---

### Font Files Not Copied to Publish Output

**Symptoms:**
- FontAwesome icons don't display in portable builds
- App may crash when trying to render icon fonts

**Root Cause:**
The `<MauiFont>` directive registers fonts with MAUI but doesn't physically copy them to the publish directory for portable builds.

**Solution:**
Add explicit copy directive to `.csproj`:

```xml
<!-- Custom Fonts -->
<MauiFont Include="Resources\Fonts\*" />

<!-- Explicitly copy font file to output for portable builds -->
<None Include="Resources\Fonts\fa-solid-900.ttf">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
</None>
```

---

## ℹ️ Debugging Tips

### Debugging Release Builds

**Problem:**
Debug output (`Debug.WriteLine`) doesn't show in Release builds by default.

**Solutions:**

1. **Use Console.WriteLine instead:**
   ```csharp
   Console.WriteLine("[MyMethod] Debug message");
   ```

2. **Add message boxes for critical debugging:**
   ```csharp
   await DisplayAlertAsync("Debug", $"Value: {someVariable}", "OK");
   ```

3. **Write to a log file:**
   ```csharp
   var logPath = Path.Combine(FileSystem.AppDataDirectory, "debug_log.txt");
   File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
   ```

4. **Disable optimizations temporarily** (in `.csproj`):
   ```xml
   <PropertyGroup Condition="'$(Configuration)' == 'Release'">
       <DebugType>full</DebugType>
       <DebugSymbols>true</DebugSymbols>
   </PropertyGroup>
   ```

---

### Checking Windows Event Viewer for MAUI Crashes

When a MAUI app crashes silently:

1. Open **Event Viewer** (Win + X ? Event Viewer)
2. Navigate to: **Windows Logs ? Application**
3. Look for **Error** events with source `.NET Runtime` or your app name
4. Key information:
   - **Exception code**: `0xc000027b` = XAML parsing error
   - **Faulting module**: `Microsoft.UI.Xaml.dll` = XAML issue
   - **Faulting application**: Shows which executable crashed

---

## ℹ️ XAML Best Practices

### Always Use Proper Object Syntax for Complex Properties

Many MAUI properties require object syntax, not simple string values:

❌ **WRONG:**
```xml
<Border Shadow="True" />
<Frame HasShadow="True" />  <!-- Frame is obsolete, use Border -->
```

✅ **CORRECT:**
```xml
<Border>
    <Border.Shadow>
        <Shadow Brush="Black" Opacity="0.3" Radius="4" Offset="2,2"/>
    </Border.Shadow>
</Border>
```

### Common Complex Properties in MAUI

These properties require object syntax:
- `Shadow` - requires `<Shadow>` object
- `Stroke` - can be `Color` or `Brush` object
- `Background` - can be `Color` or `Brush` object
- `GradientStops` - requires collection
- `FormattedString` - requires nested `Span` elements

---

## ℹ️ Architecture-Specific Issues

### Portable Build vs MSIX Package

**Portable Build Configuration:**
```xml
<WindowsPackageType>None</WindowsPackageType>
<PublishSingleFile>false</PublishSingleFile>
<SelfContained>true</SelfContained>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
```

**Key Differences:**
- **Portable:** Creates folder with EXE + DLLs, can be copied anywhere
- **MSIX:** Creates installable package, requires signing for distribution

**Common Issues:**
- Resources (fonts, images) may not copy correctly in portable builds
- Path handling differs between packaging types
- Portable builds require manual resource copying (see font issue above)

---

## ℹ️ Diagnostic Checklist for Crashes

When debugging a crash, check in this order:

1. **✔️ Windows Event Viewer** - Get the actual exception code
2. **✔️ Exception Type** - `0xc000027b` = XAML parsing
3. **✔️ Check XAML Syntax** - Look for properties set to wrong types (e.g., `Shadow="True"`)
4. **✔️ Debug vs Release** - Does it work in Debug but fail in Release?
5. **✔️ Missing Resources** - Are all fonts/images present in publish output?
6. **✔️ Build Paths** - Verify output directory structure matches expectations
7. **✔️ Add Logging** - Use Console.WriteLine or log files (not Debug.WriteLine in Release)

---

## ℹ️ Portable Build Deployment Checklist

Before creating a portable release:

- [ ] Test in **Release configuration** from Visual Studio (F5)
- [ ] Check **publish output** contains all required files:
  - [ ] All DLLs
  - [ ] Font files (`fa-solid-900.ttf`)
  - [ ] Windows App SDK runtime files
- [ ] Test the **published EXE** directly (not from Visual Studio)
- [ ] Click through all UI screens (especially Settings page)
- [ ] Verify fonts render correctly (FontAwesome icons)
- [ ] Check Windows Event Viewer for any errors

---

## ℹ️ Additional Resources

### MAUI Documentation
- [.NET MAUI Shadow](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/shadow)
- [XAML Property Element Syntax](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/xaml-syntax)
- [Publishing Unpackaged Windows Apps](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/overview)

### Debugging
- [Debugging .NET MAUI Apps](https://learn.microsoft.com/en-us/dotnet/maui/troubleshooting)
- [Windows Event Viewer Guide](https://learn.microsoft.com/en-us/shows/inside/event-viewer)

---

## ℹ️ Lessons Learned

### Key Insights from Real-World Debugging

1. **Debug mode is forgiving, Release mode is not** - Always test Release builds before deployment
2. **XAML syntax errors may not surface in Debug** - Strict validation only happens in Release
3. **Windows Event Viewer is your friend** - It captures exceptions that don't show in debugger
4. **Build paths matter** - Local vs CI/CD builds may use different directory structures
5. **Resource copying isn't automatic** - Portable builds require explicit copy directives for non-standard resources

---

**Last Updated:** 2026-01-01  
**Project Version:** v1.1.0
