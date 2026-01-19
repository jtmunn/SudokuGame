using Microsoft.Extensions.Logging;
using Sudoku.Core.Services;
using Sudoku.Maui.Pages;
using Sudoku.Maui.Services;
using Sudoku.Application.Services;
using Sudoku.Application.ViewModels;

namespace Sudoku.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
                });

            // Register Core services
            builder.Services.AddSingleton<SudokuValidator>();
            builder.Services.AddSingleton<SudokuSolver>();
            builder.Services.AddSingleton<SudokuLogicalSolver>();
            builder.Services.AddSingleton<SudokuGenerator>();

            // Register Maui services
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IGameStateService, GameStateService>();

            // Register ViewModels
            builder.Services.AddTransient<SudokuPageViewModel>();

            // Register pages
            builder.Services.AddTransient<SudokuPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
