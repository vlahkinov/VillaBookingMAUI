using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VillaBookingMAUI.Models;
using VillaBookingMAUI.Services;

namespace VillaBookingMAUI.ViewModels
{
    public partial class BookingsListViewModel : BaseViewModel
    {
        private readonly IBookingApiService _apiService;
        private List<Booking> _allBookings = new();

        public ObservableCollection<Booking> Bookings { get; } = new();

        [ObservableProperty]
        private int _selectedFilterIndex; // 0 = Всички, 1 = Къща 1, 2 = Къща 2

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isEmpty;

        public List<string> FilterOptions { get; } = new()
        {
            "Всички къщи",
            "Къща 1 ",
            "Къща 2 "
        };

        public BookingsListViewModel(IBookingApiService apiService)
        {
            _apiService = apiService;
            Title = "Резервации";
        }

        partial void OnSelectedFilterIndexChanged(int value)
        {
            ApplyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        [RelayCommand]
        private async Task LoadBookingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearError();

                _allBookings = await _apiService.GetAllBookingsAsync();
                ApplyFilters();
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

        private void ApplyFilters()
        {
            var filtered = _allBookings.AsEnumerable();

            // Филтър по къща
            if (SelectedFilterIndex == 1)
                filtered = filtered.Where(b => b.HouseId == 1);
            else if (SelectedFilterIndex == 2)
                filtered = filtered.Where(b => b.HouseId == 2);

            // Търсене по име на клиент
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLower();
                filtered = filtered.Where(b =>
                    b.ClientName.ToLower().Contains(search) ||
                    b.CreatedBy.ToLower().Contains(search));
            }

            Bookings.Clear();
            foreach (var booking in filtered)
            {
                Bookings.Add(booking);
            }

            IsEmpty = Bookings.Count == 0;
        }

        [RelayCommand]
        private async Task NavigateToDetail(Booking booking)
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
        private async Task DeleteBooking(Booking booking)
        {
            if (booking == null) return;

            var confirmed = await ShowConfirmAsync(
                "Изтриване",
                $"Сигурни ли сте, че искате да изтриете резервацията на {booking.ClientName}?");

            if (!confirmed) return;

            try
            {
                IsBusy = true;
                var (success, error) = await _apiService.DeleteBookingAsync(booking.Id);

                if (success)
                {
                    _allBookings.Remove(booking);
                    Bookings.Remove(booking);
                    IsEmpty = Bookings.Count == 0;

                    // Хаптична обратна връзка при успех
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                }
                else
                {
                    await ShowAlertAsync("Грешка", error ?? "Неуспешно изтриване.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Грешка", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
