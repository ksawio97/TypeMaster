using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class SearchArticlesViewModel : BaseViewModel
{
    [ObservableProperty]
    IEnumerable<SearchResult> _results;

    [ObservableProperty]
    SearchResult _selectedItem;

    [ObservableProperty]
    string _searchBoxText;

    INavigationService Navigation { get; }
    WikipediaService WikipediaService { get; }
    SettingsService SettingsService { get; }

    public SearchArticlesViewModel(INavigationService navigationService, WikipediaService wikipediaService, SettingsService settingsService)
    {
        Navigation = navigationService;
        WikipediaService = wikipediaService;
        SettingsService = settingsService;
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task SearchButtonClicked()
    {
        var rawResults = await WikipediaService.GetWikipediaSearchResultsAsync(SearchBoxText) ?? Array.Empty<SearchResult>();

        //filter results for only valid ones
        List<SearchResult> filteredResults = new();
        foreach (var element in rawResults)
        {
            WikipediaService.GetPageInfoArgs = new IdPageInfoArgs(element.Id, null, SettingsService.CurrentLanguage);
            
            var content = await WikipediaService.GetWikipediaPageContent();
            if (content == null)
                continue;

            content = WikipediaService.FormatPageContent(content, SettingsService.CurrentLanguage);
            if (content.Length >= (int)TextLength.Short)
                filteredResults.Add(element);
        }

        Results = filteredResults;
        WikipediaService.GetPageInfoArgs = null;
    }

    partial void OnSelectedItemChanged(SearchResult value)
    {
        var pageInfoArgs = new IdPageInfoArgs(value.Id, null, SettingsService.CurrentLanguage);
        Navigation.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task NavigateToRandomTypeTest()
    {
        var pageInfoArgs = await DraftRandomPage();
        Navigation.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
    }

    async Task<PageInfoArgs> DraftRandomPage()
    {
        PageInfoArgs pageInfoArgs;
        SearchResult? wikipediaPageInfo;
        string? content;
        do
        {
            pageInfoArgs = new RandomPageInfoArgs(null, SettingsService.CurrentLanguage);
            WikipediaService.GetPageInfoArgs = pageInfoArgs;
            (wikipediaPageInfo, content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            if (wikipediaPageInfo == null || content == null)
                continue;
            content = WikipediaService.FormatPageContent(content, WikipediaService.GetPageInfoArgs!.Language);  
        } while (content!.Length < (int)TextLength.Short);

        return new IdPageInfoArgs(wikipediaPageInfo!.Id, null, SettingsService.CurrentLanguage);
    }
}
