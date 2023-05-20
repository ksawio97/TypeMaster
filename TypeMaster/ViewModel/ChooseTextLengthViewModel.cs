using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class ChooseTextLengthViewModel : BaseViewModel
{
    [ObservableProperty]
    ButtonBindableArgs shortButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs mediumButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs longButtonBindableArgs;

    WikipediaService WikipediaService { get; }

    INavigationService NavigationService { get; }

    public ChooseTextLengthViewModel(WikipediaService wikipediaService, INavigationService navigationService)
    {
        WikipediaService = wikipediaService;
        NavigationService = navigationService;

        ShortButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Short };
        MediumButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Medium };
        LongButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Long };

        CheckIfCanBeEnabled(0);
    }

    [RelayCommand]
    async Task LoadDataAsync()
    {
        string? content = await WikipediaService.GetWikipediaPageContent();
        if (content != null)
        {
            content = WikipediaService.FormatPageContent(content, WikipediaService.GetPageInfoArgs!.Language);
            CheckIfCanBeEnabled(content.Length);
        }
    }

    [RelayCommand]
    void NavigateToTypeTest(TextLength textLength)
    {
        WikipediaService.GetPageInfoArgs!.ProvidedTextLength = textLength;
        NavigationService.TryNavigateTo<TypeTestViewModel>();
    }

    void CheckIfCanBeEnabled(int length)
    {
        ShortButtonBindableArgs.IsEnabled = (int)TextLength.Short <= length;
        MediumButtonBindableArgs.IsEnabled = (int)TextLength.Medium <= length;
        LongButtonBindableArgs.IsEnabled = (int)TextLength.Long <= length;
    }
}

public partial class ButtonBindableArgs : ObservableObject
{
    [ObservableProperty]
    bool isEnabled;

    [ObservableProperty]
    TextLength representedLength;
}