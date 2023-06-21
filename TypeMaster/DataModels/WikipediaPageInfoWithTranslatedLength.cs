using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TypeMaster.DataModels;

public class WikipediaPageInfoWithTranslatedLength : WikipediaPageInfo, INotifyPropertyChanged
{
    string? _translatedTextLength;

    public string? TranslatedTextLength
    {
        get => _translatedTextLength;
        set
        {
            if (_translatedTextLength == value)
                return;
            _translatedTextLength = value;
            NotifyPropertyChanged();
        }
    }

    public WikipediaPageInfoWithTranslatedLength(WikipediaPageInfo wikipediaPageInfo, Func<string, string> translate)
    {
        Id = wikipediaPageInfo.Id;
        Title = wikipediaPageInfo.Title;
        WPM = wikipediaPageInfo.WPM;
        SecondsSpent = wikipediaPageInfo.SecondsSpent;
        Words = wikipediaPageInfo.Words;
        ProvidedTextLength = wikipediaPageInfo.ProvidedTextLength;
        Language = wikipediaPageInfo.Language;

        TranslatedTextLength = TranslateTextLength(wikipediaPageInfo.ProvidedTextLength, translate);
    }

    public static string? TranslateTextLength(TextLength textLength, Func<string, string> translate)
    {
        switch (textLength)
        {
            case TextLength.Short:
                return translate("TextLength0");
            case TextLength.Medium:
                return translate("TextLength1");
            case TextLength.Long:
                return translate("TextLength2");
        }
        return null;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}