using VillaBookingMAUI.Views;

namespace VillaBookingMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистриране на маршрути за навигация (push pages)
            Routing.RegisterRoute("bookingDetail", typeof(BookingDetailPage));
            Routing.RegisterRoute("bookingForm", typeof(BookingFormPage));
        }
    }
}
