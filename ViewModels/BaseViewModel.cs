using CommunityToolkit.Mvvm.ComponentModel;

namespace VillaBookingMAUI.ViewModels
{
    /// <summary>
    /// Базов ViewModel клас с общи свойства за зареждане и грешки.
    /// Наследява ObservableObject от CommunityToolkit.Mvvm.
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        protected void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        protected void SetError(string message)
        {
            HasError = true;
            ErrorMessage = message;
        }

        protected async Task ShowAlertAsync(string title, string message, string cancel = "ОК")
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }

        protected async Task<bool> ShowConfirmAsync(string title, string message,
            string accept = "Да", string cancel = "Не")
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
    }
}
