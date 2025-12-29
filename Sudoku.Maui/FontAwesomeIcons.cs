namespace Sudoku.Maui
{
    /// <summary>
    /// FontAwesome 6 Free Solid icons
    /// To use FontAwesome:
    /// 1. Download fa-solid-900.ttf from https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/webfonts/fa-solid-900.ttf
    /// 2. Place it in Resources/Fonts/ folder
    /// 3. Ensure it's added to the .csproj with MauiFont
    /// 4. Register it in MauiProgram.cs: fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
    /// </summary>
    public static class FontAwesomeIcons
    {
        public const string Plus = "\uf067";           // fa-plus (New Game)
        public const string Gear = "\uf013";           // fa-gear (Settings)
        public const string Lightbulb = "\uf0eb";      // fa-lightbulb (Hint)
        public const string Check = "\uf00c";          // fa-check (Check)
        public const string RotateRight = "\uf01e";    // fa-rotate-right (Reset)
        public const string Eraser = "\uf12d";         // fa-eraser (Clear)
    }
}
