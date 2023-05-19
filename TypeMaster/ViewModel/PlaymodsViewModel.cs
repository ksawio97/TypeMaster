using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class PlaymodsViewModel : BaseViewModel
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

    public PlaymodsViewModel(INavigationService navigationService, WikipediaService wikipediaService, SettingsService settingsService)
    {
        Navigation = navigationService;
        WikipediaService = wikipediaService;
        SettingsService = settingsService;
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task SearchButtonClicked()
    {
        Results = await WikipediaService.GetWikipediaSearchResultsAsync(SearchBoxText) ?? Array.Empty<SearchResult>();
    }

    partial void OnSelectedItemChanged(SearchResult value)
    {
        if (value == null)
            return;
        var pageInfoArgs = new IdPageInfoArgs(value.Id, SettingsService.ProvidedTextLength, SettingsService.CurrentLanguage);
        Navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
        SelectedItem = null;
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToRandomTypeTest()
    {
        var pageInfoArgs = new RandomPageInfoArgs(SettingsService.ProvidedTextLength, SettingsService.CurrentLanguage);
        Navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
    }
}
