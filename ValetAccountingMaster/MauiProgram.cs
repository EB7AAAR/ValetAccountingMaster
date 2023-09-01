using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ValetAccountingMaster.Data;
using ValetAccountingMaster.Services;
using ValetAccountingMaster.View;
using ValetAccountingMaster.ViewModel;

namespace ValetAccountingMaster;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp(true)
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<FireBaseService>();
        builder.Services.AddSingleton<RecordsViewModel>();
        builder.Services.AddSingleton<DatabaseContext>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<MonthDetailsViewModel>();
        builder.Services.AddTransient<MonthDetailsPage>();
        builder.Services.AddTransient<DayDetailsViewModel>();
        builder.Services.AddTransient<DayDetailsPage>();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

        return builder.Build();
    }
}
