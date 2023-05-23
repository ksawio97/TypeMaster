using System;
using System.Collections.Generic;

namespace TypeMaster.Service;

public class ColorsService
{
    public ICollection<string> Colors => (ICollection<string>)ColorResourceDictionary.Keys;

    public bool ContainsColor(string color) => ColorResourceDictionary.Contains(color);

    readonly ResourceDictionary ColorResourceDictionary;

    public ColorsService()
    {
        ColorResourceDictionary = new ResourceDictionary();
        ColorResourceDictionary.Source = new Uri("../ResourceDictionaries/Colors.xaml", UriKind.Relative);
    }

    public SolidColorBrush? TryGetColor(string key)
    {
        if(ContainsColor(key))
            return (SolidColorBrush)ColorResourceDictionary[key];
        return null;
    }
}
