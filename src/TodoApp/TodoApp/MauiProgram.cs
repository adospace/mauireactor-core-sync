using MauiReactor;
using Microsoft.Extensions.Logging;
using TodoApp.Components;
using TodoApp.Data;

namespace TodoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiReactorApp<MainPage>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "todoapp", "mobile");
            
            Directory.CreateDirectory(dbFolder);
            var dbPath = Path.Combine(dbFolder, "todoapp.db");

            builder.Services.AddDataServices(dbPath);

            builder.Services.AddSyncServices(dbPath, new Uri($"http://{(DeviceInfo.Current.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost")}:5065"));

            return builder.Build();
        }
    }
}
