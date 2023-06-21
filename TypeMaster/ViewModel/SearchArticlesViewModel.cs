using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class SearchArticlesViewModel : AsyncViewModel
{
    [ObservableProperty]
    SearchResult _selectedItem;

    [ObservableProperty]
    string? _infoForUser;

    [ObservableProperty]
    string?[] _headerText;

    public ObservableCollection<SearchResultWithTextLength> Results { get; }

    readonly INavigationService _navigationService;

    readonly WikipediaService _wikipediaService;
    readonly SettingsService _settingsService;

    readonly CurrentPageService _currentPageService;

    string? _lastSearchLanguage;

    public SearchArticlesViewModel(INavigationService navigationService, WikipediaService wikipediaService, SettingsService settingsService, CurrentPageService currentPageService)
    {
        _navigationService = navigationService;
        _wikipediaService = wikipediaService;
        _settingsService = settingsService;
        _currentPageService = currentPageService;

        Results = new();
        HeaderText = new string[3];
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task SearchButtonClicked(string searchBoxText)
    {
        if (IsBusy) return;
        IsBusy = true;

        //filter results for only valid ones
        Results.Clear();
        _lastSearchLanguage = _settingsService.CurrentLanguage;

        //if it is id try show it
        if (int.TryParse(searchBoxText, out int id))
        {
            _currentPageService.CurrentPageInfoArgs = new IdPageInfoArgs(id, null, _lastSearchLanguage);

            (SearchResult? result, string? formatedContent) = (await _currentPageService.TryGetPageResult(), await _currentPageService.TryGetPageContent(formatted: true));
            if (result != null && IsPageValid(formatedContent))
                Results.Add(new SearchResultWithTextLength(result, formatedContent!.Length, _settingsService.GetUITextValue));
        }
        else
        {
            SearchResult[] rawResults = await _wikipediaService.GetWikipediaSearchResultsAsync(searchBoxText, _lastSearchLanguage) ?? Array.Empty<SearchResult>();

            foreach (var element in rawResults)
            {
                _currentPageService.CurrentPageInfoArgs = new IdPageInfoArgs(element.Id, null, _lastSearchLanguage);

                (SearchResult? result, string? formatedContent) = (await _currentPageService.TryGetPageResult(), await _currentPageService.TryGetPageContent(formatted: true));

                if (result != null && IsPageValid(formatedContent))
                    Results.Add(new SearchResultWithTextLength(result, formatedContent!.Length, _settingsService.GetUITextValue));
            }
        }

        IsBusy = false;
    }

    bool IsPageValid(string? content) => content != null && content.Length >= (int)TextLength.Short;

    partial void OnSelectedItemChanged(SearchResult value)
    {
        NavigateToChooseTextLengthViewModel(value.Id);
    }

    void NavigateToChooseTextLengthViewModel(int id)
    {
        var pageInfoArgs = new IdPageInfoArgs(id, null, _lastSearchLanguage!);
        _navigationService.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
    }

    public override void SetUIItemsText(object? _, OnLanguageChangedEventArgs e)
    {
        InfoForUser = e.GetText("SearchArticlesTextBlock");
        for (int i = 0; i < HeaderText.Length - 1; i++)
            HeaderText[i] = e.GetText($"ScoreboardHeader{i}");
        HeaderText[HeaderText.Length - 1] = e.GetText("ScoreboardHeader5");

        foreach (var item in Results)
            item.TranslatedTextLength = SearchResultWithTextLength.TranslateTextLength(item.UnchangableTextLength, _settingsService.GetUITextValue);

        OnPropertyChanged(nameof(HeaderText));
    }
}