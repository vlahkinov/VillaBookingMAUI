using CommunityToolkit.Maui;
using VillaBookingMAUI.Services;
using VillaBookingMAUI.ViewModels;
using VillaBookingMAUI.Views;

namespace VillaBookingMAUI
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

            // ─── HTTP Client за API комуникация ───
            // ВАЖНО: Сменете URL-а с адреса на вашия API сървър
            // За Android емулатор: http://10.0.2.2:5152
            // За iOS симулатор: http://localhost:5152
            // За физическо устройство: http://<вашият-IP>:5152
            builder.Services.AddHttpClient<IBookingApiService, BookingApiService>(client =>
            {
                client.BaseAddress = new Uri("http://192.168.1.216:5152");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler();
            });
           

            // ─── Услуги (Services) ───
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

            // ─── ViewModels ───
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<BookingsListViewModel>();
            builder.Services.AddTransient<BookingDetailViewModel>();
            builder.Services.AddTransient<BookingFormViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();

            // ─── Pages (Views) ───
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<BookingsListPage>();
            builder.Services.AddTransient<BookingDetailPage>();
            builder.Services.AddTransient<BookingFormPage>();
            builder.Services.AddTransient<CalendarPage>();

            return builder.Build();
        }
    }
}
