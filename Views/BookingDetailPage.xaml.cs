using VillaBookingMAUI.ViewModels;

namespace VillaBookingMAUI.Views
{
    public partial class BookingDetailPage : ContentPage
    {
        public BookingDetailPage(BookingDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
