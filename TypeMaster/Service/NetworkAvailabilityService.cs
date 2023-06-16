using System;
using System.Net.NetworkInformation;

namespace TypeMaster.Service;

public class NetworkAvailabilityService
{
    private bool _isAvailable;

    private bool IsAvailable
    {
        get { return _isAvailable; }
        set
        {
            _isAvailable = value;
            NetworkAvailabilityChanged(this, new NetworkAvailabilityChangedEventArgs(value));
        }
    }

    public event EventHandler<NetworkAvailabilityChangedEventArgs> NetworkAvailabilityChanged;

    public NetworkAvailabilityService()
    {
        NetworkAvailabilityChanged += (s, e) => { if (!e.NewValue) MessageBox.Show("Internet connection lost! Application won't work partialy.", "Internet connection lost!", MessageBoxButton.OK, MessageBoxImage.Warning); };
        IsAvailable = CheckAvailability();
    }
    public bool CheckAvailability()
    {
        Ping ping = new Ping();
        PingReply reply;
        try
        {
            reply = ping.Send("www.google.com");
        }
        catch (Exception)
        {
            if (IsAvailable)
                IsAvailable = false;
            return false;
        }
        if (IsAvailable != (reply.Status == IPStatus.Success))
            IsAvailable = reply.Status == IPStatus.Success;
        return IsAvailable;
    }
}

public class NetworkAvailabilityChangedEventArgs : EventArgs
{
    public bool NewValue { get; set; }

    public NetworkAvailabilityChangedEventArgs(bool newValue)
    {
        NewValue = newValue;
    }
}