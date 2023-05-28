using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class ScoreboardViewModel : AsyncViewModel
{
    public ObservableCollection<WikipediaPageInfo> Scores { get;}

    [ObservableProperty]
    WikipediaPageInfo _selectedItem;

    INavigationService _navigationService { get; }

    readonly WikipediaService _wikipediaService;

    public ScoreboardViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        Scores = new ();
        _navigationService = navigation;
        _wikipediaService = wikipediaService;
    }

    [RelayCommand]
    public async Task LoadData()
    {
        if (IsBusy)
            return;
        IsBusy = true;

        await foreach (var score in _wikipediaService.GetScoresDataAsync())
            Scores.Add(score);

        IsBusy = false;
    }

    partial void OnSelectedItemChanged(WikipediaPageInfo value)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
            return;
        var pageInfoArgs = new IdPageInfoArgs(value.Id, value.ProvidedTextLength, value.Language);
        _navigationService.TryNavigateWithPageInfoArgs<TypeTestViewModel>(pageInfoArgs);
    }
}
