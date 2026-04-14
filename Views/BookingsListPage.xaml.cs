using VillaBookingMAUI.ViewModels;

namespace VillaBookingMAUI.Views
{
    public partial class BookingsListPage : ContentPage
    {
        private readonly BookingsListViewModel _viewModel;

        public BookingsListPage(BookingsListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadBookingsCommand.Execute(null);
        }
    }
}
