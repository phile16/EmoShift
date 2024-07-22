using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace EmoShift
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);
#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Set current directory to where the app is being run from
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            
            //Settings
            Preferences.Default.Set("showFaceRect", false);
            Preferences.Default.Set("showHandRect", false);
            Preferences.Default.Set("showFaceLandmarks", false);
            Preferences.Default.Set("showEmotionRect", false);
            Preferences.Default.Set("videoFPS", 24);
            Preferences.Default.Set("shiftAlert", 2);
            Preferences.Default.Set("saveFileLocation", Directory.GetCurrentDirectory());

#if DEBUG
            Console.WriteLine("Current Dir: " + Directory.GetCurrentDirectory());
#endif

            return builder.Build();
        }
    }
}
