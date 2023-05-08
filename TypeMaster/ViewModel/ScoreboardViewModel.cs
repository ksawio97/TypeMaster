using System.Collections.Generic;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : BaseViewModel
{
    [ObservableProperty]
    HashSet<WikipediaPageInfo> _scores;
    public ScoreboardViewModel(WikipediaService wikipediaService)
    {
        Scores = wikipediaService.scores ?? new HashSet<WikipediaPageInfo>();
    }
}
