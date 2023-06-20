using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : AsyncViewModel
{
    [ObservableProperty]
    List<WikipediaPageInfoWithTranslatedLength> _scores;

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    [ObservableProperty]
    string?[] _headerText;

    INavigationService _navigationService { get; }

    readonly WikipediaService _wikipediaService;
    readonly NetworkAvailabilityService _networkAvailabilityService;

    readonly Func<string, string> _translate;

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService, NetworkAvailabilityService networkAvailabilityService, SettingsService settingsService)
    {
        Scores = new ();
        _navigationService = navigation;
        _wikipediaService = wikipediaService;
        _networkAvailabilityService = networkAvailabilityService;
        _translate = settingsService.GetUITextValue;

        HeaderText = new string[7];
    }

    [RelayCommand]
    public async Task LoadData()
    {
        if (IsBusy)
            return;
        IsBusy = true;

        await foreach (var score in _wikipediaService.GetScoresDataAsync())
            Scores.Add(new WikipediaPageInfoWithTranslatedLength(score, TranslateTextLength(score.ProvidedTextLength, _translate)));

        IsBusy = false;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        if (!_networkAvailabilityService.CheckAvailability())
            return;
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.ProvidedTextLength, value.Language);
        _navigationService.TryNavigateWithPageInfoArgs<TypeTestViewModel>(pageInfoArgs);
    }

    public override void SetUIItemsText(object? _, OnLanguageChangedEventArgs e)
    {
        for (int i = 0; i < HeaderText.Length; i++)
            HeaderText[i] = e.GetText($"ScoreboardHeader{i}");

        foreach (var score in Scores)
            score.TranslatedTextLength = TranslateTextLength(score.ProvidedTextLength, e.GetText);

        OnPropertyChanged(nameof(HeaderText));
    }

    string? TranslateTextLength(TextLength textLength, Func<string, string> translate)
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
}

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

    public WikipediaPageInfoWithTranslatedLength(WikipediaPageInfo wikipediaPageInfo, string? translatedTextLength)
    {
        Id = wikipediaPageInfo.Id;
        Title = wikipediaPageInfo.Title;
        WPM = wikipediaPageInfo.WPM;
        SecondsSpent = wikipediaPageInfo.SecondsSpent;
        Words = wikipediaPageInfo.Words;
        ProvidedTextLength = wikipediaPageInfo.ProvidedTextLength;
        Language = wikipediaPageInfo.Language;

        TranslatedTextLength = translatedTextLength;
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}