using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VillaBookingMAUI.Models;
using VillaBookingMAUI.Services;

namespace VillaBookingMAUI.ViewModels
{
    public partial class CalendarViewModel : BaseViewModel
    {
        private readonly IBookingApiService _apiService;

        [ObservableProperty]
        private DateTime _currentMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private string _monthYearDisplay = string.Empty;

        [ObservableProperty]
        private int _selectedHouseFilter; // 0 = Къща 1, 1 = Къща 2

        public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
        public ObservableCollection<Booking> MonthBookings { get; } = new();

        public List<string> HouseFilterOptions { get; } = new()
        {
            "Къща 1 – Планинска",
            "Къща 2 – Езерна"
        };

        public CalendarViewModel(IBookingApiService apiService)
        {
            _apiService = apiService;
            Title = "Календар";
            UpdateMonthDisplay();
        }

        partial void OnSelectedHouseFilterChanged(int value)
        {
            _ = LoadCalendarAsync();
        }

        [RelayCommand]
        private async Task LoadCalendarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearError();

                int houseId = SelectedHouseFilter + 1;
                var bookings = await _apiService.GetBookingsByHouseAsync(
                    houseId, CurrentMonth.Month, CurrentMonth.Year);

                MonthBookings.Clear();
                foreach (var b in bookings)
                    MonthBookings.Add(b);

                BuildCalendarGrid(bookings);
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
        private async Task PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            UpdateMonthDisplay();
            await LoadCalendarAsync();
        }

        [RelayCommand]
        private async Task NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            UpdateMonthDisplay();
            await LoadCalendarAsync();
        }

        [RelayCommand]
        private async Task GoToToday()
        {
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            UpdateMonthDisplay();
            await LoadCalendarAsync();
        }

        private void UpdateMonthDisplay()
        {
            var bulgarianMonths = new[]
            {
                "", "Януари", "Февруари", "Март", "Април", "Май", "Юни",
                "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември"
            };
            MonthYearDisplay = $"{bulgarianMonths[CurrentMonth.Month]} {CurrentMonth.Year}";
        }

        private void BuildCalendarGrid(List<Booking> bookings)
        {
            CalendarDays.Clear();

            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);

            // Monday = 0 offset (европейски стил)
            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            // Празни клетки преди 1-во число
            for (int i = 0; i < startOffset; i++)
            {
                CalendarDays.Add(new CalendarDay { IsEmpty = true });
            }

            // Дни от месеца
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);

                var dayBookings = bookings
                    .Where(b => b.StartDate.Date <= date && b.EndDate.Date > date)
                    .ToList();

                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = day,
                    IsToday = date == DateTime.Today,
                    IsBooked = dayBookings.Any(),
                    BookingClientName = dayBookings.FirstOrDefault()?.ClientName ?? string.Empty,
                    IsEmpty = false
                });
            }
        }

        [RelayCommand]
        private async Task DayTapped(CalendarDay day)
        {
            if (day == null || day.IsEmpty) return;

            if (day.IsBooked)
            {
                // Показваме информация за резервацията
                await ShowAlertAsync(
                    $"{day.Date:dd MMM yyyy}",
                    $"Заета от: {day.BookingClientName}");
            }
            else
            {
                // Предлагаме създаване на нова резервация
                var create = await ShowConfirmAsync(
                    $"{day.Date:dd MMM yyyy}",
                    "Този ден е свободен. Искате ли да създадете резервация?",
                    "Да", "Не");

                if (create)
                {
                    await Shell.Current.GoToAsync("bookingForm");
                }
            }
        }
    }

    /// <summary>
    /// Модел за една клетка от календарната решетка.
    /// </summary>
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
        public bool IsToday { get; set; }
        public bool IsBooked { get; set; }
        public bool IsEmpty { get; set; }
        public string BookingClientName { get; set; } = string.Empty;

        public Color BackgroundColor
        {
            get
            {
                if (IsEmpty) return Colors.Transparent;
                if (IsBooked) return Color.FromArgb("#E74C3C");
                if (IsToday) return Color.FromArgb("#3498DB");
                return Color.FromArgb("#2C3E50");
            }
        }

        public Color TextColor
        {
            get
            {
                if (IsEmpty) return Colors.Transparent;
                if (IsBooked || IsToday) return Colors.White;
                return Color.FromArgb("#ECF0F1");
            }
        }
    }
}
