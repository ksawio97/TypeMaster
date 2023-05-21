using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
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

    INavigationService NavigationService { get; }
    CurrentPageService CurrentPageService { get; }

    public ChooseTextLengthViewModel(INavigationService navigationService, CurrentPageService currentPageService)
    {
        NavigationService = navigationService;
        CurrentPageService = currentPageService;

        ShortButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Short };
        MediumButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Medium };
        LongButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Long };

        CheckIfCanBeEnabled(0);
    }

    [RelayCommand]
    async Task LoadDataAsync()
    {
        string? content = await CurrentPageService.TryGetPageContent(formatted: true);
        if (content != null)
            CheckIfCanBeEnabled(content.Length);
        else
        {
            Debug.WriteLine("Couldn't get page content!");
        }
    }

    [RelayCommand]
    void NavigateToTypeTest(TextLength textLength)
    {
        CurrentPageService.CurrentPageInfoArgs!.ProvidedTextLength = textLength;
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