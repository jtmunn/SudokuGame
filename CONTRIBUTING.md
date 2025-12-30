# Contributing to Sudoku

First off, **thank you** for considering contributing to this project! ?? Whether you're fixing a bug, adding a feature, or just improving documentation, every contribution helps make this game better for everyone.

---

## ?? Ways to Contribute

### ?? Report Bugs
Found a bug? [Open an issue](https://github.com/jtmunn/SudokuGame/issues/new) with:
- Clear description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Screenshots (if applicable)
- Platform (Windows, Android, iOS, macOS)

### ?? Suggest Features
Have an idea? We'd love to hear it! [Start a discussion](https://github.com/jtmunn/SudokuGame/discussions) with:
- What problem does it solve?
- How would it work?
- Any mockups or examples?

### ?? Design Contributions
- New color themes
- Icon improvements
- UI/UX enhancements
- Animation ideas

### ?? Documentation
- Fix typos
- Improve explanations
- Add examples
- Translate (future)

### ?? Testing
- Test on different devices
- Report edge cases
- Write unit tests
- Perform accessibility testing

### ?? Code Contributions
See below for our coding guidelines!

---

## ?? Getting Started

1. **Fork the repository**
   - Click the "Fork" button at the top right

2. **Clone your fork**
   ```bash
   git clone https://github.com/jtmunn/SudokuGame.git
   cd SudokuGame
   ```

3. **Create a branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

4. **Set up development environment**
   - See [DEVELOPERS.md](DEVELOPERS.md) for detailed setup instructions
   - Don't forget to add `fa-solid-900.ttf` font file!

5. **Make your changes**
   - Write clean, readable code
   - Follow existing patterns
   - Add comments when needed

6. **Test your changes**
   - Build on all target platforms if possible
   - Run existing tests (when available)
   - Manually test affected features

7. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add awesome new feature"
   ```
   (See commit message guidelines below)

8. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

9. **Open a Pull Request**
   - Go to the original repository
   - Click "New Pull Request"
   - Select your branch
   - Fill out the template

---

## ?? Pull Request Guidelines

### Before Submitting

- ? Code compiles without errors or warnings
- ? Code follows project style (see below)
- ? All existing functionality still works
- ? New features include appropriate comments
- ? Commit messages follow conventions (see below)

### PR Description Template

```markdown
## What does this PR do?
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How did you test this?

## Screenshots (if applicable)
Before/after screenshots

## Checklist
- [ ] Code compiles without warnings
- [ ] Tested on Windows/Android/iOS/macOS
- [ ] Updated documentation if needed
- [ ] Follows code style guidelines
```

---

## ?? Commit Message Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/):

### Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples
```bash
# Good commits
git commit -m "feat(ui): add dark theme support"
git commit -m "fix(solver): resolve infinite loop in backtracking"
git commit -m "docs: update README with new features"
git commit -m "refactor(grid): extract cell rendering logic"

# Bad commits (avoid these)
git commit -m "fixed stuff"
git commit -m "WIP"
git commit -m "asdfghjkl"
```

---

## ?? Code Style Guidelines

### General Rules

- **? Follow existing patterns** in the codebase
- **? Use meaningful variable names**: `selectedCell`, not `sc`
- **? Keep methods focused**: One responsibility per method
- **? Add comments** when logic isn't obvious
- **? Use constants** instead of magic numbers

### C# Conventions

```csharp
// ? GOOD
public class SudokuCell
{
    private const int MinValue = 1;
    private const int MaxValue = 9;
    
    public int Value { get; set; }
    
    /// <summary>
    /// Validates if the cell value is within allowed range.
    /// </summary>
    public bool IsValidValue()
    {
        return Value >= MinValue && Value <= MaxValue;
    }
}

// ? BAD
public class sudokucell  // Wrong naming
{
    public int v;  // Too short, not descriptive
    
    public bool check()  // Vague method name
    {
        return v >= 1 && v <= 9;  // Magic numbers
    }
}
```

### XAML Conventions

```xml
<!-- ? GOOD -->
<Border
    WidthRequest="450"
    HeightRequest="450"
    BackgroundColor="{DynamicResource CellDefaultColor}"
    StrokeThickness="2">
    <Grid ColumnDefinitions="*,*,*">
        <!-- Content -->
    </Grid>
</Border>

<!-- ? BAD -->
<Border WidthRequest="450" HeightRequest="450" BackgroundColor="#FFFFFF" StrokeThickness="2"><Grid ColumnDefinitions="*,*,*">
<!-- Content -->
</Grid></Border>
```

### Project-Specific Rules

**?? Critical**: Read [AI_INSTRUCTIONS.md](.github/copilot-instructions.md) before coding! Key rules:

1. **Never hardcode colors** - use theme resources
2. **Use modern async APIs** - `DisplayAlertAsync`, not `DisplayAlert`
3. **No obsolete APIs** - No `Frame`, use `Border`
4. **Warnings = Errors** - Fix all warnings immediately
5. **Constants over magic numbers** - See `CONSTANTS_REFERENCE.md`

---

## ?? Testing Guidelines

### Manual Testing Checklist

- [ ] Test on target platforms (Windows/Android/iOS/macOS)
- [ ] Test light and dark themes
- [ ] Test on different screen sizes
- [ ] Test with different difficulty levels
- [ ] Test all button interactions
- [ ] Test settings persistence

### Unit Testing (Future)

When writing unit tests:
- Use **AAA pattern**: Arrange, Act, Assert
- One test per scenario
- Clear test names: `ShouldReturnTrueWhenValueIsValid`

---

## ?? What We Won't Accept

- Code that introduces warnings or errors
- Hardcoded colors or magic numbers
- Obsolete API usage
- Breaking changes without discussion
- Code without proper testing
- Commits with vague messages
- Large PRs without prior discussion (open an issue first!)

---

## ?? Good First Issues

Looking for a place to start? Check out issues labeled:
- `good first issue` - Perfect for newcomers
- `help wanted` - We'd love assistance
- `documentation` - Improve docs
- `design` - UI/UX improvements

---

## ?? Communication

- **Questions?** Open a [Discussion](https://github.com/jtmunn/SudokuGame/discussions)
- **Bug?** Open an [Issue](https://github.com/jtmunn/SudokuGame/issues)
- **Feature idea?** Start a [Discussion](https://github.com/jtmunn/SudokuGame/discussions) first
- **Ready to code?** Open a [Pull Request](https://github.com/jtmunn/SudokuGame/pulls)

---

## ?? Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md (coming soon!)
- Credited in release notes
- Mentioned in project updates

---

## ?? Code of Conduct

### Our Pledge

We're committed to providing a welcoming and inclusive environment for everyone, regardless of:
- Age, body size, disability
- Ethnicity, gender identity and expression
- Level of experience, education
- Nationality, personal appearance, race
- Religion, sexual identity and orientation

### Our Standards

**Positive behavior:**
- ? Being respectful and inclusive
- ? Welcoming diverse perspectives
- ? Accepting constructive criticism gracefully
- ? Focusing on what's best for the community
- ? Showing empathy toward others

**Unacceptable behavior:**
- ? Harassment, trolling, or insults
- ? Personal or political attacks
- ? Publishing others' private information
- ? Any conduct that's inappropriate in a professional setting

### Enforcement

Unacceptable behavior can be reported by opening an issue or contacting project maintainers. All complaints will be reviewed and investigated fairly.

---

## ?? Thank You!

Every contribution, no matter how small, makes this project better. Whether you're:
- Fixing a typo
- Reporting a bug
- Suggesting a feature
- Writing code
- Improving documentation
- Testing on devices

**You're making a difference!** ??

---

<div align="center">

**Ready to contribute?** Fork the repo and let's build something awesome together! ??

</div>
