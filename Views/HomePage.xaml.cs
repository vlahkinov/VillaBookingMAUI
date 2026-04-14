using VillaBookingMAUI.ViewModels;

namespace VillaBookingMAUI.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadDataCommand.Execute(null);
        }
    }
}
