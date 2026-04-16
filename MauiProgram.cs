using CommunityToolkit.Maui;
using VillaBookingMAUI.Services;
using VillaBookingMAUI.ViewModels;
using VillaBookingMAUI.Views;

namespace VillaBookingMAUI
{
    public static class MauiProgram
    {
        private const string ApiBaseUrlEnvironmentVariable = "VILLA_BOOKING_API_BASE_URL";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var apiBaseUrl = ResolveApiBaseUrl();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ─── HTTP Client за API комуникация ───
            // ВАЖНО: Използваме HTTP (не HTTPS) за development с физическо устройство.
            // Сменете IP адреса с вашия локален IP (ipconfig в cmd).
            //
            // За Android емулатор: http://10.0.2.2:5152
            // За физическо устройство: http://<вашият-IP>:5152
            builder.Services.AddHttpClient<IBookingApiService, BookingApiService>(client =>
            {
                // *** СМЕНЕТЕ С ВАШИЯ IP АДРЕС ***
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                // За development: приемаме всички сертификати (решава SSL грешките)
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;

                return handler;
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

        private static string ResolveApiBaseUrl()
        {
            var configuredValue = Environment.GetEnvironmentVariable(ApiBaseUrlEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(configuredValue))
                return configuredValue;

            return DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5152"
                : "http://localhost:5152";
        }
    }
}
