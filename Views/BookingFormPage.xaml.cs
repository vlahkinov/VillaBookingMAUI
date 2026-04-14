using VillaBookingMAUI.ViewModels;

namespace VillaBookingMAUI.Views
{
    public partial class BookingFormPage : ContentPage
    {
        public BookingFormPage(BookingFormViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
