using System.Collections.Generic;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : BaseViewModel
{
    public IEnumerable<WikipediaPageInfo> Scores { get;}

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    INavigationService Navigation { get; }

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        Scores = wikipediaService.Scores ?? new HashSet<WikipediaPageInfo>();
        Navigation = navigation;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.ProvidedTextLength, value.Language);
        Navigation.TryNavigateWithPageInfoArgs<TypeTestViewModel>(pageInfoArgs);
    }
}
