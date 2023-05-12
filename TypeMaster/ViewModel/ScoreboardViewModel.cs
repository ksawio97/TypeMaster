using System.Collections.Generic;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : BaseViewModel
{
    public IEnumerable<WikipediaPageInfo> Scores { get;}

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    INavigationService _navigation { get; }

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        Scores = wikipediaService.scores ?? new HashSet<WikipediaPageInfo>();
        _navigation = navigation;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.AroundChars, value.Language);
        _navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
    }
}
