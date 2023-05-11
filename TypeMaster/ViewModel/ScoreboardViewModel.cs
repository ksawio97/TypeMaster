using System.Collections.Generic;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : BaseViewModel
{
    [ObservableProperty]
    HashSet<WikipediaPageInfo> _scores;

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    INavigationService _navigation;
    WikipediaService _wikipediaService;

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        Scores = wikipediaService.scores ?? new HashSet<WikipediaPageInfo>();
        _navigation = navigation;
        _wikipediaService = wikipediaService;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        _wikipediaService.getPageInfoArgs = new IdPageInfoArgs(value.Id, value.AroundChars, value.Language);
        _navigation.NavigateTo<TypeTestViewModel>();
    }
}
