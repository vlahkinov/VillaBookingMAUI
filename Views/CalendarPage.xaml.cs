using VillaBookingMAUI.ViewModels;

namespace VillaBookingMAUI.Views
{
    public partial class CalendarPage : ContentPage
    {
        private readonly CalendarViewModel _viewModel;

        public CalendarPage(CalendarViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadCalendarCommand.Execute(null);
        }
    }
}
