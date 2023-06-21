using System;

namespace TypeMaster.DataModels;

public class NetworkAvailabilityChangedEventArgs : EventArgs
{
    public bool NewValue { get; set; }

    public NetworkAvailabilityChangedEventArgs(bool newValue)
    {
        NewValue = newValue;
    }
}