using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{
    [ObservableProperty]
    string? _wikiContent;

    [ObservableProperty]
    string? _userTypeInput;

    WikipediaService wikipediaService;

    public TypeTestViewModel()
    {
        wikipediaService = new WikipediaService();
        
        LoadDataAsync();
    }

    //TO DO on text changed delete input text check correction of input push it to results Textblock (with span colored char) and show in input one less char 
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            WikiContent = await wikipediaService.TryGetWikipediaPageAsync(600, "en");
        });

        IsBusy = false;
    }

    
}
