using Microsoft.Extensions.Logging;
namespace d9.bkp.maui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // todo: figure out how to use this
        // also remember that maui has preset stuff for a toolbar
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
