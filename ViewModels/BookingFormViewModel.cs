using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VillaBookingMAUI.Models;
using VillaBookingMAUI.Services;

namespace VillaBookingMAUI.ViewModels
{
    [QueryProperty(nameof(BookingId), "bookingId")]
    public partial class BookingFormViewModel : BaseViewModel
    {
        private readonly IBookingApiService _apiService;

        [ObservableProperty]
        private int _bookingId;

        // Form fields
        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private int _guestsCount = 1;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private int _selectedHouseIndex; // 0 = Къща 1, 1 = Къща 2

        [ObservableProperty]
        private bool _isDepositPaid;

        [ObservableProperty]
        private string _createdBy = string.Empty;

        [ObservableProperty]
        private string _clientPhone = string.Empty;

        [ObservableProperty]
        private string _clientEmail = string.Empty;

        // UI state
        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _saveButtonText = "Създай резервация";

        [ObservableProperty]
        private bool _isAvailable = true;

        [ObservableProperty]
        private string _availabilityMessage = string.Empty;

        // Minimum date for pickers
        public DateTime MinDate => DateTime.Today;

        public List<string> HouseOptions { get; } = new()
        {
            "Къща 1 – Планинска",
            "Къща 2 – Езерна"
        };

        public List<int> GuestOptions { get; } = new() { 1, 2, 3, 4 };

        public BookingFormViewModel(IBookingApiService apiService)
        {
            _apiService = apiService;
            Title = "Нова резервация";

            // Автоматично попълване на CreatedBy с името на устройството
            CreatedBy = DeviceInfo.Current.Name ?? "mobile_user";
        }

        partial void OnBookingIdChanged(int value)
        {
            if (value > 0)
            {
                IsEditMode = true;
                Title = "Редактиране";
                SaveButtonText = "Запази промените";
                _ = LoadExistingBookingAsync();
            }
        }

        partial void OnStartDateChanged(DateTime value)
        {
            // Ако крайната дата е преди началната, коригирай
            if (EndDate <= value)
            {
                EndDate = value.AddDays(1);
            }
            _ = CheckAvailabilityAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            _ = CheckAvailabilityAsync();
        }

        partial void OnSelectedHouseIndexChanged(int value)
        {
            _ = CheckAvailabilityAsync();
        }

        private async Task LoadExistingBookingAsync()
        {
            try
            {
                IsBusy = true;
                var booking = await _apiService.GetBookingByIdAsync(BookingId);

                if (booking != null)
                {
                    ClientName = booking.ClientName;
                    GuestsCount = booking.GuestsCount;
                    StartDate = booking.StartDate;
                    EndDate = booking.EndDate;
                    SelectedHouseIndex = booking.HouseId - 1;
                    IsDepositPaid = booking.IsDepositPaid;
                    CreatedBy = booking.CreatedBy;
                    ClientPhone = booking.ClientPhone ?? string.Empty;
                    ClientEmail = booking.ClientEmail ?? string.Empty;
                }
                else
                {
                    await ShowAlertAsync("Грешка", "Резервацията не е намерена.");
                    await Shell.Current.GoToAsync("..");
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

        /// <summary>
        /// Проверява наличността на къщата за избрания период в реално време.
        /// </summary>
        [RelayCommand]
        private async Task CheckAvailabilityAsync()
        {
            if (StartDate >= EndDate) return;

            try
            {
                int houseId = SelectedHouseIndex + 1;
                var (available, _) = await _apiService.CheckAvailabilityAsync(houseId, StartDate, EndDate);

                // При редактиране – ако е същата къща и период, е налична
                if (IsEditMode)
                {
                    IsAvailable = true;
                    AvailabilityMessage = string.Empty;
                    return;
                }

                IsAvailable = available;
                AvailabilityMessage = available
                    ? "Къщата е свободна за избрания период."
                    : "Къщата е заета за този период! Изберете друга дата.";
            }
            catch
            {
                // Не блокираме формата при грешка в проверката
                IsAvailable = true;
                AvailabilityMessage = string.Empty;
            }
        }

        [RelayCommand]
        private async Task SaveBookingAsync()
        {
            if (IsBusy) return;

            // Валидация на клиентска страна
            var validationError = ValidateForm();
            if (validationError != null)
            {
                await ShowAlertAsync("Валидация", validationError);

                // Вибрация при грешка (използване на хардуер)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
                return;
            }

            try
            {
                IsBusy = true;

                var booking = new Booking
                {
                    Id = BookingId,
                    ClientName = ClientName.Trim(),
                    GuestsCount = GuestsCount,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    HouseId = SelectedHouseIndex + 1,
                    IsDepositPaid = IsDepositPaid,
                    CreatedBy = CreatedBy.Trim(),
                    ClientPhone = string.IsNullOrWhiteSpace(ClientPhone) ? null : ClientPhone.Trim(),
                    ClientEmail = string.IsNullOrWhiteSpace(ClientEmail) ? null : ClientEmail.Trim()
                };

                (bool success, string? error) result;

                if (IsEditMode)
                {
                    result = await _apiService.UpdateBookingAsync(booking);
                }
                else
                {
                    result = await _apiService.CreateBookingAsync(booking);
                }

                if (result.success)
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);

                    await ShowAlertAsync("Успех",
                        IsEditMode ? "Резервацията е обновена." : "Резервацията е създадена.");

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                    await ShowAlertAsync("Грешка", result.error ?? "Неизвестна грешка.");
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

        private string? ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(ClientName))
                return "Въведете име на клиента.";

            if (ClientName.Trim().Length < 2)
                return "Името на клиента трябва да е поне 2 символа.";

            if (GuestsCount < 1 || GuestsCount > 4)
                return "Броят гости трябва да е между 1 и 4.";

            if (StartDate >= EndDate)
                return "Началната дата трябва да е преди крайната.";

            if (!IsEditMode && StartDate < DateTime.Today)
                return "Началната дата не може да е в миналото.";

            if (string.IsNullOrWhiteSpace(CreatedBy))
                return "Въведете кой създава резервацията.";

            // Валидация на имейл формат (ако е попълнен)
            if (!string.IsNullOrWhiteSpace(ClientEmail) && !ClientEmail.Contains('@'))
                return "Въведете валиден имейл адрес.";

            return null;
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
