using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TypeMaster.DataModels;

public class SearchResultWithTextLength : SearchResult, INotifyPropertyChanged
{
    private string? _translatedTextLength;

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

    public readonly TextLength UnchangableTextLength;
    public SearchResultWithTextLength(SearchResult wikipediaPageInfo, int formatedContentLength, Func<string, string> translate)
    {
        Id = wikipediaPageInfo.Id;
        Title = wikipediaPageInfo.Title;
        UnchangableTextLength = formatedContentLength >= (int)TextLength.Long ? TextLength.Long : formatedContentLength >= (int)TextLength.Medium ? TextLength.Medium : TextLength.Short;

        TranslatedTextLength = TranslateTextLength(UnchangableTextLength, translate);
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