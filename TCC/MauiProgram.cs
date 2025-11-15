using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using TCC.Services;
using TCC.Views;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Toolkit.Maui;

namespace TCC
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.UseArcGISToolkit();

            builder.UseArcGISRuntime(config =>
            {
                config.UseApiKey("AAPTxy8BH1VEsoebNVZXo8HurC04hgnjRkJKCx5xoQQYiSxC1-xhKOtP0m_mBgb2QCjowAOEPqaZvJPp0gTf7rwYribsleiN2nx9D1Hk1kIHi3-UtE1pVQsCW3migDkh2xarusESHV5XykqYx66Al6B8AoguzbmTnUbOp6cWIdScbfpDSEpO4aQ-cpoyuPXI9a2sfIz73MV9GQYHttrVxXmmXqcr4EBGi19zfGJcI2BA1D0.AT1_1hxU1DJ8");
            });

            // Registrar serviços como Singleton (compartilhado em toda a aplicação)
            builder.Services.AddSingleton<DatabaseService>();

            // Registrar páginas como Transient (nova instância cada vez)
            builder.Services.AddTransient<GroupViewPage>();
            builder.Services.AddTransient<PassengerEditPage>();
            builder.Services.AddTransient<DriverEditPage>();
            builder.Services.AddTransient<Views.Index>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}