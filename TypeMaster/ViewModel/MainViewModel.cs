using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TypeMaster.Service;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    string? _wikiContent;

    WikipediaService wikipediaService;
    public MainViewModel()
    {
        Title = "MainPageTest";
        wikipediaService = new WikipediaService();
        LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            WikiContent = await wikipediaService.TryGetWikipediaPageAsync(600, "en");
        });

        IsBusy = false;
    }
}
