using VillaBookingMAUI.Models;

namespace VillaBookingMAUI.Services
{
    /// <summary>
    /// Сервиз за напомняния при предстоящи пристигания.
    /// Показва alert нотификации при отваряне на приложението.
    /// Използва HapticFeedback (хардуерна функция на телефона).
    /// </summary>
    public interface INotificationService
    {
        Task ScheduleArrivalRemindersAsync(List<Booking> bookings);
        Task CancelAllRemindersAsync();
    }

    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Проверява предстоящите пристигания и показва напомняне ако има
        /// пристигане днес или утре. Изпълнява се при всяко зареждане на Home.
        /// </summary>
        public async Task ScheduleArrivalRemindersAsync(List<Booking> bookings)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Резервации с пристигане днес
            var arrivingToday = bookings
                .Where(b => b.StartDate.Date == today)
                .ToList();

            // Резервации с пристигане утре
            var arrivingTomorrow = bookings
                .Where(b => b.StartDate.Date == tomorrow)
                .ToList();

            // Показваме alert за днешните пристигания
            if (arrivingToday.Any())
            {
                var message = string.Join("\n", arrivingToday.Select(b =>
                    $"• {b.ClientName} – {b.HouseDisplayName} ({b.GuestsDisplay})" +
                    (b.IsDepositPaid ? "" : " ⚠ Без депозит!")));

                await ShowReminderAlert(
                    $"Днес пристигат {arrivingToday.Count} гост(и)",
                    message);
            }

            // Показваме alert за утрешните пристигания
            if (arrivingTomorrow.Any())
            {
                var message = string.Join("\n", arrivingTomorrow.Select(b =>
                    $"• {b.ClientName} – {b.HouseDisplayName} ({b.GuestsDisplay})" +
                    (b.IsDepositPaid ? "" : " ⚠ Без депозит!")));

                await ShowReminderAlert(
                    $"Утре пристигат {arrivingTomorrow.Count} гост(и)",
                    message);
            }

            // Неплатени депозити за следващите 3 дни
            var unpaidSoon = bookings
                .Where(b => !b.IsDepositPaid &&
                            b.StartDate.Date >= today &&
                            b.StartDate.Date <= today.AddDays(3))
                .ToList();

            if (unpaidSoon.Any())
            {
                var message = string.Join("\n", unpaidSoon.Select(b =>
                    $"• {b.ClientName} – {b.HouseDisplayName} ({b.StartDate:dd.MM.yyyy})"));

                await ShowReminderAlert(
                    "⚠ Неплатени депозити",
                    $"Следните резервации нямат платен депозит:\n{message}");
            }

            System.Diagnostics.Debug.WriteLine(
                $"[Notifications] Checked arrivals: {arrivingToday.Count} today, " +
                $"{arrivingTomorrow.Count} tomorrow, {unpaidSoon.Count} unpaid soon");
        }

        public Task CancelAllRemindersAsync()
        {
            return Task.CompletedTask;
        }

        private async Task ShowReminderAlert(string title, string message)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Task.Delay(500);
                    await Application.Current.MainPage.DisplayAlert(title, message, "ОК");
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notifications] Alert error: {ex.Message}");
            }
        }
    }
}