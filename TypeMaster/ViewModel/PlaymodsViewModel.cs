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

    INavigationService _navigation { get; }

    WikipediaService _wikipediaService { get; }
    public PlaymodsViewModel(INavigationService navigationService, WikipediaService wikipediaService)
    {
        _navigation = navigationService;
        _wikipediaService = wikipediaService;
    }

    [RelayCommand]
    private async Task SearchButtonClicked()
    {
        PageInfoArgs pageInfoArgs;
        if (int.TryParse(SearchBoxText, out int id))
            pageInfoArgs = new IdPageInfoArgs(id, 400, "en");
        else
            pageInfoArgs = new TitlePageInfoArgs(SearchBoxText, 400, "en");
        SearchBoxText = "";

        Results = await _wikipediaService.GetWikipediaSearchResultPagesAsync(pageInfoArgs.GetUrl());
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.AroundChars, value.Language);
        _navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
    }

    [RelayCommand]
    private void NavigateToRandomTypeTest()
    {
        var pageInfoArgs = new RandomPageInfoArgs(200, "en");
        _navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
    }
}
