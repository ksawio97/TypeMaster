using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class SearchArticlesViewModel : AsyncViewModel
{
    [ObservableProperty]
    SearchResult _selectedItem;

    public ObservableCollection<SearchResult> Results { get; }

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
                Results.Add(result);
        }
        else
        {
            SearchResult[] rawResults = await _wikipediaService.GetWikipediaSearchResultsAsync(searchBoxText, _lastSearchLanguage) ?? Array.Empty<SearchResult>();

            foreach (var element in rawResults)
            {
                _currentPageService.CurrentPageInfoArgs = new IdPageInfoArgs(element.Id, null, _lastSearchLanguage);

                string? formatedContent = await _currentPageService.TryGetPageContent(formatted: true);

                if (IsPageValid(formatedContent))
                    Results.Add(element);
            }
        }

        //Results = filteredResults;
        _currentPageService.CurrentPageInfoArgs = null;
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
}
