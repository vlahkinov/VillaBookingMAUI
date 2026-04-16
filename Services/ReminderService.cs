using VillaBookingMAUI.Models;

namespace VillaBookingMAUI.Services
{
    /// <summary>
    /// Сервиз за изпращане на напомняния до клиенти чрез SMS или Email.
    /// Използва MAUI Sms и Email API (хардуерни възможности на телефона).
    /// </summary>
    public interface IReminderService
    {
        Task SendSmsReminderAsync(Booking booking);
        Task SendEmailReminderAsync(Booking booking);
        Task ShowReminderOptionsAsync(Booking booking);
    }

    public class ReminderService : IReminderService
    {
        /// <summary>
        /// Изпраща SMS напомняне чрез вградения SMS клиент на телефона.
        /// Отваря приложението за съобщения с предварително попълнен текст.
        /// </summary>
        public async Task SendSmsReminderAsync(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.ClientPhone))
            {
                await ShowAlert("Грешка", "Няма телефонен номер за този клиент.");
                return;
            }

            try
            {
                if (Sms.Default.IsComposeSupported)
                {
                    var message = ComposeReminderText(booking);

                    var smsMessage = new SmsMessage(message, new[] { booking.ClientPhone });
                    await Sms.Default.ComposeAsync(smsMessage);
                }
                else
                {
                    await ShowAlert("Грешка", "SMS не се поддържа на това устройство.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Грешка", $"Не може да се отвори SMS: {ex.Message}");
            }
        }

        /// <summary>
        /// Изпраща Email напомняне чрез вградения имейл клиент на телефона.
        /// Отваря приложението за имейл с предварително попълнено съдържание.
        /// </summary>
        public async Task SendEmailReminderAsync(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.ClientEmail))
            {
                await ShowAlert("Грешка", "Няма имейл адрес за този клиент.");
                return;
            }

            try
            {
                if (Email.Default.IsComposeSupported)
                {
                    var subject = $"Напомняне за резервация – {booking.HouseDisplayName}";
                    var body = ComposeReminderEmail(booking);

                    var emailMessage = new EmailMessage
                    {
                        Subject = subject,
                        Body = body,
                        BodyFormat = EmailBodyFormat.PlainText,
                        To = new List<string> { booking.ClientEmail }
                    };

                    await Email.Default.ComposeAsync(emailMessage);
                }
                else
                {
                    await ShowAlert("Грешка", "Имейл не се поддържа на това устройство.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Грешка", $"Не може да се отвори имейл: {ex.Message}");
            }
        }

        /// <summary>
        /// Показва меню с опции за изпращане на напомняне (SMS / Email / Отказ).
        /// </summary>
        public async Task ShowReminderOptionsAsync(Booking booking)
        {
            if (!booking.HasContact)
            {
                await ShowAlert("Информация",
                    "Няма данни за контакт с клиента. Добавете телефон или имейл в резервацията.");
                return;
            }

            var options = new List<string>();

            if (booking.HasPhone)
                options.Add($"SMS към {booking.ClientPhone}");

            if (booking.HasEmail)
                options.Add($"Имейл към {booking.ClientEmail}");

            if (Application.Current?.MainPage == null) return;

            var result = await Application.Current.MainPage.DisplayActionSheet(
                "Изпрати напомняне",
                "Отказ",
                null,
                options.ToArray());

            if (result != null && result != "Отказ")
            {
                if (result.StartsWith("SMS"))
                    await SendSmsReminderAsync(booking);
                else if (result.StartsWith("Имейл"))
                    await SendEmailReminderAsync(booking);
            }
        }

        /// <summary>
        /// Съставя текст за SMS напомняне.
        /// </summary>
        private string ComposeReminderText(Booking booking)
        {
            return $"Здравейте, {booking.ClientName}! " +
                   $"Напомняме ви за вашата резервация в {booking.HouseDisplayName} " +
                   $"от {booking.StartDate:dd.MM.yyyy} до {booking.EndDate:dd.MM.yyyy} " +
                   $"({booking.GuestsDisplay}, {booking.Nights} нощувки). " +
                   (booking.IsDepositPaid
                       ? "Депозитът ви е получен. "
                       : "Моля, платете депозита преди настаняване. ") +
                   "Очакваме ви!";
        }

        /// <summary>
        /// Съставя текст за Email напомняне (по-подробен).
        /// </summary>
        private string ComposeReminderEmail(Booking booking)
        {
            return $"Здравейте, {booking.ClientName}!\n\n" +
                   $"Напомняме ви за предстоящата ви резервация:\n\n" +
                   $"Къща: {booking.HouseDisplayName}\n" +
                   $"Период: {booking.StartDate:dd.MM.yyyy} – {booking.EndDate:dd.MM.yyyy}\n" +
                   $"Нощувки: {booking.Nights}\n" +
                   $"Гости: {booking.GuestsDisplay}\n" +
                   $"Депозит: {booking.DepositStatus}\n\n" +
                   (booking.IsDepositPaid
                       ? "Депозитът ви е получен. Благодарим ви!\n\n"
                       : "⚠ Моля, платете депозита преди датата на настаняване.\n\n") +
                   "Очакваме ви!\n" +
                   "С уважение,\nЕкипът на Вила Резервации";
        }

        private async Task ShowAlert(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "ОК");
            }
        }
    }
}
