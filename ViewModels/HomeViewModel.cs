using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VillaBookingMAUI.Models;
using VillaBookingMAUI.Services;

namespace VillaBookingMAUI.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IBookingApiService _apiService;
        private readonly IConnectivityService _connectivity;

        public ObservableCollection<Booking> UpcomingBookings { get; } = new();

        [ObservableProperty]
        private int _totalBookings;

        [ObservableProperty]
        private int _house1Bookings;

        [ObservableProperty]
        private int _house2Bookings;

        [ObservableProperty]
        private int _unpaidDeposits;

        [ObservableProperty]
        private bool _isOffline;

        [ObservableProperty]
        private string _greeting = string.Empty;

        public HomeViewModel(IBookingApiService apiService, IConnectivityService connectivity)
        {
            _apiService = apiService;
            _connectivity = connectivity;
            Title = "Начало";

            _connectivity.ConnectivityChanged += (_, connected) =>
            {
                IsOffline = !connected;
                if (connected) _ = LoadDataAsync();
            };

            UpdateGreeting();
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            Greeting = hour switch
            {
                < 12 => "Добро утро!",
                < 18 => "Добър ден!",
                _ => "Добър вечер!"
            };
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearError();
                IsOffline = !_connectivity.IsConnected;

                if (IsOffline)
                {
                    SetError("Няма интернет връзка. Проверете мрежовите настройки.");
                    return;
                }

                var allBookings = await _apiService.GetAllBookingsAsync();

                // Статистики
                TotalBookings = allBookings.Count;
                House1Bookings = allBookings.Count(b => b.HouseId == 1);
                House2Bookings = allBookings.Count(b => b.HouseId == 2);
                UnpaidDeposits = allBookings.Count(b => !b.IsDepositPaid);

                // Предстоящи резервации (от днес нататък)
                var upcoming = allBookings
                    .Where(b => b.EndDate >= DateTime.Today)
                    .OrderBy(b => b.StartDate)
                    .Take(5)
                    .ToList();

                UpcomingBookings.Clear();
                foreach (var booking in upcoming)
                {
                    UpcomingBookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                SetError($"Грешка при зареждане: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToBookingDetail(Booking booking)
        {
            if (booking == null) return;

            await Shell.Current.GoToAsync($"bookingDetail?bookingId={booking.Id}");
        }

        [RelayCommand]
        private async Task NavigateToNewBooking()
        {
            await Shell.Current.GoToAsync("bookingForm");
        }

        [RelayCommand]
        private async Task NavigateToAllBookings()
        {
            await Shell.Current.GoToAsync("//bookings");
        }
    }
}
