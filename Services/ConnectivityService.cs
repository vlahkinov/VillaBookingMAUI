namespace VillaBookingMAUI.Services
{
    /// <summary>
    /// Помощен сервиз за проверка на мрежова свързаност.
    /// Използва MAUI Connectivity API (достъп до хардуера на телефона).
    /// </summary>
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        event EventHandler<bool> ConnectivityChanged;
    }

    public class ConnectivityService : IConnectivityService, IDisposable
    {
        public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        public event EventHandler<bool>? ConnectivityChanged;

        public ConnectivityService()
        {
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var connected = e.NetworkAccess == NetworkAccess.Internet;
            ConnectivityChanged?.Invoke(this, connected);
        }

        public void Dispose()
        {
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}
