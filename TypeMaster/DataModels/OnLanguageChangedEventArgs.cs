using System;

namespace TypeMaster.DataModels;

public class OnLanguageChangedEventArgs : EventArgs
{
    public Func<string, string> GetText;
    public OnLanguageChangedEventArgs(Func<string, string> getText)
    {
        GetText = getText;
    }
}