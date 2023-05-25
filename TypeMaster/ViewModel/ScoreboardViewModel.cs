﻿using System.Collections.Generic;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : BaseViewModel
{
    public IEnumerable<WikipediaPageInfo> Scores { get;}

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    INavigationService _navigationService { get; }

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        Scores = wikipediaService.Scores ?? new HashSet<WikipediaPageInfo>();
        _navigationService = navigation;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.ProvidedTextLength, value.Language);
        _navigationService.TryNavigateWithPageInfoArgs<TypeTestViewModel>(pageInfoArgs);
    }
}
