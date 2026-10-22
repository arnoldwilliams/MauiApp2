using MauiApp2.Interfaces;
using MauiApp2.Services;
using Microsoft.Extensions.Logging;

namespace MauiApp2
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
                });

            builder.Services.AddSingleton<IHttpCommunication, HttpCommunication>();
            builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            builder.Services.AddSingleton<IStorageService, StorageService>();

            builder.Services.AddSingleton<IFormConfigurationService, FormConfigurationService>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
