using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class SearchArticlesViewModel : AsyncViewModel
{
    [ObservableProperty]
    List<SearchResult> _results;

    [ObservableProperty]
    SearchResult _selectedItem;

    INavigationService Navigation { get; }
    WikipediaService WikipediaService { get; }
    SettingsService SettingsService { get; }

    CurrentPageService CurrentPageService { get; }

    string? LastSearchLanguage;

    public SearchArticlesViewModel(INavigationService navigationService, WikipediaService wikipediaService, SettingsService settingsService, CurrentPageService currentPageService)
    {
        Navigation = navigationService;
        WikipediaService = wikipediaService;
        SettingsService = settingsService;
        CurrentPageService = currentPageService;
        Results = new();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task SearchButtonClicked(string searchBoxText)
    {
        if (IsBusy) return;
        IsBusy = true;

        //filter results for only valid ones
        List<SearchResult> filteredResults = new();
        LastSearchLanguage = SettingsService.CurrentLanguage;

        //if it is id try show it
        if (int.TryParse(searchBoxText, out int id))
        {
            CurrentPageService.CurrentPageInfoArgs = new IdPageInfoArgs(id, null, LastSearchLanguage);

            (SearchResult? result, string? formatedContent) = (await CurrentPageService.TryGetPageResult(), await CurrentPageService.TryGetPageContent(formatted: true));
            if (result != null && IsPageValid(formatedContent))
                filteredResults.Add(result);
        }
        else
        {
            SearchResult[] rawResults = await WikipediaService.GetWikipediaSearchResultsAsync(searchBoxText, LastSearchLanguage) ?? Array.Empty<SearchResult>();

            foreach (var element in rawResults)
            {
                CurrentPageService.CurrentPageInfoArgs = new IdPageInfoArgs(element.Id, null, LastSearchLanguage);

                string? formatedContent = await CurrentPageService.TryGetPageContent(formatted: true);

                if (IsPageValid(formatedContent))
                    filteredResults.Add(element);
            }
        }

        Results = filteredResults;
        CurrentPageService.CurrentPageInfoArgs = null;
        IsBusy = false;
    }

    bool IsPageValid(string? content) => content != null && content.Length >= (int)TextLength.Short;

    partial void OnSelectedItemChanged(SearchResult value)
    {
        NavigateToChooseTextLengthViewModel(value.Id);
    }

    void NavigateToChooseTextLengthViewModel(int id)
    {
        var pageInfoArgs = new IdPageInfoArgs(id, null, LastSearchLanguage);
        Navigation.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
    }
}
