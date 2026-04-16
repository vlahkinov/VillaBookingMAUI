using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VillaBookingMAUI.Models;
using VillaBookingMAUI.Services;

namespace VillaBookingMAUI.ViewModels
{
    [QueryProperty(nameof(BookingId), "bookingId")]
    public partial class BookingDetailViewModel : BaseViewModel
    {
        private readonly IBookingApiService _apiService;
        private readonly IReminderService _reminderService;

        [ObservableProperty]
        private int _bookingId;

        [ObservableProperty]
        private Booking? _booking;

        [ObservableProperty]
        private bool _isLoaded;

        public BookingDetailViewModel(IBookingApiService apiService, IReminderService reminderService)
        {
            _apiService = apiService;
            _reminderService = reminderService;
            Title = "Детайли";
        }

        partial void OnBookingIdChanged(int value)
        {
            _ = LoadBookingAsync();
        }

        [RelayCommand]
        private async Task LoadBookingAsync()
        {
            if (IsBusy || BookingId <= 0) return;

            try
            {
                IsBusy = true;
                ClearError();

                Booking = await _apiService.GetBookingByIdAsync(BookingId);

                if (Booking == null)
                {
                    SetError("Резервацията не е намерена.");
                }
                else
                {
                    IsLoaded = true;
                    Title = $"Резервация #{Booking.Id}";
                }
            }
            catch (Exception ex)
            {
                SetError($"Грешка: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task EditBooking()
        {
            if (Booking == null) return;
            await Shell.Current.GoToAsync($"bookingForm?bookingId={Booking.Id}");
        }

        [RelayCommand]
        private async Task DeleteBooking()
        {
            if (Booking == null) return;

            var confirmed = await ShowConfirmAsync(
                "Изтриване",
                $"Сигурни ли сте, че искате да изтриете тази резервация?");

            if (!confirmed) return;

            try
            {
                IsBusy = true;
                var (success, error) = await _apiService.DeleteBookingAsync(Booking.Id);

                if (success)
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                    await ShowAlertAsync("Успех", "Резервацията е изтрита.");
                    await Shell.Current.GoToAsync("..");
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

        /// <summary>
        /// Споделяне на информация за резервацията чрез системния Share API.
        /// Демонстрира използване на хардуерни възможности на телефона.
        /// </summary>
        [RelayCommand]
        private async Task ShareBooking()
        {
            if (Booking == null) return;

            try
            {
                var text = $"Резервация – Вила\n" +
                           $"Клиент: {Booking.ClientName}\n" +
                           $"Къща: {Booking.HouseDisplayName}\n" +
                           $"Период: {Booking.DateRange}\n" +
                           $"Гости: {Booking.GuestsDisplay}\n" +
                           $"Депозит: {Booking.DepositStatus}";

                if (Booking.HasPhone)
                    text += $"\nТелефон: {Booking.ClientPhone}";
                if (Booking.HasEmail)
                    text += $"\nИмейл: {Booking.ClientEmail}";

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = text,
                    Title = "Споделяне на резервация"
                });
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Грешка", $"Не може да се сподели: {ex.Message}");
            }
        }

        /// <summary>
        /// Изпращане на напомняне чрез SMS или Email.
        /// Показва ActionSheet с налични опции (SMS/Email).
        /// Демонстрира Sms API и Email API на телефона.
        /// </summary>
        [RelayCommand]
        private async Task SendReminder()
        {
            if (Booking == null) return;

            await _reminderService.ShowReminderOptionsAsync(Booking);
        }

        /// <summary>
        /// Обаждане до клиента чрез Phone Dialer API.
        /// Демонстрира PhoneDialer хардуерна възможност.
        /// </summary>
        [RelayCommand]
        private async Task CallClient()
        {
            if (Booking == null || !Booking.HasPhone) return;

            try
            {
                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(Booking.ClientPhone!);
                }
                else
                {
                    await ShowAlertAsync("Грешка", "Телефонни обаждания не се поддържат на това устройство.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Грешка", $"Не може да се осъществи обаждане: {ex.Message}");
            }
        }
    }
}
