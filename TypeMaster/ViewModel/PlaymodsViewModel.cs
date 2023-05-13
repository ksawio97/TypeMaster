using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class PlaymodsViewModel : BaseViewModel
{
    [ObservableProperty]
    List<WikipediaPageInfo> _results;

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

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
        PageInfoArgs pageInfoArgs;
        if (int.TryParse(SearchBoxText, out int id))
            pageInfoArgs = new IdPageInfoArgs(id, SettingsService.ProvidedTextLength, SettingsService.CurrentLanguage);
        else
            pageInfoArgs = new TitlePageInfoArgs(SearchBoxText, SettingsService.ProvidedTextLength, SettingsService.CurrentLanguage);
        SearchBoxText = "";

        Results = await WikipediaService.GetWikipediaSearchResultPagesAsync(pageInfoArgs.GetUrl());
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        if (value == null)
            return;
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.ProvidedTextLength, value.Language);
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
