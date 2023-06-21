using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            Scores.Add(new WikipediaPageInfoWithTranslatedLength(score,  _translate));

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
            score.TranslatedTextLength = WikipediaPageInfoWithTranslatedLength.TranslateTextLength(score.ProvidedTextLength, e.GetText);

        OnPropertyChanged(nameof(HeaderText));
    }
}