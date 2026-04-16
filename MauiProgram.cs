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
            builder.Services.AddHttpClient<IBookingApiService, BookingApiService>(client =>
            {
                // IP адресът на сървъра с API-то (може да е локален или в мрежата)
                client.BaseAddress = new Uri("http://192.168.1.216:5152");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;
                return handler;
            });

            // ─── Услуги (Services) ───
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IReminderService, ReminderService>();

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