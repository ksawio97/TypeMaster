using System;
using System.Collections.Generic;

namespace TypeMaster.Service;

public class ColorsService
{
    public ICollection<string> Colors => (ICollection<string>)_colorResourceDictionary.Keys;

    public bool ContainsColor(string color) => _colorResourceDictionary.Contains(color);

    readonly ResourceDictionary _colorResourceDictionary;

    public ColorsService()
    {
        _colorResourceDictionary = new ResourceDictionary();
        _colorResourceDictionary.Source = new Uri("../ResourceDictionaries/Colors.xaml", UriKind.Relative);
    }

    public SolidColorBrush? TryGetColor(string key)
    {
        if(ContainsColor(key))
            return (SolidColorBrush)_colorResourceDictionary[key];
        return null;
    }
}
